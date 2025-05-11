using Bitwarden.Models.Enums;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.DialogsView;

public class LoggingInOnDialog : DialogView<LoggingInOnDialog>
{
    public LoggingInOnDialog()
        : base(
            DialogCanBeClosedTappingOutside.Yes,
            DialogButtonsCountFlags.One,
            x => x.Dialog(
                "Logging in on",
                Host.BitwardenCom.CovertToString(),
                Host.BitwardenEu.CovertToString(),
                Host.SelfHosted.CovertToString()
            ))
    {
    }

    public ListView<View> HostsList => new(
        this,
        x => x.Id("AlertRadioButtonOption"),
        x => x.Id("AlertRadioButtonOptionName")
    );

    public void SelectHost(Host host)
    {
        HostsList.First(view => view.Text.Equals(host.CovertToString()))
            .Tap(screenClosed:true);
    }

    public View Cansel => new(
        this,
        x => x.Id("DismissAlertButton"));
}