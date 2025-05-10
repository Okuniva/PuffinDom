using System.Reflection;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Infrastructure;

public class CoreConstants
{
    public const string IOSScreenshotsFolderName = "IOSScreenshots";

    public const string ValidationPassed = "VALIDATION PASSED | ";

    // ReSharper disable once InconsistentNaming
    public const string iOSSimulatorName = "booted";

    public const string CrashOfInstrumentationMessage =
        "Crash of the Application. BUT it's an instrumentation stuff failure. You should try this test once again. These cases are under investigate";

    public const string AndroidBrowserBundleId = "com.android.chrome";
    public const string AndroidChromiumBrowserBundleId = "org.chromium.webview_shell";

    public const string Android21BrowserBundleId = "com.android.browser";

    public const string AppiumLogFileName = "appium.log";

    public const int MinimumDpToScrollUpOrDown = 30;

    public static readonly string RelativePathToTestDataDirectory =
        Path.Combine(GetBaseTestPath(), "TestData");

    public static readonly TimeSpan ViewWaitingTimeout = 9.Seconds();
    public static readonly TimeSpan ViewDisappearingTimeout = 6.Seconds();
    public static readonly TimeSpan DefaultWaitLogContainsTimeout = 7.Seconds();

    public static readonly TimeSpan TapAndHoldDefaultHoldDuration = 1.5.Seconds();
    public static readonly TimeSpan DefaultDelayBetweenSingularTaps = 70.Milliseconds();
    public static readonly TimeSpan DefaultDragDuration = 600.Milliseconds();

    public static readonly TimeSpan ProxiesPingDelay = 1.Seconds();
    public static readonly TimeSpan DelayAfterScrollDuration = 600.Milliseconds();
    public static readonly TimeSpan AppiumServerMaxWaitTime = 15.Seconds();
    public static readonly TimeSpan AppiumServerStartedChecksDelayBetweenRetries = 1.Second();
    public static readonly TimeSpan IOSAppiumCommandTimeout = 2.Minutes();
    public static readonly TimeSpan DroidAppiumCommandTimeout = 3.Seconds();
    public static readonly TimeSpan IOSDeviceReadyTimeout = 30.Seconds();
    public static readonly TimeSpan IOSAppLaunchTimeout = 30.Seconds();
    public static readonly TimeSpan AppiumServerStartUpTimeOut = 1.Minute();
    public static readonly TimeSpan AppiumServerStartRetriesDelay = 1.Minute();
    public static readonly TimeSpan TaskRerunDelay = 200.Milliseconds();

    public static readonly List<string> PingUrls = ["8.8.8.8"];

    public static TimeSpan WaitForAppIdleBeforeAppiumActionStartTimeout
        => CoreEnvironmentVariables.RunDroid
            ? 200.Millisecond()
            : 200.Milliseconds();

    public static TimeSpan DefaultDelayBetweenViewExistingRechecks
        => 200.Millisecond();

    public static TimeSpan DefaultDelayAfterAnyAction
        => CoreEnvironmentVariables.RunDroid
            ? 150.Millisecond()
            : 200.Milliseconds();

    public static TimeSpan DelayAfterSwipeDuration => CoreEnvironmentVariables.RunDroid
        ? 900.Milliseconds()
        : 400.Milliseconds();

    public static string GetBaseTestPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var baseProjectPath = Path.GetDirectoryName(currentDirectory.Split([$"/bin"], StringSplitOptions.None)[0]).NotNull();
#if !DEBUG
        baseProjectPath = Path.Combine(baseProjectPath, "..");
#endif

        Log.Write($"Base project path: {baseProjectPath}");
        return baseProjectPath;
    }
}