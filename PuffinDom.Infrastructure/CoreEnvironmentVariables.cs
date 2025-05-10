using System.Diagnostics.CodeAnalysis;
using dotenv.net;
using dotenv.net.Utilities;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Infrastructure;

public class CoreEnvironmentVariables
{
    private static readonly AsyncLocal<RunningConfig> _currentConfig = new();

    public static RunningConfig RunningConfig => _currentConfig.Value ?? (_currentConfig.Value = ThreadSafeRunningConfig.Instance.Pop());

    public static Uri DriverUri => new($"http://localhost:{RunningConfig.AppiumPort}/wd/hub");

    public static string DroidEmulatorId => RunningConfig.DeviceId;

    public static bool EnableEachStepScreenshots =>
        EnvReader.TryGetBooleanValue(Names.EnableLocalScreenshots, out var value)
        && value;

    public static bool RunIOS => Device.IsRunIOS();
    public static bool RunDroid => Device.IsRunDroid();

    public static Emulator Device =>
        EnvReader.GetStringValue(Names.Device) switch
        {
            "Android 33" => Emulator.Android33,
            "Android 35" => Emulator.Android35,
            "Android 21" => Emulator.AndroidTablet21,
            "iOS Latest" => Emulator.iOSLatest,
            _ => throw new ArgumentException(
                "Invalid device name. Valid devices names: 'Android 33', 'Android 35', 'Android 21', 'iOS Latest'"),
        };

    public static bool IsGoogleServicesEnabled => EnvReader.TryGetBooleanValue(Names.GoogleServices, out var value) && value;
    public static string ScreenshotsDirectory => Device == Emulator.AndroidTablet21 ? "/data/local/tmp/" : "/sdcard/";

    public static bool AppIsPreinstalled => EnvReader.TryGetBooleanValue(Names.AppIsPreinstalled, out var value) && value;

    // public static string DroidEmulatorId => EnvReader.TryGetStringValue(Names.DroidEmulatorId, out var value) ? value : string.Empty;
    public static string PackageId => EnvReader.TryGetStringValue(Names.PackageId, out var value) ? value : string.Empty;

    public static void ReloadEnvironmentVariables(bool log = true)
    {
        DotEnv.Load(new DotEnvOptions(probeLevelsToSearch: 6, probeForEnv: true));

        if (!log)
            return;

        using var logContext = Log.PushContext("Environment variables");

        Log.Write($"{nameof(EnableEachStepScreenshots)}: {EnableEachStepScreenshots}");
        Log.Write($"{nameof(RunDroid)}: {RunDroid}");
        Log.Write($"{nameof(RunIOS)}: {RunIOS}");
        Log.Write($"{nameof(Device)}: {Device}");
        Log.Write($"{nameof(DroidEmulatorId)}: {DroidEmulatorId}");
        Log.Write($"{nameof(IsGoogleServicesEnabled)}: {IsGoogleServicesEnabled}");

        if (!RunDroid && !RunIOS)
            throw new Exception("No emulators to run tests on were specified. Please check .env file.");
    }

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public static class Names
    {
        public const string EnableLocalScreenshots = "ENABLE_LOCAL_SCREENSHOTS";
        public const string InstallAppFromPath = "INSTALL_APP_FROM_PATH";
        public const string DotNetTestFilter = "DOTNET_TEST_FILTER";
        public const string Device = "DEVICE";
        public const string GoogleServices = "GOOGLE_SERVICES";
        public const string DroidBuildFtpPAth = "DROID_BUILD_FTP_PATH";
        public const string NoNewUsersCreation = "NO_NEW_USERS_CREATION";
        public const string AppIsPreinstalled = "APP_IS_PREINSTALLED";
        public const string DroidEmulatorId = "DROID_EMULATOR_ID";
        public const string LatestAppStartTime = "LATEST_APP_START_TIME";
        public const string PackageId = "PACKAGE_ID";

        // Helper method to get all environment variable names
        public static string[] GetAllNames()
        {
            return new[]
            {
                EnableLocalScreenshots,
                InstallAppFromPath,
                DotNetTestFilter,
                Device,
                GoogleServices,
                DroidBuildFtpPAth,
                NoNewUsersCreation,
                AppIsPreinstalled,
                DroidEmulatorId,
                LatestAppStartTime,
                PackageId,
            };
        }
    }
}