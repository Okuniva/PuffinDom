using Bitwarden.AppModels.Views;
using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Droid;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI;

namespace Bitwarden.TestLifecycle;

public class StartBitwardenApplicationAction
{
    public static void Run()
    {
        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                StartAppOnAndroid();
                break;
            case Platform.iOS:
                UIContext.Device.StopAppiumClientDriver();
                UIContext.Device.StartAppiumClientDriver();
                break;
        }
    }

    private static void StartAppOnAndroid()
    {
        var pathToApks = BitwardenConstants.BitwardenTestArtefactsPath;

        var freshInstall = UIContext.Device.InstallApkIfNotInstalled(
            CoreEnvironmentVariables.DroidEmulatorId,
            AndroidBundleFilePathProvider.Get(pathToApks),
            CoreEnvironmentVariables.PackageId);

        if (!freshInstall)
            UIContext.Device.ClearAppData(
                CoreEnvironmentVariables.PackageId);

        UIContext.Device
            .ClearLog()
            .OpenApp(CoreEnvironmentVariables.PackageId)
            .InvalidateCachedPageSource();
    }

    private static void TapAllowOnPushNotificationsPermissionDialog()
    {
        switch (UIContext.Platform)
        {
            case Platform.iOS:

                Dialogs.AllowNotifications
                    .AllowButton.Tap(screenClosed: true);

                return;
            case Platform.Android:

                if (Dialogs.AllowNotifications.WaitSucceeded())
                    Dialogs
                        .AllowNotifications
                        .AllowButton.Tap();

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(UIContext.Platform), UIContext.Platform, null);
        }
    }
}