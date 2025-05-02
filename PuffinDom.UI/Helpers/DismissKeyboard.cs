using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.SystemView;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.Helpers;

public class DismissKeyboard : IDismissKeyboard
{
    private readonly IOSDismissKeyboardStrategy _iosDismissKeyboardStrategy;

    public DismissKeyboard(IOSDismissKeyboardStrategy iosDismissKeyboardStrategy)
    {
        _iosDismissKeyboardStrategy = iosDismissKeyboardStrategy;
    }

    public void Dismiss(KeyboardDismissType keyboardDismiss)
    {
        if (keyboardDismiss == KeyboardDismissType.DoNot)
        {
            Log.Write("Keyboard dismissing disabled by parameter");
            return;
        }

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (keyboardDismiss)
        {
            case KeyboardDismissType.OnlyAndroid when UIContext.iOS:
                Log.Write("Keyboard dismissing disabled by iOS");
                return;
            case KeyboardDismissType.OnlyIOS when UIContext.Android:
                Log.Write("Keyboard dismissing disabled by Android");
                return;
        }

        if (UIContext.AndroidTablet21)
        {
            Log.Write("Keyboard dismissing disabled on Android Tablet 21 API");
            return;
        }

        switch (UIContext.Platform)
        {
            case Platform.iOS:
                switch (_iosDismissKeyboardStrategy)
                {
                    case IOSDismissKeyboardStrategy.AutoDismissByAppium:
                        UIContext.Device.DismissKeyboard();
                        break;
                    case IOSDismissKeyboardStrategy.KeyboardReturnButton:
                        IOSSystemViews.KeyboardReturnButton.Tap();
                        break;
                    case IOSDismissKeyboardStrategy.KeyboardDoneButton:
                        IOSSystemViews.KeyboardDoneButton.Tap();
                        break;
                    case IOSDismissKeyboardStrategy.KeyboardSearchButton:
                        IOSSystemViews.KeyboardSearchButton.Tap();
                        break;
                    case IOSDismissKeyboardStrategy.KeyboardToolbarDoneButton:
                        IOSSystemViews.KeyboardToolbarDoneButton.Tap();
                        break;
                    case IOSDismissKeyboardStrategy.KeyboardNextButton:
                        IOSSystemViews.KeyboardNextButton.Tap();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(_iosDismissKeyboardStrategy),
                            _iosDismissKeyboardStrategy,
                            "Please support");
                }

                break;
            default:
            case Platform.Android:
                UIContext.Device.DismissKeyboard();
                break;
        }
    }
}