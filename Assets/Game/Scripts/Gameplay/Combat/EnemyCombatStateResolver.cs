namespace RPGProject.Gameplay
{
    public sealed class EnemyCombatStateResolver
    {
        public EnemyCombatState Resolve(EnemyCombatIntent intent, bool isDead)
        {
            if (isDead)
            {
                return EnemyCombatState.Dead;
            }

            return intent.IntentType switch
            {
                EnemyCombatIntentType.Flee => EnemyCombatState.Fleeing,
                EnemyCombatIntentType.Chase => EnemyCombatState.Chasing,
                EnemyCombatIntentType.Attack => EnemyCombatState.Attacking,
                EnemyCombatIntentType.Hold => EnemyCombatState.Alert,
                _ => EnemyCombatState.Idle
            };
        }
    }
}
