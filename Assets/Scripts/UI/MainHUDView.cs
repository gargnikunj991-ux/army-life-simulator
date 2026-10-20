using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class MainHUDView : MonoBehaviour
    {
        [Header("Identity & Header Labels")]
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI ageAndCalendarLabel;
        public TextMeshProUGUI rankLabel;
        public TextMeshProUGUI nobleTitleLabel;

        [Header("Finances & Military Compensation")]
        public TextMeshProUGUI cashBalanceLabel;
        public TextMeshProUGUI cmpBalanceLabel;
        public TextMeshProUGUI monthlyPayLabel;

        [Header("Power Cultivation & Talent")]
        public TextMeshProUGUI powerTierLabel;
        public TextMeshProUGUI talentLabel;
        public Slider cultivationProgressBar;

        [Header("Stat Gauges (Sliders 0-100)")]
        public Slider healthSlider;
        public TextMeshProUGUI healthBarText;
        public Slider fitnessSlider;
        public TextMeshProUGUI fitnessBarText;
        public Slider combatSkillSlider;
        public TextMeshProUGUI combatBarText;
        public Slider intellectSlider;
        public TextMeshProUGUI intellectBarText;
        public Slider meritSlider;
        public Slider influenceSlider;

        [Header("Modals")]
        public LifeModal lifeModal;
        public FinanceModal financeModal;
        public RelationshipsModal relationshipsModal;
        public ActivitiesModal activitiesModal;
        public SettingsModal settingsModal;
        public DilemmaModal dilemmaModal;
        public ClanModal clanModal;
        public SchoolModal schoolModal;
        public YouthActivitiesModal youthActivitiesModal;

        [Header("5 Core Bottom Action Buttons")]
        public Button lifeButton;
        public Button financeButton;
        public Button advanceSemesterButton;
        public Button relationshipsButton;
        public Button activitiesButton;

        [Header("Top Header Action Buttons")]
        public Button settingsButton;
        public Button returnToMenuButton;

        [Header("Legacy Action Buttons (Backwards Compatibility)")]
        public Button testTalentButton;
        public Button sendMoneyHomeButton;
        public Button clanButton;
        public Button schoolButton;

        [Header("Themed Background Panels")]
        public Image headerPanelImage;
        public Image subHeaderPanelImage;
        public Image feedContainerImage;
        public Image statsPanelImage;
        public Image bottomBarImage;

        private void Awake()
        {
            EnsureUIBindings();
        }

        private void OnEnable()
        {
            GameEvents.OnSoldierStatsUpdated += UpdateHUD;
            ThemeManager.OnThemeChanged += OnThemeChanged;

            EnsureUIBindings();

            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                UpdateHUD(GameManager.Instance.currentSoldier);
            }

            // Apply Theme
            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private void OnDisable()
        {
            GameEvents.OnSoldierStatsUpdated -= UpdateHUD;
            ThemeManager.OnThemeChanged -= OnThemeChanged;

            RemoveButtonListeners();
        }

        private void Start()
        {
            EnsureUIBindings();

            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                UpdateHUD(GameManager.Instance.currentSoldier);
            }
            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        /// <summary>
        /// Robust self-healing: automatically discovers and binds missing UI buttons and modals at runtime.
        /// Also strips emojis to eliminate any missing font glyph square tofu boxes.
        /// </summary>
        public void EnsureUIBindings()
        {
            // 1. Modals auto-link
            if (lifeModal == null) lifeModal = LifeModal.Instance ?? FindAnyObjectByType<LifeModal>(FindObjectsInactive.Include);
            if (financeModal == null) financeModal = FinanceModal.Instance ?? FindAnyObjectByType<FinanceModal>(FindObjectsInactive.Include);
            if (relationshipsModal == null) relationshipsModal = RelationshipsModal.Instance ?? FindAnyObjectByType<RelationshipsModal>(FindObjectsInactive.Include);
            if (activitiesModal == null) activitiesModal = ActivitiesModal.Instance ?? FindAnyObjectByType<ActivitiesModal>(FindObjectsInactive.Include);
            if (settingsModal == null) settingsModal = SettingsModal.Instance ?? FindAnyObjectByType<SettingsModal>(FindObjectsInactive.Include);
            if (dilemmaModal == null) dilemmaModal = DilemmaModal.Instance ?? FindAnyObjectByType<DilemmaModal>(FindObjectsInactive.Include);
            if (clanModal == null) clanModal = ClanModal.Instance ?? FindAnyObjectByType<ClanModal>(FindObjectsInactive.Include);
            if (schoolModal == null) schoolModal = SchoolModal.Instance ?? FindAnyObjectByType<SchoolModal>(FindObjectsInactive.Include);
            if (youthActivitiesModal == null) youthActivitiesModal = YouthActivitiesModal.Instance ?? FindAnyObjectByType<YouthActivitiesModal>(FindObjectsInactive.Include);

            // 2. Bottom buttons auto-link
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string bName = b.gameObject.name.ToLower();
                if (lifeButton == null && bName.Contains("life")) lifeButton = b;
                else if (financeButton == null && bName.Contains("finance")) financeButton = b;
                else if (advanceSemesterButton == null && (bName.Contains("advance") || bName.Contains("grow") || bName.Contains("age"))) advanceSemesterButton = b;
                else if (relationshipsButton == null && (bName.Contains("relation") || bName.Contains("friend"))) relationshipsButton = b;
                else if (activitiesButton == null && bName.Contains("activit")) activitiesButton = b;
                else if (settingsButton == null && (bName.Contains("setting") || bName.Contains("cfg") || bName.Contains("gear"))) settingsButton = b;
                else if (returnToMenuButton == null && (bName.Contains("menu") || bName.Contains("title"))) returnToMenuButton = b;
            }

            // 3. Attach listeners cleanly
            RemoveButtonListeners();

            if (lifeButton != null) lifeButton.onClick.AddListener(OnLifeButtonClicked);
            if (financeButton != null) financeButton.onClick.AddListener(OnFinanceButtonClicked);
            if (advanceSemesterButton != null) advanceSemesterButton.onClick.AddListener(OnAdvanceSemesterClicked);
            if (relationshipsButton != null) relationshipsButton.onClick.AddListener(OnRelationshipsButtonClicked);
            if (activitiesButton != null) activitiesButton.onClick.AddListener(OnActivitiesButtonClicked);

            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);

            if (testTalentButton != null) testTalentButton.onClick.AddListener(OnTestTalentClicked);
            if (sendMoneyHomeButton != null) sendMoneyHomeButton.onClick.AddListener(OnSendMoneyHomeClicked);
            if (clanButton != null) clanButton.onClick.AddListener(OnClanButtonClicked);
            if (schoolButton != null) schoolButton.onClick.AddListener(OnSchoolButtonClicked);

            // 4. Sanitize button labels to clean ASCII
            SanitizeButtonLabel(lifeButton, "LIFE");
            SanitizeButtonLabel(financeButton, "FINANCE");
            SanitizeButtonLabel(relationshipsButton, "RELATIONS");
            SanitizeButtonLabel(activitiesButton, "ACTIVITIES");
            SanitizeButtonLabel(settingsButton, "SETTINGS");
        }

        private void RemoveButtonListeners()
        {
            if (lifeButton != null) lifeButton.onClick.RemoveListener(OnLifeButtonClicked);
            if (financeButton != null) financeButton.onClick.RemoveListener(OnFinanceButtonClicked);
            if (advanceSemesterButton != null) advanceSemesterButton.onClick.RemoveListener(OnAdvanceSemesterClicked);
            if (relationshipsButton != null) relationshipsButton.onClick.RemoveListener(OnRelationshipsButtonClicked);
            if (activitiesButton != null) activitiesButton.onClick.RemoveListener(OnActivitiesButtonClicked);

            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            if (returnToMenuButton != null) returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);

            if (testTalentButton != null) testTalentButton.onClick.RemoveListener(OnTestTalentClicked);
            if (sendMoneyHomeButton != null) sendMoneyHomeButton.onClick.RemoveListener(OnSendMoneyHomeClicked);
            if (clanButton != null) clanButton.onClick.RemoveListener(OnClanButtonClicked);
            if (schoolButton != null) schoolButton.onClick.RemoveListener(OnSchoolButtonClicked);
        }

        private void SanitizeButtonLabel(Button btn, string cleanText)
        {
            if (btn == null) return;
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null && (tmp.text.Contains("?") || tmp.text.Length > 20 || tmp.text.Contains("\u2699") || tmp.text.Contains("\U0001F464") || tmp.text.Contains("\U0001F4B0") || tmp.text.Contains("\U0001F465") || tmp.text.Contains("\u26A1")))
            {
                tmp.text = cleanText;
            }
        }

        public void UpdateHUD(SoldierProfile soldier)
        {
            if (soldier == null) return;

            // Name & Age
            if (nameLabel != null)
            {
                nameLabel.text = soldier.isDead 
                    ? $"<color=#FF4444>{soldier.characterName} (DECEASED)</color>" 
                    : soldier.characterName;
            }

            if (ageAndCalendarLabel != null)
            {
                int year = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2008;
                int sem = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentSemester : 1;
                
                string status;
                if (soldier.isDead) status = "<color=#FF0000>[DEAD]</color>";
                else if (soldier.isRetired) status = "<color=#FFFF44>[VETERAN PENSION]</color>";
                else if (soldier.isYouthPhase) status = "<color=#00D4FF>[YOUTH PHASE]</color>";
                else if (soldier.careerTrack == CareerTrack.CivilianCollege) status = "<color=#FFAA00>[COLLEGE]</color>";
                else if (soldier.isCivilian) status = "<color=#44DDFF>[CIVILIAN LIFE]</color>";
                else if (soldier.careerTrack == CareerTrack.MilitaryRecruitBasic) status = "<color=#FFAA00>[BASIC TRAINING]</color>";
                else if (soldier.careerTrack == CareerTrack.MilitaryTechSchool || soldier.careerTrack == CareerTrack.MilitaryMedicalSchool || soldier.careerTrack == CareerTrack.MilitaryWarAcademy) status = "<color=#AA88FF>[ACADEMY]</color>";
                else status = soldier.isInCombatDeployment ? "<color=#FF4444>[FRONT TOUR]</color>" : "<color=#44FF44>[BASE]</color>";

                ageAndCalendarLabel.text = $"Age {soldier.age:F1} | Year {year} S{sem} {status}";
            }

            // Rank & Nobility
            if (rankLabel != null)
            {
                string rankStr;
                if (soldier.isDead)
                {
                    rankStr = "<color=#FF4444>Fallen Hero (KIA)</color>";
                }
                else if (soldier.isRetired)
                {
                    rankStr = $"[RETIRED] {(soldier.currentRank != null ? soldier.currentRank.rankName : "Veteran")}";
                }
                else if (soldier.isYouthPhase)
                {
                    if (soldier.age < 2.0f) rankStr = "Infant";
                    else if (soldier.age < 5.0f) rankStr = "Toddler";
                    else if (soldier.age < 12.0f) rankStr = "Primary School Pupil";
                    else rankStr = "Military Academy Cadet";
                }
                else if (soldier.careerTrack == CareerTrack.CivilianCollege)
                {
                    rankStr = $"Imperial College Student ({soldier.collegeDegreeName ?? "Imperial Studies"})";
                }
                else if (soldier.careerTrack == CareerTrack.CivilianWork)
                {
                    rankStr = $"Civilian: {soldier.civilianJobTitle}";
                }
                else if (soldier.careerTrack == CareerTrack.CivilianProfessional)
                {
                    rankStr = $"Professional: {soldier.civilianJobTitle}";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryRecruitBasic)
                {
                    rankStr = "Basic Combat Recruit (Unpaid)";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryTechSchool)
                {
                    int yr = (soldier.militarySchoolSemestersCompleted / 2) + 1;
                    rankStr = $"Tech School Cadet (Year {Mathf.Min(2, yr)}/2)";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryMedicalSchool)
                {
                    int yr = (soldier.militarySchoolSemestersCompleted / 2) + 1;
                    rankStr = $"Medical Cadet (Year {Mathf.Min(3, yr)}/3)";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryWarAcademy)
                {
                    int yr = (soldier.militarySchoolSemestersCompleted / 2) + 1;
                    rankStr = $"War Academy Cadet (Year {Mathf.Min(4, yr)}/4)";
                }
                else
                {
                    rankStr = (soldier.currentRank != null) 
                        ? $"{soldier.currentRank.rankName} ({soldier.currentRank.paygrade})" 
                        : "Private Recruit (E-1)";
                }
                rankLabel.text = rankStr;
            }

            if (nobleTitleLabel != null)
            {
                string clanStatus = soldier.isClanHead ? " (Patriarch)" : "";
                nobleTitleLabel.text = $"{soldier.nobleTitle}{clanStatus}";
            }

            // Finances (Reflecting Taxes & Career Track)
            if (cashBalanceLabel != null) cashBalanceLabel.text = $"${soldier.cashBalance:N0}";
            if (cmpBalanceLabel != null) cmpBalanceLabel.text = $"{soldier.combatMeritPoints:N0} CMP";
            if (monthlyPayLabel != null)
            {
                if (soldier.isDead)
                {
                    monthlyPayLabel.text = "$0 / mo";
                }
                else if (soldier.isYouthPhase)
                {
                    monthlyPayLabel.text = "<color=#888888>Family Dependent ($0/mo)</color>";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryRecruitBasic)
                {
                    monthlyPayLabel.text = "<color=#888888>$0 / mo (Unpaid Basic Training)</color>";
                }
                else if (soldier.careerTrack == CareerTrack.CivilianCollege)
                {
                    monthlyPayLabel.text = "<color=#888888>$0 / mo (Full-Time Student)</color>";
                }
                else if (soldier.isCivilian)
                {
                    long net = SalaryCalculator.CalculateNetMonthlyPay(soldier);
                    monthlyPayLabel.text = $"+${net:N0} / mo (50% Civil Tax Deducted)";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryTechSchool)
                {
                    long pay = SalaryCalculator.CalculateGrossMonthlyPay(soldier);
                    monthlyPayLabel.text = pay > 0 ? $"+${pay:N0} / mo (Tech Cadet Stipend)" : "$0 / mo (Cadet Trainee)";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryMedicalSchool)
                {
                    long pay = SalaryCalculator.CalculateGrossMonthlyPay(soldier);
                    monthlyPayLabel.text = pay > 0 ? $"+${pay:N0} / mo (Med Cadet Stipend)" : "$0 / mo (Cadet Trainee)";
                }
                else if (soldier.careerTrack == CareerTrack.MilitaryWarAcademy)
                {
                    long pay = SalaryCalculator.CalculateGrossMonthlyPay(soldier);
                    monthlyPayLabel.text = $"+${pay:N0} / mo (War Academy Officer Cadet)";
                }
                else if (soldier.isRetired)
                {
                    monthlyPayLabel.text = "+$1,500 / mo (Pension)";
                }
                else
                {
                    long net = SalaryCalculator.CalculateNetMonthlyPay(soldier);
                    monthlyPayLabel.text = $"+${net:N0} / mo (40% Tax Deducted)";
                }
            }

            // Cultivation & Power Tier: HIDDEN FOR COMMONERS UNTIL UNLOCKED!
            bool showCultivation = soldier.knowsAboutCultivation || soldier.nobleTitle > NobleRank.Commoner;
            if (powerTierLabel != null)
            {
                powerTierLabel.gameObject.SetActive(showCultivation);
                if (showCultivation)
                {
                    string damageWarning = soldier.meridianDamage > 20f 
                        ? $" <color=#FF4444>[Dmg: {soldier.meridianDamage:F0}%]</color>" 
                        : "";

                    powerTierLabel.text = $"{PowerTierKnowledgeSystem.FormatSoldierPowerTier(soldier)}{damageWarning}";
                }
            }

            if (cultivationProgressBar != null)
            {
                cultivationProgressBar.gameObject.SetActive(showCultivation);
                if (showCultivation)
                {
                    cultivationProgressBar.value = soldier.tierProgressPercent / 100f;
                }
            }

            // Talent Display: HIDDEN UNTIL TESTED AT E-4 (OR NOBLE BIRTH)!
            if (talentLabel != null)
            {
                talentLabel.gameObject.SetActive(soldier.isPersonalTalentRevealed);
                if (soldier.isPersonalTalentRevealed)
                {
                    talentLabel.text = $"Talent: {soldier.talentScore} / 200";
                }
            }

            // Center Age Button label update
            if (advanceSemesterButton != null)
            {
                var btnText = advanceSemesterButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = soldier.isDead 
                        ? "REINCARNATE" 
                        : (soldier.isYouthPhase ? "GROW UP (+)" : "AGE (+)");
                }
            }

            // Stat Sliders & Clean Percentage Texts (NO EMOJIS TO PREVENT TOFU BOXES)
            if (healthSlider != null) healthSlider.value = soldier.health / 100f;
            if (healthBarText != null) healthBarText.text = $"Health: {soldier.health:F0}%";

            if (fitnessSlider != null) fitnessSlider.value = soldier.fitness / 100f;
            if (fitnessBarText != null) fitnessBarText.text = $"Fitness: {soldier.fitness:F0}%";

            if (combatSkillSlider != null) combatSkillSlider.value = soldier.combatSkill / 100f;
            if (combatBarText != null) combatBarText.text = $"Combat: {soldier.combatSkill:F0}%";

            if (intellectSlider != null) intellectSlider.value = soldier.intellect / 100f;
            if (intellectBarText != null) intellectBarText.text = $"Intellect: {soldier.intellect:F0}%";

            if (meritSlider != null) meritSlider.value = soldier.meritRating / 100f;
            if (influenceSlider != null) influenceSlider.value = soldier.politicalInfluence / 100f;
        }

        // ==========================================
        // 5 Core Button Click Handlers
        // ==========================================

        public void OnLifeButtonClicked()
        {
            if (lifeModal != null)
            {
                lifeModal.OpenModal();
            }
            else if (LifeModal.Instance != null)
            {
                LifeModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<LifeModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }

        public void OnFinanceButtonClicked()
        {
            if (financeModal != null)
            {
                financeModal.OpenModal();
            }
            else if (FinanceModal.Instance != null)
            {
                FinanceModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<FinanceModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }

        public void OnAdvanceSemesterClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceTurn();
            }
        }

        public void OnRelationshipsButtonClicked()
        {
            if (relationshipsModal != null)
            {
                relationshipsModal.OpenModal();
            }
            else if (RelationshipsModal.Instance != null)
            {
                RelationshipsModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<RelationshipsModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }

        public void OnActivitiesButtonClicked()
        {
            if (activitiesModal != null)
            {
                activitiesModal.OpenModal();
            }
            else if (ActivitiesModal.Instance != null)
            {
                ActivitiesModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<ActivitiesModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }

        // ==========================================
        // Header & Secondary Handlers
        // ==========================================

        public void OnSettingsButtonClicked()
        {
            if (settingsModal != null)
            {
                settingsModal.OpenModal();
            }
            else if (SettingsModal.Instance != null)
            {
                SettingsModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<SettingsModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }

        public void OnReturnToMenuClicked()
        {
            if (OpeningSceneController.Instance != null)
            {
                OpeningSceneController.Instance.ShowOpeningScreen();
            }
        }

        // Legacy / Backwards compatibility
        public void OnClanButtonClicked()
        {
            if (relationshipsModal != null)
            {
                relationshipsModal.OpenModal(showFriendsFirst: false);
            }
            else if (clanModal != null)
            {
                clanModal.OpenModal();
            }
        }

        public void OnSchoolButtonClicked()
        {
            if (schoolModal != null)
            {
                schoolModal.OpenModal();
            }
        }

        public void OnSendMoneyHomeClicked()
        {
            OnFinanceButtonClicked();
        }

        public void OnTestTalentClicked()
        {
            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                var soldier = GameManager.Instance.currentSoldier;
                if (!soldier.isYouthPhase)
                {
                    TalentTestingApparatus.PerformSelfApparatusTest(soldier);
                }
            }
        }

        // ==========================================
        // Theme Styling
        // ==========================================

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (headerPanelImage != null) headerPanelImage.color = p.panelBackground;
            if (subHeaderPanelImage != null) subHeaderPanelImage.color = p.cardBackground;
            if (feedContainerImage != null) feedContainerImage.color = p.subCardBackground;
            if (statsPanelImage != null) statsPanelImage.color = p.cardBackground;
            if (bottomBarImage != null) bottomBarImage.color = p.panelBackground;

            if (nameLabel != null) nameLabel.color = p.textPrimary;
            if (ageAndCalendarLabel != null) ageAndCalendarLabel.color = p.textSecondary;
            if (cashBalanceLabel != null) cashBalanceLabel.color = p.accentEmerald;
            if (cmpBalanceLabel != null) cmpBalanceLabel.color = p.accentPurple;
            if (powerTierLabel != null) powerTierLabel.color = p.textPrimary;

            // Recolor buttons if present
            if (lifeButton != null)
            {
                var img = lifeButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = lifeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }

            if (financeButton != null)
            {
                var img = financeButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = financeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }

            if (advanceSemesterButton != null)
            {
                var img = advanceSemesterButton.GetComponent<Image>();
                if (img != null) img.color = p.btnAgeGrowBg;
                var txt = advanceSemesterButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnAgeGrowText;
            }

            if (relationshipsButton != null)
            {
                var img = relationshipsButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = relationshipsButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }

            if (activitiesButton != null)
            {
                var img = activitiesButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = activitiesButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }
        }
    }
}
