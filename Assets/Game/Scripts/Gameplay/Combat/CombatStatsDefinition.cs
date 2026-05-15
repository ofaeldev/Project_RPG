using UnityEngine;

namespace RPGProject.Gameplay
{
    [CreateAssetMenu(
        fileName = "CombatStatsDefinition",
        menuName = "RPG Project/Combat/Stats Definition")]
    public sealed class CombatStatsDefinition : ScriptableObject
    {
        [Header("Attributes")]
        [SerializeField]
        [Min(0)]
        private int attack = 1;

        [SerializeField]
        [Min(0)]
        private int defense;

        public int Attack => attack;
        public int Defense => defense;
    }
}
