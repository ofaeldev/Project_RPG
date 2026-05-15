using UnityEngine;

namespace RPGProject.Character
{
    /// <summary>
    /// Aplica movimento fisico top-down em um Rigidbody2D.
    /// Responsabilidade unica: transformar uma direcao em velocidade.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class CharacterMotor2D : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Configuracao compartilhada de movimento deste personagem.")]
        [SerializeField]
        private CharacterMovementSettings movementSettings;

        [Tooltip("Velocidade usada caso nenhuma configuracao seja atribuida.")]
        [SerializeField]
        [Min(0f)]
        private float fallbackMoveSpeed = 4f;

        private Rigidbody2D cachedRigidbody;
        private Vector2 movementDirection;
        private Vector2 movementTarget;
        private float targetStopDistance;
        private bool hasMovementTarget;

        public Vector2 CurrentPosition => cachedRigidbody != null ? cachedRigidbody.position : transform.position;
        public bool HasMovementTarget => hasMovementTarget;

        private float MoveSpeed => movementSettings != null
            ? movementSettings.MoveSpeed
            : fallbackMoveSpeed;

        private void Awake()
        {
            cachedRigidbody = GetComponent<Rigidbody2D>();
            ConfigureRigidbody();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        /// <summary>
        /// Recebe a direcao desejada por outro componente, como input ou IA.
        /// </summary>
        public void SetMovementDirection(Vector2 direction)
        {
            hasMovementTarget = false;
            movementDirection = Vector2.ClampMagnitude(direction, 1f);
        }

        public void SetMovementTarget(Vector2 target, float stopDistance = 0f)
        {
            movementTarget = target;
            targetStopDistance = Mathf.Max(0f, stopDistance);
            hasMovementTarget = true;
            movementDirection = Vector2.zero;
        }

        public void Stop()
        {
            hasMovementTarget = false;
            movementDirection = Vector2.zero;

            if (cachedRigidbody != null)
            {
                cachedRigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void ConfigureRigidbody()
        {
            cachedRigidbody.gravityScale = 0f;
            cachedRigidbody.freezeRotation = true;
            cachedRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void ApplyMovement()
        {
            if (hasMovementTarget)
            {
                ApplyTargetMovement();
                return;
            }

            cachedRigidbody.linearVelocity = movementDirection * MoveSpeed;
        }

        private void ApplyTargetMovement()
        {
            Vector2 currentPosition = cachedRigidbody.position;
            Vector2 targetDelta = movementTarget - currentPosition;
            float targetDistance = targetDelta.magnitude;

            if (targetDistance <= targetStopDistance || targetDistance <= Mathf.Epsilon || MoveSpeed <= 0f)
            {
                Stop();
                return;
            }

            float stepDistance = MoveSpeed * Time.fixedDeltaTime;
            float nextDistanceFromTarget = Mathf.Max(targetDistance - stepDistance, targetStopDistance);

            if (nextDistanceFromTarget >= targetDistance)
            {
                Stop();
                return;
            }

            Vector2 directionToTarget = targetDelta / targetDistance;
            Vector2 nextPosition = movementTarget - directionToTarget * nextDistanceFromTarget;
            cachedRigidbody.MovePosition(nextPosition);
        }
    }
}
