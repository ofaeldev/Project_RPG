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
                GameplayUIEvents.ShowWarning("Nao foi possivel descartar o item.", source: feedbackSource);
                return false;
            }

            if (!dropper.TryDrop(stack.Item, stack.ItemId, 1, worldPosition, out _))
            {
                return false;
            }

            return inventory.RemoveItemFromSlot(slotIndex, 1);
        }
    }
}
