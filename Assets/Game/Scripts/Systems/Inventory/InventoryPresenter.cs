using System.Collections.Generic;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public sealed class InventoryPresenter
    {
        private readonly InventoryPanelView view;
        private readonly InventoryDetailsFormatter detailsFormatter;

        public InventoryPresenter(InventoryPanelView view, InventoryDetailsFormatter detailsFormatter)
        {
            this.view = view;
            this.detailsFormatter = detailsFormatter;
        }

        public void Refresh(
            InventoryUIController owner,
            InventoryManager inventory,
            InventoryInteractionFlow interactionFlow,
            List<InventoryItemStack> visibleItems,
            List<InventorySlotUI> slots,
            int minimumVisibleSlots)
        {
            visibleItems.Clear();
            if (inventory != null)
            {
                visibleItems.AddRange(inventory.GetOrderedItems());
            }

            interactionFlow.ClampSelection(visibleItems.Count);
            int visibleSlotCount = inventory != null
                ? UnityEngine.Mathf.Max(minimumVisibleSlots, inventory.SlotCapacity)
                : minimumVisibleSlots;
            EnsureSlotCount(slots, UnityEngine.Mathf.Max(visibleSlotCount, visibleItems.Count));

            for (int i = 0; i < slots.Count; i++)
            {
                InventoryItemStack stack = i < visibleItems.Count ? visibleItems[i] : null;
                slots[i].Bind(owner, i, stack, i == interactionFlow.SelectedIndex);
            }

            bool isEmpty = visibleItems.Count == 0;
            view.SetEmptyContentVisible(isEmpty, "Inventario vazio.");
            UpdateDetails(inventory, interactionFlow, visibleItems);
            UpdateUseButtons(inventory, interactionFlow, visibleItems);
        }

        public void UpdateDetails(
            InventoryManager inventory,
            InventoryInteractionFlow interactionFlow,
            IReadOnlyList<InventoryItemStack> visibleItems)
        {
            InventoryItemStack stack = interactionFlow.GetSelectedStack(visibleItems);
            if (stack == null)
            {
                view.SetDetailsText(detailsFormatter.FormatEmptySelection());
                return;
            }

            bool canUse = inventory != null && inventory.CanUseSlot(interactionFlow.SelectedIndex, new ItemUseContext(null));
            view.SetDetailsText(detailsFormatter.Format(stack, canUse));
        }

        public void UpdateUseButtons(
            InventoryManager inventory,
            InventoryInteractionFlow interactionFlow,
            IReadOnlyList<InventoryItemStack> visibleItems)
        {
            InventoryItemStack selectedStack = interactionFlow.GetSelectedStack(visibleItems);
            bool canUse = selectedStack != null &&
                inventory != null &&
                inventory.CanUseSlot(interactionFlow.SelectedIndex, new ItemUseContext(null));
            view.SetUseButtons(canUse, selectedStack != null);
        }

        private void EnsureSlotCount(List<InventorySlotUI> slots, int targetCount)
        {
            while (slots.Count < targetCount)
            {
                InventorySlotUI slot = view.CreateSlot(slots.Count);
                if (slot == null)
                {
                    break;
                }

                slots.Add(slot);
            }

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].gameObject.SetActive(i < targetCount);
            }
        }
    }
}
