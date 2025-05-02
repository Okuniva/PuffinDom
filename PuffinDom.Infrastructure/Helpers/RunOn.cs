using System.Diagnostics.CodeAnalysis;

namespace PuffinDom.Infrastructure.Helpers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum RunOn
{
    AndroidOnly,
    AndroidOnly_iOSInDevelopment,

    iOSOnly,
    iOSOnly_AndroidInDevelopment,

    AllPlatforms,
    Ignore,
}