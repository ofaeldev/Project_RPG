using System;
using NUnit.Framework;
using RPGProject.Systems;

namespace RPGProject.Tests
{
    public sealed class QuestLogContentFormatterTests
    {
        [Test]
        public void Format_ReturnsEmptyMessageWithoutEntries()
        {
            QuestLogContentFormatter formatter = new();

            Assert.AreEqual("Nenhuma quest ativa.", formatter.Format(Array.Empty<QuestLogEntry>()));
        }

        [Test]
        public void Format_IncludesQuestStateObjectiveAndReward()
        {
            QuestLogContentFormatter formatter = new();
            QuestObjectiveLogEntry objective = new("rat", "Derrote ratos", currentAmount: 1, requiredAmount: 3);
            QuestLogEntry entry = new(
                "hunt_rats",
                "Limpar o celeiro",
                "Ajude o fazendeiro.",
                "25 moedas",
                QuestState.Active,
                new[] { objective });

            string result = formatter.Format(new[] { entry });

            StringAssert.Contains("Limpar o celeiro", result);
            StringAssert.Contains("[Ativa]", result);
            StringAssert.Contains("Derrote ratos", result);
            StringAssert.Contains("1/3", result);
            StringAssert.Contains("Recompensa: 25 moedas", result);
        }
    }
}
