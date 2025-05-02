namespace PuffinDom.UI.Extensions;

public static class IntExtensions
{
    public static int ConvertFromDpToPx(this int value)
    {
        return (int)(value * UIContext.Device.Density);
    }
}