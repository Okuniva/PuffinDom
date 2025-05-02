using System.Drawing;
using PuffinDom.Infrastructure;
using PuffinDom.Tools;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Views.IOSSpecificView;
using Query = System.Func<string, string>;

namespace PuffinDom.UI.Views;

public abstract class DialogView<TView> : ScreenView<TView>, IIosNotRootView
    where TView : ScreenView<TView>
{
    private readonly DialogButtonsCountFlags _dialogButtonsCountFlags;
    private readonly DialogCanBeClosedTappingOutside _dialogCanBeClosedTappingOutside;

    protected DialogView(
        DialogCanBeClosedTappingOutside dialogCanBeClosedTappingOutside,
        DialogButtonsCountFlags dialogButtonsCountFlags,
        Query query,
        bool wait = true)
        : base(
            query,
            wait)
    {
        _dialogCanBeClosedTappingOutside = dialogCanBeClosedTappingOutside;
        _dialogButtonsCountFlags = dialogButtonsCountFlags;
        Validate(wait);
    }

    protected DialogView(
        DialogCanBeClosedTappingOutside dialogCanBeClosedTappingOutside,
        DialogButtonsCountFlags dialogButtonsCountFlags,
        Query droidQuery,
        Query iOSQuery,
        bool wait = true)
        : this(
            dialogCanBeClosedTappingOutside,
            dialogButtonsCountFlags,
            UIContext.Android ? droidQuery : iOSQuery,
            wait)
    {
    }

    private void Validate(bool wait)
    {
        ValidateDialogCanNotBeClosedTappingOutsideIfNeeded(wait);
        ValidateButtonsCount(wait);
    }

    private void ValidateButtonsCount(bool wait)
    {
        if (!wait)
            return;

        ViewInnerButtonsCountValidator.Validate(
            this,
            _dialogButtonsCountFlags);
    }

    private void ValidateDialogCanNotBeClosedTappingOutsideIfNeeded(bool wait)
    {
        if (!wait)
            return;

        if (_dialogCanBeClosedTappingOutside != DialogCanBeClosedTappingOutside.No)
        {
            Log.Write($"{this} can be closed by tapping outside.");
            return;
        }

        TapOutsideToClose();
    }

    public virtual void TapOutsideToClose(Point? customCoordinates = null)
    {
        using var logContext = Log.PushContext($"Tapping outside to close {this}");

        customCoordinates ??= new Point(
            Device.DeviceRect.Width - 7.ConvertFromDpToPx(),
            Device.DeviceRect.Y + 20.ConvertFromDpToPx());

        Device.TapCoordinates(customCoordinates.Value.X, customCoordinates.Value.Y);

        ThreadSleep.For(
            PuffinConstants.DefaultDelayAfterAnyAction,
            "Extra waiting on Dialog closing with Tapping outside. Maybe it's not needed");

        switch (_dialogCanBeClosedTappingOutside)
        {
            case DialogCanBeClosedTappingOutside.Yes:
                Disappeared($"{ViewFullName} is not closed by tapping outside");
                break;
            case DialogCanBeClosedTappingOutside.No:
                this.AssertExists($"{ViewFullName} is closed by tapping outside");
                break;
            case DialogCanBeClosedTappingOutside.YesOnAndroidOnly_SkipOnIOS:
                if (UIContext.Android)
                    Disappeared($"{ViewFullName} is not closed by tapping outside");

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(_dialogCanBeClosedTappingOutside),
                    _dialogCanBeClosedTappingOutside,
                    $"Unknown {nameof(DialogCanBeClosedTappingOutside)} value");
        }
    }
}