using Bitwarden.AppModels.Views;
using Bitwarden.Helpers.Attributes;
using Bitwarden.Models;
using Bitwarden.TestLifecycle;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI.Asserts;

namespace Bitwarden.Tests.Auth.Negative;

[TestFixture]
[Parallelizable]
public class UnverifiedDeviceTest : UITestFixtureBase
{
    private readonly User _user = new("sicebid186@daupload.com", "NQcPU}wr]5q%!9W,L", "");

    [UITest("ToDo", RunOn.AndroidOnly_iOSInDevelopment)]
    public void TryLoginByUnverifiedDevice()
    {
        using (var login = Pages.Login)
        {
            login.EmailAddressEntry.EnterText(_user.Email);
            login.ContinueButton.Tap(screenClosed: true);
        }

        using (var masterPassword = Pages.MasterPassword)
        {
            masterPassword.MasterPasswordEntry.EnterText(_user.Password);
            masterPassword.LogInWithMasterPasswordButton.Tap(screenClosed: true);
        }

        using (var verifyYourIdentity = Pages.VerifyYourIdentity)
        {
            verifyYourIdentity.NotRecognizeDescLabel(_user.Email).AssertExists();
            verifyYourIdentity.ContinueButton.AssertDisabled();
            verifyYourIdentity.ResendCodeButton.AssertEnabled();
        }
    }
}