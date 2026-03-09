using Veldridonia.Core;
using Veldridonia.Core.Fonts;
using Veldridonia.Rendering.Features;
using Veldridonia.Rendering.Pipeline;
using Veldrid;

namespace Veldridonia.Rendering;

public class Renderer
{
    private readonly GraphicsContext _graphicsContext;
    private readonly FontsContext _fontsContext;
    private readonly CommonResources _commonResources;

    // Drawables
    public RectangleRenderFeature Rectangles { get; private set; }
    public Dictionary<string, TextRenderFeature> Texts { get; private set; } = [];
    public SvgRenderFeature Svg { get; private set; }

    private readonly List<IRenderFeature> _drawables = [];

    public Renderer(GraphicsContext graphicsContext, FontsContext fontsContext)
    {
        _graphicsContext = graphicsContext;
        _fontsContext = fontsContext;
        _commonResources = new CommonResources(graphicsContext.Device, graphicsContext.MsaaFramebuffer.OutputDescription);

        Rectangles = new RectangleRenderFeature(graphicsContext.Device, _commonResources);
        _drawables.Add(Rectangles);

        Svg = new SvgRenderFeature(graphicsContext.Device, _commonResources);
        _drawables.Add(Svg);

        foreach (var fontName in _fontsContext.LoadedFonts)
        {
            var textDrawable = new TextRenderFeature(graphicsContext.Device, _commonResources, _fontsContext.GetFontTexture(fontName));
            Texts[fontName] = textDrawable;
            _drawables.Add(textDrawable);
        }
    }

    public void Initialize()
    {
        // Créer les buffers communs (projection, quad)
        var projection = _graphicsContext.OrthographicProjection;
        _commonResources.CreateBuffers(projection);

        // Initialiser tous les drawables
        foreach (var drawable in _drawables)
        {
            drawable.Initialize();
        }
    }

    public void Render()
    {
        var commandList = _graphicsContext.CreateCommandList();

        commandList.Begin();

        // Rendre dans le framebuffer MSAA
        commandList.SetFramebuffer(_graphicsContext.MsaaFramebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.White);
        commandList.ClearDepthStencil(1.0f);

        foreach (var drawable in _drawables)
        {
            drawable.Draw(commandList);
        }

        // Resoudre le MSAA vers le backbuffer de la swapchain
        var swapchainTarget = _graphicsContext.Device.SwapchainFramebuffer.ColorTargets[0].Target;
        if (_graphicsContext.SampleCount != TextureSampleCount.Count1)
        {
            commandList.ResolveTexture(_graphicsContext.MsaaColorTexture, swapchainTarget);
        }
        else
        {
            commandList.CopyTexture(_graphicsContext.MsaaColorTexture, swapchainTarget);
        }

        commandList.End();
        _graphicsContext.SubmitCommands(commandList);
        _graphicsContext.SwapBuffers();

        commandList.Dispose();
    }

    public void Dispose()
    {
        _commonResources.Dispose();
        foreach (var drawable in _drawables)
        {
            drawable.Dispose();
        }
    }
}
