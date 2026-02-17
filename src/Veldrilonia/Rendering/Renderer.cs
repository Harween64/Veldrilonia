using UIFramework.Core;
using UIFramework.Data;
using UIFramework.Rendering.Pipeline;
using UIFramework.Rendering.Shaders;
using Veldrid;

namespace UIFramework.Rendering;

public class Renderer
{
    private readonly GraphicsContext _graphicsContext;
    private readonly ShaderManager _shaderManager;
    private readonly PipelineBuilder _pipelineBuilder;
    private readonly RenderResources _renderResources;
    
    private Veldrid.Pipeline _pipeline;
    private ResourceLayout _resourceLayout;
    
    public Renderer(GraphicsContext graphicsContext)
    {
        _graphicsContext = graphicsContext;
        _shaderManager = new ShaderManager(graphicsContext.Device);
        _pipelineBuilder = new PipelineBuilder(graphicsContext.Device);
        _renderResources = new RenderResources(graphicsContext.Device);
    }

    public void Initialize(UIInstanceData[] instances)
    {
        // Charger les shaders
        var shaders = _shaderManager.LoadUIShaders();
        
        // Créer les layouts
        var modelLayout = _pipelineBuilder.CreateModelLayout();
        var instanceLayout = _pipelineBuilder.CreateInstanceLayout();
        _resourceLayout = _pipelineBuilder.CreateResourceLayout();
        
        // Créer les buffers
        var projection = _graphicsContext.OrthographicProjection;
        _renderResources.CreateBuffers(projection, instances);
        _renderResources.CreateResourceSet(_resourceLayout);
        
        // Créer le pipeline
        _pipeline = _pipelineBuilder.CreateGraphicsPipeline(
            shaders, 
            modelLayout, 
            instanceLayout, 
            _resourceLayout
        );
    }

    public void Render(UIInstanceData[] instances)
    {
        var commandList = _graphicsContext.CreateCommandList();
        
        commandList.Begin();
        commandList.SetFramebuffer(_graphicsContext.Device.SwapchainFramebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.White);
        commandList.ClearDepthStencil(1.0f);
        
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _renderResources.ResourceSet);
        commandList.SetVertexBuffer(0, _renderResources.ModelBuffer);
        commandList.SetVertexBuffer(1, _renderResources.InstanceBuffer);
        commandList.SetIndexBuffer(_renderResources.IndexBuffer, IndexFormat.UInt16);
        
        commandList.DrawIndexed(
            indexCount: (uint)_renderResources.Indices.Length,
            instanceCount: (uint)instances.Length,
            indexStart: 0,
            vertexOffset: 0,
            instanceStart: 0
        );
        
        commandList.End();
        _graphicsContext.SubmitCommands(commandList);
        _graphicsContext.SwapBuffers();
        
        commandList.Dispose();
    }

    public void UpdateInstances(UIInstanceData[] instances)
    {
        _renderResources.UpdateInstanceBuffer(instances);
    }

    public void Dispose()
    {
        _renderResources.Dispose();
        _pipeline?.Dispose();
        _resourceLayout?.Dispose();
    }
}
