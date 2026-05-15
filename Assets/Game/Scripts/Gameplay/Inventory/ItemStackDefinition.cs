using System;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [Serializable]
    public sealed class ItemStackDefinition
    {
        public ItemStackDefinition()
        {
        }

        public ItemStackDefinition(ItemDefinition item, int amount)
        {
            this.item = item;
            this.amount = Mathf.Max(1, amount);
        }

        [SerializeField]
        private ItemDefinition item;

        [SerializeField]
        [Min(1)]
        private int amount = 1;

        public ItemDefinition Item => item;
        public int Amount => Mathf.Max(1, amount);
        public bool IsValid => item != null && item.HasValidId && Amount > 0;
    }
}
