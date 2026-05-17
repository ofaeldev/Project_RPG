using System;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public readonly struct CombatAttackEvent
    {
        public CombatAttackEvent(CombatActor attacker, HealthComponent target, DamageResult damage)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
        }

        public CombatActor Attacker { get; }
        public HealthComponent Target { get; }
        public DamageResult Damage { get; }
    }

    public readonly struct EnemyStateChangedEvent
    {
        public EnemyStateChangedEvent(EnemyCombatController enemy, EnemyCombatState previousState, EnemyCombatState currentState)
        {
            Enemy = enemy;
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public EnemyCombatController Enemy { get; }
        public EnemyCombatState PreviousState { get; }
        public EnemyCombatState CurrentState { get; }
    }

    public static partial class GameplayEvents
    {
        public static event Action<CombatAttackEvent> CombatAttackResolved;
        public static event Action<EnemyStateChangedEvent> EnemyStateChanged;

        public static void PublishCombatAttackResolved(CombatActor attacker, HealthComponent target, DamageResult damage)
        {
            CombatAttackResolved?.Invoke(new CombatAttackEvent(attacker, target, damage));
        }

        public static void PublishEnemyStateChanged(EnemyCombatController enemy, EnemyCombatState previousState, EnemyCombatState currentState)
        {
            EnemyStateChanged?.Invoke(new EnemyStateChangedEvent(enemy, previousState, currentState));
        }
    }
}
