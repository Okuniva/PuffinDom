using System.Diagnostics.CodeAnalysis;
using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.Helpers;

[SuppressMessage("ReSharper", "LocalizableElement")]
public class ViewInnerButtonsCountValidator
{
    public static void Validate(View view, DialogButtonsCountFlags dialogButtonsCountFlags)
    {
        var buttonsCount = new ListView<View>(
            view,
            x => x,
            x => x.ButtonClass()).Count;

        switch (dialogButtonsCountFlags)
        {
            case DialogButtonsCountFlags.Zero:
                TypedAssert.AreEqual(0, buttonsCount, $"{view} should have 0 buttons");
                break;
            case DialogButtonsCountFlags.One:
                TypedAssert.AreEqual(1, buttonsCount, $"{view} should have 1 button");
                break;
            case DialogButtonsCountFlags.OneOnAndroid_TwoOnIOS:

                switch (UIContext.Platform)
                {
                    default:
                    case Platform.Android:
                        TypedAssert.AreEqual(1, buttonsCount, $"{view} should have 1 button");
                        break;
                    case Platform.iOS:
                        TypedAssert.AreEqual(2, buttonsCount, $"{view} should have 2 buttons");
                        break;
                }

                break;
            case DialogButtonsCountFlags.OneOnAndroid_ThreeOnIOS:

                switch (UIContext.Platform)
                {
                    default:
                    case Platform.Android:
                        TypedAssert.AreEqual(1, buttonsCount, $"{view} should have 1 button");
                        break;
                    case Platform.iOS:
                        TypedAssert.AreEqual(3, buttonsCount, $"{view} should have 2 buttons");
                        break;
                }

                break;
            case DialogButtonsCountFlags.Two:
                TypedAssert.AreEqual(2, buttonsCount, $"{view} should have 2 buttons");
                break;
            case DialogButtonsCountFlags.TwoOnAndroid_SkipOnIOS:

                if (UIContext.Platform == Platform.Android)
                    TypedAssert.AreEqual(2, buttonsCount, $"{view} should have 2 buttons");
                else
                    Log.Write("Skipping this dialog button count because it's not dialog I suppose");

                break;
            case DialogButtonsCountFlags.Three:
                TypedAssert.AreEqual(3, buttonsCount, $"{view} should have 3 buttons");
                break;
            case DialogButtonsCountFlags.Four:
                TypedAssert.AreEqual(4, buttonsCount, $"{view} should have 4 buttons");
                break;
            case DialogButtonsCountFlags.Five:
                TypedAssert.AreEqual(5, buttonsCount, $"{view} should have 5 buttons");
                break;
            case DialogButtonsCountFlags.SkipVerificationDueBugOnAndroid
                when UIContext.Android:
                Log.Write($"Skipping 'Dialog Buttons Count Verification' due to a bug. {view} has {buttonsCount} buttons");
                break;
            case DialogButtonsCountFlags.SkipVerificationDueBugOnAndroid | DialogButtonsCountFlags.One
                when UIContext.iOS:
                TypedAssert.AreEqual(1, buttonsCount, $"{view} should have 1 button");
                break;
            case DialogButtonsCountFlags.DoesNotMatter:
                Log.Write($"Skipping due doesn't matter. {view} has {buttonsCount} buttons");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(dialogButtonsCountFlags),
                    dialogButtonsCountFlags,
                    "Unknown DialogButtonsCount enum value");
        }

        Log.Write($"{CoreConstants.ValidationPassed}{view} have {buttonsCount} buttons");
    }
}