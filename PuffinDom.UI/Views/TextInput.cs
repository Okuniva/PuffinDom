using System.Drawing;
using System.Runtime.CompilerServices;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Views.IOSSpecificView;
using Query = System.Func<string, string>;

namespace PuffinDom.UI.Views;

public class TextInput : ViewWithPlaceholder<TextInput>
{
    private readonly TextInputType _inputType;
    private readonly PlaceholderPlatformFlags? _placeholderPlatformFlags;
    private readonly IDismissKeyboard _dismissKeyboard;

    public TextInput(
        View? parent,
        string placeholder,
        Query query,
        bool wait = true,
        TextInputType inputType = TextInputType.Regular,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        string? iOSPlaceholder = null,
        IOSDismissKeyboardStrategy iosDismissKeyboardStrategy = IOSDismissKeyboardStrategy.AutoDismissByAppium,
        IDismissKeyboard? dismissKeyboard = null,
        PlaceholderPlatformFlags? placeholderPlatformFlags =
            PlaceholderPlatformFlags.Android | PlaceholderPlatformFlags.iOS,
        [CallerMemberName] string viewName = ""
    )
        : base(
            parent,
            query,
            UIContext.iOS && iOSPlaceholder != null
                ? iOSPlaceholder
                : placeholder,
            placeholderPlatformFlags,
            wait,
            xPathStrategy,
            viewName)
    {
        _inputType = inputType;
        _dismissKeyboard = dismissKeyboard ?? new DismissKeyboard(iosDismissKeyboardStrategy);
        _placeholderPlatformFlags = placeholderPlatformFlags;
    }

    public TextInput(
        View? parent,
        string placeholder,
        Query queryAndroid,
        Query queryIos,
        bool wait = true,
        TextInputType inputType = TextInputType.Regular,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        string? iOSPlaceholder = null,
        IOSDismissKeyboardStrategy iosDismissKeyboardStrategy = IOSDismissKeyboardStrategy.KeyboardToolbarDoneButton,
        IDismissKeyboard? dismissKeyboard = null,
        PlaceholderPlatformFlags placeholderPlatformFlags =
            PlaceholderPlatformFlags.Android | PlaceholderPlatformFlags.iOS,
        [CallerMemberName] string viewName = ""
    )
        : this(
            parent,
            placeholder,
            UIContext.Android ? queryAndroid : queryIos,
            wait,
            inputType,
            xPathStrategy,
            iOSPlaceholder,
            iosDismissKeyboardStrategy,
            dismissKeyboard,
            placeholderPlatformFlags,
            viewName)
    {
    }

    private bool ShouldValidatePlaceholder
    {
        get
        {
            if (!_placeholderPlatformFlags.HasValue)
                return false;

            if (_placeholderPlatformFlags.Value.HasFlag(PlaceholderPlatformFlags.DoNotValidate))
                return false;

            return UIContext.Platform switch
            {
                Platform.Android
                    when UIContext.Android
                         && _placeholderPlatformFlags.Value.HasFlag(PlaceholderPlatformFlags.Android) => true,
                Platform.iOS
                    when UIContext.iOS
                         && _placeholderPlatformFlags.Value.HasFlag(PlaceholderPlatformFlags.iOS) => true,
                _ => false,
            };
        }
    }

    public TextInput ClearText()
    {
        ClearTextIfNeeded(KeyboardDismissType.Always, out _);
        return this;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private TextInput ClearTextIfNeeded(
        KeyboardDismissType keyboardDismiss,
        out bool tapped,
        KeyboardDismissType? textInputKeyboardDismissType = null,
        bool expectPlaceholder = true)
    {
        using var logContext = Log.PushContext("Clearing text");

        tapped = false;
        var currentText = Text;

        if (currentText != string.Empty && currentText != Placeholder)
        {
            Tap();
            tapped = true;
            Device.ClearText(currentText.Length);
            currentText = Text;
        }

        if (expectPlaceholder && ShouldValidatePlaceholder)
        {
            if (string.IsNullOrEmpty(Placeholder))
                TypedAssert.AreEqual(
                    string.Empty,
                    currentText,
                    $"Failed to clear text in {this}. Current text: '{currentText}'");
            else
                this.AssertPlaceholderVisible("Failed to clear text");
        }

        _dismissKeyboard.Dismiss(keyboardDismiss);

        return this;
    }

    public override TextInput Tap(
        Point? coordinates = null,
        bool screenClosed = false,
        int times = 1)
    {
        coordinates = GetTextInputTapCoordinateToAvoidNumericUpDownButtonsAndMakeTextInputClearingEasier(
            coordinates);

        base.Tap(coordinates, screenClosed, times);

        ViewWaitingStrategy.WaitCondition(
            () => Device.IsKeyboardVisible,
            "Keyboard is appearing",
            !UIContext.AndroidTablet21);

        return this;
    }

    private Point GetTextInputTapCoordinateToAvoidNumericUpDownButtonsAndMakeTextInputClearingEasier(Point? coordinates)
    {
        var leftSideX = Rect.X + 10.ConvertFromDpToPx();
        coordinates ??= new Point(leftSideX, Rect.CenterY);
        return coordinates.NotNull();
    }

    public virtual TextInput EnterText(
        string text,
        KeyboardDismissType keyboardDismiss = KeyboardDismissType.Always,
        string? expectedText = null,
        bool expectPlaceholder = false,
        bool clearText = true,
        bool assertText = true)
    {
        using var logContext = Log.PushContext(
            $"Entering text: '{text}' to {this}" +
            $" without keyboard dismissing: {keyboardDismiss}" +
            (expectedText != null ? $"; expectedText: '{expectedText}'" : ""));

        var tapped = false;
        if (clearText)
            ClearTextIfNeeded(KeyboardDismissType.DoNot, out tapped, keyboardDismiss, expectPlaceholder);

        Log.Write($"Current text: '{Text}'");

        if (text.IsEmpty())
        {
            Log.Write("No text to enter");
            return this;
        }

        if (!tapped)
            Tap();

        DeviceEnterText(text);

        _dismissKeyboard.Dismiss(keyboardDismiss);

        if (assertText)
            AssertEnteredText(expectedText ?? text, expectPlaceholder);

        return this;
    }

    protected virtual void DeviceEnterText(string text)
    {
        Device.EnterText(text);
    }

    private void AssertEnteredText(string expectedText, bool expectPlaceholder)
    {
        if (expectPlaceholder)
            this.AssertPlaceholderVisible();
        else
            AssertNewTextIsValid(expectedText, Text);
    }

    private void AssertNewTextIsValid(string expectedText, string currentText)
    {
        Log.Write(
            currentText != expectedText
                ? $"Entered text: '{expectedText}' but it is '{currentText}' in {this}"
                : $"Entered text: '{expectedText}' to {this} properly");

        switch (UIContext.Platform)
        {
            case Platform.iOS when _inputType == TextInputType.Password:
                this.AssertTextBecomesEqualTo(
                    string.Concat(Enumerable.Repeat('•', expectedText.Length)),
                    $"Text is not equal to entered text in {this}");

                break;
            case Platform.Android when UIContext.AndroidTablet21 && Placeholder != string.Empty:
                this.AssertTextContains(
                    expectedText,
                    $"Text is not equal to entered text in {this}");

                break;
            default:
                this.AssertTextBecomesEqualTo(expectedText, $"Text is not equal to entered text in {this}");
                break;
        }
    }
}