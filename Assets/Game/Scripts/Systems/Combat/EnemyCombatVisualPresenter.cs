using RPGProject.Gameplay;
using TMPro;
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

        [SerializeField]
        private CombatActor combatActor;

        [Header("State Colors")]
        [SerializeField]
        private Color idleColor = Color.white;

        [SerializeField]
        private Color alertColor = new(1f, 0.95f, 0.68f, 1f);

        [SerializeField]
        private Color chasingColor = new(1f, 0.82f, 0.58f, 1f);

        [SerializeField]
        private Color attackingColor = new(1f, 0.52f, 0.46f, 1f);

        [SerializeField]
        private Color fleeingColor = new(0.68f, 0.84f, 1f, 1f);

        [SerializeField]
        private Color returningColor = new(0.82f, 0.82f, 0.82f, 1f);

        [SerializeField]
        private Color deadColor = new(0.30f, 0.30f, 0.30f, 1f);

        [Header("Aggro Indicator")]
        [SerializeField]
        private string aggroSymbol = "!";

        [SerializeField]
        private Vector3 aggroOffset = new(0f, 0.72f, 0f);

        [SerializeField]
        [Min(0.05f)]
        private float aggroVisibleSeconds = 0.55f;

        [SerializeField]
        [Min(0.1f)]
        private float aggroFontSize = 3f;

        [SerializeField]
        private Color aggroColor = new(1f, 0.22f, 0.12f, 1f);

        [Header("Motion Feedback")]
        [SerializeField]
        [Min(0f)]
        private float attackLungeDistance = 0.08f;

        [SerializeField]
        [Min(0.01f)]
        private float attackLungeSeconds = 0.12f;

        [SerializeField]
        private Vector3 attackScalePunch = new(1.12f, 0.88f, 1f);

        [SerializeField]
        private Vector3 deathScale = new(1.12f, 0.62f, 1f);

        [Header("Animator Parameters")]
        [SerializeField]
        private string stateParameter = "CombatState";

        [SerializeField]
        private string chasingParameter = "IsChasing";

        [SerializeField]
        private string attackingParameter = "IsAttacking";

        [SerializeField]
        private string fleeingParameter = "IsFleeing";

        private TextMeshPro aggroText;
        private EnemyCombatState previousState = EnemyCombatState.Idle;
        private Vector3 baseRendererLocalPosition;
        private Vector3 baseRendererLocalScale = Vector3.one;
        private bool hasBaseRendererTransform;
        private float aggroVisibleUntilTime;
        private float attackFeedbackStartedAt = -1f;
        private Vector3 attackDirection = Vector3.right;

        public bool IsAggroIndicatorVisible => aggroText != null && aggroText.gameObject.activeSelf;
        private bool CanAnimateRendererTransform => targetRenderer != null && targetRenderer.transform != transform;

        private void Awake()
        {
            ResolveReferences();
            CacheRendererTransform();
            EnsureAggroIndicator();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CacheRendererTransform();
            EnsureAggroIndicator();
            if (controller != null)
            {
                controller.StateChanged += OnStateChanged;
                ApplyState(controller.CurrentState);
                previousState = controller.CurrentState;
            }

            if (combatActor != null)
            {
                combatActor.AttackResolved += OnAttackResolved;
            }
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.StateChanged -= OnStateChanged;
            }

            if (combatActor != null)
            {
                combatActor.AttackResolved -= OnAttackResolved;
            }

            HideAggroIndicator();
            RestoreRendererTransform();
        }

        private void LateUpdate()
        {
            UpdateAggroIndicator();
            UpdateAttackFeedback();
        }

        private void OnStateChanged(EnemyCombatController source, EnemyCombatState state)
        {
            if (ShouldShowAggro(previousState, state))
            {
                ShowAggroIndicator();
            }

            previousState = state;
            ApplyState(state);
        }

        private void ApplyState(EnemyCombatState state)
        {
            if (targetRenderer != null)
            {
                targetRenderer.color = GetStateColor(state);
            }

            if (state == EnemyCombatState.Dead)
            {
                ApplyDeathPose();
            }
            else if (attackFeedbackStartedAt < 0f)
            {
                RestoreRendererTransform();
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
                EnemyCombatState.Returning => returningColor,
                EnemyCombatState.Dead => deadColor,
                _ => idleColor
            };
        }

        private void OnAttackResolved(CombatActor source, HealthComponent targetHealth, DamageResult damage)
        {
            if (source != combatActor)
            {
                return;
            }

            BeginAttackFeedback(targetHealth);
        }

        private static bool ShouldShowAggro(EnemyCombatState previous, EnemyCombatState next)
        {
            if (previous != EnemyCombatState.Idle)
            {
                return false;
            }

            return next == EnemyCombatState.Alert
                || next == EnemyCombatState.Chasing
                || next == EnemyCombatState.Attacking;
        }

        private void ShowAggroIndicator()
        {
            EnsureAggroIndicator();
            if (aggroText == null)
            {
                return;
            }

            aggroText.text = aggroSymbol;
            aggroText.color = aggroColor;
            aggroText.fontSize = aggroFontSize;
            aggroText.transform.position = transform.position + aggroOffset;
            aggroText.gameObject.SetActive(true);
            aggroVisibleUntilTime = Time.time + aggroVisibleSeconds;
        }

        private void UpdateAggroIndicator()
        {
            if (aggroText == null || !aggroText.gameObject.activeSelf)
            {
                return;
            }

            aggroText.transform.position = transform.position + aggroOffset + Vector3.up * Mathf.Sin(Time.time * 18f) * 0.03f;
            if (Time.time >= aggroVisibleUntilTime)
            {
                HideAggroIndicator();
            }
        }

        private void HideAggroIndicator()
        {
            if (aggroText != null)
            {
                aggroText.gameObject.SetActive(false);
            }
        }

        private void BeginAttackFeedback(HealthComponent targetHealth)
        {
            if (targetRenderer == null)
            {
                return;
            }

            if (!CanAnimateRendererTransform)
            {
                return;
            }

            CacheRendererTransform();

            Vector3 direction = targetHealth != null
                ? targetHealth.transform.position - transform.position
                : Vector3.right;

            attackDirection = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.right;
            attackFeedbackStartedAt = Time.time;
        }

        private void UpdateAttackFeedback()
        {
            if (targetRenderer == null || attackFeedbackStartedAt < 0f)
            {
                return;
            }

            if (!CanAnimateRendererTransform)
            {
                attackFeedbackStartedAt = -1f;
                return;
            }

            float progress = Mathf.Clamp01((Time.time - attackFeedbackStartedAt) / attackLungeSeconds);
            float pulse = Mathf.Sin(progress * Mathf.PI);
            targetRenderer.transform.localPosition = baseRendererLocalPosition + attackDirection * attackLungeDistance * pulse;
            targetRenderer.transform.localScale = Vector3.Lerp(baseRendererLocalScale, Vector3.Scale(baseRendererLocalScale, attackScalePunch), pulse);

            if (progress >= 1f)
            {
                attackFeedbackStartedAt = -1f;
                RestoreRendererTransform();
            }
        }

        private void ApplyDeathPose()
        {
            if (targetRenderer == null)
            {
                return;
            }

            if (!CanAnimateRendererTransform)
            {
                return;
            }

            CacheRendererTransform();
            attackFeedbackStartedAt = -1f;
            targetRenderer.transform.localPosition = baseRendererLocalPosition;
            targetRenderer.transform.localScale = Vector3.Scale(baseRendererLocalScale, deathScale);
        }

        private void RestoreRendererTransform()
        {
            if (targetRenderer == null || !hasBaseRendererTransform)
            {
                return;
            }

            if (!CanAnimateRendererTransform)
            {
                return;
            }

            if (controller != null && controller.CurrentState == EnemyCombatState.Dead)
            {
                targetRenderer.transform.localPosition = baseRendererLocalPosition;
                targetRenderer.transform.localScale = Vector3.Scale(baseRendererLocalScale, deathScale);
                return;
            }

            targetRenderer.transform.localPosition = baseRendererLocalPosition;
            targetRenderer.transform.localScale = baseRendererLocalScale;
        }

        private void EnsureAggroIndicator()
        {
            if (aggroText != null)
            {
                return;
            }

            GameObject indicator = new("AggroIndicator", typeof(TextMeshPro));
            indicator.transform.SetParent(transform, true);
            aggroText = indicator.GetComponent<TextMeshPro>();
            aggroText.alignment = TextAlignmentOptions.Center;
            aggroText.fontStyle = FontStyles.Bold;
            aggroText.sortingOrder = 85;
            aggroText.textWrappingMode = TextWrappingModes.NoWrap;
            indicator.SetActive(false);
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

            if (combatActor == null)
            {
                combatActor = GetComponent<CombatActor>();
            }
        }

        private void CacheRendererTransform()
        {
            if (targetRenderer == null || hasBaseRendererTransform)
            {
                return;
            }

            baseRendererLocalPosition = targetRenderer.transform.localPosition;
            baseRendererLocalScale = targetRenderer.transform.localScale;
            hasBaseRendererTransform = true;
        }
    }
}
