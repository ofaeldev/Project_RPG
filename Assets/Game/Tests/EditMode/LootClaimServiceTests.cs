using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class LootClaimServiceTests
    {
        private readonly List<Object> createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Object asset in createdAssets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }

            createdAssets.Clear();
        }

        [Test]
        public void ClaimAll_AddsValidStacksToInventory()
        {
            LootClaimService service = new();
            TestInventoryService inventory = new();
            ItemDefinition apple = CreateItem("apple");
            ItemDefinition coin = CreateItem("coin");
            ItemStackDefinition[] loot =
            {
                new(apple, 2),
                null,
                new(coin, 5)
            };

            int claimedStacks = service.ClaimAll(loot, inventory);

            Assert.AreEqual(2, claimedStacks);
            Assert.AreEqual(2, inventory.AddedItems.Count);
            Assert.AreSame(apple, inventory.AddedItems[0].Item);
            Assert.AreEqual(2, inventory.AddedItems[0].Amount);
            Assert.AreSame(coin, inventory.AddedItems[1].Item);
            Assert.AreEqual(5, inventory.AddedItems[1].Amount);
        }

        [Test]
        public void ClaimAll_CountsOnlyStacksAcceptedByInventory()
        {
            LootClaimService service = new();
            TestInventoryService inventory = new(canAdd: false);
            ItemStackDefinition[] loot = { new(CreateItem("gem"), 1) };

            int claimedStacks = service.ClaimAll(loot, inventory);

            Assert.AreEqual(0, claimedStacks);
            Assert.AreEqual(1, inventory.AddAttempts);
        }

        [Test]
        public void ClaimAll_ReturnsZeroWithoutInventory()
        {
            LootClaimService service = new();
            ItemStackDefinition[] loot = { new(CreateItem("key"), 1) };

            int claimedStacks = service.ClaimAll(loot, null);

            Assert.AreEqual(0, claimedStacks);
        }

        private ItemDefinition CreateItem(string itemId)
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            createdAssets.Add(item);
            SetField(item, "itemId", itemId);
            SetField(item, "displayName", itemId);
            return item;
        }

        private static void SetField<T>(ItemDefinition item, string fieldName, T value)
        {
            FieldInfo field = typeof(ItemDefinition).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' was not found.");
            field.SetValue(item, value);
        }

        private sealed class TestInventoryService : IInventoryService
        {
            private readonly bool canAdd;

            public TestInventoryService(bool canAdd = true)
            {
                this.canAdd = canAdd;
            }

            public List<(ItemDefinition Item, int Amount)> AddedItems { get; } = new();
            public int AddAttempts { get; private set; }
            public IReadOnlyCollection<InventoryItemStack> Items => System.Array.Empty<InventoryItemStack>();
            public int FreeSlots => canAdd ? 1 : 0;

            public bool AddItem(ItemDefinition item, int amount = 1)
            {
                AddAttempts++;
                if (!canAdd)
                {
                    return false;
                }

                AddedItems.Add((item, amount));
                return true;
            }

            public bool CanAddItem(ItemDefinition item, int amount = 1)
            {
                return canAdd;
            }

            public bool HasItem(string itemId, int amount = 1)
            {
                return false;
            }

            public bool RemoveItem(string itemId, int amount = 1)
            {
                return false;
            }

            public bool TryUseSlot(int slotIndex, ItemUseContext context, out ItemUseResult result)
            {
                result = new ItemUseResult(false, string.Empty);
                return false;
            }
        }
    }
}
