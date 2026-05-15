using System.Collections.Generic;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [CreateAssetMenu(
        fileName = "LootTableDefinition",
        menuName = "RPG Project/Inventory/Loot Table")]
    public sealed class LootTableDefinition : ScriptableObject
    {
        [SerializeField]
        private LootDropEntry[] entries = new LootDropEntry[0];

        public IReadOnlyList<LootDropEntry> Entries => entries;

        public List<ItemStackDefinition> RollLoot()
        {
            List<ItemStackDefinition> rolledLoot = new();
            if (entries == null)
            {
                return rolledLoot;
            }

            foreach (LootDropEntry entry in entries)
            {
                if (entry == null || !entry.IsValid)
                {
                    continue;
                }

                if (Random.value > entry.DropChance)
                {
                    continue;
                }

                int amount = Random.Range(entry.MinAmount, entry.MaxAmount + 1);
                rolledLoot.Add(new ItemStackDefinition(entry.Item, amount));
            }

            return rolledLoot;
        }
    }
}
