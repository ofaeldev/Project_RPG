using UnityEngine;

namespace RPGProject.Gameplay
{
    public enum RightClickActionType
    {
        None,
        Attack,
        Talk,
        Open,
        Loot,
        Move
    }

    public readonly struct RightClickActionContext
    {
        public Vector2 ClickPosition { get; }
        public GameObject Actor { get; }
        public GameObject Target { get; }
        public RightClickActionType ActionType { get; }

        public RightClickActionContext(Vector2 clickPosition, GameObject actor, GameObject target, RightClickActionType actionType)
        {
            ClickPosition = clickPosition;
            Actor = actor;
            Target = target;
            ActionType = actionType;
        }
    }

    public interface IRightClickActionTarget
    {
        GameObject GameObject { get; }
        RightClickActionType GetPreferredRightClickAction(Vector2 clickPosition);
        float GetActionRange(RightClickActionType actionType);
        void PerformRightClickAction(RightClickActionContext context);
    }

    [RequireComponent(typeof(Collider2D))]
    public abstract class RightClickActionTarget : MonoBehaviour, IRightClickActionTarget
    {
        public GameObject GameObject => gameObject;

        public abstract RightClickActionType GetPreferredRightClickAction(Vector2 clickPosition);
        public abstract float GetActionRange(RightClickActionType actionType);

        public virtual void PerformRightClickAction(RightClickActionContext context)
        {
            Debug.Log($"Right-click action '{context.ActionType}' on '{gameObject.name}' at {context.ClickPosition}.", gameObject);
        }
    }
}
