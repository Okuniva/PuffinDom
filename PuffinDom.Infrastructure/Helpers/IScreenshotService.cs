using System;

namespace PuffinDom.Infrastructure.Helpers;

public interface IScreenshotService
{
    IDisposable TurnOffScreenshots();

    void TakeScreenshot(Platform platform, string? fileNamePostfix, bool force, bool assert = true);
}