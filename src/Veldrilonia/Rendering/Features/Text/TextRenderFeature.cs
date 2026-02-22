using System.Runtime.CompilerServices;
using Veldridonia.Rendering.Pipeline;
using Veldrid;

namespace Veldridonia.Rendering.Features;

public sealed class TextRenderFeature(GraphicsDevice graphicsDevice, CommonResources commonResources, Texture fontTexture)
    : RenderFeatureBase<GlyphData>(graphicsDevice, commonResources)
{
    private readonly Texture _fontTexture = fontTexture ?? throw new ArgumentNullException(nameof(fontTexture));

    public override void Initialize()
    {
        var factory = GraphicsDevice.ResourceFactory;

        // Charger les shaders
        var shaderManager = new ShaderManager(GraphicsDevice);
        _shaderSet = shaderManager.LoadShader(
            "Rendering/Features/Text/TextVertex.glsl",
            "Rendering/Features/Text/TextFragment.glsl"
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
                [.. _shaderSet]
            ),
            Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription,
            ResourceLayouts = [_resourceLayout]
        };

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        // Resource Set
        var resourceSetDesc = new ResourceSetDescription(
            _resourceLayout,
            CommonResources.UniformBuffer,
            _fontTexture,
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

    public override void Update(float deltaTime)
    {
    }

    public override void Draw(CommandList commandList)
    {
        if (_data.Length == 0) return;

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _resourceSet);
        commandList.SetVertexBuffer(0, CommonResources.ModelBuffer);
        commandList.SetVertexBuffer(1, _instanceBuffer);
        commandList.SetIndexBuffer(CommonResources.IndexBuffer, IndexFormat.UInt16);

        commandList.DrawIndexed(
            indexCount: (uint)CommonResources.Indices.Length,
            instanceCount: (uint)_data.Length,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0
        );
    }
}
