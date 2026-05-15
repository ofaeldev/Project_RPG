using System;
using System.Collections.Generic;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class InventoryManager : MonoBehaviour, IInventoryService
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Inventory")]
        [SerializeField]
        [Min(1)]
        private int slotCapacity = 18;

        public event Action<ItemDefinition, int, int> ItemAmountChanged;
        public event Action<string, int, int> ItemAmountChangedById;
        public event Action InventoryOrderChanged;
        public event Action<ItemUseResult, GameObject> ItemUseResolved;

        private readonly InventoryModel model = new();

        public IReadOnlyCollection<InventoryItemStack> Items => model.Items;
        public int SlotCapacity => model.SlotCapacity;
        public int UsedSlots => model.UsedSlots;
        public int FreeSlots => model.FreeSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            model.SetSlotCapacity(slotCapacity);
        }

        public void RegisterItemDefinition(ItemDefinition item)
        {
            model.RegisterItemDefinition(item);
        }

        public bool HasItem(ItemDefinition item, int amount = 1)
        {
            return model.HasItem(item, amount);
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            return model.HasItem(itemId, amount);
        }

        public int GetAmount(ItemDefinition item)
        {
            return model.GetAmount(item);
        }

        public int GetAmount(string itemId)
        {
            return model.GetAmount(itemId);
        }

        public bool CanAddItem(ItemDefinition item, int amount = 1)
        {
            return model.CanAddItem(item, amount);
        }

        public bool CanAddItem(string itemId, int amount = 1)
        {
            return model.CanAddItem(itemId, amount);
        }

        public bool AddItem(ItemDefinition item, int amount = 1)
        {
            if (!model.TryAddItem(item, amount, out InventoryChange change))
            {
                return false;
            }

            PublishChanged(change);
            return true;
        }

        public bool AddItem(string itemId, int amount = 1)
        {
            if (!model.TryAddItem(itemId, amount, out InventoryChange change))
            {
                return false;
            }

            PublishChanged(change);
            return true;
        }

        public bool RemoveItem(ItemDefinition item, int amount = 1)
        {
            return item != null && RemoveItem(item.ItemId, amount);
        }

        public bool RemoveItem(string itemId, int amount = 1)
        {
            if (!model.TryRemoveItem(itemId, amount, out InventoryChange change))
            {
                return false;
            }

            PublishChanged(change);
            return true;
        }

        public bool RemoveItemFromSlot(int slotIndex, int amount = 1)
        {
            if (!model.TryRemoveFromSlot(slotIndex, amount, out InventoryChange change))
            {
                return false;
            }

            PublishChanged(change);
            InventoryOrderChanged?.Invoke();
            return true;
        }

        public IReadOnlyList<InventoryItemStack> GetOrderedItems()
        {
            return model.GetOrderedItems();
        }

        public bool MoveItemToIndex(string itemId, int targetIndex)
        {
            if (!model.MoveItemToIndex(itemId, targetIndex))
            {
                return false;
            }

            InventoryOrderChanged?.Invoke();
            return true;
        }

        public bool MoveSlotToIndex(int sourceIndex, int targetIndex)
        {
            if (!model.MoveSlotToIndex(sourceIndex, targetIndex))
            {
                return false;
            }

            InventoryOrderChanged?.Invoke();
            return true;
        }

        public bool TryMergeSlots(int sourceIndex, int targetIndex)
        {
            if (!model.TryMergeSlots(sourceIndex, targetIndex))
            {
                return false;
            }

            InventoryOrderChanged?.Invoke();
            return true;
        }

        public bool UseItem(ItemDefinition item, GameObject user = null, GameObject target = null)
        {
            return TryUseItem(item, new ItemUseContext(user, target), out _);
        }

        public bool UseItem(string itemId, GameObject user = null, GameObject target = null)
        {
            return TryUseItem(itemId, new ItemUseContext(user, target), out _);
        }

        public bool CanUseItem(string itemId, ItemUseContext context)
        {
            return model.CanUseItem(itemId, context);
        }

        public bool CanUseSlot(int slotIndex, ItemUseContext context)
        {
            return model.CanUseSlot(slotIndex, context);
        }

        public bool TryUseItem(ItemDefinition item, ItemUseContext context, out ItemUseResult result)
        {
            bool wasUsed = model.TryUseItem(item, context, out result, out InventoryChange? consumedChange);
            PublishItemUseResolved(result, context.User);
            PublishConsumedChange(consumedChange);
            return wasUsed;
        }

        public bool TryUseItem(string itemId, ItemUseContext context, out ItemUseResult result)
        {
            bool wasUsed = model.TryUseItem(itemId, context, out result, out InventoryChange? consumedChange);
            PublishItemUseResolved(result, context.User);
            PublishConsumedChange(consumedChange);
            return wasUsed;
        }

        public bool TryUseSlot(int slotIndex, ItemUseContext context, out ItemUseResult result)
        {
            bool wasUsed = model.TryUseSlot(slotIndex, context, out result, out InventoryChange? consumedChange);
            PublishItemUseResolved(result, context.User);
            PublishConsumedChange(consumedChange);
            return wasUsed;
        }

        public IReadOnlyList<InventoryItemSnapshot> CreateInventorySnapshot()
        {
            return model.CreateInventorySnapshot();
        }

        public void LoadInventorySnapshot(IEnumerable<InventoryItemSnapshot> snapshots, bool notifyChanges = false)
        {
            IReadOnlyList<InventoryChange> changes = model.LoadInventorySnapshot(snapshots);
            if (!notifyChanges)
            {
                return;
            }

            foreach (InventoryChange change in changes)
            {
                PublishChanged(change);
            }
        }

        private void PublishConsumedChange(InventoryChange? consumedChange)
        {
            if (consumedChange.HasValue)
            {
                PublishChanged(consumedChange.Value);
            }
        }

        private void PublishItemUseResolved(ItemUseResult result, GameObject source)
        {
            if (!string.IsNullOrWhiteSpace(result.FeedbackMessage))
            {
                ItemUseResolved?.Invoke(result, source);
            }
        }

        private void PublishChanged(InventoryChange change)
        {
            ItemAmountChanged?.Invoke(change.Item, change.PreviousAmount, change.CurrentAmount);
            ItemAmountChangedById?.Invoke(change.ItemId, change.PreviousAmount, change.CurrentAmount);
        }
    }
}
