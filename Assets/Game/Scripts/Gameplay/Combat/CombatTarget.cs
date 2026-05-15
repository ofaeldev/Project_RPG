using System;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class CombatTarget : MonoBehaviour
    {
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

        private void OnDied(HealthChange change)
        {
            SetSelected(false);
            Defeated?.Invoke(this, change);
        }
    }
}
