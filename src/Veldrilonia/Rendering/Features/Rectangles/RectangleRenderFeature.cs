using System.Runtime.CompilerServices;
using Veldridonia.Rendering.Pipeline;
using Veldrid;

namespace Veldridonia.Rendering.Features;

public sealed class RectangleRenderFeature(GraphicsDevice graphicsDevice, CommonResources commonResources)
    : RenderFeatureBase<RectangleData>(graphicsDevice, commonResources)
{
    public override void Initialize()
    {
        var factory = GraphicsDevice.ResourceFactory;

        // Charger les shaders
        var shaderManager = new ShaderManager(GraphicsDevice);
        _shaderSet = shaderManager.LoadShader(
            "Rendering/Features/Rectangles/RectangleVertex.glsl",
            "Rendering/Features/Rectangles/RectangleFragment.glsl"
        );

        // Layouts
        var modelLayout = new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<InstanceModelData>(),
            [
                new VertexElementDescription("vPos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            ]
        );

        var instanceLayout = new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<RectangleData>(),
            [
                new VertexElementDescription("iPos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("iSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("iColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("iRadius", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
                new VertexElementDescription("iThickness", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
                new VertexElementDescription("iBorderColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("iDepth", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1)
            ]
        )
        {
            InstanceStepRate = 1
        };

        var layoutDescription = new ResourceLayoutDescription(
            [
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ]
        );
        _resourceLayout = factory.CreateResourceLayout(layoutDescription);

        // Pipeline
        var pipelineDescription = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
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
        var resourceSetDesc = new ResourceSetDescription(_resourceLayout, [CommonResources.UniformBuffer]);
        _resourceSet = factory.CreateResourceSet(resourceSetDesc);
    }

    public override void Update(float deltaTime)
    {
        // Rien à faire pour l'instant
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
