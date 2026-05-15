using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    public readonly struct InventoryActionMenu
    {
        public RectTransform Root { get; }
        public Button UseButton { get; }
        public Button DropButton { get; }
        public Button CancelButton { get; }

        public InventoryActionMenu(RectTransform root, Button useButton, Button dropButton, Button cancelButton)
        {
            Root = root;
            UseButton = useButton;
            DropButton = dropButton;
            CancelButton = cancelButton;
        }
    }

    public static class InventoryUIRuntimeFactory
    {
        public static RectTransform CreateSlotContainer(Transform parent)
        {
            var containerObject = new GameObject("SlotContainer", typeof(RectTransform), typeof(GridLayoutGroup));
            containerObject.transform.SetParent(parent, false);
            var slotContainer = (RectTransform)containerObject.transform;
            slotContainer.anchorMin = new Vector2(0.04f, 0.18f);
            slotContainer.anchorMax = new Vector2(0.68f, 0.86f);
            slotContainer.offsetMin = new Vector2(28f, 28f);
            slotContainer.offsetMax = new Vector2(-12f, -18f);

            GridLayoutGroup grid = containerObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(176f, 78f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            return slotContainer;
        }

        public static InventoryActionMenu CreateActionMenu(Transform parent)
        {
            var menuObject = new GameObject("ItemActionMenu", typeof(RectTransform), typeof(Image));
            menuObject.transform.SetParent(parent, false);
            var actionMenuRoot = (RectTransform)menuObject.transform;
            actionMenuRoot.sizeDelta = new Vector2(138f, 108f);
            menuObject.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.10f, 0.96f);

            Button useButton = CreateActionButton(actionMenuRoot, "Usar", new Vector2(0f, -6f));
            Button dropButton = CreateActionButton(actionMenuRoot, "Descartar", new Vector2(0f, -40f));
            Button cancelButton = CreateActionButton(actionMenuRoot, "Fechar", new Vector2(0f, -74f));
            actionMenuRoot.gameObject.SetActive(false);

            return new InventoryActionMenu(actionMenuRoot, useButton, dropButton, cancelButton);
        }

        private static Button CreateActionButton(Transform parent, string label, Vector2 anchoredPosition)
        {
            var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(-12f, 28f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.16f, 0.19f, 0.24f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            CreateButtonText(buttonObject.transform, label);
            return button;
        }

        private static void CreateButtonText(Transform parent, string label)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.text = label;
            text.fontSize = 13;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.90f, 0.93f, 0.92f, 1f);
        }
    }
}
