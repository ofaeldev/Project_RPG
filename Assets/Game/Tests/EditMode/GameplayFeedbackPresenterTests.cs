using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class GameplayFeedbackPresenterTests
    {
        [Test]
        public void TryCreateQuestStateFeedback_ForActiveQuest_CreatesQuestMessage()
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();

            try
            {
                SetField(quest, "title", "Ratos na nevoa");

                bool created = presenter.TryCreateQuestStateFeedback(quest, QuestState.Active, out GameplayFeedbackMessage message);

                Assert.IsTrue(created);
                Assert.AreEqual(FeedbackMessageType.Quest, message.MessageType);
                StringAssert.Contains("Ratos na nevoa", message.Text);
            }
            finally
            {
                Object.DestroyImmediate(quest);
            }
        }

        [Test]
        public void TryCreateItemUseFeedback_WithoutMessage_ReturnsFalse()
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();

            bool created = presenter.TryCreateItemUseFeedback(default, 0, out _);

            Assert.IsFalse(created);
        }

        [Test]
        public void TryCreateLootTakenFeedback_WhenClaimed_CreatesLootMessage()
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();
            TestLootSource lootSource = new("Corpo de Rato");

            bool created = presenter.TryCreateLootTakenFeedback(lootSource, availableStacks: 1, claimedStacks: 1, out GameplayFeedbackMessage message);

            Assert.IsTrue(created);
            Assert.AreEqual(FeedbackMessageType.Loot, message.MessageType);
            Assert.AreEqual("Corpo de Rato: loot coletado.", message.Text);
        }

        [Test]
        public void TryCreateLootTakenFeedback_WhenInventoryFull_CreatesWarning()
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();
            TestLootSource lootSource = new("Bau");

            bool created = presenter.TryCreateLootTakenFeedback(lootSource, availableStacks: 1, claimedStacks: 0, out GameplayFeedbackMessage message);

            Assert.IsTrue(created);
            Assert.AreEqual(FeedbackMessageType.Warning, message.MessageType);
            Assert.AreEqual("Bau: sem espaco no inventario.", message.Text);
        }

        [Test]
        public void TryCreateLootTakenFeedback_WithoutLootSource_CreatesEmptyLootMessage()
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();

            bool created = presenter.TryCreateLootTakenFeedback(null, availableStacks: 0, claimedStacks: 0, out GameplayFeedbackMessage message);

            Assert.IsTrue(created);
            Assert.AreEqual(FeedbackMessageType.Info, message.MessageType);
            Assert.AreEqual("Nada para saquear.", message.Text);
        }

        [Test]
        public void TryCreateInventoryDropFeedback_WhenDropped_CreatesLootMessage()
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();
            InventoryDropEvent dropEvent = new(null, "Apple", null, true, InventoryDropFailureReason.None, null);

            bool created = presenter.TryCreateInventoryDropFeedback(dropEvent, out GameplayFeedbackMessage message);

            Assert.IsTrue(created);
            Assert.AreEqual(FeedbackMessageType.Loot, message.MessageType);
            Assert.AreEqual("Item descartado: Apple", message.Text);
        }

        [TestCase(InventoryDropFailureReason.InvalidItem, "Item invalido para descartar.")]
        [TestCase(InventoryDropFailureReason.BlockedPosition, "Nao da para soltar o item aqui.")]
        [TestCase(InventoryDropFailureReason.MissingDependencies, "Nao foi possivel descartar o item.")]
        [TestCase(InventoryDropFailureReason.RemoveFailed, "Nao foi possivel descartar o item.")]
        public void TryCreateInventoryDropFeedback_WhenFailed_CreatesWarning(
            InventoryDropFailureReason reason,
            string expectedText)
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();
            InventoryDropEvent dropEvent = new(null, string.Empty, null, false, reason, null);

            bool created = presenter.TryCreateInventoryDropFeedback(dropEvent, out GameplayFeedbackMessage message);

            Assert.IsTrue(created);
            Assert.AreEqual(FeedbackMessageType.Warning, message.MessageType);
            Assert.AreEqual(expectedText, message.Text);
        }

        [TestCase(InteractionFeedbackType.EnemyAttackStarted, "Rato", "", FeedbackMessageType.Info, "Atacando Rato.")]
        [TestCase(InteractionFeedbackType.EnemyNoLoot, "Rato", "", FeedbackMessageType.Info, "Rato nao tem loot.")]
        [TestCase(InteractionFeedbackType.EnemyDefeated, "Rato", "", FeedbackMessageType.Success, "Rato derrotado.")]
        [TestCase(InteractionFeedbackType.DoorAlreadyOpen, "Capela", "", FeedbackMessageType.Info, "Porta 'Capela' ja esta aberta.")]
        [TestCase(InteractionFeedbackType.DoorLocked, "Capela", "Chave", FeedbackMessageType.Warning, "Porta 'Capela' trancada. Precisa de Chave.")]
        [TestCase(InteractionFeedbackType.DoorOpened, "Capela", "", FeedbackMessageType.Success, "Porta 'Capela' aberta.")]
        [TestCase(InteractionFeedbackType.ContainerLocked, "Bau", "Chave", FeedbackMessageType.Warning, "Bau trancado. Precisa de Chave.")]
        [TestCase(InteractionFeedbackType.ItemPickupInventoryUnavailable, "Apple", "", FeedbackMessageType.Warning, "Inventario indisponivel.")]
        [TestCase(InteractionFeedbackType.ItemPickedUp, "Apple", "Item obtido", FeedbackMessageType.Loot, "Item obtido: Apple")]
        [TestCase(InteractionFeedbackType.ItemPickupNoSpace, "Apple", "", FeedbackMessageType.Warning, "Sem espaco para Apple.")]
        [TestCase(InteractionFeedbackType.ItemPickupInvalid, "Apple", "", FeedbackMessageType.Warning, "Item invalido.")]
        public void TryCreateInteractionFeedback_CreatesExpectedMessage(
            InteractionFeedbackType feedbackType,
            string displayName,
            string detail,
            FeedbackMessageType expectedType,
            string expectedText)
        {
            GameplayFeedbackPresenter presenter = CreatePresenter();
            InteractionFeedbackEvent feedbackEvent = new(feedbackType, displayName, detail, null);

            bool created = presenter.TryCreateInteractionFeedback(feedbackEvent, out GameplayFeedbackMessage message);

            Assert.IsTrue(created);
            Assert.AreEqual(expectedType, message.MessageType);
            Assert.AreEqual(expectedText, message.Text);
        }

        private static GameplayFeedbackPresenter CreatePresenter()
        {
            return new GameplayFeedbackPresenter(
                "Quest aceita",
                "Quest concluida",
                "Recompensa recebida",
                "Objetivo atualizado");
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(target, value);
        }

        private sealed class TestLootSource : ILootSource
        {
            public TestLootSource(string displayName)
            {
                DisplayName = displayName;
            }

            public string DisplayName { get; }
            public IReadOnlyList<ItemStackDefinition> Loot => System.Array.Empty<ItemStackDefinition>();
            public int ClaimAllLoot() => 0;
        }
    }
}
