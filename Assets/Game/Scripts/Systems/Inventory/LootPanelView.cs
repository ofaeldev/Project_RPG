using RPGProject.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [System.Serializable]
    public sealed class LootPanelView
    {
        [Header("Loot UI")]
        [SerializeField]
        private GameObject lootPanel;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text contentText;

        [SerializeField]
        private Button takeAllButton;

        [SerializeField]
        private Button closeButton;

        public Button TakeAllButton => takeAllButton;
        public Button CloseButton => closeButton;
        public bool IsVisible => lootPanel != null && lootPanel.activeSelf;

        public void BindReferences(GameObject panel, TMP_Text title, TMP_Text content, Button takeAll, Button close)
        {
            lootPanel = panel;
            titleText = title;
            contentText = content;
            takeAllButton = takeAll;
            closeButton = close;
        }

        public void SetContent(ILootSource lootSource, string content)
        {
            if (titleText != null)
            {
                titleText.text = lootSource != null ? lootSource.DisplayName : "Loot";
            }

            if (contentText != null)
            {
                contentText.text = content;
            }
        }

        public void SetVisible(bool visible, Object blocker)
        {
            if (lootPanel != null)
            {
                lootPanel.SetActive(visible);
            }

            GameplayInputBlocker.Instance?.SetUIBlocker(blocker, visible);
        }
    }
}
