using System.Collections.Generic;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class CorpseLootSource : MonoBehaviour, ILootSource
    {
        [Header("Loot")]
        [SerializeField]
        private string displayName = "Corpse";

        [SerializeField]
        private ItemStackDefinition[] loot = new ItemStackDefinition[0];

        [SerializeField]
        private LootTableDefinition lootTable;

        [SerializeField]
        private bool grantLootOnlyOnce = true;

        [SerializeField]
        private bool wasLooted;

        private static readonly ItemStackDefinition[] EmptyLoot = new ItemStackDefinition[0];
        private readonly List<ItemStackDefinition> rolledLoot = new();
        private readonly LootClaimService lootClaimService = new();
        private HealthComponent health;
        private bool hasRolledLoot;

        public string DisplayName => displayName;
        public IReadOnlyList<ItemStackDefinition> Loot
        {
            get
            {
                EnsureLootRolled();
                return grantLootOnlyOnce && wasLooted ? EmptyLoot : rolledLoot;
            }
        }

        public bool WasLooted => wasLooted;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            if (health != null)
            {
                health.Died += OnDied;
                health.Revived += OnRevived;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
                health.Revived -= OnRevived;
            }
        }

        public int ClaimAllLoot()
        {
            EnsureLootRolled();

            if (grantLootOnlyOnce && wasLooted)
            {
                return 0;
            }

            int grantedStacks = lootClaimService.ClaimAll(rolledLoot, InventoryManager.Instance);
            wasLooted = grantLootOnlyOnce || grantedStacks > 0;
            return grantedStacks;
        }

        public void ResetLootRoll()
        {
            hasRolledLoot = false;
            wasLooted = false;
            rolledLoot.Clear();
        }

        private void EnsureLootRolled()
        {
            if (hasRolledLoot)
            {
                return;
            }

            rolledLoot.Clear();
            if (lootTable != null)
            {
                rolledLoot.AddRange(lootTable.RollLoot());
            }
            else if (loot != null)
            {
                rolledLoot.AddRange(loot);
            }

            hasRolledLoot = true;
        }

        private void OnDied(HealthChange change)
        {
            ResetLootRoll();
            EnsureLootRolled();
        }

        private void OnRevived(HealthChange change)
        {
            ResetLootRoll();
        }
    }
}
