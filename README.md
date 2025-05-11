# PuffinDom = UI Testing Framework powered by Appium Driver

## What is PuffinDom? 🐧✨

PuffinDom is a framework for writing easy-to-read and easy-to-maintain automated tests in C#. It defines a concise fluent API, natural language assertions, and does some magic to let you focus entirely on the business logic of your tests.

The main idea is to describe screen views by their components with specific properties:

```csharp
public class LoginPage : ScreenView<LoginPage>
{
    public LoginPage()
        : base(x => x.Text("Log in to Bitwarden"))
    {
    }

    public TextInput EmailAddressEntry => new(
        this,
        "Email address",
        x => x.Id("EmailAddressEntry"));

    public CheckBox RememberMeSwitch => new(
        this,
        x => x.Id("RememberMeSwitch"));
}
```

It deserializes the page source from the application into views defined in code. When you interact with views, you can add unlimited checks against the current page source state until framework refresh it automatically.

Currently, the framework is under development, but you can already:
- Use existing views from `PuffinDom/PuffinDom.UI/Views`
- Run tests in parallel
- Collect logs from the device/system and search logs during the test — check `PuffinDom/PuffinDom.Infrastructure/Helpers/DeviceLog`
- Work effectively with iOS page sources using a custom filter — check `PuffinDom/PuffinDom.Infrastructure/Appium/Helpers/IOSFilterXml.cs`

Check out the samples for the Bitwarden Android app (iOS support is in progress):
`PuffinDom/Samples/Bitwarden/`

## To run the sample:

1. Download the `.apk` from [Bitwarden Android Releases](https://github.com/bitwarden/android/releases). The sample supports version **2025.4.0 (20100)**.
2. Put the `.apk` into `PuffinDom/Samples/TestArtefacts/`
3. Install the required dependencies:
    - Appium (to run the Appium server)
    - Android SDK (to run ADB commands)
    - .NET version >= 8.0
4. Run an Android emulator
5. In the terminal, navigate to `Your_Local_Path/PuffinDom/Samples/Bitwarden`
6. Run the test using:
    ```bash
    dotnet test
    ```
