using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class LootService : MonoBehaviour
    {
        public static LootService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void OpenOrClaimAll(ILootSource lootSource, Object feedbackSource = null)
        {
            if (lootSource == null)
            {
                GameplayUIEvents.ShowInfo("Nada para saquear.", source: feedbackSource);
                return;
            }

            if (LootUIController.Instance != null)
            {
                LootUIController.Instance.Open(lootSource);
                return;
            }

            ClaimAll(lootSource, feedbackSource);
        }

        public int ClaimAll(ILootSource lootSource, Object feedbackSource = null)
        {
            if (lootSource == null)
            {
                GameplayUIEvents.ShowInfo("Nada para saquear.", source: feedbackSource);
                return 0;
            }

            int availableStacks = CountAvailableStacks(lootSource);
            int claimedStacks = lootSource.ClaimAllLoot();
            ShowClaimFeedback(lootSource, availableStacks, claimedStacks, feedbackSource);
            return claimedStacks;
        }

        private static int CountAvailableStacks(ILootSource lootSource)
        {
            if (lootSource.Loot == null)
            {
                return 0;
            }

            int count = 0;
            foreach (ItemStackDefinition stack in lootSource.Loot)
            {
                if (stack != null && stack.IsValid)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ShowClaimFeedback(ILootSource lootSource, int availableStacks, int claimedStacks, Object feedbackSource)
        {
            string displayName = string.IsNullOrWhiteSpace(lootSource.DisplayName) ? "Loot" : lootSource.DisplayName;
            if (claimedStacks > 0)
            {
                GameplayUIEvents.ShowLoot($"{displayName}: loot coletado.", source: feedbackSource);
                return;
            }

            if (availableStacks > 0)
            {
                GameplayUIEvents.ShowWarning($"{displayName}: sem espaco no inventario.", source: feedbackSource);
                return;
            }

            GameplayUIEvents.ShowInfo($"{displayName} vazio.", source: feedbackSource);
        }
    }
}
