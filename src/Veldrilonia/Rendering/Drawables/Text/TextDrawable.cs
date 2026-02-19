using System.Runtime.CompilerServices;
using UIFramework.Data;
using UIFramework.Rendering.Pipeline;
using UIFramework.Rendering.Shaders;
using Veldrid;

namespace UIFramework.Rendering.Drawables.Text;

public class TextDrawable(GraphicsDevice graphicsDevice, CommonResources commonResources, Texture fontTexture) : IDrawable<GlyphData>
{
    private Veldrid.Pipeline _pipeline;
    private ResourceLayout _resourceLayout;
    private ResourceSet _resourceSet;
    private DeviceBuffer _instanceBuffer;
    private GlyphData[] _data = [];

    public void Initialize()
    {
        var factory = graphicsDevice.ResourceFactory;

        // Charger les shaders
        var shaderManager = new ShaderManager(graphicsDevice);
        var shaders = shaderManager.LoadShader(
            "Rendering/Drawables/Text/TextVertex.glsl",
            "Rendering/Drawables/Text/TextFragment.glsl"
        );

        // Layouts
        var modelLayout = new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<InstanceModelData>(),
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );

        var instanceLayout = new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<GlyphData>(),
            new VertexElementDescription("GlyphPos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("GlyphSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("GlyphUvBounds", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("GlyphColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        )
        {
            InstanceStepRate = 1
        };

        var layoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("FontTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("FontSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        );
        _resourceLayout = factory.CreateResourceLayout(layoutDescription);

        // Pipeline
        var pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: false,
                comparisonKind: ComparisonKind.LessEqual
            ),
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(
                [modelLayout, instanceLayout],
                shaders
            ),
            Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription,
            ResourceLayouts = [_resourceLayout]
        };

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        // Resource Set
        var resourceSetDesc = new ResourceSetDescription(
            _resourceLayout,
            commonResources.UniformBuffer,
            fontTexture,
            CreateMdsfSampler(factory)
        );
        _resourceSet = factory.CreateResourceSet(resourceSetDesc);
    }

    private Sampler CreateMdsfSampler(ResourceFactory factory)
    {
        var samplerDesc = new SamplerDescription(
            addressModeU: SamplerAddressMode.Clamp,
            addressModeV: SamplerAddressMode.Clamp,
            addressModeW: SamplerAddressMode.Clamp,
            filter: SamplerFilter.MinLinear_MagLinear_MipLinear,
            comparisonKind: null,
            maximumAnisotropy: 0,
            minimumLod: 0,
            maximumLod: 8,
            lodBias: 0,
            borderColor: SamplerBorderColor.TransparentBlack
        );
        return factory.CreateSampler(samplerDesc);
    }

    public void UpdateInstances(GlyphData[] data)
    {
        _data = data;

        if (_instanceBuffer != null)
        {
            _instanceBuffer.Dispose();
        }

        var bufferDesc = new BufferDescription(
            (uint)(data.Length * Unsafe.SizeOf<GlyphData>()),
            BufferUsage.VertexBuffer
        );
        _instanceBuffer = graphicsDevice.ResourceFactory.CreateBuffer(bufferDesc);
        graphicsDevice.UpdateBuffer(_instanceBuffer, 0, data);
    }

    public void Update(float deltaTime)
    {
    }

    public void Draw(CommandList commandList)
    {
        if (_data.Length == 0) return;

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _resourceSet);
        commandList.SetVertexBuffer(0, commonResources.ModelBuffer);
        commandList.SetVertexBuffer(1, _instanceBuffer);
        commandList.SetIndexBuffer(commonResources.IndexBuffer, IndexFormat.UInt16);

        commandList.DrawIndexed(
            indexCount: (uint)commonResources.Indices.Length,
            instanceCount: (uint)_data.Length,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0
        );
    }

    public void Dispose()
    {
        _pipeline?.Dispose();
        _resourceLayout?.Dispose();
        _resourceSet?.Dispose();
        _instanceBuffer?.Dispose();
    }
}
