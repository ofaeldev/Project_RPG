using UnityEngine;

namespace RPGProject.Gameplay
{
    [CreateAssetMenu(
        fileName = "CreatureDefinition",
        menuName = "RPG Project/Creatures/Creature Definition")]
    public sealed class CreatureDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string creatureId = string.Empty;

        [SerializeField]
        private string displayName = "New Creature";

        [TextArea(2, 5)]
        [SerializeField]
        private string description = string.Empty;

        [Header("Combat")]
        [SerializeField]
        [Min(1)]
        private int maximumHealth = 20;

        [SerializeField]
        private CombatStatsDefinition combatStats;

        [SerializeField]
        private CombatAttackSettings attackSettings;

        [SerializeField]
        private EnemyCombatBehaviorSettings behaviorSettings;

        [Header("Rewards")]
        [SerializeField]
        [Min(0)]
        private int experienceReward;

        [SerializeField]
        private LootTableDefinition lootTable;

        public string CreatureId => creatureId;
        public string DisplayName => displayName;
        public string Description => description;
        public int MaximumHealth => Mathf.Max(1, maximumHealth);
        public CombatStatsDefinition CombatStats => combatStats;
        public CombatAttackSettings AttackSettings => attackSettings;
        public EnemyCombatBehaviorSettings BehaviorSettings => behaviorSettings;
        public int ExperienceReward => Mathf.Max(0, experienceReward);
        public LootTableDefinition LootTable => lootTable;
        public bool HasValidId => !string.IsNullOrWhiteSpace(creatureId);
    }
}
