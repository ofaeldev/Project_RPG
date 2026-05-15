using UnityEngine;

namespace RPGProject.Gameplay
{
    /// <summary>
    /// Runtime combat attributes for any creature that can deal or receive combat damage.
    /// Keep formulas outside this component so future buffs, gear, and classes can compose here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterCombatStats : MonoBehaviour, ICombatStatsProvider
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

        public int Attack => baseStats != null ? baseStats.Attack : fallbackAttack;
        public int Defense => baseStats != null ? baseStats.Defense : fallbackDefense;

        public static int GetAttack(GameObject actor)
        {
            return actor != null && actor.TryGetComponent(out ICombatStatsProvider stats)
                ? stats.Attack
                : 0;
        }

        public static int GetDefense(GameObject actor)
        {
            return actor != null && actor.TryGetComponent(out ICombatStatsProvider stats)
                ? stats.Defense
                : 0;
        }
    }
}
