using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace PuffinDom.UI.Asserts;

public static class TypedAssert
{
    public static void AreEqual<T>(T? expected, T? actual)
    {
        Assert.AreEqual(expected, actual);
    }

    public static void AreNotEqual<T>(T? expected, T? actual)
    {
        Assert.AreNotEqual(expected, actual);
    }

    public static void AreEqual<T>(T? expected, T? actual, string message, params object?[]? args)
    {
        Assert.AreEqual(expected, actual, message, args);
    }

    public static void AreNotEqual<T>(T? expected, T? actual, string message, params object?[]? args)
    {
        Assert.AreNotEqual(expected, actual, message, args);
    }

    public static void AreSame<T>(T? expected, T? actual)
    {
        Assert.AreSame(expected, actual);
    }
}