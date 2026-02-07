using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Veldrilonia
{
    // Structure représentant nos sommets (vertices)
    struct VertexPositionColor
    {
        public Vector2 Position; // Position X, Y
        public RgbaFloat Color;  // Couleur R, G, B, A

        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }

        // Taille en octets de la structure (2 floats + 4 floats = 6 * 4 bytes = 24 bytes)
        public const uint SizeInBytes = 24;
    }

    class Program
    {
        private static GraphicsDevice _graphicsDevice;
        private static CommandList _commandList;
        private static DeviceBuffer _vertexBuffer;
        private static DeviceBuffer _indexBuffer;
        private static Pipeline _pipeline;
        private static Shader[] _shaders;

        static void Main(string[] args)
        {
            Console.WriteLine("Démarrage de Veldrilonia avec Veldrid...");

            // 1. Initialisation de la Fenêtre et du GraphicsDevice
            // ----------------------------------------------------
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrilonia - Veldrid Hello World (OpenGL)",
            };

            // Crée une fenêtre SDL2
            Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

            // On force le backend OpenGL pour simplifier la gestion des shaders (pas besoin de cross-compilation SPIR-V)
            // Dans un projet réel, on utiliserait Veldrid.SPIRV pour supporter Vulkan/D3D11/Metal.
            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true, // Important pour la compatibilité entre APIs (Y vers le haut ou bas)
                PreferDepthRangeZeroToOne = true,         // Standard Vulkan/D3D12
                Debug = true
            };

            // On essaie de créer le GraphicsDevice avec OpenGL
            try
            {
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options, GraphicsBackend.OpenGL);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création du GraphicsDevice OpenGL: {ex.Message}");
                // Fallback si OpenGL n'est pas dispo (mais peu probable sur Linux/Windows/Mac desktop)
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);
            }

            Console.WriteLine($"GraphicsDevice Backend: {_graphicsDevice.BackendType}");

            // 2. Création des Ressources GPU
            // ------------------------------
            CreateResources();

            // 3. Boucle Principale
            // --------------------
            while (window.Exists)
            {
                // Gestion des événements de la fenêtre (clavier, souris, fermeture)
                InputSnapshot snapshot = window.PumpEvents();
                if (!window.Exists) break;

                // Rendu de l'image
                Draw();
            }

            // 4. Nettoyage
            // ------------
            DisposeResources();
        }

        private static void CreateResources()
        {
            ResourceFactory factory = _graphicsDevice.ResourceFactory;

            // --- A. Vertex Buffer (Les données géométriques) ---
            // On définit 4 sommets pour former un carré (quad).
            VertexPositionColor[] quadVertices =
            {
                // Position (X, Y)              // Couleur (R, G, B, A)
                new VertexPositionColor(new Vector2(-0.75f, 0.75f), RgbaFloat.Red),    // Haut-Gauche
                new VertexPositionColor(new Vector2(0.75f, 0.75f), RgbaFloat.Green),   // Haut-Droite
                new VertexPositionColor(new Vector2(-0.75f, -0.75f), RgbaFloat.Blue),  // Bas-Gauche
                new VertexPositionColor(new Vector2(0.75f, -0.75f), RgbaFloat.Yellow)  // Bas-Droite
            };

            // On crée le buffer sur le GPU
            BufferDescription vbDescription = new BufferDescription(
                4 * VertexPositionColor.SizeInBytes, // Taille totale en octets
                BufferUsage.VertexBuffer);           // Usage: C'est un Vertex Buffer
            _vertexBuffer = factory.CreateBuffer(vbDescription);

            // On envoie les données CPU vers le GPU
            _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, quadVertices);


            // --- B. Index Buffer (L'ordre des sommets) ---
            // Pour dessiner un carré avec des triangles, il faut 2 triangles.
            // On utilise les indices des sommets définis ci-dessus (0, 1, 2, 3).
            ushort[] quadIndices = { 0, 1, 2, 2, 1, 3 }; // Triangle 1 (0,1,2), Triangle 2 (2,1,3)

            // On définit la taille du buffer d'index (6 indices * taille d'un ushort)
            BufferDescription ibDescription = new BufferDescription(
                (uint)(quadIndices.Length * sizeof(ushort)),
                BufferUsage.IndexBuffer);

            _indexBuffer = factory.CreateBuffer(ibDescription);
            _graphicsDevice.UpdateBuffer(_indexBuffer, 0, quadIndices);


            // --- C. Shaders (Le code exécuté par le GPU) ---
            // On utilise des shaders GLSL directement car on cible OpenGL.

            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexShaderSource),
                "main");

            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentShaderSource),
                "main");

            // Création directe des shaders (sans SPIR-V cross-compilation)
            Shader vertexShader = factory.CreateShader(vertexShaderDesc);
            Shader fragmentShader = factory.CreateShader(fragmentShaderDesc);
            _shaders = new[] { vertexShader, fragmentShader };


            // --- D. Pipeline (La configuration du GPU) ---
            // On décrit comment le GPU doit interpréter les données et dessiner.

            // Description du layout des vertices (correspond à notre struct VertexPositionColor et au Shader input)
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2), // location 0
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));   // location 1

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend; // Mélange simple (transparence)

            // RasterizerState: Comment remplir les triangles
            // On désactive le Culling pour être sûr de voir le quad quel que soit l'ordre des vertices
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.None,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList; // On dessine des listes de triangles
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>(); // Pas de textures/uniforms pour l'instant

            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new[] { vertexLayout },
                shaders: _shaders);

            pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription; // Sortie vers l'écran

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            // --- E. CommandList (Pour enregistrer les commandes de dessin) ---
            _commandList = factory.CreateCommandList();
        }

        private static void Draw()
        {
            // 1. Début de l'enregistrement des commandes
            _commandList.Begin();

            // 2. Configuration du Framebuffer (où on dessine)
            _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);

            // 3. Effacement de l'écran (Couleur de fond: Noir)
            _commandList.ClearColorTarget(0, RgbaFloat.Black);

            // 4. Configuration du Pipeline
            _commandList.SetPipeline(_pipeline);

            // 5. Fourniture des Buffers
            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            // 6. Dessin ! (6 indices, 1 instance, offset index 0, offset vertex 0, instance start 0)
            _commandList.DrawIndexed(
                indexCount: 6,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            // 7. Fin de l'enregistrement
            _commandList.End();

            // 8. Soumission des commandes au GPU
            _graphicsDevice.SubmitCommands(_commandList);

            // 9. Échange des buffers (Affichage à l'écran) et attente de la synchro verticale
            _graphicsDevice.SwapBuffers();
        }

        private static void DisposeResources()
        {
            _pipeline.Dispose();
            foreach (var shader in _shaders) shader.Dispose();
            _commandList.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _graphicsDevice.Dispose();
        }

        // --- SHADERS DEFINITIONS (GLSL for OpenGL) ---

        private const string VertexShaderSource = @"
#version 330 core

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0.0, 1.0);
    fsin_Color = Color;
}";

        private const string FragmentShaderSource = @"
#version 330 core

in vec4 fsin_Color;
out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

    }
}
