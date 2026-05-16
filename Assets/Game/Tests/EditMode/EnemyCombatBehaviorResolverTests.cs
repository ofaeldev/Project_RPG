using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class EnemyCombatBehaviorResolverTests
    {
        private EnemyCombatBehaviorResolver resolver;
        private EnemyCombatBehaviorSettings settings;

        [SetUp]
        public void SetUp()
        {
            resolver = new EnemyCombatBehaviorResolver();
            settings = ScriptableObject.CreateInstance<EnemyCombatBehaviorSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void AggressiveOnSight_ChasesVisibleTargetOutOfRange()
        {
            Configure(EnemyEngagementPolicy.AggressiveOnSight, EnemyMovementPolicy.ChaseTarget);

            EnemyCombatIntent intent = resolver.Resolve(settings, CreateContext(targetInDetectionRange: true));

            Assert.AreEqual(EnemyCombatIntentType.Chase, intent.IntentType);
        }

        [Test]
        public void RetaliateWhenTargeted_IgnoresVisibleTargetUntilForced()
        {
            Configure(EnemyEngagementPolicy.RetaliateWhenTargeted, EnemyMovementPolicy.ChaseTarget);

            EnemyCombatIntent ignored = resolver.Resolve(settings, CreateContext(targetInDetectionRange: true));
            EnemyCombatIntent engaged = resolver.Resolve(settings, CreateContext(hasForcedTarget: true));

            Assert.AreEqual(EnemyCombatIntentType.Idle, ignored.IntentType);
            Assert.AreEqual(EnemyCombatIntentType.Chase, engaged.IntentType);
        }

        [Test]
        public void RetaliateWhenDamaged_ChasesOnlyAfterDamage()
        {
            Configure(EnemyEngagementPolicy.RetaliateWhenDamaged, EnemyMovementPolicy.ChaseTarget);

            EnemyCombatIntent ignored = resolver.Resolve(settings, CreateContext(hasForcedTarget: true));
            EnemyCombatIntent engaged = resolver.Resolve(settings, CreateContext(wasDamagedByTarget: true));

            Assert.AreEqual(EnemyCombatIntentType.Idle, ignored.IntentType);
            Assert.AreEqual(EnemyCombatIntentType.Chase, engaged.IntentType);
        }

        [Test]
        public void HoldPosition_TargetOutOfRange_Holds()
        {
            Configure(EnemyEngagementPolicy.AggressiveOnSight, EnemyMovementPolicy.HoldPosition);

            EnemyCombatIntent intent = resolver.Resolve(settings, CreateContext(targetInDetectionRange: true));

            Assert.AreEqual(EnemyCombatIntentType.Hold, intent.IntentType);
        }

        [Test]
        public void FleeWhenDamaged_FleesAfterDamage()
        {
            Configure(EnemyEngagementPolicy.RetaliateWhenDamaged, EnemyMovementPolicy.FleeWhenDamaged);

            EnemyCombatIntent intent = resolver.Resolve(settings, CreateContext(wasDamagedByTarget: true));

            Assert.AreEqual(EnemyCombatIntentType.Flee, intent.IntentType);
        }

        [Test]
        public void FleeAtLowHealth_FleesWhenThresholdReached()
        {
            Configure(EnemyEngagementPolicy.AggressiveOnSight, EnemyMovementPolicy.FleeAtLowHealth);
            SetPrivateField(settings, "lowHealthThreshold", 0.5f);

            EnemyCombatIntent intent = resolver.Resolve(settings, CreateContext(targetInDetectionRange: true, normalizedHealth: 0.4f));

            Assert.AreEqual(EnemyCombatIntentType.Flee, intent.IntentType);
        }

        [Test]
        public void TargetInAttackRange_Attacks()
        {
            Configure(EnemyEngagementPolicy.AggressiveOnSight, EnemyMovementPolicy.ChaseTarget);

            EnemyCombatIntent intent = resolver.Resolve(settings, CreateContext(targetInDetectionRange: true, targetInAttackRange: true));

            Assert.AreEqual(EnemyCombatIntentType.Attack, intent.IntentType);
        }

        [Test]
        public void KeepDistance_TooClose_Flees()
        {
            Configure(EnemyEngagementPolicy.AggressiveOnSight, EnemyMovementPolicy.KeepDistance);
            SetPrivateField(settings, "preferredDistance", 2.5f);

            EnemyCombatIntent intent = resolver.Resolve(settings, CreateContext(
                targetInDetectionRange: true,
                targetInAttackRange: true,
                targetPosition: Vector2.right));

            Assert.AreEqual(EnemyCombatIntentType.Flee, intent.IntentType);
        }

        private void Configure(EnemyEngagementPolicy engagementPolicy, EnemyMovementPolicy movementPolicy)
        {
            SetPrivateField(settings, "engagementPolicy", engagementPolicy);
            SetPrivateField(settings, "movementPolicy", movementPolicy);
        }

        private static EnemyCombatContext CreateContext(
            bool selfDead = false,
            bool hasTarget = true,
            bool targetDead = false,
            bool targetInAttackRange = false,
            bool targetInDetectionRange = false,
            bool hasForcedTarget = false,
            bool wasDamagedByTarget = false,
            float normalizedHealth = 1f,
            Vector2? targetPosition = null)
        {
            return new EnemyCombatContext(
                selfDead,
                hasTarget,
                targetDead,
                targetInAttackRange,
                targetInDetectionRange,
                hasForcedTarget,
                wasDamagedByTarget,
                normalizedHealth,
                Vector2.zero,
                targetPosition ?? Vector2.right);
        }

        private static void SetPrivateField(object targetObject, string fieldName, object value)
        {
            FieldInfo field = targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {targetObject.GetType().Name}.");
            field.SetValue(targetObject, value);
        }
    }
}
