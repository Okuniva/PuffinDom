using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PuffinDom.Tools.IOS;
using PuffinDom.Tools.Logging;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using PuffinDom.Infrastructure;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Infrastructure.Appium;

public class IOSAppiumDriverFactory
{
    public static IOSDriver Create(bool startApp = true)
    {
        var driverOptions = GetOptions(startApp);
        return new IOSDriver(PuffinConstants.DriverUri, driverOptions);
    }

    private static AppiumOptions GetOptions(bool startApp = true)
    {
        var options = new AppiumOptions
        {
            PlatformName = "iOS",
            AutomationName = "XCUITest",
            PlatformVersion = XCodeCommandLine.DetectLatestInstalledIOSRuntime(),
            ScriptTimeout = 2.Minutes(),
            PageLoadTimeout = 2.Minutes(),
            ImplicitWaitTimeout = 2.Minutes(),
        };

        if (startApp && !PuffinEnvironmentVariables.AppIsPreinstalled)
            options.App = GetAppFilePath();

        Log.Write($"iOS Simulator Version: {options.PlatformVersion}");

        options.AddAdditionalAppiumOption(MobileCapabilityType.NewCommandTimeout, (int)PuffinConstants.IOSAppiumCommandTimeout.TotalSeconds);
#if !DEBUG
            options.AddAdditionalAppiumOption("isHeadless", true);
#endif
        options.AddAdditionalAppiumOption(IOSMobileCapabilityType.LaunchTimeout, (int)PuffinConstants.IOSAppLaunchTimeout.TotalMilliseconds);
        options.AddAdditionalAppiumOption("deviceReadyTimeout", (int)PuffinConstants.IOSDeviceReadyTimeout.TotalSeconds);
        options.AddAdditionalAppiumOption("wdaStartupRetries", 3);
        options.AddAdditionalAppiumOption("wdaStartupRetryInterval", 45000);
        options.AddAdditionalAppiumOption("wdaLaunchTimeout", 120000);
        options.AddAdditionalAppiumOption("waitForIdleTimeout", 2);
        options.AddAdditionalAppiumOption("animationCoolOffTimeout", 0);
        options.AddAdditionalAppiumOption("autoFillPasswords", false);
        options.AddAdditionalAppiumOption("snapshotMaxDepth", 30);
        options.AddAdditionalAppiumOption("customSnapshotTimeout", 120);
        options.AddAdditionalAppiumOption("settings", new Dictionary<string, dynamic> { { "respectSystemAlerts", true } });

        options.AddAdditionalAppiumOption("includeDeviceCapsToSessionInfo", false);
        options.AddAdditionalAppiumOption("reduceMotion", true);
#if DEBUG
        options.AddAdditionalAppiumOption("simulatorPasteboardAutomaticSync", "on");
#endif
        options.AddAdditionalAppiumOption("skipLogCapture", true);
        options.AddAdditionalAppiumOption("showIOSLog", false);
        options.AddAdditionalAppiumOption("clearSystemFiles", false);
        return options;
    }

    private static string GetAppFilePath()
    {
        var testDataPath = Path.Combine(Environment.CurrentDirectory, PuffinConstants.RelativePathToTestDataDirectory);
        string? path;
        try
        {
            path = Directory.EnumerateDirectories(testDataPath, "*.app").SingleOrDefault();
        }
        catch (InvalidOperationException)
        {
            throw new FileNotFoundException($"DOUBLE .APP FILES WAS FOUND IN THE {testDataPath} DIRECTORY");
        }

        if (path == null)
            throw new FileNotFoundException($"NOT FOUND!!! iOS *.app file not found in the {testDataPath} directory");

        return Path.GetFullPath(path);
    }
}