using UnityEngine;

namespace RPGProject.Gameplay
{
    public enum EnemyAttackMode
    {
        Melee,
        Ranged
    }

    public enum EnemyMovementPolicy
    {
        HoldPosition,
        ChaseTarget,
        FleeWhenDamaged,
        FleeAtLowHealth,
        KeepDistance
    }

    public enum EnemyEngagementPolicy
    {
        AggressiveOnSight,
        RetaliateWhenTargeted,
        RetaliateWhenDamaged,
        Passive
    }

    [CreateAssetMenu(
        fileName = "EnemyCombatBehaviorSettings",
        menuName = "RPG Project/Combat/Enemy Behavior Settings")]
    public sealed class EnemyCombatBehaviorSettings : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        [Tooltip("How this enemy deals damage once engaged. Melee wants to stay close; ranged can later hold distance or use projectile logic.")]
        private EnemyAttackMode attackMode = EnemyAttackMode.Melee;

        [SerializeField]
        [Tooltip("What this enemy does after it has a target: stand still, chase into attack range, or flee when hurt.")]
        private EnemyMovementPolicy movementPolicy = EnemyMovementPolicy.ChaseTarget;

        [Header("Targeting")]
        [SerializeField]
        [Tooltip("Decides when this enemy starts combat. Aggressive enemies acquire visible targets; retaliating enemies engage when selected or damaged.")]
        private EnemyEngagementPolicy engagementPolicy = EnemyEngagementPolicy.AggressiveOnSight;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Maximum distance for aggressive sight checks. Retaliating enemies still use forced targets from player attacks or damage events.")]
        private float detectionRange = 4f;

        [Header("Movement")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("Preferred spacing for KeepDistance enemies, useful for future ranged or caster archetypes.")]
        private float preferredDistance = 2.5f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Health percentage where FleeAtLowHealth starts.")]
        private float lowHealthThreshold = 0.25f;

        [SerializeField]
        [Min(0.1f)]
        [Tooltip("How far the enemy tries to move away when fleeing.")]
        private float fleeDistance = 3f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Movement speed multiplier while fleeing. Values below 1 keep fleeing enemies catchable unless their base speed is high.")]
        private float fleeSpeedMultiplier = 0.85f;

        public EnemyAttackMode AttackMode => attackMode;
        public EnemyMovementPolicy MovementPolicy => movementPolicy;
        public EnemyEngagementPolicy EngagementPolicy => engagementPolicy;
        public float DetectionRange => detectionRange;
        public float PreferredDistance => preferredDistance;
        public float LowHealthThreshold => lowHealthThreshold;
        public float FleeDistance => fleeDistance;
        public float FleeSpeedMultiplier => fleeSpeedMultiplier;

        public bool ShouldAcquireOnSight => engagementPolicy == EnemyEngagementPolicy.AggressiveOnSight;
        public bool ShouldRetaliateWhenTargeted => engagementPolicy == EnemyEngagementPolicy.RetaliateWhenTargeted;
        public bool ShouldRetaliateWhenDamaged => engagementPolicy == EnemyEngagementPolicy.RetaliateWhenDamaged;
        public bool ShouldRetaliate => ShouldRetaliateWhenTargeted || ShouldRetaliateWhenDamaged;
        public bool ShouldChase => movementPolicy == EnemyMovementPolicy.ChaseTarget;
        public bool ShouldFleeWhenDamaged => movementPolicy == EnemyMovementPolicy.FleeWhenDamaged;
        public bool ShouldKeepDistance => movementPolicy == EnemyMovementPolicy.KeepDistance;
        public bool ShouldFlee(float normalizedHealth) =>
            movementPolicy == EnemyMovementPolicy.FleeAtLowHealth &&
            normalizedHealth <= lowHealthThreshold;
    }
}
