using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(CombatActor))]
    public sealed class EnemyInteractionTarget : RightClickActionTarget
    {
        [Header("Enemy")]
        [Tooltip("Nome do inimigo para debug e logs.")]
        [SerializeField]
        private string displayName = "Enemy";

        [Tooltip("O inimigo esta morto e pode ser saqueado.")]
        [SerializeField]
        private bool isDead;

        [Tooltip("Permite ataque de longa distancia quando o inimigo nao esta morto.")]
        [SerializeField]
        private bool allowRangedAttack;

        [Tooltip("Distancia maxima para ataques corpo a corpo.")]
        [SerializeField]
        [Min(0f)]
        private float meleeAttackRange = 1.5f;

        [Tooltip("Distancia maxima para ataques a longa distancia.")]
        [SerializeField]
        [Min(0f)]
        private float rangedAttackRange = 4f;

        [Tooltip("Distancia maxima para saquear o inimigo morto.")]
        [SerializeField]
        [Min(0f)]
        private float lootRange = 1.25f;

        [Header("Quest Progress")]
        [Tooltip("Alvo opcional usado para contabilizar kills em missoes.")]
        [SerializeField]
        private QuestKillTarget questKillTarget;

        [Tooltip("Desativa o GameObject imediatamente quando ele e derrotado. Use apenas para placeholders; corpos normalmente usam CorpseDecayController.")]
        [SerializeField]
        private bool deactivateOnDefeat;

        private HealthComponent health;
        private CombatActor combatActor;
        private ILootSource lootSource;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            combatActor = GetComponent<CombatActor>();
            lootSource = GetComponent<ILootSource>();

            if (questKillTarget == null)
            {
                questKillTarget = GetComponent<QuestKillTarget>();
            }
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
                RightClickActionType.Attack => IsDead ? 0f : (allowRangedAttack ? Mathf.Max(meleeAttackRange, rangedAttackRange) : meleeAttackRange),
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
                    Debug.LogWarning($"Cannot start combat against {displayName}: missing attacker or combat actor.", gameObject);
                    return;
                }

                attacker.StartAttacking(combatActor);
                GameplayUIEvents.ShowInfo($"Atacando {displayName}.", source: gameObject);
                return;
            }

            if (context.ActionType == RightClickActionType.Loot)
            {
                if (LootService.Instance != null)
                {
                    LootService.Instance.OpenOrClaimAll(lootSource, gameObject);
                    return;
                }

                GameplayUIEvents.ShowInfo($"{displayName} nao tem loot.", source: gameObject);
                return;
            }

            Debug.Log($"Action '{context.ActionType}' is not supported by {displayName}.", gameObject);
        }

        public void Defeat()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            questKillTarget?.ReportKilled();
            Debug.Log($"{displayName} defeated.", gameObject);
            GameplayUIEvents.ShowSuccess($"{displayName} derrotado.", source: gameObject);

            if (deactivateOnDefeat)
            {
                gameObject.SetActive(false);
            }
        }

        private bool IsDead => isDead || (health != null && health.IsDead);

        private void OnDied(HealthChange change)
        {
            Defeat();
        }
    }
}
