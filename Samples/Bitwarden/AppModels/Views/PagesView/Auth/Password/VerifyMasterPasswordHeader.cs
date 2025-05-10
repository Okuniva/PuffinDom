using System.Runtime.CompilerServices;
using Bitwarden.Helpers.Extensions;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Auth.Password;

public class VerifyMasterPasswordHeader: View<VerifyMasterPasswordHeader>
{
    public VerifyMasterPasswordHeader(VerifyMasterPasswordPage parent, bool wait = true, [CallerMemberName] string viewName = "")
        : base(
            parent,
            x => x.ScreenWithTitle("Verify master password"),
            viewName: viewName,
            wait: wait)
    {
    }

    public View CurrentActiveAccountIcon => GlobalViews.CurrentActiveAccountIcon;

    private View HeaderBarOptionsButton => GlobalViews.HeaderBarOptionsButton;

    private View LogOutButton => new(
        this,
        x => x.Text("Log out"),
        xPathStrategy: XPathStrategy.SkipParents);

    public void TapLogOut()
    {
        HeaderBarOptionsButton.Tap(screenClosed: true);
        LogOutButton.Tap(screenClosed: true);
    }
}