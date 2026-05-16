using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class CombatWorldUIControllerTests
    {
        private GameObject uiObject;
        private GameObject player;
        private CombatWorldUIController controller;
        private HealthComponent playerHealth;

        [SetUp]
        public void SetUp()
        {
            uiObject = new GameObject("CombatWorldUI");
            controller = uiObject.AddComponent<CombatWorldUIController>();

            player = new GameObject("Player");
            player.tag = "Player";
            playerHealth = player.AddComponent<HealthComponent>();
            InvokeLifecycle(playerHealth, "Awake");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(uiObject);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void PlayerDamage_WhenPlayerHealthBarHidden_DoesNotThrow()
        {
            InvokeLifecycle(controller, "OnEnable");

            Assert.DoesNotThrow(() => playerHealth.ApplyDamage(1));
        }

        private static void InvokeLifecycle(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            method.Invoke(target, null);
        }
    }
}
