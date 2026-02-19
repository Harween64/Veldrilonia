using System.Numerics;
using System.Runtime.InteropServices;

namespace UIFramework.Data;

[StructLayout(LayoutKind.Sequential)]
public struct InstanceModelData(Vector2 position)
{
    public Vector2 Position = position;
}
