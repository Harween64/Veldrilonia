using System.Numerics;
using Veldrid;

namespace Veldridonia.Core;

public class InputManager
{
    private readonly HashSet<Key> _pressedKeys = [];

    public void Update(InputSnapshot input)
    {
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
    }

    public bool IsKeyPressed(Key key)
    {
        return _pressedKeys.Contains(key);
    }

    public Vector2 GetDirection()
    {
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

        return direction;
    }

    public IReadOnlySet<Key> PressedKeys => _pressedKeys;
}
