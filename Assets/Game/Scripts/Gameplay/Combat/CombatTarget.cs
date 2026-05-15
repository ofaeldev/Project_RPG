using System;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class CombatTarget : MonoBehaviour
    {
        [Header("Selection")]
        [SerializeField]
        private GameObject selectionFrameRoot;

        [SerializeField]
        private Color selectedFrameColor = new(1f, 0.22f, 0.16f, 1f);

        [SerializeField]
        private Vector2 frameSize = new(0.72f, 0.58f);

        [SerializeField]
        [Min(0.01f)]
        private float frameThickness = 0.05f;

        private HealthComponent health;
        private bool isSelected;

        public event Action<CombatTarget> Selected;
        public event Action<CombatTarget> Deselected;
        public event Action<CombatTarget, HealthChange> Defeated;

        public HealthComponent Health => ResolveHealth();
        public bool IsSelected => isSelected;
        public bool CanBeAttacked => isActiveAndEnabled && ResolveHealth() != null && !health.IsDead;

        private void Awake()
        {
            ResolveHealth();
            EnsureSelectionPresenter();
        }

        private void OnEnable()
        {
            ResolveHealth();

            health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (isSelected == selected)
            {
                return;
            }

            isSelected = selected;
            EnsureSelectionPresenter();
            GetComponent<Systems.CombatSelectionPresenter>()?.SetSelectedImmediate(isSelected);

            if (isSelected)
            {
                Selected?.Invoke(this);
            }
            else
            {
                Deselected?.Invoke(this);
            }
        }

        private HealthComponent ResolveHealth()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            return health;
        }

        private void EnsureSelectionPresenter()
        {
            if (GetComponent<Systems.CombatSelectionPresenter>() != null)
            {
                return;
            }

            Systems.CombatSelectionPresenter presenter = gameObject.AddComponent<Systems.CombatSelectionPresenter>();
            presenter.Configure(selectedFrameColor, frameSize, frameThickness);
        }

        private void OnDied(HealthChange change)
        {
            SetSelected(false);
            Defeated?.Invoke(this, change);
        }
    }
}
