using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class CombatActorTests
    {
        private GameObject attacker;
        private GameObject defender;
        private CombatActor actor;
        private CombatActor target;
        private HealthComponent defenderHealth;
        private CombatAttackSettings attackSettings;
        private BasicDamageResolver resolver;

        [SetUp]
        public void SetUp()
        {
            attacker = new GameObject("Attacker");
            defender = new GameObject("Defender");
            attacker.transform.position = Vector3.zero;
            defender.transform.position = Vector3.right;

            attacker.AddComponent<HealthComponent>();
            actor = attacker.AddComponent<CombatActor>();
            SetStats(actor, attack: 4, defense: 0);

            defenderHealth = defender.AddComponent<HealthComponent>();
            target = defender.AddComponent<CombatActor>();
            SetStats(target, attack: 0, defense: 2);

            resolver = ScriptableObject.CreateInstance<BasicDamageResolver>();
            attackSettings = ScriptableObject.CreateInstance<CombatAttackSettings>();
            SetPrivateField(attackSettings, "baseDamage", 3);
            SetPrivateField(attackSettings, "attackRange", 1.5f);
            SetPrivateField(attackSettings, "attacksPerSecond", 1f);
            SetPrivateField(attackSettings, "damageResolver", resolver);
            SetPrivateField(actor, "attackSettings", attackSettings);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(defender);
            Object.DestroyImmediate(attackSettings);
            Object.DestroyImmediate(resolver);
        }

        [Test]
        public void SetTarget_SelectsAndClearsCombatTarget()
        {
            actor.SetTarget(target);

            Assert.AreSame(target, actor.CurrentTargetActor);
            Assert.AreSame(defenderHealth, actor.CurrentTargetHealth);
            Assert.IsTrue(target.IsSelected);

            actor.ClearTarget();

            Assert.IsNull(actor.CurrentTargetActor);
            Assert.IsNull(actor.CurrentTargetHealth);
            Assert.IsFalse(target.IsSelected);
        }

        [Test]
        public void TryAttackCurrentTarget_ResolvesDamageOncePerCooldown()
        {
            actor.SetTarget(target);

            Assert.IsTrue(actor.TryAttackCurrentTarget());
            Assert.AreEqual(95, defenderHealth.CurrentHealth);

            Assert.IsFalse(actor.TryAttackCurrentTarget());
            Assert.AreEqual(95, defenderHealth.CurrentHealth);
        }

        [Test]
        public void TryAttackCurrentTarget_OutOfRangePublishesEventAndDoesNotDamage()
        {
            defender.transform.position = Vector3.right * 3f;
            actor.SetTarget(target);
            int eventCount = 0;
            HealthComponent reportedTarget = null;
            actor.AttackOutOfRange += (_, targetHealth) =>
            {
                eventCount++;
                reportedTarget = targetHealth;
            };

            Assert.IsFalse(actor.TryAttackCurrentTarget());

            Assert.AreEqual(1, eventCount);
            Assert.AreSame(defenderHealth, reportedTarget);
            Assert.AreEqual(100, defenderHealth.CurrentHealth);
        }

        [Test]
        public void TryAttackCurrentTarget_SmallRangeDrift_StillDamages()
        {
            defender.transform.position = Vector3.right * 1.53f;
            actor.SetTarget(target);

            Assert.IsTrue(actor.TryAttackCurrentTarget());

            Assert.AreEqual(95, defenderHealth.CurrentHealth);
        }

        [Test]
        public void TargetDeath_ClearsCurrentTarget()
        {
            actor.SetTarget(target);

            defenderHealth.ApplyDamage(100);

            Assert.IsNull(actor.CurrentTargetHealth);
            Assert.IsNull(actor.CurrentTargetActor);
            Assert.IsFalse(target.IsSelected);
        }

        private static void SetStats(CombatActor targetObject, int attack, int defense)
        {
            SetPrivateField(targetObject, "fallbackAttack", attack);
            SetPrivateField(targetObject, "fallbackDefense", defense);
        }

        private static void SetPrivateField(object targetObject, string fieldName, object value)
        {
            FieldInfo field = targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {targetObject.GetType().Name}.");
            field.SetValue(targetObject, value);
        }
    }
}
