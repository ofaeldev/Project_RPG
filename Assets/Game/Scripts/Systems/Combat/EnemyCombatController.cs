using RPGProject.Character;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(CombatActor))]
    public sealed class EnemyCombatController : MonoBehaviour
    {
        [Header("Behavior")]
        [SerializeField]
        private EnemyCombatBehaviorSettings behaviorSettings;

        [SerializeField]
        private CombatActor combatActor;

        [SerializeField]
        private HealthComponent explicitTarget;

        [Header("Legacy Migration")]
        [SerializeField]
        [HideInInspector]
        private CombatAttackSettings attackSettings;

        [SerializeField]
        [HideInInspector]
        [Min(0f)]
        private float fallbackDetectionRange = 4f;

        [SerializeField]
        [HideInInspector]
        [Min(0f)]
        private float fallbackAttackRange = 1.1f;

        [SerializeField]
        [HideInInspector]
        [Min(1)]
        private int fallbackDamage = 2;

        [SerializeField]
        [HideInInspector]
        [Min(0.01f)]
        private float fallbackAttackInterval = 1.2f;

        [SerializeField]
        [HideInInspector]
        private bool fallbackShouldChase = true;

        private HealthComponent health;
        private CharacterMotor2D motor;
        private HealthComponent targetHealth;

        private float DetectionRange => behaviorSettings != null ? behaviorSettings.DetectionRange : fallbackDetectionRange;
        private float AttackRange => combatActor != null ? combatActor.AttackRange : fallbackAttackRange;
        private bool ShouldChaseTarget => behaviorSettings != null ? behaviorSettings.ShouldChase : fallbackShouldChase;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            motor = GetComponent<CharacterMotor2D>();
            ResolveCombatActor();
        }

        private void Update()
        {
            if (health == null || health.IsDead)
            {
                StopMoving();
                combatActor?.ClearTarget();
                return;
            }

            ResolveTarget();
            if (targetHealth == null || targetHealth.IsDead)
            {
                StopMoving();
                combatActor?.ClearTarget();
                return;
            }

            if (ShouldFlee())
            {
                FleeFromTarget();
                return;
            }

            if (!IsTargetInRange())
            {
                if (ShouldChaseTarget)
                {
                    MoveTowardTarget();
                }
                else
                {
                    StopMoving();
                }

                return;
            }

            StopMoving();
            combatActor.SetTarget(targetHealth);
            combatActor.TryAttackCurrentTarget();
        }

        private void ResolveTarget()
        {
            if (targetHealth != null && !targetHealth.IsDead && IsWithinDetectionRange(targetHealth.transform.position))
            {
                return;
            }

            if (explicitTarget != null && !explicitTarget.IsDead && IsWithinDetectionRange(explicitTarget.transform.position))
            {
                targetHealth = explicitTarget;
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || !player.TryGetComponent(out HealthComponent playerHealth))
            {
                targetHealth = null;
                return;
            }

            targetHealth = IsWithinDetectionRange(player.transform.position) ? playerHealth : null;
        }

        public void SetTarget(HealthComponent target)
        {
            explicitTarget = target;
            targetHealth = target;
        }

        private bool IsWithinDetectionRange(Vector2 position)
        {
            float range = DetectionRange;
            return range <= 0f || Vector2.SqrMagnitude(position - (Vector2)transform.position) <= range * range;
        }

        private bool IsTargetInRange()
        {
            float range = AttackRange;
            return Vector2.SqrMagnitude((Vector2)targetHealth.transform.position - (Vector2)transform.position) <= range * range;
        }

        private bool ShouldFlee()
        {
            return behaviorSettings != null && behaviorSettings.ShouldFlee(health.NormalizedHealth);
        }

        private void MoveTowardTarget()
        {
            if (motor == null)
            {
                return;
            }

            motor.SetMovementTarget(targetHealth.transform.position, AttackRange);
        }

        private void FleeFromTarget()
        {
            if (motor == null)
            {
                return;
            }

            Vector2 currentPosition = transform.position;
            Vector2 targetPosition = targetHealth.transform.position;
            Vector2 awayDirection = currentPosition - targetPosition;
            if (awayDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                awayDirection = Vector2.right;
            }

            float fleeDistance = behaviorSettings != null ? behaviorSettings.FleeDistance : DetectionRange;
            motor.SetMovementTarget(currentPosition + awayDirection.normalized * fleeDistance, 0.1f);
        }

        private void StopMoving()
        {
            motor?.Stop();
        }

        private void ResolveCombatActor()
        {
            if (combatActor == null)
            {
                combatActor = GetComponent<CombatActor>();
            }

            if (combatActor == null)
            {
                combatActor = gameObject.AddComponent<CombatActor>();
            }

            if (combatActor != null && attackSettings != null)
            {
                combatActor.Configure(attackSettings, fallbackAttackRange, fallbackDamage, fallbackAttackInterval, shouldSelectTargets: false);
            }
        }
    }
}
