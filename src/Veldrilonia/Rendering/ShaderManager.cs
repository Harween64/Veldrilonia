using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace Veldridonia.Rendering;

public class ShaderManager(GraphicsDevice _graphicsDevice)
{
    public ShaderSet LoadShader(string vertexPath, string fragmentPath)
    {
        string vertexCode = File.ReadAllText(vertexPath);
        string fragmentCode = File.ReadAllText(fragmentPath);

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

        var shaders = _graphicsDevice.ResourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        return new ShaderSet(shaders);
    }
}
