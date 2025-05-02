using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PuffinDom.Tools;
using PuffinDom.Tools.Logging;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Service.Options;
using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Appium.Helpers;

namespace PuffinDom.Infrastructure.Appium.Helpers;

public class AppiumServerWrapper : IAppiumServerWrapper
{
    private AppiumLocalService? _server;

    public void RestartAppiumServer(bool force = true)
    {
        using var logContext = Log.PushContext("Starting Appium server");

        if (_server != null && !force)
        {
            Log.Write("Appium server is already running, skipping restart");
            return;
        }

        var maxTries = 3;

        if (_server != null)
        {
            _server.Dispose();
            _server = null;
        }

        ThrowIfAppiumServerRuns();

        while (_server == null)
            try
            {
                var arguments = new OptionCollector();
                arguments.AddArguments(new KeyValuePair<string, string>("--base-path", "/wd/hub"));

                var serverService = new AppiumServiceBuilder()
                    .WithArguments(arguments)
                    .UsingPort(PuffinConstants.AppiumPort)
                    .WithStartUpTimeOut(PuffinConstants.AppiumServerStartUpTimeOut)
                    .WithLogFile(new FileInfo(PuffinConstants.AppiumLogFileName))
                    .Build();

                serverService.OutputDataReceived += (_, e) => Log.Write($"Appium {e.Data}");
                serverService.Start();
                _server = serverService;

                var start = DateTime.Now;

                while (!_server.IsRunning)
                {
                    var elapsed = DateTime.Now.Subtract(start).Ticks;
                    if (elapsed >= PuffinConstants.AppiumServerMaxWaitTime.Ticks)
                    {
                        Log.Write($">>>>> {elapsed} ticks elapsed, timeout value is {PuffinConstants.AppiumServerMaxWaitTime.Ticks}");

                        throw new TimeoutException(
                            $"Timed out waiting for Appium server to start after waiting for {PuffinConstants.AppiumServerMaxWaitTime.Seconds}s");
                    }

                    Task.Delay(PuffinConstants.AppiumServerStartedChecksDelayBetweenRetries).Wait();
                }

                Log.Write("Appium server started");
            }
            catch (Exception e)
            {
                Log.Write(e, "Failed to start Appium server");

                if (maxTries > 0)
                {
                    if (_server != null)
                    {
                        _server.Dispose();
                        _server = null;
                    }

                    ThreadSleep.For(
                        PuffinConstants.AppiumServerStartRetriesDelay,
                        "Retrying to start Appium server");

                    maxTries--;
                }
                else
                {
                    Log.Write("Failed to start Appium server after 3 tries");
                    throw;
                }
            }
    }

    private static void ThrowIfAppiumServerRuns()
    {
        try
        {
            using var httpClient = new HttpClient();

            var serverRunning = httpClient
                .GetAsync("http://127.0.0.1:4723/wd/hub/sessions")
                .Result
                .IsSuccessStatusCode;

            if (serverRunning)
                throw new InvalidOperationException(
                    "Appium server is already running but not connected to current managed code, skipping restart");
        }
        catch (Exception)
        {
            //Log.Write(e, "Failed to check if Appium server is running. Probably not running");
        }
    }

    public void Dispose()
    {
        Log.Write("Stopping existing Appium server");

        if (_server == null)
            return;

        _server.Dispose();
        _server = null;
    }
}