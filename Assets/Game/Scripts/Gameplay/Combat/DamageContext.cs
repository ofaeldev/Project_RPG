using UnityEngine;

namespace RPGProject.Gameplay
{
    public readonly struct DamageContext
    {
        public DamageContext(GameObject attacker, CombatTarget target, CombatAttackSettings attackSettings)
        {
            Attacker = attacker;
            Target = target;
            TargetHealth = target != null ? target.Health : null;
            AttackSettings = attackSettings;
        }

        public DamageContext(GameObject attacker, HealthComponent targetHealth, CombatAttackSettings attackSettings)
        {
            Attacker = attacker;
            Target = targetHealth != null ? targetHealth.GetComponent<CombatTarget>() : null;
            TargetHealth = targetHealth;
            AttackSettings = attackSettings;
        }

        public GameObject Attacker { get; }
        public CombatTarget Target { get; }
        public HealthComponent TargetHealth { get; }
        public CombatAttackSettings AttackSettings { get; }
        public GameObject TargetObject => Target != null
            ? Target.gameObject
            : TargetHealth != null
                ? TargetHealth.gameObject
                : null;
    }
}
