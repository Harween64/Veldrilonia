using System.Runtime.CompilerServices;
using Veldridonia.Core.Svg;
using Veldridonia.Rendering.Pipeline;
using Veldrid;

namespace Veldridonia.Rendering.Features;

/// <summary>
/// Feature de rendu pour les fichiers SVG tesselles.
/// Utilise un pipeline non-instance avec des triangles issus de la tessellation.
/// MSAA x4 pour l'anticrenelage.
/// </summary>
public sealed class SvgRenderFeature : IRenderFeature
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CommonResources _commonResources;

    private Veldrid.Pipeline? _pipeline;
    private ResourceLayout? _resourceLayout;
    private ResourceSet? _resourceSet;
    private ShaderSet? _shaderSet;

    // Buffers SVG (vertex + index propres, pas de quad partage)
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _indexBuffer;
    private uint _vertexBufferCapacity;
    private uint _indexBufferCapacity;

    private SvgVertex[] _vertices = [];
    private uint[] _indices = [];

    public SvgRenderFeature(GraphicsDevice graphicsDevice, CommonResources commonResources)
    {
        _graphicsDevice = graphicsDevice;
        _commonResources = commonResources;
    }

    public void Initialize()
    {
        var factory = _graphicsDevice.ResourceFactory;

        // Charger les shaders
        var shaderManager = new ShaderManager(_graphicsDevice);
        _shaderSet = shaderManager.LoadShader(
            "Rendering/Features/Svg/SvgVertex.glsl",
            "Rendering/Features/Svg/SvgFragment.glsl"
        );

        // Layout des vertices SVG (position + couleur par vertex)
        var vertexLayout = new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<SvgVertex>(),
            [
                new VertexElementDescription("vPos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("vColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            ]
        );

        // Resource layout (projection uniquement)
        var layoutDescription = new ResourceLayoutDescription(
            [
                new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ]
        );
        _resourceLayout = factory.CreateResourceLayout(layoutDescription);

        // Pipeline avec MSAA x4
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
                [vertexLayout],
                [.. _shaderSet]
            ),
            Outputs = _commonResources.OutputDescription,
            ResourceLayouts = [_resourceLayout]
        };

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        // Resource Set
        var resourceSetDesc = new ResourceSetDescription(_resourceLayout, [_commonResources.UniformBuffer]);
        _resourceSet = factory.CreateResourceSet(resourceSetDesc);
    }

    /// <summary>
    /// Met a jour le maillage SVG a rendre.
    /// </summary>
    public void UpdateMesh(SvgMeshData meshData)
    {
        _vertices = meshData.Vertices;
        _indices = meshData.Indices;

        if (_vertices.Length == 0 || _indices.Length == 0)
            return;

        // Vertex buffer
        uint requiredVertexSize = (uint)(_vertices.Length * Unsafe.SizeOf<SvgVertex>());
        if (_vertexBuffer == null || _vertexBufferCapacity < requiredVertexSize)
        {
            _vertexBuffer?.Dispose();
            _vertexBufferCapacity = requiredVertexSize;
            _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(_vertexBufferCapacity, BufferUsage.VertexBuffer | BufferUsage.Dynamic)
            );
        }
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices);

        // Index buffer
        uint requiredIndexSize = (uint)(_indices.Length * sizeof(uint));
        if (_indexBuffer == null || _indexBufferCapacity < requiredIndexSize)
        {
            _indexBuffer?.Dispose();
            _indexBufferCapacity = requiredIndexSize;
            _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(_indexBufferCapacity, BufferUsage.IndexBuffer | BufferUsage.Dynamic)
            );
        }
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices);
    }

    public void Update(float deltaTime)
    {
    }

    public void Draw(CommandList commandList)
    {
        if (_vertices.Length == 0 || _indices.Length == 0)
            return;

        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _resourceSet);
        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);

        commandList.DrawIndexed(
            indexCount: (uint)_indices.Length,
            instanceCount: 1,
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
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _shaderSet?.Dispose();
    }
}
