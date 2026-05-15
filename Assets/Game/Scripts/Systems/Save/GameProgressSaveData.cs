using System;

namespace RPGProject.Systems
{
    [Serializable]
    public sealed class GameProgressSaveData
    {
        public QuestProgressSnapshot[] quests = Array.Empty<QuestProgressSnapshot>();
        public DialogueProgressSnapshot[] dialogues = Array.Empty<DialogueProgressSnapshot>();
        public InventoryItemSnapshot[] inventory = Array.Empty<InventoryItemSnapshot>();
        public WorldObjectStateSnapshot[] worldObjects = Array.Empty<WorldObjectStateSnapshot>();
    }
}
