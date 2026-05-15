using UnityEngine.InputSystem;

namespace RPGProject.Systems
{
    public static class LootKeyboardInput
    {
        public static bool WasTakeAllPressed(Keyboard keyboard, Key takeAllKey)
        {
            return keyboard != null && keyboard[takeAllKey].wasPressedThisFrame;
        }

        public static bool WasClosePressed(Keyboard keyboard, Key closeKey)
        {
            return keyboard != null && keyboard[closeKey].wasPressedThisFrame;
        }
    }
}
