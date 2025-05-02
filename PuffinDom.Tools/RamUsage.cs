using System;

namespace PuffinDom.Tools;

public class RamUsage
{
    public static void Log()
    {
        Logging.Log.Write(
            $"RAM usage: " +
            $"{GC.GetTotalMemory(false) / (1024.0 * 1024.0):F2} MB");
    }
}