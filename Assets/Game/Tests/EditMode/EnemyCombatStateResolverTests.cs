using NUnit.Framework;
using RPGProject.Gameplay;

namespace RPGProject.Tests
{
    public sealed class EnemyCombatStateResolverTests
    {
        [TestCase(EnemyCombatIntentType.Idle, false, EnemyCombatState.Idle)]
        [TestCase(EnemyCombatIntentType.Hold, false, EnemyCombatState.Alert)]
        [TestCase(EnemyCombatIntentType.Chase, false, EnemyCombatState.Chasing)]
        [TestCase(EnemyCombatIntentType.Attack, false, EnemyCombatState.Attacking)]
        [TestCase(EnemyCombatIntentType.Flee, false, EnemyCombatState.Fleeing)]
        [TestCase(EnemyCombatIntentType.Attack, true, EnemyCombatState.Dead)]
        public void Resolve_MapsIntentToState(EnemyCombatIntentType intentType, bool isDead, EnemyCombatState expected)
        {
            EnemyCombatStateResolver resolver = new();

            EnemyCombatState state = resolver.Resolve(new EnemyCombatIntent(intentType), isDead);

            Assert.AreEqual(expected, state);
        }
    }
}
