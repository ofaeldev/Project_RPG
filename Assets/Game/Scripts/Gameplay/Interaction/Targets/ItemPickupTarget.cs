using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickupTarget : RightClickActionTarget, IPersistentWorldState
    {
        [Header("Item Pickup")]
        [SerializeField]
        private string displayName = "Item";

        [Tooltip("Preferred item reference. Use this for real content.")]
        [SerializeField]
        private ItemDefinition item;

        [Tooltip("Legacy/string fallback id. Useful while item assets are not created yet.")]
        [SerializeField]
        private string fallbackItemId = string.Empty;

        [SerializeField]
        [Min(1)]
        private int amount = 1;

        [SerializeField]
        [Min(0f)]
        private float pickupRange = 1.25f;

        [SerializeField]
        private bool deactivateOnPickup = true;

        private bool wasPickedUp;

        public ItemDefinition Item => item;
        public string ItemId => item != null ? item.ItemId : fallbackItemId;
        public int Amount => Mathf.Max(1, amount);
        public bool WasPickedUp => wasPickedUp;

        protected virtual string PickupFeedbackPrefix => "Item obtido";

        public void Initialize(ItemDefinition item, string fallbackItemId, int amount, string displayName = null)
        {
            this.item = item;
            this.fallbackItemId = fallbackItemId;
            this.amount = Mathf.Max(1, amount);
            this.displayName = !string.IsNullOrWhiteSpace(displayName)
                ? displayName
                : GetDisplayText();
            wasPickedUp = false;

            if (item != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RegisterItemDefinition(item);
            }
        }

        private void OnEnable()
        {
            if (item != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RegisterItemDefinition(item);
            }
        }

        public override RightClickActionType GetPreferredRightClickAction(Vector2 clickPosition)
        {
            return RightClickActionType.Loot;
        }

        public override float GetActionRange(RightClickActionType actionType)
        {
            return actionType == RightClickActionType.Loot ? pickupRange : 0f;
        }

        public override void PerformRightClickAction(RightClickActionContext context)
        {
            if (wasPickedUp)
            {
                return;
            }

            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning($"Cannot pick up '{displayName}' because InventoryManager is missing.", gameObject);
                GameplayUIEvents.ShowWarning("Inventario indisponivel.", source: gameObject);
                return;
            }

            if (!TryAddToInventory())
            {
                Debug.LogWarning($"ItemPickupTarget '{name}' could not be added to the inventory.", gameObject);
                return;
            }

            wasPickedUp = true;
            string itemText = GetDisplayText();
            Debug.Log($"Picked up '{itemText}' x{Amount}.", gameObject);
            GameplayUIEvents.ShowLoot($"{PickupFeedbackPrefix}: {itemText}", source: gameObject);

            if (deactivateOnPickup)
            {
                gameObject.SetActive(false);
            }
        }

        private bool TryAddToInventory()
        {
            if (item != null)
            {
                if (!InventoryManager.Instance.CanAddItem(item, Amount))
                {
                    GameplayUIEvents.ShowWarning($"Sem espaco para {GetDisplayText()}.", source: gameObject);
                    return false;
                }

                return InventoryManager.Instance.AddItem(item, Amount);
            }

            if (string.IsNullOrWhiteSpace(fallbackItemId))
            {
                GameplayUIEvents.ShowWarning("Item invalido.", source: gameObject);
                return false;
            }

            if (!InventoryManager.Instance.CanAddItem(fallbackItemId, Amount))
            {
                GameplayUIEvents.ShowWarning($"Sem espaco para {GetDisplayText()}.", source: gameObject);
                return false;
            }

            return InventoryManager.Instance.AddItem(fallbackItemId, Amount);
        }

        private string GetDisplayText()
        {
            string itemName = item != null && !string.IsNullOrWhiteSpace(item.DisplayName)
                ? item.DisplayName
                : displayName;

            return Amount > 1 ? $"{itemName} x{Amount}" : itemName;
        }

        public WorldObjectStateSnapshot CaptureWorldState(string worldObjectId)
        {
            return new WorldObjectStateSnapshot(worldObjectId, wasPickedUp);
        }

        public void RestoreWorldState(WorldObjectStateSnapshot snapshot)
        {
            wasPickedUp = snapshot.FlagA;
            if (wasPickedUp && deactivateOnPickup)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
