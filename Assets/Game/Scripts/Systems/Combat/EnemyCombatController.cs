using RPGProject.Character;
using RPGProject.Gameplay;
using System;
using UnityEngine;

namespace RPGProject.Systems
{
    public enum EnemyCombatState
    {
        Idle,
        Alert,
        Chasing,
        Attacking,
        Fleeing,
        Dead
    }

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
        private EnemyCombatState currentState = EnemyCombatState.Idle;
        private bool hasForcedTarget;
        private bool wasDamagedByTarget;
        private readonly EnemyCombatBehaviorResolver behaviorResolver = new();

        public event Action<EnemyCombatController, EnemyCombatState> StateChanged;

        public EnemyCombatState CurrentState => currentState;
        public HealthComponent CurrentTarget => targetHealth;
        private float DetectionRange => behaviorSettings != null ? behaviorSettings.DetectionRange : fallbackDetectionRange;
        private float AttackRange => combatActor != null ? combatActor.AttackRange : fallbackAttackRange;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            motor = GetComponent<CharacterMotor2D>();
            ResolveCombatActor();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            PublishState(EnemyCombatState.Idle);
        }

        private void Update()
        {
            ResolveReferences();

            ResolveTarget();
            EnemyCombatContext context = CreateContext();
            EnemyCombatIntent intent = ResolveIntent(context);
            ExecuteIntent(intent);
        }

        private EnemyCombatIntent ResolveIntent(EnemyCombatContext context)
        {
            if (behaviorSettings != null)
            {
                return behaviorResolver.Resolve(behaviorSettings, context);
            }

            if (context.SelfDead || !context.HasTarget || context.TargetDead || !context.TargetInDetectionRange)
            {
                return EnemyCombatIntent.Idle;
            }

            if (context.TargetInAttackRange)
            {
                return EnemyCombatIntent.Attack;
            }

            return fallbackShouldChase ? EnemyCombatIntent.Chase : EnemyCombatIntent.Hold;
        }

        private void ExecuteIntent(EnemyCombatIntent intent)
        {
            switch (intent.IntentType)
            {
                case EnemyCombatIntentType.Flee:
                    FleeFromTarget();
                    PublishState(EnemyCombatState.Fleeing);
                    break;
                case EnemyCombatIntentType.Chase:
                    MoveTowardTarget();
                    PublishState(EnemyCombatState.Chasing);
                    break;
                case EnemyCombatIntentType.Attack:
                    StopMoving();
                    combatActor.SetTarget(targetHealth);
                    combatActor.TryAttackCurrentTarget();
                    PublishState(EnemyCombatState.Attacking);
                    break;
                case EnemyCombatIntentType.Hold:
                    StopMoving();
                    combatActor?.ClearTarget();
                    PublishState(EnemyCombatState.Alert);
                    break;
                default:
                    StopMoving();
                    combatActor?.ClearTarget();
                    PublishState(health != null && health.IsDead ? EnemyCombatState.Dead : EnemyCombatState.Idle);
                    break;
            }
        }

        private void ResolveTarget()
        {
            if (targetHealth != null && !targetHealth.IsDead && IsTargetStillRelevant(targetHealth))
            {
                return;
            }

            if (explicitTarget != null && !explicitTarget.IsDead)
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
            hasForcedTarget = target != null;
        }

        public void ClearTarget()
        {
            explicitTarget = null;
            targetHealth = null;
            hasForcedTarget = false;
            wasDamagedByTarget = false;
            combatActor?.ClearTarget();
            StopMoving();
            PublishState(health != null && health.IsDead ? EnemyCombatState.Dead : EnemyCombatState.Idle);
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

        private void MoveTowardTarget()
        {
            if (motor == null || targetHealth == null)
            {
                return;
            }

            motor.SetMovementTarget(targetHealth.transform.position, AttackRange);
        }

        private void FleeFromTarget()
        {
            if (motor == null || targetHealth == null)
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
            float fleeSpeedMultiplier = behaviorSettings != null ? behaviorSettings.FleeSpeedMultiplier : 1f;
            motor.SetMovementTarget(currentPosition + awayDirection.normalized * fleeDistance, 0.1f, fleeSpeedMultiplier);
        }

        private EnemyCombatContext CreateContext()
        {
            bool hasTarget = targetHealth != null;
            bool targetDead = targetHealth != null && targetHealth.IsDead;
            Vector2 selfPosition = transform.position;
            Vector2 targetPosition = targetHealth != null ? targetHealth.transform.position : transform.position;

            return new EnemyCombatContext(
                health == null || health.IsDead,
                hasTarget,
                targetDead,
                hasTarget && !targetDead && IsTargetInRange(),
                hasTarget && !targetDead && IsWithinDetectionRange(targetPosition),
                hasForcedTarget,
                wasDamagedByTarget,
                health != null ? health.NormalizedHealth : 0f,
                selfPosition,
                targetPosition);
        }

        private bool IsTargetStillRelevant(HealthComponent candidate)
        {
            if (candidate == explicitTarget || hasForcedTarget || wasDamagedByTarget)
            {
                return true;
            }

            return IsWithinDetectionRange(candidate.transform.position);
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

        private void ResolveReferences()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            if (motor == null)
            {
                motor = GetComponent<CharacterMotor2D>();
            }

            ResolveCombatActor();
        }

        private void SubscribeEvents()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
                health.HealthChanged += OnHealthChanged;
            }

            if (combatActor != null)
            {
                combatActor.Selected -= OnSelectedAsTarget;
                combatActor.Selected += OnSelectedAsTarget;
            }
        }

        private void UnsubscribeEvents()
        {
            if (health != null)
            {
                health.HealthChanged -= OnHealthChanged;
            }

            if (combatActor != null)
            {
                combatActor.Selected -= OnSelectedAsTarget;
            }
        }

        private void OnHealthChanged(HealthChange change)
        {
            if (behaviorSettings == null || !behaviorSettings.ShouldRetaliate || change.ChangeType != HealthChangeType.Damage)
            {
                return;
            }

            HealthComponent attacker = ResolveSourceHealth(change.Source);
            if (attacker != null && !attacker.IsDead)
            {
                explicitTarget = attacker;
                targetHealth = attacker;
                wasDamagedByTarget = true;
                hasForcedTarget = behaviorSettings.ShouldRetaliateWhenTargeted;
            }
        }

        private void OnSelectedAsTarget(CombatActor selectedActor)
        {
            if (behaviorSettings == null || !behaviorSettings.ShouldRetaliateWhenTargeted)
            {
                return;
            }

            HealthComponent attacker = FindPlayerHealth();
            if (attacker != null)
            {
                SetTarget(attacker);
            }
        }

        private static HealthComponent ResolveSourceHealth(UnityEngine.Object source)
        {
            return source switch
            {
                CombatActor actor => actor.Health,
                HealthComponent sourceHealth => sourceHealth,
                GameObject sourceObject => sourceObject.GetComponent<HealthComponent>(),
                Component sourceComponent => sourceComponent.GetComponent<HealthComponent>(),
                _ => null
            };
        }

        private static HealthComponent FindPlayerHealth()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player != null && player.TryGetComponent(out HealthComponent playerHealth)
                ? playerHealth
                : null;
        }

        private void PublishState(EnemyCombatState nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;
            StateChanged?.Invoke(this, currentState);
        }
    }
}
