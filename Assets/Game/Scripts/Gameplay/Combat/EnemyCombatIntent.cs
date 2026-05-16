namespace RPGProject.Gameplay
{
    public enum EnemyCombatIntentType
    {
        Idle,
        Hold,
        Chase,
        Attack,
        Flee
    }

    public readonly struct EnemyCombatIntent
    {
        public EnemyCombatIntent(EnemyCombatIntentType intentType)
        {
            IntentType = intentType;
        }

        public EnemyCombatIntentType IntentType { get; }

        public static EnemyCombatIntent Idle { get; } = new(EnemyCombatIntentType.Idle);
        public static EnemyCombatIntent Hold { get; } = new(EnemyCombatIntentType.Hold);
        public static EnemyCombatIntent Chase { get; } = new(EnemyCombatIntentType.Chase);
        public static EnemyCombatIntent Attack { get; } = new(EnemyCombatIntentType.Attack);
        public static EnemyCombatIntent Flee { get; } = new(EnemyCombatIntentType.Flee);
    }
}
