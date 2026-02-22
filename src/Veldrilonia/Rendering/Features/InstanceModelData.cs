using System.Numerics;
using System.Runtime.InteropServices;

namespace Veldridonia.Rendering.Features;

[StructLayout(LayoutKind.Sequential)]
public struct InstanceModelData(Vector2 position)
{
    public Vector2 Position = position;
}
