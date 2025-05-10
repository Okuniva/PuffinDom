using Bitwarden.Helpers;
using Bitwarden.Helpers.Attributes;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools;
using PuffinDom.Tools.Droid;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;

namespace Bitwarden.TestLifecycle;

public class AfterEachTestAction
{
    private static bool IsPlatformEnabled
    {
        get
        {
            var testPlatform = NUnitTestHelper
                .GetAttribute<UITestAttribute>()
                .NotNull()
                .TestPlatform;

            return testPlatform switch
            {
                RunOn.AndroidOnly => UIContext.Android,
                RunOn.AndroidOnly_iOSInDevelopment => UIContext.Android,
                RunOn.iOSOnly => UIContext.iOS,
                RunOn.iOSOnly_AndroidInDevelopment => UIContext.iOS,
                RunOn.AllPlatforms => true,
                RunOn.Ignore => false,
                _ => throw new ArgumentOutOfRangeException(nameof(testPlatform), testPlatform, "Unsupported platform"),
            };
        }
    }

    public static bool Run()
    {
        UIContext.Device.StopDeviceLogStream();
        if (!IsPlatformEnabled) return false;

        RamUsage.Log();

        Exception? postException = null;
        try
        {
            CheckIfAndroidGmsModuleWasUpdatedDuringTheTest();
            CheckIfAndroidAppCrashed();
        }
        catch (Exception e)
        {
            Log.Write(e, "PostException thrown");
            postException = e;
        }
        finally
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed || postException != null)
                SaveResults(postException);
        }
        return true;
    }

    private static void CheckIfAndroidGmsModuleWasUpdatedDuringTheTest()
    {
        using var logContext = Log.PushContext("Check if com.google.android.gms updated during test");
        if (UIContext.Android
            && UIContext.Device.LogContains("Update system package com.google.android.gms"))
            throw new TechnicalCrashFailTestException(BitwardenConstants.GmsModuleWasUpdatedOnAndroidSoTheAppWasClosed);
    }

    private static void CheckIfAndroidAppCrashed()
    {
        if (UIContext.iOS)
            return;

        using var logContext = Log.PushContext("CheckIfAppCrashed");

        if (UIContext.Device.LogContainsRegex($"ActivityManager.*{BitwardenConstants.BundleId}.*crashed.*") ||
            UIContext.Device.LogContainsRegex($"ActivityManager.*{BitwardenConstants.BundleId}.*has died.*"))
        {
            Log.Write("App Crashed. Send adb tombstone bugreport");
            Adb.SaveApplicationTombstone(BitwardenConstants.BugreportFileName);

            throw new FailTestException(BitwardenConstants.AppCrashed, "Tombstone App Crashed");
        }

        AppModels.Views.Dialogs.AndroidAppKeepsStopping.AssertDoesNotExist(BitwardenConstants.AppCrashed);
    }

    private static void TakeAppLastSnapshot()
    {
        try
        {
            UIContext.Device.InvalidateCachedPageSource();
            Log.Write(UIContext.Device.GetScreenAsXml("Last Snapshot"));
            UIContext.Device.AndroidOpenedAppBundleId.IgnoreMethodResult();
        }
        catch (Exception e)
        {
            Log.Write(e, "Failed Get Last XML Snapshot");
        }
    }

    private static void SaveResults(Exception? postException)
    {
        TakeAppLastSnapshot();
    }
}