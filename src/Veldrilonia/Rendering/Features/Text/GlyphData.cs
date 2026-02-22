using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Veldridonia.Rendering.Features;

[StructLayout(LayoutKind.Sequential)]
public struct GlyphData
{
    public Vector2 Position;
    public Vector2 Size;
    public Vector4 UvBounds;
    public RgbaFloat Color;

    public GlyphData(Vector2 position, Vector2 size, Vector4 uvBounds, RgbaFloat color)
    {
        Position = position;
        Size = size;
        UvBounds = uvBounds;
        Color = color;
    }
}
