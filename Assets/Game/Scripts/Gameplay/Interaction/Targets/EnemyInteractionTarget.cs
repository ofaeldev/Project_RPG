using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(CombatActor))]
    public sealed class EnemyInteractionTarget : RightClickActionTarget
    {
        [Header("Interaction")]
        [Tooltip("Distancia maxima para saquear o inimigo morto.")]
        [SerializeField]
        [Min(0f)]
        private float lootRange = 1.25f;

        [SerializeField]
        private bool isDead;

        [Tooltip("Desativa o GameObject imediatamente quando ele e derrotado. Use apenas para placeholders; corpos normalmente usam CorpseDecayController.")]
        [SerializeField]
        private bool deactivateOnDefeat;

        private HealthComponent health;
        private CombatActor combatActor;
        private ILootSource lootSource;
        private CreatureIdentity identity;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            combatActor = GetComponent<CombatActor>();
            lootSource = GetComponent<ILootSource>();
            identity = GetComponent<CreatureIdentity>();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        public override RightClickActionType GetPreferredRightClickAction(Vector2 clickPosition)
        {
            return IsDead ? RightClickActionType.Loot : RightClickActionType.Attack;
        }

        public override float GetActionRange(RightClickActionType actionType)
        {
            return actionType switch
            {
                RightClickActionType.Attack => IsDead || combatActor == null ? 0f : combatActor.AttackRange,
                RightClickActionType.Loot => lootRange,
                _ => 0f,
            };
        }

        public override void PerformRightClickAction(RightClickActionContext context)
        {
            if (context.ActionType == RightClickActionType.Attack)
            {
                AutoAttackController attacker = context.Actor != null
                    ? context.Actor.GetComponent<AutoAttackController>()
                    : null;

                if (attacker == null || combatActor == null)
                {
                    Debug.LogWarning($"Cannot start combat against {DisplayName}: missing attacker or combat actor.", gameObject);
                    return;
                }

                attacker.StartAttacking(combatActor);
                GameplayUIEvents.ShowInfo($"Atacando {DisplayName}.", source: gameObject);
                return;
            }

            if (context.ActionType == RightClickActionType.Loot)
            {
                if (LootService.Instance != null)
                {
                    LootService.Instance.OpenOrClaimAll(lootSource, gameObject);
                    return;
                }

                GameplayUIEvents.ShowInfo($"{DisplayName} nao tem loot.", source: gameObject);
                return;
            }

            Debug.Log($"Action '{context.ActionType}' is not supported by {DisplayName}.", gameObject);
        }

        public void Defeat()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            Debug.Log($"{DisplayName} defeated.", gameObject);
            GameplayUIEvents.ShowSuccess($"{DisplayName} derrotado.", source: gameObject);

            if (deactivateOnDefeat)
            {
                gameObject.SetActive(false);
            }
        }

        private bool IsDead => isDead || (health != null && health.IsDead);
        private string DisplayName => ResolveIdentity() != null ? identity.DisplayName : gameObject.name;

        private CreatureIdentity ResolveIdentity()
        {
            if (identity == null)
            {
                identity = GetComponent<CreatureIdentity>();
            }

            return identity;
        }

        private void OnDied(HealthChange change)
        {
            Defeat();
        }
    }
}
