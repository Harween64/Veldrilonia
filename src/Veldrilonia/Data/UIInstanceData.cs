using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace UIFramework.Data;

[StructLayout(LayoutKind.Sequential)]
public struct UIInstanceData(
    Vector2 position, 
    Vector2 size, 
    RgbaFloat color, 
    float cornerRadius = 0, 
    float borderThickness = 0, 
    RgbaFloat? borderColor = null, 
    float depth = 0)
{
    public Vector2 Position = position;
    public Vector2 Size = size;
    public Vector4 Color = color.ToVector4();
    public float CornerRadius = cornerRadius;
    public float BorderThickness = borderThickness;
    public Vector4 BorderColor = (borderColor ?? color).ToVector4();
    public float Depth = depth;
}
