using System.Diagnostics;

namespace PuffinDom.Tools.IOS.Types;

[DebuggerDisplay("{" + nameof(Name) + "}")]
public class SimulatorRuntimeSupportedDevice
{
    
    public string Name { get; set; } = null!;

    
    public string Identifier { get; set; } = null!;

    
    public string ProductFamily { get; set; } = null!;
}