using UnityEngine;

namespace RPGProject.Gameplay
{
    public readonly struct ItemUseContext
    {
        public GameObject User { get; }
        public GameObject Target { get; }

        public ItemUseContext(GameObject user, GameObject target = null)
        {
            User = user;
            Target = target;
        }
    }

    public readonly struct ItemUseResult
    {
        public bool WasUsed { get; }
        public string FeedbackMessage { get; }

        public ItemUseResult(bool wasUsed, string feedbackMessage)
        {
            WasUsed = wasUsed;
            FeedbackMessage = feedbackMessage;
        }

        public static ItemUseResult Success(string feedbackMessage)
        {
            return new ItemUseResult(true, feedbackMessage);
        }

        public static ItemUseResult Failed(string feedbackMessage)
        {
            return new ItemUseResult(false, feedbackMessage);
        }
    }

    public abstract class ItemUseEffect : ScriptableObject
    {
        [Tooltip("Fallback message shown when the item cannot be used.")]
        [SerializeField]
        private string cannotUseMessage = "Este item nao pode ser usado agora.";

        public virtual bool CanUse(ItemDefinition item, ItemUseContext context)
        {
            return item != null;
        }

        public ItemUseResult Use(ItemDefinition item, ItemUseContext context)
        {
            if (!CanUse(item, context))
            {
                return ItemUseResult.Failed(cannotUseMessage);
            }

            return OnUse(item, context);
        }

        protected abstract ItemUseResult OnUse(ItemDefinition item, ItemUseContext context);
    }
}
