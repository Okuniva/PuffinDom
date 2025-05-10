using Bitwarden.Helpers.Extensions;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Auth.Password;

public class MasterPasswordPage : ScreenView<MasterPasswordPage>
{
    public MasterPasswordPage()
        : base(x => x.ScreenWithTitle("Bitwarden"))
    {
    }

    public MasterPasswordHeader Header => new(this);

    public TextInput MasterPasswordEntry => new(
        this,
        "Master password",
        x => x.Id("MasterPasswordEntry"),
        inputType: TextInputType.Password);

    public View GetMasterPasswordHintLabel => new(
        this,
        x => x.Id("GetMasterPasswordHintLabel"));

    public View LogInWithMasterPasswordButton => new(
        this,
        x => x.Id("LogInWithMasterPasswordButton"));

    public View EnterpriseSingleSignOnButton => new(
        this,
        x => x.Id("LogInWithSsoButton"));

    public View LoggingInAsLabel(string email, string host) => new(
        this,
        x => x.Text($"Logging in as {email} on {host}"));

    public View NotYouLabel => new(
        this,
        x => x.Id("NotYouLabel"));
}