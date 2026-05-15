using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class NPCInteractionTarget : RightClickActionTarget
    {
        [Header("NPC")]
        [Tooltip("Nome do NPC para debug e logs.")]
        [SerializeField]
        private string displayName = "NPC";

        [Tooltip("Distancia maxima para iniciar dialogo.")]
        [SerializeField]
        [Min(0f)]
        private float talkRange = 1.5f;

        [Tooltip("Alvo opcional usado para contabilizar objetivos de missao do tipo Talk.")]
        [SerializeField]
        private string talkObjectiveTargetId = string.Empty;

        [Tooltip("Dialogo padrao. Quando ha quest, normalmente funciona como o dialogo de oferta.")]
        [SerializeField]
        private DialogueDefinition dialogueDefinition;

        [Header("Quest Offer")]
        [Tooltip("Opcional: missao oferecida por este NPC apos o dialogo.")]
        [SerializeField]
        private QuestDefinition questToOffer;

        [Tooltip("Caso verdadeiro, a missao sera aceita automaticamente depois do dialogo de oferta.")]
        [SerializeField]
        private bool startQuestAfterDialogue;

        [Header("Quest Dialogue Overrides")]
        [Tooltip("Dialogo usado quando a missao ja foi aceita e ainda esta ativa.")]
        [SerializeField]
        private DialogueDefinition activeQuestDialogueDefinition;

        [Tooltip("Dialogo usado quando a missao ja foi concluida.")]
        [SerializeField]
        private DialogueDefinition completedQuestDialogueDefinition;

        [Tooltip("Dialogo usado depois que a recompensa ja foi recebida.")]
        [SerializeField]
        private DialogueDefinition rewardClaimedQuestDialogueDefinition;

        [Tooltip("Dialogo usado quando a missao falhou.")]
        [SerializeField]
        private DialogueDefinition failedQuestDialogueDefinition;

        public string DisplayName => displayName;
        public QuestDefinition QuestToOffer => questToOffer;

        private void OnEnable()
        {
            RegisterQuestDefinition();
        }

        private void Start()
        {
            RegisterQuestDefinition();
        }

        public override RightClickActionType GetPreferredRightClickAction(Vector2 clickPosition)
        {
            return RightClickActionType.Talk;
        }

        public override float GetActionRange(RightClickActionType actionType)
        {
            return actionType == RightClickActionType.Talk ? talkRange : 0f;
        }

        public override void PerformRightClickAction(RightClickActionContext context)
        {
            if (context.ActionType != RightClickActionType.Talk)
            {
                Debug.LogWarning($"Action '{context.ActionType}' is not supported by NPC '{displayName}'.", gameObject);
                return;
            }

            ReportTalkObjectiveProgress();

            if (DialogueManager.Instance == null)
            {
                Debug.LogWarning($"DialogueManager is not present in the scene. Cannot start dialogue with '{displayName}'.", gameObject);
                return;
            }

            DialogueDefinition selectedDialogue = ResolveDialogueDefinition(out QuestDefinition questToStart);
            if (selectedDialogue == null)
            {
                Debug.Log($"NPC '{displayName}' does not have a DialogueDefinition configured.", gameObject);
                return;
            }

            DialogueManager.Instance.StartDialogue(selectedDialogue, gameObject, questToStart);
        }

        private DialogueDefinition ResolveDialogueDefinition(out QuestDefinition questToStart)
        {
            questToStart = null;

            if (questToOffer == null || QuestManager.Instance == null || string.IsNullOrWhiteSpace(questToOffer.QuestId))
            {
                questToStart = startQuestAfterDialogue ? questToOffer : null;
                return dialogueDefinition;
            }

            QuestState questState = QuestManager.Instance.GetQuestState(questToOffer.QuestId);
            switch (questState)
            {
                case QuestState.Active:
                    return activeQuestDialogueDefinition != null ? activeQuestDialogueDefinition : dialogueDefinition;
                case QuestState.Completed:
                    QuestManager.Instance.TryClaimReward(questToOffer);
                    return completedQuestDialogueDefinition != null ? completedQuestDialogueDefinition : dialogueDefinition;
                case QuestState.RewardClaimed:
                    return rewardClaimedQuestDialogueDefinition != null ? rewardClaimedQuestDialogueDefinition : completedQuestDialogueDefinition != null ? completedQuestDialogueDefinition : dialogueDefinition;
                case QuestState.Failed:
                    return failedQuestDialogueDefinition != null ? failedQuestDialogueDefinition : dialogueDefinition;
                case QuestState.Locked:
                case QuestState.Available:
                default:
                    questToStart = startQuestAfterDialogue ? questToOffer : null;
                    return dialogueDefinition;
            }
        }

        private void RegisterQuestDefinition()
        {
            if (questToOffer != null && QuestManager.Instance != null)
            {
                QuestManager.Instance.RegisterQuestDefinition(questToOffer);
            }
        }

        private void ReportTalkObjectiveProgress()
        {
            if (!string.IsNullOrWhiteSpace(talkObjectiveTargetId) && QuestManager.Instance != null)
            {
                QuestManager.Instance.ReportObjectiveProgress(QuestObjectiveType.Talk, talkObjectiveTargetId);
            }
        }
    }
}
