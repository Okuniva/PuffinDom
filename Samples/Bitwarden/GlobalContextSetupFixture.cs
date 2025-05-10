using System.IO;
using Bitwarden;
using NUnit.Framework;
using PuffinDom.Infrastructure;
using PuffinDom.UI;

// SetupFixture runs once for all tests under the same namespace,
// It placed outside the namespace to run once for all tests in the assembly
// https://docs.nunit.org/articles/nunit/writing-tests/attributes/setupfixture.html

[assembly: LevelOfParallelism(1)]

// ReSharper disable once CheckNamespace
[SetUpFixture]
#pragma warning disable CA1050
public class UIContextSetupFixture
#pragma warning restore CA1050
{
    [OneTimeSetUp]
    public void RunBeforeTestsRunning()
    {
        CoreEnvironmentVariables.ReloadEnvironmentVariables();
        UIContext.Device.StartAppiumIfNeeded();
        UIContext.Device.StartMacOSPingStream();
    }

    [OneTimeTearDown]
    public void RunAfterAllTestsRun()
    {
        UIContext.Device.Dispose();
    }
}