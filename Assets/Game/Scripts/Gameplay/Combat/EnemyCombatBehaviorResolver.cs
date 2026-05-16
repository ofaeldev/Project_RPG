namespace RPGProject.Gameplay
{
    public sealed class EnemyCombatBehaviorResolver
    {
        public EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            if (context.SelfDead)
            {
                return EnemyCombatIntent.Idle;
            }

            if (!CanEngage(settings, context))
            {
                return EnemyCombatIntent.Idle;
            }

            if (ShouldFlee(settings, context))
            {
                return EnemyCombatIntent.Flee;
            }

            if (settings != null && settings.ShouldKeepDistance && context.DistanceToTarget < settings.PreferredDistance)
            {
                return EnemyCombatIntent.Flee;
            }

            if (context.TargetInAttackRange)
            {
                return EnemyCombatIntent.Attack;
            }

            if (settings == null || settings.ShouldChase)
            {
                return EnemyCombatIntent.Chase;
            }

            if (settings.ShouldKeepDistance)
            {
                return EnemyCombatIntent.Chase;
            }

            return EnemyCombatIntent.Hold;
        }

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

        private static bool ShouldFlee(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            if (settings == null)
            {
                return false;
            }

            return settings.ShouldFlee(context.NormalizedHealth)
                || (settings.ShouldFleeWhenDamaged && context.WasDamagedByTarget);
        }
    }
}
