using PuffinDom.Infrastructure.Appium;
using PuffinDom.Infrastructure.Appium.Helpers;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;
using PuffinDom.Tools.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;
using PuffinDom.Tools.Extensions;
using Platform = PuffinDom.Infrastructure.Helpers.Platform;
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

namespace PuffinDom.Infrastructure.Appium;

internal class AppiumDriverWrapper : IDisposable
{
    private readonly DeviceManager _deviceManager;
    private readonly IOSFilterXml _iosFilterXml;
    private string _cachedPageSource = "";
    private AppiumDriver? _driver;

    public AppiumDriverWrapper(
        DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
        _iosFilterXml = new IOSFilterXml(deviceManager);
    }

    public string PageSource(string contextName, bool full = false)
    {
        if (_cachedPageSource.HasChars())
            return _cachedPageSource;

        _deviceManager.TakeScreenshot(contextName, assert: false);

        using var logContext = Log.PushContext("Getting page source");

        try
        {
            RunAppiumCommand(() => _cachedPageSource = GetPageSource(full));
        }
        catch (Exception e)
        {
            Log.Write(e, "Failed to get Page Source with Appium");
            throw;
        }

        Log.WriteInDebug($"Xml Debug: {_cachedPageSource.Replace(Environment.NewLine, "")}");
        return _cachedPageSource;
    }

    public void RecreateDriver(bool startTouchApp = true)
    {
        Stop();

        _driver = _deviceManager.Platform == Platform.Android
            ? DroidAppiumDriverFactory.Create()
            : IOSAppiumDriverFactory.Create(startTouchApp);

        InvalidateCachedPageSource();
    }

    private void RunAppiumCommand(Action action)
    {
        try
        {
            action();
        }
        catch (NoSuchElementException)
        {
            throw;
        }
        catch (WebDriverException e)
        {
            Log.Write(e, "Failed to run Appium command with Appium Exception");
            Log.Write("Restarting appium server and driver");
            var restartIosApp = e is not StaleElementReferenceException;
            _deviceManager.RestartAppiumFully(restartIosApp);

            try
            {
                action();
            }
            catch (Exception finalException)
            {
                Log.Write(e, "Failed to run Appium command");
                throw new TechnicalCrashFailTestException("Failed to run Appium command", finalException);
            }
        }
    }

    public void InvalidateCachedPageSource()
    {
        _cachedPageSource = "";
    }

    public void EnterText(string text)
    {
        IWebElement? element = null;

        RunAppiumCommand(
            () => element = _driver?.SwitchTo().ActiveElement());

        element?.SendKeys(text);

        InvalidateCachedPageSource();
    }

    public void ClearText()
    {
        RunAppiumCommand(
            () => _driver?
                .SwitchTo()
                .ActiveElement()
                .Clear());

        InvalidateCachedPageSource();
    }

    public void TapCoordinates(
        int x,
        int y,
        int times = 1,
        TimeSpan? pauseBetweenTaps = null,
        TimeSpan? timeToHold = null)
    {
        InvalidateCachedPageSource();

        if (times < 1)
            throw new ArgumentOutOfRangeException(nameof(times), "Times should be greater than 0");

        pauseBetweenTaps ??= PuffinConstants.DefaultDelayBetweenSingularTaps;

        var touchDevice = new PointerInputDevice(PointerKind.Touch);
        var sequence = new ActionSequence(touchDevice);

        sequence
            .SetPoint(touchDevice, x, y)
            .Down(touchDevice)
            .Wait(
                touchDevice,
                timeToHold ?? 50.Milliseconds())
            .Up(touchDevice);

        for (var i = 1; i < times; i++)
            sequence
                .Wait(touchDevice, pauseBetweenTaps.Value)
                .Down(touchDevice)
                .Wait(
                    touchDevice,
                    timeToHold ?? 50.Milliseconds())
                .Up(touchDevice);

        if (times > 1)
            Log.Write(
                $"Appium tapping {x}, {y} for {times} times with pause between taps:{pauseBetweenTaps.Value.ToDisplayString()}");
        else if (timeToHold != null)
            Log.Write(
                $"Appium tapping {x}, {y} with hold duration: {timeToHold.Value.ToDisplayString()}");
        else
            Log.Write(
                $"Appium tapping [{x}, {y}]");

        RunAppiumCommand(() => _driver?.PerformActions(new List<ActionSequence> { sequence }));
    }

    private string GetPageSource(bool full = false)
    {
        return _deviceManager.Platform switch
        {
            Platform.Android => _driver?.PageSource,
            Platform.iOS =>
                full
                    ? IOSFilterXml.FilterFullPageXml(_driver?.PageSource!)
                    : _iosFilterXml.FilterXml(
                        _driver?
                            .ExecuteScript(
                                "mobile: source",
                                new Dictionary<string, object>
                                {
                                    { "format", "xml" },
                                    { "excludedAttributes", "visible,index,accessible,type" },
                                })
                            .ToString() ?? throw new Exception("Failed to get page source")
                    ),
            _ => throw new ArgumentOutOfRangeException(),
        } ?? throw new InvalidOperationException();
    }

    public void PressHomeButton()
    {
        using var logContext = Log.PushContext("Press home button");
        _driver?
            .ExecuteScript(
                "mobile: pressButton",
                new Dictionary<string, object>
                {
                    { "name", "home" },
                });
    }

    public void DragCoordinates(
        int fromX,
        int fromY,
        int toX,
        int toY,
        bool preventInertia = true,
        TimeSpan? duration = null)
    {
        InvalidateCachedPageSource();

        var touchDevice = new PointerInputDevice(PointerKind.Touch);
        duration ??= PuffinConstants.DefaultDragDuration;

        Log.Write(
            $"Dragging from {fromX}, {fromY} to {toX}, {toY} " +
            $"with duration {duration.Value.ToDisplayString()} " +
            $"and {(preventInertia ? "inertia prevented" : "inertia allowed")}");

        var sequence = new ActionSequence(touchDevice)
            .SetPoint(touchDevice, fromX, fromY)
            .Down(touchDevice)
            .SetPoint(touchDevice, toX, toY, duration);

        if (preventInertia)
            sequence = sequence.Wait(touchDevice, 20.Milliseconds());

        sequence = sequence.Up(touchDevice);

        RunAppiumCommand(
            () => _driver?
                .PerformActions(
                [
                    sequence,
                ]));
    }

    public bool IsKeyboardVisible()
    {
        var isKeyboardShown = false;

        RunAppiumCommand(
            () => isKeyboardShown = _driver!.IsKeyboardShown());

        return isKeyboardShown;
    }

    public void Stop()
    {
        _driver?.Dispose();
        _driver = null;
    }

    public void RestartDriverIfNeeded()
    {
        if (_driver == null)
            RecreateDriver();
        else
            Log.Write("Appium Driver is already created, skipping recreation");
    }

    public void Dispose()
    {
        Log.Write("Disposing Appium Driver");

        _driver?.Dispose();
    }
}