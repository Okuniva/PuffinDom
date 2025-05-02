using System;
using System.IO;
using System.Runtime.InteropServices;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Tools;

public class RunningOSTools
{
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static int? FreeMbOnHardDrive
    {
        get
        {
            try
            {
                var driveInfo = new DriveInfo(
                    Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory)!);

                if (driveInfo.IsReady)
                    return (int)(driveInfo.TotalFreeSpace / 1024 / 1024);

                Log.Write("Could not get free space on hard drive");

                return null;
            }
            catch (Exception e)
            {
                Log.Write(e, "Error while getting free space on hard drive");
                return null;
            }
        }
    }
}