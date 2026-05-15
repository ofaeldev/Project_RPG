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
        FleeAtLowHealth
    }

    [CreateAssetMenu(
        fileName = "EnemyCombatBehaviorSettings",
        menuName = "RPG Project/Combat/Enemy Behavior Settings")]
    public sealed class EnemyCombatBehaviorSettings : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private EnemyAttackMode attackMode = EnemyAttackMode.Melee;

        [SerializeField]
        private EnemyMovementPolicy movementPolicy = EnemyMovementPolicy.ChaseTarget;

        [Header("Targeting")]
        [SerializeField]
        [Min(0f)]
        private float detectionRange = 4f;

        [Header("Movement")]
        [SerializeField]
        [Range(0f, 1f)]
        private float lowHealthThreshold = 0.25f;

        [SerializeField]
        [Min(0.1f)]
        private float fleeDistance = 3f;

        public EnemyAttackMode AttackMode => attackMode;
        public EnemyMovementPolicy MovementPolicy => movementPolicy;
        public float DetectionRange => detectionRange;
        public float LowHealthThreshold => lowHealthThreshold;
        public float FleeDistance => fleeDistance;

        public bool ShouldChase => movementPolicy == EnemyMovementPolicy.ChaseTarget;
        public bool ShouldFlee(float normalizedHealth) =>
            movementPolicy == EnemyMovementPolicy.FleeAtLowHealth &&
            normalizedHealth <= lowHealthThreshold;
    }
}
