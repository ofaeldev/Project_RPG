using System.Reflection;
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
    }
}
