using Veldrid;

namespace UIFramework.Core;

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

    public IReadOnlySet<Key> PressedKeys => _pressedKeys;
}
