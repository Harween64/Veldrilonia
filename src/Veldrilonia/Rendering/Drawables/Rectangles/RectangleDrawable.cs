using System.Runtime.CompilerServices;
using UIFramework.Data;
using UIFramework.Rendering.Pipeline;
using UIFramework.Rendering.Shaders;
using Veldrid;

namespace UIFramework.Rendering.Drawables.Rectangles;

public class RectangleDrawable(GraphicsDevice graphicsDevice, CommonResources commonResources) : IDrawable<RectangleData>
{
    private Veldrid.Pipeline? _pipeline;
    private ResourceLayout? _resourceLayout;
    private ResourceSet? _resourceSet;
    private DeviceBuffer? _instanceBuffer;
    private RectangleData[] _data = [];

    public void Initialize()
    {
        var factory = graphicsDevice.ResourceFactory;

        // Charger les shaders
        var shaderManager = new ShaderManager(graphicsDevice);
        var shaders = shaderManager.LoadShader(
            "Rendering/Drawables/Rectangles/RectangleVertex.glsl",
            "Rendering/Drawables/Rectangles/RectangleFragment.glsl"
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
                shaders
            ),
            Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription,
            ResourceLayouts = [_resourceLayout]
        };

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        // Resource Set
        var resourceSetDesc = new ResourceSetDescription(_resourceLayout, [commonResources.UniformBuffer]);
        _resourceSet = factory.CreateResourceSet(resourceSetDesc);
    }

    public void UpdateInstances(RectangleData[] data)
    {
        _data = data;

        if (_instanceBuffer != null)
        {
            _instanceBuffer.Dispose();
        }

        var bufferDesc = new BufferDescription(
            (uint)(data.Length * Unsafe.SizeOf<RectangleData>()),
            BufferUsage.VertexBuffer
        );
        _instanceBuffer = graphicsDevice.ResourceFactory.CreateBuffer(bufferDesc);
        graphicsDevice.UpdateBuffer(_instanceBuffer, 0, data);
    }

    public void Update(float deltaTime)
    {
        // Rien à faire pour l'instant
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
