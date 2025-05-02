using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using PuffinDom.Infrastructure.Helpers.Device;

namespace PuffinDom.UnitTests;

[TestFixture]
public class EmulatorNameTestFixture
{
    [Theory]
    [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
    public void EmulatorNameTest(Emulator emulator)
    {
        var containsAndroid = emulator.ToString().Contains("Android");
        var containsIOS = emulator.ToString().Contains("iOS");

        if (containsAndroid && containsIOS)
            Assert.Fail("The emulator name shouldn't contain 'Android' and 'iOS' substrings in the same time");

        if (!containsAndroid && !containsIOS)
            Assert.Fail("The emulator name should contain either 'Android' or 'iOS' substring.");
    }
}