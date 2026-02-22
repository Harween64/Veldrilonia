using System.Numerics;
using System.Runtime.CompilerServices;
using Veldridonia.Rendering.Features;
using Veldrid;

namespace Veldridonia.Rendering.Pipeline;

public class CommonResources
{
    private readonly GraphicsDevice _graphicsDevice;

    public DeviceBuffer? IndexBuffer { get; private set; }
    public DeviceBuffer? UniformBuffer { get; private set; }
    public DeviceBuffer? ModelBuffer { get; private set; }

    public ushort[] Indices { get; private set; }
    public Vector2[] QuadVertices { get; private set; }

    public CommonResources(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

       // Indices pour 2 triangles formant un quad
        Indices = [0, 1, 2, 0, 2, 3];

        // Vertices du quad (0,0) à (1,1)
        QuadVertices =
        [
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        ];
    }

    public void CreateBuffers(Matrix4x4 projection)
    {
        // Index Buffer
        var ibDescription = new BufferDescription(
            (uint)(Indices.Length * sizeof(ushort)),
            BufferUsage.IndexBuffer
        );
        IndexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(ibDescription);
        _graphicsDevice.UpdateBuffer(IndexBuffer, 0, Indices);

        // Uniform Buffer (Projection)
        var ubDescription = new BufferDescription(
            (uint)Unsafe.SizeOf<Matrix4x4>(),
            BufferUsage.UniformBuffer
        );
        UniformBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(ubDescription);
        _graphicsDevice.UpdateBuffer(UniformBuffer, 0, ref projection);

        // Model Buffer (Quad vertices)
        var modelBufferDesc = new BufferDescription(
            (uint)(QuadVertices.Length * Unsafe.SizeOf<InstanceModelData>()),
            BufferUsage.VertexBuffer
        );
        ModelBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(modelBufferDesc);
        _graphicsDevice.UpdateBuffer(ModelBuffer, 0, QuadVertices);
    }

    public void Dispose()
    {
        IndexBuffer?.Dispose();
        UniformBuffer?.Dispose();
        ModelBuffer?.Dispose();
    }
}
