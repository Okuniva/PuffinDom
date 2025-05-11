using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.DialogsView;

public class InvalidCredentialErrorDialog : DialogView<InvalidCredentialErrorDialog>
{
    public InvalidCredentialErrorDialog()
        : base(
            DialogCanBeClosedTappingOutside.Yes,
            DialogButtonsCountFlags.One,
            x => x.Dialog(
                "An error has occurred.",
                "Username or password is incorrect. Try again."
            ))
    {
    }

    public View Ok => new(
        this,
        x => x.Id("AcceptAlertButton"));
}