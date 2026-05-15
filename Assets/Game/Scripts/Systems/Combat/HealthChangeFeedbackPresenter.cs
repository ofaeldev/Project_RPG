using RPGProject.Gameplay;
using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class HealthChangeFeedbackPresenter : MonoBehaviour
    {
        [Header("Floating Text")]
        [SerializeField]
        private Vector3 spawnOffset = new(0f, 0.65f, 0f);

        [SerializeField]
        private Vector3 floatOffset = new(0f, 0.55f, 0f);

        [SerializeField]
        [Min(0.05f)]
        private float lifetime = 0.8f;

        [SerializeField]
        [Min(0.1f)]
        private float fontSize = 3f;

        [Header("Colors")]
        [SerializeField]
        private Color damageColor = new(1f, 0.22f, 0.16f, 1f);

        [SerializeField]
        private Color healColor = new(0.3f, 1f, 0.36f, 1f);

        private HealthComponent health;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
            }
        }

        private void OnHealthChanged(HealthChange change)
        {
            if (change.Amount == 0)
            {
                return;
            }

            switch (change.ChangeType)
            {
                case HealthChangeType.Damage:
                    SpawnFloatingText(change.Amount.ToString(), damageColor);
                    break;
                case HealthChangeType.Heal:
                case HealthChangeType.Revive:
                    SpawnFloatingText($"+{change.Amount}", healColor);
                    break;
            }
        }

        private void SpawnFloatingText(string value, Color color)
        {
            GameObject textObject = new("FloatingCombatText", typeof(TextMeshPro), typeof(FloatingCombatText));
            textObject.transform.position = transform.position + spawnOffset;

            TextMeshPro text = textObject.GetComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.sortingOrder = 80;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            FloatingCombatText floatingText = textObject.GetComponent<FloatingCombatText>();
            floatingText.Initialize(value, color, textObject.transform.position, floatOffset, lifetime);
        }
    }
}
