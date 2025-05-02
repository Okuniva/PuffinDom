using System.Diagnostics.CodeAnalysis;

namespace PuffinDom.UI.Enums;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum DialogCanBeClosedTappingOutside
{
    Yes,
    No,
    YesOnAndroidOnly_SkipOnIOS,
}