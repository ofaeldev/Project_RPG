using RPGProject.Gameplay;
using TMPro;
using UnityEngine;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NPCInteractionTarget))]
    public sealed class NPCQuestIndicator : MonoBehaviour
    {
        private enum IndicatorVisualState
        {
            Hidden,
            Available,
            Active,
            Completed
        }

        [Header("Indicator")]
        [SerializeField]
        private TextMeshPro indicatorText;

        [SerializeField]
        private Vector3 worldOffset = new Vector3(0f, 1.15f, 0f);

        [SerializeField]
        private float fontSize = 4f;

        [SerializeField]
        [Min(0f)]
        private int sortingOrder = 40;

        [Header("Symbols")]
        [SerializeField]
        private string availableSymbol = "!";

        [SerializeField]
        private string activeSymbol = "...";

        [SerializeField]
        private string completedSymbol = "?";

        [Header("Colors")]
        [SerializeField]
        private Color availableColor = new Color(1f, 0.86f, 0.24f, 1f);

        [SerializeField]
        private Color activeColor = new Color(0.62f, 0.78f, 1f, 1f);

        [SerializeField]
        private Color completedColor = new Color(0.42f, 1f, 0.46f, 1f);

        [Header("Polish")]
        [SerializeField]
        [Min(0f)]
        private float bobAmplitude = 0.06f;

        [SerializeField]
        [Min(0f)]
        private float bobFrequency = 1.6f;

        [SerializeField]
        [Min(0f)]
        private float pulseAmount = 0.08f;

        [SerializeField]
        [Min(0f)]
        private float pulseFrequency = 1.25f;

        private NPCInteractionTarget npcTarget;
        private bool isSubscribed;
        private IndicatorVisualState visualState = IndicatorVisualState.Hidden;
        private float animationSeed;

        private void Awake()
        {
            npcTarget = GetComponent<NPCInteractionTarget>();
            animationSeed = Mathf.Abs(transform.position.x * 7.13f + transform.position.y * 3.91f);
            EnsureIndicator();
            Refresh();
        }

        private void OnEnable()
        {
            TrySubscribe();
            Refresh();
        }

        private void Update()
        {
            if (!isSubscribed)
            {
                TrySubscribe();
            }

            FaceCamera();
            AnimateIndicator();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (fontSize < 0f)
            {
                fontSize = 0f;
            }

            if (indicatorText != null)
            {
                ApplyTextDefaults();
                Refresh();
            }
        }
#endif

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Refresh()
        {
            EnsureIndicator();

            if (indicatorText == null || npcTarget == null || npcTarget.QuestToOffer == null)
            {
                SetVisible(false);
                return;
            }

            ApplyState(GetVisualState());
        }

        private IndicatorVisualState GetVisualState()
        {
            if (npcTarget == null || npcTarget.QuestToOffer == null)
            {
                return IndicatorVisualState.Hidden;
            }

            QuestState questState = QuestManager.Instance != null
                ? QuestManager.Instance.GetQuestState(npcTarget.QuestToOffer.QuestId)
                : QuestState.Available;

            return questState switch
            {
                QuestState.Locked => IndicatorVisualState.Available,
                QuestState.Available => IndicatorVisualState.Available,
                QuestState.Active => IndicatorVisualState.Active,
                QuestState.Completed => IndicatorVisualState.Completed,
                _ => IndicatorVisualState.Hidden
            };
        }

        private void ApplyState(IndicatorVisualState nextState)
        {
            visualState = nextState;

            switch (visualState)
            {
                case IndicatorVisualState.Available:
                    ApplyIndicator(availableSymbol, availableColor);
                    return;
                case IndicatorVisualState.Active:
                    ApplyIndicator(activeSymbol, activeColor);
                    return;
                case IndicatorVisualState.Completed:
                    ApplyIndicator(completedSymbol, completedColor);
                    return;
                default:
                    SetVisible(false);
                    return;
            }
        }

        private void EnsureIndicator()
        {
            if (indicatorText != null)
            {
                return;
            }

            Transform existing = transform.Find("QuestIndicator");
            GameObject indicatorObject = existing != null ? existing.gameObject : new GameObject("QuestIndicator");
            indicatorObject.transform.SetParent(transform, false);
            indicatorObject.transform.localPosition = worldOffset;
            indicatorText = indicatorObject.GetComponent<TextMeshPro>();
            if (indicatorText == null)
            {
                indicatorText = indicatorObject.AddComponent<TextMeshPro>();
            }

            ApplyTextDefaults();
        }

        private void ApplyTextDefaults()
        {
            indicatorText.alignment = TextAlignmentOptions.Center;
            indicatorText.fontSize = fontSize;
            indicatorText.textWrappingMode = TextWrappingModes.NoWrap;
            indicatorText.sortingOrder = sortingOrder;
            indicatorText.fontStyle = FontStyles.Bold;
        }

        private void ApplyIndicator(string symbol, Color color)
        {
            indicatorText.text = symbol;
            indicatorText.color = color;
            indicatorText.fontSize = fontSize;
            indicatorText.transform.localPosition = worldOffset;
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            if (indicatorText != null)
            {
                indicatorText.gameObject.SetActive(visible);
            }
        }

        private void FaceCamera()
        {
            if (indicatorText == null || Camera.main == null)
            {
                return;
            }

            indicatorText.transform.rotation = Camera.main.transform.rotation;
        }

        private void AnimateIndicator()
        {
            if (indicatorText == null || visualState == IndicatorVisualState.Hidden)
            {
                return;
            }

            float time = Time.time + animationSeed;
            float bob = bobAmplitude > 0f && bobFrequency > 0f
                ? Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude
                : 0f;
            float pulse = pulseAmount > 0f && pulseFrequency > 0f
                ? 1f + Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f) * pulseAmount
                : 1f;

            indicatorText.transform.localPosition = worldOffset + Vector3.up * bob;
            indicatorText.fontSize = fontSize * pulse;
        }

        private void TrySubscribe()
        {
            if (isSubscribed || QuestManager.Instance == null)
            {
                return;
            }

            QuestManager.Instance.QuestStateChanged += OnQuestStateChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || QuestManager.Instance == null)
            {
                return;
            }

            QuestManager.Instance.QuestStateChanged -= OnQuestStateChanged;
            isSubscribed = false;
        }

        private void OnQuestStateChanged(QuestDefinition quest, QuestState state)
        {
            if (npcTarget != null && npcTarget.QuestToOffer == quest)
            {
                Refresh();
            }
        }
    }
}
