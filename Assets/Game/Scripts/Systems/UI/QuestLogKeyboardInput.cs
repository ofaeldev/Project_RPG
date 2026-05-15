using UnityEngine.InputSystem;

namespace RPGProject.Systems
{
    public static class QuestLogKeyboardInput
    {
        public static bool WasTogglePressed(Keyboard keyboard, Key toggleKey)
        {
            return keyboard != null && keyboard[toggleKey].wasPressedThisFrame;
        }
    }
}
