using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class CombatSelectionPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private CombatActor combatActor;

        [SerializeField]
        private CombatTarget legacyCombatTarget;

        [SerializeField]
        private GameObject selectionFrameRoot;

        [Header("Frame")]
        [SerializeField]
        private Color selectedFrameColor = new(1f, 0.22f, 0.16f, 1f);

        [SerializeField]
        private Vector2 frameSize = new(0.72f, 0.58f);

        [SerializeField]
        [Min(0.01f)]
        private float frameThickness = 0.05f;

        private static Sprite frameSprite;

        private void Awake()
        {
            ResolveTarget();
            EnsureSelectionFrame();
            ApplySelectionVisual(IsSelected());
        }

        private void OnEnable()
        {
            ResolveTarget();

            if (combatActor != null)
            {
                combatActor.Selected += OnActorSelected;
                combatActor.Deselected += OnActorDeselected;
            }

            if (legacyCombatTarget != null)
            {
                legacyCombatTarget.Selected += OnLegacyTargetSelected;
                legacyCombatTarget.Deselected += OnLegacyTargetDeselected;
            }

            ApplySelectionVisual(IsSelected());
        }

        private void OnDisable()
        {
            if (combatActor != null)
            {
                combatActor.Selected -= OnActorSelected;
                combatActor.Deselected -= OnActorDeselected;
            }

            if (legacyCombatTarget != null)
            {
                legacyCombatTarget.Selected -= OnLegacyTargetSelected;
                legacyCombatTarget.Deselected -= OnLegacyTargetDeselected;
            }
        }

        public void Configure(Color color, Vector2 size, float thickness)
        {
            selectedFrameColor = color;
            frameSize = size;
            frameThickness = Mathf.Max(0.01f, thickness);

            if (selectionFrameRoot != null)
            {
                Destroy(selectionFrameRoot);
                selectionFrameRoot = null;
            }

            EnsureSelectionFrame();
            ApplySelectionVisual(IsSelected());
        }

        public void SetSelectedImmediate(bool selected)
        {
            EnsureSelectionFrame();
            ApplySelectionVisual(selected);
        }

        private void ResolveTarget()
        {
            if (combatActor == null)
            {
                combatActor = GetComponent<CombatActor>();
            }

            if (legacyCombatTarget == null)
            {
                legacyCombatTarget = GetComponent<CombatTarget>();
            }
        }

        private bool IsSelected()
        {
            if (combatActor != null)
            {
                return combatActor.IsSelected;
            }

            return legacyCombatTarget != null && legacyCombatTarget.IsSelected;
        }

        private void OnActorSelected(CombatActor actor)
        {
            ApplySelectionVisual(true);
        }

        private void OnActorDeselected(CombatActor actor)
        {
            ApplySelectionVisual(false);
        }

        private void OnLegacyTargetSelected(CombatTarget target)
        {
            ApplySelectionVisual(true);
        }

        private void OnLegacyTargetDeselected(CombatTarget target)
        {
            ApplySelectionVisual(false);
        }

        private void EnsureSelectionFrame()
        {
            if (selectionFrameRoot != null)
            {
                return;
            }

            Transform existing = transform.Find("SelectionFrame");
            selectionFrameRoot = existing != null ? existing.gameObject : new GameObject("SelectionFrame");
            selectionFrameRoot.transform.SetParent(transform, false);
            selectionFrameRoot.transform.localPosition = Vector3.zero;

            CreateFrameBar("Top", new Vector2(0f, frameSize.y * 0.5f), new Vector2(frameSize.x, frameThickness));
            CreateFrameBar("Bottom", new Vector2(0f, -frameSize.y * 0.5f), new Vector2(frameSize.x, frameThickness));
            CreateFrameBar("Left", new Vector2(-frameSize.x * 0.5f, 0f), new Vector2(frameThickness, frameSize.y));
            CreateFrameBar("Right", new Vector2(frameSize.x * 0.5f, 0f), new Vector2(frameThickness, frameSize.y));
            selectionFrameRoot.SetActive(false);
        }

        private void CreateFrameBar(string barName, Vector2 localPosition, Vector2 localScale)
        {
            Transform existing = selectionFrameRoot.transform.Find(barName);
            GameObject bar = existing != null ? existing.gameObject : new GameObject(barName);
            bar.transform.SetParent(selectionFrameRoot.transform, false);
            bar.transform.localPosition = localPosition;
            bar.transform.localScale = localScale;

            SpriteRenderer renderer = bar.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = bar.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetFrameSprite();
            renderer.color = selectedFrameColor;
            renderer.sortingOrder = 35;
        }

        private void ApplySelectionVisual(bool selected)
        {
            if (selectionFrameRoot == null)
            {
                return;
            }

            selectionFrameRoot.SetActive(selected);
        }

        private static Sprite GetFrameSprite()
        {
            if (frameSprite != null)
            {
                return frameSprite;
            }

            Texture2D texture = new(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            frameSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            frameSprite.hideFlags = HideFlags.HideAndDontSave;
            return frameSprite;
        }
    }
}
