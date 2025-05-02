using System;

namespace PuffinDom.Tools.Droid.Enums;

[Flags]
public enum EmulatorStartupFlags
{
    NoWindow,
    NoSnapshots,
    WipeData,
}