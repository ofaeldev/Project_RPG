using Unity.Cinemachine;
using UnityEngine;

namespace RPGProject.Systems
{
    /// <summary>
    /// Configura a Cinemachine Virtual Camera para seguir o jogador em runtime.
    /// Util enquanto o setup de cenas estiver sendo criado e garante robustez.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CinemachinePlayerFollower : MonoBehaviour
    {
        [Header("Player Target")]
        [SerializeField]
        [Tooltip("Referencia preferida para o jogador. Use o nome apenas como fallback de migracao.")]
        private Transform playerTarget;

        [SerializeField]
        [Tooltip("Nome fallback do GameObject do jogador que a camera deve seguir.")]
        private string playerName = "Player";

        private CinemachineCamera virtualCamera;

        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineCamera>();
            ConfigureFollowTarget();
        }

        private void ConfigureFollowTarget()
        {
            if (playerTarget == null)
            {
                GameObject player = GameObject.Find(playerName);
                playerTarget = player != null ? player.transform : null;
            }

            if (playerTarget == null)
            {
                Debug.LogError($"CinemachinePlayerFollower nao encontrou o jogador '{playerName}'.", this);
                return;
            }

            virtualCamera.Follow = playerTarget;
            virtualCamera.LookAt = playerTarget;
        }
    }
}
