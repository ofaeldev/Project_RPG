using RPGProject.Systems;

namespace RPGProject.Gameplay
{
    public sealed class InventoryRequirementService
    {
        public bool TrySatisfy(InventoryRequirement requirement, IInventoryService inventory)
        {
            return requirement != null
                && requirement.HasRequirement
                && requirement.IsMet(inventory)
                && requirement.TryConsume(inventory);
        }
    }
}
