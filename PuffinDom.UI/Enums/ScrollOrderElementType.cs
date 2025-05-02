using System.Diagnostics.CodeAnalysis;

namespace PuffinDom.UI.Enums;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum ScrollOrderElementType
{
    Scrollable,
    Fixed,
    FixedOnAndroid_NotFixedOnIOS,
}