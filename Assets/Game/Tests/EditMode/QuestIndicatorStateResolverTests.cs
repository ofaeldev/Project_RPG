using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Tests
{
    public sealed class QuestIndicatorStateResolverTests
    {
        [Test]
        public void Resolve_WithoutQuest_ReturnsHidden()
        {
            Assert.AreEqual(
                QuestIndicatorVisualState.Hidden,
                QuestIndicatorStateResolver.Resolve(null, QuestState.Available));
        }

        [TestCase(QuestState.Locked, QuestIndicatorVisualState.Available)]
        [TestCase(QuestState.Available, QuestIndicatorVisualState.Available)]
        [TestCase(QuestState.Active, QuestIndicatorVisualState.Active)]
        [TestCase(QuestState.Completed, QuestIndicatorVisualState.Completed)]
        [TestCase(QuestState.RewardClaimed, QuestIndicatorVisualState.Hidden)]
        [TestCase(QuestState.Failed, QuestIndicatorVisualState.Hidden)]
        public void Resolve_MapsQuestStateToVisualState(QuestState questState, QuestIndicatorVisualState expected)
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();

            try
            {
                Assert.AreEqual(expected, QuestIndicatorStateResolver.Resolve(quest, questState));
            }
            finally
            {
                Object.DestroyImmediate(quest);
            }
        }
    }
}
