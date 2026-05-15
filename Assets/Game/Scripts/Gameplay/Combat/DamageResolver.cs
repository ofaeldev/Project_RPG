using UnityEngine;

namespace RPGProject.Gameplay
{
    public abstract class DamageResolver : ScriptableObject
    {
        public abstract DamageResult ResolveDamage(DamageContext context);
    }
}
