using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    public enum FeedbackMessageType
    {
        Info,
        Success,
        Warning,
        Error,
        Quest,
        Loot
    }

    [DisallowMultipleComponent]
    public sealed class GlobalFeedbackUIController : MonoBehaviour
    {
        public static GlobalFeedbackUIController Instance { get; private set; }

        [Header("Feedback UI")]
        [SerializeField]
        private GameObject feedbackRoot;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private TMP_Text messageText;

        [Header("Timing")]
        [Tooltip("Tempo padrao, em segundos, que uma mensagem fica visivel.")]
        [SerializeField]
        [Min(0.1f)]
        private float defaultVisibleSeconds = 2.5f;

        [Tooltip("Tempo usado para fade in/out.")]
        [SerializeField]
        [Min(0f)]
        private float fadeSeconds = 0.18f;

        [Tooltip("Tempo minimo antes do mesmo objeto poder mostrar outro feedback igual.")]
        [SerializeField]
        [Min(0f)]
        private float sameSourceCooldownSeconds = 1.2f;

        [Tooltip("Pausa curta entre esconder um feedback e mostrar outro de um objeto diferente.")]
        [SerializeField]
        [Min(0f)]
        private float replacementDelaySeconds = 0.08f;

        [Header("Colors")]
        [SerializeField]
        private Color infoColor = new Color(0.2f, 0.28f, 0.38f, 0.92f);

        [SerializeField]
        private Color successColor = new Color(0.16f, 0.4f, 0.22f, 0.92f);

        [SerializeField]
        private Color warningColor = new Color(0.58f, 0.38f, 0.12f, 0.94f);

        [SerializeField]
        private Color errorColor = new Color(0.55f, 0.16f, 0.16f, 0.94f);

        [SerializeField]
        private Color questColor = new Color(0.24f, 0.21f, 0.5f, 0.94f);

        [SerializeField]
        private Color lootColor = new Color(0.12f, 0.42f, 0.42f, 0.94f);

        private readonly Queue<GameplayFeedbackMessage> pendingMessages = new();
        private readonly FeedbackRateLimiter rateLimiter = new();
        private Coroutine displayRoutine;
        private GameplayFeedbackMessage currentMessage;
        private bool hasCurrentMessage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            HideImmediate();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ShowInfo(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Info, visibleSeconds, source);
        }

        public void ShowSuccess(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Success, visibleSeconds, source);
        }

        public void ShowWarning(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Warning, visibleSeconds, source);
        }

        public void ShowLoot(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Loot, visibleSeconds, source);
        }

        public void ShowQuest(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Quest, visibleSeconds, source);
        }

        public void Show(string message, FeedbackMessageType messageType = FeedbackMessageType.Info, float visibleSeconds = -1f, Object source = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var nextMessage = new GameplayFeedbackMessage(
                message,
                messageType,
                visibleSeconds > 0f ? visibleSeconds : defaultVisibleSeconds,
                source != null ? source.GetInstanceID() : 0);

            if (rateLimiter.ShouldSuppress(nextMessage, hasCurrentMessage, currentMessage, Time.unscaledTime))
            {
                return;
            }

            rateLimiter.Register(nextMessage, Time.unscaledTime, sameSourceCooldownSeconds);

            if (displayRoutine == null)
            {
                pendingMessages.Enqueue(nextMessage);
                displayRoutine = StartCoroutine(DisplayMessagesRoutine());
                return;
            }

            pendingMessages.Clear();
            StopCoroutine(displayRoutine);
            displayRoutine = StartCoroutine(ReplaceMessageRoutine(nextMessage));
        }

        private IEnumerator DisplayMessagesRoutine()
        {
            while (pendingMessages.Count > 0)
            {
                GameplayFeedbackMessage message = pendingMessages.Dequeue();
                yield return DisplayMessageRoutine(message);
            }

            HideImmediate();
            displayRoutine = null;
        }

        private IEnumerator ReplaceMessageRoutine(GameplayFeedbackMessage message)
        {
            if (canvasGroup != null && canvasGroup.alpha > 0f)
            {
                yield return FadeTo(0f);
            }

            if (replacementDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(replacementDelaySeconds);
            }

            yield return DisplayMessageRoutine(message);
            HideImmediate();
            displayRoutine = null;
        }

        private IEnumerator DisplayMessageRoutine(GameplayFeedbackMessage message)
        {
            currentMessage = message;
            hasCurrentMessage = true;
            ApplyMessage(message);
            yield return FadeTo(1f);
            yield return new WaitForSecondsRealtime(message.VisibleSeconds);
            yield return FadeTo(0f);
            hasCurrentMessage = false;
        }

        private void ApplyMessage(GameplayFeedbackMessage message)
        {
            if (feedbackRoot != null)
            {
                feedbackRoot.SetActive(true);
            }

            if (messageText != null)
            {
                messageText.text = message.Text;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = GetColor(message.MessageType);
            }
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (canvasGroup == null || fadeSeconds <= 0f)
            {
                SetAlpha(targetAlpha);
                yield break;
            }

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeSeconds);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        private void HideImmediate()
        {
            SetAlpha(0f);
            hasCurrentMessage = false;

            if (feedbackRoot != null)
            {
                feedbackRoot.SetActive(false);
            }
        }

        private void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }

        private Color GetColor(FeedbackMessageType messageType)
        {
            return messageType switch
            {
                FeedbackMessageType.Success => successColor,
                FeedbackMessageType.Warning => warningColor,
                FeedbackMessageType.Error => errorColor,
                FeedbackMessageType.Quest => questColor,
                FeedbackMessageType.Loot => lootColor,
                _ => infoColor,
            };
        }

    }
}
