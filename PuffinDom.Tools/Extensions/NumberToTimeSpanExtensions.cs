namespace PuffinDom.Tools.Extensions;

public static class NumberToTimeSpanExtensions
{
    public static TimeSpan Seconds(this int seconds)
    {
        return TimeSpan.FromSeconds(seconds);
    }

    public static TimeSpan Seconds(this double seconds)
    {
        return TimeSpan.FromSeconds(seconds);
    }

    public static TimeSpan Minutes(this int minute)
    {
        return TimeSpan.FromMinutes(minute);
    }

    public static TimeSpan Minute(this int minute)
    {
        return TimeSpan.FromMinutes(minute);
    }

    public static TimeSpan Hours(this int hours)
    {
        return TimeSpan.FromHours(hours);
    }

    public static TimeSpan Hour(this int hours)
    {
        return Hours(hours);
    }

    public static TimeSpan Second(this int second)
    {
        return Seconds(second);
    }

    public static TimeSpan Milliseconds(this int milliseconds)
    {
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    public static TimeSpan Millisecond(this int millisecond)
    {
        return Milliseconds(millisecond);
    }

    public static TimeSpan Milliseconds(this double milliseconds)
    {
        return new TimeSpan((long)(milliseconds * 10000));
    }
}