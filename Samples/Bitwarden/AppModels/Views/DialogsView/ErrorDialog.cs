using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.DialogsView;

public class ErrorDialog : DialogView<ErrorDialog>
{
    public ErrorDialog()
        : base(
            DialogCanBeClosedTappingOutside.Yes,
            DialogButtonsCountFlags.One,
            x => x.Dialog(
                "An error has occurred.",
                "We were unable to process your request. Please try again or contact us."
            ))
    {
    }

    public View Ok => new(
        this,
        x => x.Id("AcceptAlertButton"));
}