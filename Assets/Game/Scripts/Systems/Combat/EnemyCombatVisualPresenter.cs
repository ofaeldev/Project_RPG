using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyCombatController))]
    public sealed class EnemyCombatVisualPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private EnemyCombatController controller;

        [SerializeField]
        private SpriteRenderer targetRenderer;

        [SerializeField]
        private Animator animator;

        [Header("State Colors")]
        [SerializeField]
        private Color idleColor = Color.white;

        [SerializeField]
        private Color alertColor = new(1f, 0.92f, 0.58f, 1f);

        [SerializeField]
        private Color chasingColor = new(1f, 0.72f, 0.42f, 1f);

        [SerializeField]
        private Color attackingColor = new(1f, 0.38f, 0.32f, 1f);

        [SerializeField]
        private Color fleeingColor = new(0.62f, 0.78f, 1f, 1f);

        [SerializeField]
        private Color deadColor = new(0.38f, 0.38f, 0.38f, 1f);

        [Header("Animator Parameters")]
        [SerializeField]
        private string stateParameter = "CombatState";

        [SerializeField]
        private string chasingParameter = "IsChasing";

        [SerializeField]
        private string attackingParameter = "IsAttacking";

        [SerializeField]
        private string fleeingParameter = "IsFleeing";

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (controller != null)
            {
                controller.StateChanged += OnStateChanged;
                ApplyState(controller.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.StateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(EnemyCombatController source, EnemyCombatState state)
        {
            ApplyState(state);
        }

        private void ApplyState(EnemyCombatState state)
        {
            if (targetRenderer != null)
            {
                targetRenderer.color = GetStateColor(state);
            }

            if (animator == null)
            {
                return;
            }

            SetIntegerIfConfigured(stateParameter, (int)state);
            SetBoolIfConfigured(chasingParameter, state == EnemyCombatState.Chasing);
            SetBoolIfConfigured(attackingParameter, state == EnemyCombatState.Attacking);
            SetBoolIfConfigured(fleeingParameter, state == EnemyCombatState.Fleeing);
        }

        private Color GetStateColor(EnemyCombatState state)
        {
            return state switch
            {
                EnemyCombatState.Alert => alertColor,
                EnemyCombatState.Chasing => chasingColor,
                EnemyCombatState.Attacking => attackingColor,
                EnemyCombatState.Fleeing => fleeingColor,
                EnemyCombatState.Dead => deadColor,
                _ => idleColor
            };
        }

        private void SetIntegerIfConfigured(string parameterName, int value)
        {
            if (!string.IsNullOrWhiteSpace(parameterName) && HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Int))
            {
                animator.SetInteger(parameterName, value);
            }
        }

        private void SetBoolIfConfigured(string parameterName, bool value)
        {
            if (!string.IsNullOrWhiteSpace(parameterName) && HasAnimatorParameter(parameterName, AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(parameterName, value);
            }
        }

        private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == parameterType && parameter.name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private void ResolveReferences()
        {
            if (controller == null)
            {
                controller = GetComponent<EnemyCombatController>();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
    }
}
