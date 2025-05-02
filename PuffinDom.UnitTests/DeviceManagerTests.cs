using System;
using System.Linq;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;

namespace PuffinDom.UnitTests;

[TestFixture]
public class DeviceManagerTests
{
    [Test]
    public void NoDeviceManagerMethodsReturnVoid()
    {
        var badMethods = typeof(DeviceManager)
            .GetMethods()
            .Where(x => x.ReturnType == typeof(void) && x.Name != nameof(IDisposable.Dispose))
            .ToList();

        if (badMethods.Any())
            Assert.Fail(badMethods.Aggregate(string.Empty,
                (current, method) => $"{current}{Environment.NewLine}{method}"));
    }
}