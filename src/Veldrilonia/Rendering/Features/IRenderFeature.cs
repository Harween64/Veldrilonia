using Veldrid;

namespace Veldridonia.Rendering.Features;

public interface IRenderFeature : IDisposable
{
    void Initialize();
    void Draw(CommandList commandList);
    void Update(float deltaTime);
}

public interface IRenderFeature<TData> : IRenderFeature where TData : struct
{
    void UpdateInstances(TData[] data);
}
