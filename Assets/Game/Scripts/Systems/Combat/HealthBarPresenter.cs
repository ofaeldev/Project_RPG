using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class HealthBarPresenter : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField]
        private Transform barRoot;

        [SerializeField]
        private SpriteRenderer fillRenderer;

        [SerializeField]
        private SpriteRenderer backgroundRenderer;

        [SerializeField]
        private Vector3 worldOffset = new(0f, 0.55f, 0f);

        [SerializeField]
        private Vector2 size = new(0.62f, 0.08f);

        [SerializeField]
        private Color fillColor = new(0.2f, 0.95f, 0.25f, 1f);

        [SerializeField]
        private Color backgroundColor = new(0.14f, 0.05f, 0.05f, 0.95f);

        [SerializeField]
        private bool hideWhenFull = true;

        private HealthComponent health;
        private static Sprite barSprite;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            EnsureBar();
            Refresh();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            health.HealthChanged += OnHealthChanged;
            health.Revived += OnHealthChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
                health.Revived -= OnHealthChanged;
            }
        }

        private void LateUpdate()
        {
            if (barRoot != null)
            {
                barRoot.localPosition = worldOffset;
            }
        }

        public void Refresh()
        {
            EnsureBar();

            if (health == null || barRoot == null || fillRenderer == null)
            {
                return;
            }

            bool shouldShow = !health.IsDead && (!hideWhenFull || health.CurrentHealth < health.MaximumHealth);
            barRoot.gameObject.SetActive(shouldShow);

            if (!shouldShow)
            {
                return;
            }

            float normalizedHealth = Mathf.Clamp01(health.NormalizedHealth);
            backgroundRenderer.transform.localScale = size;
            fillRenderer.transform.localScale = new Vector3(size.x * normalizedHealth, size.y, 1f);
            fillRenderer.transform.localPosition = new Vector3(-size.x * (1f - normalizedHealth) * 0.5f, 0f, -0.01f);
        }

        private void OnHealthChanged(HealthChange change)
        {
            Refresh();
        }

        private void EnsureBar()
        {
            if (barRoot == null)
            {
                Transform existing = transform.Find("HealthBar");
                GameObject root = existing != null ? existing.gameObject : new GameObject("HealthBar");
                root.transform.SetParent(transform, false);
                root.transform.localPosition = worldOffset;
                barRoot = root.transform;
            }

            backgroundRenderer = EnsureRenderer("Background", backgroundRenderer, backgroundColor, 45);
            fillRenderer = EnsureRenderer("Fill", fillRenderer, fillColor, 46);
        }

        private SpriteRenderer EnsureRenderer(string childName, SpriteRenderer currentRenderer, Color color, int sortingOrder)
        {
            if (currentRenderer != null)
            {
                return currentRenderer;
            }

            Transform existing = barRoot.Find(childName);
            GameObject child = existing != null ? existing.gameObject : new GameObject(childName);
            child.transform.SetParent(barRoot, false);
            child.transform.localPosition = Vector3.zero;

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
