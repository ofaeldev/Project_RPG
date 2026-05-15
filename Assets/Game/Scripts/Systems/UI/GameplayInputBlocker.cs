using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPGProject.Systems
{
    /// <summary>
    /// Centraliza o bloqueio de input de gameplay enquanto menus, dialogos ou cliques de UI estao ativos.
    /// UIs registram seu estado aqui; sistemas de gameplay apenas consultam as propriedades de bloqueio.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayInputBlocker : MonoBehaviour
    {
        public static GameplayInputBlocker Instance { get; private set; }

        [Header("Input Blocking")]
        [Tooltip("Frames extras de bloqueio depois que uma UI fecha, evitando que o clique do botao vaze para o mundo.")]
        [SerializeField]
        [Min(0)]
        private int blockFramesAfterRelease = 2;

        [Tooltip("Bloqueia acoes de gameplay quando o ponteiro esta sobre qualquer elemento de UI.")]
        [SerializeField]
        private bool blockActionsWhenPointerIsOverUI = true;

        private readonly HashSet<int> activeUIBlockerIds = new();
        private int blockedUntilFrame = -1;

        public bool ShouldBlockGameplayInput =>
            activeUIBlockerIds.Count > 0 ||
            Time.frameCount <= blockedUntilFrame;

        public bool ShouldBlockGameplayAction =>
            ShouldBlockGameplayInput ||
            (blockActionsWhenPointerIsOverUI &&
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject());

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

        public void SetBlocker(Object blocker, bool shouldBlock)
        {
            SetUIBlocker(blocker, shouldBlock);
        }

        public void SetUIBlocker(Object blocker, bool shouldBlock)
        {
            if (shouldBlock)
            {
                RegisterUIBlocker(blocker);
            }
            else
            {
                UnregisterUIBlocker(blocker);
            }
        }

        public void RegisterBlocker(Object blocker)
        {
            RegisterUIBlocker(blocker);
        }

        public void UnregisterBlocker(Object blocker)
        {
            UnregisterUIBlocker(blocker);
        }

        public void RegisterUIBlocker(Object blocker)
        {
            if (blocker == null)
            {
                return;
            }

            activeUIBlockerIds.Add(blocker.GetInstanceID());
        }

        public void UnregisterUIBlocker(Object blocker)
        {
            if (blocker == null)
            {
                return;
            }

            if (activeUIBlockerIds.Remove(blocker.GetInstanceID()))
            {
                blockedUntilFrame = Mathf.Max(blockedUntilFrame, Time.frameCount + blockFramesAfterRelease);
            }
        }
    }
}
