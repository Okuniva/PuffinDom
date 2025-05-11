using Bitwarden.Helpers.Extensions;
using Bitwarden.Models.Enums;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Auth.Password;

public class VerifyMasterPasswordPage : ScreenView<VerifyMasterPasswordPage>
{
    public VerifyMasterPasswordPage()
        : base(x => x.ScreenWithTitle("Verify master password"))
    {
    }

    public VerifyMasterPasswordHeader Header => new(this);

    public TextInput MasterPasswordEntry => new(
        this,
        "Master password",
        x => x.Id("MasterPasswordEntry"),
        inputType: TextInputType.Password);

    public View YourVaultIsLockedDescLabel => new(
        this,
        x => x.Text("Your vault is locked. Verify your master password to continue."));

    public View UserAndEnvironmentDataLabel(string email, Host host) => new(
        this,
        x => x.Text($"Logged in as {email} on {host.CovertToString()}."));

    public View UnlockButton => new(
        this,
        x => x.Id("UnlockVaultButton"));

    protected override void Validate()
    {
        base.Validate();
        MasterPasswordEntry.Wait();
        YourVaultIsLockedDescLabel.Wait();
        UnlockButton.Wait();
    }
}