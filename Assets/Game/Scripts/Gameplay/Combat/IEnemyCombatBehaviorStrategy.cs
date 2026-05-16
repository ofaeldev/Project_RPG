namespace RPGProject.Gameplay
{
    public interface IEnemyCombatBehaviorStrategy
    {
        bool CanResolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context);
        EnemyCombatIntent Resolve(EnemyCombatBehaviorSettings settings, EnemyCombatContext context);
    }
}
