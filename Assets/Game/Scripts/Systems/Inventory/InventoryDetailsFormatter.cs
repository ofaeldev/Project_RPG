namespace RPGProject.Systems
{
    public sealed class InventoryDetailsFormatter
    {
        public string FormatEmptySelection()
        {
            return "Selecione um item para ver detalhes.";
        }

        public string Format(InventoryItemStack stack, bool canUse)
        {
            if (stack == null)
            {
                return FormatEmptySelection();
            }

            if (stack.Item == null)
            {
                return $"{stack.ItemId}\nQuantidade: {stack.Amount}";
            }

            string usable = canUse ? "Usavel agora" : "Nao usavel aqui";
            string description = string.IsNullOrWhiteSpace(stack.Item.Description) ? "Sem descricao." : stack.Item.Description;
            string stackLimit = stack.Item.IsStackable ? $" / {stack.Item.MaxStackSize}" : string.Empty;
            return $"<b>{stack.Item.DisplayName}</b>\n{description}\nQuantidade: {stack.Amount}{stackLimit}\nCategoria: {stack.Item.Category}\n{usable}";
        }
    }
}
