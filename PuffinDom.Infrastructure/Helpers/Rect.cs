using System;

namespace PuffinDom.Infrastructure.Helpers;

public class Rect : IEquatable<Rect>
{
    public Rect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int Height { get; }

    public int Width { get; }

    public int Y { get; }

    public int X { get; }

    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;
    public int RightX => X + Width;

    
    public int BottomY => Y + Height;

    public bool Equals(Rect? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}, Width: {Width}, Height: {Height}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Rect other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Height, Width, Y, X);
    }
}