using Bitwarden.Helpers.Extensions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Vault;

public class VaultPage : ScreenView<VaultPage>
{
    public VaultPage()
        : base(x => x.ScreenWithTitle("My vault"))
    {
    }

    public VaultHeader Header => new(this);

    public View AddItemButton => new(
        this,
        x => x.Id("AddItemButton"));
}