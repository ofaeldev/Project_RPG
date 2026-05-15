using System;
using System.Collections.Generic;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public sealed class QuestRewardService
    {
        private readonly IInventoryService inventory;

        public QuestRewardService(IInventoryService inventory)
        {
            this.inventory = inventory;
        }

        public bool TryGrantRewardItems(QuestDefinition quest)
        {
            if (quest == null || !quest.HasRewardItems)
            {
                return true;
            }

            if (inventory == null)
            {
                return !HasValidRewardItems(quest);
            }

            if (!CanGrantRewardItems(quest))
            {
                return false;
            }

            foreach (ItemStackDefinition rewardItem in quest.RewardItems)
            {
                if (rewardItem == null || !rewardItem.IsValid)
                {
                    continue;
                }

                if (!inventory.AddItem(rewardItem.Item, rewardItem.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasValidRewardItems(QuestDefinition quest)
        {
            if (quest == null || !quest.HasRewardItems)
            {
                return false;
            }

            foreach (ItemStackDefinition rewardItem in quest.RewardItems)
            {
                if (rewardItem != null && rewardItem.IsValid)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanGrantRewardItems(QuestDefinition quest)
        {
            if (quest == null || inventory == null)
            {
                return false;
            }

            var rewardAmountsByItemId = new Dictionary<string, RewardItemCapacityRequest>();
            foreach (ItemStackDefinition rewardItem in quest.RewardItems)
            {
                if (rewardItem == null || !rewardItem.IsValid)
                {
                    continue;
                }

                string itemId = rewardItem.Item.ItemId;
                if (!rewardAmountsByItemId.TryGetValue(itemId, out RewardItemCapacityRequest request))
                {
                    request = new RewardItemCapacityRequest(rewardItem.Item, 0);
                }

                request.Amount += rewardItem.Amount;
                rewardAmountsByItemId[itemId] = request;
            }

            int requiredFreeSlots = 0;
            foreach (RewardItemCapacityRequest request in rewardAmountsByItemId.Values)
            {
                requiredFreeSlots += CountRequiredNewSlots(request.Item, request.Amount);
                if (requiredFreeSlots > inventory.FreeSlots)
                {
                    return false;
                }
            }

            return true;
        }

        private int CountRequiredNewSlots(ItemDefinition item, int amount)
        {
            if (inventory == null || item == null || amount <= 0)
            {
                return 0;
            }

            int remainingAmount = amount;
            if (item.IsStackable)
            {
                foreach (InventoryItemStack stack in inventory.Items)
                {
                    if (stack == null || !string.Equals(stack.ItemId, item.ItemId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    remainingAmount -= Math.Max(0, stack.MaxAmount - stack.Amount);
                    if (remainingAmount <= 0)
                    {
                        return 0;
                    }
                }

                return (int)Math.Ceiling(remainingAmount / (float)item.MaxStackSize);
            }

            return remainingAmount;
        }

        private struct RewardItemCapacityRequest
        {
            public ItemDefinition Item { get; }
            public int Amount { get; set; }

            public RewardItemCapacityRequest(ItemDefinition item, int amount)
            {
                Item = item;
                Amount = amount;
            }
        }
    }
}
