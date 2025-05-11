using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Auth;

public class WelcomePage: ScreenView<WelcomePage>
{
    public WelcomePage()
        : base(x => x.PackageId("action_bar_root"))
    {
    }

    public View ChooseAccountCreationButton => new(
        this,
        x => x.Id("ChooseAccountCreationButton")); 
    
    public View ChooseLoginButton => new(
        this,
        x => x.Id("ChooseLoginButton"));
}