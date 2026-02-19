using System.Numerics;
using UIFramework.Core;
using UIFramework.Data;
using UIFramework.Rendering;
using Veldrid;

namespace UIFramework;

public class Program
{
    public static void Main(string[] args)
    {
        // Initialisation
        var window = new Window(960, 960, "Mon Moteur 2D Veldrid");
        var graphicsContext = new GraphicsContext(window);
        var fontsContext = new FontsContext(graphicsContext.Device);
        var inputManager = new InputManager();
        var renderer = new Renderer(graphicsContext, fontsContext);

        fontsContext.LoadFont();
        renderer.Initialize();

        // Génération de données de test
        //var instances = GenerateRandomRectangles(window.Width, window.Height, 1000);
        var instances = fontsContext.CreateTextInstances("Hello Veldrilonia!", new Vector2(50, 50), 50.0f);
        instances = [..instances, ..fontsContext.CreateTextInstances("This is a test of the Veldrid UI Framework.", new Vector2(50, 150), 25.0f)];
        instances = [..instances, ..fontsContext.CreateTextInstances("MSDF text rendering with Veldrid!", new Vector2(50, 200), 10.0f)];

        // Mise à jour des données du renderer
        renderer.Text.UpdateInstances(instances);

        // Variables de temps
        long previousTicks = Environment.TickCount64;

        float speed = 100f;
        // Boucle principale
        while (window.Exists)
        {
            // Calcul du delta time
            var currentTicks = Environment.TickCount64;
            var deltaTime = (currentTicks - previousTicks) / 1000f;
            previousTicks = currentTicks;

            // Gestion des inputs
            var input = window.PumpEvents();
            inputManager.Update(input);

            // Exemple d'utilisation des inputs
            var direction = inputManager.GetDirection();

            if (direction != Vector2.Zero)
            {
                var distanceInGraphicsCoordinates = speed * deltaTime;
                var distanceInPixels = distanceInGraphicsCoordinates * graphicsContext.PixelsPerUnit;
                for (int i = 0; i < instances.Length; i++)
                {
                    instances[i].Position += Vector2.Normalize(direction) * distanceInGraphicsCoordinates;
                }
                renderer.Text.UpdateInstances(instances);
                //_position += Vector2.Normalize(direction) * distance;
            }

            // Rendu
            renderer.Render();
        }

        // Nettoyage
        renderer.Dispose();
    }

    static RectangleData[] GenerateRandomRectangles(int windowWidth, int windowHeight, int count)
    {
        var random = new Random();
        var rectangles = new RectangleData[count];

        for (int i = 0; i < count; i++)
        {
            rectangles[i] = new RectangleData(
                position: new Vector2(
                    random.Next(0, windowWidth - 100),
                    random.Next(0, windowHeight - 100)
                ),
                size: new Vector2(
                    random.Next(10, 100),
                    random.Next(10, 100)
                ),
                color: new RgbaFloat(
                    r: (float)random.NextDouble(),
                    g: (float)random.NextDouble(),
                    b: (float)random.NextDouble(),
                    a: 1.0f
                ),
                cornerRadius: random.Next(0, 10),
                borderThickness: random.Next(1, 8),
                borderColor: new RgbaFloat(
                    r: (float)random.NextDouble(),
                    g: (float)random.NextDouble(),
                    b: (float)random.NextDouble(),
                    a: 1.0f
                ),
                depth: (float)random.NextDouble()
            );
        }

        return rectangles;
    }
}
