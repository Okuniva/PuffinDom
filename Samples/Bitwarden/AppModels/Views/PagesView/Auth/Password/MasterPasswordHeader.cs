using System.Runtime.CompilerServices;
using Bitwarden.Helpers.Extensions;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Auth.Password;

public class MasterPasswordHeader : View<MasterPasswordHeader>
{
    public MasterPasswordHeader(MasterPasswordPage parent, bool wait = true, [CallerMemberName] string viewName = "")
        : base(
            parent,
            x => x.ScreenWithTitle("Bitwarden"),
            viewName: viewName,
            wait: wait)
    {
    }

    public View CloseButton => new(
        this,
        x => x.Id("CloseButton"));

    private View HeaderBarOptionsButton => GlobalViews.HeaderBarOptionsButton;

    private View GetYourMasterPasswordHintLabel => new(
        this,
        x => x.Id("Get your master password hint"),
        xPathStrategy: XPathStrategy.SkipParents);

    public void TapGetYourMasterPasswordHintLabel()
    {
        HeaderBarOptionsButton.Tap(screenClosed: true);
        GetYourMasterPasswordHintLabel.Tap(screenClosed: true);
    }
}