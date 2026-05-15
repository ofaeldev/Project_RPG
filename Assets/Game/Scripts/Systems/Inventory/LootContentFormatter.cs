using System.Text;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public sealed class LootContentFormatter
    {
        private readonly StringBuilder stringBuilder = new();

        public string Format(ILootSource lootSource)
        {
            if (lootSource == null || lootSource.Loot == null || lootSource.Loot.Count == 0)
            {
                return "Nada para pegar.";
            }

            stringBuilder.Clear();
            foreach (ItemStackDefinition stack in lootSource.Loot)
            {
                if (stack == null || !stack.IsValid)
                {
                    continue;
                }

                string itemName = !string.IsNullOrWhiteSpace(stack.Item.DisplayName)
                    ? stack.Item.DisplayName
                    : stack.Item.ItemId;
                stringBuilder.Append("<b>");
                stringBuilder.Append(itemName);
                stringBuilder.Append("</b> <color=#B8C2D6>x");
                stringBuilder.Append(stack.Amount);
                stringBuilder.AppendLine("</color>");
            }

            return stringBuilder.Length > 0 ? stringBuilder.ToString() : "Nada para pegar.";
        }
    }
}
