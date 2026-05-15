using System.Collections.Generic;
using RPGProject.Systems;

namespace RPGProject.Gameplay
{
    public sealed class LootClaimService
    {
        public int ClaimAll(IReadOnlyList<ItemStackDefinition> loot, IInventoryService inventory)
        {
            if (loot == null || inventory == null)
            {
                return 0;
            }

            int grantedStacks = 0;
            foreach (ItemStackDefinition stack in loot)
            {
                if (stack != null && stack.IsValid && inventory.AddItem(stack.Item, stack.Amount))
                {
                    grantedStacks++;
                }
            }

            return grantedStacks;
        }
    }
}
