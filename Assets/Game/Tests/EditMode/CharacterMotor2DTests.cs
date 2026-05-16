using NUnit.Framework;
using RPGProject.Character;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class CharacterMotor2DTests
    {
        private GameObject actor;
        private CharacterMotor2D motor;

        [SetUp]
        public void SetUp()
        {
            actor = new GameObject("Actor");
            actor.AddComponent<Rigidbody2D>();
            motor = actor.AddComponent<CharacterMotor2D>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void SetMovementTarget_StoresStopDistanceAndSpeedMultiplier()
        {
            motor.SetMovementTarget(Vector2.right, 0.25f, 0.75f);

            Assert.IsTrue(motor.HasMovementTarget);
            Assert.AreEqual(0.25f, motor.TargetStopDistance);
            Assert.AreEqual(0.75f, motor.MovementSpeedMultiplier);
        }

        [Test]
        public void Stop_ResetsSpeedMultiplier()
        {
            motor.SetMovementTarget(Vector2.right, 0.25f, 0.75f);

            motor.Stop();

            Assert.IsFalse(motor.HasMovementTarget);
            Assert.AreEqual(1f, motor.MovementSpeedMultiplier);
        }
    }
}
