using System;
using Veldrid;

namespace UIFramework.Rendering.Drawables;

public interface IDrawable : IDisposable
{
    void Initialize();
    void Draw(CommandList commandList);
    void Update(float deltaTime);
}

public interface IDrawable<TData> : IDrawable where TData : struct
{
    void UpdateInstances(TData[] data);
}
