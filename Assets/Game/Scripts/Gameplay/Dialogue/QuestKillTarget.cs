using RPGProject.Systems;
using UnityEngine;

namespace RPGProject.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class QuestKillTarget : MonoBehaviour
    {
        [Header("Quest Kill Target")]
        [Tooltip("Identificador usado por objetivos de kill. Ex: rat.")]
        [SerializeField]
        private string targetId = string.Empty;

        [Tooltip("Quantidade contabilizada quando este alvo morre.")]
        [SerializeField]
        [Min(1)]
        private int killAmount = 1;

        [Tooltip("Impede que o mesmo alvo conte mais de uma vez.")]
        [SerializeField]
        private bool reportOnlyOnce = true;

        private bool hasReported;
        private HealthComponent health;

        public string TargetId => targetId;
        public bool HasReported => hasReported;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }

            if (health != null)
            {
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        public bool ReportKilled()
        {
            if (reportOnlyOnce && hasReported)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(targetId))
            {
                Debug.LogWarning($"QuestKillTarget on '{name}' does not have a valid targetId.", this);
                return false;
            }

            hasReported = true;
            return QuestManager.Instance != null && QuestManager.Instance.ReportKill(targetId, killAmount);
        }

        private void OnDied(HealthChange change)
        {
            ReportKilled();
        }
    }
}
