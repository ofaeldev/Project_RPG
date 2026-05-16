using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(CombatActor))]
    public sealed class AutoAttackController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private CombatActor combatActor;

        [Header("Legacy Migration")]
        [SerializeField]
        [HideInInspector]
        private CombatAttackSettings attackSettings;

        [SerializeField]
        [HideInInspector]
        [Min(0f)]
        private float fallbackAttackRange = 1.5f;

        [SerializeField]
        [HideInInspector]
        [Min(1)]
        private int fallbackDamage = 5;

        [SerializeField]
        [HideInInspector]
        [Min(0.01f)]
        private float fallbackAttackInterval = 1f;

        [Header("Target Follow")]
        [SerializeField]
        private bool followTarget = true;

        [SerializeField]
        [Min(0f)]
        [Tooltip("When following a combat target, move slightly inside attack range so moving/fleeing targets do not hover exactly on the range edge.")]
        private float followAttackRangeBuffer = 0.15f;

        [Header("Feedback")]
        [SerializeField]
        private string outOfRangeFollowDisabledMessage = "Alvo fora de alcance. Aproxime-se ou ative Follow.";

        private PlayerMovementController movementController;

        public CombatActor CurrentTarget => combatActor != null ? combatActor.CurrentTargetActor : null;
        public bool HasTarget => combatActor != null && combatActor.HasTarget;
        public bool FollowTarget => followTarget;

        private void Awake()
        {
            movementController = GetComponent<PlayerMovementController>();
            ResolveCombatActor();
        }

        private void Update()
        {
            if (combatActor == null || !combatActor.HasTarget)
            {
                return;
            }

            if (GameplayInputBlocker.Instance != null && GameplayInputBlocker.Instance.ShouldBlockGameplayAction)
            {
                return;
            }

            if (!combatActor.HasValidTarget)
            {
                StopAttacking();
                return;
            }

            if (!combatActor.IsCurrentTargetInRange())
            {
                if (followTarget)
                {
                    movementController.SetMoveTarget(combatActor.CurrentTargetHealth.transform.position, GetFollowStopDistance());
                }
                else
                {
                    movementController.ClearMoveTarget();
                    combatActor.TryAttackCurrentTarget();
                }

                return;
            }

            movementController.ClearMoveTarget();
            combatActor.TryAttackCurrentTarget();
        }

        private void OnDisable()
        {
            UnsubscribeCombatActorEvents();
            StopAttacking();
        }

        private void OnEnable()
        {
            ResolveCombatActor();
            SubscribeCombatActorEvents();
        }

        public void StartAttacking(CombatActor target)
        {
            if (target == null || !target.CanBeAttacked)
            {
                StopAttacking();
                return;
            }

            ResolveCombatActor();
            if (combatActor == null)
            {
                return;
            }

            combatActor.SetTarget(target);
        }

        public void StartAttacking(CombatTarget target)
        {
            if (target == null || !target.CanBeAttacked)
            {
                StopAttacking();
                return;
            }

            ResolveCombatActor();
            combatActor?.SetTarget(target);
        }

        public void SetFollowTarget(bool shouldFollow)
        {
            followTarget = shouldFollow;

            if (!followTarget && combatActor != null && combatActor.HasTarget && !combatActor.IsCurrentTargetInRange())
            {
                movementController.ClearMoveTarget();
            }
        }

        public void StopAttacking()
        {
            combatActor?.ClearTarget();
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
                combatActor.Configure(attackSettings, fallbackAttackRange, fallbackDamage, fallbackAttackInterval, shouldSelectTargets: true);
            }
        }

        private void SubscribeCombatActorEvents()
        {
            if (combatActor != null)
            {
                combatActor.AttackOutOfRange -= OnAttackOutOfRange;
                combatActor.AttackOutOfRange += OnAttackOutOfRange;
            }
        }

        private void UnsubscribeCombatActorEvents()
        {
            if (combatActor != null)
            {
                combatActor.AttackOutOfRange -= OnAttackOutOfRange;
            }
        }

        private void OnAttackOutOfRange(CombatActor sourceActor, HealthComponent targetHealth)
        {
            if (sourceActor != combatActor || followTarget)
            {
                return;
            }

            GameplayUIEvents.ShowWarning(
                outOfRangeFollowDisabledMessage,
                source: targetHealth != null ? targetHealth.gameObject : gameObject);
        }

        private float GetFollowStopDistance()
        {
            return combatActor != null
                ? Mathf.Max(0f, combatActor.AttackRange - followAttackRangeBuffer)
                : 0f;
        }
    }
}
