using System;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class DilemmaModal : MonoBehaviour
    {
        public static DilemmaModal Instance { get; private set; }

        [Header("UI Panel Elements")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;
        public TextMeshProUGUI categoryBadgeText;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;

        [Header("Choice A")]
        public Button choiceAButton;
        public TextMeshProUGUI choiceAText;

        [Header("Choice B")]
        public Button choiceBButton;
        public TextMeshProUGUI choiceBText;

        [Header("Choice C")]
        public Button choiceCButton;
        public TextMeshProUGUI choiceCText;

        private Action onChoiceASelected;
        private Action onChoiceBSelected;
        private Action onChoiceCSelected;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnChildhoodDilemmaTriggered += DisplayDilemma;
            ThemeManager.OnThemeChanged += OnThemeChanged;

            if (choiceAButton != null) choiceAButton.onClick.AddListener(OnChoiceAClicked);
            if (choiceBButton != null) choiceBButton.onClick.AddListener(OnChoiceBClicked);
            if (choiceCButton != null) choiceCButton.onClick.AddListener(OnChoiceCClicked);

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            ApplyThemePalette(p);
        }

        private void OnDisable()
        {
            GameEvents.OnChildhoodDilemmaTriggered -= DisplayDilemma;
            ThemeManager.OnThemeChanged -= OnThemeChanged;

            if (choiceAButton != null) choiceAButton.onClick.RemoveListener(OnChoiceAClicked);
            if (choiceBButton != null) choiceBButton.onClick.RemoveListener(OnChoiceBClicked);
            if (choiceCButton != null) choiceCButton.onClick.RemoveListener(OnChoiceCClicked);
        }

        public void DisplayDilemma(
            string category, 
            string title, 
            string description, 
            string choiceALabel, 
            string choiceBLabel, 
            string choiceCLabel, 
            Action actionA, 
            Action actionB, 
            Action actionC)
        {
            onChoiceASelected = actionA;
            onChoiceBSelected = actionB;
            onChoiceCSelected = actionC;

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            string hexBadge = ColorUtility.ToHtmlStringRGB(p.accentCoyote);

            if (categoryBadgeText != null) categoryBadgeText.text = $"<color=#{hexBadge}><b>[{category.ToUpper()}]</b></color>";
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;

            if (choiceAText != null) choiceAText.text = choiceALabel;
            if (choiceBText != null) choiceBText.text = choiceBLabel;

            bool hasChoiceC = !string.IsNullOrEmpty(choiceCLabel);
            if (choiceCButton != null)
            {
                choiceCButton.gameObject.SetActive(hasChoiceC);
                if (hasChoiceC && choiceCText != null)
                {
                    choiceCText.text = choiceCLabel;
                }
            }

            var rtA = choiceAButton != null ? choiceAButton.GetComponent<RectTransform>() : null;
            var rtB = choiceBButton != null ? choiceBButton.GetComponent<RectTransform>() : null;
            if (!hasChoiceC && rtA != null && rtB != null)
            {
                rtA.anchorMin = new Vector2(0.06f, 0.20f);
                rtA.anchorMax = new Vector2(0.94f, 0.35f);
                rtB.anchorMin = new Vector2(0.06f, 0.04f);
                rtB.anchorMax = new Vector2(0.94f, 0.19f);
            }
            else if (hasChoiceC && rtA != null && rtB != null)
            {
                rtA.anchorMin = new Vector2(0.06f, 0.29f);
                rtA.anchorMax = new Vector2(0.94f, 0.41f);
                rtB.anchorMin = new Vector2(0.06f, 0.15f);
                rtB.anchorMax = new Vector2(0.94f, 0.27f);
            }

            if (descriptionText != null)
            {
                descriptionText.enableAutoSizing = true;
                descriptionText.fontSizeMin = 16f;
                descriptionText.fontSizeMax = 28f;
            }
            if (choiceAText != null)
            {
                choiceAText.enableAutoSizing = true;
                choiceAText.fontSizeMin = 14f;
                choiceAText.fontSizeMax = 24f;
            }
            if (choiceBText != null)
            {
                choiceBText.enableAutoSizing = true;
                choiceBText.fontSizeMin = 14f;
                choiceBText.fontSizeMax = 24f;
            }

            ApplyThemePalette(p);

            AudioManager.PlayAlertPing();

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (modalBackdropImage != null)
            {
                modalBackdropImage.color = p.mode == ThemeMode.Light 
                    ? new Color(0.12f, 0.12f, 0.10f, 0.75f) 
                    : new Color(0f, 0f, 0f, 0.90f);
            }

            if (dialogPanelImage != null) dialogPanelImage.color = p.cardBackground;
            if (titleText != null) titleText.color = p.accentGold;
            if (descriptionText != null) descriptionText.color = p.textPrimary;

            StyleChoiceButton(choiceAButton, choiceAText, p);
            StyleChoiceButton(choiceBButton, choiceBText, p);
            StyleChoiceButton(choiceCButton, choiceCText, p);
        }

        private void StyleChoiceButton(Button btn, TextMeshProUGUI txt, ThemePalette p)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = p.btnNormalBg;
            if (txt != null) txt.color = p.btnNormalText;
        }

        private void OnChoiceAClicked()
        {
            AudioManager.PlayUiClick();
            Action callback = onChoiceASelected;
            CloseAndRefresh();
            callback?.Invoke();
        }

        private void OnChoiceBClicked()
        {
            AudioManager.PlayUiClick();
            Action callback = onChoiceBSelected;
            CloseAndRefresh();
            callback?.Invoke();
        }

        private void OnChoiceCClicked()
        {
            AudioManager.PlayUiClick();
            Action callback = onChoiceCSelected;
            CloseAndRefresh();
            callback?.Invoke();
        }

        private void CloseAndRefresh()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }

            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                GameEvents.OnSoldierStatsUpdated?.Invoke(GameManager.Instance.currentSoldier);
            }
        }
    }
}
