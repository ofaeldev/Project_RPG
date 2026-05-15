using RPGProject.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGProject.Systems
{
    public static class DialogueKeyboardInput
    {
        public static bool WasAdvancePressed(Keyboard keyboard)
        {
            return keyboard != null &&
                (keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame ||
                keyboard.spaceKey.wasPressedThisFrame);
        }

        public static bool TryGetChoiceIndex(Keyboard keyboard, int choiceCount, out int choiceIndex)
        {
            choiceIndex = -1;
            if (keyboard == null)
            {
                return false;
            }

            int maxKeyboardChoices = Mathf.Min(choiceCount, 9);
            for (int i = 0; i < maxKeyboardChoices; i++)
            {
                if (KeyboardShortcutUtility.WasNumberPressed(keyboard, i))
                {
                    choiceIndex = i;
                    return true;
                }
            }

            return false;
        }
    }
}
