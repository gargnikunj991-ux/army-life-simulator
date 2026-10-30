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
        public Slider bgmVolumeSlider;
        public TextMeshProUGUI bgmVolumeLabel;

        [Header("Gameplay Accessibility & Visual Options")]
        public Button screenShakeButton;
        public TextMeshProUGUI screenShakeButtonText;
        public Button riskConfirmButton;
        public TextMeshProUGUI riskConfirmButtonText;
        public Button textSpeedButton;
        public TextMeshProUGUI textSpeedButtonText;
        public Button highContrastButton;
        public TextMeshProUGUI highContrastButtonText;

        [Header("Service Record Management")]
        public Button returnToTitleButton;
        public Button resignCommissionButton;

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
                else if (screenShakeButton == null && (bName.Contains("shake") || bName.Contains("screenshake")))
                {
                    screenShakeButton = b;
                    screenShakeButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (riskConfirmButton == null && (bName.Contains("risk") || bName.Contains("confirm")))
                {
                    riskConfirmButton = b;
                    riskConfirmButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (textSpeedButton == null && (bName.Contains("textspeed") || bName.Contains("speed")))
                {
                    textSpeedButton = b;
                    textSpeedButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
                else if (highContrastButton == null && (bName.Contains("contrast") || bName.Contains("a11y")))
                {
                    highContrastButton = b;
                    highContrastButtonText = b.GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            // Self-healing: ensure DesertTan and NightOps buttons exist in ThemeRow
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

            // Dynamic self-healing: create Accessibility & Visual Options row if absent
            EnsureAccessibilityRow();

            var sliders = GetComponentsInChildren<Slider>(true);
            foreach (var s in sliders)
            {
                string sName = s.gameObject.name.ToLower();
                if (masterVolumeSlider == null && sName.Contains("master")) masterVolumeSlider = s;
                else if (sfxVolumeSlider == null && sName.Contains("sfx")) sfxVolumeSlider = s;
                else if (bgmVolumeSlider == null && (sName.Contains("bgm") || sName.Contains("music") || sName.Contains("ambience"))) bgmVolumeSlider = s;
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
                else if (bgmVolumeLabel == null && (tName.Contains("bvol") || tName.Contains("bgm") || tName.Contains("music"))) bgmVolumeLabel = t;
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

        private void EnsureAccessibilityRow()
        {
            if (paceStandardButton == null || paceStandardButton.transform.parent == null) return;
            var contentParent = paceStandardButton.transform.parent.parent;
            if (contentParent == null) return;

            Transform existingRow = contentParent.Find("VisualA11yRow");
            if (existingRow == null)
            {
                var rowObj = new GameObject("VisualA11yRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                rowObj.transform.SetParent(contentParent, false);
                // Position after pacing description
                if (pacingDescriptionText != null)
                {
                    rowObj.transform.SetSiblingIndex(pacingDescriptionText.transform.GetSiblingIndex() + 1);
                }

                var hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 10;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;

                var le = rowObj.GetComponent<LayoutElement>();
                le.minHeight = 44;
                le.preferredHeight = 44;

                // Screen Shake Button
                var shakeObj = Instantiate(paceStandardButton.gameObject, rowObj.transform);
                shakeObj.name = "ScreenShakeBtn";
                screenShakeButton = shakeObj.GetComponent<Button>();
                screenShakeButtonText = shakeObj.GetComponentInChildren<TextMeshProUGUI>();

                // Risk Warning Confirmation Button
                var riskObj = Instantiate(paceStandardButton.gameObject, rowObj.transform);
                riskObj.name = "RiskConfirmBtn";
                riskConfirmButton = riskObj.GetComponent<Button>();
                riskConfirmButtonText = riskObj.GetComponentInChildren<TextMeshProUGUI>();

                // Text Speed Button
                var speedObj = Instantiate(paceStandardButton.gameObject, rowObj.transform);
                speedObj.name = "TextSpeedBtn";
                textSpeedButton = speedObj.GetComponent<Button>();
                textSpeedButtonText = speedObj.GetComponentInChildren<TextMeshProUGUI>();

                // High Contrast Button
                var contrastObj = Instantiate(paceStandardButton.gameObject, rowObj.transform);
                contrastObj.name = "HighContrastBtn";
                highContrastButton = contrastObj.GetComponent<Button>();
                highContrastButtonText = contrastObj.GetComponentInChildren<TextMeshProUGUI>();
            }
            else
            {
                if (screenShakeButton == null)
                {
                    var t = existingRow.Find("ScreenShakeBtn");
                    if (t != null) { screenShakeButton = t.GetComponent<Button>(); screenShakeButtonText = t.GetComponentInChildren<TextMeshProUGUI>(); }
                }
                if (riskConfirmButton == null)
                {
                    var t = existingRow.Find("RiskConfirmBtn");
                    if (t != null) { riskConfirmButton = t.GetComponent<Button>(); riskConfirmButtonText = t.GetComponentInChildren<TextMeshProUGUI>(); }
                }
                if (textSpeedButton == null)
                {
                    var t = existingRow.Find("TextSpeedBtn");
                    if (t != null) { textSpeedButton = t.GetComponent<Button>(); textSpeedButtonText = t.GetComponentInChildren<TextMeshProUGUI>(); }
                }
                if (highContrastButton == null)
                {
                    var t = existingRow.Find("HighContrastBtn");
                    if (t != null) { highContrastButton = t.GetComponent<Button>(); highContrastButtonText = t.GetComponentInChildren<TextMeshProUGUI>(); }
                }
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

            if (difficultyStoryButton != null) difficultyStoryButton.onClick.AddListener(() => SetDifficulty(GameDifficulty.Story));
            if (difficultyRealisticButton != null) difficultyRealisticButton.onClick.AddListener(() => SetDifficulty(GameDifficulty.Realistic));
            if (difficultyIroncladButton != null) difficultyIroncladButton.onClick.AddListener(() => SetDifficulty(GameDifficulty.Ironclad));

            if (paceStandardButton != null) paceStandardButton.onClick.AddListener(() => SetPacing(SimulationPacing.Standard));
            if (paceFastButton != null) paceFastButton.onClick.AddListener(() => SetPacing(SimulationPacing.Fast));

            if (screenShakeButton != null) screenShakeButton.onClick.AddListener(OnScreenShakeToggled);
            if (riskConfirmButton != null) riskConfirmButton.onClick.AddListener(OnRiskConfirmToggled);
            if (textSpeedButton != null) textSpeedButton.onClick.AddListener(OnTextSpeedClicked);
            if (highContrastButton != null) highContrastButton.onClick.AddListener(OnHighContrastToggled);

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);

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

            if (screenShakeButton != null) screenShakeButton.onClick.RemoveListener(OnScreenShakeToggled);
            if (riskConfirmButton != null) riskConfirmButton.onClick.RemoveListener(OnRiskConfirmToggled);
            if (textSpeedButton != null) textSpeedButton.onClick.RemoveListener(OnTextSpeedClicked);
            if (highContrastButton != null) highContrastButton.onClick.RemoveListener(OnHighContrastToggled);

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);

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
            if (modalRoot != null && modalRoot != gameObject)
            {
                modalRoot.SetActive(false);
            }
            gameObject.SetActive(false);
        }

        public void RefreshUI()
        {
            ThemeMode currentTheme = ThemeManager.Instance.CurrentMode;
            ThemePalette p = ThemeManager.GetPalette(currentTheme);

            UpdateThemeButtonsUI(currentTheme, p);

            GameDifficulty diff = DifficultySettings.CurrentDifficulty;
            UpdateDifficultyUI(diff, p);

            SimulationPacing pace = DifficultySettings.CurrentPacing;
            UpdatePacingUI(pace, p);

            UpdateVisualOptionsUI(p);

            // Audio
            float mVol = PlayerPrefs.GetFloat("ArmyLife_MasterVol", 0.8f);
            float sVol = PlayerPrefs.GetFloat("ArmyLife_SFXVol", 0.9f);
            float bVol = PlayerPrefs.GetFloat("ArmyLife_BgmVol", 0.75f);
            if (masterVolumeSlider != null) masterVolumeSlider.value = mVol;
            if (masterVolumeLabel != null) masterVolumeLabel.text = $"Master Volume: {Mathf.RoundToInt(mVol * 100)}%";
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sVol;
            if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"Sound FX: {Mathf.RoundToInt(sVol * 100)}%";
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = bVol;
            if (bgmVolumeLabel != null) bgmVolumeLabel.text = $"Ambience & Music: {Mathf.RoundToInt(bVol * 100)}%";

            ApplyThemePalette(p);

            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            bool canResign = s != null && !s.isDead && !s.isRetired;
            if (resignCommissionButton != null)
            {
                resignCommissionButton.gameObject.SetActive(canResign);
            }
        }

        // ====================================================================
        // THEME BUTTON HANDLERS
        // ====================================================================

        private void OnDarkThemeClicked()
        {
            ThemeManager.Instance.SetTheme(ThemeMode.Dark);
            RefreshUI();
        }

        private void OnLightThemeClicked()
        {
            ThemeManager.Instance.SetTheme(ThemeMode.Light);
            RefreshUI();
        }

        private void OnDesertThemeClicked()
        {
            ThemeManager.Instance.SetTheme(ThemeMode.DesertTan);
            RefreshUI();
        }

        private void OnNightOpsThemeClicked()
        {
            ThemeManager.Instance.SetTheme(ThemeMode.NightOps);
            RefreshUI();
        }

        private void OnCycleThemeClicked()
        {
            ThemeManager.Instance.CycleNextTheme();
            RefreshUI();
        }

        private void UpdateThemeButtonsUI(ThemeMode mode, ThemePalette p)
        {
            if (darkThemeButtonText != null) darkThemeButtonText.text = mode == ThemeMode.Dark ? "<b>TACTICAL DRAB [ACTIVE]</b>" : "TACTICAL DRAB";
            if (lightThemeButtonText != null) lightThemeButtonText.text = mode == ThemeMode.Light ? "<b>CLASSIFIED DOSSIER [ACTIVE]</b>" : "CLASSIFIED DOSSIER";
            if (desertThemeButtonText != null) desertThemeButtonText.text = mode == ThemeMode.DesertTan ? "<b>DESERT COYOTE [ACTIVE]</b>" : "DESERT COYOTE";
            if (nightOpsThemeButtonText != null) nightOpsThemeButtonText.text = mode == ThemeMode.NightOps ? "<b>NIGHT OPS HUD [ACTIVE]</b>" : "NIGHT OPS HUD";
            if (cycleThemeButtonText != null) cycleThemeButtonText.text = $"THEME: <b>{ThemeManager.GetThemeDisplayName(mode).ToUpper()}</b>";

            StyleButton(darkThemeButton, darkThemeButtonText, mode == ThemeMode.Dark, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            StyleButton(lightThemeButton, lightThemeButtonText, mode == ThemeMode.Light, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            StyleButton(desertThemeButton, desertThemeButtonText, mode == ThemeMode.DesertTan, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            StyleButton(nightOpsThemeButton, nightOpsThemeButtonText, mode == ThemeMode.NightOps, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
        }

        // ====================================================================
        // DIFFICULTY & PACING HANDLERS
        // ====================================================================

        public void SetDifficulty(GameDifficulty diff)
        {
            DifficultySettings.CurrentDifficulty = diff;
            ThemePalette p = ThemeManager.GetPalette(ThemeManager.Instance.CurrentMode);
            UpdateDifficultyUI(diff, p);

            GameEvents.OnFeedEventLogged?.Invoke("SETTINGS",
                $"<color=#00D4FF><b>[DIFFICULTY UPDATED]</b></color> Game Mode set to <b>{diff.ToString().ToUpper()}</b>. {DifficultySettings.GetDifficultyDescription(diff)}");
        }

        private void UpdateDifficultyUI(GameDifficulty diff, ThemePalette p)
        {
            if (difficultyDescriptionText != null)
            {
                difficultyDescriptionText.text = DifficultySettings.GetDifficultyDescription(diff);
            }

            if (difficultyStoryButton != null)
            {
                var txt = difficultyStoryButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = diff == GameDifficulty.Story ? "<b>STORY [ACTIVE]</b>" : "STORY";
                StyleButton(difficultyStoryButton, txt, diff == GameDifficulty.Story, p.accentEmerald, Color.black, p.btnNormalBg, p.btnNormalText);
            }

            if (difficultyRealisticButton != null)
            {
                var txt = difficultyRealisticButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = diff == GameDifficulty.Realistic ? "<b>REALISTIC [ACTIVE]</b>" : "REALISTIC";
                StyleButton(difficultyRealisticButton, txt, diff == GameDifficulty.Realistic, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            }

            if (difficultyIroncladButton != null)
            {
                var txt = difficultyIroncladButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = diff == GameDifficulty.Ironclad ? "<b>IRONCLAD [ACTIVE]</b>" : "IRONCLAD";
                StyleButton(difficultyIroncladButton, txt, diff == GameDifficulty.Ironclad, p.accentCrimson, Color.white, p.btnNormalBg, p.btnNormalText);
            }
        }

        public void SetPacing(SimulationPacing pace)
        {
            DifficultySettings.CurrentPacing = pace;
            ThemePalette p = ThemeManager.GetPalette(ThemeManager.Instance.CurrentMode);
            UpdatePacingUI(pace, p);

            GameEvents.OnFeedEventLogged?.Invoke("SETTINGS",
                $"<color=#00D4FF><b>[PACING UPDATED]</b></color> Simulation pacing set to <b>{pace.ToString().ToUpper()}</b>.");
        }

        private void UpdatePacingUI(SimulationPacing pace, ThemePalette p)
        {
            if (pacingDescriptionText != null)
            {
                pacingDescriptionText.text = DifficultySettings.GetPacingDescription(pace);
            }

            if (paceStandardButton != null)
            {
                var txt = paceStandardButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = pace == SimulationPacing.Standard ? "<b>STANDARD [ACTIVE]</b>" : "STANDARD";
                StyleButton(paceStandardButton, txt, pace == SimulationPacing.Standard, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            }

            if (paceFastButton != null)
            {
                var txt = paceFastButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = pace == SimulationPacing.Fast ? "<b>FAST [ACTIVE]</b>" : "FAST-FORWARD";
                StyleButton(paceFastButton, txt, pace == SimulationPacing.Fast, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            }
        }

        // ====================================================================
        // VISUAL & ACCESSIBILITY OPTIONS
        // ====================================================================

        private void OnScreenShakeToggled()
        {
            DifficultySettings.ScreenShakeEnabled = !DifficultySettings.ScreenShakeEnabled;
            ThemePalette p = ThemeManager.GetPalette(ThemeManager.Instance.CurrentMode);
            UpdateVisualOptionsUI(p);
        }

        private void OnRiskConfirmToggled()
        {
            DifficultySettings.RiskConfirmationEnabled = !DifficultySettings.RiskConfirmationEnabled;
            ThemePalette p = ThemeManager.GetPalette(ThemeManager.Instance.CurrentMode);
            UpdateVisualOptionsUI(p);
        }

        private void OnTextSpeedClicked()
        {
            int next = ((int)DifficultySettings.CurrentTextSpeed + 1) % 3;
            DifficultySettings.CurrentTextSpeed = (TextFlowSpeed)next;
            ThemePalette p = ThemeManager.GetPalette(ThemeManager.Instance.CurrentMode);
            UpdateVisualOptionsUI(p);
        }

        private void OnHighContrastToggled()
        {
            DifficultySettings.HighContrastEnabled = !DifficultySettings.HighContrastEnabled;
            ThemePalette p = ThemeManager.GetPalette(ThemeManager.Instance.CurrentMode);
            UpdateVisualOptionsUI(p);
        }

        private void UpdateVisualOptionsUI(ThemePalette p)
        {
            if (screenShakeButton != null)
            {
                bool on = DifficultySettings.ScreenShakeEnabled;
                if (screenShakeButtonText != null) screenShakeButtonText.text = on ? "SHAKE: <b>ON</b>" : "SHAKE: <b>OFF</b>";
                StyleButton(screenShakeButton, screenShakeButtonText, on, p.accentEmerald, Color.black, p.btnNormalBg, p.btnNormalText);
            }

            if (riskConfirmButton != null)
            {
                bool on = DifficultySettings.RiskConfirmationEnabled;
                if (riskConfirmButtonText != null) riskConfirmButtonText.text = on ? "RISK WARN: <b>ON</b>" : "RISK WARN: <b>OFF</b>";
                StyleButton(riskConfirmButton, riskConfirmButtonText, on, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            }

            if (textSpeedButton != null)
            {
                var spd = DifficultySettings.CurrentTextSpeed;
                if (textSpeedButtonText != null) textSpeedButtonText.text = $"SPEED: <b>{spd.ToString().ToUpper()}</b>";
                StyleButton(textSpeedButton, textSpeedButtonText, spd != TextFlowSpeed.Normal, p.accentCobalt, Color.white, p.btnNormalBg, p.btnNormalText);
            }

            if (highContrastButton != null)
            {
                bool on = DifficultySettings.HighContrastEnabled;
                if (highContrastButtonText != null) highContrastButtonText.text = on ? "CONTRAST: <b>HIGH</b>" : "CONTRAST: <b>STD</b>";
                StyleButton(highContrastButton, highContrastButtonText, on, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            }
        }

        // ====================================================================
        // AUDIO SLIDERS & BUTTONS
        // ====================================================================

        private void OnMasterVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat("ArmyLife_MasterVol", val);
            if (masterVolumeLabel != null) masterVolumeLabel.text = $"Master Volume: {Mathf.RoundToInt(val * 100)}%";
            if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(val);
        }

        private void OnSfxVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat("ArmyLife_SFXVol", val);
            if (sfxVolumeLabel != null) sfxVolumeLabel.text = $"Sound FX: {Mathf.RoundToInt(val * 100)}%";
            if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(val);
        }

        private void OnBgmVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat("ArmyLife_BgmVol", val);
            if (bgmVolumeLabel != null) bgmVolumeLabel.text = $"Ambience & Music: {Mathf.RoundToInt(val * 100)}%";
            if (AudioManager.Instance != null) AudioManager.Instance.SetBgmVolume(val);
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
            UpdateThemeButtonsUI(mode, palette);
            UpdateDifficultyUI(DifficultySettings.CurrentDifficulty, palette);
            UpdatePacingUI(DifficultySettings.CurrentPacing, palette);
            UpdateVisualOptionsUI(palette);
        }

        public void ApplyThemePalette(ThemePalette p)
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
            if (bgmVolumeLabel != null) bgmVolumeLabel.color = p.textPrimary;

            if (closeButton != null)
            {
                StyleButton(closeButton, closeButton.GetComponentInChildren<TextMeshProUGUI>(), false, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            }

            if (returnToTitleButton != null)
            {
                StyleButton(returnToTitleButton, returnToTitleButton.GetComponentInChildren<TextMeshProUGUI>(), false, p.accentGold, Color.black, p.btnNormalBg, p.btnNormalText);
            }

            if (resignCommissionButton != null)
            {
                StyleButton(resignCommissionButton, resignCommissionButton.GetComponentInChildren<TextMeshProUGUI>(), false, p.accentCrimson, Color.white, p.btnNormalBg, p.btnNormalText);
            }

            // Recolor all section header labels inside the modal content
            if (modalRoot != null)
            {
                var texts = modalRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (t.name.EndsWith("Label") || t.name.Contains("Section"))
                    {
                        t.color = p.accentGold;
                    }
                }
            }
        }

        private static void StyleButton(Button btn, TextMeshProUGUI txt, bool isActive, Color activeBg, Color activeText, Color inactiveBg, Color inactiveText)
        {
            if (btn == null) return;
            Color bg = isActive ? activeBg : inactiveBg;
            Color text = isActive ? activeText : inactiveText;

            var colors = btn.colors;
            colors.normalColor = bg;
            colors.highlightedColor = Color.Lerp(bg, Color.white, 0.20f);
            colors.pressedColor = Color.Lerp(bg, Color.black, 0.20f);
            colors.selectedColor = bg;
            btn.colors = colors;

            var img = btn.GetComponent<Image>();
            if (img != null) img.color = bg;

            if (txt != null) txt.color = text;
        }
    }
}
