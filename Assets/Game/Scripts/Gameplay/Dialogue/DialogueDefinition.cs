using System.Collections.Generic;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    public enum DialogueChoiceAction
    {
        Continue,
        AcceptQuest,
        DeclineQuest,
        Close
    }

    public enum DialogueSpeakerSide
    {
        Left,
        Right
    }

    public enum DialogueConditionType
    {
        QuestState,
        HasItem
    }

    [CreateAssetMenu(menuName = "RPG/Dialogue/Dialogue Definition", fileName = "NewDialogueDefinition")]
    public sealed class DialogueDefinition : ScriptableObject
    {
        [Header("Dialogue")]
        [Tooltip("Identificador unico do dialogo para progresso e salvamento futuro.")]
        [SerializeField]
        private string dialogueId = string.Empty;

        [Tooltip("Nome exibido do dialogo ou NPC que fala.")]
        [SerializeField]
        private string displayName = "Dialogue";

        [Tooltip("Permite que o mesmo dialogo seja repetido mais de uma vez.")]
        [SerializeField]
        private bool isRepeatable = true;

        [Tooltip("Linhas que fazem parte deste dialogo.")]
        [SerializeField]
        private DialogueLine[] lines = new DialogueLine[0];

        [Header("Conditions")]
        [Tooltip("Condicoes que precisam ser verdadeiras para este dialogo poder iniciar.")]
        [SerializeField]
        private DialogueCondition[] conditions = new DialogueCondition[0];

        [Header("Choices")]
        [Tooltip("Escolhas exibidas ao final do dialogo. Se vazio, o dialogo fecha normalmente.")]
        [SerializeField]
        private DialogueChoice[] choices = new DialogueChoice[0];

        [Header("Quest Link")]
        [Tooltip("Missao que sera ativada automaticamente apos a conclusao deste dialogo.")]
        [SerializeField]
        private QuestDefinition questToStart;

        [Tooltip("Comecar a missao relacionada assim que o dialogo terminar.")]
        [SerializeField]
        private bool startQuestAfterDialogue;

        public string DialogueId => dialogueId;
        public string DisplayName => displayName;
        public bool IsRepeatable => isRepeatable;
        public IReadOnlyList<DialogueLine> Lines => lines;
        public IReadOnlyList<DialogueCondition> Conditions => conditions;
        public IReadOnlyList<DialogueChoice> Choices => choices;
        public QuestDefinition QuestToStart => questToStart;
        public bool StartQuestAfterDialogue => startQuestAfterDialogue;

        public bool HasLines => lines != null && lines.Length > 0;
        public bool HasChoices => choices != null && choices.Length > 0;
        public bool AreConditionsMet => DialogueCondition.AreAllMet(conditions);
    }

    [System.Serializable]
    public sealed class DialogueCondition
    {
        [Tooltip("Quest usada pela condicao.")]
        [SerializeField]
        private QuestDefinition quest;

        [SerializeField]
        private DialogueConditionType conditionType = DialogueConditionType.QuestState;

        [Tooltip("Estado esperado dessa quest.")]
        [SerializeField]
        private QuestState requiredState = QuestState.Active;

        [Tooltip("Item exigido quando a condicao e do tipo HasItem.")]
        [SerializeField]
        private InventoryRequirement itemRequirement = new();

        [Tooltip("Inverte o resultado da condicao.")]
        [SerializeField]
        private bool invert;

        public QuestDefinition Quest => quest;
        public DialogueConditionType ConditionType => conditionType;
        public QuestState RequiredState => requiredState;
        public InventoryRequirement ItemRequirement => itemRequirement;
        public bool Invert => invert;

        public bool IsMet()
        {
            bool result = conditionType switch
            {
                DialogueConditionType.HasItem => itemRequirement != null && itemRequirement.IsMet(),
                _ => quest != null
                    && QuestManager.Instance != null
                    && QuestManager.Instance.GetQuestState(quest.QuestId) == requiredState,
            };

            return invert ? !result : result;
        }

        public static bool AreAllMet(IReadOnlyList<DialogueCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                DialogueCondition condition = conditions[i];
                if (condition != null && !condition.IsMet())
                {
                    return false;
                }
            }

            return true;
        }
    }

    [System.Serializable]
    public sealed class DialogueLine
    {
        [Tooltip("Nome do personagem que diz esta linha.")]
        [SerializeField]
        private string speakerName = string.Empty;

        [Tooltip("Lado do retrato usado por esta fala. Esquerda normalmente e NPC, direita normalmente e jogador.")]
        [SerializeField]
        private DialogueSpeakerSide speakerSide = DialogueSpeakerSide.Left;

        [Tooltip("Retrato exibido nesta fala. Se vazio, a UI mantem o placeholder/retrato atual daquele lado.")]
        [SerializeField]
        private Sprite portrait;

        [Tooltip("Texto dessa linha de dialogo.")]
        [TextArea(2, 5)]
        [SerializeField]
        private string text = string.Empty;

        public string SpeakerName => speakerName;
        public DialogueSpeakerSide SpeakerSide => speakerSide;
        public Sprite Portrait => portrait;
        public string Text => text;
    }

    [System.Serializable]
    public sealed class DialogueChoice
    {
        [Tooltip("Texto exibido no botao de escolha.")]
        [SerializeField]
        private string text = "Continue";

        [Tooltip("Acao executada quando o jogador escolhe esta opcao.")]
        [SerializeField]
        private DialogueChoiceAction action = DialogueChoiceAction.Continue;

        [Tooltip("Dialogo opcional que sera iniciado depois desta escolha.")]
        [SerializeField]
        private DialogueDefinition nextDialogue;

        [Tooltip("Condicoes opcionais para esta escolha aparecer.")]
        [SerializeField]
        private DialogueCondition[] conditions = new DialogueCondition[0];

        public string Text => text;
        public DialogueChoiceAction Action => action;
        public DialogueDefinition NextDialogue => nextDialogue;
        public IReadOnlyList<DialogueCondition> Conditions => conditions;
        public bool AreConditionsMet => DialogueCondition.AreAllMet(conditions);
    }
}
