using Bitwarden.AppModels.AppScenarios.Auth;
using Bitwarden.AppModels.Views;
using Bitwarden.Helpers.Attributes;
using Bitwarden.Helpers.Extensions;
using Bitwarden.Models;
using Bitwarden.TestLifecycle;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Extensions;

namespace Bitwarden.Tests.Auth.Positive;

[TestFixture]
[Parallelizable]
public class RememberMeTest : UITestFixtureBase
{
    private readonly User _user = 
        new("biveyin321@bamsrad.com", "M@sterP$$w0rd", "DI");

    public override void BeforeEachTest()
    {
        base.BeforeEachTest();

        SignInScenario.SignIn(_user, true);

        UIContext.Device.ReopenBitwarden();
    }

    [UITest("TODO", RunOn.AndroidOnly_iOSInDevelopment)]
    public void EnableRememberMeLoginAndReopenApplicationEnterMasterPass()
    {
        using (var verifyMasterPassword = Pages.VerifyMasterPassword)
        {
            verifyMasterPassword.Header.CurrentActiveAccountIcon.AssertTextEquals(_user.AvatarIconInitials);
            verifyMasterPassword.UnlockButton.AssertDisabled();
            verifyMasterPassword.UserAndEnvironmentDataLabel(_user.Email, _user.Host);
            verifyMasterPassword.MasterPasswordEntry.EnterText(_user.Password);
            verifyMasterPassword.UnlockButton.Tap(screenClosed: true);
        }

        Pages.Vault.Wait();
    }

    [UITest("TODO", RunOn.AndroidOnly_iOSInDevelopment)]
    public void EnableRememberMeLoginAndReopenApplicationLogout()
    {
        Pages.VerifyMasterPassword.Header.TapLogOut();
        Dialogs.LogOut.Yes.Tap(screenClosed: true);

        Pages.Login.Wait();
    }
}