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
    /// Interactive Tactical Combat Firefight Modal:
    /// Presents high-stakes battlefield dilemma cards during combat tours with 3 situational tactical choices:
    /// 1. Heavy Suppression & Fireteam Flank (Heavy Gunner & Scout synergy)
    /// 2. Danger-Close Artillery / CAS Air Support (Destructive strike on enemy strongpoints)
    /// 3. Emergency Smoke & Tactical Pullback / CASEVAC (Protects wounded, saves squadmates, valor awards)
    /// Directly determines mission victory, squad survival, Purple Hearts, and the Medal of Honor!
    /// </summary>
    public class TacticalFirefightModal : MonoBehaviour
    {
        public static TacticalFirefightModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header Elements")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI situationText;
        public TextMeshProUGUI theaterBriefingText;
        public Button closeButton;

        [Header("Dynamic Cards Container")]
        public Transform choicesContainer;

        private TacticalMission currentMission;
        private Action onMissionCompletedCallback;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            EnsureUIHierarchy();

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
            EnsureUIHierarchy();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);

            onMissionCompletedCallback?.Invoke();
            onMissionCompletedCallback = null;
        }

        /// <summary>
        /// Initiates an interactive combat operation dilemma card.
        /// </summary>
        public void LaunchFirefight(SoldierProfile soldier, WarTheaterInfo theater = null, Action onComplete = null)
        {
            if (soldier == null) return;

            onMissionCompletedCallback = onComplete;
            if (theater == null) theater = WarTheaterInfo.GetInfo(soldier.currentTheater);

            currentMission = CombatOperationsEngine.GenerateMission(theater, soldier);
            OpenModal();
            DisplayTacticalBriefing(soldier, theater, currentMission);
        }

        private void DisplayTacticalBriefing(SoldierProfile soldier, WarTheaterInfo theater, TacticalMission mission)
        {
            if (titleText != null)
            {
                titleText.text = $"<color=#FF4444>TACTICAL FIREFIGHT ENGAGEMENT • {mission.missionName.ToUpper()}</color>";
            }

            if (theaterBriefingText != null)
            {
                string mosStr = soldier.militaryMOS.ToString();
                theaterBriefingText.text = $"Theater: <color=#00FFAA><b>{theater.theaterName}</b> (Danger Lv {theater.dangerLevel})</color> • " +
                                          $"Assigned MOS: <color=#FFD700><b>{mosStr}</b></color> • Enemy Threat: <b>{mission.baseEnemyThreat:F0}%</b>";
            }

            if (situationText != null)
            {
                situationText.text = $"<b>SITUATION REPORT:</b>\n{mission.objectiveDescription}\n\n" +
                                     $"<color=#FFCC00>Enemy contact is immediate! Your fireteam awaits your direct tactical command:</color>";
            }

            if (choicesContainer == null) return;

            for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(choicesContainer.GetChild(i).gameObject);
            }

            // Choice 1: Heavy Suppression & Flank
            CreateTacticalChoiceCard(
                "TACTICAL ORDER: SUPPRESS & FLANK",
                "Order Heavy Gunner Briggs to lay continuous belt-fed suppression fire on enemy positions while the Lead Scout guides your fireteam in a swift flanking assault.",
                "Bonus: +20% Accuracy & Combat lethality. Leverages Heavy Gunner & Lead Scout readiness.",
                new Color(0.70f, 0.35f, 0.15f),
                () => ExecuteOrder(soldier, mission, TacticalChoiceOrder.HeavySuppression, theater)
            );

            // Choice 2: Call Danger-Close Artillery / Air Strike
            CreateTacticalChoiceCard(
                "TACTICAL ORDER: CALL DANGER-CLOSE ARTILLERY",
                "Radio battalion fire direction control for an immediate barrage on the enemy grid. High probability of obliterating hostile fortifications, but risks tactical disorientation.",
                "Bonus: High impact destruction. Decimates enemy Nemeses and fortified emplacements.",
                new Color(0.65f, 0.20f, 0.20f),
                () => ExecuteOrder(soldier, mission, TacticalChoiceOrder.ArtilleryStrikeCall, theater)
            );

            // Choice 3: Emergency Smoke & CASEVAC
            CreateTacticalChoiceCard(
                "TACTICAL ORDER: POP SMOKE & PULLBACK / CASEVAC",
                "Deploy M18 purple smoke grenades to break enemy line-of-sight. The Combat Medic extracts any pinned or wounded comrades to fortified cover.",
                "Bonus: Guaranteed squad survival. Awards valor citations and Purple Heart triage safety.",
                new Color(0.20f, 0.55f, 0.40f),
                () => ExecuteOrder(soldier, mission, TacticalChoiceOrder.EmergencyCASEVAC, theater)
            );
        }

        private void ExecuteOrder(SoldierProfile soldier, TacticalMission mission, TacticalChoiceOrder order, WarTheaterInfo theater)
        {
            var outcome = CombatOperationsEngine.ExecuteMission(soldier, mission, order, theater);
            DisplayBattleOutcome(soldier, outcome);
        }

        private void DisplayBattleOutcome(SoldierProfile soldier, CombatOperationOutcome outcome)
        {
            if (titleText != null)
            {
                bool isDecisive = outcome.outcomeType == TacticalOutcomeType.DecisiveVictory;
                bool isDefeat = outcome.outcomeType == TacticalOutcomeType.CatastrophicAmbush || outcome.outcomeType == TacticalOutcomeType.TacticalWithdrawal;
                string outcomeColor = isDecisive ? "#00FFAA" : (isDefeat ? "#FF4444" : "#FFD700");
                titleText.text = $"<color={outcomeColor}>AFTER-ACTION REPORT • {outcome.headline.ToUpper()}</color>";
            }

            if (situationText != null)
            {
                string honorsText = outcome.combatHonors != null && outcome.combatHonors.Count > 0 
                    ? string.Join(", ", outcome.combatHonors) 
                    : "None";

                situationText.text = $"<b>MISSION DEBRIEF:</b>\n{outcome.reportSummary}\n\n" +
                                     $"<b>Casualties:</b> {outcome.casualties}   |   " +
                                     $"<b>Bounty & Hazard Pay:</b> <color=#00FFAA>+${outcome.totalCashAwarded:N0}</color>\n" +
                                     $"<b>Tactical Medal Citations:</b> {honorsText}";
            }

            if (theaterBriefingText != null)
            {
                string squadStatus = "";
                if (soldier.squadMates != null)
                {
                    foreach (var mate in soldier.squadMates)
                    {
                        string status = mate.isDead ? "<color=#FF4444>[KIA]</color>" : (mate.isInjured ? "<color=#FF8800>[WIA]</color>" : "<color=#00FFAA>[OK]</color>");
                        squadStatus += $"{mate.nickname} {status}  ";
                    }
                }
                theaterBriefingText.text = $"<b>Squad Roster Status:</b> {squadStatus}";
            }

            if (choicesContainer == null) return;

            for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(choicesContainer.GetChild(i).gameObject);
            }

            // Dismiss Button
            CreateTacticalChoiceCard(
                "DISMISS AFTER-ACTION REPORT",
                "Acknowledge mission debrief and return to barracks duty with updated combat honors.",
                "Click to conclude operational dispatch.",
                new Color(0.20f, 0.50f, 0.70f),
                CloseModal
            );
        }

        private void CreateTacticalChoiceCard(string title, string description, string notes, Color btnColor, Action onClick)
        {
            var cardObj = new GameObject($"Choice_{title}");
            cardObj.transform.SetParent(choicesContainer, false);

            var img = cardObj.AddComponent<Image>();
            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            img.color = p.subCardBackground;

            var layout = cardObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.spacing = 6;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var fitter = cardObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(cardObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 22;
            text.color = p.textPrimary;
            text.text = $"<b>{title}</b>\n{description}\n<size=18><color=#FFD700><i>{notes}</i></color></size>";

            var btnObj = new GameObject("Btn");
            btnObj.transform.SetParent(cardObj.transform, false);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = btnColor;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.minHeight = 44;
            btnLayout.preferredHeight = 48;

            var btnTxtObj = new GameObject("BtnText");
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            var btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
            btnTxt.fontSize = 20;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;
            btnTxt.text = $"<b>EXECUTE ORDER</b>";

            var rt = btnTxtObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void EnsureUIHierarchy()
        {
            if (modalRoot != null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var foundCanvas = FindAnyObjectByType<Canvas>();
                if (foundCanvas != null) transform.SetParent(foundCanvas.transform, false);
            }

            modalRoot = gameObject;
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            modalBackdropImage = GetComponent<Image>();
            if (modalBackdropImage == null) modalBackdropImage = gameObject.AddComponent<Image>();
            modalBackdropImage.color = new Color(0f, 0f, 0f, 0.85f);

            var panelObj = new GameObject("DialogPanel");
            panelObj.transform.SetParent(transform, false);
            var pRect = panelObj.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(920, 800);

            dialogPanelImage = panelObj.AddComponent<Image>();
            dialogPanelImage.color = new Color(0.08f, 0.10f, 0.09f, 0.98f);

            var pLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            pLayout.padding = new RectOffset(24, 24, 20, 20);
            pLayout.spacing = 10;
            pLayout.childControlHeight = true;
            pLayout.childControlWidth = true;
            pLayout.childForceExpandHeight = false;
            pLayout.childForceExpandWidth = true;

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.35f, 0.35f);

            // Theater Briefing
            var tbObj = new GameObject("TheaterBriefing");
            tbObj.transform.SetParent(panelObj.transform, false);
            theaterBriefingText = tbObj.AddComponent<TextMeshProUGUI>();
            theaterBriefingText.fontSize = 20;
            theaterBriefingText.alignment = TextAlignmentOptions.Center;
            theaterBriefingText.color = new Color(0.85f, 0.85f, 0.85f);

            // Situation
            var sitObj = new GameObject("Situation");
            sitObj.transform.SetParent(panelObj.transform, false);
            situationText = sitObj.AddComponent<TextMeshProUGUI>();
            situationText.fontSize = 20;
            situationText.color = Color.white;

            // Scroll container for choices
            var scrollObj = new GameObject("ScrollContainer");
            scrollObj.transform.SetParent(panelObj.transform, false);
            var scrollLayout = scrollObj.AddComponent<LayoutElement>();
            scrollLayout.minHeight = 440;
            scrollLayout.flexibleHeight = 1;

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewObj = new GameObject("Viewport");
            viewObj.transform.SetParent(scrollObj.transform, false);
            var viewRect = viewObj.AddComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.offsetMin = Vector2.zero;
            viewRect.offsetMax = Vector2.zero;
            viewObj.AddComponent<RectMask2D>();
            scrollRect.viewport = viewRect;

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 0);

            var cLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            cLayout.padding = new RectOffset(6, 6, 6, 6);
            cLayout.spacing = 10;
            cLayout.childControlHeight = true;
            cLayout.childControlWidth = true;
            cLayout.childForceExpandHeight = false;
            cLayout.childForceExpandWidth = true;

            var cFitter = contentObj.AddComponent<ContentSizeFitter>();
            cFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;
            choicesContainer = contentObj.transform;

            // Close Button
            var closeObj = new GameObject("CloseBtn");
            closeObj.transform.SetParent(panelObj.transform, false);
            var closeImg = closeObj.AddComponent<Image>();
            closeImg.color = new Color(0.25f, 0.28f, 0.30f);
            closeButton = closeObj.AddComponent<Button>();
            closeButton.targetGraphic = closeImg;
            var closeLayout = closeObj.AddComponent<LayoutElement>();
            closeLayout.minHeight = 40;

            var closeTxtObj = new GameObject("CloseTxt");
            closeTxtObj.transform.SetParent(closeObj.transform, false);
            var closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTxt.fontSize = 20;
            closeTxt.alignment = TextAlignmentOptions.Center;
            closeTxt.color = Color.white;
            closeTxt.text = "<b>RETURN TO BARRACKS</b>";
            var cRt = closeTxtObj.GetComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (dialogPanelImage != null) dialogPanelImage.color = p.cardBackground;
            if (titleText != null) titleText.color = p.accentGold;
            if (situationText != null) situationText.color = p.textPrimary;
            if (theaterBriefingText != null) theaterBriefingText.color = p.textSecondary;
        }
    }
}
