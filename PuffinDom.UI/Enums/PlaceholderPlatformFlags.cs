using System.Diagnostics.CodeAnalysis;

namespace PuffinDom.UI.Enums;

[Flags]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PlaceholderPlatformFlags
{
    Android,
    OnlyNewAndroid,
    iOS,
    OnlyAndroid,
    DoNotValidate,
}