using System.Collections.Generic;
using RPGProject.Gameplay;
using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class CombatWorldUIController : MonoBehaviour
    {
        [Header("Discovery")]
        [SerializeField]
        [Min(0.1f)]
        private float discoveryInterval = 0.5f;

        [SerializeField]
        private bool showPlayerHealthBars;

        [Header("Health Bars")]
        [SerializeField]
        private Vector3 healthBarOffset = new(0f, 0.55f, 0f);

        [SerializeField]
        private Vector2 healthBarSize = new(0.62f, 0.08f);

        [SerializeField]
        private Color healthFillColor = new(0.2f, 0.95f, 0.25f, 1f);

        [SerializeField]
        private Color healthBackgroundColor = new(0.14f, 0.05f, 0.05f, 0.95f);

        [SerializeField]
        private bool hideHealthBarWhenFull = true;

        [Header("Selection")]
        [SerializeField]
        private Color selectedFrameColor = new(1f, 0.22f, 0.16f, 1f);

        [SerializeField]
        private Vector2 selectionFrameSize = new(0.72f, 0.58f);

        [SerializeField]
        [Min(0.01f)]
        private float selectionFrameThickness = 0.05f;

        [Header("Floating Text")]
        [SerializeField]
        private Vector3 floatingTextSpawnOffset = new(0f, 0.65f, 0f);

        [SerializeField]
        private Vector3 floatingTextMoveOffset = new(0f, 0.55f, 0f);

        [SerializeField]
        [Min(0.05f)]
        private float floatingTextLifetime = 0.8f;

        [SerializeField]
        [Min(0.1f)]
        private float floatingTextFontSize = 3f;

        [SerializeField]
        private Color damageColor = new(1f, 0.22f, 0.16f, 1f);

        [SerializeField]
        private Color healColor = new(0.3f, 1f, 0.36f, 1f);

        private readonly Dictionary<HealthComponent, HealthBarView> healthBars = new();
        private readonly Dictionary<HealthComponent, System.Action<HealthChange>> healthChangedHandlers = new();
        private readonly Dictionary<HealthComponent, System.Action<HealthChange>> healthRevivedHandlers = new();
        private readonly Dictionary<CombatActor, SelectionFrameView> actorSelectionFrames = new();
        private readonly Dictionary<CombatTarget, SelectionFrameView> legacySelectionFrames = new();
        private readonly List<HealthComponent> healthRemovalBuffer = new();
        private readonly List<CombatActor> actorRemovalBuffer = new();
        private readonly List<CombatTarget> targetRemovalBuffer = new();
        private float nextDiscoveryTime;

        private void OnEnable()
        {
            DiscoverCombatObjects();
        }

        private void OnDisable()
        {
            ClearHealthBars();
            ClearActorSelectionFrames();
            ClearLegacySelectionFrames();
        }

        private void Update()
        {
            if (Time.time >= nextDiscoveryTime)
            {
                DiscoverCombatObjects();
                nextDiscoveryTime = Time.time + discoveryInterval;
            }
        }

        private void LateUpdate()
        {
            UpdateHealthBars();
            UpdateSelectionFrames();
        }

        private void DiscoverCombatObjects()
        {
            foreach (HealthComponent health in FindObjectsByType<HealthComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                RegisterHealth(health);
            }

            foreach (CombatActor actor in FindObjectsByType<CombatActor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                RegisterActor(actor);
            }

            foreach (CombatTarget target in FindObjectsByType<CombatTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                RegisterLegacyTarget(target);
            }
        }

        private void RegisterHealth(HealthComponent health)
        {
            if (health == null || healthBars.ContainsKey(health))
            {
                return;
            }

            HealthBarView view = null;
            if (ShouldShowHealthBar(health))
            {
                view = new HealthBarView(transform, health, healthBarOffset, healthBarSize, healthFillColor, healthBackgroundColor);
                view.Refresh(hideHealthBarWhenFull);
            }

            healthBars.Add(health, view);

            System.Action<HealthChange> changedHandler = change => OnHealthChanged(health, change);
            System.Action<HealthChange> revivedHandler = change => OnHealthRevived(health, change);
            healthChangedHandlers.Add(health, changedHandler);
            healthRevivedHandlers.Add(health, revivedHandler);
            health.HealthChanged += changedHandler;
            health.Revived += revivedHandler;
        }

        private void RegisterActor(CombatActor actor)
        {
            if (actor == null || actorSelectionFrames.ContainsKey(actor))
            {
                return;
            }

            SelectionFrameView view = new(transform, actor.transform, selectionFrameSize, selectionFrameThickness, selectedFrameColor);
            view.SetVisible(actor.IsSelected);
            actorSelectionFrames.Add(actor, view);
            actor.Selected += OnActorSelected;
            actor.Deselected += OnActorDeselected;
        }

        private void RegisterLegacyTarget(CombatTarget target)
        {
            if (target == null || legacySelectionFrames.ContainsKey(target))
            {
                return;
            }

            SelectionFrameView view = new(transform, target.transform, selectionFrameSize, selectionFrameThickness, selectedFrameColor);
            view.SetVisible(target.IsSelected);
            legacySelectionFrames.Add(target, view);
            target.Selected += OnLegacyTargetSelected;
            target.Deselected += OnLegacyTargetDeselected;
        }

        private bool ShouldShowHealthBar(HealthComponent health)
        {
            return showPlayerHealthBars || !health.CompareTag("Player");
        }

        private void OnHealthChanged(HealthComponent health, HealthChange change)
        {
            if (health != null && healthBars.TryGetValue(health, out HealthBarView targetView))
            {
                targetView?.Refresh(hideHealthBarWhenFull);
            }

            SpawnFloatingText(health, change);
        }

        private void OnHealthRevived(HealthComponent health, HealthChange change)
        {
            if (health != null && healthBars.TryGetValue(health, out HealthBarView view))
            {
                view?.Refresh(hideHealthBarWhenFull);
            }
        }

        private void SpawnFloatingText(HealthComponent health, HealthChange change)
        {
            if (health == null || change.Amount == 0)
            {
                return;
            }

            switch (change.ChangeType)
            {
                case HealthChangeType.Damage:
                    CreateFloatingText(health.transform.position + floatingTextSpawnOffset, change.Amount.ToString(), damageColor);
                    break;
                case HealthChangeType.Heal:
                case HealthChangeType.Revive:
                    CreateFloatingText(health.transform.position + floatingTextSpawnOffset, $"+{change.Amount}", healColor);
                    break;
            }
        }

        private void CreateFloatingText(Vector3 position, string value, Color color)
        {
            GameObject textObject = new("FloatingCombatText", typeof(TextMeshPro), typeof(FloatingCombatText));
            textObject.transform.SetParent(transform, true);
            textObject.transform.position = position;

            TextMeshPro text = textObject.GetComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = floatingTextFontSize;
            text.fontStyle = FontStyles.Bold;
            text.sortingOrder = 80;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            FloatingCombatText floatingText = textObject.GetComponent<FloatingCombatText>();
            floatingText.Initialize(value, color, position, floatingTextMoveOffset, floatingTextLifetime);
        }

        private void OnActorSelected(CombatActor actor)
        {
            if (actorSelectionFrames.TryGetValue(actor, out SelectionFrameView view))
            {
                view.SetVisible(true);
            }
        }

        private void OnActorDeselected(CombatActor actor)
        {
            if (actorSelectionFrames.TryGetValue(actor, out SelectionFrameView view))
            {
                view.SetVisible(false);
            }
        }

        private void OnLegacyTargetSelected(CombatTarget target)
        {
            if (legacySelectionFrames.TryGetValue(target, out SelectionFrameView view))
            {
                view.SetVisible(true);
            }
        }

        private void OnLegacyTargetDeselected(CombatTarget target)
        {
            if (legacySelectionFrames.TryGetValue(target, out SelectionFrameView view))
            {
                view.SetVisible(false);
            }
        }

        private void UpdateHealthBars()
        {
            healthRemovalBuffer.Clear();
            foreach ((HealthComponent health, HealthBarView view) in healthBars)
            {
                if (health == null)
                {
                    healthRemovalBuffer.Add(health);
                    continue;
                }

                view?.UpdatePosition();
            }

            foreach (HealthComponent health in healthRemovalBuffer)
            {
                UnregisterHealth(health);
            }
        }

        private void UpdateSelectionFrames()
        {
            actorRemovalBuffer.Clear();
            foreach ((CombatActor actor, SelectionFrameView view) in actorSelectionFrames)
            {
                if (actor == null)
                {
                    actorRemovalBuffer.Add(actor);
                    continue;
                }

                view.UpdatePosition();
            }

            foreach (CombatActor actor in actorRemovalBuffer)
            {
                UnregisterActor(actor);
            }

            targetRemovalBuffer.Clear();
            foreach ((CombatTarget target, SelectionFrameView view) in legacySelectionFrames)
            {
                if (target == null)
                {
                    targetRemovalBuffer.Add(target);
                    continue;
                }

                view.UpdatePosition();
            }

            foreach (CombatTarget target in targetRemovalBuffer)
            {
                UnregisterLegacyTarget(target);
            }
        }

        private void UnregisterHealth(HealthComponent health)
        {
            if (ReferenceEquals(health, null) || !healthBars.TryGetValue(health, out HealthBarView view))
            {
                return;
            }

            if (healthChangedHandlers.TryGetValue(health, out System.Action<HealthChange> changedHandler))
            {
                health.HealthChanged -= changedHandler;
                healthChangedHandlers.Remove(health);
            }

            if (healthRevivedHandlers.TryGetValue(health, out System.Action<HealthChange> revivedHandler))
            {
                health.Revived -= revivedHandler;
                healthRevivedHandlers.Remove(health);
            }

            view?.Destroy();
            healthBars.Remove(health);
        }

        private void UnregisterActor(CombatActor actor)
        {
            if (ReferenceEquals(actor, null) || !actorSelectionFrames.TryGetValue(actor, out SelectionFrameView view))
            {
                return;
            }

            actor.Selected -= OnActorSelected;
            actor.Deselected -= OnActorDeselected;
            view.Destroy();
            actorSelectionFrames.Remove(actor);
        }

        private void UnregisterLegacyTarget(CombatTarget target)
        {
            if (ReferenceEquals(target, null) || !legacySelectionFrames.TryGetValue(target, out SelectionFrameView view))
            {
                return;
            }

            target.Selected -= OnLegacyTargetSelected;
            target.Deselected -= OnLegacyTargetDeselected;
            view.Destroy();
            legacySelectionFrames.Remove(target);
        }

        private void ClearHealthBars()
        {
            foreach ((HealthComponent health, HealthBarView view) in healthBars)
            {
                if (health != null)
                {
                    if (healthChangedHandlers.TryGetValue(health, out System.Action<HealthChange> changedHandler))
                    {
                        health.HealthChanged -= changedHandler;
                    }

                    if (healthRevivedHandlers.TryGetValue(health, out System.Action<HealthChange> revivedHandler))
                    {
                        health.Revived -= revivedHandler;
                    }
                }

                view?.Destroy();
            }

            healthBars.Clear();
            healthChangedHandlers.Clear();
            healthRevivedHandlers.Clear();
        }

        private void ClearActorSelectionFrames()
        {
            foreach ((CombatActor actor, SelectionFrameView view) in actorSelectionFrames)
            {
                if (actor != null)
                {
                    actor.Selected -= OnActorSelected;
                    actor.Deselected -= OnActorDeselected;
                }

                view.Destroy();
            }

            actorSelectionFrames.Clear();
        }

        private void ClearLegacySelectionFrames()
        {
            foreach ((CombatTarget target, SelectionFrameView view) in legacySelectionFrames)
            {
                if (target != null)
                {
                    target.Selected -= OnLegacyTargetSelected;
                    target.Deselected -= OnLegacyTargetDeselected;
                }

                view.Destroy();
            }

            legacySelectionFrames.Clear();
        }

        private sealed class HealthBarView
        {
            private readonly HealthComponent health;
            private readonly Transform root;
            private readonly SpriteRenderer backgroundRenderer;
            private readonly SpriteRenderer fillRenderer;
            private readonly Vector3 offset;
            private readonly Vector2 size;

            public HealthBarView(Transform parent, HealthComponent health, Vector3 offset, Vector2 size, Color fillColor, Color backgroundColor)
            {
                this.health = health;
                this.offset = offset;
                this.size = size;

                root = new GameObject($"{health.name}_HealthBar").transform;
                root.SetParent(parent, true);
                backgroundRenderer = CreateRenderer(root, "Background", backgroundColor, 45);
                fillRenderer = CreateRenderer(root, "Fill", fillColor, 46);
                UpdatePosition();
            }

            public void UpdatePosition()
            {
                if (health != null)
                {
                    root.position = health.transform.position + offset;
                }
            }

            public void Refresh(bool hideWhenFull)
            {
                bool shouldShow = health != null && !health.IsDead && (!hideWhenFull || health.CurrentHealth < health.MaximumHealth);
                root.gameObject.SetActive(shouldShow);
                if (!shouldShow)
                {
                    return;
                }

                float normalizedHealth = Mathf.Clamp01(health.NormalizedHealth);
                backgroundRenderer.transform.localScale = size;
                fillRenderer.transform.localScale = new Vector3(size.x * normalizedHealth, size.y, 1f);
                fillRenderer.transform.localPosition = new Vector3(-size.x * (1f - normalizedHealth) * 0.5f, 0f, -0.01f);
            }

            public void Destroy()
            {
                if (root != null)
                {
                    Object.Destroy(root.gameObject);
                }
            }
        }

        private sealed class SelectionFrameView
        {
            private readonly Transform target;
            private readonly Transform root;

            public SelectionFrameView(Transform parent, Transform target, Vector2 size, float thickness, Color color)
            {
                this.target = target;
                root = new GameObject($"{target.name}_SelectionFrame").transform;
                root.SetParent(parent, true);
                CreateFrameBar(root, "Top", new Vector2(0f, size.y * 0.5f), new Vector2(size.x, thickness), color);
                CreateFrameBar(root, "Bottom", new Vector2(0f, -size.y * 0.5f), new Vector2(size.x, thickness), color);
                CreateFrameBar(root, "Left", new Vector2(-size.x * 0.5f, 0f), new Vector2(thickness, size.y), color);
                CreateFrameBar(root, "Right", new Vector2(size.x * 0.5f, 0f), new Vector2(thickness, size.y), color);
                UpdatePosition();
            }

            public void SetVisible(bool visible)
            {
                root.gameObject.SetActive(visible);
            }

            public void UpdatePosition()
            {
                if (target != null)
                {
                    root.position = target.position;
                }
            }

            public void Destroy()
            {
                if (root != null)
                {
                    Object.Destroy(root.gameObject);
                }
            }
        }

        private static void CreateFrameBar(Transform parent, string name, Vector2 localPosition, Vector2 localScale, Color color)
        {
            SpriteRenderer renderer = CreateRenderer(parent, name, color, 35);
            renderer.transform.localPosition = localPosition;
            renderer.transform.localScale = localScale;
        }

        private static SpriteRenderer CreateRenderer(Transform parent, string name, Color color, int sortingOrder)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = Vector3.zero;

            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = WorldUiSpriteCache.WhiteSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }
    }
}
