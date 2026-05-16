using System.Reflection;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class HitFlashPresenterTests
    {
        private GameObject target;
        private SpriteRenderer renderer;
        private HealthComponent health;
        private HitFlashPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            target = new GameObject("Target", typeof(SpriteRenderer));
            renderer = target.GetComponent<SpriteRenderer>();
            health = target.AddComponent<HealthComponent>();
            presenter = target.AddComponent<HitFlashPresenter>();

            InvokeLifecycle(health, "Awake");
            InvokeLifecycle(presenter, "Awake");
            InvokeLifecycle(presenter, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(target);
        }

        [Test]
        public void DamageFlash_RestoresColorFromDamageMoment()
        {
            Color returningColor = new(0.82f, 0.82f, 0.82f, 1f);
            renderer.color = returningColor;

            health.ApplyDamage(1);
            InvokeLifecycle(presenter, "RestoreColor");

            Assert.AreEqual(returningColor, renderer.color);
        }

        private static void InvokeLifecycle(object targetObject, string methodName)
        {
            MethodInfo method = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            method.Invoke(targetObject, null);
        }

    }
}
