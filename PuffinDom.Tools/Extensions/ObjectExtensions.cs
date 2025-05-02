using System.Diagnostics;
using JetBrains.Annotations;

namespace PuffinDom.Tools.Extensions;

public static class ObjectExtensions
{
    [ContractAnnotation("instanceOrNull: null => halt; instanceOrNull: notnull => notnull")]
    public static T NotNull<T>(this T? instanceOrNull, string? name = null)
        where T : class
    {
        if (instanceOrNull == null)
            throw new NullReferenceException($"reference {name ?? ""} is null but must not be");

        return instanceOrNull;
    }

    public static T NotNull<T>(this T? instanceOrNull, string? name = null)
        where T : struct
    {
        if (instanceOrNull == null)
            throw new NullReferenceException($"nullable value {name ?? ""} is null but must not be");

        return instanceOrNull.Value;
    }
}