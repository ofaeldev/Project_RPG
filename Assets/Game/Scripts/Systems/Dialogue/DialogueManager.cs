using System;
using System.Collections.Generic;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [Serializable]
    public struct DialogueProgressSnapshot
    {
        [SerializeField]
        private string dialogueId;

        [SerializeField]
        private bool hasBeenSeen;

        [SerializeField]
        private int lastLineIndex;

        public string DialogueId => dialogueId;
        public bool HasBeenSeen => hasBeenSeen;
        public int LastLineIndex => lastLineIndex;

        public DialogueProgressSnapshot(string dialogueId, bool hasBeenSeen, int lastLineIndex)
        {
            this.dialogueId = dialogueId;
            this.hasBeenSeen = hasBeenSeen;
            this.lastLineIndex = lastLineIndex;
        }
    }

    public sealed class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public event Action<DialogueDefinition, GameObject> DialogueStarted;
        public event Action<DialogueDefinition, DialogueLine, int> DialogueLineStarted;
        public event Action<DialogueDefinition, IReadOnlyList<DialogueChoice>> DialogueChoicesPresented;
        public event Action<DialogueDefinition, DialogueChoice, int> DialogueChoiceSelected;
        public event Action<DialogueDefinition> DialogueCompleted;

        private readonly Dictionary<string, DialogueProgress> dialogueProgress = new();
        private readonly List<DialogueChoice> currentAvailableChoices = new();
        private DialogueDefinition currentDialogue;
        private GameObject currentSource;
        private int currentLineIndex;
        private QuestDefinition queuedQuest;
        private bool isWaitingForChoice;

        public bool IsDialogueActive => currentDialogue != null;
        public bool IsWaitingForChoice => isWaitingForChoice;
        public DialogueDefinition CurrentDialogue => currentDialogue;
        public int CurrentLineIndex => currentLineIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public bool StartDialogue(DialogueDefinition dialogue, GameObject source, QuestDefinition questToStart = null)
        {
            if (IsDialogueActive)
            {
                Debug.Log($"Dialogue '{currentDialogue.DisplayName}' is already active.", source);
                return false;
            }

            if (dialogue == null)
            {
                Debug.LogWarning("DialogueManager received a null DialogueDefinition.", this);
                return false;
            }

            if (!dialogue.HasLines)
            {
                Debug.LogWarning($"DialogueDefinition '{dialogue.name}' does not contain lines.", this);
                return false;
            }

            if (!dialogue.AreConditionsMet)
            {
                Debug.Log($"Dialogue '{dialogue.name}' conditions are not met.", source);
                return false;
            }

            if (!dialogue.IsRepeatable && HasSeenDialogue(dialogue.DialogueId))
            {
                Debug.Log($"Dialogue '{dialogue.name}' was already seen and is not repeatable.", source);
                return false;
            }

            queuedQuest = questToStart != null
                ? questToStart
                : dialogue.StartQuestAfterDialogue ? dialogue.QuestToStart : null;
            currentDialogue = dialogue;
            currentSource = source;
            currentLineIndex = 0;
            isWaitingForChoice = false;
            currentAvailableChoices.Clear();
            DialogueStarted?.Invoke(currentDialogue, currentSource);
            PublishCurrentLine();
            return true;
        }

        public void AdvanceDialogue()
        {
            if (currentDialogue == null || isWaitingForChoice)
            {
                return;
            }

            currentLineIndex++;
            if (currentLineIndex >= currentDialogue.Lines.Count)
            {
                if (currentDialogue.HasChoices)
                {
                    PresentChoices();
                }
                else
                {
                    CompleteCurrentDialogue(shouldStartQueuedQuest: true);
                }
            }
            else
            {
                PublishCurrentLine();
            }
        }

        public void SelectChoice(int choiceIndex)
        {
            if (currentDialogue == null || !isWaitingForChoice || currentAvailableChoices.Count == 0)
            {
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= currentAvailableChoices.Count)
            {
                Debug.LogWarning($"Dialogue choice index '{choiceIndex}' is invalid for '{currentDialogue.DisplayName}'.", this);
                return;
            }

            DialogueChoice choice = currentAvailableChoices[choiceIndex];
            DialogueChoiceSelected?.Invoke(currentDialogue, choice, choiceIndex);

            DialogueDefinition nextDialogue = choice.NextDialogue;
            bool shouldStartQueuedQuest = choice.Action == DialogueChoiceAction.AcceptQuest
                || (choice.Action == DialogueChoiceAction.Continue && nextDialogue == null);

            if (choice.Action == DialogueChoiceAction.DeclineQuest || choice.Action == DialogueChoiceAction.Close)
            {
                queuedQuest = null;
            }

            CompleteCurrentDialogue(shouldStartQueuedQuest, nextDialogue);
        }

        public bool HasSeenDialogue(string dialogueId)
        {
            return !string.IsNullOrWhiteSpace(dialogueId)
                && dialogueProgress.TryGetValue(dialogueId, out DialogueProgress progress)
                && progress.HasBeenSeen;
        }

        public void ResetDialogueProgress(string dialogueId)
        {
            if (!string.IsNullOrWhiteSpace(dialogueId))
            {
                dialogueProgress.Remove(dialogueId);
            }
        }

        public IReadOnlyList<DialogueProgressSnapshot> CreateProgressSnapshot()
        {
            var snapshots = new List<DialogueProgressSnapshot>(dialogueProgress.Count);
            foreach (DialogueProgress progress in dialogueProgress.Values)
            {
                snapshots.Add(new DialogueProgressSnapshot(
                    progress.DialogueId,
                    progress.HasBeenSeen,
                    progress.LastLineIndex));
            }

            return snapshots;
        }

        public void LoadProgressSnapshot(IEnumerable<DialogueProgressSnapshot> snapshots)
        {
            dialogueProgress.Clear();
            if (snapshots == null)
            {
                return;
            }

            foreach (DialogueProgressSnapshot snapshot in snapshots)
            {
                if (string.IsNullOrWhiteSpace(snapshot.DialogueId))
                {
                    continue;
                }

                var progress = new DialogueProgress(snapshot.DialogueId)
                {
                    HasBeenSeen = snapshot.HasBeenSeen,
                    LastLineIndex = snapshot.LastLineIndex,
                };
                dialogueProgress[snapshot.DialogueId] = progress;
            }
        }

        private void PublishCurrentLine()
        {
            if (currentDialogue == null || currentLineIndex < 0 || currentLineIndex >= currentDialogue.Lines.Count)
            {
                return;
            }

            DialogueLine line = currentDialogue.Lines[currentLineIndex];
            Debug.Log($"[Dialogue] {currentDialogue.DisplayName}: {line.SpeakerName} - {line.Text}");
            DialogueLineStarted?.Invoke(currentDialogue, line, currentLineIndex);
        }

        private void PresentChoices()
        {
            currentAvailableChoices.Clear();
            foreach (DialogueChoice choice in currentDialogue.Choices)
            {
                if (choice != null && choice.AreConditionsMet)
                {
                    currentAvailableChoices.Add(choice);
                }
            }

            if (currentAvailableChoices.Count == 0)
            {
                CompleteCurrentDialogue(shouldStartQueuedQuest: true);
                return;
            }

            isWaitingForChoice = true;
            DialogueChoicesPresented?.Invoke(currentDialogue, currentAvailableChoices);
        }

        private void CompleteCurrentDialogue(bool shouldStartQueuedQuest, DialogueDefinition nextDialogue = null)
        {
            if (currentDialogue == null)
            {
                return;
            }

            DialogueDefinition completedDialogue = currentDialogue;
            GameObject completedSource = currentSource;
            QuestDefinition questForNextDialogue = queuedQuest;

            MarkDialogueAsSeen(completedDialogue.DialogueId);
            Debug.Log($"Dialogue '{completedDialogue.DisplayName}' completed.");
            DialogueCompleted?.Invoke(completedDialogue);

            if (shouldStartQueuedQuest && queuedQuest != null)
            {
                QuestManager.Instance?.StartQuest(queuedQuest);
                questForNextDialogue = null;
            }

            queuedQuest = null;
            currentDialogue = null;
            currentSource = null;
            currentLineIndex = 0;
            isWaitingForChoice = false;
            currentAvailableChoices.Clear();

            if (nextDialogue != null)
            {
                StartDialogue(nextDialogue, completedSource, questForNextDialogue);
            }
        }

        private void MarkDialogueAsSeen(string dialogueId)
        {
            if (string.IsNullOrWhiteSpace(dialogueId))
            {
                return;
            }

            if (!dialogueProgress.TryGetValue(dialogueId, out DialogueProgress progress))
            {
                progress = new DialogueProgress(dialogueId);
                dialogueProgress[dialogueId] = progress;
            }

            progress.HasBeenSeen = true;
            progress.LastLineIndex = currentLineIndex;
        }

        private sealed class DialogueProgress
        {
            public string DialogueId { get; }
            public bool HasBeenSeen { get; set; }
            public int LastLineIndex { get; set; }

            public DialogueProgress(string dialogueId)
            {
                DialogueId = dialogueId;
                HasBeenSeen = false;
                LastLineIndex = -1;
            }
        }
    }
}
