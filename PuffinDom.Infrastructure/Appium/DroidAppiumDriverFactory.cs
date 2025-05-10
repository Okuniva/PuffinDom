using PuffinDom.Tools.Logging;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using PuffinDom.Infrastructure;

namespace PuffinDom.Infrastructure.Appium;

public class DroidAppiumDriverFactory
{
    public static AndroidDriver Create()
    {
        using var logContext = Log.PushContext("Appium android driver starting");

        var driver = new AndroidDriver(CoreEnvironmentVariables.DriverUri, GetOptions());

        driver.ConfiguratorSetWaitForIdleTimeout(
            (int)CoreConstants.WaitForAppIdleBeforeAppiumActionStartTimeout.TotalMilliseconds);

        return driver;
    }

    private static AppiumOptions GetOptions()
    {
        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UIAutomator2",
        };

        options.AddAdditionalAppiumOption(
            MobileCapabilityType.NewCommandTimeout,
            (int)CoreConstants.DroidAppiumCommandTimeout.TotalMilliseconds);

        options.AddAdditionalAppiumOption(
            "skipLogcatCapture",
            true);

        return options;
    }
}