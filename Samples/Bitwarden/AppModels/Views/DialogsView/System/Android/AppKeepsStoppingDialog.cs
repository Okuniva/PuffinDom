using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.DialogsView.System.Android;

public class AppKeepsStoppingDialog : ScreenView<AppKeepsStoppingDialog>
{
    public AppKeepsStoppingDialog()
        : base(x => x.Text($"{BitwardenConstants.ProductName} keeps stopping"))
    {
    }

    public View AppInfoButton => new(
        this,
        x => x.Text("App info"));

    public View CloseAppButton => new(
        this,
        x => x.Text("Close app"));
}