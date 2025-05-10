using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Tools.Droid;

public class SDKManager
{
    public static void InstallPackage(string package)
    {
        ExternalProgramRunner.Run(
            "sdkmanager",
            $"\"{package}\"",
            timeout: 30.Minutes(),
            message: $"Installing package {package}");
    }
}
