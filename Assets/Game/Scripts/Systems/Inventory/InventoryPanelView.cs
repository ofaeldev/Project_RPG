using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [System.Serializable]
    public sealed class InventoryPanelView
    {
        [Header("Inventory UI")]
        [SerializeField]
        private GameObject inventoryPanel;

        [SerializeField]
        private Button toggleButton;

        [SerializeField]
        private TMP_Text toggleButtonText;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text contentText;

        [SerializeField]
        private TMP_Text detailsText;

        [SerializeField]
        private Button useButton;

        [SerializeField]
        private TMP_Text useButtonText;

        [Header("Slots")]
        [SerializeField]
        private RectTransform slotContainer;

        [SerializeField]
        private InventorySlotUI slotPrefab;

        [Header("Action Menu")]
        [SerializeField]
        private RectTransform actionMenuRoot;

        [SerializeField]
        private Button actionUseButton;

        [SerializeField]
        private Button actionDropButton;

        [SerializeField]
        private Button actionCancelButton;

        public GameObject Panel => inventoryPanel;
        public Button ToggleButton => toggleButton;
        public Button UseButton => useButton;
        public Button ActionUseButton => actionUseButton;
        public Button ActionDropButton => actionDropButton;
        public Button ActionCancelButton => actionCancelButton;
        public RectTransform SlotContainer => slotContainer;
        public InventorySlotUI SlotPrefab => slotPrefab;
        public bool IsVisible => inventoryPanel == null || inventoryPanel.activeSelf;

        public void BindReferences(
            GameObject panel,
            Button toggle,
            TMP_Text toggleText,
            TMP_Text title,
            TMP_Text content,
            TMP_Text details,
            Button use,
            TMP_Text useText,
            RectTransform slots,
            InventorySlotUI slotTemplate,
            RectTransform actionMenu,
            Button actionUse,
            Button actionDrop,
            Button actionCancel)
        {
            inventoryPanel = panel;
            toggleButton = toggle;
            toggleButtonText = toggleText;
            titleText = title;
            contentText = content;
            detailsText = details;
            useButton = use;
            useButtonText = useText;
            slotContainer = slots;
            slotPrefab = slotTemplate;
            actionMenuRoot = actionMenu;
            actionUseButton = actionUse;
            actionDropButton = actionDrop;
            actionCancelButton = actionCancel;
        }

        public void Initialize(Transform ownerTransform)
        {
            EnsureSlotContainer();
            EnsureActionMenu();
            SetTitle("Inventory");

            void EnsureSlotContainer()
            {
                if (slotContainer != null || inventoryPanel == null)
                {
                    return;
                }

                slotContainer = InventoryUIRuntimeFactory.CreateSlotContainer(inventoryPanel.transform);
            }

            void EnsureActionMenu()
            {
                if (actionMenuRoot != null || inventoryPanel == null)
                {
                    return;
                }

                InventoryActionMenu actionMenu = InventoryUIRuntimeFactory.CreateActionMenu(inventoryPanel.transform);
                actionMenuRoot = actionMenu.Root;
                actionUseButton = actionMenu.UseButton;
                actionDropButton = actionMenu.DropButton;
                actionCancelButton = actionMenu.CancelButton;
            }
        }

        public void SetPanelVisible(bool visible, Object blocker, string openButtonLabel, string closeButtonLabel)
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(visible);
            }

            GameplayInputBlocker.Instance?.SetUIBlocker(blocker, visible);
            SetToggleButtonText(visible ? closeButtonLabel : openButtonLabel);

            if (!visible)
            {
                HideActionMenu();
            }
        }

        public void SetEmptyContentVisible(bool visible, string message)
        {
            if (contentText == null)
            {
                return;
            }

            contentText.gameObject.SetActive(visible);
            contentText.text = visible ? message : string.Empty;
        }

        public void SetDetailsText(string value)
        {
            if (detailsText != null)
            {
                detailsText.text = value;
            }
        }

        public void SetUseButtons(bool canUse, bool hasSelection)
        {
            if (useButton != null)
            {
                useButton.interactable = canUse;
            }

            if (actionUseButton != null)
            {
                actionUseButton.interactable = canUse;
            }

            if (actionDropButton != null)
            {
                actionDropButton.interactable = hasSelection;
            }

            if (useButtonText != null)
            {
                useButtonText.text = !hasSelection ? "Selecione" : canUse ? "Usar" : "Nao usavel";
            }
        }

        public void ShowActionMenuNear(Transform slotTransform)
        {
            if (actionMenuRoot == null || slotTransform == null)
            {
                return;
            }

            actionMenuRoot.gameObject.SetActive(true);
            actionMenuRoot.position = slotTransform.position + new Vector3(90f, -42f, 0f);
        }

        public void HideActionMenu()
        {
            if (actionMenuRoot != null)
            {
                actionMenuRoot.gameObject.SetActive(false);
            }
        }

        public bool IsPointerOutsidePanel(PointerEventData eventData, Canvas rootCanvas)
        {
            if (inventoryPanel == null || inventoryPanel.transform is not RectTransform panelRect)
            {
                return true;
            }

            Camera eventCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? rootCanvas.worldCamera
                : null;
            return !RectTransformUtility.RectangleContainsScreenPoint(panelRect, eventData.position, eventCamera);
        }

        public InventorySlotUI CreateSlot(int index)
        {
            if (slotContainer == null)
            {
                return null;
            }

            InventorySlotUI slot = slotPrefab != null
                ? Object.Instantiate(slotPrefab, slotContainer)
                : new GameObject($"InventorySlot_{index + 1}", typeof(RectTransform), typeof(InventorySlotUI)).GetComponent<InventorySlotUI>();

            slot.transform.SetParent(slotContainer, false);
            var rect = (RectTransform)slot.transform;
            rect.sizeDelta = new Vector2(176f, 78f);
            return slot;
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
