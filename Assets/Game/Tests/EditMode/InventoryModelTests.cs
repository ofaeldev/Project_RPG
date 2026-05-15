using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class InventoryModelTests
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
        public void AddItem_RespectsMaxStackSize()
        {
            InventoryModel inventory = new(slotCapacity: 1);
            ItemDefinition potion = CreateItem("potion", isStackable: true, maxStackSize: 5);

            Assert.IsTrue(inventory.TryAddItem(potion, 3, out InventoryChange firstChange));
            Assert.AreEqual(0, firstChange.PreviousAmount);
            Assert.AreEqual(3, firstChange.CurrentAmount);

            Assert.IsTrue(inventory.TryAddItem(potion, 2, out InventoryChange secondChange));
            Assert.AreEqual(3, secondChange.PreviousAmount);
            Assert.AreEqual(5, secondChange.CurrentAmount);

            Assert.IsFalse(inventory.TryAddItem(potion, 1, out _));
            Assert.AreEqual(5, inventory.GetAmount("potion"));
        }

        [Test]
        public void AddItem_NonStackableStoresSingleUnit()
        {
            InventoryModel inventory = new();
            ItemDefinition sword = CreateItem("sword", isStackable: false);

            Assert.IsTrue(inventory.CanAddItem(sword, 3));
            Assert.IsTrue(inventory.TryAddItem(sword, 3, out InventoryChange change));
            Assert.AreEqual(3, change.CurrentAmount);
            Assert.AreEqual(3, inventory.GetOrderedItems().Count);

            Assert.IsTrue(inventory.CanAddItem(sword));
            Assert.AreEqual(3, inventory.GetAmount(sword));
        }

        [Test]
        public void AddItem_SplitsStackAcrossFreeSlots()
        {
            InventoryModel inventory = new(slotCapacity: 3);
            ItemDefinition berry = CreateItem("berry", isStackable: true, maxStackSize: 5);

            Assert.IsTrue(inventory.TryAddItem(berry, 12, out InventoryChange change));

            IReadOnlyList<InventoryItemStack> items = inventory.GetOrderedItems();
            Assert.AreEqual(12, change.CurrentAmount);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(5, items[0].Amount);
            Assert.AreEqual(5, items[1].Amount);
            Assert.AreEqual(2, items[2].Amount);
        }

        [Test]
        public void RemoveItem_RemovesStackWhenAmountReachesZero()
        {
            InventoryModel inventory = new();
            ItemDefinition coin = CreateItem("coin", maxStackSize: 20);
            inventory.TryAddItem(coin, 2, out _);

            Assert.IsTrue(inventory.TryRemoveItem("coin", 2, out InventoryChange change));
            Assert.AreEqual(2, change.PreviousAmount);
            Assert.AreEqual(0, change.CurrentAmount);
            Assert.IsFalse(inventory.HasItem("coin"));
            Assert.AreEqual(0, inventory.GetOrderedItems().Count);
        }

        [Test]
        public void MoveItemToIndex_ReordersVisibleItems()
        {
            InventoryModel inventory = new();
            ItemDefinition key = CreateItem("key");
            ItemDefinition potion = CreateItem("potion");
            ItemDefinition coin = CreateItem("coin");
            inventory.TryAddItem(key, 1, out _);
            inventory.TryAddItem(potion, 1, out _);
            inventory.TryAddItem(coin, 1, out _);

            Assert.IsTrue(inventory.MoveItemToIndex("coin", 0));

            IReadOnlyList<InventoryItemStack> items = inventory.GetOrderedItems();
            Assert.AreEqual("coin", items[0].ItemId);
            Assert.AreEqual("key", items[1].ItemId);
            Assert.AreEqual("potion", items[2].ItemId);
        }

        [Test]
        public void TryMergeSlots_CombinesMatchingStacksAndFreesSourceSlot()
        {
            InventoryModel inventory = new(slotCapacity: 3);
            ItemDefinition meat = CreateItem("meat", maxStackSize: 10);
            inventory.TryAddItem(meat, 10, out _);
            inventory.TryAddItem(CreateItem("apple"), 1, out _);
            inventory.TryAddItem(meat, 2, out _);
            inventory.TryRemoveFromSlot(0, 8, out _);

            Assert.IsTrue(inventory.TryMergeSlots(2, 0));

            IReadOnlyList<InventoryItemStack> items = inventory.GetOrderedItems();
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("meat", items[0].ItemId);
            Assert.AreEqual(4, items[0].Amount);
            Assert.AreEqual("apple", items[1].ItemId);
        }

        [Test]
        public void TryRemoveFromSlot_RemovesSelectedNonStackableSlotOnly()
        {
            InventoryModel inventory = new();
            ItemDefinition key = CreateItem("key", isStackable: false);
            inventory.TryAddItem(key, 2, out _);

            Assert.IsTrue(inventory.TryRemoveFromSlot(1, 1, out InventoryChange change));

            IReadOnlyList<InventoryItemStack> items = inventory.GetOrderedItems();
            Assert.AreEqual(2, change.PreviousAmount);
            Assert.AreEqual(1, change.CurrentAmount);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("key", items[0].ItemId);
        }

        [Test]
        public void Snapshot_RestoresAmountsAndOrder()
        {
            InventoryModel source = new();
            source.TryAddItem(CreateItem("key"), 1, out _);
            source.TryAddItem(CreateItem("coin", maxStackSize: 10), 7, out _);

            InventoryModel restored = new();
            restored.LoadInventorySnapshot(source.CreateInventorySnapshot());

            IReadOnlyList<InventoryItemStack> items = restored.GetOrderedItems();
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("key", items[0].ItemId);
            Assert.AreEqual("coin", items[1].ItemId);
            Assert.AreEqual(7, restored.GetAmount("coin"));
        }

        [Test]
        public void RegisterItemDefinition_AttachesDefinitionToLoadedStack()
        {
            InventoryModel inventory = new(slotCapacity: 2);
            inventory.LoadInventorySnapshot(new[] { new InventoryItemSnapshot("meat", 9) });
            ItemDefinition meat = CreateItem("meat", maxStackSize: 10);

            inventory.RegisterItemDefinition(meat);

            IReadOnlyList<InventoryItemStack> items = inventory.GetOrderedItems();
            Assert.AreSame(meat, items[0].Item);
            Assert.IsTrue(inventory.CanAddItem(meat, 1));
            Assert.IsTrue(inventory.TryAddItem(meat, 1, out _));
            Assert.AreEqual(10, inventory.GetAmount("meat"));
            Assert.AreEqual(1, inventory.GetOrderedItems().Count);
        }

        [Test]
        public void RegisterItemDefinition_SplitsLoadedStackThatExceedsMaxStackSize()
        {
            InventoryModel inventory = new(slotCapacity: 3);
            inventory.LoadInventorySnapshot(new[] { new InventoryItemSnapshot("meat", 12) });
            ItemDefinition meat = CreateItem("meat", maxStackSize: 10);

            inventory.RegisterItemDefinition(meat);

            IReadOnlyList<InventoryItemStack> items = inventory.GetOrderedItems();
            Assert.AreEqual(12, inventory.GetAmount("meat"));
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(10, items[0].Amount);
            Assert.AreEqual(2, items[1].Amount);
        }

        [Test]
        public void TryUseItem_ConsumesOneItemWhenEffectSucceeds()
        {
            InventoryModel inventory = new();
            TestItemUseEffect effect = CreateUseEffect(wasUsed: true, message: "Usado.");
            ItemDefinition potion = CreateItem("potion", maxStackSize: 5, useEffect: effect, consumeOnUse: true);
            inventory.TryAddItem(potion, 2, out _);

            bool wasUsed = inventory.TryUseItem("potion", new ItemUseContext(null), out ItemUseResult result, out InventoryChange? consumedChange);

            Assert.IsTrue(wasUsed);
            Assert.IsTrue(result.WasUsed);
            Assert.AreEqual("Usado.", result.FeedbackMessage);
            Assert.IsTrue(consumedChange.HasValue);
            Assert.AreEqual(2, consumedChange.Value.PreviousAmount);
            Assert.AreEqual(1, consumedChange.Value.CurrentAmount);
            Assert.AreEqual(1, inventory.GetAmount("potion"));
        }

        [Test]
        public void TryUseItem_DoesNotConsumeWhenEffectFails()
        {
            InventoryModel inventory = new();
            TestItemUseEffect effect = CreateUseEffect(wasUsed: false, message: "Falhou.");
            ItemDefinition potion = CreateItem("potion", useEffect: effect, consumeOnUse: true);
            inventory.TryAddItem(potion, 1, out _);

            bool wasUsed = inventory.TryUseItem("potion", new ItemUseContext(null), out ItemUseResult result, out InventoryChange? consumedChange);

            Assert.IsFalse(wasUsed);
            Assert.IsFalse(result.WasUsed);
            Assert.AreEqual("Falhou.", result.FeedbackMessage);
            Assert.IsFalse(consumedChange.HasValue);
            Assert.AreEqual(1, inventory.GetAmount("potion"));
        }

        private ItemDefinition CreateItem(
            string itemId,
            bool isStackable = true,
            int maxStackSize = 99,
            ItemUseEffect useEffect = null,
            bool consumeOnUse = false)
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            createdAssets.Add(item);
            SetField(item, "itemId", itemId);
            SetField(item, "displayName", itemId);
            SetField(item, "isStackable", isStackable);
            SetField(item, "maxStackSize", maxStackSize);
            SetField(item, "useEffect", useEffect);
            SetField(item, "consumeOnUse", consumeOnUse);
            return item;
        }

        private TestItemUseEffect CreateUseEffect(bool wasUsed, string message)
        {
            TestItemUseEffect effect = ScriptableObject.CreateInstance<TestItemUseEffect>();
            effect.Result = new ItemUseResult(wasUsed, message);
            createdAssets.Add(effect);
            return effect;
        }

        private static void SetField<T>(ItemDefinition item, string fieldName, T value)
        {
            FieldInfo field = typeof(ItemDefinition).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' was not found.");
            field.SetValue(item, value);
        }

        private sealed class TestItemUseEffect : ItemUseEffect
        {
            public ItemUseResult Result { get; set; }

            protected override ItemUseResult OnUse(ItemDefinition item, ItemUseContext context)
            {
                return Result;
            }
        }
    }
}
