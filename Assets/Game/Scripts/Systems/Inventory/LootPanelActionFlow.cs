using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class LootPanelActionFlow
    {
        public bool TryTakeAll(ILootSource lootSource, LootService lootService)
        {
            if (lootSource == null)
            {
                return false;
            }

            if (lootService != null)
            {
                return lootService.ClaimAll(lootSource, lootSource as Object) > 0;
            }

            int availableStacks = CountAvailableStacks(lootSource);
            int claimedStacks = lootSource.ClaimAllLoot();
            GameplayEvents.PublishLootTaken(lootSource, availableStacks, claimedStacks, lootSource as Object);
            return claimedStacks > 0;
        }

        private static int CountAvailableStacks(ILootSource lootSource)
        {
            if (lootSource.Loot == null)
            {
                return 0;
            }

            int count = 0;
            foreach (ItemStackDefinition stack in lootSource.Loot)
            {
                if (stack != null && stack.IsValid)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
