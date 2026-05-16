namespace RPGProject.Gameplay
{
    public sealed class DeadOrInvalidEnemyBehaviorStrategy : IEnemyCombatBehaviorStrategy
    {
        public bool CanResolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return context.SelfDead || !context.HasTarget || context.TargetDead;
        }

        public EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return EnemyCombatIntent.Idle;
        }
    }

    public sealed class DisengagedEnemyBehaviorStrategy : IEnemyCombatBehaviorStrategy
    {
        private readonly EnemyEngagementResolver engagementResolver = new();

        public bool CanResolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return !engagementResolver.CanEngage(settings, context);
        }

        public EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return EnemyCombatIntent.Idle;
        }
    }

    public sealed class FleeEnemyBehaviorStrategy : IEnemyCombatBehaviorStrategy
    {
        public bool CanResolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return settings != null &&
                (settings.ShouldFlee(context.NormalizedHealth) ||
                 settings.ShouldFleeWhenDamaged && context.WasDamagedByTarget ||
                 settings.ShouldKeepDistance && context.DistanceToTarget < settings.PreferredDistance);
        }

        public EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return EnemyCombatIntent.Flee;
        }
    }

    public sealed class AttackEnemyBehaviorStrategy : IEnemyCombatBehaviorStrategy
    {
        public bool CanResolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return context.TargetInAttackRange;
        }

        public EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return EnemyCombatIntent.Attack;
        }
    }

    public sealed class MovementEnemyBehaviorStrategy : IEnemyCombatBehaviorStrategy
    {
        public bool CanResolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return true;
        }

        public EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            if (settings == null || settings.ShouldChase || settings.ShouldKeepDistance)
            {
                return EnemyCombatIntent.Chase;
            }

            return EnemyCombatIntent.Hold;
        }
    }
}
