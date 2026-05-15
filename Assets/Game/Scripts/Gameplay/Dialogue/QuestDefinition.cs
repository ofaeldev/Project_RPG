using UnityEngine;

namespace RPGProject.Gameplay
{
    public enum QuestType
    {
        Story,
        Side,
        Fetch,
        Kill,
        Explore,
        Delivery,
        Custom
    }

    public enum QuestObjectiveType
    {
        Kill,
        Collect,
        Talk,
        Explore,
        Custom
    }

    [CreateAssetMenu(menuName = "RPG/Quest/Quest Definition", fileName = "NewQuestDefinition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [Header("Quest")]
        [Tooltip("Identificador unico da missao para progresso e salvamento futuro.")]
        [SerializeField]
        private string questId = string.Empty;

        [Tooltip("Tipo de missao para organizacao e filtros.")]
        [SerializeField]
        private QuestType questType = QuestType.Side;

        [Tooltip("Titulo curto da missao.")]
        [SerializeField]
        private string title = "New Quest";

        [Tooltip("Descricao longa da missao e objetivos.")]
        [TextArea(3, 6)]
        [SerializeField]
        private string description = string.Empty;

        [Tooltip("Texto de recompensa ou incentivo para o jogador.")]
        [TextArea(2, 4)]
        [SerializeField]
        private string rewardDescription = string.Empty;

        [Tooltip("Itens entregues quando a recompensa da missao e resgatada.")]
        [SerializeField]
        private ItemStackDefinition[] rewardItems = new ItemStackDefinition[0];

        [Header("Objectives")]
        [Tooltip("Objetivos que precisam ser completados para concluir a missao.")]
        [SerializeField]
        private QuestObjectiveDefinition[] objectives = new QuestObjectiveDefinition[0];

        [Tooltip("A missao pode ser repetida depois de completada.")]
        [SerializeField]
        private bool isRepeatable;

        public string QuestId => questId;
        public QuestType QuestType => questType;
        public string Title => title;
        public string Description => description;
        public string RewardDescription => rewardDescription;
        public ItemStackDefinition[] RewardItems => rewardItems;
        public QuestObjectiveDefinition[] Objectives => objectives;
        public bool IsRepeatable => isRepeatable;

        public bool HasObjectives => objectives != null && objectives.Length > 0;
        public bool HasRewardItems => rewardItems != null && rewardItems.Length > 0;
    }

    [System.Serializable]
    public sealed class QuestObjectiveDefinition
    {
        [Tooltip("Identificador unico deste objetivo dentro da missao.")]
        [SerializeField]
        private string objectiveId = string.Empty;

        [Tooltip("Tipo de objetivo.")]
        [SerializeField]
        private QuestObjectiveType objectiveType = QuestObjectiveType.Kill;

        [Tooltip("Identificador do alvo contado pelo objetivo. Ex: rat.")]
        [SerializeField]
        private string targetId = string.Empty;

        [Tooltip("Texto exibivel do objetivo.")]
        [SerializeField]
        private string description = string.Empty;

        [Tooltip("Quantidade necessaria para completar o objetivo.")]
        [SerializeField]
        [Min(1)]
        private int requiredAmount = 1;

        public string ObjectiveId => objectiveId;
        public QuestObjectiveType ObjectiveType => objectiveType;
        public string TargetId => targetId;
        public string Description => description;
        public int RequiredAmount => Mathf.Max(1, requiredAmount);
    }
}
