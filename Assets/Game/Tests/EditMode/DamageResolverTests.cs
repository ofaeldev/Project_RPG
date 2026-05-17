using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class DamageResolverTests
    {
        private GameObject attacker;
        private GameObject defender;
        private BasicDamageResolver resolver;
        private CombatAttackSettings attackSettings;

        [SetUp]
        public void SetUp()
        {
            attacker = new GameObject("Attacker");
            defender = new GameObject("Defender");
            defender.AddComponent<HealthComponent>();
            defender.AddComponent<CombatTarget>();

            resolver = ScriptableObject.CreateInstance<BasicDamageResolver>();
            attackSettings = ScriptableObject.CreateInstance<CombatAttackSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(defender);
            Object.DestroyImmediate(resolver);
            Object.DestroyImmediate(attackSettings);
        }

        [Test]
        public void ResolveDamage_CombinesBaseDamageAttackAndDefense()
        {
            AddStats(attacker, attack: 4, defense: 0);
            AddStats(defender, attack: 0, defense: 2);
            SetPrivateField(attackSettings, "baseDamage", 3);

            DamageResult result = resolver.ResolveDamage(new DamageContext(attacker, defender.GetComponent<CombatTarget>(), attackSettings));

            Assert.AreEqual(5, result.Amount);
        }

        [Test]
        public void ResolveDamage_DoesNotReturnNegativeDamage()
        {
            AddStats(attacker, attack: 1, defense: 0);
            AddStats(defender, attack: 0, defense: 99);
            SetPrivateField(attackSettings, "baseDamage", 3);

            DamageResult result = resolver.ResolveDamage(new DamageContext(attacker, defender.GetComponent<CombatTarget>(), attackSettings));

            Assert.AreEqual(0, result.Amount);
            Assert.IsFalse(result.HasDamage);
        }

        private static void AddStats(GameObject target, int attack, int defense)
        {
            CharacterCombatStats stats = target.AddComponent<CharacterCombatStats>();
            SetPrivateField(stats, "fallbackAttack", attack);
            SetPrivateField(stats, "fallbackDefense", defense);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
