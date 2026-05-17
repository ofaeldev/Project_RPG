using NUnit.Framework;
using RPGProject.Systems;

namespace RPGProject.Tests
{
    public sealed class AutoAttackEngagementResolverTests
    {
        private readonly AutoAttackEngagementResolver resolver = new();

        [Test]
        public void Resolve_NoCombatActor_DoesNothing()
        {
            AutoAttackEngagementAction action = resolver.Resolve(CreateContext(hasCombatActor: false));

            Assert.AreEqual(AutoAttackEngagementAction.None, action);
        }

        [Test]
        public void Resolve_NoTarget_DoesNothing()
        {
            AutoAttackEngagementAction action = resolver.Resolve(CreateContext(hasTarget: false));

            Assert.AreEqual(AutoAttackEngagementAction.None, action);
        }

        [Test]
        public void Resolve_InputBlocked_DoesNothing()
        {
            AutoAttackEngagementAction action = resolver.Resolve(CreateContext(inputBlocked: true));

            Assert.AreEqual(AutoAttackEngagementAction.None, action);
        }

        [Test]
        public void Resolve_InvalidTarget_StopsAttacking()
        {
            AutoAttackEngagementAction action = resolver.Resolve(CreateContext(hasValidTarget: false));

            Assert.AreEqual(AutoAttackEngagementAction.StopAttacking, action);
        }

        [Test]
        public void Resolve_TargetInRange_Attacks()
        {
            AutoAttackEngagementAction action = resolver.Resolve(CreateContext(targetInRange: true));

            Assert.AreEqual(AutoAttackEngagementAction.Attack, action);
        }

        [Test]
        public void Resolve_TargetOutOfRangeWithFollow_MovesToTarget()
        {
            AutoAttackEngagementAction action = resolver.Resolve(CreateContext(followTarget: true));

            Assert.AreEqual(AutoAttackEngagementAction.MoveToTarget, action);
        }

        [Test]
        public void Resolve_TargetOutOfRangeWithoutFollow_AttacksOutOfRange()
        {
            AutoAttackEngagementAction action = resolver.Resolve(CreateContext(followTarget: false));

            Assert.AreEqual(AutoAttackEngagementAction.AttackOutOfRange, action);
        }

        private static AutoAttackEngagementContext CreateContext(
            bool hasCombatActor = true,
            bool hasTarget = true,
            bool inputBlocked = false,
            bool hasValidTarget = true,
            bool targetInRange = false,
            bool followTarget = true)
        {
            return new AutoAttackEngagementContext(
                hasCombatActor,
                hasTarget,
                inputBlocked,
                hasValidTarget,
                targetInRange,
                followTarget);
        }
    }
}
