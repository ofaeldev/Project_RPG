using System;
using System.Collections.Generic;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [Serializable]
    public struct InventoryItemSnapshot
    {
        [SerializeField]
        private string itemId;

        [SerializeField]
        private int amount;

        public string ItemId => itemId;
        public int Amount => amount;

        public InventoryItemSnapshot(string itemId, int amount)
        {
            this.itemId = itemId;
            this.amount = amount;
        }
    }

    public sealed class InventoryItemStack
    {
        public ItemDefinition Item { get; private set; }
        public string ItemId { get; }
        public int Amount { get; private set; }
        public int MaxAmount { get; private set; }
        public bool IsFull => Amount >= MaxAmount;
        public bool IsStackable => Item == null || Item.IsStackable;

        public InventoryItemStack(ItemDefinition item, int amount)
            : this(item, item != null ? item.ItemId : string.Empty, amount, item != null ? item.MaxStackSize : int.MaxValue)
        {
        }

        public InventoryItemStack(string itemId, int amount)
            : this(null, itemId, amount, int.MaxValue)
        {
        }

        private InventoryItemStack(ItemDefinition item, string itemId, int amount, int maxAmount)
        {
            Item = item;
            ItemId = itemId;
            MaxAmount = Mathf.Max(1, maxAmount);
            Amount = Mathf.Clamp(amount, 0, MaxAmount);
        }

        public int AddUpTo(int amount)
        {
            if (amount <= 0 || IsFull)
            {
                return 0;
            }

            int acceptedAmount = Mathf.Min(amount, MaxAmount - Amount);
            Amount += acceptedAmount;
            return acceptedAmount;
        }

        public int AttachDefinition(ItemDefinition item)
        {
            if (item == null || !string.Equals(ItemId, item.ItemId, StringComparison.Ordinal))
            {
                return 0;
            }

            int previousAmount = Amount;
            Item = item;
            MaxAmount = item.MaxStackSize;
            Amount = Mathf.Clamp(Amount, 0, MaxAmount);
            return Mathf.Max(0, previousAmount - Amount);
        }

        public bool Remove(int amount)
        {
            if (amount <= 0 || Amount < amount)
            {
                return false;
            }

            Amount -= amount;
            return true;
        }
    }

    public readonly struct InventoryChange
    {
        public ItemDefinition Item { get; }
        public string ItemId { get; }
        public int PreviousAmount { get; }
        public int CurrentAmount { get; }

        public InventoryChange(ItemDefinition item, string itemId, int previousAmount, int currentAmount)
        {
            Item = item;
            ItemId = item != null ? item.ItemId : itemId;
            PreviousAmount = previousAmount;
            CurrentAmount = currentAmount;
        }
    }

    public sealed class InventoryModel
    {
        private readonly List<InventoryItemStack> slots = new();
        private readonly Dictionary<string, ItemDefinition> itemDefinitionsById = new();
        private int slotCapacity;

        public InventoryModel(int slotCapacity = 18)
        {
            SetSlotCapacity(slotCapacity);
        }

        public IReadOnlyCollection<InventoryItemStack> Items => slots;
        public int SlotCapacity => slotCapacity;
        public int UsedSlots => slots.Count;
        public int FreeSlots => Mathf.Max(0, slotCapacity - slots.Count);

        public void SetSlotCapacity(int capacity)
        {
            slotCapacity = Mathf.Max(1, capacity);
        }

        public void RegisterItemDefinition(ItemDefinition item)
        {
            if (item != null && item.HasValidId)
            {
                itemDefinitionsById[item.ItemId] = item;
                AttachDefinitionToExistingStacks(item);
            }
        }

        public bool HasItem(ItemDefinition item, int amount = 1)
        {
            return item != null && HasItem(item.ItemId, amount);
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            return GetAmount(itemId) >= Mathf.Max(1, amount);
        }

        public int GetAmount(ItemDefinition item)
        {
            return item != null ? GetAmount(item.ItemId) : 0;
        }

        public int GetAmount(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            int amount = 0;
            foreach (InventoryItemStack stack in slots)
            {
                if (string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
                {
                    amount += stack.Amount;
                }
            }

            return amount;
        }

        public bool CanAddItem(ItemDefinition item, int amount = 1)
        {
            if (item == null || !item.HasValidId || amount <= 0)
            {
                return false;
            }

            return GetRemainingCapacity(item) >= amount;
        }

        public bool CanAddItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            ItemDefinition knownDefinition = GetRegisteredDefinition(itemId);
            return knownDefinition == null ? FreeSlots > 0 : CanAddItem(knownDefinition, amount);
        }

        public bool TryAddItem(ItemDefinition item, int amount, out InventoryChange change)
        {
            change = default;
            if (!CanAddItem(item, amount))
            {
                return false;
            }

            RegisterItemDefinition(item);
            int previousAmount = GetAmount(item.ItemId);
            int remainingAmount = amount;

            if (item.IsStackable)
            {
                foreach (InventoryItemStack stack in slots)
                {
                    if (remainingAmount <= 0)
                    {
                        break;
                    }

                    if (stack.Item == item || string.Equals(stack.ItemId, item.ItemId, StringComparison.Ordinal))
                    {
                        remainingAmount -= stack.AddUpTo(remainingAmount);
                    }
                }
            }

            while (remainingAmount > 0)
            {
                int stackAmount = item.IsStackable ? Mathf.Min(remainingAmount, item.MaxStackSize) : 1;
                slots.Add(new InventoryItemStack(item, stackAmount));
                remainingAmount -= stackAmount;
            }

            change = new InventoryChange(item, item.ItemId, previousAmount, GetAmount(item.ItemId));
            return true;
        }

        public bool TryAddItem(string itemId, int amount, out InventoryChange change)
        {
            change = default;
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            ItemDefinition knownDefinition = GetRegisteredDefinition(itemId);
            if (knownDefinition != null)
            {
                return TryAddItem(knownDefinition, amount, out change);
            }

            if (FreeSlots <= 0)
            {
                return false;
            }

            int previousAmount = GetAmount(itemId);
            slots.Add(new InventoryItemStack(itemId, amount));
            change = new InventoryChange(null, itemId, previousAmount, GetAmount(itemId));
            return true;
        }

        public bool TryRemoveItem(ItemDefinition item, int amount, out InventoryChange change)
        {
            change = default;
            return item != null && TryRemoveItem(item.ItemId, amount, out change);
        }

        public bool TryRemoveItem(string itemId, int amount, out InventoryChange change)
        {
            change = default;
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0 || GetAmount(itemId) < amount)
            {
                return false;
            }

            int previousAmount = GetAmount(itemId);
            int remainingAmount = amount;
            for (int i = slots.Count - 1; i >= 0 && remainingAmount > 0; i--)
            {
                InventoryItemStack stack = slots[i];
                if (!string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
                {
                    continue;
                }

                int amountFromStack = Mathf.Min(stack.Amount, remainingAmount);
                stack.Remove(amountFromStack);
                remainingAmount -= amountFromStack;

                if (stack.Amount <= 0)
                {
                    slots.RemoveAt(i);
                }
            }

            change = new InventoryChange(GetRegisteredDefinition(itemId), itemId, previousAmount, GetAmount(itemId));
            return true;
        }

        public bool TryRemoveFromSlot(int slotIndex, int amount, out InventoryChange change)
        {
            change = default;
            if (slotIndex < 0 || slotIndex >= slots.Count || amount <= 0)
            {
                return false;
            }

            InventoryItemStack stack = slots[slotIndex];
            int previousAmount = GetAmount(stack.ItemId);
            if (!stack.Remove(amount))
            {
                return false;
            }

            if (stack.Amount <= 0)
            {
                slots.RemoveAt(slotIndex);
            }

            change = new InventoryChange(GetRegisteredDefinition(stack.ItemId), stack.ItemId, previousAmount, GetAmount(stack.ItemId));
            return true;
        }

        public IReadOnlyList<InventoryItemStack> GetOrderedItems()
        {
            return new List<InventoryItemStack>(slots);
        }

        public bool MoveItemToIndex(string itemId, int targetIndex)
        {
            int sourceIndex = FindSlotIndex(itemId);
            return sourceIndex >= 0 && MoveSlotToIndex(sourceIndex, targetIndex);
        }

        public bool MoveSlotToIndex(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= slots.Count)
            {
                return false;
            }

            InventoryItemStack stack = slots[sourceIndex];
            slots.RemoveAt(sourceIndex);
            int clampedIndex = Mathf.Clamp(targetIndex, 0, slots.Count);
            slots.Insert(clampedIndex, stack);
            return true;
        }

        public bool TryMergeSlots(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= slots.Count || targetIndex < 0 || targetIndex >= slots.Count || sourceIndex == targetIndex)
            {
                return false;
            }

            InventoryItemStack source = slots[sourceIndex];
            InventoryItemStack target = slots[targetIndex];
            if (!CanMerge(source, target))
            {
                return false;
            }

            int movedAmount = target.AddUpTo(source.Amount);
            if (movedAmount <= 0 || !source.Remove(movedAmount))
            {
                return false;
            }

            if (source.Amount <= 0)
            {
                slots.RemoveAt(sourceIndex);
            }

            return true;
        }

        public bool CanUseItem(string itemId, ItemUseContext context)
        {
            InventoryItemStack stack = FindFirstStack(itemId);
            ItemDefinition item = stack != null && stack.Item != null ? stack.Item : GetRegisteredDefinition(itemId);
            return stack != null && item != null && item.UseEffect != null && item.UseEffect.CanUse(item, context);
        }

        public bool CanUseSlot(int slotIndex, ItemUseContext context)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
            {
                return false;
            }

            InventoryItemStack stack = slots[slotIndex];
            ItemDefinition item = stack.Item != null ? stack.Item : GetRegisteredDefinition(stack.ItemId);
            return item != null && item.UseEffect != null && item.UseEffect.CanUse(item, context);
        }

        public bool TryUseItem(ItemDefinition item, ItemUseContext context, out ItemUseResult result, out InventoryChange? consumedChange)
        {
            consumedChange = null;
            if (item == null)
            {
                result = ItemUseResult.Failed("Item invalido.");
                return false;
            }

            RegisterItemDefinition(item);
            return TryUseItem(item.ItemId, context, out result, out consumedChange);
        }

        public bool TryUseItem(string itemId, ItemUseContext context, out ItemUseResult result, out InventoryChange? consumedChange)
        {
            consumedChange = null;
            result = ItemUseResult.Failed("Item indisponivel.");
            InventoryItemStack stack = FindFirstStack(itemId);
            if (stack == null)
            {
                return false;
            }

            ItemDefinition item = stack.Item != null ? stack.Item : GetRegisteredDefinition(itemId);
            if (item == null || item.UseEffect == null)
            {
                string displayName = item != null ? item.DisplayName : itemId;
                result = ItemUseResult.Failed($"{displayName} nao pode ser usado.");
                return false;
            }

            result = item.UseEffect.Use(item, context);
            if (!result.WasUsed)
            {
                return false;
            }

            if (item.ConsumeOnUse && TryRemoveItem(itemId, 1, out InventoryChange change))
            {
                consumedChange = change;
            }

            return true;
        }

        public bool TryUseSlot(int slotIndex, ItemUseContext context, out ItemUseResult result, out InventoryChange? consumedChange)
        {
            consumedChange = null;
            result = ItemUseResult.Failed("Item indisponivel.");
            if (slotIndex < 0 || slotIndex >= slots.Count)
            {
                return false;
            }

            InventoryItemStack stack = slots[slotIndex];
            ItemDefinition item = stack.Item != null ? stack.Item : GetRegisteredDefinition(stack.ItemId);
            if (item == null || item.UseEffect == null)
            {
                string displayName = item != null ? item.DisplayName : stack.ItemId;
                result = ItemUseResult.Failed($"{displayName} nao pode ser usado.");
                return false;
            }

            result = item.UseEffect.Use(item, context);
            if (!result.WasUsed)
            {
                return false;
            }

            if (item.ConsumeOnUse && TryRemoveFromSlot(slotIndex, 1, out InventoryChange change))
            {
                consumedChange = change;
            }

            return true;
        }

        public IReadOnlyList<InventoryItemSnapshot> CreateInventorySnapshot()
        {
            var amountsById = new Dictionary<string, int>();
            foreach (InventoryItemStack stack in slots)
            {
                if (string.IsNullOrWhiteSpace(stack.ItemId) || stack.Amount <= 0)
                {
                    continue;
                }

                amountsById.TryGetValue(stack.ItemId, out int amount);
                amountsById[stack.ItemId] = amount + stack.Amount;
            }

            var snapshots = new List<InventoryItemSnapshot>(amountsById.Count);
            foreach (KeyValuePair<string, int> entry in amountsById)
            {
                snapshots.Add(new InventoryItemSnapshot(entry.Key, entry.Value));
            }

            return snapshots;
        }

        public IReadOnlyList<InventoryChange> LoadInventorySnapshot(IEnumerable<InventoryItemSnapshot> snapshots)
        {
            slots.Clear();

            var changes = new List<InventoryChange>();
            if (snapshots == null)
            {
                return changes;
            }

            foreach (InventoryItemSnapshot snapshot in snapshots)
            {
                if (string.IsNullOrWhiteSpace(snapshot.ItemId) || snapshot.Amount <= 0)
                {
                    continue;
                }

                ItemDefinition knownDefinition = GetRegisteredDefinition(snapshot.ItemId);
                int previousAmount = GetAmount(snapshot.ItemId);
                if (knownDefinition != null)
                {
                    AddSnapshotItem(knownDefinition, snapshot.Amount);
                }
                else if (FreeSlots > 0)
                {
                    slots.Add(new InventoryItemStack(snapshot.ItemId, snapshot.Amount));
                }

                changes.Add(new InventoryChange(knownDefinition, snapshot.ItemId, previousAmount, GetAmount(snapshot.ItemId)));
            }

            return changes;
        }

        private void AddSnapshotItem(ItemDefinition item, int amount)
        {
            int remainingAmount = amount;
            while (remainingAmount > 0 && FreeSlots > 0)
            {
                int stackAmount = item.IsStackable ? Mathf.Min(remainingAmount, item.MaxStackSize) : 1;
                slots.Add(new InventoryItemStack(item, stackAmount));
                remainingAmount -= stackAmount;
            }
        }

        private void AttachDefinitionToExistingStacks(ItemDefinition item)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventoryItemStack stack = slots[i];
                if (string.Equals(stack.ItemId, item.ItemId, StringComparison.Ordinal))
                {
                    int overflowAmount = stack.AttachDefinition(item);
                    while (overflowAmount > 0 && FreeSlots > 0)
                    {
                        int splitAmount = item.IsStackable ? Mathf.Min(overflowAmount, item.MaxStackSize) : 1;
                        slots.Insert(i + 1, new InventoryItemStack(item, splitAmount));
                        overflowAmount -= splitAmount;
                        i++;
                    }
                }
            }
        }

        private int GetRemainingCapacity(ItemDefinition item)
        {
            int remainingCapacity = FreeSlots * item.MaxStackSize;
            if (item.IsStackable)
            {
                foreach (InventoryItemStack stack in slots)
                {
                    if (stack.Item == item || string.Equals(stack.ItemId, item.ItemId, StringComparison.Ordinal))
                    {
                        remainingCapacity += Mathf.Max(0, stack.MaxAmount - stack.Amount);
                    }
                }
            }

            return remainingCapacity;
        }

        private InventoryItemStack FindFirstStack(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            foreach (InventoryItemStack stack in slots)
            {
                if (string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
                {
                    return stack;
                }
            }

            return null;
        }

        private static bool CanMerge(InventoryItemStack source, InventoryItemStack target)
        {
            return source != null
                && target != null
                && source.IsStackable
                && target.IsStackable
                && string.Equals(source.ItemId, target.ItemId, StringComparison.Ordinal)
                && !target.IsFull;
        }

        private int FindSlotIndex(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return -1;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (string.Equals(slots[i].ItemId, itemId, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private ItemDefinition GetRegisteredDefinition(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId) && itemDefinitionsById.TryGetValue(itemId, out ItemDefinition item)
                ? item
                : null;
        }
    }
}
