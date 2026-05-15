using System;
using System.Collections.Generic;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public sealed class QuestLogProjection
    {
        private readonly Func<string, QuestDefinition> questDefinitionResolver;

        public QuestLogProjection(Func<string, QuestDefinition> questDefinitionResolver)
        {
            this.questDefinitionResolver = questDefinitionResolver;
        }

        public IReadOnlyList<QuestLogEntry> CreateEntries(
            IEnumerable<QuestProgress> questProgress,
            bool includeRewardClaimed = true,
            bool includeFailed = true)
        {
            var entries = new List<QuestLogEntry>();
            if (questProgress == null)
            {
                return entries;
            }

            foreach (QuestProgress progress in questProgress)
            {
                if (progress == null || !ShouldIncludeQuestInLog(progress.State, includeRewardClaimed, includeFailed))
                {
                    continue;
                }

                QuestDefinition quest = questDefinitionResolver?.Invoke(progress.QuestId);
                entries.Add(CreateQuestLogEntry(progress, quest));
            }

            return entries;
        }

        private static QuestLogEntry CreateQuestLogEntry(QuestProgress progress, QuestDefinition quest)
        {
            string questId = quest != null ? quest.QuestId : progress.QuestId;
            string title = quest != null && !string.IsNullOrWhiteSpace(quest.Title) ? quest.Title : questId;
            string description = quest != null ? quest.Description : string.Empty;
            string rewardDescription = quest != null ? quest.RewardDescription : string.Empty;

            var objectives = new List<QuestObjectiveLogEntry>();
            if (quest != null && quest.HasObjectives)
            {
                foreach (QuestObjectiveDefinition objective in quest.Objectives)
                {
                    if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
                    {
                        continue;
                    }

                    QuestObjectiveProgress objectiveProgress = progress.GetObjectiveProgress(objective.ObjectiveId);
                    objectives.Add(new QuestObjectiveLogEntry(
                        objective.ObjectiveId,
                        objective.Description,
                        objectiveProgress != null ? objectiveProgress.CurrentAmount : 0,
                        objective.RequiredAmount));
                }
            }
            else
            {
                foreach (QuestObjectiveProgress objectiveProgress in progress.Objectives)
                {
                    objectives.Add(new QuestObjectiveLogEntry(
                        objectiveProgress.ObjectiveId,
                        objectiveProgress.ObjectiveId,
                        objectiveProgress.CurrentAmount,
                        objectiveProgress.RequiredAmount));
                }
            }

            return new QuestLogEntry(questId, title, description, rewardDescription, progress.State, objectives);
        }

        private static bool ShouldIncludeQuestInLog(QuestState state, bool includeRewardClaimed, bool includeFailed)
        {
            return state == QuestState.Active
                || state == QuestState.Completed
                || (includeRewardClaimed && state == QuestState.RewardClaimed)
                || (includeFailed && state == QuestState.Failed);
        }
    }
}
