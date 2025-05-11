using Bitwarden.AppModels.Views;
using Bitwarden.Helpers.Attributes;
using Bitwarden.Models;
using Bitwarden.TestLifecycle;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Extensions;

namespace Bitwarden.Tests.Auth.Negative;

[TestFixture]
[NonParallelizable]
public class LoginWithoutInternetTest : UITestFixtureBase
{
    private readonly User _user = new("biveyin321@bamsrad.com", "M@sterP$$w0rd", "DI");

    [UITest("ToDo", RunOn.AndroidOnly, "iOS not support Turn off internet")]
    public void TryToLoginWithoutInternet()
    {
        using (var login = Pages.Login)
        {
            login.EmailAddressEntry.EnterText(_user.Email);
            login.ContinueButton.Tap(screenClosed: true);
        }

        using (var masterPassword = Pages.MasterPassword)
        {
            masterPassword.MasterPasswordEntry.EnterText(_user.Password);
            UIContext.Device.TurnOffInternet();
            masterPassword.LogInWithMasterPasswordButton.Tap(screenClosed: true);
        }

        Dialogs.Error.Ok.Tap(screenClosed: true);

        UIContext.Device.TurnOnInternet();
        Pages.MasterPassword.LogInWithMasterPasswordButton.Tap(screenClosed: true);
        Pages.Vault.Wait();
    }

    public override void TearDownMethod()
    {
        UIContext.Device.TurnOnInternet();
        base.TearDownMethod();
    }
}