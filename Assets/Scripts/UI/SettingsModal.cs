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
        public Button desertThemeButton;
        public Button nightOpsThemeButton;
        public Button cycleThemeButton;
        public TextMeshProUGUI darkThemeButtonText;
        public TextMeshProUGUI lightThemeButtonText;
        public TextMeshProUGUI desertThemeButtonText;
        public TextMeshProUGUI nightOpsThemeButtonText;
        public TextMeshProUGUI cycleThemeButtonText;

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
                else if (desertThemeButton == null && (bName.Contains("desert") || bName.Contains("coyote") || bName.Contains("tan") || bName.Contains("arid")))
                {
                    desertThemeButton = b;
                    desertThemeButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (nightOpsThemeButton == null && (bName.Contains("night") || bName.Contains("ops") || bName.Contains("hud") || bName.Contains("phosphor")))
                {
                    nightOpsThemeButton = b;
                    nightOpsThemeButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (cycleThemeButton == null && (bName.Contains("cycle") || bName.Contains("switchtheme")))
                {
                    cycleThemeButton = b;
                    cycleThemeButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (difficultyStoryButton == null && bName.Contains("story")) difficultyStoryButton = b;
                else if (difficultyRealisticButton == null && (bName.Contains("real") || bName.Contains("norm"))) difficultyRealisticButton = b;
                else if (difficultyIroncladButton == null && (bName.Contains("iron") || bName.Contains("hard"))) difficultyIroncladButton = b;
                else if (paceStandardButton == null && (bName.Contains("std") || bName.Contains("standard"))) paceStandardButton = b;
                else if (paceFastButton == null && bName.Contains("fast")) paceFastButton = b;
                else if (returnToTitleButton == null && (bName.Contains("title") || bName.Contains("menu"))) returnToTitleButton = b;
                else if (resignCommissionButton == null && bName.Contains("resign")) resignCommissionButton = b;
            }

            // Runtime self-healing: ensure DesertTan and NightOps buttons exist in ThemeRow
            if (darkThemeButton != null && darkThemeButton.transform.parent != null)
            {
                var themeRowTransform = darkThemeButton.transform.parent;
                if (desertThemeButton == null)
                {
                    var existing = themeRowTransform.Find("DesertThemeBtn");
                    if (existing != null)
                    {
                        desertThemeButton = existing.GetComponent<Button>();
                        desertThemeButtonText = existing.GetComponentInChildren<TextMeshProUGUI>();
                    }
                    else
                    {
                        var obj = Instantiate(darkThemeButton.gameObject, themeRowTransform);
                        obj.name = "DesertThemeBtn";
                        desertThemeButton = obj.GetComponent<Button>();
                        desertThemeButtonText = obj.GetComponentInChildren<TextMeshProUGUI>();
                        if (desertThemeButtonText != null) desertThemeButtonText.text = "DESERT COYOTE";
                    }
                }

                if (nightOpsThemeButton == null)
                {
                    var existing = themeRowTransform.Find("NightOpsThemeBtn");
                    if (existing != null)
                    {
                        nightOpsThemeButton = existing.GetComponent<Button>();
                        nightOpsThemeButtonText = existing.GetComponentInChildren<TextMeshProUGUI>();
                    }
                    else
                    {
                        var obj = Instantiate(darkThemeButton.gameObject, themeRowTransform);
                        obj.name = "NightOpsThemeBtn";
                        nightOpsThemeButton = obj.GetComponent<Button>();
                        nightOpsThemeButtonText = obj.GetComponentInChildren<TextMeshProUGUI>();
                        if (nightOpsThemeButtonText != null) nightOpsThemeButtonText.text = "NIGHT OPS HUD";
                    }
                }
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
            if (desertThemeButton != null) desertThemeButton.onClick.AddListener(OnDesertThemeClicked);
            if (nightOpsThemeButton != null) nightOpsThemeButton.onClick.AddListener(OnNightOpsThemeClicked);
            if (cycleThemeButton != null) cycleThemeButton.onClick.AddListener(OnCycleThemeClicked);

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
            if (desertThemeButton != null) desertThemeButton.onClick.RemoveListener(OnDesertThemeClicked);
            if (nightOpsThemeButton != null) nightOpsThemeButton.onClick.RemoveListener(OnNightOpsThemeClicked);
            if (cycleThemeButton != null) cycleThemeButton.onClick.RemoveListener(OnCycleThemeClicked);

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

        private void OnDesertThemeClicked()
        {
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.SetTheme(ThemeMode.DesertTan);
            }
            UpdateThemeButtonsUI(ThemeMode.DesertTan);
        }

        private void OnNightOpsThemeClicked()
        {
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.SetTheme(ThemeMode.NightOps);
            }
            UpdateThemeButtonsUI(ThemeMode.NightOps);
        }

        private void OnCycleThemeClicked()
        {
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.CycleNextTheme();
                UpdateThemeButtonsUI(ThemeManager.Instance.CurrentMode);
            }
        }

        private void UpdateThemeButtonsUI(ThemeMode mode)
        {
            ThemePalette p = ThemeManager.GetPalette(mode);

            if (darkThemeButtonText != null)
            {
                darkThemeButtonText.text = mode == ThemeMode.Dark ? "<b>TACTICAL DRAB [ACTIVE]</b>" : "TACTICAL DRAB";
            }
            if (lightThemeButtonText != null)
            {
                lightThemeButtonText.text = mode == ThemeMode.Light ? "<b>CLASSIFIED DOSSIER [ACTIVE]</b>" : "CLASSIFIED DOSSIER";
            }
            if (desertThemeButtonText != null)
            {
                desertThemeButtonText.text = mode == ThemeMode.DesertTan ? "<b>DESERT COYOTE [ACTIVE]</b>" : "DESERT COYOTE";
            }
            if (nightOpsThemeButtonText != null)
            {
                nightOpsThemeButtonText.text = mode == ThemeMode.NightOps ? "<b>NIGHT OPS HUD [ACTIVE]</b>" : "NIGHT OPS HUD";
            }
            if (cycleThemeButtonText != null)
            {
                cycleThemeButtonText.text = $"THEME: <b>{ThemeManager.GetThemeDisplayName(mode).ToUpper()}</b>";
            }

            HighlightThemeButton(darkThemeButton, darkThemeButtonText, mode == ThemeMode.Dark, p);
            HighlightThemeButton(lightThemeButton, lightThemeButtonText, mode == ThemeMode.Light, p);
            HighlightThemeButton(desertThemeButton, desertThemeButtonText, mode == ThemeMode.DesertTan, p);
            HighlightThemeButton(nightOpsThemeButton, nightOpsThemeButtonText, mode == ThemeMode.NightOps, p);
        }

        private void HighlightThemeButton(Button btn, TextMeshProUGUI txt, bool isActive, ThemePalette p)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? p.btnAgeGrowBg : p.btnNormalBg;
            }
            if (txt != null)
            {
                txt.color = isActive ? p.btnAgeGrowText : p.btnNormalText;
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
            if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(val);
        }

        private void OnSfxVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat(PREF_SFX_VOL, val);
            if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"Sound FX: {Mathf.RoundToInt(val * 100)}%";
            if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(val);
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
            if (modalBackdropImage != null)
            {
                modalBackdropImage.color = p.mode == ThemeMode.Light 
                    ? new Color(0.12f, 0.12f, 0.10f, 0.75f) 
                    : new Color(0f, 0f, 0f, 0.90f);
            }

            if (dialogPanelImage != null) dialogPanelImage.color = p.cardBackground;
            if (titleText != null) titleText.color = p.accentGold;
            if (difficultyDescriptionText != null) difficultyDescriptionText.color = p.textSecondary;
            if (pacingDescriptionText != null) pacingDescriptionText.color = p.textSecondary;
            if (masterVolumeLabel != null) masterVolumeLabel.color = p.textPrimary;
            if (sfxVolumeLabel != null) sfxVolumeLabel.color = p.textPrimary;

            if (closeButton != null)
            {
                var img = closeButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }

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
