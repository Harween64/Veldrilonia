using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Veldridonia.Core;

public class Window
{
    public Sdl2Window SdlWindow { get; private set; }
    public int Width => SdlWindow.Width;
    public int Height => SdlWindow.Height;
    public bool Exists => SdlWindow.Exists;

    public Window(int width, int height, string title)
    {
        var windowCI = new WindowCreateInfo(
            x: 100,
            y: 100,
            windowWidth: width,
            windowHeight: height,
            windowInitialState: WindowState.Normal,
            windowTitle: title
        );

        SdlWindow = VeldridStartup.CreateWindow(ref windowCI);
    }

    public InputSnapshot PumpEvents()
    {
        return SdlWindow.PumpEvents();
    }
}
