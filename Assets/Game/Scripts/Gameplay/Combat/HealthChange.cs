using UnityEngine;

namespace RPGProject.Gameplay
{
    public readonly struct HealthChange
    {
        public HealthChange(
            HealthChangeType changeType,
            int previousHealth,
            int currentHealth,
            int maximumHealth,
            int amount,
            Object source)
        {
            ChangeType = changeType;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            MaximumHealth = maximumHealth;
            Amount = amount;
            Source = source;
        }

        public HealthChangeType ChangeType { get; }
        public int PreviousHealth { get; }
        public int CurrentHealth { get; }
        public int MaximumHealth { get; }
        public int Amount { get; }
        public Object Source { get; }
        public bool WasFatal => PreviousHealth > 0 && CurrentHealth <= 0;
    }
}
