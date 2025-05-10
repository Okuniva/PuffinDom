using Bitwarden.Helpers;
using Bitwarden.Helpers.Attributes;
using Bitwarden.Helpers.Extensions;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI;

namespace Bitwarden.TestLifecycle;

public class BeforeEachTestAction
{
    public static void Run()
    {
        SetUpContext();
        using var logContext = Log.PushContext($"BeforeEachTest. Test #{GlobalContext.TestIndex}");

        IgnoreTestIfNeeded();

        LogTestMetadata();
        ValidateFreeSpaceOnHostMachine();

        UIContext.Device
            .BootDeviceIfNeeded()
            .StartAppiumIfNeeded();

        LocalTestContext.Reset();

        if (UIContext.Android)
            SetupAndroidDeviceTime();
    }

    private static void SetupAndroidDeviceTime()
    {
        try
        {
            UIContext.Device.SetAndroidDeviceTime();

            Log.Write("Android device time set successfully");
        }
        catch (Exception ex)
        {
            Log.Write($"Failed to set Android device time: {ex.Message}");
        }
    }

    private static void SetUpContext()
    {
        GlobalContext.TestStartTime = DateTime.Now;
        Log.Write($"TestStartTime: {GlobalContext.TestStartTime}\n");
        GlobalContext.TestIndex++;
    }

    private static void ValidateFreeSpaceOnHostMachine()
    {
        // ReSharper disable once InvertIf
        if (RunningOSTools.FreeMbOnHardDrive < BitwardenConstants.MinFreeSpaceOnHardDriveMb)
        {
            Log.Write($"Free space on hard drive is less than {BitwardenConstants.MinFreeSpaceOnHardDriveMb} MB");
            throw new TechnicalCrashFailTestException(BitwardenConstants.NotEnoughFreeSpaceOnHardDrive);
        }
    }

    private static void IgnoreTestIfNeeded()
    {
        var uiTestAttribute = NUnitTestHelper
            .GetAttribute<UITestAttribute>()
            .NotNull();

        IgnoreTestIfPlatformUnsupported(uiTestAttribute);
    }

    private static void IgnoreTestIfPlatformUnsupported(UITestAttribute uiTestAttribute)
    {
        uiTestAttribute
            .TestPlatform.ThrowIfPlatformIsNotEnabled(uiTestAttribute.PlatformIgnoringReasonMessage);
    }

    private static void LogTestArguments()
    {
        var testParameters =
            TestContext.CurrentContext.Test.Arguments.Where(arg => arg != null).Cast<object>().ToList();

        if (!testParameters.Any())
            return;

        Log.Write("Test arguments:");
        foreach (var argument in testParameters)
            Log.Write($"  {argument.GetType().Name}: {argument}");
    }

    private static void LogTestMetadata()
    {
        Log.Write($"Platform: {UIContext.Platform}");
        Log.Write($"TestName: {TestContext.CurrentContext.Test.Name}");
        LogTestArguments();
        Log.Write($"Date: {DateTime.Now.ToString(BitwardenConstants.TestFullDisplayDateFormat)}\n");
        RamUsage.Log();
    }
}