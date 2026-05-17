using RPGProject.Gameplay;
using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    /// <summary>
    /// Responsavel por transformar um item do inventario em um pickup no mundo.
    /// Mantem a logica de drop separada da UI e reaproveita ItemPickupTarget.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InventoryWorldDropper : MonoBehaviour
    {
        public static InventoryWorldDropper Instance { get; private set; }

        [Header("Drop")]
        [SerializeField]
        private Transform defaultDropOrigin;

        [SerializeField]
        [Min(0.1f)]
        private float fallbackDropDistance = 0.8f;

        [SerializeField]
        [Min(0.05f)]
        private float pickupColliderRadius = 0.22f;

        [Tooltip("Camadas que impedem o drop. Use para paredes, props solidos e areas invalidas.")]
        [SerializeField]
        private LayerMask blockedDropMask = Physics2D.DefaultRaycastLayers;

        [SerializeField]
        private string droppedItemPrefix = "Drop";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool TryDrop(ItemDefinition item, string fallbackItemId, int amount, Vector2 requestedWorldPosition, out GameObject droppedObject)
        {
            return TryDrop(item, fallbackItemId, amount, requestedWorldPosition, out droppedObject, out _, out _);
        }

        public bool TryDrop(
            ItemDefinition item,
            string fallbackItemId,
            int amount,
            Vector2 requestedWorldPosition,
            out GameObject droppedObject,
            out InventoryDropFailureReason failureReason,
            out string itemName)
        {
            droppedObject = null;
            failureReason = InventoryDropFailureReason.None;
            itemName = ResolveItemName(item, fallbackItemId);

            if ((item == null || !item.HasValidId) && string.IsNullOrWhiteSpace(fallbackItemId))
            {
                failureReason = InventoryDropFailureReason.InvalidItem;
                return false;
            }

            Vector2 dropPosition = ResolveDropPosition(requestedWorldPosition);
            if (IsBlocked(dropPosition))
            {
                failureReason = InventoryDropFailureReason.BlockedPosition;
                return false;
            }

            droppedObject = new GameObject($"{droppedItemPrefix}_{itemName}");
            droppedObject.transform.position = dropPosition;

            var spriteRenderer = droppedObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = item != null ? item.Icon : null;
            spriteRenderer.color = spriteRenderer.sprite != null ? Color.white : GetFallbackColor(item);
            spriteRenderer.sortingOrder = 2;

            if (spriteRenderer.sprite == null)
            {
                AddFallbackLabel(droppedObject.transform, itemName);
            }

            var collider = droppedObject.AddComponent<CircleCollider2D>();
            collider.radius = pickupColliderRadius;
            collider.isTrigger = true;

            var pickup = droppedObject.AddComponent<ItemPickupTarget>();
            pickup.Initialize(item, item != null ? item.ItemId : fallbackItemId, amount, itemName);

            return true;
        }

        private static string ResolveItemName(ItemDefinition item, string fallbackItemId)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.DisplayName))
            {
                return item.DisplayName;
            }

            return fallbackItemId;
        }

        private Vector2 ResolveDropPosition(Vector2 requestedWorldPosition)
        {
            if (requestedWorldPosition.sqrMagnitude > 0.001f)
            {
                return requestedWorldPosition;
            }

            if (defaultDropOrigin != null)
            {
                return (Vector2)defaultDropOrigin.position + Vector2.down * fallbackDropDistance;
            }

            return transform.position;
        }

        private bool IsBlocked(Vector2 worldPosition)
        {
            if (blockedDropMask.value == 0)
            {
                return false;
            }

            return Physics2D.OverlapCircle(worldPosition, pickupColliderRadius, blockedDropMask) != null;
        }

        private static Color GetFallbackColor(ItemDefinition item)
        {
            if (item == null)
            {
                return new Color(0.70f, 0.76f, 0.82f, 1f);
            }

            return item.Category switch
            {
                ItemCategory.Key => new Color(0.96f, 0.76f, 0.30f, 1f),
                ItemCategory.Consumable => new Color(0.45f, 0.84f, 0.47f, 1f),
                ItemCategory.Quest => new Color(0.56f, 0.62f, 0.98f, 1f),
                ItemCategory.Equipment => new Color(0.90f, 0.48f, 0.42f, 1f),
                ItemCategory.Currency => new Color(1f, 0.88f, 0.36f, 1f),
                _ => new Color(0.70f, 0.76f, 0.82f, 1f),
            };
        }

        private static void AddFallbackLabel(Transform parent, string itemName)
        {
            var labelObject = new GameObject("FallbackIcon", typeof(RectTransform), typeof(TextMeshPro));
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = Vector3.zero;

            TMP_Text label = labelObject.GetComponent<TMP_Text>();
            label.text = string.IsNullOrWhiteSpace(itemName) ? "?" : itemName.Substring(0, 1).ToUpperInvariant();
            label.fontSize = 4;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.95f, 0.92f, 0.78f, 1f);
        }
    }
}
