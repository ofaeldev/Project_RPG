using System.Reflection;
using NUnit.Framework;
using RPGProject.Character;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class EnemyCombatControllerTests
    {
        private GameObject player;
        private GameObject enemy;
        private HealthComponent playerHealth;
        private HealthComponent enemyHealth;
        private CombatActor playerActor;
        private CombatActor enemyActor;
        private CharacterMotor2D enemyMotor;
        private EnemyCombatController controller;
        private EnemyCombatBehaviorSettings behavior;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;
            playerHealth = player.AddComponent<HealthComponent>();
            InvokeLifecycle(playerHealth, "Awake");
            playerActor = player.AddComponent<CombatActor>();

            enemy = new GameObject("Enemy");
            enemy.transform.position = Vector3.right * 3f;
            enemy.AddComponent<Rigidbody2D>();
            enemyMotor = enemy.AddComponent<CharacterMotor2D>();
            enemyHealth = enemy.AddComponent<HealthComponent>();
            InvokeLifecycle(enemyHealth, "Awake");
            enemyActor = enemy.AddComponent<CombatActor>();
            controller = enemy.AddComponent<EnemyCombatController>();

            behavior = ScriptableObject.CreateInstance<EnemyCombatBehaviorSettings>();
            SetPrivateField(controller, "behaviorSettings", behavior);
            InvokeLifecycle(controller, "Awake");
            InvokeLifecycle(controller, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(behavior);
        }

        [Test]
        public void RetaliateWhenTargeted_EngagesPlayerWhenSelected()
        {
            SetPrivateField(behavior, "engagementPolicy", EnemyEngagementPolicy.RetaliateWhenTargeted);

            playerActor.SetTarget(enemyActor);

            Assert.AreSame(playerHealth, controller.CurrentTarget);
        }

        [Test]
        public void Passive_DoesNotEngageWhenSelectedOrDamaged()
        {
            SetPrivateField(behavior, "engagementPolicy", EnemyEngagementPolicy.Passive);

            playerActor.SetTarget(enemyActor);
            enemyHealth.ApplyDamage(1, playerActor);

            Assert.IsNull(controller.CurrentTarget);
        }

        [Test]
        public void HoldPosition_TargetOutOfRange_ReportsAlertState()
        {
            SetPrivateField(behavior, "engagementPolicy", EnemyEngagementPolicy.RetaliateWhenTargeted);
            SetPrivateField(behavior, "movementPolicy", EnemyMovementPolicy.HoldPosition);
            SetPrivateField(enemyActor, "fallbackAttackRange", 1f);

            controller.SetTarget(playerHealth);
            InvokeUpdate(controller);

            Assert.AreEqual(EnemyCombatState.Alert, controller.CurrentState);
            Assert.IsFalse(enemyActor.HasTarget);
        }

        [Test]
        public void FleeAtLowHealth_UsesConfiguredFleeSpeedMultiplier()
        {
            SetPrivateField(behavior, "engagementPolicy", EnemyEngagementPolicy.RetaliateWhenTargeted);
            SetPrivateField(behavior, "movementPolicy", EnemyMovementPolicy.FleeAtLowHealth);
            SetPrivateField(behavior, "lowHealthThreshold", 0.9f);
            SetPrivateField(behavior, "fleeSpeedMultiplier", 0.6f);

            controller.SetTarget(playerHealth);
            enemyHealth.ApplyDamage(20);
            InvokeUpdate(controller);

            Assert.AreEqual(EnemyCombatState.Fleeing, controller.CurrentState);
            Assert.IsTrue(enemyMotor.HasMovementTarget);
            Assert.AreEqual(0.6f, enemyMotor.MovementSpeedMultiplier);
        }

        private static void InvokeUpdate(EnemyCombatController target)
        {
            InvokeLifecycle(target, "Update");
        }

        private static void InvokeLifecycle(EnemyCombatController target, string methodName)
        {
            MethodInfo method = typeof(EnemyCombatController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            method.Invoke(target, null);
        }

        private static void InvokeLifecycle(HealthComponent target, string methodName)
        {
            MethodInfo method = typeof(HealthComponent).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            method.Invoke(target, null);
        }

        private static void SetPrivateField(object targetObject, string fieldName, object value)
        {
            FieldInfo field = targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {targetObject.GetType().Name}.");
            field.SetValue(targetObject, value);
        }
    }
}
