using System;
using ArmyLife.Core;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class SettingsModal : MonoBehaviour
    {
        public static SettingsModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header")]
        public TextMeshProUGUI titleText;
        public Button closeButton;

        [Header("Theme Selection")]
        public Button darkThemeButton;
        public Button lightThemeButton;
        public TextMeshProUGUI darkThemeButtonText;
        public TextMeshProUGUI lightThemeButtonText;

        [Header("Gameplay Difficulty")]
        public Button difficultyStoryButton;
        public Button difficultyRealisticButton;
        public Button difficultyIroncladButton;
        public TextMeshProUGUI difficultyDescriptionText;

        [Header("Simulation Pacing")]
        public Button paceStandardButton;
        public Button paceFastButton;
        public TextMeshProUGUI pacingDescriptionText;

        [Header("Audio Settings (Future Audio Engine)")]
        public Slider masterVolumeSlider;
        public TextMeshProUGUI masterVolumeLabel;
        public Slider sfxVolumeSlider;
        public TextMeshProUGUI sfxVolumeLabel;

        [Header("Service Record Management")]
        public Button returnToTitleButton;
        public Button resignCommissionButton;

        private const string PREF_DIFFICULTY = "ArmyLife_Difficulty";
        private const string PREF_PACING = "ArmyLife_Pacing";
        private const string PREF_MASTER_VOL = "ArmyLife_MasterVol";
        private const string PREF_SFX_VOL = "ArmyLife_SfxVol";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            if (darkThemeButton != null) darkThemeButton.onClick.AddListener(OnDarkThemeClicked);
            if (lightThemeButton != null) lightThemeButton.onClick.AddListener(OnLightThemeClicked);

            if (difficultyStoryButton != null) difficultyStoryButton.onClick.AddListener(() => SetDifficulty("Story"));
            if (difficultyRealisticButton != null) difficultyRealisticButton.onClick.AddListener(() => SetDifficulty("Realistic"));
            if (difficultyIroncladButton != null) difficultyIroncladButton.onClick.AddListener(() => SetDifficulty("Ironclad"));

            if (paceStandardButton != null) paceStandardButton.onClick.AddListener(() => SetPacing("Standard"));
            if (paceFastButton != null) paceFastButton.onClick.AddListener(() => SetPacing("Fast"));

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

            if (returnToTitleButton != null) returnToTitleButton.onClick.AddListener(OnReturnToTitleClicked);
            if (resignCommissionButton != null) resignCommissionButton.onClick.AddListener(OnResignCommissionClicked);

            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            if (darkThemeButton != null) darkThemeButton.onClick.RemoveListener(OnDarkThemeClicked);
            if (lightThemeButton != null) lightThemeButton.onClick.RemoveListener(OnLightThemeClicked);

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);

            if (returnToTitleButton != null) returnToTitleButton.onClick.RemoveListener(OnReturnToTitleClicked);
            if (resignCommissionButton != null) resignCommissionButton.onClick.RemoveListener(OnResignCommissionClicked);

            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            RefreshUI();
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        public void RefreshUI()
        {
            // Theme button styling
            ThemeMode currentTheme = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            UpdateThemeButtonsUI(currentTheme);

            // Difficulty
            string diff = PlayerPrefs.GetString(PREF_DIFFICULTY, "Realistic");
            UpdateDifficultyUI(diff);

            // Pacing
            string pace = PlayerPrefs.GetString(PREF_PACING, "Standard");
            UpdatePacingUI(pace);

            // Audio
            float mVol = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 0.8f);
            float sVol = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.9f);
            if (masterVolumeSlider != null) masterVolumeSlider.value = mVol;
            if (masterVolumeLabel != null) masterVolumeLabel.text = $"Master Volume: {Mathf.RoundToInt(mVol * 100)}%";
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sVol;
            if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"Sound FX: {Mathf.RoundToInt(sVol * 100)}%";

            // Apply current theme palette to dialog itself
            ApplyThemePalette(ThemeManager.GetPalette(currentTheme));
        }

        private void OnDarkThemeClicked()
        {
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.SetTheme(ThemeMode.Dark);
            }
            UpdateThemeButtonsUI(ThemeMode.Dark);
        }

        private void OnLightThemeClicked()
        {
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.SetTheme(ThemeMode.Light);
            }
            UpdateThemeButtonsUI(ThemeMode.Light);
        }

        private void UpdateThemeButtonsUI(ThemeMode mode)
        {
            if (darkThemeButtonText != null)
            {
                darkThemeButtonText.text = mode == ThemeMode.Dark ? "<b>DARK MODE [ACTIVE]</b>" : "DARK MODE";
            }
            if (lightThemeButtonText != null)
            {
                lightThemeButtonText.text = mode == ThemeMode.Light ? "<b>LIGHT MODE [ACTIVE]</b>" : "LIGHT MODE";
            }
        }

        private void SetDifficulty(string diff)
        {
            PlayerPrefs.SetString(PREF_DIFFICULTY, diff);
            PlayerPrefs.Save();
            UpdateDifficultyUI(diff);

            GameEvents.OnFeedEventLogged?.Invoke("SETTINGS",
                $"<color=#00D4FF>[SETTINGS UPDATED]</color> Game Difficulty set to <b>{diff.ToUpper()}</b>.");
        }

        private void UpdateDifficultyUI(string diff)
        {
            if (difficultyDescriptionText != null)
            {
                switch (diff)
                {
                    case "Story":
                        difficultyDescriptionText.text = "<b>Story Mode:</b> Low combat casualties, forgiving meridian backlashes, higher clan allowances.";
                        break;
                    case "Ironclad":
                        difficultyDescriptionText.text = "<color=#FF4444><b>Ironclad Mode:</b> Brutal battlefield mortality, fatal meridian explosions, strict promotion boards. Permadeath enabled!</color>";
                        break;
                    default:
                        difficultyDescriptionText.text = "<b>Standard Valerian War:</b> Authentic simulation. 40% imperial tax, wartime drafts, competitive peer examinations.";
                        break;
                }
            }
        }

        private void SetPacing(string pace)
        {
            PlayerPrefs.SetString(PREF_PACING, pace);
            PlayerPrefs.Save();
            UpdatePacingUI(pace);
        }

        private void UpdatePacingUI(string pace)
        {
            if (pacingDescriptionText != null)
            {
                pacingDescriptionText.text = pace == "Fast" 
                    ? "<b>Fast-Forward:</b> Instant event resolution without animation delays." 
                    : "<b>Standard Pacing:</b> Full historical log feeds and step-by-step decision intervals.";
            }
        }

        private void OnMasterVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat(PREF_MASTER_VOL, val);
            if (masterVolumeLabel != null) masterVolumeLabel.text = $"Master Volume: {Mathf.RoundToInt(val * 100)}%";
        }

        private void OnSfxVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat(PREF_SFX_VOL, val);
            if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"Sound FX: {Mathf.RoundToInt(val * 100)}%";
        }

        private void OnReturnToTitleClicked()
        {
            CloseModal();
            if (OpeningSceneController.Instance != null)
            {
                OpeningSceneController.Instance.ShowOpeningScreen();
            }
        }

        private void OnResignCommissionClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null && !s.isDead && !s.isRetired)
            {
                s.isRetired = true;
                s.isInCombatDeployment = false;
                GameEvents.OnFeedEventLogged?.Invoke("RETIREMENT",
                    $"<color=#FF8844><b>[EARLY DISCHARGE]</b></color> You voluntarily submitted your resignation papers to Imperial High Command. " +
                    $"You are now a civilian veteran at age {s.age:F1}.");
                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            }
            CloseModal();
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
            UpdateThemeButtonsUI(mode);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (titleText != null) titleText.color = p.textPrimary;
            if (difficultyDescriptionText != null) difficultyDescriptionText.color = p.textSecondary;
            if (pacingDescriptionText != null) pacingDescriptionText.color = p.textSecondary;
            if (masterVolumeLabel != null) masterVolumeLabel.color = p.textPrimary;
            if (sfxVolumeLabel != null) sfxVolumeLabel.color = p.textPrimary;
        }
    }
}
