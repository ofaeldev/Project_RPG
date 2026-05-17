using UnityEngine;

namespace RPGProject.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class CreatureIdentity : MonoBehaviour
    {
        [SerializeField]
        private CreatureDefinition creatureDefinition;

        [SerializeField]
        [Min(0)]
        private int displayIndex;

        [SerializeField]
        private string fallbackDisplayName = "Creature";

        public CreatureDefinition Definition => creatureDefinition;

        public string DisplayName
        {
            get
            {
                string baseName = creatureDefinition != null && !string.IsNullOrWhiteSpace(creatureDefinition.DisplayName)
                    ? creatureDefinition.DisplayName
                    : fallbackDisplayName;

                return displayIndex > 0 ? $"{baseName} {displayIndex}" : baseName;
            }
        }
    }
}
