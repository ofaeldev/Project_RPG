using System;
using System.Collections.Generic;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    public enum QuestState
    {
        Locked,
        Available,
        Active,
        Completed,
        RewardClaimed,
        Failed
    }

    public sealed class QuestObjectiveProgress
    {
        public string ObjectiveId { get; }
        public int CurrentAmount { get; private set; }
        public int RequiredAmount { get; private set; }
        public bool IsComplete => CurrentAmount >= RequiredAmount;

        public QuestObjectiveProgress(string objectiveId, int currentAmount, int requiredAmount)
        {
            ObjectiveId = objectiveId;
            RequiredAmount = Mathf.Max(1, requiredAmount);
            CurrentAmount = Mathf.Clamp(currentAmount, 0, RequiredAmount);
        }

        public bool AddAmount(int amount)
        {
            if (amount <= 0 || IsComplete)
            {
                return false;
            }

            int previousAmount = CurrentAmount;
            CurrentAmount = Mathf.Clamp(CurrentAmount + amount, 0, RequiredAmount);
            return CurrentAmount != previousAmount;
        }
    }

    public sealed class QuestProgress
    {
        private readonly Dictionary<string, QuestObjectiveProgress> objectivesById = new();

        public string QuestId { get; }
        public QuestState State { get; private set; }
        public IEnumerable<QuestObjectiveProgress> Objectives => objectivesById.Values;

        public QuestProgress(string questId)
            : this(questId, QuestState.Available)
        {
        }

        public QuestProgress(string questId, QuestState state)
        {
            QuestId = questId;
            State = state;
        }

        public bool Start(QuestDefinition quest)
        {
            if (State == QuestState.Active)
            {
                return false;
            }

            if ((State == QuestState.Completed || State == QuestState.RewardClaimed) && !quest.IsRepeatable)
            {
                return false;
            }

            State = QuestState.Active;
            SyncObjectives(quest, resetCompletedQuest: quest.IsRepeatable);
            return true;
        }

        public bool Complete()
        {
            if (State != QuestState.Active && State != QuestState.Available)
            {
                return false;
            }

            State = QuestState.Completed;
            return true;
        }

        public bool ClaimReward()
        {
            if (State != QuestState.Completed)
            {
                return false;
            }

            State = QuestState.RewardClaimed;
            return true;
        }

        public bool Fail()
        {
            if (State == QuestState.Failed)
            {
                return false;
            }

            State = QuestState.Failed;
            return true;
        }

        public bool SetState(QuestState state)
        {
            if (State == state)
            {
                return false;
            }

            State = state;
            return true;
        }

        public QuestObjectiveProgress GetObjectiveProgress(string objectiveId)
        {
            return !string.IsNullOrWhiteSpace(objectiveId) && objectivesById.TryGetValue(objectiveId, out QuestObjectiveProgress progress)
                ? progress
                : null;
        }

        public bool AddObjectiveProgress(QuestObjectiveDefinition objective, int amount, out QuestObjectiveProgress progress)
        {
            progress = null;
            if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
            {
                return false;
            }

            if (!objectivesById.TryGetValue(objective.ObjectiveId, out progress))
            {
                progress = new QuestObjectiveProgress(objective.ObjectiveId, 0, objective.RequiredAmount);
                objectivesById[objective.ObjectiveId] = progress;
            }

            return progress.AddAmount(amount);
        }

        public bool AreObjectivesComplete(QuestDefinition quest)
        {
            if (quest == null || !quest.HasObjectives)
            {
                return true;
            }

            foreach (QuestObjectiveDefinition objective in quest.Objectives)
            {
                if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
                {
                    continue;
                }

                QuestObjectiveProgress progress = GetObjectiveProgress(objective.ObjectiveId);
                if (progress == null || !progress.IsComplete)
                {
                    return false;
                }
            }

            return true;
        }

        public void SyncObjectives(QuestDefinition quest, bool resetCompletedQuest)
        {
            if (quest == null || !quest.HasObjectives)
            {
                return;
            }

            foreach (QuestObjectiveDefinition objective in quest.Objectives)
            {
                if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
                {
                    continue;
                }

                if (!objectivesById.ContainsKey(objective.ObjectiveId) || resetCompletedQuest)
                {
                    objectivesById[objective.ObjectiveId] = new QuestObjectiveProgress(objective.ObjectiveId, 0, objective.RequiredAmount);
                }
            }
        }

        public void LoadObjectiveProgress(IEnumerable<QuestObjectiveProgressSnapshot> snapshots)
        {
            objectivesById.Clear();
            if (snapshots == null)
            {
                return;
            }

            foreach (QuestObjectiveProgressSnapshot snapshot in snapshots)
            {
                if (string.IsNullOrWhiteSpace(snapshot.ObjectiveId))
                {
                    continue;
                }

                objectivesById[snapshot.ObjectiveId] = new QuestObjectiveProgress(
                    snapshot.ObjectiveId,
                    snapshot.CurrentAmount,
                    snapshot.RequiredAmount);
            }
        }
    }

    [Serializable]
    public struct QuestObjectiveProgressSnapshot
    {
        [SerializeField]
        private string objectiveId;

        [SerializeField]
        private int currentAmount;

        [SerializeField]
        private int requiredAmount;

        public string ObjectiveId => objectiveId;
        public int CurrentAmount => currentAmount;
        public int RequiredAmount => requiredAmount;

        public QuestObjectiveProgressSnapshot(string objectiveId, int currentAmount, int requiredAmount)
        {
            this.objectiveId = objectiveId;
            this.currentAmount = currentAmount;
            this.requiredAmount = requiredAmount;
        }
    }

    [Serializable]
    public struct QuestProgressSnapshot
    {
        [SerializeField]
        private string questId;

        [SerializeField]
        private QuestState state;

        [SerializeField]
        private QuestObjectiveProgressSnapshot[] objectives;

        public string QuestId => questId;
        public QuestState State => state;
        public QuestObjectiveProgressSnapshot[] Objectives => objectives;

        public QuestProgressSnapshot(string questId, QuestState state, QuestObjectiveProgressSnapshot[] objectives)
        {
            this.questId = questId;
            this.state = state;
            this.objectives = objectives;
        }
    }

    public sealed class QuestLogEntry
    {
        public string QuestId { get; }
        public string Title { get; }
        public string Description { get; }
        public string RewardDescription { get; }
        public QuestState State { get; }
        public IReadOnlyList<QuestObjectiveLogEntry> Objectives { get; }

        public QuestLogEntry(
            string questId,
            string title,
            string description,
            string rewardDescription,
            QuestState state,
            IReadOnlyList<QuestObjectiveLogEntry> objectives)
        {
            QuestId = questId;
            Title = title;
            Description = description;
            RewardDescription = rewardDescription;
            State = state;
            Objectives = objectives;
        }
    }

    public sealed class QuestObjectiveLogEntry
    {
        public string ObjectiveId { get; }
        public string Description { get; }
        public int CurrentAmount { get; }
        public int RequiredAmount { get; }
        public bool IsComplete => CurrentAmount >= RequiredAmount;

        public QuestObjectiveLogEntry(string objectiveId, string description, int currentAmount, int requiredAmount)
        {
            ObjectiveId = objectiveId;
            Description = description;
            CurrentAmount = currentAmount;
            RequiredAmount = Mathf.Max(1, requiredAmount);
        }
    }

    public sealed class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public event Action<QuestDefinition, QuestState> QuestStateChanged;
        public event Action<string, QuestState> QuestProgressChanged;
        public event Action<QuestDefinition, QuestObjectiveDefinition, QuestObjectiveProgress> QuestObjectiveProgressChanged;
        public event Action<QuestDefinition> QuestRewardClaimed;

        private readonly Dictionary<string, QuestProgress> questProgressById = new();
        private readonly Dictionary<string, QuestDefinition> questDefinitionsById = new();
        private QuestLogProjection questLogProjection;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            questLogProjection = new QuestLogProjection(GetQuestDefinition);
        }

        public bool HasQuest(string questId)
        {
            return !string.IsNullOrWhiteSpace(questId) && questProgressById.ContainsKey(questId);
        }

        public QuestState GetQuestState(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) || !questProgressById.TryGetValue(questId, out QuestProgress progress))
            {
                return QuestState.Locked;
            }

            return progress.State;
        }

        public bool IsQuestActive(string questId)
        {
            return GetQuestState(questId) == QuestState.Active;
        }

        public bool IsQuestCompleted(string questId)
        {
            return GetQuestState(questId) == QuestState.Completed || GetQuestState(questId) == QuestState.RewardClaimed;
        }

        public bool StartQuest(QuestDefinition quest)
        {
            return TryAcceptQuest(quest);
        }

        public bool TryAcceptQuest(QuestDefinition quest)
        {
            if (!TryValidateQuest(quest))
            {
                return false;
            }

            RegisterQuestDefinition(quest);

            if (!questProgressById.TryGetValue(quest.QuestId, out QuestProgress progress))
            {
                progress = new QuestProgress(quest.QuestId);
                questProgressById[quest.QuestId] = progress;
            }

            if (progress.State == QuestState.Active)
            {
                Debug.Log($"Quest '{quest.Title}' is already active.", this);
                return false;
            }

            if ((progress.State == QuestState.Completed || progress.State == QuestState.RewardClaimed) && !quest.IsRepeatable)
            {
                Debug.Log($"Quest '{quest.Title}' is already completed and is not repeatable.", this);
                return false;
            }

            if (!progress.Start(quest))
            {
                return false;
            }

            PublishQuestStateChanged(quest, progress.State);
            Debug.Log($"Quest '{quest.Title}' accepted.", this);
            return true;
        }

        public bool ReportKill(string targetId, int amount = 1)
        {
            return ReportObjectiveProgress(QuestObjectiveType.Kill, targetId, amount);
        }

        public bool ReportObjectiveProgress(QuestObjectiveType objectiveType, string targetId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(targetId) || amount <= 0)
            {
                return false;
            }

            bool changedAnyQuest = false;
            foreach (QuestDefinition quest in questDefinitionsById.Values)
            {
                if (quest == null || GetQuestState(quest.QuestId) != QuestState.Active || !questProgressById.TryGetValue(quest.QuestId, out QuestProgress progress))
                {
                    continue;
                }

                foreach (QuestObjectiveDefinition objective in quest.Objectives)
                {
                    if (!DoesObjectiveMatch(objective, objectiveType, targetId))
                    {
                        continue;
                    }

                    if (progress.AddObjectiveProgress(objective, amount, out QuestObjectiveProgress objectiveProgress))
                    {
                        changedAnyQuest = true;
                        QuestObjectiveProgressChanged?.Invoke(quest, objective, objectiveProgress);
                        Debug.Log($"Quest '{quest.Title}' objective '{objective.ObjectiveId}': {objectiveProgress.CurrentAmount}/{objectiveProgress.RequiredAmount}.", this);
                    }
                }

                if (progress.AreObjectivesComplete(quest))
                {
                    CompleteQuest(quest.QuestId);
                }
            }

            return changedAnyQuest;
        }

        public bool CompleteQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) || !questProgressById.TryGetValue(questId, out QuestProgress progress))
            {
                return false;
            }

            if (!progress.Complete())
            {
                return false;
            }

            PublishQuestStateChanged(GetQuestDefinition(questId), progress.State, questId);
            Debug.Log($"Quest '{questId}' completed.", this);
            return true;
        }

        public bool TryClaimReward(QuestDefinition quest)
        {
            if (!TryValidateQuest(quest) || !questProgressById.TryGetValue(quest.QuestId, out QuestProgress progress))
            {
                return false;
            }

            if (progress.State != QuestState.Completed)
            {
                return false;
            }

            var rewardService = new QuestRewardService(InventoryManager.Instance);
            if (!rewardService.TryGrantRewardItems(quest))
            {
                Debug.LogWarning($"Quest '{quest.Title}' reward could not be claimed because the inventory cannot receive every reward item.", this);
                return false;
            }

            if (!progress.ClaimReward())
            {
                return false;
            }

            PublishQuestStateChanged(quest, progress.State);
            QuestRewardClaimed?.Invoke(quest);
            Debug.Log($"Quest '{quest.Title}' reward claimed: {quest.RewardDescription}", this);
            return true;
        }

        public bool FailQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) || !questProgressById.TryGetValue(questId, out QuestProgress progress))
            {
                return false;
            }

            if (!progress.Fail())
            {
                return false;
            }

            PublishQuestStateChanged(GetQuestDefinition(questId), progress.State, questId);
            Debug.Log($"Quest '{questId}' failed.", this);
            return true;
        }

        public IReadOnlyList<QuestProgressSnapshot> CreateProgressSnapshot()
        {
            var snapshots = new List<QuestProgressSnapshot>(questProgressById.Count);
            foreach (QuestProgress progress in questProgressById.Values)
            {
                var objectiveSnapshots = new List<QuestObjectiveProgressSnapshot>();
                foreach (QuestObjectiveProgress objectiveProgress in progress.Objectives)
                {
                    objectiveSnapshots.Add(new QuestObjectiveProgressSnapshot(
                        objectiveProgress.ObjectiveId,
                        objectiveProgress.CurrentAmount,
                        objectiveProgress.RequiredAmount));
                }

                snapshots.Add(new QuestProgressSnapshot(progress.QuestId, progress.State, objectiveSnapshots.ToArray()));
            }

            return snapshots;
        }

        public IReadOnlyList<QuestLogEntry> GetQuestLogEntries(bool includeRewardClaimed = true, bool includeFailed = true)
        {
            questLogProjection ??= new QuestLogProjection(GetQuestDefinition);
            return questLogProjection.CreateEntries(questProgressById.Values, includeRewardClaimed, includeFailed);
        }

        public void LoadProgressSnapshot(IEnumerable<QuestProgressSnapshot> snapshots, bool notifyChanges = false)
        {
            if (snapshots == null)
            {
                return;
            }

            questProgressById.Clear();

            foreach (QuestProgressSnapshot snapshot in snapshots)
            {
                if (string.IsNullOrWhiteSpace(snapshot.QuestId))
                {
                    continue;
                }

                var progress = new QuestProgress(snapshot.QuestId, snapshot.State);
                progress.LoadObjectiveProgress(snapshot.Objectives);
                questProgressById[snapshot.QuestId] = progress;

                if (notifyChanges)
                {
                    PublishQuestStateChanged(GetQuestDefinition(snapshot.QuestId), snapshot.State, snapshot.QuestId);
                }
            }
        }

        private static bool DoesObjectiveMatch(QuestObjectiveDefinition objective, QuestObjectiveType objectiveType, string targetId)
        {
            return objective != null
                && objective.ObjectiveType == objectiveType
                && string.Equals(objective.TargetId, targetId, StringComparison.OrdinalIgnoreCase);
        }

        private bool TryValidateQuest(QuestDefinition quest)
        {
            if (quest == null)
            {
                Debug.LogWarning("QuestManager received a null QuestDefinition.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(quest.QuestId))
            {
                Debug.LogWarning($"QuestDefinition '{quest.name}' does not have a valid QuestId.", this);
                return false;
            }

            return true;
        }

        public void RegisterQuestDefinition(QuestDefinition quest)
        {
            if (quest != null && !string.IsNullOrWhiteSpace(quest.QuestId))
            {
                questDefinitionsById[quest.QuestId] = quest;
            }
        }

        private QuestDefinition GetQuestDefinition(string questId)
        {
            return !string.IsNullOrWhiteSpace(questId) && questDefinitionsById.TryGetValue(questId, out QuestDefinition quest)
                ? quest
                : null;
        }

        private void PublishQuestStateChanged(QuestDefinition quest, QuestState state, string questIdOverride = null)
        {
            string questId = quest != null ? quest.QuestId : questIdOverride;
            QuestStateChanged?.Invoke(quest, state);
            QuestProgressChanged?.Invoke(questId, state);
        }
    }
}
