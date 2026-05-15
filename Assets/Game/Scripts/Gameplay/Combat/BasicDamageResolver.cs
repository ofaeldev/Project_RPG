using UnityEngine;

namespace RPGProject.Gameplay
{
    [CreateAssetMenu(
        fileName = "BasicDamageResolver",
        menuName = "RPG Project/Combat/Basic Damage Resolver")]
    public sealed class BasicDamageResolver : DamageResolver
    {
        public override DamageResult ResolveDamage(DamageContext context)
        {
            int baseDamage = context.AttackSettings != null ? context.AttackSettings.Damage : 0;
            return ResolveBasicDamage(context.Attacker, context.TargetObject, baseDamage);
        }

        public static DamageResult ResolveBasicDamage(GameObject attacker, GameObject target, int baseDamage)
        {
            int attack = CharacterCombatStats.GetAttack(attacker);
            int defense = CharacterCombatStats.GetDefense(target);
            int amount = baseDamage + attack - defense;
            return new DamageResult(Mathf.Max(0, amount));
        }
    }
}
