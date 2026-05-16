using System.Collections.Generic;
using RPGProject.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class InventoryUIController : MonoBehaviour
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

        [SerializeField]
        [Min(4)]
        private int minimumVisibleSlots = 18;

        [Header("Action Menu")]
        [SerializeField]
        private RectTransform actionMenuRoot;

        [SerializeField]
        private Button actionUseButton;

        [SerializeField]
        private Button actionDropButton;

        [SerializeField]
        private Button actionCancelButton;

        [Header("Settings")]
        [SerializeField]
        private bool hidePanelOnStart = true;

        [SerializeField]
        private bool allowKeyboardToggle = true;

        [SerializeField]
        private Key toggleKey = Key.I;

        [SerializeField]
        private Key useSelectedKey = Key.U;

        [SerializeField]
        private Key dropSelectedKey = Key.Delete;

        [SerializeField]
        private string openButtonLabel = "Inventory";

        [SerializeField]
        private string closeButtonLabel = "Fechar";

        private readonly InventoryPanelView view = new();
        private readonly List<InventoryItemStack> visibleItems = new();
        private readonly List<InventorySlotUI> slots = new();
        private readonly InventoryDragIconPresenter dragIconPresenter = new();
        private readonly InventoryInteractionFlow interactionFlow = new();
        private readonly InventorySlotTransferService slotTransferService = new();
        private readonly InventoryDropFlow dropFlow = new();
        private InventoryItemActionFlow itemActionFlow;
        private Canvas rootCanvas;
        private readonly InventoryDetailsFormatter detailsFormatter = new();
        private InventoryPresenter presenter;
        private bool isSubscribed;

        private void Awake()
        {
            itemActionFlow = new InventoryItemActionFlow(dropFlow);
            rootCanvas = GetComponentInParent<Canvas>();
            BindView();
            view.Initialize(transform);
            presenter = new InventoryPresenter(view, detailsFormatter);

            if (view.ToggleButton != null)
            {
                view.ToggleButton.onClick.AddListener(ToggleInventory);
            }

            if (view.UseButton != null)
            {
                view.UseButton.onClick.AddListener(UseSelectedItem);
            }

            if (view.ActionUseButton != null)
            {
                view.ActionUseButton.onClick.AddListener(UseSelectedItem);
            }

            if (view.ActionDropButton != null)
            {
                view.ActionDropButton.onClick.AddListener(DropSelectedItemNearPlayer);
            }

            if (view.ActionCancelButton != null)
            {
                view.ActionCancelButton.onClick.AddListener(HideActionMenu);
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
                ToggleInventory();
            }

            if (view.IsVisible)
            {
                HandleInventoryKeyboard();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
            GameplayInputBlocker.Instance?.UnregisterUIBlocker(this);
        }

        public void ToggleInventory()
        {
            bool nextVisible = !view.IsVisible;
            SetPanelVisible(nextVisible);
            Refresh();
        }

        public void Refresh()
        {
            presenter ??= new InventoryPresenter(view, detailsFormatter);
            presenter.Refresh(this, InventoryManager.Instance, interactionFlow, visibleItems, slots, minimumVisibleSlots);
        }

        public void SelectSlot(int slotIndex, bool showActions)
        {
            if (!interactionFlow.SelectSlot(slotIndex, visibleItems.Count))
            {
                HideActionMenu();
                Refresh();
                return;
            }

            Refresh();

            if (showActions)
            {
                ShowActionMenuNearSlot(slotIndex);
            }
        }

        public void BeginSlotDrag(InventorySlotUI slot, PointerEventData eventData)
        {
            interactionFlow.BeginDrag(slot);
            SelectSlot(slot.SlotIndex, false);
            HideActionMenu();
            slot.SetDragging(true);
            CreateDragIcon(slot.Stack);
            UpdateSlotDrag(eventData);
        }

        public void UpdateSlotDrag(PointerEventData eventData)
        {
            dragIconPresenter.MoveTo(eventData.position);
        }

        public void DropDraggedSlotOn(int targetSlotIndex)
        {
            interactionFlow.SetDropTarget(targetSlotIndex);
        }

        public void EndSlotDrag(InventorySlotUI slot, PointerEventData eventData)
        {
            slot.SetDragging(false);
            DestroyDragIcon();

            InventorySlotUI draggedSlot = interactionFlow.DraggedSlot;
            if (draggedSlot == null || draggedSlot.Stack == null)
            {
                interactionFlow.ClearDrag();
                return;
            }

            InventoryItemStack draggedStack = draggedSlot.Stack;
            if (interactionFlow.HasPendingSlotTransfer())
            {
                slotTransferService.TryMoveOrMerge(
                    InventoryManager.Instance,
                    draggedSlot.SlotIndex,
                    interactionFlow.PendingDropTargetIndex,
                    visibleItems.Count);
                interactionFlow.SelectDropTarget(visibleItems.Count);
                interactionFlow.ClearDrag();
                Refresh();
                return;
            }

            if (IsPointerOutsideInventory(eventData) && TryDropStackAtPointer(draggedSlot.SlotIndex, draggedStack, eventData))
            {
                interactionFlow.ClearDrag();
                Refresh();
                return;
            }

            interactionFlow.ClearDrag();
            Refresh();
        }

        public void UseSelectedItem()
        {
            itemActionFlow ??= new InventoryItemActionFlow(dropFlow);
            if (!itemActionFlow.TryUseSelected(InventoryManager.Instance, interactionFlow, visibleItems))
            {
                return;
            }

            HideActionMenu();
            Refresh();
        }

        public void DropSelectedItemNearPlayer()
        {
            itemActionFlow ??= new InventoryItemActionFlow(dropFlow);
            if (itemActionFlow.TryDropSelected(
                InventoryManager.Instance,
                InventoryWorldDropper.Instance,
                interactionFlow,
                visibleItems,
                Vector2.zero,
                gameObject))
            {
                HideActionMenu();
                Refresh();
            }
        }

        private void TrySubscribe()
        {
            if (isSubscribed || InventoryManager.Instance == null)
            {
                return;
            }

            InventoryManager.Instance.ItemAmountChangedById += OnItemAmountChanged;
            InventoryManager.Instance.InventoryOrderChanged += OnInventoryOrderChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || InventoryManager.Instance == null)
            {
                return;
            }

            InventoryManager.Instance.ItemAmountChangedById -= OnItemAmountChanged;
            InventoryManager.Instance.InventoryOrderChanged -= OnInventoryOrderChanged;
            isSubscribed = false;
        }

        private void OnItemAmountChanged(string itemId, int previousAmount, int currentAmount)
        {
            Refresh();
        }

        private void OnInventoryOrderChanged()
        {
            Refresh();
        }

        private InventoryItemStack GetSelectedStack()
        {
            return interactionFlow.GetSelectedStack(visibleItems);
        }

        private void HandleInventoryKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (InventoryKeyboardInput.WasKeyPressed(keyboard, useSelectedKey))
            {
                UseSelectedItem();
                return;
            }

            if (InventoryKeyboardInput.WasKeyPressed(keyboard, dropSelectedKey))
            {
                DropSelectedItemNearPlayer();
                return;
            }

            if (InventoryKeyboardInput.TryGetSlotIndex(keyboard, visibleItems.Count, out int slotIndex))
            {
                SelectSlot(slotIndex, true);
            }
        }

        private void UpdateUseButtons()
        {
            presenter?.UpdateUseButtons(InventoryManager.Instance, interactionFlow, visibleItems);
        }

        private bool TryDropStackAtPointer(int slotIndex, InventoryItemStack stack, PointerEventData eventData)
        {
            Vector2 worldPosition = Camera.main != null
                ? Camera.main.ScreenToWorldPoint(eventData.position)
                : Vector2.zero;

            return TryDropStackAtWorldPosition(slotIndex, stack, worldPosition);
        }

        private bool TryDropStackAtWorldPosition(int slotIndex, InventoryItemStack stack, Vector2 worldPosition)
        {
            if (stack == null || InventoryWorldDropper.Instance == null || InventoryManager.Instance == null)
            {
                GameplayUIEvents.ShowWarning("Nao foi possivel descartar o item.", source: gameObject);
                return false;
            }

            return dropFlow.TryDropFromSlot(
                InventoryManager.Instance,
                InventoryWorldDropper.Instance,
                stack,
                slotIndex,
                worldPosition,
                gameObject);
        }

        private void SetPanelVisible(bool visible)
        {
            view.SetPanelVisible(visible, this, openButtonLabel, closeButtonLabel);
        }

        private void ShowActionMenuNearSlot(int slotIndex)
        {
            EnsureActionMenu();
            if (slotIndex < 0 || slotIndex >= slots.Count)
            {
                return;
            }

            view.ShowActionMenuNear(slots[slotIndex].transform);
        }

        private void HideActionMenu()
        {
            view.HideActionMenu();
        }

        private bool IsPointerOutsideInventory(PointerEventData eventData)
        {
            return view.IsPointerOutsidePanel(eventData, rootCanvas);
        }

        private void CreateDragIcon(InventoryItemStack stack)
        {
            dragIconPresenter.Create(rootCanvas, stack);
        }

        private void DestroyDragIcon()
        {
            dragIconPresenter.Destroy();
        }

        private void EnsureSlotContainer()
        {
            view.Initialize(transform);
        }

        private void EnsureActionMenu()
        {
            view.Initialize(transform);
        }

        private void BindView()
        {
            view.BindReferences(
                inventoryPanel,
                toggleButton,
                toggleButtonText,
                titleText,
                contentText,
                detailsText,
                useButton,
                useButtonText,
                slotContainer,
                slotPrefab,
                actionMenuRoot,
                actionUseButton,
                actionDropButton,
                actionCancelButton);
        }

    }
}
