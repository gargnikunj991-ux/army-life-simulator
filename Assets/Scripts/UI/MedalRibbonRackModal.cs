using System;
using System.Collections.Generic;
using System.Text;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    /// <summary>
    /// Full-screen modal presenting:
    /// 1. Dress Uniform & Ribbon Rack Inspection (Standard 3-per-row military order of precedence).
    /// 2. Historical Parchment Medal Citations Dossier with CO signatures & Imperial seals.
    /// 3. Solemn Regimental Memorial Wall honoring fallen squadmates with wreath tributes.
    /// </summary>
    public class MedalRibbonRackModal : MonoBehaviour
    {
        public static MedalRibbonRackModal Instance { get; private set; }

        [Header("Modal Root & Uniform Background")]
        public GameObject modalRoot;
        public Image uniformBackdrop;
        public Image dressUniformJacketPanel;

        [Header("Header Elements")]
        public TextMeshProUGUI headerTitleText;
        public TextMeshProUGUI soldierSubtitleText;
        public Button closeButton;

        [Header("Navigation Tabs")]
        public Button tabRackButton;
        public Button tabCitationsButton;
        public Button tabMemorialButton;
        public TextMeshProUGUI tabRackButtonText;
        public TextMeshProUGUI tabCitationsButtonText;
        public TextMeshProUGUI tabMemorialButtonText;

        [Header("Tab Panels")]
        public GameObject tabRackPanel;
        public GameObject tabCitationsPanel;
        public GameObject tabMemorialPanel;

        [Header("Tab 1: Ribbon Rack & Uniform Details")]
        public Transform ribbonRackContainer;
        public TextMeshProUGUI rankBadgeText;
        public TextMeshProUGUI woundStarsBadgeText;
        public TextMeshProUGUI longevityHashesBadgeText;
        public TextMeshProUGUI combatInfantryBadgeText;
        public TextMeshProUGUI selectedRibbonTitleText;
        public TextMeshProUGUI selectedRibbonPrecedenceText;
        public TextMeshProUGUI selectedRibbonCriteriaText;
        public TextMeshProUGUI selectedRibbonBountyText;
        public TextMeshProUGUI selectedRibbonDateText;

        [Header("Tab 2: Medal Citations Dossier")]
        public Transform citationsContentContainer;

        [Header("Tab 3: Regimental Memorial Wall")]
        public Transform memorialContentContainer;
        public TextMeshProUGUI memorialHeaderQuoteText;

        // Current active state
        private SoldierProfile _currentSoldier;
        private MedalRibbonDefinition _selectedMedal;
        private int _currentTabIndex = 0;
        private bool _isInitialized = false;

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

            EnsureUIStructure();

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
            EnsureUIStructure();
            SetupListeners();
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            RemoveListeners();
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void SetupListeners()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseModal);

            if (tabRackButton != null)
                tabRackButton.onClick.AddListener(() => SwitchTab(0));

            if (tabCitationsButton != null)
                tabCitationsButton.onClick.AddListener(() => SwitchTab(1));

            if (tabMemorialButton != null)
                tabMemorialButton.onClick.AddListener(() => SwitchTab(2));
        }

        private void RemoveListeners()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(CloseModal);

            if (tabRackButton != null)
                tabRackButton.onClick.RemoveAllListeners();

            if (tabCitationsButton != null)
                tabCitationsButton.onClick.RemoveAllListeners();

            if (tabMemorialButton != null)
                tabMemorialButton.onClick.RemoveAllListeners();
        }

        // =========================================================================================
        // PUBLIC LIFECYCLE & MODAL OPEN / CLOSE
        // =========================================================================================

        /// <summary>
        /// Opens the full-screen modal inspecting the soldier's dress uniform, ribbon rack,
        /// official citations dossier, and regimental memorial wall.
        /// </summary>
        public void OpenModal(SoldierProfile soldier)
        {
            _currentSoldier = soldier ?? (GameManager.Instance != null ? GameManager.Instance.currentSoldier : null);

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (modalRoot != null)
                modalRoot.SetActive(true);

            EnsureUIStructure();
            SwitchTab(0);
            RefreshUI();
        }

        /// <summary>
        /// Closes the modal.
        /// </summary>
        public void CloseModal()
        {
            if (modalRoot != null && modalRoot != gameObject)
            {
                modalRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Switches active tab:
        /// 0 = Ribbon Rack & Uniform
        /// 1 = Medal Citations Dossier
        /// 2 = Regimental Memorial Wall
        /// </summary>
        public void SwitchTab(int tabIndex)
        {
            _currentTabIndex = Mathf.Clamp(tabIndex, 0, 2);

            if (tabRackPanel != null) tabRackPanel.SetActive(_currentTabIndex == 0);
            if (tabCitationsPanel != null) tabCitationsPanel.SetActive(_currentTabIndex == 1);
            if (tabMemorialPanel != null) tabMemorialPanel.SetActive(_currentTabIndex == 2);

            UpdateTabButtonVisuals();

            if (_currentTabIndex == 0) RefreshRibbonRackTab();
            else if (_currentTabIndex == 1) RefreshCitationsTab();
            else if (_currentTabIndex == 2) RefreshMemorialWallTab();
        }

        private void UpdateTabButtonVisuals()
        {
            Color activeColor = new Color(0.85f, 0.70f, 0.25f, 1f); // Imperial Gold
            Color inactiveColor = new Color(0.70f, 0.72f, 0.75f, 0.8f);

            if (tabRackButtonText != null)
                tabRackButtonText.color = (_currentTabIndex == 0) ? activeColor : inactiveColor;

            if (tabCitationsButtonText != null)
                tabCitationsButtonText.color = (_currentTabIndex == 1) ? activeColor : inactiveColor;

            if (tabMemorialButtonText != null)
                tabMemorialButtonText.color = (_currentTabIndex == 2) ? activeColor : inactiveColor;
        }

        // =========================================================================================
        // TAB 1: RIBBON RACK & DRESS UNIFORM
        // =========================================================================================

        public void RefreshUI()
        {
            if (_currentSoldier == null) return;

            string rankName = _currentSoldier.currentRank != null ? _currentSoldier.currentRank.rankName : "Private";
            string paygrade = _currentSoldier.currentRank != null ? _currentSoldier.currentRank.paygrade : "E-1";
            string branch = _currentSoldier.militaryBranch == MilitaryBranch.AirForce ? "Imperial Air Force" : "Imperial Army";
            string dept = !string.IsNullOrEmpty(_currentSoldier.assignedDepartment) ? _currentSoldier.assignedDepartment : "Infantry Vanguard";

            if (soldierSubtitleText != null)
            {
                soldierSubtitleText.text = $"<color=#00FFAA><b>{rankName.ToUpper()} {_currentSoldier.characterName.ToUpper()}</b></color> " +
                    $"• Paygrade: <color=#FFD700><b>{paygrade}</b></color> • Branch: <b>{branch}</b> ({dept})";
            }

            switch (_currentTabIndex)
            {
                case 0: RefreshRibbonRackTab(); break;
                case 1: RefreshCitationsTab(); break;
                case 2: RefreshMemorialWallTab(); break;
            }
        }

        private static void SafeDestroy(GameObject obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        private void RefreshRibbonRackTab()
        {
            if (_currentSoldier == null) return;

            // 1. Populate Uniform Badges & Insignias
            int woundStars = MedalRackEngine.CalculateWoundStars(_currentSoldier);
            int longevityHashes = MedalRackEngine.CalculateLongevityHashes(_currentSoldier);
            bool hasCIB = _currentSoldier.HasMedal("Combat Infantry Badge") || _currentSoldier.isInCombatDeployment;

            if (rankBadgeText != null)
            {
                string rank = _currentSoldier.currentRank != null ? _currentSoldier.currentRank.rankName : "Private";
                string pay = _currentSoldier.currentRank != null ? _currentSoldier.currentRank.paygrade : "E-1";
                rankBadgeText.text = $"<b>Rank:</b> <color=#FFD700>{rank}</color> ({pay})";
            }

            if (woundStarsBadgeText != null)
            {
                woundStarsBadgeText.text = woundStars > 0 
                    ? $"<b>Wound Stars:</b> <color=#D4AF37>★ x{woundStars}</color> <color=#B57EDC>(Combat Casualty Stipend)</color>" 
                    : "<b>Wound Stars:</b> <color=#888888>None (Unwounded)</color>";
            }

            if (longevityHashesBadgeText != null)
            {
                int years = (_currentSoldier.totalSemestersLived / 2);
                longevityHashesBadgeText.text = longevityHashes > 0
                    ? $"<b>Service Hashes:</b> <color=#FFD700>═ x{longevityHashes}</color> ({years} Years Active Duty)"
                    : "<b>Service Hashes:</b> <color=#888888>0 (Under 3 Years)</color>";
            }

            if (combatInfantryBadgeText != null)
            {
                combatInfantryBadgeText.text = hasCIB
                    ? "<b>Combat Badge:</b> <color=#00FFAA>⚔ Combat Infantry Badge (CIB)</color>"
                    : "<b>Combat Badge:</b> <color=#888888>Non-Combat Infantry</color>";
            }

            // 2. Build Ribbon Rack Rows
            if (ribbonRackContainer != null)
            {
                // Clear existing ribbon row objects
                for (int i = ribbonRackContainer.childCount - 1; i >= 0; i--)
                {
                    SafeDestroy(ribbonRackContainer.GetChild(i).gameObject);
                }

                var mountedRows = MedalRackEngine.BuildRibbonRack(_currentSoldier);

                if (mountedRows.Count == 0)
                {
                    var emptyObj = new GameObject("EmptyRackText");
                    emptyObj.transform.SetParent(ribbonRackContainer, false);
                    var txt = emptyObj.AddComponent<TextMeshProUGUI>();
                    txt.text = "<color=#888888><i>No combat ribbons or decorations mounted. " +
                        "Survive frontline tours and perform heroic actions to earn honors.</i></color>";
                    txt.fontSize = 15f;
                    txt.alignment = TextAlignmentOptions.Center;
                }
                else
                {
                    foreach (var row in mountedRows)
                    {
                        CreateMountedRowUI(row);
                    }

                    // Auto-select first (highest precedence) ribbon if none selected
                    if (_selectedMedal == null && mountedRows[0].ribbons.Count > 0)
                    {
                        SelectMedal(mountedRows[0].ribbons[0]);
                    }
                    else if (_selectedMedal != null)
                    {
                        SelectMedal(_selectedMedal);
                    }
                }
            }
        }

        private void CreateMountedRowUI(MountedRibbonRow row)
        {
            var rowObj = new GameObject($"RibbonRow_{row.rowIndex}");
            rowObj.transform.SetParent(ribbonRackContainer, false);

            var rt = rowObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(360, 36);

            var hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            foreach (var rib in row.ribbons)
            {
                CreateRibbonBarItem(rowObj.transform, rib);
            }
        }

        private void CreateRibbonBarItem(Transform parent, MedalRibbonDefinition def)
        {
            var itemObj = new GameObject($"Ribbon_{def.medalId}");
            itemObj.transform.SetParent(parent, false);

            var rt = itemObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 30);

            // Background mounting frame
            var bgImg = itemObj.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.12f, 1f);

            var outline = itemObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.70f, 0.25f, 0.8f); // Gold mounting rail
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // Container for vertical stripes
            var stripesContainer = new GameObject("Stripes");
            stripesContainer.transform.SetParent(itemObj.transform, false);
            var stripesRt = stripesContainer.AddComponent<RectTransform>();
            stripesRt.anchorMin = Vector2.zero;
            stripesRt.anchorMax = Vector2.one;
            stripesRt.offsetMin = Vector2.zero;
            stripesRt.offsetMax = Vector2.zero;

            var hlg = stripesContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 0;

            Color[] colors = def.GetColors();
            for (int i = 0; i < colors.Length; i++)
            {
                var stripeObj = new GameObject($"Stripe_{i}");
                stripeObj.transform.SetParent(stripesContainer.transform, false);
                var stripeImg = stripeObj.AddComponent<Image>();
                stripeImg.color = colors[i];
            }

            // Center device icon (e.g. ★, V, ⚔, ✚)
            if (!string.IsNullOrEmpty(def.deviceIcon))
            {
                var deviceObj = new GameObject("DeviceIcon");
                deviceObj.transform.SetParent(itemObj.transform, false);
                var devRt = deviceObj.AddComponent<RectTransform>();
                devRt.anchorMin = Vector2.zero;
                devRt.anchorMax = Vector2.one;
                devRt.offsetMin = Vector2.zero;
                devRt.offsetMax = Vector2.zero;

                var devText = deviceObj.AddComponent<TextMeshProUGUI>();
                devText.text = def.deviceIcon;
                devText.fontSize = 16f;
                devText.fontStyle = FontStyles.Bold;
                devText.alignment = TextAlignmentOptions.Center;
                devText.color = new Color(1f, 0.84f, 0f, 0.95f); // Rich gold
            }

            // Button trigger for selection
            var btn = itemObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.onClick.AddListener(() => SelectMedal(def));
        }

        /// <summary>
        /// Selects a medal and inspects its criteria, order of precedence, and cash bounty.
        /// </summary>
        public void SelectMedal(MedalRibbonDefinition def)
        {
            _selectedMedal = def;
            if (def == null) return;

            if (selectedRibbonTitleText != null)
            {
                selectedRibbonTitleText.text = $"<color=#FFD700><b>{def.displayName.ToUpper()}</b></color>";
            }

            if (selectedRibbonPrecedenceText != null)
            {
                selectedRibbonPrecedenceText.text = $"Order of Precedence: <color=#00FFAA><b>#{def.precedenceOrder}</b></color> " +
                    $"in Imperial Armed Forces • Category: <b>{def.category}</b>";
            }

            if (selectedRibbonCriteriaText != null)
            {
                selectedRibbonCriteriaText.text = $"<b>Award Criteria:</b>\n{def.criteriaDescription}";
            }

            if (selectedRibbonBountyText != null)
            {
                long bounty = def.imperialBounty > 0 ? def.imperialBounty : EconomyData.GetMedalCashReward(def.displayName);
                selectedRibbonBountyText.text = $"Imperial Treasury Bounty: <color=#00FF88><b>+${bounty:N0}</b></color> (Awarded Upon Decoration)";
            }

            if (selectedRibbonDateText != null)
            {
                string theater = _currentSoldier != null ? WarTheaterInfo.GetInfo(_currentSoldier.currentTheater).theaterName : "The Western Front";
                int year = 940 + (_currentSoldier != null ? _currentSoldier.totalSemestersLived / 2 : 1);
                selectedRibbonDateText.text = $"Theater of Action: <b>{theater}</b> • Conferred Imperial Year <b>{year}</b>";
            }
        }

        // =========================================================================================
        // TAB 2: MEDAL CITATIONS DOSSIER
        // =========================================================================================

        private void RefreshCitationsTab()
        {
            if (_currentSoldier == null || citationsContentContainer == null) return;

            // Clear old cards
            for (int i = citationsContentContainer.childCount - 1; i >= 0; i--)
            {
                SafeDestroy(citationsContentContainer.GetChild(i).gameObject);
            }

            var citations = MedalRackEngine.GetAllCitations(_currentSoldier);

            if (citations.Count == 0)
            {
                var emptyObj = new GameObject("EmptyCitationsText");
                emptyObj.transform.SetParent(citationsContentContainer, false);
                var txt = emptyObj.AddComponent<TextMeshProUGUI>();
                txt.text = "<color=#888888><i>No official citations currently registered in the Imperial High Command dossier. " +
                    "Valor citations are conferred for frontline heroism and campaign tours.</i></color>";
                txt.fontSize = 15f;
                txt.alignment = TextAlignmentOptions.Center;
                return;
            }

            foreach (var cit in citations)
            {
                CreateParchmentCitationCard(citationsContentContainer, cit);
            }
        }

        private void CreateParchmentCitationCard(Transform parent, AwardedMedalCitation cit)
        {
            var cardObj = new GameObject($"Citation_{cit.medalId}");
            cardObj.transform.SetParent(parent, false);

            var rt = cardObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(580, 200);

            var img = cardObj.AddComponent<Image>();
            img.color = new Color(0.14f, 0.13f, 0.12f, 0.95f); // Aged dark parchment

            var outline = cardObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.65f, 0.52f, 0.28f, 0.6f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var vlg = cardObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 6f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Header seal & title
            var headObj = new GameObject("CitationHeader");
            headObj.transform.SetParent(cardObj.transform, false);
            var headTxt = headObj.AddComponent<TextMeshProUGUI>();
            headTxt.text = $"<color=#FFD700><b>THE VALERIAN IMPERIAL HIGH COMMAND</b></color>\n" +
                $"<size=12><color=#C0C0C0>CITATION TO ACCOMPANY THE CONFERRAL OF THE</color></size>\n" +
                $"<size=16><color=#00FFAA><b>{cit.medalName.ToUpper()}</b></color></size>";
            headTxt.alignment = TextAlignmentOptions.Center;
            headTxt.fontSize = 13f;

            // Recipient & Date line
            var infoObj = new GameObject("CitationInfo");
            infoObj.transform.SetParent(cardObj.transform, false);
            var infoTxt = infoObj.AddComponent<TextMeshProUGUI>();
            infoTxt.text = $"<b>Recipient:</b> {cit.recipientRank} {cit.recipientName}  |  <b>Theater:</b> {cit.theaterOfAction}  |  <b>Date:</b> {cit.dateAwarded}";
            infoTxt.fontSize = 12f;
            infoTxt.color = new Color(0.9f, 0.9f, 0.85f);

            // Body Citation Text
            var bodyObj = new GameObject("CitationBody");
            bodyObj.transform.SetParent(cardObj.transform, false);
            var bodyTxt = bodyObj.AddComponent<TextMeshProUGUI>();
            bodyTxt.text = $"<i>\"{cit.citationText}\"</i>";
            bodyTxt.fontSize = 12f;
            bodyTxt.color = new Color(0.85f, 0.85f, 0.80f);

            // Signature & Seal Footer
            var footObj = new GameObject("CitationFooter");
            footObj.transform.SetParent(cardObj.transform, false);
            var footTxt = footObj.AddComponent<TextMeshProUGUI>();
            footTxt.text = $"<b>Signed:</b> {cit.commandingOfficerName} ({cit.commandingOfficerTitle})  •  <b>Imperial Seal:</b> <color=#D4AF37>{cit.imperialSealCode}</color>";
            footTxt.fontSize = 11f;
            footTxt.color = new Color(0.70f, 0.70f, 0.65f);
        }

        // =========================================================================================
        // TAB 3: REGIMENTAL MEMORIAL WALL
        // =========================================================================================

        private void RefreshMemorialWallTab()
        {
            if (_currentSoldier == null || memorialContentContainer == null) return;

            // Clear old cards
            for (int i = memorialContentContainer.childCount - 1; i >= 0; i--)
            {
                SafeDestroy(memorialContentContainer.GetChild(i).gameObject);
            }

            var memorials = MedalRackEngine.GetMemorialWall(_currentSoldier);

            if (memorials.Count == 0)
            {
                var emptyObj = new GameObject("EmptyMemorialText");
                emptyObj.transform.SetParent(memorialContentContainer, false);
                var txt = emptyObj.AddComponent<TextMeshProUGUI>();
                txt.text = "<color=#888888><i>The squad stands whole. No fallen comrades recorded in this campaign.\n" +
                    "May fortune continue to favor your fireteam on the frontlines.</i></color>";
                txt.fontSize = 15f;
                txt.alignment = TextAlignmentOptions.Center;
                return;
            }

            foreach (var hero in memorials)
            {
                CreateMemorialPlaqueCard(memorialContentContainer, hero);
            }
        }

        private void CreateMemorialPlaqueCard(Transform parent, FallenHeroMemorial hero)
        {
            var plaqueObj = new GameObject($"MemorialPlaque_{hero.mateId}");
            plaqueObj.transform.SetParent(parent, false);

            var rt = plaqueObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(580, 160);

            var img = plaqueObj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.09f, 0.11f, 0.98f); // Polished black granite/marble

            var outline = plaqueObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.75f, 0.65f, 0.35f, 0.7f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var vlg = plaqueObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 5f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Name & Role Header
            var nameObj = new GameObject("HeroName");
            nameObj.transform.SetParent(plaqueObj.transform, false);
            var nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
            nameTxt.text = $"<color=#FFD700><b>{hero.GetFullDisplayName().ToUpper()}</b></color> " +
                $"• <color=#00FFAA>{hero.roleTitle}</color> • Campaign: <b>{hero.campaignFallen}</b>";
            nameTxt.fontSize = 14f;

            // Death Cause
            var causeObj = new GameObject("HeroCause");
            causeObj.transform.SetParent(plaqueObj.transform, false);
            var causeTxt = causeObj.AddComponent<TextMeshProUGUI>();
            causeTxt.text = $"<b>Cause of Death:</b> <color=#FF8866>{hero.causeOfDeath}</color>";
            causeTxt.fontSize = 12f;

            // Final Words
            var wordsObj = new GameObject("HeroWords");
            wordsObj.transform.SetParent(plaqueObj.transform, false);
            var wordsTxt = wordsObj.AddComponent<TextMeshProUGUI>();
            wordsTxt.text = $"<b>Final Words:</b> <i><color=#E0E0E0>{hero.finalWords}</color></i>";
            wordsTxt.fontSize = 12f;

            // Posthumous Honor & Wreath Action Row
            var actionRow = new GameObject("ActionRow");
            actionRow.transform.SetParent(plaqueObj.transform, false);
            var actionRt = actionRow.AddComponent<RectTransform>();
            actionRt.sizeDelta = new Vector2(540, 36);

            var hlg = actionRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 16f;

            // Posthumous medal label
            var medalObj = new GameObject("PosthumousMedal");
            medalObj.transform.SetParent(actionRow.transform, false);
            var medalTxt = medalObj.AddComponent<TextMeshProUGUI>();
            medalTxt.text = $"<b>Decorated:</b> <color=#FFD700>🏅 {hero.posthumousMedal}</color>";
            medalTxt.fontSize = 12f;
            var medalLe = medalObj.AddComponent<LayoutElement>();
            medalLe.preferredWidth = 320;

            // Wreath button
            var wreathBtnObj = new GameObject("LayWreathButton");
            wreathBtnObj.transform.SetParent(actionRow.transform, false);
            var wBtnRt = wreathBtnObj.AddComponent<RectTransform>();
            wBtnRt.sizeDelta = new Vector2(190, 30);

            var wImg = wreathBtnObj.AddComponent<Image>();
            wImg.color = hero.wreathLaid 
                ? new Color(0.15f, 0.35f, 0.20f, 1f) 
                : new Color(0.35f, 0.28f, 0.12f, 1f);

            var wBtn = wreathBtnObj.AddComponent<Button>();
            var wTxtObj = new GameObject("WreathText");
            wTxtObj.transform.SetParent(wreathBtnObj.transform, false);
            var wTxt = wTxtObj.AddComponent<TextMeshProUGUI>();
            wTxt.fontSize = 11f;
            wTxt.fontStyle = FontStyles.Bold;
            wTxt.alignment = TextAlignmentOptions.Center;

            if (hero.wreathLaid)
            {
                wTxt.text = $"<color=#00FFAA>✓ Wreath Laid ({hero.tributesPaidCount})</color>";
            }
            else
            {
                wTxt.text = "<color=#FFD700>🌿 Lay Laurel Wreath (-$50)</color>";
            }

            wBtn.onClick.AddListener(() =>
            {
                OnLayWreathClicked(hero, wImg, wTxt);
            });
        }

        private void OnLayWreathClicked(FallenHeroMemorial hero, Image btnImg, TextMeshProUGUI btnTxt)
        {
            if (_currentSoldier == null || hero == null) return;

            bool success = MedalRackEngine.LayWreathAtMemorial(_currentSoldier, hero);
            if (success)
            {
                if (btnImg != null) btnImg.color = new Color(0.15f, 0.35f, 0.20f, 1f);
                if (btnTxt != null) btnTxt.text = $"<color=#00FFAA>✓ Wreath Laid ({hero.tributesPaidCount})</color>";

                // Refresh soldier subtitle to reflect any updated cash/stats
                RefreshUI();
            }
        }

        // =========================================================================================
        // PROCEDURAL RUNTIME UI CREATION (Self-healing fallback)
        // =========================================================================================

        private void EnsureUIStructure()
        {
            if (_isInitialized && modalRoot != null) return;

            // If modalRoot is assigned in Inspector, bind child references
            if (modalRoot != null)
            {
                BindExistingComponents();
                _isInitialized = true;
                return;
            }

            // Otherwise, construct dynamic Canvas structure programmatically
            ConstructProceduralUI();
            _isInitialized = true;
        }

        private void BindExistingComponents()
        {
            if (closeButton == null)
                closeButton = modalRoot.GetComponentInChildren<Button>(true);

            if (headerTitleText == null)
            {
                var texts = modalRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (t.gameObject.name.ToLower().Contains("title")) headerTitleText = t;
                    else if (t.gameObject.name.ToLower().Contains("subtitle")) soldierSubtitleText = t;
                }
            }
        }

        private void ConstructProceduralUI()
        {
            // Ensure Canvas and RectTransform
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("MedalRackCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                transform.SetParent(canvasObj.transform, false);
            }

            // Root Overlay Panel
            modalRoot = new GameObject("MedalRackModalRoot");
            modalRoot.transform.SetParent(transform, false);
            var rootRt = modalRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // Semi-transparent backdrop
            uniformBackdrop = modalRoot.AddComponent<Image>();
            uniformBackdrop.color = new Color(0.04f, 0.05f, 0.08f, 0.95f);

            // Dress Uniform Jacket Main Window
            var jacketObj = new GameObject("DressUniformJacket");
            jacketObj.transform.SetParent(modalRoot.transform, false);
            var jacketRt = jacketObj.AddComponent<RectTransform>();
            jacketRt.anchorMin = new Vector2(0.5f, 0.5f);
            jacketRt.anchorMax = new Vector2(0.5f, 0.5f);
            jacketRt.sizeDelta = new Vector2(980, 680);

            dressUniformJacketPanel = jacketObj.AddComponent<Image>();
            dressUniformJacketPanel.color = new Color(0.08f, 0.11f, 0.16f, 0.98f); // Deep navy dress uniform wool

            var jacketOutline = jacketObj.AddComponent<Outline>();
            jacketOutline.effectColor = new Color(0.85f, 0.70f, 0.25f, 0.9f); // Gold epaulette trim
            jacketOutline.effectDistance = new Vector2(2f, -2f);

            // ==================== HEADER BAR ====================
            var headerObj = new GameObject("HeaderBar");
            headerObj.transform.SetParent(jacketObj.transform, false);
            var headerRt = headerObj.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.sizeDelta = new Vector2(0, 75);

            var headerImg = headerObj.AddComponent<Image>();
            headerImg.color = new Color(0.06f, 0.08f, 0.12f, 1f);

            var titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(headerObj.transform, false);
            var titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.45f);
            titleRt.anchorMax = new Vector2(0.85f, 1f);
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = Vector2.zero;

            headerTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            headerTitleText.text = "<color=#FFD700><b>DRESS UNIFORM INSPECTION • MEDAL RIBBON RACK & HONORS</b></color>";
            headerTitleText.fontSize = 20f;
            headerTitleText.fontStyle = FontStyles.Bold;
            headerTitleText.alignment = TextAlignmentOptions.MidlineLeft;

            var subtitleObj = new GameObject("SubtitleText");
            subtitleObj.transform.SetParent(headerObj.transform, false);
            var subtitleRt = subtitleObj.AddComponent<RectTransform>();
            subtitleRt.anchorMin = new Vector2(0f, 0f);
            subtitleRt.anchorMax = new Vector2(0.85f, 0.45f);
            subtitleRt.offsetMin = new Vector2(20, 4);
            subtitleRt.offsetMax = Vector2.zero;

            soldierSubtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
            soldierSubtitleText.text = "Sergeant Marcus Vance • Imperial Army (Infantry)";
            soldierSubtitleText.fontSize = 13f;
            soldierSubtitleText.color = new Color(0.8f, 0.82f, 0.85f);

            // Close Button
            var closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(headerObj.transform, false);
            var closeRt = closeBtnObj.AddComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 0.5f);
            closeRt.anchorMax = new Vector2(1f, 0.5f);
            closeRt.pivot = new Vector2(1f, 0.5f);
            closeRt.anchoredPosition = new Vector2(-16, 0);
            closeRt.sizeDelta = new Vector2(38, 38);

            var closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = new Color(0.4f, 0.15f, 0.15f, 1f);

            closeButton = closeBtnObj.AddComponent<Button>();
            var closeTxtObj = new GameObject("XText");
            closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
            var closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTxt.text = "✕";
            closeTxt.fontSize = 18f;
            closeTxt.fontStyle = FontStyles.Bold;
            closeTxt.alignment = TextAlignmentOptions.Center;
            closeTxt.color = Color.white;

            // ==================== NAVIGATION TABS BAR ====================
            var tabsBarObj = new GameObject("TabsBar");
            tabsBarObj.transform.SetParent(jacketObj.transform, false);
            var tabsRt = tabsBarObj.AddComponent<RectTransform>();
            tabsRt.anchorMin = new Vector2(0f, 1f);
            tabsRt.anchorMax = new Vector2(1f, 1f);
            tabsRt.pivot = new Vector2(0.5f, 1f);
            tabsRt.anchoredPosition = new Vector2(0, -75);
            tabsRt.sizeDelta = new Vector2(0, 42);

            var tabsImg = tabsBarObj.AddComponent<Image>();
            tabsImg.color = new Color(0.05f, 0.07f, 0.10f, 1f);

            var tabsHlg = tabsBarObj.AddComponent<HorizontalLayoutGroup>();
            tabsHlg.childForceExpandWidth = true;
            tabsHlg.childForceExpandHeight = true;
            tabsHlg.spacing = 4f;

            tabRackButton = CreateTabButton(tabsBarObj.transform, "🎖️ Ribbon Rack & Uniform", out tabRackButtonText);
            tabCitationsButton = CreateTabButton(tabsBarObj.transform, "📜 Medal Citations Dossier", out tabCitationsButtonText);
            tabMemorialButton = CreateTabButton(tabsBarObj.transform, "🏛️ Regimental Memorial Wall", out tabMemorialButtonText);

            // ==================== TAB CONTENT CONTAINER ====================
            var contentAreaObj = new GameObject("TabContentArea");
            contentAreaObj.transform.SetParent(jacketObj.transform, false);
            var contentRt = contentAreaObj.AddComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = new Vector2(16, 16);
            contentRt.offsetMax = new Vector2(-16, -122);

            // TAB 1: RACK & UNIFORM
            tabRackPanel = new GameObject("Tab1_RibbonRack");
            tabRackPanel.transform.SetParent(contentAreaObj.transform, false);
            var t1Rt = tabRackPanel.AddComponent<RectTransform>();
            t1Rt.anchorMin = Vector2.zero;
            t1Rt.anchorMax = Vector2.one;
            t1Rt.offsetMin = Vector2.zero;
            t1Rt.offsetMax = Vector2.zero;

            ConstructTab1UI(tabRackPanel.transform);

            // TAB 2: CITATIONS DOSSIER
            tabCitationsPanel = new GameObject("Tab2_Citations");
            tabCitationsPanel.transform.SetParent(contentAreaObj.transform, false);
            var t2Rt = tabCitationsPanel.AddComponent<RectTransform>();
            t2Rt.anchorMin = Vector2.zero;
            t2Rt.anchorMax = Vector2.one;
            t2Rt.offsetMin = Vector2.zero;
            t2Rt.offsetMax = Vector2.zero;

            ConstructTab2UI(tabCitationsPanel.transform);

            // TAB 3: MEMORIAL WALL
            tabMemorialPanel = new GameObject("Tab3_Memorial");
            tabMemorialPanel.transform.SetParent(contentAreaObj.transform, false);
            var t3Rt = tabMemorialPanel.AddComponent<RectTransform>();
            t3Rt.anchorMin = Vector2.zero;
            t3Rt.anchorMax = Vector2.one;
            t3Rt.offsetMin = Vector2.zero;
            t3Rt.offsetMax = Vector2.zero;

            ConstructTab3UI(tabMemorialPanel.transform);
        }

        private Button CreateTabButton(Transform parent, string label, out TextMeshProUGUI btnText)
        {
            var btnObj = new GameObject($"TabBtn_{label}");
            btnObj.transform.SetParent(parent, false);

            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.10f, 0.13f, 0.18f, 1f);

            var btn = btnObj.AddComponent<Button>();

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = label;
            btnText.fontSize = 13f;
            btnText.fontStyle = FontStyles.Bold;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = new Color(0.8f, 0.8f, 0.85f);

            return btn;
        }

        private void ConstructTab1UI(Transform parent)
        {
            // Left Panel: The Uniform Chest & Ribbon Rack
            var leftObj = new GameObject("LeftPanel_RackAndBadges");
            leftObj.transform.SetParent(parent, false);
            var leftRt = leftObj.AddComponent<RectTransform>();
            leftRt.anchorMin = new Vector2(0f, 0f);
            leftRt.anchorMax = new Vector2(0.55f, 1f);
            leftRt.offsetMin = Vector2.zero;
            leftRt.offsetMax = new Vector2(-10, 0);

            var leftImg = leftObj.AddComponent<Image>();
            leftImg.color = new Color(0.06f, 0.08f, 0.12f, 0.9f);

            var leftVlg = leftObj.AddComponent<VerticalLayoutGroup>();
            leftVlg.padding = new RectOffset(16, 16, 14, 14);
            leftVlg.spacing = 10f;
            leftVlg.childForceExpandWidth = true;
            leftVlg.childForceExpandHeight = false;

            // Rack Section Header
            var rackHeadObj = new GameObject("RackHeader");
            rackHeadObj.transform.SetParent(leftObj.transform, false);
            var rhText = rackHeadObj.AddComponent<TextMeshProUGUI>();
            rhText.text = "<color=#FFD700><b>OFFICIAL RIBBON RACK (ORDER OF PRECEDENCE)</b></color>";
            rhText.fontSize = 14f;

            // Ribbon Rack Mounting Felt Board
            var feltBoardObj = new GameObject("RibbonRackBoard");
            feltBoardObj.transform.SetParent(leftObj.transform, false);
            var feltRt = feltBoardObj.AddComponent<RectTransform>();
            feltRt.sizeDelta = new Vector2(480, 240);

            var feltImg = feltBoardObj.AddComponent<Image>();
            feltImg.color = new Color(0.12f, 0.14f, 0.18f, 1f); // Dark charcoal uniform mounting felt

            var feltOutline = feltBoardObj.AddComponent<Outline>();
            feltOutline.effectColor = new Color(0.7f, 0.6f, 0.3f, 0.8f);
            feltOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var feltVlg = feltBoardObj.AddComponent<VerticalLayoutGroup>();
            feltVlg.padding = new RectOffset(10, 10, 10, 10);
            feltVlg.spacing = 8f;
            feltVlg.childAlignment = TextAnchor.UpperCenter;
            feltVlg.childForceExpandWidth = true;
            feltVlg.childForceExpandHeight = false;

            ribbonRackContainer = feltBoardObj.transform;

            // Badges & Devices Header
            var badgesHeadObj = new GameObject("BadgesHeader");
            badgesHeadObj.transform.SetParent(leftObj.transform, false);
            var bhText = badgesHeadObj.AddComponent<TextMeshProUGUI>();
            bhText.text = "<color=#00FFAA><b>UNIFORM INSIGNIAS, DEVICES & BADGES</b></color>";
            bhText.fontSize = 13f;

            // Badges Content Box
            var badgesBoxObj = new GameObject("BadgesBox");
            badgesBoxObj.transform.SetParent(leftObj.transform, false);
            var bbRt = badgesBoxObj.AddComponent<RectTransform>();
            bbRt.sizeDelta = new Vector2(480, 160);

            var bbImg = badgesBoxObj.AddComponent<Image>();
            bbImg.color = new Color(0.09f, 0.11f, 0.15f, 1f);

            var bbVlg = badgesBoxObj.AddComponent<VerticalLayoutGroup>();
            bbVlg.padding = new RectOffset(12, 12, 10, 10);
            bbVlg.spacing = 6f;
            bbVlg.childForceExpandWidth = true;
            bbVlg.childForceExpandHeight = false;

            rankBadgeText = CreateBadgeLabel(badgesBoxObj.transform, "Rank: Sergeant (E-5)");
            woundStarsBadgeText = CreateBadgeLabel(badgesBoxObj.transform, "Wound Stars: None");
            longevityHashesBadgeText = CreateBadgeLabel(badgesBoxObj.transform, "Service Hashes: 0");
            combatInfantryBadgeText = CreateBadgeLabel(badgesBoxObj.transform, "Combat Badge: None");

            // Right Panel: Selected Medal Inspection Details
            var rightObj = new GameObject("RightPanel_Inspection");
            rightObj.transform.SetParent(parent, false);
            var rightRt = rightObj.AddComponent<RectTransform>();
            rightRt.anchorMin = new Vector2(0.55f, 0f);
            rightRt.anchorMax = new Vector2(1f, 1f);
            rightRt.offsetMin = new Vector2(10, 0);
            rightRt.offsetMax = Vector2.zero;

            var rightImg = rightObj.AddComponent<Image>();
            rightImg.color = new Color(0.06f, 0.08f, 0.12f, 0.9f);

            var rightVlg = rightObj.AddComponent<VerticalLayoutGroup>();
            rightVlg.padding = new RectOffset(16, 16, 16, 16);
            rightVlg.spacing = 10f;
            rightVlg.childForceExpandWidth = true;
            rightVlg.childForceExpandHeight = false;

            var rHeadObj = new GameObject("InspectionHeader");
            rHeadObj.transform.SetParent(rightObj.transform, false);
            var rhText2 = rHeadObj.AddComponent<TextMeshProUGUI>();
            rhText2.text = "<color=#FFD700><b>MEDAL INSPECTION & AWARD HISTORY</b></color>";
            rhText2.fontSize = 14f;

            selectedRibbonTitleText = CreateDetailText(rightObj.transform, "SILVER CROSS", 18f, FontStyles.Bold);
            selectedRibbonPrecedenceText = CreateDetailText(rightObj.transform, "Order of Precedence: #3", 13f, FontStyles.Normal);
            selectedRibbonBountyText = CreateDetailText(rightObj.transform, "Imperial Treasury Bounty: +$15,000", 13f, FontStyles.Normal);
            selectedRibbonDateText = CreateDetailText(rightObj.transform, "Theater: Arden Ridge Trench Lines", 12f, FontStyles.Italic);
            selectedRibbonCriteriaText = CreateDetailText(rightObj.transform, "Award Criteria: Gallantry in action...", 13f, FontStyles.Normal);
        }

        private TextMeshProUGUI CreateBadgeLabel(Transform parent, string text)
        {
            var obj = new GameObject("BadgeLabel");
            obj.transform.SetParent(parent, false);
            var txt = obj.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = 13f;
            txt.color = Color.white;
            return txt;
        }

        private TextMeshProUGUI CreateDetailText(Transform parent, string text, float size, FontStyles style)
        {
            var obj = new GameObject("DetailText");
            obj.transform.SetParent(parent, false);
            var txt = obj.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = Color.white;
            return txt;
        }

        private void ConstructTab2UI(Transform parent)
        {
            // Scrollable view of parchment citations
            var scrollObj = new GameObject("CitationsScrollView");
            scrollObj.transform.SetParent(parent, false);
            var scrollRt = scrollObj.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = Vector2.zero;

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            var vpRt = viewportObj.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            viewportObj.AddComponent<RectMask2D>();

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            var cRt = contentObj.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.sizeDelta = new Vector2(0, 0);

            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(14, 14, 14, 14);
            vlg.spacing = 14f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRt;
            scrollRect.content = cRt;

            citationsContentContainer = contentObj.transform;
        }

        private void ConstructTab3UI(Transform parent)
        {
            // Regimental Memorial Wall of black polished marble
            var topBannerObj = new GameObject("MemorialBanner");
            topBannerObj.transform.SetParent(parent, false);
            var tbRt = topBannerObj.AddComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 1f);
            tbRt.anchorMax = new Vector2(1f, 1f);
            tbRt.pivot = new Vector2(0.5f, 1f);
            tbRt.sizeDelta = new Vector2(0, 60);

            var tbImg = topBannerObj.AddComponent<Image>();
            tbImg.color = new Color(0.06f, 0.07f, 0.09f, 1f);

            var quoteObj = new GameObject("MemorialQuote");
            quoteObj.transform.SetParent(topBannerObj.transform, false);
            var qRt = quoteObj.AddComponent<RectTransform>();
            qRt.anchorMin = Vector2.zero;
            qRt.anchorMax = Vector2.one;
            qRt.offsetMin = Vector2.zero;
            qRt.offsetMax = Vector2.zero;

            memorialHeaderQuoteText = quoteObj.AddComponent<TextMeshProUGUI>();
            memorialHeaderQuoteText.text = "<color=#FFD700><b>REGIMENTAL WALL OF HONOR • IN SACRED MEMORY OF OUR FALLEN BROTHERS</b></color>\n" +
                "<i>\"They shall grow not old, as we that are left grow old: Age shall not weary them, nor the years condemn.\"</i>";
            memorialHeaderQuoteText.fontSize = 13f;
            memorialHeaderQuoteText.alignment = TextAlignmentOptions.Center;

            // Scrollable list of plaques
            var scrollObj = new GameObject("MemorialScrollView");
            scrollObj.transform.SetParent(parent, false);
            var scrollRt = scrollObj.AddComponent<RectTransform>();
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = Vector2.zero;
            scrollRt.offsetMax = new Vector2(0, -65);

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            var vpRt = viewportObj.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;
            viewportObj.AddComponent<RectMask2D>();

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            var cRt = contentObj.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.sizeDelta = new Vector2(0, 0);

            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(14, 14, 14, 14);
            vlg.spacing = 12f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRt;
            scrollRect.content = cRt;

            memorialContentContainer = contentObj.transform;
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            if (uniformBackdrop != null) uniformBackdrop.color = palette.backgroundColor;
            if (dressUniformJacketPanel != null) dressUniformJacketPanel.color = palette.panelBackground;
            if (headerTitleText != null) headerTitleText.color = palette.accentGold;
        }
    }
}
