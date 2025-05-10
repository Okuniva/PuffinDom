using System.Runtime.CompilerServices;
using Bitwarden.Helpers.Extensions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Vault;

public class VaultHeader : View<VaultHeader>
{
    public VaultHeader(VaultPage parent, bool wait = true, [CallerMemberName] string viewName = "")
        : base(
            parent,
            x => x.Id("HeaderBarComponent"),
            viewName: viewName,
            wait: wait)
    {
    }

    public View CurrentActiveAccountIcon => GlobalViews.CurrentActiveAccountIcon;

    public View SearchButton => new(
        this,
        x => x.Id("SearchButton"));
}