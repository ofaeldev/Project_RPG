using UnityEngine;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class GameplayUIManager : MonoBehaviour
    {
        [SerializeField]
        private GlobalFeedbackUIController feedbackController;

        [Header("Quest Feedback")]
        [SerializeField]
        private string questAcceptedPrefix = "Quest aceita";

        [SerializeField]
        private string questCompletedPrefix = "Quest concluida";

        [SerializeField]
        private string questRewardPrefix = "Recompensa recebida";

        [SerializeField]
        private string objectiveUpdatedPrefix = "Objetivo atualizado";

        private bool questEventsSubscribed;
        private bool inventoryEventsSubscribed;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            GameplayUIEvents.FeedbackRequested += OnFeedbackRequested;
            SubscribeGameplayEvents();
        }

        private void OnDisable()
        {
            GameplayUIEvents.FeedbackRequested -= OnFeedbackRequested;
            UnsubscribeGameplayEvents();
        }

        private void Start()
        {
            SubscribeGameplayEvents();
        }

        private void OnFeedbackRequested(string message, FeedbackMessageType messageType, float visibleSeconds, Object source)
        {
            ResolveReferences();
            feedbackController?.Show(message, messageType, visibleSeconds, source);
        }

        private void SubscribeGameplayEvents()
        {
            if (!questEventsSubscribed && QuestManager.Instance != null)
            {
                QuestManager.Instance.QuestStateChanged += OnQuestStateChanged;
                QuestManager.Instance.QuestObjectiveProgressChanged += OnQuestObjectiveProgressChanged;
                QuestManager.Instance.QuestRewardClaimed += OnQuestRewardClaimed;
                questEventsSubscribed = true;
            }

            if (!inventoryEventsSubscribed && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ItemUseResolved += OnItemUseResolved;
                inventoryEventsSubscribed = true;
            }
        }

        private void UnsubscribeGameplayEvents()
        {
            if (questEventsSubscribed && QuestManager.Instance != null)
            {
                QuestManager.Instance.QuestStateChanged -= OnQuestStateChanged;
                QuestManager.Instance.QuestObjectiveProgressChanged -= OnQuestObjectiveProgressChanged;
                QuestManager.Instance.QuestRewardClaimed -= OnQuestRewardClaimed;
            }

            if (inventoryEventsSubscribed && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ItemUseResolved -= OnItemUseResolved;
            }

            questEventsSubscribed = false;
            inventoryEventsSubscribed = false;
        }

        private void OnQuestStateChanged(QuestDefinition quest, QuestState state)
        {
            if (quest == null)
            {
                return;
            }

            switch (state)
            {
                case QuestState.Active:
                    GameplayUIEvents.ShowQuest($"{questAcceptedPrefix}: {quest.Title}", source: quest);
                    break;
                case QuestState.Completed:
                    GameplayUIEvents.ShowQuest($"{questCompletedPrefix}: {quest.Title}", source: quest);
                    break;
                case QuestState.Failed:
                    GameplayUIEvents.ShowWarning($"Quest falhou: {quest.Title}", source: quest);
                    break;
            }
        }

        private void OnQuestObjectiveProgressChanged(QuestDefinition quest, QuestObjectiveDefinition objective, QuestObjectiveProgress progress)
        {
            if (quest == null || objective == null || progress == null)
            {
                return;
            }

            string objectiveText = string.IsNullOrWhiteSpace(objective.Description) ? objective.ObjectiveId : objective.Description;
            GameplayUIEvents.ShowQuest($"{objectiveUpdatedPrefix}: {objectiveText} {progress.CurrentAmount}/{progress.RequiredAmount}", source: quest);
        }

        private void OnQuestRewardClaimed(QuestDefinition quest)
        {
            if (quest == null)
            {
                return;
            }

            string reward = string.IsNullOrWhiteSpace(quest.RewardDescription) ? quest.Title : quest.RewardDescription;
            GameplayUIEvents.ShowSuccess($"{questRewardPrefix}: {reward}", source: quest);
        }

        private static void OnItemUseResolved(ItemUseResult result, GameObject source)
        {
            if (string.IsNullOrWhiteSpace(result.FeedbackMessage))
            {
                return;
            }

            if (result.WasUsed)
            {
                GameplayUIEvents.ShowSuccess(result.FeedbackMessage, source: source);
            }
            else
            {
                GameplayUIEvents.ShowWarning(result.FeedbackMessage, source: source);
            }
        }

        private void ResolveReferences()
        {
            if (feedbackController == null)
            {
                feedbackController = GetComponent<GlobalFeedbackUIController>();
            }

            if (feedbackController == null)
            {
                feedbackController = GlobalFeedbackUIController.Instance;
            }
        }
    }
}
