using UIFramework.Core;
using UIFramework.Data;
using UIFramework.Rendering.Drawables;
using UIFramework.Rendering.Drawables.Rectangles;
using UIFramework.Rendering.Drawables.Text;
using UIFramework.Rendering.Pipeline;
using Veldrid;

namespace UIFramework.Rendering;

public class Renderer
{
    private readonly GraphicsContext _graphicsContext;
    private readonly FontsContext _fontsContext;
    private readonly CommonResources _commonResources;

    // Drawables
    public RectangleDrawable Rectangles { get; private set; }
    public TextDrawable Text { get; private set; }

    private readonly List<IDrawable> _drawables = [];

    public Renderer(GraphicsContext graphicsContext, FontsContext fontsContext)
    {
        _graphicsContext = graphicsContext;
        _fontsContext = fontsContext;
        _commonResources = new CommonResources(graphicsContext.Device);

        Rectangles = new RectangleDrawable(graphicsContext.Device, _commonResources);
        Text = new TextDrawable(graphicsContext.Device, _commonResources, fontsContext.GetFontTexture());

        _drawables.Add(Rectangles);
        _drawables.Add(Text);
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
        commandList.SetFramebuffer(_graphicsContext.Device.SwapchainFramebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.White);
        commandList.ClearDepthStencil(1.0f);

        foreach (var drawable in _drawables)
        {
            drawable.Draw(commandList);
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
