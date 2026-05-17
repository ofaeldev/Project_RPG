using UnityEngine;
using UnityEngine.Serialization;

namespace RPGProject.Gameplay
{
    [CreateAssetMenu(
        fileName = "CombatAttackSettings",
        menuName = "RPG Project/Combat/Attack Settings")]
    public sealed class CombatAttackSettings : ScriptableObject
    {
        [Header("Basic Attack")]
        [SerializeField]
        [Min(0f)]
        private float attackRange = 1.5f;

        [SerializeField]
        [Min(1)]
        [FormerlySerializedAs("damage")]
        private int baseDamage = 5;

        [SerializeField]
        [Min(0.01f)]
        private float attacksPerSecond = 1f;

        [SerializeField]
        private DamageResolver damageResolver;

        public float AttackRange => attackRange;
        public int BaseDamage => baseDamage;
        public int Damage => BaseDamage;
        public float AttackInterval => 1f / Mathf.Max(0.01f, attacksPerSecond);
        public DamageResolver DamageResolver => damageResolver;
    }
}
