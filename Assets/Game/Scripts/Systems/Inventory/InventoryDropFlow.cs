using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class InventoryDropFlow
    {
        public bool TryDropFromSlot(
            InventoryManager inventory,
            InventoryWorldDropper dropper,
            InventoryItemStack stack,
            int slotIndex,
            Vector2 worldPosition,
            Object feedbackSource)
        {
            if (stack == null || dropper == null || inventory == null)
            {
                GameplayEvents.PublishInventoryDropResolved(
                    stack,
                    GetItemName(stack),
                    null,
                    false,
                    InventoryDropFailureReason.MissingDependencies,
                    feedbackSource);
                return false;
            }

            if (!dropper.TryDrop(
                    stack.Item,
                    stack.ItemId,
                    1,
                    worldPosition,
                    out GameObject droppedObject,
                    out InventoryDropFailureReason failureReason,
                    out string itemName))
            {
                GameplayEvents.PublishInventoryDropResolved(
                    stack,
                    itemName,
                    null,
                    false,
                    failureReason,
                    feedbackSource);
                return false;
            }

            bool removed = inventory.RemoveItemFromSlot(slotIndex, 1);
            GameplayEvents.PublishInventoryDropResolved(
                stack,
                itemName,
                droppedObject,
                removed,
                removed ? InventoryDropFailureReason.None : InventoryDropFailureReason.RemoveFailed,
                removed ? droppedObject : feedbackSource);
            return removed;
        }

        private static string GetItemName(InventoryItemStack stack)
        {
            if (stack == null)
            {
                return string.Empty;
            }

            return stack.Item != null && !string.IsNullOrWhiteSpace(stack.Item.DisplayName)
                ? stack.Item.DisplayName
                : stack.ItemId;
        }
    }
}
