using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [System.Serializable]
    public sealed class InventoryRequirement
    {
        [Tooltip("Preferred item reference for this requirement.")]
        [SerializeField]
        private ItemDefinition item;

        [Tooltip("Fallback item id while item assets are not created yet.")]
        [SerializeField]
        private string itemId = string.Empty;

        [SerializeField]
        [Min(1)]
        private int amount = 1;

        [Tooltip("If true, the required item amount is removed after the requirement succeeds.")]
        [SerializeField]
        private bool consumeOnSuccess;

        public ItemDefinition Item => item;
        public string ItemId => item != null ? item.ItemId : itemId;
        public int Amount => Mathf.Max(1, amount);
        public bool ConsumeOnSuccess => consumeOnSuccess;
        public bool HasRequirement => !string.IsNullOrWhiteSpace(ItemId);

        public bool IsMet()
        {
            return !HasRequirement
                || (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(ItemId, Amount));
        }

        public bool TryConsume()
        {
            if (!HasRequirement || !consumeOnSuccess)
            {
                return true;
            }

            return InventoryManager.Instance != null && InventoryManager.Instance.RemoveItem(ItemId, Amount);
        }

        public string GetDisplayText()
        {
            string itemName = item != null && !string.IsNullOrWhiteSpace(item.DisplayName)
                ? item.DisplayName
                : ItemId;

            if (string.IsNullOrWhiteSpace(itemName))
            {
                itemName = "um item";
            }

            return Amount > 1 ? $"{itemName} x{Amount}" : itemName;
        }
    }
}
