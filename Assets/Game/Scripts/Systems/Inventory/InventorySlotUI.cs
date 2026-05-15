using RPGProject.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    /// <summary>
    /// Visual reutilizavel de um slot de inventario.
    /// Mantem apenas apresentacao e eventos de ponteiro; a regra continua no InventoryUIController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("Visuals")]
        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text amountText;

        [SerializeField]
        private TMP_Text categoryText;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [Header("Colors")]
        [SerializeField]
        private Color normalColor = new(0.10f, 0.12f, 0.15f, 0.94f);

        [SerializeField]
        private Color selectedColor = new(0.22f, 0.28f, 0.36f, 0.98f);

        [SerializeField]
        private Color emptyColor = new(0.06f, 0.07f, 0.09f, 0.55f);

        private InventoryUIController owner;
        private InventoryItemStack stack;
        private int slotIndex;

        public InventoryItemStack Stack => stack;
        public int SlotIndex => slotIndex;

        private void Awake()
        {
            EnsureVisuals();
        }

        public void Bind(InventoryUIController owner, int slotIndex, InventoryItemStack stack, bool isSelected)
        {
            this.owner = owner;
            this.slotIndex = slotIndex;
            this.stack = stack;

            EnsureVisuals();
            Refresh(isSelected);
        }

        public void SetDragging(bool isDragging)
        {
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = isDragging ? 0.45f : 1f;
            canvasGroup.blocksRaycasts = !isDragging;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                owner?.SelectSlot(slotIndex, true);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (stack != null)
            {
                owner?.BeginSlotDrag(this, eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            owner?.UpdateSlotDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            owner?.EndSlotDrag(this, eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            owner?.DropDraggedSlotOn(slotIndex);
        }

        private void Refresh(bool isSelected)
        {
            bool hasStack = stack != null && stack.Amount > 0;
            if (backgroundImage != null)
            {
                backgroundImage.color = hasStack ? (isSelected ? selectedColor : normalColor) : emptyColor;
            }

            ItemDefinition item = stack?.Item;
            string itemName = item != null && !string.IsNullOrWhiteSpace(item.DisplayName)
                ? item.DisplayName
                : stack?.ItemId;

            if (nameText != null)
            {
                nameText.text = hasStack ? itemName : "Vazio";
            }

            if (amountText != null)
            {
                amountText.text = hasStack && stack.Amount > 1 ? $"x{stack.Amount}" : string.Empty;
            }

            if (categoryText != null)
            {
                categoryText.text = hasStack && item != null ? item.Category.ToString() : string.Empty;
            }

            if (iconImage == null)
            {
                return;
            }

            iconImage.enabled = hasStack;
            iconImage.sprite = item != null ? item.Icon : null;
            iconImage.color = iconImage.sprite != null ? Color.white : GetCategoryColor(item);
        }

        private static Color GetCategoryColor(ItemDefinition item)
        {
            if (item == null)
            {
                return new Color(0.46f, 0.50f, 0.56f, 1f);
            }

            return item.Category switch
            {
                ItemCategory.Key => new Color(0.94f, 0.74f, 0.32f, 1f),
                ItemCategory.Consumable => new Color(0.45f, 0.82f, 0.45f, 1f),
                ItemCategory.Quest => new Color(0.55f, 0.62f, 0.96f, 1f),
                ItemCategory.Equipment => new Color(0.86f, 0.47f, 0.42f, 1f),
                ItemCategory.Currency => new Color(0.96f, 0.86f, 0.35f, 1f),
                _ => new Color(0.66f, 0.72f, 0.78f, 1f),
            };
        }

        private void EnsureVisuals()
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null)
            {
                gameObject.AddComponent<RectTransform>();
            }

            if (canvasGroup == null && !TryGetComponent(out canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (backgroundImage == null && !TryGetComponent(out backgroundImage))
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            iconImage ??= CreateImage("Icon", new Vector2(8f, -8f), new Vector2(46f, 46f));
            nameText ??= CreateText("NameText", new Vector2(62f, -8f), new Vector2(104f, 24f), 14, TextAlignmentOptions.Left);
            categoryText ??= CreateText("CategoryText", new Vector2(62f, -34f), new Vector2(104f, 20f), 11, TextAlignmentOptions.Left);
            amountText ??= CreateText("AmountText", new Vector2(-8f, -56f), new Vector2(48f, 18f), 12, TextAlignmentOptions.Right);
        }

        private Image CreateImage(string childName, Vector2 anchoredPosition, Vector2 size)
        {
            var child = new GameObject(childName, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(transform, false);
            var rect = (RectTransform)child.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return child.GetComponent<Image>();
        }

        private TMP_Text CreateText(string childName, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAlignmentOptions alignment)
        {
            var child = new GameObject(childName, typeof(RectTransform), typeof(TextMeshProUGUI));
            child.transform.SetParent(transform, false);
            var rect = (RectTransform)child.transform;
            rect.anchorMin = alignment == TextAlignmentOptions.Right ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = alignment == TextAlignmentOptions.Right ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TMP_Text text = child.GetComponent<TMP_Text>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.color = new Color(0.88f, 0.92f, 0.90f, 1f);
            return text;
        }
    }
}
