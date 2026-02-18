using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace UIFramework.Rendering.Shaders;

public class ShaderManager(GraphicsDevice _graphicsDevice)
{
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

    public Shader[] LoadGlyphShader()
    {
        // Charger et compiler le shader de glyphes
        string vertexCode = File.ReadAllText("Rendering/Shaders/GlyphVertex.glsl");
        string fragmentCode = File.ReadAllText("Rendering/Shaders/GlyphFragment.glsl");

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

        return _graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
    }
}
