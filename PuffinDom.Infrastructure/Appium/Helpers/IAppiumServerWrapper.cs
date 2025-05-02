namespace PuffinDom.Infrastructure.Appium.Helpers;

public interface IAppiumServerWrapper : IDisposable
{
    void RestartAppiumServer(bool force = true);
}

