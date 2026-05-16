using NUnit.Framework;
using RPGProject.Gameplay;
using System.Reflection;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class LootSourceAvailabilityTests
    {
        [Test]
        public void HasAvailableLoot_WithoutLoot_ReturnsFalse()
        {
            Assert.IsFalse(LootSourceAvailability.HasAvailableLoot((ItemStackDefinition[])null));
        }

        [Test]
        public void HasAvailableLoot_WithOnlyInvalidStacks_ReturnsFalse()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();

            try
            {
                ItemStackDefinition[] loot =
                {
                    null,
                    new(item, 1)
                };

                Assert.IsFalse(LootSourceAvailability.HasAvailableLoot(loot));
            }
            finally
            {
                Object.DestroyImmediate(item);
            }
        }

        [Test]
        public void HasAvailableLoot_WithValidStack_ReturnsTrue()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();

            try
            {
                SetItemId(item, "valid-stack-item");
                ItemStackDefinition[] loot = { new(item, 1) };

                Assert.IsTrue(LootSourceAvailability.HasAvailableLoot(loot));
            }
            finally
            {
                Object.DestroyImmediate(item);
            }
        }

        private static void SetItemId(ItemDefinition item, string itemId)
        {
            FieldInfo field = typeof(ItemDefinition).GetField("itemId", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(item, itemId);
        }
    }
}
