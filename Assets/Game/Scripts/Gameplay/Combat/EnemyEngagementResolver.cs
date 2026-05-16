namespace RPGProject.Gameplay
{
    public sealed class EnemyEngagementResolver
    {
        public bool CanEngage(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            if (!context.HasTarget || context.TargetDead)
            {
                return false;
            }

            if (settings == null)
            {
                return context.TargetInDetectionRange;
            }

            return settings.EngagementPolicy switch
            {
                EnemyEngagementPolicy.AggressiveOnSight => context.TargetInDetectionRange,
                EnemyEngagementPolicy.RetaliateWhenTargeted => context.HasForcedTarget,
                EnemyEngagementPolicy.RetaliateWhenDamaged => context.WasDamagedByTarget,
                EnemyEngagementPolicy.Passive => false,
                _ => false
            };
        }
    }
}
