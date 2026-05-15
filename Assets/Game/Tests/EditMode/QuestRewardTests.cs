using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class QuestRewardTests
    {
        private readonly List<Object> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(createdObject);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void ClaimReward_AddsConfiguredItemsToInventory()
        {
            InventoryManager inventory = CreateManager<InventoryManager>("InventoryManager");
            QuestManager questManager = CreateManager<QuestManager>("QuestManager");
            ItemDefinition coin = CreateItem("coin", isStackable: true, maxStackSize: 99);
            QuestDefinition quest = CreateQuest("rat_hunt", CreateStack(coin, 25));

            Assert.IsTrue(questManager.TryAcceptQuest(quest));
            Assert.IsTrue(questManager.CompleteQuest(quest.QuestId));

            Assert.IsTrue(questManager.TryClaimReward(quest));

            Assert.AreEqual(QuestState.RewardClaimed, questManager.GetQuestState(quest.QuestId));
            Assert.AreEqual(25, inventory.GetAmount(coin));
        }

        [Test]
        public void ClaimReward_DoesNotClaimWhenInventoryCannotFitRewards()
        {
            InventoryManager inventory = CreateManager<InventoryManager>("InventoryManager");
            QuestManager questManager = CreateManager<QuestManager>("QuestManager");
            ItemDefinition reward = CreateItem("reward", isStackable: false);
            QuestDefinition quest = CreateQuest("full_bag", CreateStack(reward, 1));

            for (int i = 0; i < inventory.SlotCapacity; i++)
            {
                Assert.IsTrue(inventory.AddItem(CreateItem($"filler_{i}", isStackable: false), 1));
            }

            Assert.IsTrue(questManager.TryAcceptQuest(quest));
            Assert.IsTrue(questManager.CompleteQuest(quest.QuestId));

            Assert.IsFalse(questManager.TryClaimReward(quest));

            Assert.AreEqual(QuestState.Completed, questManager.GetQuestState(quest.QuestId));
            Assert.AreEqual(0, inventory.GetAmount(reward));
        }

        private T CreateManager<T>(string name) where T : Component
        {
            GameObject gameObject = new(name);
            createdObjects.Add(gameObject);
            T manager = gameObject.AddComponent<T>();
            InvokeUnityMessage(manager, "Awake");
            return manager;
        }

        private ItemDefinition CreateItem(string itemId, bool isStackable = true, int maxStackSize = 99)
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            createdObjects.Add(item);
            SetField(item, "itemId", itemId);
            SetField(item, "displayName", itemId);
            SetField(item, "isStackable", isStackable);
            SetField(item, "maxStackSize", maxStackSize);
            return item;
        }

        private QuestDefinition CreateQuest(string questId, params ItemStackDefinition[] rewardItems)
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            createdObjects.Add(quest);
            SetField(quest, "questId", questId);
            SetField(quest, "title", questId);
            SetField(quest, "rewardItems", rewardItems);
            return quest;
        }

        private static ItemStackDefinition CreateStack(ItemDefinition item, int amount)
        {
            var stack = new ItemStackDefinition();
            SetField(stack, "item", item);
            SetField(stack, "amount", amount);
            return stack;
        }

        private static void SetField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            FieldInfo field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' was not found.");
            field.SetValue(target, value);
        }

        private static void InvokeUnityMessage<T>(T target, string methodName) where T : Component
        {
            MethodInfo method = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(target, null);
        }
    }
}
