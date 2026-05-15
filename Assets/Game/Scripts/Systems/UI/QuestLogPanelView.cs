using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [System.Serializable]
    public sealed class QuestLogPanelView
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

        public Button ToggleButton => toggleButton;
        public bool IsVisible => questLogPanel == null || questLogPanel.activeSelf;

        public void BindReferences(
            GameObject panel,
            Button toggle,
            TMP_Text toggleText,
            TMP_Text title,
            TMP_Text content)
        {
            questLogPanel = panel;
            toggleButton = toggle;
            toggleButtonText = toggleText;
            titleText = title;
            contentText = content;
        }

        public void Initialize()
        {
            SetTitle("Quest Log");
        }

        public void SetPanelVisible(bool visible, Object blocker, string openButtonLabel, string closeButtonLabel)
        {
            if (questLogPanel != null)
            {
                questLogPanel.SetActive(visible);
            }

            GameplayInputBlocker.Instance?.SetUIBlocker(blocker, visible);
            SetToggleButtonText(visible ? closeButtonLabel : openButtonLabel);
        }

        public void SetContent(string content)
        {
            if (contentText != null)
            {
                contentText.text = content;
            }
        }

        private void SetTitle(string value)
        {
            if (titleText != null)
            {
                titleText.text = value;
            }
        }

        private void SetToggleButtonText(string value)
        {
            if (toggleButtonText != null)
            {
                toggleButtonText.text = value;
            }
        }
    }
}
