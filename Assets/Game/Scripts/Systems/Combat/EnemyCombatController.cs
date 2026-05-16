using RPGProject.Character;
using RPGProject.Gameplay;
using System;
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

        [Header("Leash")]
        [SerializeField]
        private bool returnHomeWhenTargetLost = true;

        [SerializeField]
        [Min(0f)]
        private float leashDistanceFromHome = 6f;

        [SerializeField]
        [Min(0f)]
        private float returnHomeStopDistance = 0.08f;

        [SerializeField]
        [Min(0.1f)]
        private float returnHomeSpeedMultiplier = 1f;

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
        private Vector2 homePosition;
        private bool hasHomePosition;
        private readonly EnemyCombatBehaviorResolver behaviorResolver = new();
        private readonly EnemyCombatStateResolver stateResolver = new();

        public event Action<EnemyCombatController, EnemyCombatState> StateChanged;

        public EnemyCombatState CurrentState => currentState;
        public HealthComponent CurrentTarget => targetHealth;
        public Vector2 HomePosition => homePosition;
        private float DetectionRange => behaviorSettings != null ? behaviorSettings.DetectionRange : fallbackDetectionRange;
        private float AttackRange => combatActor != null ? combatActor.AttackRange : fallbackAttackRange;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            motor = GetComponent<CharacterMotor2D>();
            ResolveCombatActor();
            CaptureHomePosition();
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
            if (ShouldReturnHome())
            {
                ReturnHome();
                PublishState(EnemyCombatState.Returning);
                return;
            }

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
                    break;
                case EnemyCombatIntentType.Chase:
                    MoveTowardTarget();
                    break;
                case EnemyCombatIntentType.Attack:
                    StopMoving();
                    combatActor.SetTarget(targetHealth);
                    combatActor.TryAttackCurrentTarget();
                    break;
                case EnemyCombatIntentType.Hold:
                    StopMoving();
                    combatActor?.ClearTarget();
                    break;
                default:
                    StopMoving();
                    combatActor?.ClearTarget();
                    break;
            }

            PublishState(stateResolver.Resolve(intent, health != null && health.IsDead));
        }

        private void ResolveTarget()
        {
            if (ShouldBreakLeash())
            {
                targetHealth = null;
                explicitTarget = null;
                hasForcedTarget = false;
                wasDamagedByTarget = false;
                combatActor?.ClearTarget();
                return;
            }

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

            targetHealth = ShouldAcquireTargetOnSight() && IsWithinDetectionRange(player.transform.position) ? playerHealth : null;
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

        public void SetHomePosition(Vector2 position)
        {
            homePosition = position;
            hasHomePosition = true;
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

        private bool ShouldBreakLeash()
        {
            if (!hasHomePosition || leashDistanceFromHome <= 0f || targetHealth == null)
            {
                return false;
            }

            if (hasForcedTarget || wasDamagedByTarget)
            {
                return false;
            }

            return Vector2.SqrMagnitude((Vector2)transform.position - homePosition) > leashDistanceFromHome * leashDistanceFromHome;
        }

        private bool ShouldReturnHome()
        {
            if (!returnHomeWhenTargetLost || !hasHomePosition || targetHealth != null || health == null || health.IsDead)
            {
                return false;
            }

            return Vector2.SqrMagnitude((Vector2)transform.position - homePosition) > returnHomeStopDistance * returnHomeStopDistance;
        }

        private void ReturnHome()
        {
            if (motor == null)
            {
                return;
            }

            motor.SetMovementTarget(homePosition, returnHomeStopDistance, returnHomeSpeedMultiplier);
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

        private bool ShouldAcquireTargetOnSight()
        {
            return behaviorSettings == null || behaviorSettings.ShouldAcquireOnSight;
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
            CaptureHomePosition();
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

            EnemyCombatState previousState = currentState;
            currentState = nextState;
            StateChanged?.Invoke(this, currentState);
            GameplayEvents.PublishEnemyStateChanged(this, previousState, currentState);
        }

        private void CaptureHomePosition()
        {
            if (hasHomePosition)
            {
                return;
            }

            homePosition = transform.position;
            hasHomePosition = true;
        }
    }
}
