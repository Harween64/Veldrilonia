using System.Collections;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace UIFramework.Rendering.Shaders;

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

/// <summary>
/// Immutable read-only collection of shaders that manages their disposal.
/// </summary>
public sealed class ShaderSet(Shader[] shaders) : IReadOnlyList<Shader>, IDisposable
{
    private readonly Shader[] _shaders = shaders ?? throw new ArgumentNullException(nameof(shaders));

    public int Count => _shaders.Length;

    public Shader this[int index] => _shaders[index];

    public IEnumerator<Shader> GetEnumerator() => ((IEnumerable<Shader>)_shaders).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _shaders.GetEnumerator();

    public void Dispose()
    {
        foreach (var shader in _shaders)
        {
            shader.Dispose();
        }
    }
}
