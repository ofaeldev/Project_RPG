using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class InventoryRequirementServiceTests
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
        public void TrySatisfy_ReturnsFalseWithoutRequirement()
        {
            InventoryRequirementService service = new();
            InventoryRequirement requirement = new();

            Assert.IsFalse(service.TrySatisfy(requirement, new TestInventoryService()));
        }

        [Test]
        public void TrySatisfy_ReturnsFalseWhenInventoryDoesNotHaveItem()
        {
            InventoryRequirementService service = new();
            InventoryRequirement requirement = CreateRequirement("key", amount: 1, consumeOnSuccess: false);

            Assert.IsFalse(service.TrySatisfy(requirement, new TestInventoryService()));
        }

        [Test]
        public void TrySatisfy_ConsumesRequiredItemWhenConfigured()
        {
            InventoryRequirementService service = new();
            InventoryRequirement requirement = CreateRequirement("key", amount: 2, consumeOnSuccess: true);
            TestInventoryService inventory = new();
            inventory.SetAmount("key", 2);

            Assert.IsTrue(service.TrySatisfy(requirement, inventory));
            Assert.AreEqual(0, inventory.GetAmount("key"));
        }

        [Test]
        public void TrySatisfy_DoesNotConsumeWhenRequirementOnlyChecksOwnership()
        {
            InventoryRequirementService service = new();
            InventoryRequirement requirement = CreateRequirement("key", amount: 1, consumeOnSuccess: false);
            TestInventoryService inventory = new();
            inventory.SetAmount("key", 1);

            Assert.IsTrue(service.TrySatisfy(requirement, inventory));
            Assert.AreEqual(1, inventory.GetAmount("key"));
        }

        private InventoryRequirement CreateRequirement(string itemId, int amount, bool consumeOnSuccess)
        {
            InventoryRequirement requirement = new();
            SetField(requirement, "itemId", itemId);
            SetField(requirement, "amount", amount);
            SetField(requirement, "consumeOnSuccess", consumeOnSuccess);
            return requirement;
        }

        private static void SetField<T>(InventoryRequirement requirement, string fieldName, T value)
        {
            FieldInfo field = typeof(InventoryRequirement).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' was not found.");
            field.SetValue(requirement, value);
        }

        private sealed class TestInventoryService : IInventoryService
        {
            private readonly Dictionary<string, int> amounts = new();

            public IReadOnlyCollection<InventoryItemStack> Items => System.Array.Empty<InventoryItemStack>();
            public int FreeSlots => 0;

            public void SetAmount(string itemId, int amount)
            {
                amounts[itemId] = amount;
            }

            public int GetAmount(string itemId)
            {
                return amounts.TryGetValue(itemId, out int amount) ? amount : 0;
            }

            public bool AddItem(ItemDefinition item, int amount = 1)
            {
                return false;
            }

            public bool CanAddItem(ItemDefinition item, int amount = 1)
            {
                return false;
            }

            public bool HasItem(string itemId, int amount = 1)
            {
                return GetAmount(itemId) >= amount;
            }

            public bool RemoveItem(string itemId, int amount = 1)
            {
                if (!HasItem(itemId, amount))
                {
                    return false;
                }

                amounts[itemId] = GetAmount(itemId) - amount;
                return true;
            }

            public bool TryUseSlot(int slotIndex, ItemUseContext context, out ItemUseResult result)
            {
                result = new ItemUseResult(false, string.Empty);
                return false;
            }
        }
    }
}
