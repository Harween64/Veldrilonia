using System.Numerics;
using System.Runtime.InteropServices;

namespace Veldridonia.Core.Svg;

/// <summary>
/// Vertex tesselle d'un SVG, pret pour le GPU.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SvgVertex
{
    public Vector2 Position;
    public Vector4 Color;

    public SvgVertex(Vector2 position, Vector4 color)
    {
        Position = position;
        Color = color;
    }
}

/// <summary>
/// Maillage triangule issu de la tessellation d'un fichier SVG.
/// </summary>
public sealed class SvgMeshData
{
    public SvgVertex[] Vertices { get; }
    public uint[] Indices { get; }

    public SvgMeshData(SvgVertex[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }

    /// <summary>
    /// Fusionne plusieurs maillages en un seul (batching pour un seul draw call).
    /// </summary>
    public static SvgMeshData Merge(IReadOnlyList<SvgMeshData> meshes)
    {
        int totalVertices = 0;
        int totalIndices = 0;
        foreach (var mesh in meshes)
        {
            totalVertices += mesh.Vertices.Length;
            totalIndices += mesh.Indices.Length;
        }

        var vertices = new SvgVertex[totalVertices];
        var indices = new uint[totalIndices];

        int vertexOffset = 0;
        int indexOffset = 0;
        foreach (var mesh in meshes)
        {
            Array.Copy(mesh.Vertices, 0, vertices, vertexOffset, mesh.Vertices.Length);
            for (int i = 0; i < mesh.Indices.Length; i++)
            {
                indices[indexOffset + i] = mesh.Indices[i] + (uint)vertexOffset;
            }
            vertexOffset += mesh.Vertices.Length;
            indexOffset += mesh.Indices.Length;
        }

        return new SvgMeshData(vertices, indices);
    }
}
