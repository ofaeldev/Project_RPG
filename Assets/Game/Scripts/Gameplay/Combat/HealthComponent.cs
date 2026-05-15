using System;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HealthComponent : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField]
        [Min(1)]
        private int maximumHealth = 100;

        [SerializeField]
        [Min(0)]
        private int currentHealth = 100;

        [SerializeField]
        private bool startAtFullHealth = true;

        public event Action<HealthChange> HealthChanged;
        public event Action<HealthChange> Died;
        public event Action<HealthChange> Revived;

        public int MaximumHealth => maximumHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0;
        public float NormalizedHealth => maximumHealth > 0 ? (float)currentHealth / maximumHealth : 0f;

        private void Awake()
        {
            maximumHealth = Mathf.Max(1, maximumHealth);
            currentHealth = startAtFullHealth ? maximumHealth : Mathf.Clamp(currentHealth, 0, maximumHealth);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1, maximumHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);
        }
#endif

        public bool ApplyDamage(int amount, UnityEngine.Object source = null)
        {
            if (amount <= 0 || IsDead)
            {
                return false;
            }

            int previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            HealthChange change = PublishChange(HealthChangeType.Damage, previousHealth, amount, source);

            if (change.WasFatal)
            {
                Died?.Invoke(change);
            }

            return previousHealth != currentHealth;
        }

        public bool Heal(int amount, UnityEngine.Object source = null)
        {
            if (amount <= 0 || IsDead || currentHealth >= maximumHealth)
            {
                return false;
            }

            int previousHealth = currentHealth;
            currentHealth = Mathf.Min(maximumHealth, currentHealth + amount);
            PublishChange(HealthChangeType.Heal, previousHealth, amount, source);
            return previousHealth != currentHealth;
        }

        public bool SetMaximumHealth(int value, bool fillToMaximum = false, UnityEngine.Object source = null)
        {
            int nextMaximumHealth = Mathf.Max(1, value);
            int previousMaximumHealth = maximumHealth;
            int previousHealth = currentHealth;

            maximumHealth = nextMaximumHealth;
            currentHealth = fillToMaximum
                ? maximumHealth
                : Mathf.Clamp(currentHealth, 0, maximumHealth);

            if (previousMaximumHealth == maximumHealth && previousHealth == currentHealth)
            {
                return false;
            }

            HealthChange change = PublishChange(HealthChangeType.SetMaximum, previousHealth, maximumHealth - previousMaximumHealth, source);

            if (previousHealth > 0 && currentHealth <= 0)
            {
                Died?.Invoke(change);
            }

            return true;
        }

        public bool Revive(int healthAmount = 1, UnityEngine.Object source = null)
        {
            if (!IsDead)
            {
                return false;
            }

            int previousHealth = currentHealth;
            currentHealth = Mathf.Clamp(healthAmount, 1, maximumHealth);
            HealthChange change = PublishChange(HealthChangeType.Revive, previousHealth, currentHealth, source);
            Revived?.Invoke(change);
            return true;
        }

        private HealthChange PublishChange(HealthChangeType changeType, int previousHealth, int amount, UnityEngine.Object source)
        {
            HealthChange change = new(changeType, previousHealth, currentHealth, maximumHealth, amount, source);
            HealthChanged?.Invoke(change);
            return change;
        }
    }
}
