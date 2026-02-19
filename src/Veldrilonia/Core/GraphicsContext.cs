using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;

namespace UIFramework.Core;

public class GraphicsContext
{
    public GraphicsDevice Device { get; private set; }
    public Matrix4x4 OrthographicProjection { get; private set; }
    private readonly Window _window;

    public GraphicsContext(Window window)
    {
        _window = window;

        Device = VeldridStartup.CreateGraphicsDevice(window.SdlWindow, new GraphicsDeviceOptions
        {
            SwapchainDepthFormat = PixelFormat.D32_Float_S8_UInt,
            SyncToVerticalBlank = true,
        });

        OrthographicProjection = CreateOrthographicProjection();
        Matrix4x4.Decompose(OrthographicProjection, out var scale, out _, out _);
        PixelsPerUnit = scale.AsVector2(); // Assuming uniform scaling
    }

    public Vector2 PixelsPerUnit { get; private set; }

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

    private Matrix4x4 CreateOrthographicProjection()
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
