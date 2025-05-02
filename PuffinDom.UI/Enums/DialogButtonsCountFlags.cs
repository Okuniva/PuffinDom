namespace PuffinDom.UI.Enums;

// ReSharper disable InconsistentNaming
[Flags]
public enum DialogButtonsCountFlags
{
    Zero,
    One,
    OneOnAndroid_TwoOnIOS,
    OneOnAndroid_ThreeOnIOS,
    Two,
    TwoOnAndroid_SkipOnIOS,
    Three,
    Four,
    Five,
    SkipVerificationDueBugOnAndroid,
    DoesNotMatter,
}