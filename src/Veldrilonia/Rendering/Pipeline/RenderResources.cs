using System.Numerics;
using System.Runtime.CompilerServices;
using UIFramework.Data;
using Veldrid;

namespace UIFramework.Rendering.Pipeline;

public class RenderResources
{
    private readonly GraphicsDevice _graphicsDevice;
    
    public DeviceBuffer IndexBuffer { get; private set; }
    public DeviceBuffer UniformBuffer { get; private set; }
    public DeviceBuffer ModelBuffer { get; private set; }
    public DeviceBuffer InstanceBuffer { get; private set; }
    public DeviceBuffer GlyphInstanceBuffer { get; private set; }
    public ResourceSet ResourceSet { get; private set; }
    public ResourceSet GlyphResourceSet { get; private set; }
    public ushort[] Indices { get; private set; }
    public Vector2[] QuadVertices { get; private set; }

    public RenderResources(GraphicsDevice graphicsDevice)
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

    public void UpdateInstanceBuffer(UIInstanceData[] instances)
    {
        if (InstanceBuffer != null)
        {
            InstanceBuffer.Dispose();
        }

        var instanceBufferDesc = new BufferDescription(
            (uint)(instances.Length * Unsafe.SizeOf<UIInstanceData>()),
            BufferUsage.VertexBuffer
        );
        InstanceBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(instanceBufferDesc);
        _graphicsDevice.UpdateBuffer(InstanceBuffer, 0, instances);
    }

    public void UpdateInstanceBuffer(UIGlyphData[] glyphs)
    {
        if (GlyphInstanceBuffer != null)
        {
            GlyphInstanceBuffer.Dispose();
        }

        var instanceBufferDesc = new BufferDescription(
            (uint)(glyphs.Length * Unsafe.SizeOf<UIGlyphData>()),
            BufferUsage.VertexBuffer
        );
        GlyphInstanceBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(instanceBufferDesc);
        _graphicsDevice.UpdateBuffer(GlyphInstanceBuffer, 0, glyphs);
    }

    public void CreateResourceSet(ResourceLayout resourceLayout)
    {
        var resourceSetDesc = new ResourceSetDescription(resourceLayout, UniformBuffer);
        ResourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(resourceSetDesc);
    }

    public void CreateGlyphResourceSet(ResourceLayout textResourceLayout, Texture fontTexture)
    {
        GlyphResourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(textResourceLayout, 
                UniformBuffer,
                fontTexture,
                CreateMdsfSampler()
            )
        );
    }

    public Sampler CreateMdsfSampler()
    {
        // Crée une description pour un sampler parfait pour le MSDF
        var samplerDesc = new SamplerDescription(
            addressModeU: SamplerAddressMode.Clamp, // Ne pas répéter horizontalement
            addressModeV: SamplerAddressMode.Clamp, // Ne pas répéter verticalement
            addressModeW: SamplerAddressMode.Clamp, // (Inutile pour la 2D, mais on le met)
            filter: SamplerFilter.MinLinear_MagLinear_MipLinear, // Filtrage linéaire (très important !)
            comparisonKind: null,
            maximumAnisotropy: 0,
            minimumLod: 0,
            maximumLod: 8, // Permet les mipmaps si disponibles
            lodBias: 0,
            borderColor: SamplerBorderColor.TransparentBlack
        );

        // Crée le sampler
       return _graphicsDevice.ResourceFactory.CreateSampler(samplerDesc);
    }

    public void Dispose()
    {
        IndexBuffer?.Dispose();
        UniformBuffer?.Dispose();
        ModelBuffer?.Dispose();
        InstanceBuffer?.Dispose();
        ResourceSet?.Dispose();
        GlyphResourceSet?.Dispose();
    }
}
