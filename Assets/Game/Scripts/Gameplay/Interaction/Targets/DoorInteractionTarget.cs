using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class DoorInteractionTarget : RightClickActionTarget, IPersistentWorldState
    {
        [Header("Door")]
        [SerializeField]
        private string displayName = "Door";

        [SerializeField]
        [Min(0f)]
        private float openRange = 1.25f;

        [SerializeField]
        private bool isLocked;

        [SerializeField]
        private bool isOpen;

        [Header("Unlock Requirement")]
        [SerializeField]
        private InventoryRequirement unlockRequirement = new();

        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;

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
            if (isOpen)
            {
                GameplayUIEvents.ShowInfo($"Porta '{displayName}' ja esta aberta.", source: gameObject);
                return;
            }

            if (isLocked && !TryUnlock())
            {
                GameplayUIEvents.ShowWarning($"Porta '{displayName}' trancada. Precisa de {unlockRequirement.GetDisplayText()}.", source: gameObject);
                return;
            }

            isOpen = true;
            GameplayUIEvents.ShowSuccess($"Porta '{displayName}' aberta.", source: gameObject);
        }

        public WorldObjectStateSnapshot CaptureWorldState(string worldObjectId)
        {
            return new WorldObjectStateSnapshot(worldObjectId, isOpen, isLocked);
        }

        public void RestoreWorldState(WorldObjectStateSnapshot snapshot)
        {
            isOpen = snapshot.FlagA;
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
