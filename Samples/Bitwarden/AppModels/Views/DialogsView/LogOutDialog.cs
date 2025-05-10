using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.DialogsView;

public class LogOutDialog: DialogView<LogOutDialog>
{
    public LogOutDialog()
        : base(
            DialogCanBeClosedTappingOutside.Yes,
            DialogButtonsCountFlags.Two,
            x => x.Dialog(
                "Log out",
                "Are you sure you want to log out?"
            ))
    {
    }

    public View Cansel => new(
        this,
        x => x.Id("DismissAlertButton"));

    public View Yes => new(
        this,
        x => x.Id("AcceptAlertButton"));
}