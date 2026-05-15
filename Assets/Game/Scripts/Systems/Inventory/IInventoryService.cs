using System.Collections.Generic;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public interface IInventoryService
    {
        IReadOnlyCollection<InventoryItemStack> Items { get; }
        int FreeSlots { get; }

        bool AddItem(ItemDefinition item, int amount = 1);
        bool CanAddItem(ItemDefinition item, int amount = 1);
        bool TryUseSlot(int slotIndex, ItemUseContext context, out ItemUseResult result);
    }
}
