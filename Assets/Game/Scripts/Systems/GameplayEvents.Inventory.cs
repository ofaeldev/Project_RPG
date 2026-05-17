using System;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    public readonly struct InventoryItemActionEvent
    {
        public InventoryItemActionEvent(InventoryItemStack stack, int slotIndex, string actionName, bool succeeded)
        {
            Stack = stack;
            SlotIndex = slotIndex;
            ActionName = actionName;
            Succeeded = succeeded;
        }

        public InventoryItemStack Stack { get; }
        public int SlotIndex { get; }
        public string ActionName { get; }
        public bool Succeeded { get; }
    }

    public enum InventoryDropFailureReason
    {
        None,
        MissingDependencies,
        InvalidItem,
        BlockedPosition,
        RemoveFailed
    }

    public readonly struct InventoryDropEvent
    {
        public InventoryDropEvent(
            InventoryItemStack stack,
            string itemName,
            GameObject droppedObject,
            bool succeeded,
            InventoryDropFailureReason failureReason,
            UnityEngine.Object feedbackSource)
        {
            Stack = stack;
            ItemName = itemName;
            DroppedObject = droppedObject;
            Succeeded = succeeded;
            FailureReason = failureReason;
            FeedbackSource = feedbackSource;
        }

        public InventoryItemStack Stack { get; }
        public string ItemName { get; }
        public GameObject DroppedObject { get; }
        public bool Succeeded { get; }
        public InventoryDropFailureReason FailureReason { get; }
        public UnityEngine.Object FeedbackSource { get; }
    }

    public readonly struct LootTakenEvent
    {
        public LootTakenEvent(ILootSource lootSource, bool succeeded)
            : this(lootSource, succeeded ? 1 : 0, succeeded ? 1 : 0, lootSource as UnityEngine.Object)
        {
        }

        public LootTakenEvent(ILootSource lootSource, int availableStacks, int claimedStacks, UnityEngine.Object feedbackSource)
        {
            LootSource = lootSource;
            AvailableStacks = availableStacks;
            ClaimedStacks = claimedStacks;
            FeedbackSource = feedbackSource;
        }

        public ILootSource LootSource { get; }
        public int AvailableStacks { get; }
        public int ClaimedStacks { get; }
        public UnityEngine.Object FeedbackSource { get; }
        public bool Succeeded => ClaimedStacks > 0;
    }

    public static partial class GameplayEvents
    {
        public static event Action<InventoryItemActionEvent> InventoryItemActionResolved;
        public static event Action<InventoryDropEvent> InventoryDropResolved;
        public static event Action<LootTakenEvent> LootTaken;

        public static void PublishInventoryItemActionResolved(InventoryItemStack stack, int slotIndex, string actionName, bool succeeded)
        {
            InventoryItemActionResolved?.Invoke(new InventoryItemActionEvent(stack, slotIndex, actionName, succeeded));
        }

        public static void PublishInventoryDropResolved(
            InventoryItemStack stack,
            string itemName,
            GameObject droppedObject,
            bool succeeded,
            InventoryDropFailureReason failureReason,
            UnityEngine.Object feedbackSource)
        {
            InventoryDropResolved?.Invoke(new InventoryDropEvent(
                stack,
                itemName,
                droppedObject,
                succeeded,
                failureReason,
                feedbackSource));
        }

        public static void PublishLootTaken(ILootSource lootSource, bool succeeded)
        {
            LootTaken?.Invoke(new LootTakenEvent(lootSource, succeeded));
        }

        public static void PublishLootTaken(ILootSource lootSource, int availableStacks, int claimedStacks, UnityEngine.Object feedbackSource)
        {
            LootTaken?.Invoke(new LootTakenEvent(lootSource, availableStacks, claimedStacks, feedbackSource));
        }
    }
}
