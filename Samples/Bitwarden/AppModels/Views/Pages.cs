using Bitwarden.AppModels.Views.PagesView;
using Bitwarden.AppModels.Views.PagesView.Auth;
using Bitwarden.AppModels.Views.PagesView.Auth.Password;
using Bitwarden.AppModels.Views.PagesView.Vault;

namespace Bitwarden.AppModels.Views;

public static class Pages
{
    public static LoginPage Login => new();
    public static WelcomePage Welcome => new();
    public static MasterPasswordPage MasterPassword => new();
    public static VerifyMasterPasswordPage VerifyMasterPassword => new();
    public static VerifyYourIdentityPage VerifyYourIdentity => new();
    public static VaultPage Vault => new();
}