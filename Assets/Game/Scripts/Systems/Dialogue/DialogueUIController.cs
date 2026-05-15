using System.Collections.Generic;
using RPGProject.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGProject.Systems
{
    [DisallowMultipleComponent]
    public sealed class DialogueUIController : MonoBehaviour
    {
        [Header("Dialogue UI")]
        [Tooltip("Root panel that contains the dialogue UI.")]
        [SerializeField]
        private GameObject dialoguePanel;

        [Tooltip("Speaker name text.")]
        [SerializeField]
        private TMP_Text speakerNameText;

        [Tooltip("Main dialogue text.")]
        [SerializeField]
        private TMP_Text dialogueText;

        [Tooltip("Dialogue progress text, such as current line / total lines.")]
        [SerializeField]
        private TMP_Text progressText;

        [Tooltip("Button used to advance dialogue, or the first choice when choices are visible.")]
        [SerializeField]
        private Button nextButton;

        [Tooltip("Button used to close dialogue, or the second choice when choices are visible.")]
        [SerializeField]
        private Button closeButton;

        [Tooltip("Optional dedicated choice buttons. If empty, next/close buttons are reused for the first two choices.")]
        [SerializeField]
        private Button[] choiceButtons = new Button[0];

        [Header("Portraits")]
        [Tooltip("Portrait image shown on the left side of the dialogue box, usually the NPC.")]
        [SerializeField]
        private Image leftPortraitImage;

        [Tooltip("Portrait image shown on the right side of the dialogue box, usually the player.")]
        [SerializeField]
        private Image rightPortraitImage;

        [Tooltip("Tint used by the portrait currently speaking.")]
        [SerializeField]
        private Color activePortraitTint = Color.white;

        [Tooltip("Tint used by the portrait that is not currently speaking.")]
        [SerializeField]
        private Color inactivePortraitTint = new Color(1f, 1f, 1f, 0.38f);

        [Tooltip("Fallback color for the left portrait when no sprite is assigned.")]
        [SerializeField]
        private Color leftPortraitPlaceholderColor = new Color(0.36f, 0.58f, 0.94f, 1f);

        [Tooltip("Fallback color for the right portrait when no sprite is assigned.")]
        [SerializeField]
        private Color rightPortraitPlaceholderColor = new Color(0.94f, 0.68f, 0.35f, 1f);

        [Header("Settings")]
        [SerializeField]
        private bool hidePanelOnStart = true;

        [Tooltip("Allows keyboard input to advance dialogue.")]
        [SerializeField]
        private bool allowKeyboardAdvance = true;

        [Header("Typewriter")]
        [Tooltip("Shows dialogue text gradually before advancing to the next line.")]
        [SerializeField]
        private bool useTypewriter = true;

        [Tooltip("Characters revealed per second.")]
        [SerializeField]
        [Min(1f)]
        private float typewriterCharactersPerSecond = 45f;

        private DialogueDefinition currentDialogue;
        private int currentLineIndex;
        private bool isChoosing;
        private bool isTyping;
        private Coroutine typewriterCoroutine;
        private DialogueTypewriter typewriter;
        private string nextButtonDefaultText;
        private string closeButtonDefaultText;
        private string[] choiceButtonDefaultTexts = new string[0];
        private int currentChoiceCount;
        private bool isSubscribed;

        private void Awake()
        {
            if (hidePanelOnStart)
            {
                HidePanel();
            }

            RegisterButtonCallbacks();
            CacheButtonDefaultText();
            typewriter = new DialogueTypewriter(dialogueText);
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            StopTypewriter();
            UnsubscribeFromDialogueManager();
            GameplayInputBlocker.Instance?.UnregisterUIBlocker(this);
        }

        private void Update()
        {
            if (!isSubscribed && DialogueManager.Instance != null)
            {
                TrySubscribe();
            }

            if (!allowKeyboardAdvance)
            {
                return;
            }

            if (isChoosing)
            {
                TrySelectChoiceFromKeyboard();
                return;
            }

            if (IsAdvanceInputPressed())
            {
                AdvanceOrCompleteLine();
            }
        }

        private void RegisterButtonCallbacks()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            if (choiceButtons == null)
            {
                return;
            }

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                Button choiceButton = choiceButtons[i];
                if (choiceButton == null)
                {
                    continue;
                }

                int choiceIndex = i;
                choiceButton.onClick.AddListener(() => SelectChoice(choiceIndex));
            }
        }

        private void TrySubscribe()
        {
            if (isSubscribed || DialogueManager.Instance == null)
            {
                return;
            }

            DialogueManager.Instance.DialogueLineStarted += OnDialogueLineStarted;
            DialogueManager.Instance.DialogueChoicesPresented += OnDialogueChoicesPresented;
            DialogueManager.Instance.DialogueCompleted += OnDialogueCompleted;
            isSubscribed = true;
        }

        private void UnsubscribeFromDialogueManager()
        {
            if (!isSubscribed || DialogueManager.Instance == null)
            {
                return;
            }

            DialogueManager.Instance.DialogueLineStarted -= OnDialogueLineStarted;
            DialogueManager.Instance.DialogueChoicesPresented -= OnDialogueChoicesPresented;
            DialogueManager.Instance.DialogueCompleted -= OnDialogueCompleted;
            isSubscribed = false;
        }

        private void OnDialogueLineStarted(DialogueDefinition dialogue, DialogueLine line, int lineIndex)
        {
            currentDialogue = dialogue;
            currentLineIndex = lineIndex;
            isChoosing = false;
            currentChoiceCount = 0;
            RestoreButtonText();
            UpdateDialogueUI(line, lineIndex);
            ShowPanel();
        }

        private void OnDialogueChoicesPresented(DialogueDefinition dialogue, IReadOnlyList<DialogueChoice> choices)
        {
            currentDialogue = dialogue;
            isChoosing = true;
            currentChoiceCount = GetVisibleChoiceCount(choices);
            CompleteTypewriter();
            UpdateChoiceButtons(choices);
            ShowPanel();
        }

        private void OnDialogueCompleted(DialogueDefinition dialogue)
        {
            isChoosing = false;
            currentChoiceCount = 0;
            StopTypewriter();
            RestoreButtonText();
            HidePanel();
            currentDialogue = null;
            currentLineIndex = -1;
        }

        private void UpdateDialogueUI(DialogueLine line, int lineIndex)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = string.IsNullOrWhiteSpace(line.SpeakerName) ? currentDialogue.DisplayName : line.SpeakerName;
            }

            StartTypewriter(line.Text);
            UpdatePortraits(line);

            if (progressText != null && currentDialogue != null)
            {
                progressText.text = $"{lineIndex + 1}/{currentDialogue.Lines.Count}";
            }

            UpdateButtonStates(lineIndex);
        }

        private void UpdateButtonStates(int lineIndex)
        {
            bool isLastLine = currentDialogue != null && lineIndex >= currentDialogue.Lines.Count - 1;

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(!isLastLine || (currentDialogue.HasChoices && !HasConfiguredChoiceButtons()));
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(isLastLine && !currentDialogue.HasChoices);
            }

            SetConfiguredChoiceButtonsActive(false);
        }

        private void OnNextButtonClicked()
        {
            if (isChoosing)
            {
                SelectChoice(0);
                return;
            }

            AdvanceOrCompleteLine();
        }

        private void OnCloseButtonClicked()
        {
            if (isChoosing)
            {
                SelectChoice(1);
                return;
            }

            AdvanceOrCompleteLine();
        }

        private void ShowPanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            GameplayInputBlocker.Instance?.RegisterUIBlocker(this);
        }

        private void HidePanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            GameplayInputBlocker.Instance?.UnregisterUIBlocker(this);
        }

        private void AdvanceDialogue()
        {
            DialogueManager.Instance?.AdvanceDialogue();
        }

        private void AdvanceOrCompleteLine()
        {
            if (isTyping)
            {
                CompleteTypewriter();
                return;
            }

            AdvanceDialogue();
        }

        private void SelectChoice(int choiceIndex)
        {
            DialogueManager.Instance?.SelectChoice(choiceIndex);
        }

        private void UpdateChoiceButtons(IReadOnlyList<DialogueChoice> choices)
        {
            if (!HasConfiguredChoiceButtons())
            {
                SetChoiceButton(nextButton, choices, 0);
                SetChoiceButton(closeButton, choices, 1);
                return;
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(false);
            }

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                SetChoiceButton(choiceButtons[i], choices, i);
            }
        }

        private static void SetChoiceButton(Button button, IReadOnlyList<DialogueChoice> choices, int index)
        {
            if (button == null)
            {
                return;
            }

            bool hasChoice = choices != null && index >= 0 && index < choices.Count;
            button.gameObject.SetActive(hasChoice);

            if (hasChoice)
            {
                SetButtonText(button, choices[index].Text);
            }
        }

        private void CacheButtonDefaultText()
        {
            nextButtonDefaultText = GetButtonText(nextButton);
            closeButtonDefaultText = GetButtonText(closeButton);

            if (choiceButtons == null)
            {
                choiceButtonDefaultTexts = new string[0];
                return;
            }

            choiceButtonDefaultTexts = new string[choiceButtons.Length];
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                choiceButtonDefaultTexts[i] = GetButtonText(choiceButtons[i]);
            }
        }

        private void RestoreButtonText()
        {
            SetButtonText(nextButton, nextButtonDefaultText);
            SetButtonText(closeButton, closeButtonDefaultText);

            if (choiceButtons == null)
            {
                return;
            }

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                string defaultText = choiceButtonDefaultTexts != null && i < choiceButtonDefaultTexts.Length
                    ? choiceButtonDefaultTexts[i]
                    : string.Empty;
                SetButtonText(choiceButtons[i], defaultText);
            }
        }

        private bool HasConfiguredChoiceButtons()
        {
            return choiceButtons != null && choiceButtons.Length > 0;
        }

        private void SetConfiguredChoiceButtonsActive(bool active)
        {
            if (choiceButtons == null)
            {
                return;
            }

            foreach (Button choiceButton in choiceButtons)
            {
                if (choiceButton != null)
                {
                    choiceButton.gameObject.SetActive(active);
                }
            }
        }

        private void UpdatePortraits(DialogueLine line)
        {
            bool isLeftSpeaker = line.SpeakerSide == DialogueSpeakerSide.Left;
            SetPortraitState(leftPortraitImage, isLeftSpeaker, line, leftPortraitPlaceholderColor);
            SetPortraitState(rightPortraitImage, !isLeftSpeaker, line, rightPortraitPlaceholderColor);
        }

        private void SetPortraitState(Image portraitImage, bool isActiveSpeaker, DialogueLine line, Color placeholderColor)
        {
            if (portraitImage == null)
            {
                return;
            }

            if (isActiveSpeaker && line.Portrait != null)
            {
                portraitImage.sprite = line.Portrait;
            }

            portraitImage.preserveAspect = true;
            if (portraitImage.sprite != null)
            {
                portraitImage.color = isActiveSpeaker ? activePortraitTint : inactivePortraitTint;
                return;
            }

            float alpha = isActiveSpeaker ? placeholderColor.a : Mathf.Min(placeholderColor.a, inactivePortraitTint.a);
            portraitImage.color = new Color(placeholderColor.r, placeholderColor.g, placeholderColor.b, alpha);
        }

        private int GetVisibleChoiceCount(IReadOnlyList<DialogueChoice> choices)
        {
            if (choices == null)
            {
                return 0;
            }

            int buttonCount = HasConfiguredChoiceButtons() ? choiceButtons.Length : 2;
            return Mathf.Min(choices.Count, buttonCount);
        }

        private static string GetButtonText(Button button)
        {
            TMP_Text text = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
            return text != null ? text.text : string.Empty;
        }

        private static void SetButtonText(Button button, string value)
        {
            TMP_Text text = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
            if (text != null)
            {
                text.text = value;
            }
        }

        private bool IsAdvanceInputPressed()
        {
            if (currentDialogue == null || DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive)
            {
                return false;
            }

            Keyboard keyboard = Keyboard.current;
            return DialogueKeyboardInput.WasAdvancePressed(keyboard);
        }

        private void TrySelectChoiceFromKeyboard()
        {
            if (DialogueKeyboardInput.TryGetChoiceIndex(Keyboard.current, currentChoiceCount, out int choiceIndex))
            {
                SelectChoice(choiceIndex);
            }
        }

        private void StartTypewriter(string text)
        {
            StopTypewriter();

            typewriter ??= new DialogueTypewriter(dialogueText);
            if (!typewriter.HasTextTarget)
            {
                return;
            }

            if (!useTypewriter || string.IsNullOrEmpty(text))
            {
                isTyping = false;
                typewriter.SetInstant(text);
                return;
            }

            isTyping = true;
            typewriter.PrepareHidden(text);
            typewriterCoroutine = StartCoroutine(RunTypewriter(text.Length));
        }

        private void CompleteTypewriter()
        {
            if (!isTyping && typewriterCoroutine == null)
            {
                return;
            }

            StopTypewriter();

            typewriter?.Complete();
        }

        private void StopTypewriter()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            isTyping = false;
        }

        private System.Collections.IEnumerator RunTypewriter(int characterCount)
        {
            yield return typewriter.Reveal(characterCount, typewriterCharactersPerSecond);
            isTyping = false;
            typewriterCoroutine = null;
        }
    }
}
