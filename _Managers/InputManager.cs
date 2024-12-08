using Microsoft.Xna.Framework.Input;

namespace BitQuest;

public static class InputManager
{
    private static KeyboardState _currentKB;
    private static KeyboardState _lastKB;

    // Update the current and previous keyboard states
    public static void Update()
    {
        _lastKB = _currentKB;
        _currentKB = Keyboard.GetState();
    }

    // Check if a key is currently being held down
    public static bool IsKeyDown(Keys key)
    {
        return _currentKB.IsKeyDown(key);
    }

    // Check if a key was just pressed in the current frame
    public static bool IsKeyPressed(Keys key)
    {
        return _currentKB.IsKeyDown(key) && !_lastKB.IsKeyDown(key);
    }

    // Check if a key was just released in the current frame
    public static bool IsKeyReleased(Keys key)
    {
        return !_currentKB.IsKeyDown(key) && _lastKB.IsKeyDown(key);
    }
}
