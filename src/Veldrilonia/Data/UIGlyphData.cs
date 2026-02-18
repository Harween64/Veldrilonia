using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace UIFramework.Data;

[StructLayout(LayoutKind.Sequential)]
public struct UIGlyphData(
    Vector2 position, 
    Vector2 size, 
    Vector4 uvBounds,
    RgbaFloat color)
{
    public Vector2 Position = position;
    public Vector2 Size = size;
    public Vector4 UvBounds = uvBounds; 
    public Vector4 Color = color.ToVector4();
}