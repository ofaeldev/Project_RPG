using System.Collections;
using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class DialogueTypewriter
    {
        private readonly TMP_Text targetText;

        public DialogueTypewriter(TMP_Text targetText)
        {
            this.targetText = targetText;
        }

        public bool HasTextTarget => targetText != null;

        public void SetInstant(string text)
        {
            if (targetText == null)
            {
                return;
            }

            targetText.text = text;
            targetText.maxVisibleCharacters = int.MaxValue;
        }

        public void PrepareHidden(string text)
        {
            if (targetText == null)
            {
                return;
            }

            targetText.text = text;
            targetText.maxVisibleCharacters = 0;
        }

        public void Complete()
        {
            if (targetText != null)
            {
                targetText.maxVisibleCharacters = int.MaxValue;
            }
        }

        public IEnumerator Reveal(int characterCount, float charactersPerSecond)
        {
            float delay = 1f / Mathf.Max(1f, charactersPerSecond);

            for (int visibleCharacters = 0; visibleCharacters <= characterCount; visibleCharacters++)
            {
                if (targetText == null)
                {
                    break;
                }

                targetText.maxVisibleCharacters = visibleCharacters;
                yield return new WaitForSecondsRealtime(delay);
            }
        }
    }
}
