using System.Runtime.CompilerServices;
using UIFramework.Rendering.Pipeline;
using UIFramework.Rendering.Shaders;
using Veldrid;

namespace UIFramework.Rendering.Drawables;

/// <summary>
/// Abstract base class for drawable objects with generic instance data management.
/// Handles shader loading, pipeline creation, and efficient buffer management.
/// </summary>
public abstract class Drawable<T>(GraphicsDevice graphicsDevice, CommonResources commonResources) : IDrawable<T> where T : unmanaged
{
    protected GraphicsDevice GraphicsDevice { get; } = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
    protected CommonResources CommonResources { get; } = commonResources ?? throw new ArgumentNullException(nameof(commonResources));

    protected Veldrid.Pipeline? _pipeline;
    protected ResourceLayout? _resourceLayout;
    protected ResourceSet? _resourceSet;
    protected DeviceBuffer? _instanceBuffer;
    protected ShaderSet? _shaderSet;
    protected T[] _data = [];
    protected uint _instanceBufferCapacity;

    public abstract void Initialize();

    public virtual void Update(float deltaTime)
    {
    }

    public abstract void Draw(CommandList commandList);

    public void UpdateInstances(T[] data)
    {
        _data = data;

        if (data.Length == 0)
        {
            return;
        }

        uint requiredSize = (uint)(data.Length * Unsafe.SizeOf<T>());

        // Only reallocate if buffer doesn't exist or is too small
        if (_instanceBuffer == null || _instanceBufferCapacity < requiredSize)
        {
            _instanceBuffer?.Dispose();
            _instanceBufferCapacity = requiredSize;
            var bufferDesc = new BufferDescription(
                _instanceBufferCapacity,
                BufferUsage.VertexBuffer | BufferUsage.Dynamic
            );
            _instanceBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(bufferDesc);
        }

        GraphicsDevice.UpdateBuffer(_instanceBuffer, 0, data);
    }

    public virtual void Dispose()
    {
        _pipeline?.Dispose();
        _resourceLayout?.Dispose();
        _resourceSet?.Dispose();
        _instanceBuffer?.Dispose();
        _shaderSet?.Dispose();
    }
}
