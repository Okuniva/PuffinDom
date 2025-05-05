using System.Diagnostics.CodeAnalysis;
using dotenv.net;
using dotenv.net.Utilities;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Tools.Logging;

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

    public static string? LoadedEnvFilePath { get; }

    public static void ReloadEnvironmentVariables(bool log = true)
    {
        using var logContext = log ? Log.PushContext("Environment variables") : null;

        // Log the current directory and potential .env file locations
        var currentDir = Directory.GetCurrentDirectory();
        Log.Write($"Current directory: {currentDir}");

        // Track environment variables before loading to detect changes
        var beforeVars = new Dictionary<string, string>();
        foreach (var key in Names.GetAllNames())
            beforeVars[key] = Environment.GetEnvironmentVariable(key) ?? string.Empty;

        // Get potential paths that might be searched
        var searchPaths = new List<string> { currentDir };
        var dirInfo = new DirectoryInfo(currentDir);
        for (var i = 0; i < 4; i++) // probeLevelsToSearch: 4
        {
            dirInfo = dirInfo?.Parent;
            if (dirInfo == null)
                break;

            searchPaths.Add(dirInfo.FullName);
        }

        Log.Write($"Potential .env search paths:\n  {string.Join("\n  ", searchPaths)}");

        // Load environment variables
        DotEnv.Load(new DotEnvOptions(probeLevelsToSearch: 4, probeForEnv: true));

        // Detect which variables were changed to determine which .env file was loaded
        var changedVars = new List<string>();
        foreach (var key in Names.GetAllNames())
        {
            var afterValue = Environment.GetEnvironmentVariable(key) ?? string.Empty;
            if (beforeVars[key] != afterValue)
                changedVars.Add(key);
        }

        if (changedVars.Count > 0)
        {
            Log.Write($"Loaded environment variables: {string.Join(", ", changedVars)}");

            // Try to determine which .env file was loaded by checking if files exist
            foreach (var path in searchPaths)
            {
                var envFile = Path.Combine(path, ".env");
                if (File.Exists(envFile))
                {
                    Log.Write($"Found .env file at: {envFile}");

                    // If environment-specific .env files should be checked (probeForEnv: true)
                    var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                                  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                                  "Development";

                    var envSpecificFile = Path.Combine(path, $".env.{envName}");
                    if (File.Exists(envSpecificFile))
                        Log.Write($"Found environment-specific .env file at: {envSpecificFile}");
                }
            }
        }
        else
            Log.Write("No environment variables were loaded from .env files");

        if (!log)
            return;

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