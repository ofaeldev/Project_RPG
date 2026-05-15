using System.Collections.Generic;
using System.Text;
using RPGProject.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class QuestLogUIController : MonoBehaviour
    {
        [Header("Quest Log UI")]
        [SerializeField]
        private GameObject questLogPanel;

        [SerializeField]
        private Button toggleButton;

        [SerializeField]
        private TMP_Text toggleButtonText;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text contentText;

        [Header("Settings")]
        [SerializeField]
        private bool hidePanelOnStart = true;

        [SerializeField]
        private bool allowKeyboardToggle = true;

        [SerializeField]
        private Key toggleKey = Key.J;

        [SerializeField]
        private string openButtonLabel = "Quests";

        [SerializeField]
        private string closeButtonLabel = "Fechar";

        [SerializeField]
        private bool includeRewardClaimed = true;

        [SerializeField]
        private bool includeFailed = true;

        private readonly StringBuilder stringBuilder = new();
        private bool isSubscribed;

        private void Awake()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleQuestLog);
            }

            if (titleText != null)
            {
                titleText.text = "Quest Log";
            }

            if (hidePanelOnStart)
            {
                SetPanelVisible(false);
            }

            Refresh();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Update()
        {
            if (!isSubscribed)
            {
                TrySubscribe();
            }

            if (allowKeyboardToggle && Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                ToggleQuestLog();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
            GameplayInputBlocker.Instance?.UnregisterUIBlocker(this);
        }

        public void ToggleQuestLog()
        {
            bool nextVisible = questLogPanel == null || !questLogPanel.activeSelf;
            SetPanelVisible(nextVisible);
            Refresh();
        }

        public void Refresh()
        {
            if (contentText == null)
            {
                return;
            }

            IReadOnlyList<QuestLogEntry> entries = QuestManager.Instance != null
                ? QuestManager.Instance.GetQuestLogEntries(includeRewardClaimed, includeFailed)
                : null;

            if (entries == null || entries.Count == 0)
            {
                contentText.text = "Nenhuma quest ativa.";
                return;
            }

            stringBuilder.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                AppendQuest(entries[i]);
                if (i < entries.Count - 1)
                {
                    stringBuilder.AppendLine();
                }
            }

            contentText.text = stringBuilder.ToString();
        }

        private void AppendQuest(QuestLogEntry entry)
        {
            stringBuilder.Append("<b>");
            stringBuilder.Append(entry.Title);
            stringBuilder.Append("</b> <size=75%><color=#B8C2D6>[");
            stringBuilder.Append(GetStateLabel(entry.State));
            stringBuilder.AppendLine("]</color></size>");

            if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                stringBuilder.AppendLine(entry.Description);
            }

            if (entry.Objectives != null && entry.Objectives.Count > 0)
            {
                foreach (QuestObjectiveLogEntry objective in entry.Objectives)
                {
                    string status = objective.IsComplete ? "<color=#8EE68E>✓</color>" : "<color=#F0D17A>•</color>";
                    string objectiveText = string.IsNullOrWhiteSpace(objective.Description) ? objective.ObjectiveId : objective.Description;
                    stringBuilder.Append(status);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(objectiveText);
                    stringBuilder.Append(" <color=#B8C2D6>");
                    stringBuilder.Append(objective.CurrentAmount);
                    stringBuilder.Append('/');
                    stringBuilder.Append(objective.RequiredAmount);
                    stringBuilder.AppendLine("</color>");
                }
            }

            if (!string.IsNullOrWhiteSpace(entry.RewardDescription))
            {
                stringBuilder.Append("<color=#DDBB68>Recompensa: ");
                stringBuilder.Append(entry.RewardDescription);
                stringBuilder.AppendLine("</color>");
            }
        }

        private void TrySubscribe()
        {
            if (isSubscribed || QuestManager.Instance == null)
            {
                return;
            }

            QuestManager.Instance.QuestStateChanged += OnQuestStateChanged;
            QuestManager.Instance.QuestObjectiveProgressChanged += OnQuestObjectiveProgressChanged;
            QuestManager.Instance.QuestRewardClaimed += OnQuestRewardClaimed;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || QuestManager.Instance == null)
            {
                return;
            }

            QuestManager.Instance.QuestStateChanged -= OnQuestStateChanged;
            QuestManager.Instance.QuestObjectiveProgressChanged -= OnQuestObjectiveProgressChanged;
            QuestManager.Instance.QuestRewardClaimed -= OnQuestRewardClaimed;
            isSubscribed = false;
        }

        private void OnQuestStateChanged(QuestDefinition quest, QuestState state)
        {
            Refresh();
        }

        private void OnQuestObjectiveProgressChanged(QuestDefinition quest, QuestObjectiveDefinition objective, QuestObjectiveProgress progress)
        {
            Refresh();
        }

        private void OnQuestRewardClaimed(QuestDefinition quest)
        {
            Refresh();
        }

        private void SetPanelVisible(bool visible)
        {
            if (questLogPanel != null)
            {
                questLogPanel.SetActive(visible);
            }

            GameplayInputBlocker.Instance?.SetUIBlocker(this, visible);

            if (toggleButtonText != null)
            {
                toggleButtonText.text = visible ? closeButtonLabel : openButtonLabel;
            }
        }

        private static string GetStateLabel(QuestState state)
        {
            return state switch
            {
                QuestState.Active => "Ativa",
                QuestState.Completed => "Completa",
                QuestState.RewardClaimed => "Recompensa recebida",
                QuestState.Failed => "Falhou",
                QuestState.Available => "Disponivel",
                _ => "Bloqueada",
            };
        }
    }
}
