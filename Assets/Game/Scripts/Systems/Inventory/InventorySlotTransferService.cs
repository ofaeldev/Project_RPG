namespace RPGProject.Systems
{
    public sealed class InventorySlotTransferService
    {
        public bool TryMoveOrMerge(InventoryManager inventory, int sourceIndex, int targetIndex, int visibleItemCount)
        {
            if (inventory == null || sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            {
                return false;
            }

            bool didMerge = targetIndex < visibleItemCount && inventory.TryMergeSlots(sourceIndex, targetIndex);
            if (didMerge)
            {
                return true;
            }

            return inventory.MoveSlotToIndex(sourceIndex, targetIndex);
        }
    }
}
