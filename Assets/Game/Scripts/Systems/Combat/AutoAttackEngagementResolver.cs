namespace RPGProject.Systems
{
    public enum AutoAttackEngagementAction
    {
        None,
        StopAttacking,
        MoveToTarget,
        Attack,
        AttackOutOfRange
    }

    public readonly struct AutoAttackEngagementContext
    {
        public AutoAttackEngagementContext(
            bool hasCombatActor,
            bool hasTarget,
            bool inputBlocked,
            bool hasValidTarget,
            bool targetInRange,
            bool followTarget)
        {
            HasCombatActor = hasCombatActor;
            HasTarget = hasTarget;
            InputBlocked = inputBlocked;
            HasValidTarget = hasValidTarget;
            TargetInRange = targetInRange;
            FollowTarget = followTarget;
        }

        public bool HasCombatActor { get; }
        public bool HasTarget { get; }
        public bool InputBlocked { get; }
        public bool HasValidTarget { get; }
        public bool TargetInRange { get; }
        public bool FollowTarget { get; }
    }

    public sealed class AutoAttackEngagementResolver
    {
        public AutoAttackEngagementAction Resolve(AutoAttackEngagementContext context)
        {
            if (!context.HasCombatActor || !context.HasTarget || context.InputBlocked)
            {
                return AutoAttackEngagementAction.None;
            }

            if (!context.HasValidTarget)
            {
                return AutoAttackEngagementAction.StopAttacking;
            }

            if (context.TargetInRange)
            {
                return AutoAttackEngagementAction.Attack;
            }

            return context.FollowTarget
                ? AutoAttackEngagementAction.MoveToTarget
                : AutoAttackEngagementAction.AttackOutOfRange;
        }
    }
}
