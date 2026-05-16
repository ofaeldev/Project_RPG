using System.Collections.Generic;

namespace RPGProject.Gameplay
{
    public static class LootSourceAvailability
    {
        public static bool HasAvailableLoot(ILootSource source)
        {
            return source != null && HasAvailableLoot(source.Loot);
        }

        public static bool HasAvailableLoot(IReadOnlyList<ItemStackDefinition> loot)
        {
            if (loot == null)
            {
                return false;
            }

            for (int i = 0; i < loot.Count; i++)
            {
                if (loot[i] != null && loot[i].IsValid)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
