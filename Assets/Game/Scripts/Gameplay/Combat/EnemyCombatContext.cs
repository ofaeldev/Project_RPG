using UnityEngine;

namespace RPGProject.Gameplay
{
    public readonly struct EnemyCombatContext
    {
        public EnemyCombatContext(
            bool selfDead,
            bool hasTarget,
            bool targetDead,
            bool targetInAttackRange,
            bool targetInDetectionRange,
            bool hasForcedTarget,
            bool wasDamagedByTarget,
            float normalizedHealth,
            Vector2 selfPosition,
            Vector2 targetPosition)
        {
            SelfDead = selfDead;
            HasTarget = hasTarget;
            TargetDead = targetDead;
            TargetInAttackRange = targetInAttackRange;
            TargetInDetectionRange = targetInDetectionRange;
            HasForcedTarget = hasForcedTarget;
            WasDamagedByTarget = wasDamagedByTarget;
            NormalizedHealth = normalizedHealth;
            SelfPosition = selfPosition;
            TargetPosition = targetPosition;
            DistanceToTarget = Vector2.Distance(selfPosition, targetPosition);
        }

        public bool SelfDead { get; }
        public bool HasTarget { get; }
        public bool TargetDead { get; }
        public bool TargetInAttackRange { get; }
        public bool TargetInDetectionRange { get; }
        public bool HasForcedTarget { get; }
        public bool WasDamagedByTarget { get; }
        public float NormalizedHealth { get; }
        public Vector2 SelfPosition { get; }
        public Vector2 TargetPosition { get; }
        public float DistanceToTarget { get; }
    }
}
