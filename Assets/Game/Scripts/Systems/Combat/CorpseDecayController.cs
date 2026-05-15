using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class CorpseDecayController : MonoBehaviour
    {
        [Header("Decay")]
        [SerializeField]
        private SpriteRenderer targetRenderer;

        [SerializeField]
        [Min(0f)]
        private float despawnDelay = 18f;

        [SerializeField]
        private Color freshCorpseColor = new(0.46f, 0.34f, 0.28f, 1f);

        [SerializeField]
        private Color decayedCorpseColor = new(0.18f, 0.14f, 0.12f, 1f);

        [SerializeField]
        private bool disableAfterDecay = true;

        private HealthComponent health;
        private Collider2D[] colliders;
        private Color originalColor;
        private bool hasOriginalColor;
        private bool isDecaying;
        private float decayStartedAt;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            colliders = GetComponents<Collider2D>();
            ResolveRenderer();
            CacheOriginalColor();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            health.Died += OnDied;
            health.Revived += OnRevived;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
                health.Revived -= OnRevived;
            }
        }

        private void Update()
        {
            if (!isDecaying || targetRenderer == null)
            {
                return;
            }

            float progress = despawnDelay > 0f ? Mathf.Clamp01((Time.time - decayStartedAt) / despawnDelay) : 1f;
            targetRenderer.color = Color.Lerp(freshCorpseColor, decayedCorpseColor, progress);

            if (progress >= 1f && disableAfterDecay)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDied(HealthChange change)
        {
            ResolveRenderer();
            CacheOriginalColor();
            SetColliderTriggers(true);
            isDecaying = true;
            decayStartedAt = Time.time;

            if (targetRenderer != null)
            {
                targetRenderer.color = freshCorpseColor;
            }
        }

        private void OnRevived(HealthChange change)
        {
            isDecaying = false;
            SetColliderTriggers(false);

            if (targetRenderer != null && hasOriginalColor)
            {
                targetRenderer.color = originalColor;
            }
        }

        private void ResolveRenderer()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void CacheOriginalColor()
        {
            if (targetRenderer == null || hasOriginalColor)
            {
                return;
            }

            originalColor = targetRenderer.color;
            hasOriginalColor = true;
        }

        private void SetColliderTriggers(bool isTrigger)
        {
            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponents<Collider2D>();
            }

            foreach (Collider2D collider in colliders)
            {
                if (collider != null)
                {
                    collider.isTrigger = isTrigger;
                }
            }
        }
    }
}
