using PuffinDom.Tools.ExternalApplicationsTools;

namespace PuffinDom.Tools.Droid;

public class SDKManager
{
    public static void InstallPackage(string package)
    {
        ExternalProgramRunner.Run(
            "sdkmanager",
            $"\"{package}\"",
            message: $"Installing package {package}");
    }
}