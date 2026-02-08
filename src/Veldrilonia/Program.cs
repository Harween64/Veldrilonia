using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

public static class Program
{
    private static GraphicsDevice _graphicsDevice;
    private static Sdl2Window _window;

    private static Shader[] _shaders;
    private static Pipeline _pipeline;

    public static void Main()
    {
        // Création de la configuration de la fenêtre
        WindowCreateInfo windowCI = new WindowCreateInfo(
            x: 100,
            y: 100,
            windowWidth: 960,
            windowHeight: 540,
            windowInitialState: WindowState.Normal,
            windowTitle: "Mon Moteur 2D Veldrid"
        );

        // Création de la fenêtre et du contexte graphique
        _window = VeldridStartup.CreateWindow(ref windowCI);
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window);

        VertexPositionTexture[] vertices =
        [
            new (new Vector2(-0.5f, -0.5f), new Vector2(0, 1)),
            new (new Vector2(-0.5f, 0.5f), new Vector2(0, 0)),
            new (new Vector2(0.5f, 0.5f), new Vector2(1, 0)),
            new (new Vector2(0.5f, -0.5f), new Vector2(1, 1))
        ];

        // 1. On décrit le buffer (Taille + Usage)
        var bDescription = new BufferDescription(
            (uint)(vertices.Length * Unsafe.SizeOf<VertexPositionTexture>()),
            BufferUsage.VertexBuffer);

        // 2. On demande à l'usine de créer le buffer vide sur le GPU
        var _vertexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(bDescription);

        // 3. On remplit le buffer avec nos données C#
        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);


        ushort[] indices = [
            0, 1, 2,
            0, 2, 3
        ];

        bDescription = new BufferDescription(
            (uint)(indices.Length * sizeof(ushort)),
            BufferUsage.IndexBuffer);
        var _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(bDescription);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, indices);

        Matrix4x4[] transforms = [Matrix4x4.Identity];
        bDescription = new BufferDescription((uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer);
        var _uniformBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(bDescription);
        _graphicsDevice.UpdateBuffer(_uniformBuffer, 0, transforms);

        // Le code source du Vertex Shader (L'architecte)
        string vertexCode = """
                #version 450
                layout(location = 0) in vec2 Position;
                layout(location = 1) in vec2 TextureCoordinate;
                layout(location = 0) out vec2 fsin_TextureCoordinate;
                layout(set = 0, binding = 0) uniform Transformation {
                    mat4 World;
                };

                void main()
                {
                    gl_Position = World * vec4(Position, 0, 1);
                    fsin_TextureCoordinate = TextureCoordinate;
                }
                """;

        // Le code source du Fragment Shader (Le peintre)
        string fragmentCode = """
                #version 450
                layout(location = 0) in vec2 fsin_TextureCoordinate;
                layout(location = 0) out vec4 fsout_Color;

                layout(set = 0, binding = 1) uniform texture2D SurfaceTexture;
                layout(set = 0, binding = 2) uniform sampler SurfaceSampler;

                void main()
                {
                    fsout_Color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_TextureCoordinate);
                }
                """;
        var vertexShaderDesc = new ShaderDescription(
            ShaderStages.Vertex,
            Encoding.UTF8.GetBytes(vertexCode),
            "main"
        );
        var fragmentShaderDesc = new ShaderDescription(
            ShaderStages.Fragment,
            Encoding.UTF8.GetBytes(fragmentCode),
            "main"
        );


        _shaders = _graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            (uint)Unsafe.SizeOf<VertexPositionTexture>(),
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TextureCoordinate", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );

        // 1. On charge les pixels depuis le fichier
        var image = new Veldrid.ImageSharp.ImageSharpTexture("./Assets/create.png");

        // 2. On crée la texture sur la carte graphique
        Texture _texture = image.CreateDeviceTexture(_graphicsDevice, _graphicsDevice.ResourceFactory);

        // 3. On crée une "Vue" pour que le shader puisse lire la texture
        TextureView _textureView = _graphicsDevice.ResourceFactory.CreateTextureView(_texture);


        ResourceLayout _resourceLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(new (
            new ResourceLayoutElementDescription("Transformation", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));
        ResourceSetDescription setDescription = new (_resourceLayout, _uniformBuffer, _textureView, _graphicsDevice.PointSampler);
        ResourceSet _resourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(setDescription);

        GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();

        // 1. Comment on mélange les couleurs (ici, par défaut)
        pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;

        // 2. La gestion de la profondeur (ici, désactivé pour 2D)
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.Disabled;

        // 3. Comment remplir les triangles (ici, remplissage plein, pas de fil de fer)
        pipelineDescription.RasterizerState = RasterizerStateDescription.Default;

        // 4. Notre choix de tout à l'heure !
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;

        // 5. Nos Shaders et notre format de Vertex
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            new[] { vertexLayout }, // Le plan de nos données
            _shaders);              // Nos programmes

        // 6. On lui dit sur quoi on dessine (la fenêtre)
        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;
        pipelineDescription.ResourceLayouts = [_resourceLayout];

        // CRÉATION DU PIPELINE
        _pipeline = _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

 
        while (_window.Exists)
        {
            // Gestion des événements de la fenêtre
            _window.PumpEvents();

            // Vérification si la fenêtre est fermée
            if (!_window.Exists)
                break;


            var rotation = Matrix4x4.CreateRotationZ((float)Environment.TickCount / 1000f);
            _graphicsDevice.UpdateBuffer(_uniformBuffer, 0, ref rotation);

            var commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

            // Commandes de rendu
            commandList.Begin();
            // Configuration du framebuffer et nettoyage de l'écran
            commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Grey);
            commandList.SetPipeline(_pipeline);
            commandList.SetVertexBuffer(0, _vertexBuffer);
            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            commandList.SetGraphicsResourceSet(0, _resourceSet);
            commandList.DrawIndexed(
                indexCount: (uint)indices.Length,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );

            commandList.End();
            _graphicsDevice.SubmitCommands(commandList);

            // Logique de rendu (placeholder)
            _graphicsDevice.SwapBuffers();
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColor(Vector2 position, RgbaFloat color)
{
    public Vector2 Position = position;
    public Vector4 Color = color.ToVector4();
}



[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionTexture(Vector2 position, Vector2 textureCoordinate)
{
    public Vector2 Position = position;
    public Vector2 TextureCoordinate = textureCoordinate;
}

