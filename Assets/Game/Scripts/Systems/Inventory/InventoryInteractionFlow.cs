using System.Collections.Generic;
using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class InventoryInteractionFlow
    {
        public int SelectedIndex { get; private set; } = -1;
        public InventorySlotUI DraggedSlot { get; private set; }
        public int PendingDropTargetIndex { get; private set; } = -1;

        public void ClampSelection(int itemCount)
        {
            SelectedIndex = itemCount == 0 ? -1 : Mathf.Clamp(SelectedIndex, 0, itemCount - 1);
        }

        public bool SelectSlot(int slotIndex, int itemCount)
        {
            if (slotIndex < 0 || slotIndex >= itemCount)
            {
                SelectedIndex = -1;
                return false;
            }

            SelectedIndex = slotIndex;
            return true;
        }

        public InventoryItemStack GetSelectedStack(IReadOnlyList<InventoryItemStack> items)
        {
            return SelectedIndex >= 0 && items != null && SelectedIndex < items.Count ? items[SelectedIndex] : null;
        }

        public void BeginDrag(InventorySlotUI slot)
        {
            DraggedSlot = slot;
            PendingDropTargetIndex = -1;
            if (slot != null)
            {
                SelectedIndex = slot.SlotIndex;
            }
        }

        public void SetDropTarget(int targetSlotIndex)
        {
            PendingDropTargetIndex = targetSlotIndex;
        }

        public bool HasPendingSlotTransfer()
        {
            return DraggedSlot != null &&
                PendingDropTargetIndex >= 0 &&
                PendingDropTargetIndex != DraggedSlot.SlotIndex;
        }

        public void SelectDropTarget(int itemCount)
        {
            SelectedIndex = Mathf.Clamp(PendingDropTargetIndex, 0, Mathf.Max(0, itemCount - 1));
        }

        public void ClearDrag()
        {
            DraggedSlot = null;
            PendingDropTargetIndex = -1;
        }
    }
}
