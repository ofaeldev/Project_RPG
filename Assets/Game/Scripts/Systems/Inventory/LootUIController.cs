using RPGProject.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class LootUIController : MonoBehaviour
    {
        public static LootUIController Instance { get; private set; }

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

        [Header("Input")]
        [SerializeField]
        private Key takeAllKey = Key.T;

        [SerializeField]
        private Key closeKey = Key.Escape;

        private readonly LootPanelView view = new();
        private readonly LootContentFormatter contentFormatter = new();
        private readonly LootPanelActionFlow actionFlow = new();
        private ILootSource currentLootSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            view.BindReferences(lootPanel, titleText, contentText, takeAllButton, closeButton);

            if (view.TakeAllButton != null)
            {
                view.TakeAllButton.onClick.AddListener(TakeAll);
            }

            if (view.CloseButton != null)
            {
                view.CloseButton.onClick.AddListener(Close);
            }

            SetVisible(false);
        }

        private void Update()
        {
            if (!view.IsVisible || Keyboard.current == null)
            {
                return;
            }

            if (LootKeyboardInput.WasTakeAllPressed(Keyboard.current, takeAllKey))
            {
                TakeAll();
                return;
            }

            if (LootKeyboardInput.WasClosePressed(Keyboard.current, closeKey))
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            GameplayInputBlocker.Instance?.UnregisterUIBlocker(this);

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Open(ILootSource lootSource)
        {
            currentLootSource = lootSource;
            Refresh();
            SetVisible(true);
        }

        public void TakeAll()
        {
            if (currentLootSource == null)
            {
                Close();
                return;
            }

            actionFlow.TryTakeAll(currentLootSource, LootService.Instance);
            Close();
        }

        public void Close()
        {
            currentLootSource = null;
            SetVisible(false);
        }

        private void Refresh()
        {
            view.SetContent(currentLootSource, contentFormatter.Format(currentLootSource));
        }

        private void SetVisible(bool visible)
        {
            view.SetVisible(visible, this);
        }
    }
}
