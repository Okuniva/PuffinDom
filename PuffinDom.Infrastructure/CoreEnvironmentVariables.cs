using System.Diagnostics.CodeAnalysis;
using PuffinDom.Infrastructure.Helpers.Device;
using dotenv.net.Utilities;

namespace PuffinDom.Infrastructure;

public static class CoreEnvironmentVariables
{
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
    public static string DroidEmulatorId => EnvReader.TryGetStringValue(Names.DroidEmulatorId, out var value) ? value : string.Empty;
    public static string PackageId => EnvReader.TryGetStringValue(Names.PackageId, out var value) ? value : string.Empty;

    // public static void WriteNewValueToEnvFile(string key, DateTime value)
    // {
    //     var envSampleFile = File.ReadAllLines(Path.Combine(Constants.RelativePathToRootDirectory, ".env")).ToList();
    //     var dateAsString = value.ToString("dd/MM/yyyy HH:mm:ss");
    //     envSampleFile.RemoveMatchingItems(x => x.Contains('=') && x.Substring(0, x.IndexOf('=')) == key);
    //     envSampleFile.Add($"{key}={dateAsString}");
    //     File.WriteAllLines(Path.Combine(Constants.RelativePathToRootDirectory, ".env"), envSampleFile);
    //     Log.Write("New value was written to .env file: " + key + "=" + dateAsString);
    //     ReloadEnvironmentVariables(false);
    // }
    //
    // public static void WriteNewValueToEnvFile(string key, string value)
    // {
    //     var envSampleFile = File.ReadAllLines(Path.Combine(Constants.RelativePathToRootDirectory, ".env")).ToList();
    //     envSampleFile.RemoveMatchingItems(x => x.Contains('=') && x.Substring(0, x.IndexOf('=')) == key);
    //     envSampleFile.Add($"{key}={value}");
    //     File.WriteAllLines(Path.Combine(Constants.RelativePathToRootDirectory, ".env"), envSampleFile);
    //     Log.Write("New value was written to .env file: " + key + "=" + value);
    //     ReloadEnvironmentVariables(false);
    // }


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
    }
}