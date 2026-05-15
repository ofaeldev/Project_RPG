using System.Collections.Generic;
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

        private readonly QuestLogPanelView view = new();
        private readonly QuestLogContentFormatter contentFormatter = new();
        private bool isSubscribed;

        private void Awake()
        {
            BindView();
            view.Initialize();

            if (view.ToggleButton != null)
            {
                view.ToggleButton.onClick.AddListener(ToggleQuestLog);
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

            if (allowKeyboardToggle && QuestLogKeyboardInput.WasTogglePressed(Keyboard.current, toggleKey))
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
            bool nextVisible = !view.IsVisible;
            SetPanelVisible(nextVisible);
            Refresh();
        }

        public void Refresh()
        {
            IReadOnlyList<QuestLogEntry> entries = QuestManager.Instance != null
                ? QuestManager.Instance.GetQuestLogEntries(includeRewardClaimed, includeFailed)
                : null;

            view.SetContent(contentFormatter.Format(entries));
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
            view.SetPanelVisible(visible, this, openButtonLabel, closeButtonLabel);
        }

        private void BindView()
        {
            view.BindReferences(questLogPanel, toggleButton, toggleButtonText, titleText, contentText);
        }
    }
}
