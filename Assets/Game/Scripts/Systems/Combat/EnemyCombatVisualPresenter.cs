using RPGProject.Gameplay;
using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class EnemyCombatVisualPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SpriteRenderer targetRenderer;

        [Header("Health Bar")]
        [SerializeField]
        private Vector3 healthBarOffset = new(0f, 0.55f, 0f);

        [SerializeField]
        private Vector2 healthBarSize = new(0.62f, 0.08f);

        [SerializeField]
        private bool hideHealthBarWhenFull = true;

        [Header("Hit Text")]
        [SerializeField]
        private Vector3 textSpawnOffset = new(0f, 0.65f, 0f);

        [SerializeField]
        private Vector3 textFloatOffset = new(0f, 0.55f, 0f);

        [SerializeField]
        private float textLifetime = 0.8f;

        [SerializeField]
        private float textFontSize = 3f;

        [Header("Hit Flash")]
        [SerializeField]
        private Color flashColor = Color.white;

        [SerializeField]
        private float flashSeconds = 0.08f;

        [Header("Corpse Decay")]
        [SerializeField]
        private float corpseDespawnDelay = 18f;

        [SerializeField]
        private Color freshCorpseColor = new(0.46f, 0.34f, 0.28f, 1f);

        [SerializeField]
        private Color decayedCorpseColor = new(0.18f, 0.14f, 0.12f, 1f);

        [Header("Colors")]
        [SerializeField]
        private Color healthFillColor = new(0.2f, 0.95f, 0.25f, 1f);

        [SerializeField]
        private Color healthBackgroundColor = new(0.14f, 0.05f, 0.05f, 0.95f);

        [SerializeField]
        private Color damageTextColor = new(1f, 0.22f, 0.16f, 1f);

        private HealthComponent health;
        private Transform healthBarRoot;
        private SpriteRenderer healthFillRenderer;
        private SpriteRenderer healthBackgroundRenderer;
        private Collider2D[] colliders;
        private Color originalColor;
        private bool hasOriginalColor;
        private bool isDecaying;
        private float decayStartedAt;
        private float flashUntilTime;
        private static Sprite barSprite;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            colliders = GetComponents<Collider2D>();
            ResolveRenderer();
            CacheOriginalColor();
            EnsureHealthBar();
            RefreshHealthBar();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            health.HealthChanged += OnHealthChanged;
            health.Died += OnDied;
            health.Revived += OnRevived;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
                health.Died -= OnDied;
                health.Revived -= OnRevived;
            }
        }

        private void Update()
        {
            UpdateHitFlash();
            UpdateCorpseDecay();
        }

        private void LateUpdate()
        {
            if (healthBarRoot != null)
            {
                healthBarRoot.localPosition = healthBarOffset;
            }
        }

        private void OnHealthChanged(HealthChange change)
        {
            RefreshHealthBar();

            if (change.ChangeType == HealthChangeType.Damage && change.Amount > 0)
            {
                SpawnFloatingText(change.Amount.ToString(), damageTextColor);
                StartHitFlash();
            }
        }

        private void OnDied(HealthChange change)
        {
            RefreshHealthBar();
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
            RestoreOriginalColor();
            RefreshHealthBar();
        }

        private void EnsureHealthBar()
        {
            if (healthBarRoot == null)
            {
                Transform existing = transform.Find("HealthBar");
                GameObject root = existing != null ? existing.gameObject : new GameObject("HealthBar");
                root.transform.SetParent(transform, false);
                healthBarRoot = root.transform;
            }

            healthBackgroundRenderer = EnsureBarRenderer("Background", healthBackgroundRenderer, healthBackgroundColor, 45);
            healthFillRenderer = EnsureBarRenderer("Fill", healthFillRenderer, healthFillColor, 46);
        }

        private SpriteRenderer EnsureBarRenderer(string childName, SpriteRenderer currentRenderer, Color color, int sortingOrder)
        {
            if (currentRenderer != null)
            {
                return currentRenderer;
            }

            Transform existing = healthBarRoot.Find(childName);
            GameObject child = existing != null ? existing.gameObject : new GameObject(childName);
            child.transform.SetParent(healthBarRoot, false);

            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = child.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetBarSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void RefreshHealthBar()
        {
            EnsureHealthBar();

            bool shouldShow = health != null && !health.IsDead && (!hideHealthBarWhenFull || health.CurrentHealth < health.MaximumHealth);
            healthBarRoot.gameObject.SetActive(shouldShow);

            if (!shouldShow)
            {
                return;
            }

            float normalizedHealth = Mathf.Clamp01(health.NormalizedHealth);
            healthBackgroundRenderer.transform.localScale = healthBarSize;
            healthFillRenderer.transform.localScale = new Vector3(healthBarSize.x * normalizedHealth, healthBarSize.y, 1f);
            healthFillRenderer.transform.localPosition = new Vector3(-healthBarSize.x * (1f - normalizedHealth) * 0.5f, 0f, -0.01f);
        }

        private void SpawnFloatingText(string value, Color color)
        {
            GameObject textObject = new("FloatingCombatText", typeof(TextMeshPro), typeof(FloatingCombatText));
            Vector3 position = transform.position + textSpawnOffset;
            TextMeshPro text = textObject.GetComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = textFontSize;
            text.fontStyle = FontStyles.Bold;
            text.sortingOrder = 80;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            FloatingCombatText floatingText = textObject.GetComponent<FloatingCombatText>();
            floatingText.Initialize(value, color, position, textFloatOffset, textLifetime);
        }

        private void StartHitFlash()
        {
            ResolveRenderer();
            CacheOriginalColor();

            if (targetRenderer == null || isDecaying)
            {
                return;
            }

            targetRenderer.color = flashColor;
            flashUntilTime = Time.time + Mathf.Max(0.01f, flashSeconds);
        }

        private void UpdateHitFlash()
        {
            if (flashUntilTime > 0f && Time.time >= flashUntilTime)
            {
                flashUntilTime = 0f;
                RestoreOriginalColor();
            }
        }

        private void UpdateCorpseDecay()
        {
            if (!isDecaying || targetRenderer == null)
            {
                return;
            }

            float progress = corpseDespawnDelay > 0f ? Mathf.Clamp01((Time.time - decayStartedAt) / corpseDespawnDelay) : 1f;
            targetRenderer.color = Color.Lerp(freshCorpseColor, decayedCorpseColor, progress);

            if (progress >= 1f)
            {
                gameObject.SetActive(false);
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

        private void RestoreOriginalColor()
        {
            if (targetRenderer != null && hasOriginalColor && !isDecaying)
            {
                targetRenderer.color = originalColor;
            }
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

        private static Sprite GetBarSprite()
        {
            if (barSprite != null)
            {
                return barSprite;
            }

            Texture2D texture = new(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            barSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            barSprite.hideFlags = HideFlags.HideAndDontSave;
            return barSprite;
        }
    }
}
