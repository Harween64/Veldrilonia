using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Veldridonia.Rendering.Features;

[StructLayout(LayoutKind.Sequential)]
public struct RectangleData
{
    public Vector2 Position;
    public Vector2 Size;
    public RgbaFloat Color;
    public float CornerRadius;
    public float BorderThickness;
    public RgbaFloat BorderColor;
    public float Depth;

    public RectangleData(Vector2 position, Vector2 size, RgbaFloat color, float cornerRadius, float borderThickness, RgbaFloat borderColor, float depth)
    {
        Position = position;
        Size = size;
        Color = color;
        CornerRadius = cornerRadius;
        BorderThickness = borderThickness;
        BorderColor = borderColor;
        Depth = depth;
    }
}
