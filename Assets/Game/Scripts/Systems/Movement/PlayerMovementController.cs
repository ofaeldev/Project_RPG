using RPGProject.Character;
using RPGProject.Inputs;
using UnityEngine;

namespace RPGProject.Systems
{
    /// <summary>
    /// Converte input e alvos de clique em intencao de movimento.
    /// Nao aplica fisica diretamente; isso fica no CharacterMotor2D.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterMotor2D))]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        private CharacterMotor2D motor;
        private PlayerInputReader inputReader;
        private Vector2 movementInput;
        private Vector2 clickTarget;
        private bool hasClickTarget;
        private float stopDistance;

        [Header("Click-to-move")]
        [Tooltip("Distancia minima usada para cliques livres no chao. Evita micro-correcao visual no destino.")]
        [SerializeField]
        [Min(0f)]
        private float freeClickStopDistance = 0.03f;

        public bool HasMoveTarget => hasClickTarget;
        public Vector2 CurrentPosition => motor != null ? motor.CurrentPosition : transform.position;
        public float StopDistance => stopDistance;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor2D>();
            inputReader = GetComponent<PlayerInputReader>();

            if (motor == null)
            {
                Debug.LogError("CharacterMotor2D nao encontrado. O PlayerMovementController exige um motor fisico.", this);
            }

            if (inputReader == null)
            {
                Debug.LogError("PlayerInputReader nao encontrado. O PlayerMovementController exige input do jogador.", this);
            }
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.MovementChanged += OnMovementChanged;
                inputReader.ClickMovePressedEvent += OnClickMovePressed;
                movementInput = inputReader.Movement;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.MovementChanged -= OnMovementChanged;
                inputReader.ClickMovePressedEvent -= OnClickMovePressed;
            }

            ClearMoveTarget();
        }

        private void Update()
        {
            UpdateMovementIntent();
        }

        public void SetMoveTarget(Vector2 worldPosition, float stopDistance = 0f)
        {
            clickTarget = worldPosition;
            hasClickTarget = true;
            this.stopDistance = Mathf.Max(0f, stopDistance);
            motor?.SetMovementTarget(clickTarget, this.stopDistance);
        }

        public void ClearMoveTarget()
        {
            hasClickTarget = false;
            stopDistance = 0f;
            motor?.Stop();
        }

        public bool IsAtMoveTarget()
        {
            if (!hasClickTarget || motor == null)
            {
                return false;
            }

            return Vector2.SqrMagnitude(clickTarget - motor.CurrentPosition) <= stopDistance * stopDistance;
        }

        private void UpdateMovementIntent()
        {
            if (motor == null)
            {
                return;
            }

            GameplayInputBlocker inputBlocker = GameplayInputBlocker.Instance;
            if (inputBlocker != null && inputBlocker.ShouldBlockGameplayInput)
            {
                ClearMoveTarget();
                return;
            }

            if (movementInput.sqrMagnitude > 0f)
            {
                hasClickTarget = false;
                motor.SetMovementDirection(movementInput);
                return;
            }

            if (!hasClickTarget)
            {
                motor.Stop();
                return;
            }

            Vector2 currentPosition = motor.CurrentPosition;
            Vector2 delta = clickTarget - currentPosition;
            float distanceSquared = delta.sqrMagnitude;
            float stopDistanceSquared = stopDistance * stopDistance;

            if (distanceSquared <= stopDistanceSquared || distanceSquared <= Mathf.Epsilon)
            {
                hasClickTarget = false;
                motor.Stop();
                return;
            }

            if (!motor.HasMovementTarget)
            {
                motor.SetMovementTarget(clickTarget, stopDistance);
            }
        }

        private void OnMovementChanged(Vector2 movement)
        {
            movementInput = movement;
        }

        private void OnClickMovePressed(Vector2 worldPosition)
        {
            GameplayInputBlocker inputBlocker = GameplayInputBlocker.Instance;
            if (inputBlocker != null && (inputBlocker.ShouldBlockGameplayInput || inputBlocker.ShouldBlockGameplayAction))
            {
                ClearMoveTarget();
                return;
            }

            SetMoveTarget(worldPosition, freeClickStopDistance);
        }
    }
}
