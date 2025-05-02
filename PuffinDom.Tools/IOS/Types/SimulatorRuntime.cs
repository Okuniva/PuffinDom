using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace PuffinDom.Tools.IOS.Types;

[DebuggerDisplay("{" + nameof(Name) + "}")]
public class SimulatorRuntime
{
    private string _version = null!;

    
    public string Identifier { get; set; } = null!;

    
    public string Version
    {
        get => _version;
        set
        {
            _version = value;
            ParsedVersion = System.Version.Parse(value);
        }
    }

    
    public bool IsAvailable { get; set; }

    
    public string Name { get; set; } = null!;

    
    public SimulatorRuntimeSupportedDevice[] SupportedDeviceTypes { get; set; } = null!;

    [JsonIgnore]
    
    public Version ParsedVersion { get; private set; } = null!;
}