using UnityEngine;

namespace RPGProject.Inputs
{
    public sealed class PlayerInputState
    {
        public Vector2 Movement { get; private set; }
        public Vector2 PointerScreenPosition { get; private set; }
        public Vector2 PointerWorldPosition { get; private set; }
        public bool IsMoving => Movement.sqrMagnitude > 0f;

        public bool SetMovement(Vector2 movement)
        {
            Vector2 nextMovement = Vector2.ClampMagnitude(movement, 1f);
            if (Movement == nextMovement)
            {
                return false;
            }

            Movement = nextMovement;
            return true;
        }

        public bool SetPointer(Vector2 screenPosition, Vector2 worldPosition)
        {
            if (PointerScreenPosition == screenPosition && PointerWorldPosition == worldPosition)
            {
                return false;
            }

            PointerScreenPosition = screenPosition;
            PointerWorldPosition = worldPosition;
            return true;
        }

        public void Reset()
        {
            Movement = Vector2.zero;
            PointerScreenPosition = Vector2.zero;
            PointerWorldPosition = Vector2.zero;
        }
    }
}
