using System.Diagnostics;

namespace PuffinDom.Tools.IOS.Types;

[DebuggerDisplay("{Name} - {UDID}")]
public class SimulatorDevice
{
    // ReSharper disable once InconsistentNaming
    
    public string UDID { get; init; } = null!;

    
    public bool IsAvailable { get; set; }

    
    public string Name { get; init; } = null!;

    
    public string DeviceTypeIdentifier { get; init; } = null!;

    
    public string State { get; init; } = null!;
}