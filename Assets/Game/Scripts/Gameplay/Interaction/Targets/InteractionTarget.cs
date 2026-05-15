using UnityEngine;

namespace RPGProject.Gameplay
{
    public enum InteractionCategory
    {
        World,
        Enemy,
        NPC,
        Container,
        Door,
        Other
    }

    [RequireComponent(typeof(Collider2D))]
    public sealed class InteractionTarget : RightClickActionTarget
    {
        [Header("Interaction Target")]
        [Tooltip("Categoria básica de interação usada pelo clique direito.")]
        [SerializeField]
        private InteractionCategory category = InteractionCategory.Other;

        [Tooltip("Nome amigável do alvo para debug e logs.")]
        [SerializeField]
        private string displayName = "Interaction Target";

        [Tooltip("A ação padrão deste objeto quando ele é clicado.")]
        [SerializeField]
        private RightClickActionType defaultAction = RightClickActionType.Open;

        [Tooltip("O alvo está morto e pode ser saqueado quando estiver na categoria inimigo.")]
        [SerializeField]
        private bool isDead;

        [Tooltip("A distância máxima para executar a ação padrão.")]
        [SerializeField]
        [Min(0f)]
        private float attackRange = 1.5f;

        [Tooltip("Distância máxima para iniciar diálogo.")]
        [SerializeField]
        [Min(0f)]
        private float talkRange = 1.5f;

        [Tooltip("Distância máxima para abrir portas, baús e similares.")]
        [SerializeField]
        [Min(0f)]
        private float openRange = 1.25f;

        [Tooltip("Distância máxima para saquear um inimigo morto.")]
        [SerializeField]
        [Min(0f)]
        private float lootRange = 1.25f;

        [Tooltip("Distância em que o jogador para antes de executar a ação de mover neste alvo.")]
        [SerializeField]
        [Min(0f)]
        private float moveStopDistance = 0f;

        [Header("Attack Options")]
        [Tooltip("Permite que esse alvo seja atacado de longa distância.")]
        [SerializeField]
        private bool allowRangedAttack;

        [Tooltip("Distância de ataque quando o alvo permite ataque a longa distância.")]
        [SerializeField]
        [Min(0f)]
        private float rangedAttackRange = 4f;

        public bool IsDead => isDead;

        public override RightClickActionType GetPreferredRightClickAction(Vector2 clickPosition)
        {
            return category switch
            {
                InteractionCategory.Enemy => isDead ? RightClickActionType.Loot : RightClickActionType.Attack,
                InteractionCategory.NPC => RightClickActionType.Talk,
                InteractionCategory.Container => RightClickActionType.Open,
                InteractionCategory.Door => RightClickActionType.Open,
                InteractionCategory.World => RightClickActionType.Move,
                _ => defaultAction,
            };
        }

        public override float GetActionRange(RightClickActionType actionType)
        {
            return actionType switch
            {
                RightClickActionType.Attack => allowRangedAttack ? Mathf.Max(rangedAttackRange, attackRange) : attackRange,
                RightClickActionType.Talk => talkRange,
                RightClickActionType.Open => openRange,
                RightClickActionType.Loot => lootRange,
                RightClickActionType.Move => moveStopDistance,
                _ => 0f,
            };
        }

        public override void PerformRightClickAction(RightClickActionContext context)
        {
            Debug.Log($"Generic interaction '{context.ActionType}' on '{displayName}'.", gameObject);

            switch (context.ActionType)
            {
                case RightClickActionType.Open:
                    Debug.Log($"Abrindo {displayName}.", gameObject);
                    break;
                case RightClickActionType.Talk:
                    Debug.Log($"Iniciando conversa com {displayName}.", gameObject);
                    break;
                case RightClickActionType.Attack:
                    Debug.Log($"Atacando {displayName}.", gameObject);
                    break;
                case RightClickActionType.Loot:
                    Debug.Log($"Saqueando {displayName}.", gameObject);
                    break;
                case RightClickActionType.Move:
                    Debug.Log($"Movendo até {displayName}.", gameObject);
                    break;
                default:
                    Debug.Log($"Nenhuma ação definida para {displayName}.", gameObject);
                    break;
            }
        }
    }
}
