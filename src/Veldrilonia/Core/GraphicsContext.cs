using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;

namespace UIFramework.Core;

public class GraphicsContext
{
    public GraphicsDevice Device { get; private set; }
    private readonly Window _window;

    public GraphicsContext(Window window)
    {
        _window = window;
        
        Device = VeldridStartup.CreateGraphicsDevice(window.SdlWindow, new GraphicsDeviceOptions
        {
            SwapchainDepthFormat = PixelFormat.D32_Float_S8_UInt,
            SyncToVerticalBlank = true,
        });
    }

    public void SwapBuffers()
    {
        Device.SwapBuffers();
    }

    public CommandList CreateCommandList()
    {
        return Device.ResourceFactory.CreateCommandList();
    }

    public void SubmitCommands(CommandList commandList)
    {
        Device.SubmitCommands(commandList);
    }

    public Matrix4x4 CreateOrthographicProjection()
    {
        return Matrix4x4.CreateOrthographicOffCenter(
            left: 0, 
            right: _window.Width,
            bottom: _window.Height, 
            top: 0,
            zNearPlane: 0f, 
            zFarPlane: 1f
        );
    }
}
