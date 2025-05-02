using System;

namespace PuffinDom.Tools.Logging;

public static class TimeSpanExtensions
{
    public static string ToDisplayString(this TimeSpan timeSpan)
    {
        return timeSpan switch
        {
            { Hours: 0, Minutes: 0, Seconds: 0, Milliseconds: 0 } => "0 ms.",
            { Hours: 0, Minutes: 0, Seconds: 0, Milliseconds: > 0 } => $"{timeSpan.TotalSeconds:0.###} sec.",
            { Hours: 0, Minutes: 0, Seconds: > 0 } => $"{timeSpan.TotalSeconds:0.###} sec.",
            { Hours: 0, Minutes: > 0, Seconds: 0 } => $"{timeSpan.Minutes} min.",
            { Hours: 0, Minutes: > 0, Seconds: > 0 } => $"{timeSpan.Minutes} min. {timeSpan.Seconds} sec.",
            { Hours: > 0, Minutes: 0, Seconds: 0 } => $"{timeSpan.Hours} hours",
            { Hours: > 0, Minutes: > 0, Seconds: 0 } => $"{timeSpan.Hours} hours {timeSpan.Minutes} min.",
            _ => $"{timeSpan.Hours} h. {timeSpan.Minutes} min. {timeSpan.Seconds} sec.",
        };
    }
}