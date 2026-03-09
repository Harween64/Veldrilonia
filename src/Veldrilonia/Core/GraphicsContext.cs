using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;

namespace Veldridonia.Core;

public class GraphicsContext
{
    public GraphicsDevice Device { get; private set; }
    public Matrix4x4 OrthographicProjection { get; private set; }
    private readonly Window _window;

    // Ressources MSAA
    public Texture MsaaColorTexture { get; private set; } = null!;
    public Texture MsaaDepthTexture { get; private set; } = null!;
    public Framebuffer MsaaFramebuffer { get; private set; } = null!;
    public TextureSampleCount SampleCount { get; private set; }
    public PixelFormat ColorFormat { get; private set; }
    public PixelFormat DepthFormat { get; private set; }
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

        (SampleCount, ColorFormat, DepthFormat) = GetSupportedMsaaConfig(TextureSampleCount.Count4);
        CreateMsaaResources();
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

    /// <summary>
    /// Verifie le support hardware et degrade le sample count / depth format si necessaire.
    /// </summary>
    private (TextureSampleCount sampleCount, PixelFormat colorFormat, PixelFormat depthFormat) GetSupportedMsaaConfig(TextureSampleCount desired)
    {
        var swapchainColorFormat = Device.SwapchainFramebuffer.OutputDescription.ColorAttachments[0].Format;
        var allFormats = Enum.GetValues<PixelFormat>();
        var allSampleCounts = Enum.GetValues<TextureSampleCount>();

        // Mettre la meme usage que celle utilisee a la creation de la texture couleur MSAA.
        var colorUsage = TextureUsage.RenderTarget; // Ou Sampled | RenderTarget si necessaire.

        foreach (var sampleCount in allSampleCounts)
        {
            if (sampleCount > desired)
                continue;

            foreach (var colorFormat in allFormats)
            {
                // Contrainte du pipeline actuel: resolve/copy direct vers swapchain.
                if (colorFormat != swapchainColorFormat)
                    continue;

                bool colorOk = Device.GetPixelFormatSupport(
                                colorFormat,
                                TextureType.Texture2D,
                                colorUsage,
                                out var colorProps)
                            && colorProps.IsSampleCountSupported(sampleCount);

                if (!colorOk)
                    continue;

                foreach (var depthFormat in allFormats)
                {
                    bool depthOk = Device.GetPixelFormatSupport(
                                    depthFormat,
                                    TextureType.Texture2D,
                                    TextureUsage.DepthStencil,
                                    out var depthProps)
                                && depthProps.IsSampleCountSupported(sampleCount);

                    if (depthOk)
                        return (sampleCount, colorFormat, depthFormat);
                }
            }
        }

        // Fallback robuste.
        foreach (var depthFormat in allFormats)
        {
            bool depthOk = Device.GetPixelFormatSupport(
                            depthFormat,
                            TextureType.Texture2D,
                            TextureUsage.DepthStencil,
                            out var depthProps)
                        && depthProps.IsSampleCountSupported(TextureSampleCount.Count1);

            if (depthOk)
                return (TextureSampleCount.Count1, swapchainColorFormat, depthFormat);
        }

        return (TextureSampleCount.Count1, swapchainColorFormat, PixelFormat.D32_Float_S8_UInt);
    }

    private void CreateMsaaResources()
    {
        var swapchain = Device.SwapchainFramebuffer;
        //var colorFormat = swapchain.OutputDescription.ColorAttachments[0].Format;

        MsaaColorTexture = Device.ResourceFactory.CreateTexture(new TextureDescription(
            swapchain.Width, swapchain.Height, 1, 1, 1,
            ColorFormat,
            TextureUsage.Sampled | TextureUsage.RenderTarget,
            TextureType.Texture2D,
            SampleCount
        ));

        MsaaDepthTexture = Device.ResourceFactory.CreateTexture(new TextureDescription(
            swapchain.Width, swapchain.Height, 1, 1, 1,
            DepthFormat,
            TextureUsage.DepthStencil,
            TextureType.Texture2D,
            SampleCount
        ));

        MsaaFramebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
            MsaaDepthTexture, MsaaColorTexture
        ));
    }

    public void Dispose()
    {
        MsaaFramebuffer?.Dispose();
        MsaaColorTexture?.Dispose();
        MsaaDepthTexture?.Dispose();
    }
}
