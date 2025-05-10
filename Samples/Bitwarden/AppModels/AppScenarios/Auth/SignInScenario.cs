using Bitwarden.AppModels.Views;
using Bitwarden.Models;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Extensions;

namespace Bitwarden.AppModels.AppScenarios.Auth;

public class SignInScenario
{
    public static void SignIn(User user, bool rememberMe = false)
    {
        using (var login = Pages.Login)
        {
            login.EmailAddressEntry.EnterText(user.Email);
            if (rememberMe)
                login.RememberMeSwitch.TapToSwitch(false, true);
            login.ContinueButton.Tap(screenClosed: true);
        }

        using (var masterPassword = Pages.MasterPassword)
        {
            masterPassword.MasterPasswordEntry.EnterText(user.Password);
            masterPassword.LogInWithMasterPasswordButton.Tap(screenClosed: true);
        }

        Pages.Vault.Wait();
    }
}