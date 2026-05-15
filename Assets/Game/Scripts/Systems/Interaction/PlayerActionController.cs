using RPGProject.Gameplay;
using RPGProject.Inputs;
using UnityEngine;

namespace RPGProject.Systems
{
    /// <summary>
    /// Ponte entre input de acoes do jogador e alvos interativos do mundo.
    /// Nao le teclado/mouse diretamente e nao aplica movimento/fisica.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerMovementController))]
    public sealed class PlayerActionController : MonoBehaviour
    {
        [Header("Interaction")]
        [Tooltip("Camada usada para localizar alvos interativos com clique direito.")]
        [SerializeField]
        private LayerMask interactionMask = Physics2D.AllLayers;

        [Tooltip("Raio usado para detectar o alvo logo abaixo do cursor.")]
        [SerializeField]
        private float interactionRadius = 0.1f;

        private PlayerInputReader inputReader;
        private PlayerMovementController movementController;
        private AutoAttackController autoAttackController;
        private IRightClickActionTarget pendingActionTarget;
        private RightClickActionType pendingActionType;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            movementController = GetComponent<PlayerMovementController>();
            autoAttackController = GetComponent<AutoAttackController>();

            if (inputReader == null)
            {
                Debug.LogError("PlayerActionController precisa de PlayerInputReader.", this);
            }

            if (movementController == null)
            {
                Debug.LogError("PlayerActionController precisa de PlayerMovementController.", this);
            }
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.ClickMovePressedEvent += OnClickMovePressed;
                inputReader.RightClickActionPressedEvent += OnRightClickActionPressed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.ClickMovePressedEvent -= OnClickMovePressed;
                inputReader.RightClickActionPressedEvent -= OnRightClickActionPressed;
            }

            ClearPendingAction();
        }

        private void Update()
        {
            if (pendingActionTarget != null)
            {
                if (GameplayInputBlocker.Instance != null && GameplayInputBlocker.Instance.ShouldBlockGameplayAction)
                {
                    ClearPendingAction();
                    autoAttackController?.StopAttacking();
                    movementController?.ClearMoveTarget();
                    return;
                }

                if (movementController != null && movementController.HasMoveTarget && movementController.IsAtMoveTarget())
                {
                    TryExecutePendingAction();
                }
            }
        }

        private void ProcessRightClick(Vector2 worldPosition)
        {
            IRightClickActionTarget target = FindTopPriorityTarget(worldPosition);

            if (target == null)
            {
                ClearPendingAction();
                autoAttackController?.StopAttacking();
                movementController.SetMoveTarget(worldPosition, 0f);
                return;
            }

            RightClickActionType actionType = target.GetPreferredRightClickAction(worldPosition);
            float actionRange = target.GetActionRange(actionType);

            if (actionType == RightClickActionType.Attack)
            {
                ClearPendingAction();
                ExecuteAction(target, worldPosition, actionType);
                return;
            }

            if (IsWithinRange(target, actionRange))
            {
                ExecuteAction(target, worldPosition, actionType);
                return;
            }

            pendingActionTarget = target;
            pendingActionType = actionType;
            movementController.SetMoveTarget(target.GameObject.transform.position, actionRange);
        }

        private void TryExecutePendingAction()
        {
            if (pendingActionTarget == null)
            {
                return;
            }

            float actionRange = pendingActionTarget.GetActionRange(pendingActionType);
            if (!IsWithinRange(pendingActionTarget, actionRange))
            {
                return;
            }

            ExecuteAction(pendingActionTarget, pendingActionTarget.GameObject.transform.position, pendingActionType);
            ClearPendingAction();
        }

        private void ExecuteAction(IRightClickActionTarget target, Vector2 worldPosition, RightClickActionType actionType)
        {
            var context = new RightClickActionContext(worldPosition, gameObject, target.GameObject, actionType);
            movementController.ClearMoveTarget();
            target.PerformRightClickAction(context);
        }

        private bool IsWithinRange(IRightClickActionTarget target, float actionRange)
        {
            if (target == null)
            {
                return false;
            }

            if (actionRange <= 0f)
            {
                return true;
            }

            Vector2 currentPosition = movementController.CurrentPosition;
            Vector2 targetPosition = target.GameObject.transform.position;
            return Vector2.Distance(currentPosition, targetPosition) <= actionRange;
        }

        private void ClearPendingAction()
        {
            pendingActionTarget = null;
            pendingActionType = RightClickActionType.None;
        }

        private void OnClickMovePressed(Vector2 worldPosition)
        {
            ClearPendingAction();
            autoAttackController?.StopAttacking();
        }

        private void OnRightClickActionPressed(Vector2 worldPosition)
        {
            if (inputReader == null || movementController == null)
            {
                return;
            }

            if (GameplayInputBlocker.Instance != null && GameplayInputBlocker.Instance.ShouldBlockGameplayAction)
            {
                ClearPendingAction();
                autoAttackController?.StopAttacking();
                movementController.ClearMoveTarget();
                return;
            }

            ProcessRightClick(worldPosition);
        }

        private IRightClickActionTarget FindTopPriorityTarget(Vector2 worldPosition)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, interactionRadius, interactionMask);
            IRightClickActionTarget bestTarget = null;
            RightClickActionType bestPriority = RightClickActionType.None;

            foreach (Collider2D hit in hits)
            {
                if (hit == null)
                {
                    continue;
                }

                IRightClickActionTarget candidate = hit.GetComponent<IRightClickActionTarget>();
                if (candidate == null)
                {
                    continue;
                }

                RightClickActionType candidateAction = candidate.GetPreferredRightClickAction(worldPosition);
                if (IsHigherPriority(candidateAction, bestPriority))
                {
                    bestPriority = candidateAction;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
        }

        private static bool IsHigherPriority(RightClickActionType candidate, RightClickActionType current)
        {
            if (candidate == RightClickActionType.None)
            {
                return false;
            }

            if (current == RightClickActionType.None)
            {
                return true;
            }

            return candidate < current;
        }
    }
}
