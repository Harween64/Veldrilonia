using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace UIFramework.Rendering.Shaders;

public class ShaderManager
{
    private readonly GraphicsDevice _graphicsDevice;

    public ShaderManager(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Shader[] LoadUIShaders()
    {
        string vertexCode = File.ReadAllText("Rendering/Shaders/UIVertex.glsl");
        string fragmentCode = File.ReadAllText("Rendering/Shaders/UIFragment.glsl");

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

        // Compile HLSL to SPIR-V for cross-platform compatibility
        return _graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
    }
}
