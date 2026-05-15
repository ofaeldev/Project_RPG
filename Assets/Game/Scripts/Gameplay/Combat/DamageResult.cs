namespace RPGProject.Gameplay
{
    public readonly struct DamageResult
    {
        public DamageResult(int amount)
        {
            Amount = amount;
        }

        public int Amount { get; }
        public bool HasDamage => Amount > 0;
    }
}
