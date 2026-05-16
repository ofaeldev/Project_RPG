using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class HitFlashPresenter : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer targetRenderer;

        [SerializeField]
        private Color flashColor = new(1f, 1f, 1f, 1f);

        [SerializeField]
        [Min(0.01f)]
        private float flashSeconds = 0.08f;

        private HealthComponent health;
        private Color originalColor;
        private bool isFlashing;
        private float flashUntilTime;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            ResolveRenderer();
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

            RestoreColor();
        }

        private void Update()
        {
            if (flashUntilTime > 0f && Time.time >= flashUntilTime)
            {
                flashUntilTime = 0f;
                RestoreColor();
            }
        }

        private void OnHealthChanged(HealthChange change)
        {
            if (change.ChangeType != HealthChangeType.Damage || change.Amount <= 0)
            {
                return;
            }

            if (targetRenderer == null)
            {
                return;
            }

            originalColor = targetRenderer.color;
            isFlashing = true;
            targetRenderer.color = flashColor;
            flashUntilTime = Time.time + flashSeconds;
        }

        private void ResolveRenderer()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void RestoreColor()
        {
            if (targetRenderer != null && isFlashing)
            {
                targetRenderer.color = originalColor;
            }

            isFlashing = false;
        }
    }
}
