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
        private GameplayFeedbackPresenter presenter;

        private void Awake()
        {
            ResolveReferences();
            CreatePresenter();
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
            CreatePresenter();
            if (presenter.TryCreateQuestStateFeedback(quest, state, out GameplayFeedbackMessage message))
            {
                GameplayUIEvents.Show(message.Text, message.MessageType, message.VisibleSeconds, quest);
            }
        }

        private void OnQuestObjectiveProgressChanged(QuestDefinition quest, QuestObjectiveDefinition objective, QuestObjectiveProgress progress)
        {
            CreatePresenter();
            if (presenter.TryCreateObjectiveFeedback(quest, objective, progress, out GameplayFeedbackMessage message))
            {
                GameplayUIEvents.Show(message.Text, message.MessageType, message.VisibleSeconds, quest);
            }
        }

        private void OnQuestRewardClaimed(QuestDefinition quest)
        {
            CreatePresenter();
            if (presenter.TryCreateRewardFeedback(quest, out GameplayFeedbackMessage message))
            {
                GameplayUIEvents.Show(message.Text, message.MessageType, message.VisibleSeconds, quest);
            }
        }

        private void OnItemUseResolved(ItemUseResult result, GameObject source)
        {
            CreatePresenter();
            int sourceId = source != null ? source.GetInstanceID() : 0;
            if (presenter.TryCreateItemUseFeedback(result, sourceId, out GameplayFeedbackMessage message))
            {
                GameplayUIEvents.Show(message.Text, message.MessageType, message.VisibleSeconds, source);
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

        private void CreatePresenter()
        {
            presenter = new GameplayFeedbackPresenter(
                questAcceptedPrefix,
                questCompletedPrefix,
                questRewardPrefix,
                objectiveUpdatedPrefix);
        }
    }
}
