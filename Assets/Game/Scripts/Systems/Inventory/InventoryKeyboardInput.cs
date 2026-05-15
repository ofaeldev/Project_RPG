using RPGProject.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGProject.Systems
{
    public static class InventoryKeyboardInput
    {
        public static bool WasKeyPressed(Keyboard keyboard, Key key)
        {
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }

        public static bool TryGetSlotIndex(Keyboard keyboard, int itemCount, out int slotIndex)
        {
            slotIndex = -1;
            if (keyboard == null)
            {
                return false;
            }

            int maxSelectable = Mathf.Min(itemCount, 9);
            for (int i = 0; i < maxSelectable; i++)
            {
                if (KeyboardShortcutUtility.WasNumberPressed(keyboard, i))
                {
                    slotIndex = i;
                    return true;
                }
            }

            return false;
        }
    }
}
