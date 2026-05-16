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
                bool serviceSucceeded = lootService.ClaimAll(lootSource, lootSource as Object) > 0;
                GameplayEvents.PublishLootTaken(lootSource, serviceSucceeded);
                return serviceSucceeded;
            }

            bool directSucceeded = lootSource.ClaimAllLoot() > 0;
            GameplayEvents.PublishLootTaken(lootSource, directSucceeded);
            return directSucceeded;
        }
    }
}
