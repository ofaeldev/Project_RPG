using System;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [Serializable]
    public sealed class LootDropEntry
    {
        [SerializeField]
        private ItemDefinition item;

        [SerializeField]
        private LootRarity rarity = LootRarity.Common;

        [SerializeField]
        [Range(0f, 1f)]
        private float dropChance = 0.5f;

        [SerializeField]
        [Min(1)]
        private int minAmount = 1;

        [SerializeField]
        [Min(1)]
        private int maxAmount = 1;

        public ItemDefinition Item => item;
        public LootRarity Rarity => rarity;
        public float DropChance => Mathf.Clamp01(dropChance);
        public int MinAmount => Mathf.Max(1, minAmount);
        public int MaxAmount => Mathf.Max(MinAmount, maxAmount);
        public bool IsValid => item != null && item.HasValidId && DropChance > 0f;
    }
}
