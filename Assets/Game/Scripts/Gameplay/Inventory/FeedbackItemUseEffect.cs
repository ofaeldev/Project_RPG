using UnityEngine;

namespace RPGProject.Gameplay
{
    [CreateAssetMenu(menuName = "RPG/Inventory/Use Effects/Feedback", fileName = "NewFeedbackUseEffect")]
    public sealed class FeedbackItemUseEffect : ItemUseEffect
    {
        [Header("Context")]
        [Tooltip("Quando ativo, o item so pode ser usado se houver um alvo valido no contexto.")]
        [SerializeField]
        private bool requiresTarget;

        [Tooltip("Mensagem mostrada quando o jogador tenta usar este item sem alvo.")]
        [SerializeField]
        private string missingTargetMessage = "Use este item no alvo correto.";

        [Header("Feedback")]
        [TextArea(2, 4)]
        [SerializeField]
        private string feedbackMessage = "Voce usou o item.";

        public override bool CanUse(ItemDefinition item, ItemUseContext context)
        {
            return base.CanUse(item, context) && (!requiresTarget || context.Target != null);
        }

        protected override ItemUseResult OnUse(ItemDefinition item, ItemUseContext context)
        {
            if (requiresTarget && context.Target == null)
            {
                return ItemUseResult.Failed(missingTargetMessage);
            }

            string itemName = item != null ? item.DisplayName : "item";
            string message = string.IsNullOrWhiteSpace(feedbackMessage)
                ? $"Voce usou {itemName}."
                : feedbackMessage.Replace("{item}", itemName);

            return ItemUseResult.Success(message);
        }
    }
}
