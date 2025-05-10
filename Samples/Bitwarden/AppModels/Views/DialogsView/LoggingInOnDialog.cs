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
                "bitwarden.com",
                "bitwarden.eu",
                "Self-hosted"
            ))
    {
    }

    public ListView<CheckBox> HostsList => new(
        this,
        x => x.Id("AlertRadioButtonOption"),
        x => x.Id("AlertRadioButtonOptionName")
    );

    public View Cansel => new(
        this,
        x => x.Id("DismissAlertButton"));
}