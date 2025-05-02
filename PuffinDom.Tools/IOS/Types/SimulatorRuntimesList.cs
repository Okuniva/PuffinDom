using System.Linq;

namespace PuffinDom.Tools.IOS.Types;

public class SimulatorRuntimesList
{
    private const string IOSNamePrefix = "iOS";

    
    public SimulatorRuntime[] Runtimes { get; set; } = null!;

    
    public SimulatorRuntime[] GetAvailableIOSRuntimes()
    {
        return Runtimes.Where(runtime => runtime.IsAvailable && runtime.Name.StartsWith(IOSNamePrefix)).ToArray();
    }
}