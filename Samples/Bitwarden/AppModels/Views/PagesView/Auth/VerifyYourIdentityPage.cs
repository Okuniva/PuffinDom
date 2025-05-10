using PuffinDom.UI.Asserts;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.PagesView.Auth;

public class VerifyYourIdentityPage : ScreenView<VerifyYourIdentityPage>
{
    public VerifyYourIdentityPage()
        : base(x => x.Text("Verify your identity"))
    {
    }

    public View NotRecognizeDescLabel(string email) => new(
        this,
        x => x.Text(
            $"We don't recognize this device. Enter the 8 digit verification code that was emailed to {email}."));

    public TextInput VerificationCodeEntry => new(
        this,
        "Verification code",
        x => x.Id("VerificationCodeEntry"),
        inputType: TextInputType.Password);


    public View ContinueButton => new(
        this,
        x => x.Text("Continue"));

    public View ResendCodeButton => new(
        this,
        x => x.Text("Resend code"));

    protected override void Validate()
    {
        base.Validate();
        VerificationCodeEntry.Wait();
        ContinueButton.Wait();
        ResendCodeButton.Wait();
    }
}