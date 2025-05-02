using System;
using System.Text.RegularExpressions;
using PuffinDom.Tools.Droid.Enums;

namespace PuffinDom.Tools.Droid;


public class ConnectedDevice
{
    private static readonly Regex _emulatorPortRegex = new(@"emulator-(\d+)");

    public ConnectedDevice(string serial, ConnectedDeviceState state)
    {
        Serial = serial ?? throw new ArgumentNullException(nameof(serial));
        State = state;

        var match = _emulatorPortRegex.Match(Serial);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var newPort))
            Port = newPort;
        else
            Port = -1;
    }

    public bool IsEmulator => Port != -1;

    public string Serial { get; }

    public ConnectedDeviceState State { get; }

    public int Port { get; }
}