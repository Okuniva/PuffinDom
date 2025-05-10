using System.Diagnostics.CodeAnalysis;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;
using PuffinDom.UI;

namespace Bitwarden.TestLifecycle;

[AllureNUnit]
[AllureDisplayIgnored]
public abstract class UITestFixtureBase
{
    [SetUp]
    public virtual void BeforeEachTest()
    {
        BeforeEachTestAction.Run();
        StartApp();
    }

    protected virtual void StartApp() => StartBitwardenApplicationAction.Run();

    [TearDown]
    public virtual void TearDownMethod()
    {
        AfterEachTestAction.Run();
    }
}