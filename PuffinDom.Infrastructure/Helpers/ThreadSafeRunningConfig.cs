using System.Collections.Concurrent;

namespace PuffinDom.Infrastructure.Helpers;

public class ThreadSafeRunningConfig
{
    private static readonly Lazy<ThreadSafeRunningConfig> _instance =
        new(() => new ThreadSafeRunningConfig(), LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly ConcurrentStack<RunningConfig> _stackConfigs;

    private ThreadSafeRunningConfig()
    {
        _stackConfigs = new ConcurrentStack<RunningConfig>(
            new[]
            {
                new RunningConfig("http://127.0.0.1", 4723, "emulator-5554"),
            });
    }

    public static ThreadSafeRunningConfig Instance => _instance.Value;

    public RunningConfig Pop()
    {
        if (_stackConfigs.TryPop(out var config))
            return config;

        throw new InvalidOperationException("Stack configuration is empty");
    }
}

public class RunningConfig
{
    public RunningConfig(string appiumUrl, int appiumPort, string deviceId)
    {
        AppiumUrl = appiumUrl;
        AppiumPort = appiumPort;
        DeviceId = deviceId;
    }

    public string AppiumUrl { get; set; }
    public int AppiumPort { get; set; }
    public string DeviceId { get; set; }
}