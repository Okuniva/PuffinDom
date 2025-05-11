using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Auth;

public class LoginPage : ScreenView<LoginPage>
{
    public LoginPage()
        : base(x => x.Text("Log in to Bitwarden"))
    {
    }

    public TextInput EmailAddressEntry => new(
        this,
        "Email address",
        x => x.Id("EmailAddressEntry"));

    public View HostSelectorDropdown => new(
        this,
        x => x.Id("RegionSelectorDropdown"));

    public CheckBox RememberMeSwitch => new(
        this,
        x => x.Id("RememberMeSwitch"));

    public View ContinueButton => new(
        this,
        x => x.Id("ContinueButton"));

    public View CreateAccountLabel => new(
        this,
        x => x.Id("CreateAccountLabel"));
}