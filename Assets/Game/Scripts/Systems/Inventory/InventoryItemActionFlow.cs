using System.Collections.Generic;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class InventoryItemActionFlow
    {
        private readonly InventoryDropFlow dropFlow;

        public InventoryItemActionFlow(InventoryDropFlow dropFlow)
        {
            this.dropFlow = dropFlow;
        }

        public bool TryUseSelected(
            InventoryManager inventory,
            InventoryInteractionFlow interactionFlow,
            IReadOnlyList<InventoryItemStack> visibleItems)
        {
            if (inventory == null || interactionFlow == null)
            {
                return false;
            }

            InventoryItemStack selectedStack = interactionFlow.GetSelectedStack(visibleItems);
            if (selectedStack == null)
            {
                return false;
            }

            bool succeeded = inventory.TryUseSlot(interactionFlow.SelectedIndex, new ItemUseContext(null), out _);
            GameplayEvents.PublishInventoryItemActionResolved(selectedStack, interactionFlow.SelectedIndex, "Use", succeeded);
            return succeeded;
        }

        public bool TryDropSelected(
            InventoryManager inventory,
            InventoryWorldDropper dropper,
            InventoryInteractionFlow interactionFlow,
            IReadOnlyList<InventoryItemStack> visibleItems,
            Vector2 worldPosition,
            Object feedbackSource)
        {
            if (inventory == null || interactionFlow == null || dropFlow == null)
            {
                GameplayUIEvents.ShowWarning("Nao foi possivel descartar o item.", source: feedbackSource);
                return false;
            }

            InventoryItemStack selectedStack = interactionFlow.GetSelectedStack(visibleItems);
            if (selectedStack == null)
            {
                return false;
            }

            bool succeeded = dropFlow.TryDropFromSlot(
                inventory,
                dropper,
                selectedStack,
                interactionFlow.SelectedIndex,
                worldPosition,
                feedbackSource);
            GameplayEvents.PublishInventoryItemActionResolved(selectedStack, interactionFlow.SelectedIndex, "Drop", succeeded);
            return succeeded;
        }
    }
}
