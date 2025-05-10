using Bitwarden.AppModels.Views;
using Bitwarden.Helpers.Attributes;
using Bitwarden.Models;
using Bitwarden.TestLifecycle;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI.Asserts;

namespace Bitwarden.Tests.Auth.Positive;

[TestFixture]
[Parallelizable]
public class LoginTest : UITestFixtureBase
{
    private readonly User _user = new("biveyin321@bamsrad.com", "M@sterP$$w0rd", "DI");

    [UITest("ToDo", RunOn.AndroidOnly_iOSInDevelopment)]
    public void SuccessLoginWithValidCredentials()
    {
        using (var login = Pages.Login)
        {
            login.ContinueButton.AssertDisabled();
            login.EmailAddressEntry.EnterText(_user.Email);
            login.ContinueButton
                .AssertEnabled()
                .Tap(screenClosed: true);
        }

        using (var masterPassword = Pages.MasterPassword)
        {
            masterPassword.LogInWithMasterPasswordButton.AssertDisabled();
            masterPassword.MasterPasswordEntry.EnterText(_user.Password);
            masterPassword.LogInWithMasterPasswordButton
                .AssertEnabled()
                .Tap(screenClosed: true);
        }

        using (var vault = Pages.Vault)
        {
            vault.AddItemButton.AssertExists();
            vault.Header.CurrentActiveAccountIcon.AssertTextEquals(_user.AvatarIconInitials);
        }
    }
}