namespace PuffinDom.Tools.Extensions;

public static class CastExtensions
{
    public static T CastTo<T>(this object @object) => (T)@object;

    public static int ToInt(this float @float) => (int)@float;

    public static int ToInt(this double value) => (int)value;

    public static double ToDouble(this int value) => value;

    public static double ToDouble(this float value) => value;

    public static double ToDouble(this decimal value) => (double)value;
}