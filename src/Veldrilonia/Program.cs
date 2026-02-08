using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

public static class Program
{
    private static GraphicsDevice _graphicsDevice;
    private static Sdl2Window _window;

    public static void Main()
    {
          // 1. Création de la configuration de la fenêtre
          WindowCreateInfo windowCI = new WindowCreateInfo(
               x: 100,
               y: 100,
               windowWidth: 960,
               windowHeight: 540,
               windowInitialState: WindowState.Normal,
               windowTitle: "Mon Moteur 2D Veldrid"
          );

          // 2. Création de la fenêtre et du contexte graphique
          _window = VeldridStartup.CreateWindow(ref windowCI);
          _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window);
    }
}






