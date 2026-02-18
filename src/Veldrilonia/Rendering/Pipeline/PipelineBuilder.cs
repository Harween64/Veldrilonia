using System.Numerics;
using System.Runtime.CompilerServices;
using UIFramework.Data;
using Veldrid;

namespace UIFramework.Rendering.Pipeline;

public class PipelineBuilder
{
    private readonly GraphicsDevice _graphicsDevice;

    public PipelineBuilder(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public VertexLayoutDescription CreateInstanceModelLayout()
    {
        return new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<InstanceModelData>(),
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );
    }

    public VertexLayoutDescription CreateInstanceLayout()
    {
        return new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<UIInstanceData>(),
            new VertexElementDescription("InstancePos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("InstanceSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("InstanceColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("InstanceCornerRadius", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("InstanceBorderThickness", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription("InstanceBorderColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("InstanceDepth", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1)
        )
        {
            InstanceStepRate = 1
        };
    }

    public VertexLayoutDescription CreateGlyphLayout()
    {
        return new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<UIGlyphData>(),
            new VertexElementDescription("GlyphPos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("GlyphSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("GlyphUvBounds", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("GlyphColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        )
        {
            InstanceStepRate = 1 // Très important : 1 instance = 1 lettre
        };
    }

    public ResourceLayout CreateGlyphResourceLayout()
    {
        var layoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("FontTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("FontSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        );
        
        return _graphicsDevice.ResourceFactory.CreateResourceLayout(layoutDescription);
    }

    public ResourceLayout CreateResourceLayout()
    {
        var layoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        );
        
        return _graphicsDevice.ResourceFactory.CreateResourceLayout(layoutDescription);
    }

    public Veldrid.Pipeline CreateGraphicsPipeline(
        Shader[] shaders, 
        VertexLayoutDescription modelLayout, 
        VertexLayoutDescription instanceLayout,
        ResourceLayout resourceLayout)
    {
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
            
            Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription,
            ResourceLayouts = [resourceLayout]
        };

        return _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
    }
}
