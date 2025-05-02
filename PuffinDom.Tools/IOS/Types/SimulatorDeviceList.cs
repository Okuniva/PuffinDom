using System.Collections.Generic;
using System.Linq;

namespace PuffinDom.Tools.IOS.Types;

public class SimulatorDeviceList
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public Dictionary<string, SimulatorDevice[]> Devices { get; init; } = null!;

    public SimulatorDevice[] AllDevices => Devices.Values.SelectMany(devices => devices).ToArray();
}