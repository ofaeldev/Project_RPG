using NUnit.Framework;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class HealthComponentTests
    {
        private GameObject testObject;
        private HealthComponent health;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("HealthComponentTest");
            health = testObject.AddComponent<HealthComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void ApplyDamage_ReducesCurrentHealth()
        {
            bool changed = health.ApplyDamage(25);

            Assert.IsTrue(changed);
            Assert.AreEqual(75, health.CurrentHealth);
            Assert.AreEqual(100, health.MaximumHealth);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void ApplyDamage_ClampsAtZeroAndPublishesDeathOnce()
        {
            int deathEvents = 0;
            HealthChange lastDeathChange = default;
            health.Died += change =>
            {
                deathEvents++;
                lastDeathChange = change;
            };

            Assert.IsTrue(health.ApplyDamage(150));
            Assert.IsFalse(health.ApplyDamage(10));

            Assert.AreEqual(0, health.CurrentHealth);
            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(1, deathEvents);
            Assert.IsTrue(lastDeathChange.WasFatal);
            Assert.AreEqual(HealthChangeType.Damage, lastDeathChange.ChangeType);
        }

        [Test]
        public void Heal_RestoresHealthWithoutExceedingMaximum()
        {
            health.ApplyDamage(80);

            bool changed = health.Heal(200);

            Assert.IsTrue(changed);
            Assert.AreEqual(100, health.CurrentHealth);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void Heal_DoesNotReviveDeadTargets()
        {
            health.ApplyDamage(100);

            bool changed = health.Heal(25);

            Assert.IsFalse(changed);
            Assert.AreEqual(0, health.CurrentHealth);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void Revive_RestoresDeadTargetWithinMaximum()
        {
            int reviveEvents = 0;
            health.Revived += _ => reviveEvents++;
            health.ApplyDamage(100);

            bool changed = health.Revive(250);

            Assert.IsTrue(changed);
            Assert.AreEqual(100, health.CurrentHealth);
            Assert.IsFalse(health.IsDead);
            Assert.AreEqual(1, reviveEvents);
        }

        [Test]
        public void SetMaximumHealth_ClampsCurrentHealth()
        {
            bool changed = health.SetMaximumHealth(40);

            Assert.IsTrue(changed);
            Assert.AreEqual(40, health.MaximumHealth);
            Assert.AreEqual(40, health.CurrentHealth);
            Assert.AreEqual(1f, health.NormalizedHealth);
        }

        [Test]
        public void HealthChanged_ReportsPreviousAndCurrentHealth()
        {
            HealthChange receivedChange = default;
            health.HealthChanged += change => receivedChange = change;

            health.ApplyDamage(15);

            Assert.AreEqual(HealthChangeType.Damage, receivedChange.ChangeType);
            Assert.AreEqual(100, receivedChange.PreviousHealth);
            Assert.AreEqual(85, receivedChange.CurrentHealth);
            Assert.AreEqual(100, receivedChange.MaximumHealth);
            Assert.AreEqual(15, receivedChange.Amount);
        }
    }
}
