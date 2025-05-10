using Bitwarden.AppModels.Views;
using Bitwarden.Helpers.Attributes;
using Bitwarden.TestLifecycle;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI.Asserts;

namespace Bitwarden.Tests.Auth.Negative;

[TestFixture]
[Parallelizable]
public class UnverifiedDeviceTest : UITestFixtureBase
{
    [UITest("ToDo", RunOn.AndroidOnly_iOSInDevelopment)]
    public void TryLoginByUnverifiedDevice()
    {
        const string email = "biveyin321@bamsrad.com"; // ToDo av replace
        const string password = "M@sterP$$w0rd";

        using (var login = Pages.Login)
        {
            login.EmailAddressEntry.EnterText(email);
            login.ContinueButton.Tap(screenClosed: true);
        }

        using (var masterPassword = Pages.MasterPassword)
        {
            masterPassword.MasterPasswordEntry.EnterText(password);
            masterPassword.LogInWithMasterPasswordButton.Tap(screenClosed: true);
        }

        using (var verifyYourIdentity = Pages.VerifyYourIdentity)
        {
            verifyYourIdentity.NotRecognizeDescLabel(email).AssertExists();
            verifyYourIdentity.ContinueButton.AssertDisabled();
            verifyYourIdentity.ResendCodeButton.AssertEnabled();
        }
    }
}