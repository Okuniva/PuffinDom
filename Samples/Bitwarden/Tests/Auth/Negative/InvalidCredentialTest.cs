using Bitwarden.AppModels.Views;
using Bitwarden.Helpers.Attributes;
using Bitwarden.Models;
using Bitwarden.Models.Enums;
using Bitwarden.TestLifecycle;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI.Asserts;

namespace Bitwarden.Tests.Auth.Negative;

[TestFixture]
[Parallelizable]
public class InvalidCredentialTest : UITestFixtureBase
{
    [UITest("ToDo", RunOn.AndroidOnly_iOSInDevelopment)]
    public void TryLoginWithInvalidHost()
    {
        var user = new User("biveyin321@bamsrad.com", "M@sterP$$w0rd", "");

        using (var login = Pages.Login)
        {
            login.EmailAddressEntry.EnterText(user.Email);
            login.HostSelectorDropdown.Tap(screenClosed: true);
            Dialogs.LoggingInOn.SelectHost(Host.BitwardenEu);
            login.ContinueButton.Tap(screenClosed: true);
        }

        using (var masterPassword = Pages.MasterPassword)
        {
            masterPassword.MasterPasswordEntry.EnterText(user.Password);
            masterPassword.LogInWithMasterPasswordButton.Tap(screenClosed: true);
        }

        Dialogs.InvalidCredentialError.Ok.Tap(screenClosed: true);
    }
    
    [UITest("ToDo", RunOn.AndroidOnly_iOSInDevelopment)]
    public void TryLoginWithInvalidPassword()
    {
        const string invalidPassword = "SuperWrongPass!123!@";
        var user = new User("biveyin321@bamsrad.com", invalidPassword, "");

        using (var login = Pages.Login)
        {
            login.EmailAddressEntry.EnterText(user.Email);
            login.ContinueButton.Tap(screenClosed: true);
        }

        using (var masterPassword = Pages.MasterPassword)
        {
            masterPassword.MasterPasswordEntry.EnterText(user.Password);
            masterPassword.LogInWithMasterPasswordButton.Tap(screenClosed: true);
        }

        Dialogs.InvalidCredentialError.Ok.Tap(screenClosed: true);
    }
}