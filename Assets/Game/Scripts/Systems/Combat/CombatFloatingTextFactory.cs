using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class CombatFloatingTextFactory
    {
        private readonly Transform parent;
        private readonly ComponentPool<FloatingCombatText> pool;
        private readonly float fontSize;

        public CombatFloatingTextFactory(Transform parent, float fontSize)
        {
            this.parent = parent;
            this.fontSize = fontSize;
            pool = new ComponentPool<FloatingCombatText>(Create, parent);
        }

        public FloatingCombatText Spawn(string value, Color color, Vector3 position, Vector3 moveOffset, float lifetime)
        {
            FloatingCombatText floatingText = pool.Get();
            if (floatingText == null)
            {
                return null;
            }

            floatingText.Completed -= Release;
            floatingText.Completed += Release;
            floatingText.Initialize(value, color, position, moveOffset, lifetime);
            return floatingText;
        }

        public void Clear()
        {
            pool.Clear();
        }

        private FloatingCombatText Create()
        {
            GameObject textObject = new("FloatingCombatText", typeof(TextMeshPro), typeof(FloatingCombatText));
            textObject.transform.SetParent(parent, true);

            TextMeshPro text = textObject.GetComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.sortingOrder = 80;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            FloatingCombatText floatingText = textObject.GetComponent<FloatingCombatText>();
            textObject.SetActive(false);
            return floatingText;
        }

        private void Release(FloatingCombatText floatingText)
        {
            if (floatingText != null)
            {
                floatingText.Completed -= Release;
                pool.Release(floatingText);
            }
        }
    }
}
