using System;
using Core.Tools.Disposables;
using PuffinDom.Tools.Droid;
using PuffinDom.Tools.IOS;
using PuffinDom.Tools.Logging;
using PuffinDom.Infrastructure;

namespace PuffinDom.Infrastructure.Helpers;

public class ScreenshotService : IScreenshotService
{
    private static bool _screenshotsEnabled;

    public ScreenshotService()
    {
        ScreenshotsEnabled = PuffinEnvironmentVariables.EnableEachStepScreenshots;
    }

    private bool ScreenshotsEnabled
    {
        get => _screenshotsEnabled;
        set
        {
            _screenshotsEnabled = value;

            if (PuffinEnvironmentVariables.EnableEachStepScreenshots)
                Log.Write(
                    value
                        ? "Screenshots enabled"
                        : "Screenshots disabled");
        }
    }

    public IDisposable TurnOffScreenshots()
    {
        var changed = false;
        if (PuffinEnvironmentVariables.EnableEachStepScreenshots && ScreenshotsEnabled)
        {
            changed = true;
            ScreenshotsEnabled = false;
        }

        var usingBlockFinishAction = new DisposableObject();

        if (changed)
            usingBlockFinishAction.WhenDisposed(() => ScreenshotsEnabled = true);

        return usingBlockFinishAction;
    }

    public void TakeScreenshot(Platform platform, string? fileNamePostfix, bool force, bool assert = true)
    {
        if (!PuffinEnvironmentVariables.EnableEachStepScreenshots || !ScreenshotsEnabled)
            if (!force)
                return;

        try
        {
            var currentTime = $"{DateTime.Now:HH-mm-ss-fff}";

            var fileName = fileNamePostfix == null
                ? $"{currentTime}"
                : $"{currentTime}__{MakeFileNamePartSafe(fileNamePostfix)}";

            switch (platform)
            {
                default:
                case Platform.Android:
                    Adb.TakeScreenshot(
                        PuffinEnvironmentVariables.DroidEmulatorId,
                        fileName,
                        assert,
                        PuffinEnvironmentVariables.ScreenshotsDirectory);

                    break;
                case Platform.iOS:
                    XCodeCommandLine.TakeScreenshot(
                        PuffinConstants.iOSSimulatorName,
                        PuffinConstants.IOSScreenshotsFolderName,
                        fileName);

                    break;
            }
        }
        catch (Exception)
        {
            if (assert)
                throw;
        }
    }

    private static string MakeFileNamePartSafe(string fileNamePostfix)
    {
        return fileNamePostfix
            .Replace(" ", "_")
            .Replace("\'", "_")
            .Replace("\\", "_")
            .Replace("/", "_")
            .Replace(":", "_")
            .Replace("\"", "_")
            .Replace("*", "_")
            .Replace("?", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace("|", "_")
            .Replace(",", "_")
            .Replace(";", "_")
            .Replace("=", "_")
            .Replace("!", "_");
    }
}