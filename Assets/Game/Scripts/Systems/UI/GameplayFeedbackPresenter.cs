using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public sealed class GameplayFeedbackPresenter
    {
        private readonly string questAcceptedPrefix;
        private readonly string questCompletedPrefix;
        private readonly string questRewardPrefix;
        private readonly string objectiveUpdatedPrefix;

        public GameplayFeedbackPresenter(
            string questAcceptedPrefix,
            string questCompletedPrefix,
            string questRewardPrefix,
            string objectiveUpdatedPrefix)
        {
            this.questAcceptedPrefix = questAcceptedPrefix;
            this.questCompletedPrefix = questCompletedPrefix;
            this.questRewardPrefix = questRewardPrefix;
            this.objectiveUpdatedPrefix = objectiveUpdatedPrefix;
        }

        public bool TryCreateQuestStateFeedback(QuestDefinition quest, QuestState state, out GameplayFeedbackMessage message)
        {
            message = default;
            if (quest == null)
            {
                return false;
            }

            switch (state)
            {
                case QuestState.Active:
                    message = new GameplayFeedbackMessage($"{questAcceptedPrefix}: {quest.Title}", FeedbackMessageType.Quest, -1f, quest.GetInstanceID());
                    return true;
                case QuestState.Completed:
                    message = new GameplayFeedbackMessage($"{questCompletedPrefix}: {quest.Title}", FeedbackMessageType.Quest, -1f, quest.GetInstanceID());
                    return true;
                case QuestState.Failed:
                    message = new GameplayFeedbackMessage($"Quest falhou: {quest.Title}", FeedbackMessageType.Warning, -1f, quest.GetInstanceID());
                    return true;
                default:
                    return false;
            }
        }

        public bool TryCreateObjectiveFeedback(
            QuestDefinition quest,
            QuestObjectiveDefinition objective,
            QuestObjectiveProgress progress,
            out GameplayFeedbackMessage message)
        {
            message = default;
            if (quest == null || objective == null || progress == null)
            {
                return false;
            }

            string objectiveText = string.IsNullOrWhiteSpace(objective.Description) ? objective.ObjectiveId : objective.Description;
            message = new GameplayFeedbackMessage(
                $"{objectiveUpdatedPrefix}: {objectiveText} {progress.CurrentAmount}/{progress.RequiredAmount}",
                FeedbackMessageType.Quest,
                -1f,
                quest.GetInstanceID());
            return true;
        }

        public bool TryCreateRewardFeedback(QuestDefinition quest, out GameplayFeedbackMessage message)
        {
            message = default;
            if (quest == null)
            {
                return false;
            }

            string reward = string.IsNullOrWhiteSpace(quest.RewardDescription) ? quest.Title : quest.RewardDescription;
            message = new GameplayFeedbackMessage($"{questRewardPrefix}: {reward}", FeedbackMessageType.Success, -1f, quest.GetInstanceID());
            return true;
        }

        public bool TryCreateItemUseFeedback(ItemUseResult result, int sourceId, out GameplayFeedbackMessage message)
        {
            message = default;
            if (string.IsNullOrWhiteSpace(result.FeedbackMessage))
            {
                return false;
            }

            message = new GameplayFeedbackMessage(
                result.FeedbackMessage,
                result.WasUsed ? FeedbackMessageType.Success : FeedbackMessageType.Warning,
                -1f,
                sourceId);
            return true;
        }

        public bool TryCreateLootTakenFeedback(
            ILootSource lootSource,
            int availableStacks,
            int claimedStacks,
            out GameplayFeedbackMessage message)
        {
            message = default;
            string displayName = lootSource != null && !string.IsNullOrWhiteSpace(lootSource.DisplayName)
                ? lootSource.DisplayName
                : "Loot";

            if (claimedStacks > 0)
            {
                message = new GameplayFeedbackMessage($"{displayName}: loot coletado.", FeedbackMessageType.Loot, -1f, 0);
                return true;
            }

            if (availableStacks > 0)
            {
                message = new GameplayFeedbackMessage($"{displayName}: sem espaco no inventario.", FeedbackMessageType.Warning, -1f, 0);
                return true;
            }

            message = new GameplayFeedbackMessage(
                lootSource == null ? "Nada para saquear." : $"{displayName} vazio.",
                FeedbackMessageType.Info,
                -1f,
                0);
            return true;
        }

        public bool TryCreateInventoryDropFeedback(InventoryDropEvent dropEvent, out GameplayFeedbackMessage message)
        {
            message = default;
            if (dropEvent.Succeeded)
            {
                string itemName = !string.IsNullOrWhiteSpace(dropEvent.ItemName)
                    ? dropEvent.ItemName
                    : ResolveStackDisplayName(dropEvent.Stack);
                message = new GameplayFeedbackMessage($"Item descartado: {itemName}", FeedbackMessageType.Loot, -1f, 0);
                return true;
            }

            string text = dropEvent.FailureReason switch
            {
                InventoryDropFailureReason.InvalidItem => "Item invalido para descartar.",
                InventoryDropFailureReason.BlockedPosition => "Nao da para soltar o item aqui.",
                _ => "Nao foi possivel descartar o item."
            };

            message = new GameplayFeedbackMessage(text, FeedbackMessageType.Warning, -1f, 0);
            return true;
        }

        public bool TryCreateInteractionFeedback(InteractionFeedbackEvent feedbackEvent, out GameplayFeedbackMessage message)
        {
            message = default;
            string name = feedbackEvent.DisplayName;
            string detail = feedbackEvent.Detail;

            switch (feedbackEvent.FeedbackType)
            {
                case InteractionFeedbackType.EnemyAttackStarted:
                    message = new GameplayFeedbackMessage($"Atacando {name}.", FeedbackMessageType.Info, -1f, 0);
                    return true;
                case InteractionFeedbackType.EnemyNoLoot:
                    message = new GameplayFeedbackMessage($"{name} nao tem loot.", FeedbackMessageType.Info, -1f, 0);
                    return true;
                case InteractionFeedbackType.EnemyDefeated:
                    message = new GameplayFeedbackMessage($"{name} derrotado.", FeedbackMessageType.Success, -1f, 0);
                    return true;
                case InteractionFeedbackType.DoorAlreadyOpen:
                    message = new GameplayFeedbackMessage($"Porta '{name}' ja esta aberta.", FeedbackMessageType.Info, -1f, 0);
                    return true;
                case InteractionFeedbackType.DoorLocked:
                    message = new GameplayFeedbackMessage($"Porta '{name}' trancada. Precisa de {detail}.", FeedbackMessageType.Warning, -1f, 0);
                    return true;
                case InteractionFeedbackType.DoorOpened:
                    message = new GameplayFeedbackMessage($"Porta '{name}' aberta.", FeedbackMessageType.Success, -1f, 0);
                    return true;
                case InteractionFeedbackType.ContainerLocked:
                    message = new GameplayFeedbackMessage($"{name} trancado. Precisa de {detail}.", FeedbackMessageType.Warning, -1f, 0);
                    return true;
                case InteractionFeedbackType.ItemPickupInventoryUnavailable:
                    message = new GameplayFeedbackMessage("Inventario indisponivel.", FeedbackMessageType.Warning, -1f, 0);
                    return true;
                case InteractionFeedbackType.ItemPickedUp:
                    message = new GameplayFeedbackMessage($"{detail}: {name}", FeedbackMessageType.Loot, -1f, 0);
                    return true;
                case InteractionFeedbackType.ItemPickupNoSpace:
                    message = new GameplayFeedbackMessage($"Sem espaco para {name}.", FeedbackMessageType.Warning, -1f, 0);
                    return true;
                case InteractionFeedbackType.ItemPickupInvalid:
                    message = new GameplayFeedbackMessage("Item invalido.", FeedbackMessageType.Warning, -1f, 0);
                    return true;
                default:
                    return false;
            }
        }

        private static string ResolveStackDisplayName(InventoryItemStack stack)
        {
            if (stack == null)
            {
                return string.Empty;
            }

            return stack.Item != null && !string.IsNullOrWhiteSpace(stack.Item.DisplayName)
                ? stack.Item.DisplayName
                : stack.ItemId;
        }
    }
}
