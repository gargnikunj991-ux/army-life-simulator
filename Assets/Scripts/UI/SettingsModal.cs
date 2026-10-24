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

        [Header("Audio Settings (Audio Engine)")]
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
        private const string PREF_SFX_VOL = "ArmyLife_SFXVol";

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

            EnsureUIBindings();

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EnsureUIBindings();
            SetupListeners();
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            RemoveListeners();
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void EnsureUIBindings()
        {
            if (modalRoot == null)
            {
                var overlay = transform.Find("SettingsOverlay");
                if (overlay != null) modalRoot = overlay.gameObject;
            }

            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string bName = b.gameObject.name.ToLower();
                if (closeButton == null && bName.Contains("close")) closeButton = b;
                else if (darkThemeButton == null && bName.Contains("dark"))
                {
                    darkThemeButton = b;
                    darkThemeButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (lightThemeButton == null && bName.Contains("light"))
                {
                    lightThemeButton = b;
                    lightThemeButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (difficultyStoryButton == null && bName.Contains("story")) difficultyStoryButton = b;
                else if (difficultyRealisticButton == null && (bName.Contains("real") || bName.Contains("norm"))) difficultyRealisticButton = b;
                else if (difficultyIroncladButton == null && (bName.Contains("iron") || bName.Contains("hard"))) difficultyIroncladButton = b;
                else if (paceStandardButton == null && (bName.Contains("std") || bName.Contains("standard"))) paceStandardButton = b;
                else if (paceFastButton == null && bName.Contains("fast")) paceFastButton = b;
                else if (returnToTitleButton == null && (bName.Contains("title") || bName.Contains("menu"))) returnToTitleButton = b;
                else if (resignCommissionButton == null && bName.Contains("resign")) resignCommissionButton = b;
            }

            var sliders = GetComponentsInChildren<Slider>(true);
            foreach (var s in sliders)
            {
                string sName = s.gameObject.name.ToLower();
                if (masterVolumeSlider == null && sName.Contains("master")) masterVolumeSlider = s;
                else if (sfxVolumeSlider == null && sName.Contains("sfx")) sfxVolumeSlider = s;
            }

            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                string tName = t.gameObject.name.ToLower();
                if (titleText == null && tName.Contains("title") && !tName.Contains("menu")) titleText = t;
                else if (difficultyDescriptionText == null && (tName.Contains("diffdesc") || tName.Contains("diff_desc"))) difficultyDescriptionText = t;
                else if (pacingDescriptionText == null && (tName.Contains("pacedesc") || tName.Contains("pace_desc"))) pacingDescriptionText = t;
                else if (masterVolumeLabel == null && tName.Contains("mvol")) masterVolumeLabel = t;
                else if (sfxVolumeLabel == null && tName.Contains("svol")) sfxVolumeLabel = t;
            }

            if (modalBackdropImage == null && modalRoot != null)
            {
                modalBackdropImage = modalRoot.GetComponent<Image>();
            }

            if (dialogPanelImage == null)
            {
                var box = transform.Find("SettingsOverlay/SettingsBox");
                if (box != null) dialogPanelImage = box.GetComponent<Image>();
            }
        }

        private void SetupListeners()
        {
            RemoveListeners();

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
        }

        private void RemoveListeners()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            if (darkThemeButton != null) darkThemeButton.onClick.RemoveListener(OnDarkThemeClicked);
            if (lightThemeButton != null) lightThemeButton.onClick.RemoveListener(OnLightThemeClicked);

            if (difficultyStoryButton != null) difficultyStoryButton.onClick.RemoveAllListeners();
            if (difficultyRealisticButton != null) difficultyRealisticButton.onClick.RemoveAllListeners();
            if (difficultyIroncladButton != null) difficultyIroncladButton.onClick.RemoveAllListeners();

            if (paceStandardButton != null) paceStandardButton.onClick.RemoveAllListeners();
            if (paceFastButton != null) paceFastButton.onClick.RemoveAllListeners();

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);

            if (returnToTitleButton != null) returnToTitleButton.onClick.RemoveListener(OnReturnToTitleClicked);
            if (resignCommissionButton != null) resignCommissionButton.onClick.RemoveListener(OnResignCommissionClicked);
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
                modalRoot.transform.SetAsLastSibling();
            }

            EnsureUIBindings();
            SetupListeners();
            RefreshUI();
        }

        public void CloseModal()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
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

            // Resign Commission button: only visible if character exists, alive, and in service
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            bool canResign = s != null && !s.isDead && !s.isRetired;
            if (resignCommissionButton != null)
            {
                resignCommissionButton.gameObject.SetActive(canResign);
            }
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
            UpdateDifficultyButtonsUI(diff);
            if (difficultyDescriptionText != null)
            {
                switch (diff)
                {
                    case "Story":
                        difficultyDescriptionText.text = "<b>Story Mode:</b> Low combat casualties, forgiving court-martials, higher rations.";
                        break;
                    case "Ironclad":
                        difficultyDescriptionText.text = "<color=#FF4444><b>Ironclad Mode:</b> Brutal frontline mortality, strict court-martials, permadeath enabled!</color>";
                        break;
                    default:
                        difficultyDescriptionText.text = "<b>Realistic Mode:</b> Authentic military simulation, 40% war tax, combat tours, competitive promotion boards.";
                        break;
                }
            }
        }

        private void UpdateDifficultyButtonsUI(string diff)
        {
            if (difficultyStoryButton != null)
            {
                var txt = difficultyStoryButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = diff == "Story" ? "<b>STORY [ACTIVE]</b>" : "STORY";
            }
            if (difficultyRealisticButton != null)
            {
                var txt = difficultyRealisticButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = diff == "Realistic" ? "<b>REALISTIC [ACTIVE]</b>" : "REALISTIC";
            }
            if (difficultyIroncladButton != null)
            {
                var txt = difficultyIroncladButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = diff == "Ironclad" ? "<b>IRONCLAD [ACTIVE]</b>" : "IRONCLAD";
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
            UpdatePacingButtonsUI(pace);
            if (pacingDescriptionText != null)
            {
                pacingDescriptionText.text = pace == "Fast" 
                    ? "<b>Fast-Forward:</b> Instant event resolution without animation delays." 
                    : "<b>Standard Pacing:</b> Full historical log feeds and step-by-step decision intervals.";
            }
        }

        private void UpdatePacingButtonsUI(string pace)
        {
            if (paceStandardButton != null)
            {
                var txt = paceStandardButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = pace == "Standard" ? "<b>STANDARD [ACTIVE]</b>" : "STANDARD (DETAILED)";
            }
            if (paceFastButton != null)
            {
                var txt = paceFastButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = pace == "Fast" ? "<b>FAST [ACTIVE]</b>" : "FAST-FORWARD";
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
            if (dialogPanelImage != null) dialogPanelImage.color = p.cardBackground;
            if (titleText != null) titleText.color = p.accentCobalt;
            if (difficultyDescriptionText != null) difficultyDescriptionText.color = p.textSecondary;
            if (pacingDescriptionText != null) pacingDescriptionText.color = p.textSecondary;
            if (masterVolumeLabel != null) masterVolumeLabel.color = p.textPrimary;
            if (sfxVolumeLabel != null) sfxVolumeLabel.color = p.textPrimary;

            // Recolor all section header labels inside the modal content
            if (modalRoot != null)
            {
                var texts = modalRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (t.name.EndsWith("Label") || t.name.Contains("Section"))
                    {
                        t.color = p.textPrimary;
                    }
                }
            }
        }
    }
}
