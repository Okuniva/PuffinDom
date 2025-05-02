using System.Diagnostics.CodeAnalysis;

namespace PuffinDom.Infrastructure.Helpers.Device;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Emulator
{
    Android33,
    Android35,
    AndroidTablet21,
    iOSLatest,
}