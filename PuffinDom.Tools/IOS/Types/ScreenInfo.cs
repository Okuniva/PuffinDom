namespace PuffinDom.Tools.IOS.Types;

public class ScreenInfo
{
    public ScreenInfo(double density, int width, int height)
    {
        Density = density;
        Width = width;
        Height = height;
    }

    public double Density { get; }
    public int Width { get; }
    public int Height { get; }
}