using System;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    /// <summary>
    /// Centralizes attack execution for any actor. Input, AI, animation, and UI should decide
    /// what to do around this component instead of duplicating damage/cooldown rules.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class CombatActor : MonoBehaviour, ICombatStatsProvider
    {
        [Header("Base Stats")]
        [SerializeField]
        private CombatStatsDefinition baseStats;

        [SerializeField]
        [Min(0)]
        private int fallbackAttack = 1;

        [SerializeField]
        [Min(0)]
        private int fallbackDefense;

        [Header("Attack")]
        [SerializeField]
        private CombatAttackSettings attackSettings;

        [SerializeField]
        [Min(0f)]
        private float fallbackAttackRange = 1.5f;

        [SerializeField]
        [Min(1)]
        private int fallbackDamage = 5;

        [SerializeField]
        [Min(0.01f)]
        private float fallbackAttackInterval = 1f;

        [Header("Targeting")]
        [SerializeField]
        private bool selectTargets = true;

        private HealthComponent health;
        private CombatActor currentTargetActor;
        private HealthComponent currentTargetHealth;
        private CombatTarget currentTarget;
        private bool isSelected;
        private float nextAttackTime;

        public event Action<CombatActor, HealthComponent, HealthComponent> TargetChanged;
        public event Action<CombatActor, HealthComponent, DamageResult> AttackResolved;
        public event Action<CombatActor, HealthComponent> AttackOutOfRange;
        public event Action<CombatActor, HealthComponent, HealthChange> TargetDefeated;
        public event Action<CombatActor> Selected;
        public event Action<CombatActor> Deselected;
        public event Action<CombatActor, HealthChange> Defeated;

        public HealthComponent Health => ResolveHealth();
        public int Attack => baseStats != null ? baseStats.Attack : fallbackAttack;
        public int Defense => baseStats != null ? baseStats.Defense : fallbackDefense;
        public CombatAttackSettings AttackSettings => attackSettings;
        public CombatActor CurrentTargetActor => currentTargetActor;
        public HealthComponent CurrentTargetHealth => currentTargetHealth;
        public CombatTarget CurrentTarget => currentTarget;
        public bool IsSelected => isSelected;
        public bool CanBeAttacked => isActiveAndEnabled && ResolveHealth() != null && !health.IsDead;
        public bool HasTarget => currentTargetHealth != null;
        public bool HasValidTarget => currentTargetHealth != null && !currentTargetHealth.IsDead;
        public float AttackRange => attackSettings != null ? attackSettings.AttackRange : fallbackAttackRange;
        public float AttackInterval => attackSettings != null ? attackSettings.AttackInterval : fallbackAttackInterval;

        public void Configure(
            CombatAttackSettings settings,
            float fallbackRange,
            int fallbackBaseDamage,
            float fallbackInterval,
            bool shouldSelectTargets)
        {
            attackSettings = settings;
            fallbackAttackRange = Mathf.Max(0f, fallbackRange);
            fallbackDamage = Mathf.Max(1, fallbackBaseDamage);
            fallbackAttackInterval = Mathf.Max(0.01f, fallbackInterval);
            selectTargets = shouldSelectTargets;
        }

        public void SetTarget(CombatActor target)
        {
            SetTarget(
                target != null ? target.Health : null,
                target != null ? target.GetComponent<CombatTarget>() : null,
                target);
        }

        public void SetTarget(CombatTarget target)
        {
            SetTarget(target != null ? target.Health : null, target, null);
        }

        public void SetTarget(HealthComponent targetHealth)
        {
            SetTarget(
                targetHealth,
                targetHealth != null ? targetHealth.GetComponent<CombatTarget>() : null,
                targetHealth != null ? targetHealth.GetComponent<CombatActor>() : null);
        }

        public void ClearTarget()
        {
            if (currentTargetHealth == null && currentTarget == null && currentTargetActor == null)
            {
                return;
            }

            HealthComponent previousTargetHealth = currentTargetHealth;
            UnsubscribeFromCurrentTarget();
            SetTargetSelected(false);
            currentTargetHealth = null;
            currentTarget = null;
            currentTargetActor = null;
            nextAttackTime = 0f;
            TargetChanged?.Invoke(this, previousTargetHealth, null);
        }

        public bool IsCurrentTargetInRange()
        {
            if (!HasValidTarget)
            {
                return false;
            }

            return IsTargetInRange(currentTargetHealth);
        }

        public bool IsTargetInRange(HealthComponent targetHealth)
        {
            if (targetHealth == null)
            {
                return false;
            }

            float attackRange = AttackRange;
            Vector2 currentPosition = transform.position;
            Vector2 targetPosition = targetHealth.transform.position;
            return Vector2.SqrMagnitude(targetPosition - currentPosition) <= attackRange * attackRange;
        }

        public bool TryAttackCurrentTarget()
        {
            if (!HasValidTarget)
            {
                ClearTarget();
                return false;
            }

            HealthComponent targetHealth = currentTargetHealth;
            if (!IsTargetInRange(targetHealth))
            {
                AttackOutOfRange?.Invoke(this, targetHealth);
                return false;
            }

            if (Time.time < nextAttackTime)
            {
                return false;
            }

            DamageResult damage = ResolveDamage(targetHealth);
            if (damage.HasDamage)
            {
                targetHealth.ApplyDamage(damage.Amount, this);
            }

            nextAttackTime = Time.time + AttackInterval;
            AttackResolved?.Invoke(this, targetHealth, damage);
            return true;
        }

        public void SetSelected(bool selected)
        {
            if (isSelected == selected)
            {
                return;
            }

            isSelected = selected;

            if (isSelected)
            {
                Selected?.Invoke(this);
            }
            else
            {
                Deselected?.Invoke(this);
            }
        }

        private void Awake()
        {
            ResolveHealth();
        }

        private void OnEnable()
        {
            ResolveHealth();

            if (health != null)
            {
                health.Died += OnSelfDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnSelfDied;
            }

            ClearTarget();
            SetSelected(false);
        }

        private void SetTarget(HealthComponent targetHealth, CombatTarget target, CombatActor targetActor)
        {
            if (targetHealth == currentTargetHealth && target == currentTarget && targetActor == currentTargetActor)
            {
                return;
            }

            HealthComponent previousTargetHealth = currentTargetHealth;
            UnsubscribeFromCurrentTarget();
            SetTargetSelected(false);

            currentTargetHealth = targetHealth;
            currentTarget = target;
            currentTargetActor = targetActor;
            nextAttackTime = 0f;

            if (currentTargetHealth != null)
            {
                currentTargetHealth.Died += OnTargetDied;
            }

            SetTargetSelected(true);
            TargetChanged?.Invoke(this, previousTargetHealth, currentTargetHealth);
        }

        private DamageResult ResolveDamage(HealthComponent targetHealth)
        {
            CombatTarget target = currentTarget != null
                ? currentTarget
                : targetHealth != null
                    ? targetHealth.GetComponent<CombatTarget>()
                    : null;

            if (attackSettings != null && attackSettings.DamageResolver != null)
            {
                return attackSettings.DamageResolver.ResolveDamage(new DamageContext(gameObject, targetHealth, attackSettings));
            }

            GameObject targetObject = target != null
                ? target.gameObject
                : targetHealth != null
                    ? targetHealth.gameObject
                    : null;
            int baseDamage = attackSettings != null ? attackSettings.Damage : fallbackDamage;
            return BasicDamageResolver.ResolveBasicDamage(gameObject, targetObject, baseDamage);
        }

        private void OnTargetDied(HealthChange change)
        {
            HealthComponent defeatedTarget = currentTargetHealth;
            TargetDefeated?.Invoke(this, defeatedTarget, change);
            ClearTarget();
        }

        private void UnsubscribeFromCurrentTarget()
        {
            if (currentTargetHealth != null)
            {
                currentTargetHealth.Died -= OnTargetDied;
            }
        }

        private void SetTargetSelected(bool selected)
        {
            if (!selectTargets)
            {
                return;
            }

            if (currentTargetActor != null)
            {
                currentTargetActor.SetSelected(selected);
                return;
            }

            if (currentTarget != null)
            {
                currentTarget.SetSelected(selected);
            }
        }

        private HealthComponent ResolveHealth()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            return health;
        }

        private void OnSelfDied(HealthChange change)
        {
            SetSelected(false);
            Defeated?.Invoke(this, change);
        }
    }
}
