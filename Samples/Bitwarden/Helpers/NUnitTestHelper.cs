using System.Reflection;
using NUnit.Framework;

namespace Bitwarden.Helpers;

public static class NUnitTestHelper
{
    public static bool TestHasAttribute<TType>()
        where TType : Attribute
    {
        return GetAttribute<TType>() != null;
    }

    public static TType? GetAttribute<TType>()
        where TType : Attribute
    {
        var currentTest = TestContext.CurrentContext.Test;

        return Assembly.GetCallingAssembly()
            .GetTypes()
            .First(t => t.FullName == currentTest.ClassName)
            .GetMethod(
                currentTest.MethodName!,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetCustomAttribute<TType>();
    }
}