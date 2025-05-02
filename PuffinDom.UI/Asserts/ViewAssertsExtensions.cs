using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PuffinDom.Infrastructure;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.Asserts;

public static class ViewAssertsExtensions
{
    public static TView AssertEnabled<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        ViewWaitingStrategy.WaitCondition(
            () => view.Enabled,
            $"Waiting for {view} is Enabled",
            true,
            customMessage: customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} is Enabled");
        return view;
    }

    public static TView AssertDisabled<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        ViewWaitingStrategy.WaitCondition(
            () => view.Enabled == false,
            $"Waiting for {view} is Disabled",
            true,
            customMessage: customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} is Disabled");
        return view;
    }

    public static TView AndroidAssertDisabled<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        if (UIContext.iOS)
            return view;

        ViewWaitingStrategy.WaitCondition(
            () => view.Enabled == false,
            $"Waiting for {view} is Disabled",
            true,
            customMessage: customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} is Disabled");
        return view;
    }

    public static TView AssertSelected<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        if (!view.Selected)
            throw new ViewAssertionException(
                $"{view} is not Selected but should be",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} is Selected");
        return view;
    }

    public static TView AssertExists<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        if (view.DoesNotExist())
            throw new ViewNotFoundException(
                $"{view} not found but should be on screen right now",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} found");
        return view;
    }

    public static TView AssertDoesNotExist<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        if (view.Exists())
            throw new ViewAssertionException(
                $"{view} found but should not be on screen right now",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} is not on screen");
        return view;
    }

    public static ListView<TItem> AssertEmpty<TItem>(this ListView<TItem> listView, string? customMessage = null)
        where TItem : View
    {
        if (!ViewWaitingStrategy.WaitCondition(
                listView.IsEmpty,
                $"Waiting for {listView} to be empty"))
            throw new ViewAssertionException($"List {listView} is not empty and has ({listView.Count}) items but should be empty", customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}List {listView} is empty");
        return listView;
    }

    public static ListView<TItem> AssertContainsOnlyOneRow<TItem>(this ListView<TItem> listView)
        where TItem : View
    {
        if (!ViewWaitingStrategy.WaitCondition(() => listView.Count == 1, listView.ViewFullName))
            throw new ViewAssertionException(
                $"List {listView} contains {listView.Count} elements " +
                "but should contain 1 row");

        Log.Write($"{PuffinConstants.ValidationPassed}List {listView} contains only 1 row");
        return listView;
    }

    public static ListView<TItem> AssertNotEmpty<TItem>(this ListView<TItem> listView, TimeSpan? timeout = null)
        where TItem : View
    {
        if (!ViewWaitingStrategy.WaitCondition(listView.Any, listView.ViewFullName, maxTime: timeout))
            throw new ViewAssertionException($"List {listView} is empty but should not be empty");

        Log.Write($"{PuffinConstants.ValidationPassed}List {listView} is not empty");
        return listView;
    }

    public static void AssertContainsRows<TItem>(this ListView<TItem> listView, int rowsCount)
        where TItem : View
    {
        if (!ViewWaitingStrategy.WaitCondition(() => listView.Count == rowsCount, listView.ViewFullName))
            throw new ViewAssertionException(
                $"List {listView} contains {listView.Count} rows " +
                $"but should contain {rowsCount} rows");

        Log.Write($"{PuffinConstants.ValidationPassed}List {listView} contains {rowsCount} rows");
    }

    public static void AssertContainsRow<TItem>(
        this ListView<TItem> listView,
        Func<TItem, bool> predicate,
        string customMessage)
        where TItem : View
    {
        if (!ViewWaitingStrategy.WaitCondition(() => listView.Any(predicate), listView.ViewFullName))
            throw new ViewAssertionException(
                $"List {listView} contain row with specific data" +
                $"but it doesn't",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}List {listView} contains needed row");
    }

    public static void AssertContainsNoLessRows<TItem>(this ListView<TItem> listView, int minRowsCount, string? customMessage = null)
        where TItem : View
    {
        if (!ViewWaitingStrategy.WaitCondition(() => listView.Count >= minRowsCount, listView.ViewFullName))
            throw new ViewAssertionException(
                $"List {listView} contains {listView.Count} rows " +
                $"but should contain minimum {minRowsCount} rows",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}List {listView} contains {listView.Count} rows");
    }

    public static TView AssertChecked<TView>(this TView checkBox)
        where TView : CheckBox
    {
        if (!ViewWaitingStrategy.WaitCondition(() => checkBox.IsChecked, checkBox.ViewFullName))
            throw new ViewAssertionException($"{checkBox} is not checked but should be checked");

        Log.Write($"{PuffinConstants.ValidationPassed}{checkBox} is checked");
        return checkBox;
    }

    public static TView AssertCheckedOnAndroid<TView>(this TView checkBox)
        where TView : CheckBox
    {
        if (!UIContext.Android)
            return checkBox;

        if (!ViewWaitingStrategy.WaitCondition(() => checkBox.IsChecked, checkBox.ViewFullName))
            throw new ViewAssertionException($"{checkBox} is not checked but should be checked");

        Log.Write($"{PuffinConstants.ValidationPassed}{checkBox} is checked");
        return checkBox;
    }

    public static TView AssertCheckedStatus<TView>(this TView checkBox, bool expectedStatus)
        where TView : CheckBox
    {
        return expectedStatus
            ? checkBox.AssertChecked()
            : checkBox.AssertUnchecked();
    }

    public static TView AssertUnchecked<TView>(this TView checkBox)
        where TView : CheckBox
    {
        if (!ViewWaitingStrategy.WaitCondition(() => !checkBox.IsChecked, checkBox.ViewFullName))
            throw new ViewAssertionException($"CheckBox {checkBox} is checked but should be unchecked");

        Log.Write($"{PuffinConstants.ValidationPassed}{checkBox} is unchecked");
        return checkBox;
    }

    public static TView AssertTextEquals<TView>(
        this TView view,
        string expectedText,
        string? customMessage = null)
        where TView : View
    {
        if (UIContext.AndroidTablet21 &&
            view is IViewWithPlaceholder { IsPlaceholderShouldBeVisible: true } viewWithPlaceholder)
        {
            expectedText += viewWithPlaceholder.VisiblePlaceholder;
            Log.Write(
                $"{expectedText} was modified to by adding " +
                $"{viewWithPlaceholder.VisiblePlaceholder}");
        }

        if (expectedText != view.Text)
            throw new FailTestException(
                $"{view} with text '{view.Text}' should have text: '{expectedText}'",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view.Text} has text: '{expectedText}'");

        return view;
    }

    public static TView AssertTextBecomesEqualTo<TView>(
        this TView view,
        string expectedText,
        string? customMessage = null,
        TimeSpan? maxTime = null)
        where TView : View
    {
        if (!ViewWaitingStrategy.WaitCondition(
                () => expectedText == view.Text,
                $"Text becomes {expectedText}",
                maxTime: maxTime))
            throw new ViewAssertionException(
                $"{view} with text '{view.Text}' does not become to view with text: '{expectedText}'",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} has text: '{expectedText}'");

        return view;
    }

    public static TView AssertTextBecomesContaining<TView>(
        this TView view,
        string expectedText,
        string? customMessage = null,
        TimeSpan? maxTime = null)
        where TView : View
    {
        if (!ViewWaitingStrategy.WaitCondition(
                () => view.Text.Contains(expectedText),
                $"Text becomes contains {expectedText}",
                maxTime: maxTime))
            throw new ViewAssertionException(
                $"{view} with text '{view.Text}' does not become to contains text: '{expectedText}'",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} contains text: '{expectedText}'");

        return view;
    }

    public static TView AssertTextWithPlaceholdersEquals<TView>(this TView view, string expectedText, string? customMessage = null)
        where TView : View
    {
        var regexPatternToCheck = expectedText.ReplacePlaceholdersWithRegexAnyAndSafeRegexKeywords();

        var matchSuccess = Regex.Match(
                view.Text,
                regexPatternToCheck)
            .Success;

        if (!matchSuccess)
            throw new ViewAssertionException(
                $"{view} with text '{view.Text}' does not have placeholder which matches regex: '{regexPatternToCheck}'",
                customMessage);

        Log.Write(
            regexPatternToCheck != expectedText
                ? $"{PuffinConstants.ValidationPassed}{view} has placeholder '{view.Text}' which matches regex: '{regexPatternToCheck}'"
                : $"{PuffinConstants.ValidationPassed}{view} has placeholder '{view.Text}'");

        return view;
    }

    public static TView AssertTextStartsWith<TView>(this TView view, string expectedStartText, string? customMessage = null)
        where TView : View
    {
        if (!view.Text.StartsWith(expectedStartText))
            throw new ViewAssertionException(
                $"{view} text '{view.Text}' does not to start with text: '{expectedStartText}'",
                customMessage);

        Log.Write(
            $"{PuffinConstants.ValidationPassed}{view} with text '{view.Text}' starts with text: '{expectedStartText}'");

        return view;
    }

    [PublicAPI]
    public static TView AssertTextEndsWith<TView>(this TView view, string expectedEndText, string? customMessage = null)
        where TView : View
    {
        if (!view.Text.EndsWith(expectedEndText))
            throw new ViewAssertionException(
                $"{view} with text '{view.Text}' does not to end with text: '{expectedEndText}'",
                customMessage);

        Log.Write(
            $"{PuffinConstants.ValidationPassed}{view} with text '{view.Text}' ends with text: '{expectedEndText}'");

        return view;
    }

    public static TView AssertTextNotEquals<TView>(this TView view, string expectedText, string? customMessage = null)
        where TView : View
    {
        var viewText = view.Text;

        if (viewText == expectedText)
            throw new ViewAssertionException(
                $"{view} with text '{viewText}' should not have text '{expectedText}' but it has",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} with text '{viewText}' must not have text '{expectedText}'");
        return view;
    }

    public static TView AssertTextContains<TView>(this TView view, string expectedText, string? customMessage = null)
        where TView : View
    {
        var viewText = view.Text;

        if (!viewText.Contains(expectedText))
            throw new ViewAssertionException(
                $"{view} with text '{viewText}' does not to contain text: '{expectedText}'",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} with Text '{viewText}' contains text '{expectedText}'");

        return view;
    }

    public static TView AssertTestDoesNotContain<TView>(this TView view, string expectedText, string? customMessage = null)
        where TView : View
    {
        var viewText = view.Text;

        if (viewText.Contains(expectedText))
            throw new ViewAssertionException(
                $"{view} with text '{viewText}' should not to contain text: '{expectedText}'",
                customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} with Text '{viewText}' contains text '{expectedText}'");

        return view;
    }

    public static TView AssertTextIsNotEmpty<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        if (view.Text == string.Empty)
            throw new ViewAssertionException($"{view} should not have empty text but it has", customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} with text '{view.Text}' should not be empty");
        return view;
    }

    // ReSharper disable once UnusedMember.Global
    public static TView AssertTextIsEmpty<TView>(this TView view, string? customMessage = null)
        where TView : View
    {
        if (!ViewWaitingStrategy.WaitCondition(() => string.IsNullOrEmpty(view.Text), view.ViewFullName))
            throw new ViewAssertionException($"{view} has text '{view.Text}' but it should be empty", customMessage);

        Log.Write($"{PuffinConstants.ValidationPassed}{view} has empty text");
        return view;
    }
}