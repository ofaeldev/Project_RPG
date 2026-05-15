using System.Collections.Generic;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ContainerInteractionTarget : RightClickActionTarget, ILootSource, IPersistentWorldState
    {
        [Header("Container")]
        [SerializeField]
        private string displayName = "Container";

        [SerializeField]
        [Min(0f)]
        private float openRange = 1.25f;

        [SerializeField]
        private bool isLocked;

        [SerializeField]
        private bool wasOpened;

        [Header("Unlock Requirement")]
        [SerializeField]
        private InventoryRequirement unlockRequirement = new();

        [Header("Loot")]
        [SerializeField]
        private ItemStackDefinition[] loot = new ItemStackDefinition[0];

        [SerializeField]
        private bool grantLootOnlyOnce = true;

        public string DisplayName => displayName;
        public bool WasOpened => wasOpened;
        public IReadOnlyList<ItemStackDefinition> Loot => loot;

        public override RightClickActionType GetPreferredRightClickAction(Vector2 clickPosition)
        {
            return RightClickActionType.Open;
        }

        public override float GetActionRange(RightClickActionType actionType)
        {
            return actionType == RightClickActionType.Open ? openRange : 0f;
        }

        public override void PerformRightClickAction(RightClickActionContext context)
        {
            if (isLocked && !TryUnlock())
            {
                GameplayUIEvents.ShowWarning($"{displayName} trancado. Precisa de {unlockRequirement.GetDisplayText()}.", source: gameObject);
                return;
            }

            if (grantLootOnlyOnce && wasOpened)
            {
                LootService.Instance?.OpenOrClaimAll(this, gameObject);
                return;
            }

            if (LootService.Instance != null)
            {
                LootService.Instance.OpenOrClaimAll(this, gameObject);
                return;
            }

            ClaimAllLoot();
        }

        public int ClaimAllLoot()
        {
            if (InventoryManager.Instance == null || loot == null)
            {
                return 0;
            }

            int grantedStacks = 0;
            foreach (ItemStackDefinition stack in loot)
            {
                if (stack != null && stack.IsValid && InventoryManager.Instance.AddItem(stack.Item, stack.Amount))
                {
                    grantedStacks++;
                }
            }

            wasOpened = grantLootOnlyOnce || grantedStacks > 0;
            return grantedStacks;
        }

        public WorldObjectStateSnapshot CaptureWorldState(string worldObjectId)
        {
            return new WorldObjectStateSnapshot(worldObjectId, wasOpened, isLocked);
        }

        public void RestoreWorldState(WorldObjectStateSnapshot snapshot)
        {
            wasOpened = snapshot.FlagA;
            isLocked = snapshot.FlagB;
        }

        private bool TryUnlock()
        {
            if (!unlockRequirement.HasRequirement)
            {
                return false;
            }

            if (!unlockRequirement.IsMet())
            {
                return false;
            }

            if (!unlockRequirement.TryConsume())
            {
                return false;
            }

            isLocked = false;
            return true;
        }
    }
}
