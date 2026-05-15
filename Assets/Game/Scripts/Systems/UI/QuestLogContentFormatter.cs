using System.Collections.Generic;
using System.Text;

namespace RPGProject.Systems
{
    public sealed class QuestLogContentFormatter
    {
        private readonly StringBuilder stringBuilder = new();

        public string Format(IReadOnlyList<QuestLogEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return "Nenhuma quest ativa.";
            }

            stringBuilder.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                AppendQuest(entries[i]);
                if (i < entries.Count - 1)
                {
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }

        private void AppendQuest(QuestLogEntry entry)
        {
            stringBuilder.Append("<b>");
            stringBuilder.Append(entry.Title);
            stringBuilder.Append("</b> <size=75%><color=#B8C2D6>[");
            stringBuilder.Append(GetStateLabel(entry.State));
            stringBuilder.AppendLine("]</color></size>");

            if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                stringBuilder.AppendLine(entry.Description);
            }

            if (entry.Objectives != null && entry.Objectives.Count > 0)
            {
                foreach (QuestObjectiveLogEntry objective in entry.Objectives)
                {
                    AppendObjective(objective);
                }
            }

            if (!string.IsNullOrWhiteSpace(entry.RewardDescription))
            {
                stringBuilder.Append("<color=#DDBB68>Recompensa: ");
                stringBuilder.Append(entry.RewardDescription);
                stringBuilder.AppendLine("</color>");
            }
        }

        private void AppendObjective(QuestObjectiveLogEntry objective)
        {
            string status = objective.IsComplete ? "<color=#8EE68E>OK</color>" : "<color=#F0D17A>-</color>";
            string objectiveText = string.IsNullOrWhiteSpace(objective.Description) ? objective.ObjectiveId : objective.Description;
            stringBuilder.Append(status);
            stringBuilder.Append(' ');
            stringBuilder.Append(objectiveText);
            stringBuilder.Append(" <color=#B8C2D6>");
            stringBuilder.Append(objective.CurrentAmount);
            stringBuilder.Append('/');
            stringBuilder.Append(objective.RequiredAmount);
            stringBuilder.AppendLine("</color>");
        }

        private static string GetStateLabel(QuestState state)
        {
            return state switch
            {
                QuestState.Active => "Ativa",
                QuestState.Completed => "Completa",
                QuestState.RewardClaimed => "Recompensa recebida",
                QuestState.Failed => "Falhou",
                QuestState.Available => "Disponivel",
                _ => "Bloqueada",
            };
        }
    }
}
