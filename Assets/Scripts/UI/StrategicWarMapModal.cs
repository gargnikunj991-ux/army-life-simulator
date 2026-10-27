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
    /// Interactive Strategic Campaign War Map Modal:
    /// Visualizes frontline territory momentum (-100 to +100) across all 5 authentic combat theaters,
    /// macro-campaign Allied dominance score, emergency supply airdrops, and theater transfer petitions.
    /// </summary>
    public class StrategicWarMapModal : MonoBehaviour
    {
        public static StrategicWarMapModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header Elements")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;
        public TextMeshProUGUI macroDominanceText;
        public Button closeButton;

        [Header("Scroll Container")]
        public Transform theatersContainer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            EnsureUIStructure();

            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void OpenModal()
        {
            EnsureUIStructure();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            RefreshUI();

            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void RefreshUI()
        {
            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (soldier == null) return;

            float macroScore = DynamicWarFrontEngine.GetMacroWarDominanceScore();

            if (titleText != null)
            {
                titleText.text = "SUPREME ALLIED HIGH COMMAND • STRATEGIC WAR MAP";
            }

            if (subtitleText != null)
            {
                string rank = soldier.currentRank != null ? soldier.currentRank.rankName : "Soldier";
                string theater = WarTheaterInfo.GetInfo(soldier.currentTheater).theaterName;
                subtitleText.text = $"Deploying Officer: <color=#FFD700><b>{rank} {soldier.characterName}</b></color> • Current Post: <color=#00FFAA><b>{theater}</b></color>";
            }

            if (macroDominanceText != null)
            {
                string colorHex = macroScore >= 60f ? "#00FFAA" : (macroScore >= 40f ? "#FFD700" : "#FF5555");
                string statusWord = macroScore >= 60f ? "ALLIED STRATEGIC ADVANTAGE" : (macroScore >= 40f ? "CONTESTED FRONTLINE STALEMATE" : "CRITICAL ENEMY PRESSURE");
                macroDominanceText.text = $"Overall War Dominance: <color={colorHex}><b>{macroScore:F1}%</b> [{statusWord}]</color> • Active Theaters: <b>5</b>";
            }

            if (theatersContainer == null) return;

            for (int i = theatersContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(theatersContainer.GetChild(i).gameObject);
            }

            var allTheaters = WarTheaterInfo.GetAllTheaters();
            foreach (var theater in allTheaters)
            {
                CreateTheaterCard(theater, soldier);
            }
        }

        private void CreateTheaterCard(WarTheaterInfo theater, SoldierProfile soldier)
        {
            var cardObj = new GameObject($"TheaterCard_{theater.theaterType}");
            cardObj.transform.SetParent(theatersContainer, false);

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);

            var cardImg = cardObj.AddComponent<Image>();
            cardImg.color = p.subCardBackground;

            var vlg = cardObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 6;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var fitter = cardObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            bool isCurrent = soldier.currentTheater == theater.theaterType;
            string currentTag = isCurrent ? " <color=#FFD700><b>[CURRENT POST]</b></color>" : "";
            int progress = DynamicWarFrontEngine.GetFrontlineProgress(theater.theaterType);
            string statusDesc = DynamicWarFrontEngine.GetTheaterStatusText(theater.theaterType);

            // Title & Info
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(cardObj.transform, false);
            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 24;
            titleTmp.color = p.textPrimary;
            titleTmp.text = $"<b>{theater.theaterName}</b>{currentTag} • Danger: <b>{theater.dangerLevel}/5</b> • Hazard Pay: <color=#00FFAA>+${theater.hazardPayRate}/mo</color>\n" +
                           $"<size=19>{statusDesc}\n" +
                           $"<i>Terrain: {theater.specializedTerrainTrait} | Weather: {theater.weatherAndEnvironment}</i></size>";

            // Progress Gauge Visual
            CreateProgressGauge(cardObj.transform, progress, p);

            // Action Buttons Row
            var btnRow = new GameObject("ButtonsRow");
            btnRow.transform.SetParent(cardObj.transform, false);
            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            var hFitter = btnRow.AddComponent<ContentSizeFitter>();
            hFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Button 1: Transfer (if not current)
            if (!isCurrent && !soldier.isInCombatDeployment && !soldier.inMedicalConvalescence)
            {
                CreateActionButton(btnRow.transform, "REQUEST TRANSFER", new Color(0.20f, 0.45f, 0.70f), () =>
                {
                    DynamicWarFrontEngine.RequestTheaterTransfer(theater.theaterType, soldier);
                    RefreshUI();
                });
            }

            // Button 2: Airdrop Supplies ($200)
            CreateActionButton(btnRow.transform, "AIRDROP SUPPLIES ($200)", new Color(0.20f, 0.55f, 0.40f), () =>
            {
                if (soldier.cashBalance >= 200L)
                {
                    DynamicWarFrontEngine.AirdropSupplyCrate(theater.theaterType, soldier);
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("SUPPLY", "<color=#FF6666>[INSUFFICIENT FUNDS]</color> You lack $200 in cash to fund an emergency airdrop.");
                }
                RefreshUI();
            });

            // Button 3: Spearhead Counter-Offensive (if currently stationed here)
            if (isCurrent && !soldier.isDead && !soldier.inMedicalConvalescence)
            {
                CreateActionButton(btnRow.transform, "SPEARHEAD ASSAULT", new Color(0.75f, 0.25f, 0.20f), () =>
                {
                    CloseModal();
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.TriggerCombatTourBriefing();
                    }
                });
            }
        }

        private void CreateProgressGauge(Transform parent, int progress, ThemePalette p)
        {
            var gaugeObj = new GameObject("ProgressGauge");
            gaugeObj.transform.SetParent(parent, false);

            var le = gaugeObj.AddComponent<LayoutElement>();
            le.preferredHeight = 16;
            le.minHeight = 14;

            var bg = gaugeObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.15f, 1f);

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(gaugeObj.transform, false);
            var fillImg = fillObj.AddComponent<Image>();

            // progress -100..+100 mapped to 0..1
            float norm = (progress + 100f) / 200f;
            fillImg.color = progress >= 0 ? new Color(0.0f, 0.85f, 0.60f) : new Color(0.95f, 0.30f, 0.25f);

            var rt = fillObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(Mathf.Clamp01(norm), 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void CreateActionButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);

            var img = btnObj.AddComponent<Image>();
            img.color = color;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 44;
            le.minHeight = 40;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 19;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;

            var rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void EnsureUIStructure()
        {
            if (theatersContainer == null)
            {
                var foundScroll = GetComponentInChildren<ScrollRect>(true);
                if (foundScroll != null && foundScroll.content != null)
                {
                    theatersContainer = foundScroll.content.transform;
                }
            }

            if (closeButton == null)
            {
                var btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                {
                    if (b.name.Contains("Close") || b.name.Contains("Back"))
                    {
                        closeButton = b;
                        break;
                    }
                }
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
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (titleText != null) titleText.color = p.accentGold;
            if (subtitleText != null) subtitleText.color = p.textSecondary;
            if (macroDominanceText != null) macroDominanceText.color = p.textPrimary;

            if (closeButton != null)
            {
                var img = closeButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }
        }
    }
}
