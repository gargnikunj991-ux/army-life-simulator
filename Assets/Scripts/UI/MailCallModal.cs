using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    /// <summary>
    /// Full-screen modal presenting mail call letters with vintage military airmail / aged parchment aesthetics.
    /// Displays sender header, military censor seal stamp, letter contents, and 2-3 interactive action buttons.
    /// </summary>
    public class MailCallModal : MonoBehaviour
    {
        public static MailCallModal Instance { get; private set; }

        [Header("Modal Hierarchy")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public RectTransform letterCardPanel;
        public Image letterCardBgImage;

        [Header("Letter Header & Postal Markings")]
        public TextMeshProUGUI headerBannerText;
        public TextMeshProUGUI senderHeaderText;
        public TextMeshProUGUI subjectText;
        public GameObject censorStampContainer;
        public TextMeshProUGUI censorStampText;
        public GameObject remittanceBadgeContainer;
        public TextMeshProUGUI remittanceBadgeText;

        [Header("Letter Body")]
        public ScrollRect letterScrollRect;
        public TextMeshProUGUI letterBodyText;

        [Header("Interactive Action Buttons")]
        public Transform buttonContainer;
        public Button option1Button;
        public TextMeshProUGUI option1ButtonText;
        public Button option2Button;
        public TextMeshProUGUI option2ButtonText;
        public Button option3Button;
        public TextMeshProUGUI option3ButtonText;
        public Button closeButton;

        // Current Letter & Queue State
        public MailLetter CurrentLetter { get; private set; }
        private Queue<MailLetter> mailQueue = new Queue<MailLetter>();
        private Action<MailLetter, int> onOptionSelectedCallback;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsureUIHierarchy();

            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (option1Button != null) option1Button.onClick.AddListener(() => OnOptionButtonClicked(0));
            if (option2Button != null) option2Button.onClick.AddListener(() => OnOptionButtonClicked(1));
            if (option3Button != null) option3Button.onClick.AddListener(() => OnOptionButtonClicked(2));
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
        }

        private void OnDisable()
        {
            if (option1Button != null) option1Button.onClick.RemoveAllListeners();
            if (option2Button != null) option2Button.onClick.RemoveAllListeners();
            if (option3Button != null) option3Button.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Enqueues multiple letters received during the semester and displays the first.
        /// </summary>
        public void OpenMailQueue(List<MailLetter> letters)
        {
            if (letters == null || letters.Count == 0) return;

            mailQueue.Clear();
            foreach (var l in letters)
            {
                if (l != null) mailQueue.Enqueue(l);
            }

            if (mailQueue.Count > 0)
            {
                OpenModal(mailQueue.Dequeue());
            }
        }

        /// <summary>
        /// Opens the modal to display a specific letter.
        /// </summary>
        public void OpenModal(MailLetter letter, Action<MailLetter, int> callback = null)
        {
            if (letter == null) return;

            EnsureUIHierarchy();
            CurrentLetter = letter;
            onOptionSelectedCallback = callback;

            gameObject.SetActive(true);
            if (modalRoot != null) modalRoot.SetActive(true);
            transform.SetAsLastSibling();

            PopulateLetterData(letter);
        }

        /// <summary>
        /// Populates text elements, censor stamps, and response buttons from the letter data.
        /// </summary>
        private void PopulateLetterData(MailLetter letter)
        {
            // Header banner
            if (headerBannerText != null)
            {
                string categoryName = letter.isDearJohn ? "PERSONAL HEARTBREAK" : letter.category.ToString().ToUpper();
                headerBannerText.text = $"<color=#8B0000><b>[VALERIAN FIELD POST • {categoryName}]</b></color>";
            }

            // Sender & Date
            if (senderHeaderText != null)
            {
                string semesterStr = (letter.timestampSemester == 1) ? "Spring/Summer" : "Autumn/Winter";
                senderHeaderText.text = $"<b>FROM:</b> {letter.senderName} ({letter.relation})  |  <b>DATE:</b> {letter.timestampYear}, {semesterStr}";
            }

            // Subject
            if (subjectText != null)
            {
                subjectText.text = $"<b>RE:</b> {letter.subject}";
            }

            // Censor Stamp
            if (censorStampContainer != null)
            {
                censorStampContainer.SetActive(!string.IsNullOrEmpty(letter.censorStampText));
            }
            if (censorStampText != null)
            {
                censorStampText.text = !string.IsNullOrEmpty(letter.censorStampText)
                    ? $"<color=#8B0000><b>* {letter.censorStampText} *</b></color>"
                    : "<color=#8B0000><b>* PASSED CENSOR *</b></color>";
            }

            // Remittance Request Badge
            if (remittanceBadgeContainer != null)
            {
                bool req = letter.requiresRemittance && letter.remittanceAmountRequested > 0;
                remittanceBadgeContainer.SetActive(req);
                if (req && remittanceBadgeText != null)
                {
                    remittanceBadgeText.text = $"<color=#FFD700><b>FINANCIAL REMITTANCE REQUESTED: ${letter.remittanceAmountRequested:N0}</b></color>";
                }
            }

            // Letter Body
            if (letterBodyText != null)
            {
                letterBodyText.text = letter.letterBodyText;
            }

            // Reset scroll position to top
            if (letterScrollRect != null)
            {
                letterScrollRect.normalizedPosition = new Vector2(0f, 1f);
            }

            // Action Buttons Setup
            SetupOptionButtons(letter);
        }

        private void SetupOptionButtons(MailLetter letter)
        {
            var options = letter.responseOptions;
            int count = (options != null) ? options.Count : 0;

            // Option 1
            if (option1Button != null)
            {
                bool active = count > 0;
                option1Button.gameObject.SetActive(active);
                if (active && option1ButtonText != null)
                {
                    option1ButtonText.text = FormatButtonLabel(options[0]);
                }
            }

            // Option 2
            if (option2Button != null)
            {
                bool active = count > 1;
                option2Button.gameObject.SetActive(active);
                if (active && option2ButtonText != null)
                {
                    option2ButtonText.text = FormatButtonLabel(options[1]);
                }
            }

            // Option 3
            if (option3Button != null)
            {
                bool active = count > 2;
                option3Button.gameObject.SetActive(active);
                if (active && option3ButtonText != null)
                {
                    option3ButtonText.text = FormatButtonLabel(options[2]);
                }
            }
        }

        private string FormatButtonLabel(MailResponseOption opt)
        {
            if (opt == null) return "Confirm";
            if (opt.remittanceCost > 0)
            {
                return $"<color=#00FFAA><b>{opt.buttonLabel}</b></color>";
            }
            if (opt.optionId.EndsWith("_ignore") || opt.optionId.EndsWith("_burn") || opt.optionId.EndsWith("_default"))
            {
                return $"<color=#FF7777><b>{opt.buttonLabel}</b></color>";
            }
            return $"<b>{opt.buttonLabel}</b>";
        }

        private void OnOptionButtonClicked(int optionIndex)
        {
            if (CurrentLetter == null) return;

            SoldierProfile soldier = (GameManager.Instance != null) ? GameManager.Instance.currentSoldier : null;

            // If callback provided, trigger it
            onOptionSelectedCallback?.Invoke(CurrentLetter, optionIndex);

            // Execute through MailCallEngine
            if (soldier != null)
            {
                MailCallEngine.ResolveLetterOption(soldier, CurrentLetter, optionIndex);
            }

            // Check if more letters exist in the queue
            if (mailQueue.Count > 0)
            {
                OpenModal(mailQueue.Dequeue(), onOptionSelectedCallback);
            }
            else
            {
                CloseModal();
            }
        }

        /// <summary>
        /// Closes the modal view.
        /// </summary>
        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);

            CurrentLetter = null;
            mailQueue.Clear();

            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                GameEvents.OnSoldierStatsUpdated?.Invoke(GameManager.Instance.currentSoldier);
            }
        }

        /// <summary>
        /// Robust programmatic UI builder that creates full-screen parchment modal elements
        /// if fields are not wired in the editor scene/prefab.
        /// </summary>
        public void EnsureUIHierarchy()
        {
            if (modalRoot != null && letterBodyText != null && option1Button != null) return;

            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
            SetFullScreen(rootRect);

            // 1. Overlay Backdrop
            if (modalRoot == null)
            {
                var overlayObj = CreateUIElement("MailModalOverlay", transform);
                SetFullScreen(overlayObj.GetComponent<RectTransform>());
                modalBackdropImage = overlayObj.AddComponent<Image>();
                modalBackdropImage.color = new Color(0.04f, 0.05f, 0.08f, 0.90f);
                modalRoot = overlayObj;
            }

            // 2. Vintage Letter Parchment Card
            if (letterCardPanel == null)
            {
                var cardObj = CreateUIElement("ParchmentLetterCard", modalRoot.transform);
                letterCardPanel = cardObj.GetComponent<RectTransform>();
                // Center letter card with margin
                letterCardPanel.anchorMin = new Vector2(0.06f, 0.05f);
                letterCardPanel.anchorMax = new Vector2(0.94f, 0.95f);
                letterCardPanel.offsetMin = Vector2.zero;
                letterCardPanel.offsetMax = Vector2.zero;

                letterCardBgImage = cardObj.AddComponent<Image>();
                // Authentic aged parchment paper tone: #EFE4CE
                letterCardBgImage.color = new Color(0.93f, 0.89f, 0.81f, 1f);

                // Vintage Letter Border Frame
                var borderObj = CreateUIElement("BorderFrame", cardObj.transform);
                var borderRect = borderObj.GetComponent<RectTransform>();
                borderRect.anchorMin = new Vector2(0.015f, 0.015f);
                borderRect.anchorMax = new Vector2(0.985f, 0.985f);
                borderRect.offsetMin = Vector2.zero;
                borderRect.offsetMax = Vector2.zero;
                var borderImg = borderObj.AddComponent<Image>();
                borderImg.color = new Color(0.70f, 0.65f, 0.55f, 0.4f);
            }

            // 3. Header Banner Text
            if (headerBannerText == null)
            {
                headerBannerText = CreateText("HeaderBannerText", letterCardPanel, 
                    "VALERIAN FIELD POST • MAIL CALL", 28, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.35f, 0.12f, 0.12f));
                SetRect(headerBannerText.GetComponent<RectTransform>(), 0.05f, 0.92f, 0.95f, 0.98f);
            }

            // 4. Censor Rubber Stamp
            if (censorStampContainer == null)
            {
                censorStampContainer = CreateUIElement("CensorStampBadge", letterCardPanel);
                var stampRect = censorStampContainer.GetComponent<RectTransform>();
                stampRect.anchorMin = new Vector2(0.60f, 0.84f);
                stampRect.anchorMax = new Vector2(0.95f, 0.92f);
                stampRect.offsetMin = Vector2.zero;
                stampRect.offsetMax = Vector2.zero;
                stampRect.localRotation = Quaternion.Euler(0, 0, -4f); // Vintage crooked stamp effect

                var stampBorder = censorStampContainer.AddComponent<Image>();
                stampBorder.color = new Color(0.65f, 0.15f, 0.15f, 0.25f);

                censorStampText = CreateText("StampText", censorStampContainer.transform, 
                    "* PASSED MILITARY CENSOR #42 *", 18, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.65f, 0.12f, 0.12f));
                SetFullScreen(censorStampText.GetComponent<RectTransform>());
            }

            // 5. Sender & Date Text
            if (senderHeaderText == null)
            {
                senderHeaderText = CreateText("SenderHeaderText", letterCardPanel,
                    "FROM: Evelyn Vance (Sweetheart)  |  DATE: Year 2012, Autumn/Winter", 20, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.18f, 0.17f, 0.15f));
                SetRect(senderHeaderText.GetComponent<RectTransform>(), 0.05f, 0.86f, 0.60f, 0.91f);
            }

            // 6. Subject Text
            if (subjectText == null)
            {
                subjectText = CreateText("SubjectText", letterCardPanel,
                    "RE: Pressed Wildflowers & Missing You", 22, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.12f, 0.12f, 0.12f));
                SetRect(subjectText.GetComponent<RectTransform>(), 0.05f, 0.80f, 0.95f, 0.85f);
            }

            // 7. Remittance Requested Badge
            if (remittanceBadgeContainer == null)
            {
                remittanceBadgeContainer = CreateUIElement("RemittanceBadge", letterCardPanel);
                var remRect = remittanceBadgeContainer.GetComponent<RectTransform>();
                remRect.anchorMin = new Vector2(0.05f, 0.74f);
                remRect.anchorMax = new Vector2(0.95f, 0.79f);
                remRect.offsetMin = Vector2.zero;
                remRect.offsetMax = Vector2.zero;

                var remBg = remittanceBadgeContainer.AddComponent<Image>();
                remBg.color = new Color(0.20f, 0.16f, 0.08f, 0.85f);

                remittanceBadgeText = CreateText("RemittanceText", remittanceBadgeContainer.transform,
                    "FINANCIAL REMITTANCE REQUESTED: $400", 20, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.85f, 0.2f));
                SetFullScreen(remittanceBadgeText.GetComponent<RectTransform>());
            }

            // 8. Letter Body Scroll View
            if (letterBodyText == null)
            {
                var scrollObj = CreateUIElement("LetterScrollView", letterCardPanel);
                SetRect(scrollObj.GetComponent<RectTransform>(), 0.05f, 0.26f, 0.95f, 0.73f);
                letterScrollRect = scrollObj.AddComponent<ScrollRect>();
                letterScrollRect.horizontal = false;
                letterScrollRect.vertical = true;
                letterScrollRect.scrollSensitivity = 30f;

                var viewport = CreateUIElement("Viewport", scrollObj.transform);
                SetFullScreen(viewport.GetComponent<RectTransform>());
                viewport.AddComponent<RectMask2D>();
                letterScrollRect.viewport = viewport.GetComponent<RectTransform>();

                var contentObj = CreateUIElement("Content", viewport.transform);
                var contentRect = contentObj.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.sizeDelta = new Vector2(0f, 600f);
                letterScrollRect.content = contentRect;

                letterBodyText = CreateText("LetterBodyText", contentObj.transform,
                    "My Dearest...\n\nKeep your head low in the trenches.", 22, TextAlignmentOptions.TopLeft, FontStyles.Normal, new Color(0.12f, 0.11f, 0.10f));
                SetFullScreen(letterBodyText.GetComponent<RectTransform>());
            }

            // 9. Action Buttons Container & Options
            if (buttonContainer == null)
            {
                var btnContainerObj = CreateUIElement("ButtonContainer", letterCardPanel);
                buttonContainer = btnContainerObj.GetComponent<RectTransform>();
                SetRect(buttonContainer.GetComponent<RectTransform>(), 0.05f, 0.02f, 0.95f, 0.24f);

                var vLayout = btnContainerObj.AddComponent<VerticalLayoutGroup>();
                vLayout.spacing = 8f;
                vLayout.childControlHeight = true;
                vLayout.childControlWidth = true;
                vLayout.childForceExpandHeight = true;
                vLayout.childForceExpandWidth = true;

                // Option 1 Button (Olive green / affirmative)
                option1Button = CreateButton("Option1Button", buttonContainer, out option1ButtonText, 
                    "Send Remittance ($400)", new Color(0.18f, 0.35f, 0.22f));

                // Option 2 Button (Warm brown / secondary)
                option2Button = CreateButton("Option2Button", buttonContainer, out option2ButtonText, 
                    "Write Back Encouragement", new Color(0.35f, 0.26f, 0.18f));

                // Option 3 Button (Dark crimson / ignore)
                option3Button = CreateButton("Option3Button", buttonContainer, out option3ButtonText, 
                    "Burn Letter (Duty First)", new Color(0.40f, 0.15f, 0.15f));
            }
        }

        // =========================================================================
        // UI RECT & ELEMENT HELPER METHODS
        // =========================================================================

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize,
            TextAlignmentOptions alignment, FontStyles fontStyle, Color color)
        {
            var go = CreateUIElement(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, out TextMeshProUGUI label, string text, Color bgColor)
        {
            var go = CreateUIElement(name, parent);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();

            label = CreateText("Label", go.transform, text, 20, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            SetFullScreen(label.GetComponent<RectTransform>());

            return btn;
        }
    }
}
