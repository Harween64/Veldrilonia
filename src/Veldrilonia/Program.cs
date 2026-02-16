using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

// Création de la configuration de la fenêtre
WindowCreateInfo windowCI = new WindowCreateInfo(
    x: 100,
    y: 100,
    windowWidth: 960,
    windowHeight: 960,
    windowInitialState: WindowState.Normal,
    windowTitle: "Mon Moteur 2D Veldrid"
);

// Création de la fenêtre et du contexte graphique
Sdl2Window _window = VeldridStartup.CreateWindow(ref windowCI);
GraphicsDevice _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, new GraphicsDeviceOptions
{
    SwapchainDepthFormat = PixelFormat.D32_Float_S8_UInt, // Format de profondeur pour la 2D
    SyncToVerticalBlank = true, // V-Sync pour éviter le tearing
});

ushort[] indices = [
    0, 1, 2,
    0, 2, 3
];

var ibDescription = new BufferDescription(
    (uint)(indices.Length * sizeof(ushort)),
    BufferUsage.IndexBuffer);
var _indexBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(ibDescription);
_graphicsDevice.UpdateBuffer(_indexBuffer, 0, indices);

var ubDescription = new BufferDescription((uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer);
var _uniformBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(ubDescription);
Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(
    left: 0, right: _window.Width,
    bottom: _window.Height, top: 0,
    zNearPlane: 0f, zFarPlane: 1f);
_graphicsDevice.UpdateBuffer(_uniformBuffer, 0, ref projection);

// Le code source du Vertex Shader (L'architecte)
string vertexCode = """
        #version 450
        layout(location = 0) in vec2 vPos; 
        layout(location = 1) in vec2 iPos;
        layout(location = 2) in vec2 iSize;
        layout(location = 3) in vec4 iColor;
        layout(location = 4) in float iRadius;
        layout(location = 5) in float iThickness;
        layout(location = 6) in vec4 iBorderColor;
        layout(location = 7) in float iDepth;

        layout(set = 0, binding = 0) uniform ProjectionBuffer {
            mat4 Projection;
        };

        layout(location = 0) out vec4 fColor;
        layout(location = 1) out vec2 fUV;
        layout(location = 2) out vec2 fSize;
        layout(location = 3) out float fRadius;
        layout(location = 4) out float fThickness;
        layout(location = 5) out vec4 fBorderColor;

        void main() {
            vec2 worldPos = iPos + (vPos * iSize);
            gl_Position = Projection * vec4(worldPos, 0.0, 1.0);
            gl_Position.z = iDepth;
            
            fColor = iColor;
            fUV = vPos;
            fSize = iSize;
            fRadius = iRadius;
            fThickness = iThickness;
            fBorderColor = iBorderColor;
        }
        """;

// Le code source du Fragment Shader (Le peintre)
string fragmentCode = """
#version 450

// Entrées
layout(location = 0) in vec4 fColor;
layout(location = 1) in vec2 fUV;
layout(location = 2) in vec2 fSize;
layout(location = 3) in float fRadius;
layout(location = 4) in float fThickness;
layout(location = 5) in vec4 fBorderColor;

// Sortie
layout(location = 0) out vec4 outColor;

void main() 
{
    // 1. Coordonnées centrées en pixels (0,0 est au centre du bouton)
    vec2 pixelPos = fUV * fSize;
    vec2 p = pixelPos - (fSize / 2.0);

    // 2. Calcul SDF (Signed Distance Field) pour une Boîte Arrondie
    // C'est la formule magique : négatif = dedans, positif = dehors
    vec2 q = abs(p) - (fSize / 2.0 - vec2(fRadius));
    float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - fRadius;

    // 3. Masque d'opacité (Anti-aliasing)
    // On veut que tout ce qui est "dedans" (dist < 0) soit opaque
    // smoothstep(-0.5, 0.5, dist) crée une transition douce de 1px autour du bord
    float alphaMask = 1.0 - smoothstep(-0.5, 0.5, dist);
    if (alphaMask <= 0.0) 
    {
        discard;
    }

    if (fThickness > 0.0) 
    {
        // Distance to the inner edge (inset by border thickness)
        float innerDist = dist + fThickness;

        // fillMask: 1 inside fill area, 0 in border or outside (with AA)
        float fillMask = 1.0 - smoothstep(-0.5, 0.5, innerDist);

        // Pick fill color inside, border color in the border strip
        vec4 blendedColor = mix(fBorderColor, fColor, fillMask);

        // Clip everything to the outer shape boundary
        outColor = vec4(blendedColor.rgb, blendedColor.a * alphaMask);
    } 
    else 
    {
        // 4. Couleur finale
        outColor = vec4(fColor.rgb, fColor.a * alphaMask);
    }
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


Shader[] _shaders = _graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

Vector2[] quadVertices = [
    new Vector2(0, 0), 
    new Vector2(1, 0),
    new Vector2(1, 1),
    new Vector2(0, 1)
];

var modelLayout = new VertexLayoutDescription(
    (uint)Unsafe.SizeOf<InstanceModelData>(),
    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
);

// Pseudo-code de configuration
var instanceLayout = new VertexLayoutDescription(
    (uint)Unsafe.SizeOf<UIInstanceData>(),
    new VertexElementDescription("InstancePos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
    new VertexElementDescription("InstanceSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
    new VertexElementDescription("InstanceColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
    new VertexElementDescription("InstanceCornerRadius", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
    new VertexElementDescription("InstanceBorderThickness", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
    new VertexElementDescription("InstanceBorderColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
    new VertexElementDescription("InstanceDepth", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1)
)
{
    InstanceStepRate = 1
};

ResourceLayoutDescription layoutDescription = new( new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex) );
ResourceLayout _resourceLayout = _graphicsDevice.ResourceFactory.CreateResourceLayout(layoutDescription);

ResourceSetDescription resourceSetDescription = new(_resourceLayout, _uniformBuffer); 
ResourceSet _resourceSet = _graphicsDevice.ResourceFactory.CreateResourceSet(resourceSetDescription);

GraphicsPipelineDescription pipelineDescription = new()
{
    // 1. Comment on mélange les couleurs (ici, par défaut)
    BlendState = BlendStateDescription.SingleAlphaBlend,

    // 2. Gestion de la profondeur pour la Transparence 2D (Modifié)
    // On garde le DepthTest pour clipper si besoin (facultatif), 
    // MAIS on désactive DepthWrite pour que les pixels transparents ne "bloquent" pas le fond.
    DepthStencilState = new DepthStencilStateDescription(
        depthTestEnabled: true, 
        depthWriteEnabled: false, // <-- CLÉ DU SUCCÈS POUR L'ANTI-ALIASING
        comparisonKind: ComparisonKind.LessEqual
    ),

    // 3. Comment remplir les triangles (ici, remplissage plein, pas de fil de fer)
    RasterizerState = RasterizerStateDescription.CullNone,

    // 4. Notre choix de tout à l'heure !
    PrimitiveTopology = PrimitiveTopology.TriangleList,

    // 5. Nos Shaders et notre format de Vertex
    ShaderSet = new ShaderSetDescription(
        [modelLayout, instanceLayout], // Le plan de nos données
        _shaders),              // Nos programmes

    // 6. On lui dit sur quoi on dessine (la fenêtre)
    Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription,
    ResourceLayouts = [_resourceLayout]
};

// CRÉATION DU PIPELINE
Pipeline _pipeline = _graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

HashSet<Key> _pressedKeys = [];
long _previousTicks = Environment.TickCount64;
float speed = 1.0f;


var random = new Random(); 
UIInstanceData[] uIInstanceData = [];
while (uIInstanceData.Length < 1000) 
{
    uIInstanceData =
    [
        .. uIInstanceData, 
        new UIInstanceData(
            position: new Vector2(random.Next(0, _window.Width-100), random.Next(0, _window.Height-100)), 
            size: new Vector2(random.Next(10, 100), random.Next(10, 100)), 
            color: new RgbaFloat(
                r: (float)random.NextDouble(),
                g: (float)random.NextDouble(),
                b: (float)random.NextDouble(),
                a: 1.0f),
            cornerRadius: random.Next(0, 10),
            borderThickness: random.Next(1, 8),
            borderColor: new RgbaFloat(
                r: (float)random.NextDouble(),
                g: (float)random.NextDouble(),
                b: (float)random.NextDouble(),
                a: 1.0f),
            depth: (float)random.NextDouble()// Z-Index aléatoire pour la profondeur
            )
,
    ]; 
}      

var instanceModelBufferDescription = new BufferDescription((uint)(quadVertices.Length * Unsafe.SizeOf<InstanceModelData>()), BufferUsage.VertexBuffer);
var _instanceModelBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(instanceModelBufferDescription);
_graphicsDevice.UpdateBuffer(_instanceModelBuffer, 0, quadVertices);

var instanceBufferDescription = new BufferDescription((uint)(uIInstanceData.Length * Unsafe.SizeOf<UIInstanceData>()), BufferUsage.VertexBuffer);
var _instanceBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(instanceBufferDescription);
_graphicsDevice.UpdateBuffer(_instanceBuffer, 0, uIInstanceData);

while (_window.Exists)
{
    // Vérification si la fenêtre est fermée
    if (!_window.Exists)
        break;

    var currentTicks = Environment.TickCount64;
    var deltaTime = (currentTicks - _previousTicks) / 1000f;
    _previousTicks = currentTicks;

    // Gestion des événements de la fenêtre
    var input = _window.PumpEvents();
    foreach (var keyEvent in input.KeyEvents)
    {
        if (keyEvent.Down)
        {
            _pressedKeys.Add(keyEvent.Key);
        }
        else
        {
            _pressedKeys.Remove(keyEvent.Key);
        }
    }

    var direction = Vector2.Zero;
    foreach (var key in _pressedKeys)
    {
        direction.X += key switch
        {
            Key.Left => -1,
            Key.Right => 1,
            _ => 0
        };

        direction.Y += key switch
        {
            Key.Up => 1,
            Key.Down => -1,
            _ => 0
        };
    }
    
    var commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

    // Commandes de rendu
    commandList.Begin();
    // Configuration du framebuffer et nettoyage de l'écran
    commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
    commandList.ClearColorTarget(0, RgbaFloat.White);
    commandList.ClearDepthStencil(1.0f);

    commandList.SetPipeline(_pipeline);
    commandList.SetGraphicsResourceSet(0, _resourceSet);
    commandList.SetVertexBuffer(0, _instanceModelBuffer);
    commandList.SetVertexBuffer(1, _instanceBuffer);
    commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

    commandList.DrawIndexed(
        indexCount: (uint)indices.Length,
        instanceCount: (uint)uIInstanceData.Length,
        indexStart: 0,
        vertexOffset: 0,
        instanceStart: 0
    );

    commandList.End();
    _graphicsDevice.SubmitCommands(commandList);
// Logique de rendu (placeholder)
    _graphicsDevice.SwapBuffers();
    commandList.Dispose();
}


[StructLayout(LayoutKind.Sequential)]
public struct InstanceModelData(Vector2 position)
{
    public Vector2 Position = position;
}

[StructLayout(LayoutKind.Sequential)]
public struct UIInstanceData(Vector2 position, Vector2 size, RgbaFloat color, float cornerRadius = 0, float borderThickness = 1, RgbaFloat? borderColor = null, float depth = 0)
{
    public Vector2 Position = position;
    public Vector2 Size = size;
    public Vector4 Color = color.ToVector4();
    public float CornerRadius = cornerRadius;
    public float BorderThickness = borderThickness;
    public Vector4 BorderColor = (borderColor ?? color).ToVector4();
    public float Depth = depth; 
}