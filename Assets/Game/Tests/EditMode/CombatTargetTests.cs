using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class CombatTargetTests
    {
        private GameObject testObject;
        private SpriteRenderer spriteRenderer;
        private HealthComponent health;
        private CombatTarget target;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("CombatTargetTest");
            spriteRenderer = testObject.AddComponent<SpriteRenderer>();
            health = testObject.AddComponent<HealthComponent>();
            target = testObject.AddComponent<CombatTarget>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void SetSelected_TracksSelectionState()
        {
            target.SetSelected(true);

            Assert.IsTrue(target.IsSelected);
            Assert.IsTrue(testObject.transform.Find("SelectionFrame").gameObject.activeSelf);
            Assert.AreEqual(Color.white, spriteRenderer.color);

            target.SetSelected(false);

            Assert.IsFalse(target.IsSelected);
            Assert.IsFalse(testObject.transform.Find("SelectionFrame").gameObject.activeSelf);
            Assert.AreEqual(Color.white, spriteRenderer.color);
        }

        [Test]
        public void HealthDeath_DeselectsAndPublishesDefeated()
        {
            int defeatedEvents = 0;
            HealthChange defeatedChange = default;
            target.Defeated += (_, change) =>
            {
                defeatedEvents++;
                defeatedChange = change;
            };
            target.SetSelected(true);

            health.ApplyDamage(100);
            InvokeTargetDeath(new HealthChange(HealthChangeType.Damage, 100, 0, 100, 100, null));

            Assert.IsFalse(target.IsSelected);
            Assert.IsFalse(target.CanBeAttacked);
            Assert.AreEqual(1, defeatedEvents);
            Assert.IsTrue(defeatedChange.WasFatal);
        }

        private void InvokeTargetDeath(HealthChange change)
        {
            MethodInfo onDied = typeof(CombatTarget).GetMethod("OnDied", BindingFlags.Instance | BindingFlags.NonPublic);
            onDied.Invoke(target, new object[] { change });
        }
    }
}
