using System.Collections;
using Veldrid;

namespace Veldridonia.Rendering;

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
