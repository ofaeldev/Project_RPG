using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public sealed class GameplayFeedbackPresenter
    {
        private readonly string questAcceptedPrefix;
        private readonly string questCompletedPrefix;
        private readonly string questRewardPrefix;
        private readonly string objectiveUpdatedPrefix;

        public GameplayFeedbackPresenter(
            string questAcceptedPrefix,
            string questCompletedPrefix,
            string questRewardPrefix,
            string objectiveUpdatedPrefix)
        {
            this.questAcceptedPrefix = questAcceptedPrefix;
            this.questCompletedPrefix = questCompletedPrefix;
            this.questRewardPrefix = questRewardPrefix;
            this.objectiveUpdatedPrefix = objectiveUpdatedPrefix;
        }

        public bool TryCreateQuestStateFeedback(QuestDefinition quest, QuestState state, out GameplayFeedbackMessage message)
        {
            message = default;
            if (quest == null)
            {
                return false;
            }

            switch (state)
            {
                case QuestState.Active:
                    message = new GameplayFeedbackMessage($"{questAcceptedPrefix}: {quest.Title}", FeedbackMessageType.Quest, -1f, quest.GetInstanceID());
                    return true;
                case QuestState.Completed:
                    message = new GameplayFeedbackMessage($"{questCompletedPrefix}: {quest.Title}", FeedbackMessageType.Quest, -1f, quest.GetInstanceID());
                    return true;
                case QuestState.Failed:
                    message = new GameplayFeedbackMessage($"Quest falhou: {quest.Title}", FeedbackMessageType.Warning, -1f, quest.GetInstanceID());
                    return true;
                default:
                    return false;
            }
        }

        public bool TryCreateObjectiveFeedback(
            QuestDefinition quest,
            QuestObjectiveDefinition objective,
            QuestObjectiveProgress progress,
            out GameplayFeedbackMessage message)
        {
            message = default;
            if (quest == null || objective == null || progress == null)
            {
                return false;
            }

            string objectiveText = string.IsNullOrWhiteSpace(objective.Description) ? objective.ObjectiveId : objective.Description;
            message = new GameplayFeedbackMessage(
                $"{objectiveUpdatedPrefix}: {objectiveText} {progress.CurrentAmount}/{progress.RequiredAmount}",
                FeedbackMessageType.Quest,
                -1f,
                quest.GetInstanceID());
            return true;
        }

        public bool TryCreateRewardFeedback(QuestDefinition quest, out GameplayFeedbackMessage message)
        {
            message = default;
            if (quest == null)
            {
                return false;
            }

            string reward = string.IsNullOrWhiteSpace(quest.RewardDescription) ? quest.Title : quest.RewardDescription;
            message = new GameplayFeedbackMessage($"{questRewardPrefix}: {reward}", FeedbackMessageType.Success, -1f, quest.GetInstanceID());
            return true;
        }

        public bool TryCreateItemUseFeedback(ItemUseResult result, int sourceId, out GameplayFeedbackMessage message)
        {
            message = default;
            if (string.IsNullOrWhiteSpace(result.FeedbackMessage))
            {
                return false;
            }

            message = new GameplayFeedbackMessage(
                result.FeedbackMessage,
                result.WasUsed ? FeedbackMessageType.Success : FeedbackMessageType.Warning,
                -1f,
                sourceId);
            return true;
        }
    }
}
