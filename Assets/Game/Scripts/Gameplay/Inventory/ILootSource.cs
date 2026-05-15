using System.Collections.Generic;

namespace RPGProject.Gameplay
{
    public interface ILootSource
    {
        string DisplayName { get; }
        IReadOnlyList<ItemStackDefinition> Loot { get; }
        int ClaimAllLoot();
    }
}
