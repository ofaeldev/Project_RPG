namespace RPGProject.Gameplay
{
    public sealed class EnemyCombatBehaviorResolver
    {
        private readonly IEnemyCombatBehaviorStrategy[] strategies;
        private readonly EnemyEngagementResolver engagementResolver = new();

        public EnemyCombatBehaviorResolver()
            : this(new IEnemyCombatBehaviorStrategy[]
            {
                new DeadOrInvalidEnemyBehaviorStrategy(),
                new DisengagedEnemyBehaviorStrategy(),
                new FleeEnemyBehaviorStrategy(),
                new AttackEnemyBehaviorStrategy(),
                new MovementEnemyBehaviorStrategy()
            })
        {
        }

        public EnemyCombatBehaviorResolver(IEnemyCombatBehaviorStrategy[] strategies)
        {
            this.strategies = strategies;
        }

        public EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            if (strategies == null || strategies.Length == 0)
            {
                return EnemyCombatIntent.Idle;
            }

            for (int i = 0; i < strategies.Length; i++)
            {
                IEnemyCombatBehaviorStrategy strategy = strategies[i];
                if (strategy != null && strategy.CanResolve(settings, context))
                {
                    return strategy.Resolve(settings, context);
                }
            }

            return EnemyCombatIntent.Idle;
        }

        public bool CanEngage(EnemyCombatBehaviorSettings settings, EnemyCombatContext context)
        {
            return engagementResolver.CanEngage(settings, context);
        }
    }
}
