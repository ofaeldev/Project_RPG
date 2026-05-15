using UnityEngine;

namespace RPGProject.Gameplay
{
    public enum ItemCategory
    {
        Generic,
        Key,
        Consumable,
        Quest,
        Equipment,
        Currency
    }

    [CreateAssetMenu(menuName = "RPG/Inventory/Item Definition", fileName = "NewItemDefinition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable id used by inventory, save/load, quests, and locks. Keep this unique.")]
        [SerializeField]
        private string itemId = string.Empty;

        [SerializeField]
        private string displayName = "New Item";

        [TextArea(2, 5)]
        [SerializeField]
        private string description = string.Empty;

        [SerializeField]
        private ItemCategory category = ItemCategory.Generic;

        [SerializeField]
        private Sprite icon;

        [Header("Use")]
        [Tooltip("Optional behaviour executed when the player uses this item.")]
        [SerializeField]
        private ItemUseEffect useEffect;

        [Tooltip("If true, one item is consumed after a successful use.")]
        [SerializeField]
        private bool consumeOnUse;

        [Header("Stacking")]
        [Tooltip("If false, this item will always occupy/represent a single unit.")]
        [SerializeField]
        private bool isStackable = true;

        [SerializeField]
        [Min(1)]
        private int maxStackSize = 99;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public Sprite Icon => icon;
        public ItemUseEffect UseEffect => useEffect;
        public bool ConsumeOnUse => consumeOnUse;
        public bool CanBeUsed => useEffect != null;
        public bool IsStackable => isStackable;
        public int MaxStackSize => isStackable ? Mathf.Max(1, maxStackSize) : 1;
        public bool HasValidId => !string.IsNullOrWhiteSpace(itemId);
    }
}
