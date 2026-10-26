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
        public CareerDossierModal careerDossierModal;
        public MailCallModal mailCallModal;
        public MedalRibbonRackModal medalRibbonRackModal;
        public TacticalMinigamesModal tacticalMinigamesModal;

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
                var s = GameManager.Instance.currentSoldier;
                UpdateHUD(s);
                if (!s.hasAnsweredWartimeCall && s.age <= 18.5f && !s.isDead && !s.isRetired)
                {
                    GameManager.Instance.TriggerWartimeMobilizationPopup();
                }
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
                var s = GameManager.Instance.currentSoldier;
                UpdateHUD(s);
                if (!s.hasAnsweredWartimeCall && s.age <= 18.5f && !s.isDead && !s.isRetired)
                {
                    GameManager.Instance.TriggerWartimeMobilizationPopup();
                }
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
            if (careerDossierModal == null) careerDossierModal = CareerDossierModal.Instance ?? FindAnyObjectByType<CareerDossierModal>(FindObjectsInactive.Include);
            if (mailCallModal == null) mailCallModal = MailCallModal.Instance ?? FindAnyObjectByType<MailCallModal>(FindObjectsInactive.Include);
            if (medalRibbonRackModal == null) medalRibbonRackModal = MedalRibbonRackModal.Instance ?? FindAnyObjectByType<MedalRibbonRackModal>(FindObjectsInactive.Include);
            if (tacticalMinigamesModal == null) tacticalMinigamesModal = TacticalMinigamesModal.Instance ?? FindAnyObjectByType<TacticalMinigamesModal>(FindObjectsInactive.Include);

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
                int year = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2026;
                int sem = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentSemester : 1;
                
                string status;
                if (soldier.isDead) status = "<color=#FF0000>[KIA / DECEASED]</color>";
                else if (soldier.isRetired) status = "<color=#00FFAA>[RETIRED VETERAN]</color>";
                else if (soldier.careerTrack == CareerTrack.MilitaryWarAcademy)
                {
                    string top10 = soldier.isAcademyTop10Percent ? " (TOP 10%)" : "";
                    status = $"<color=#FFD700>[ACADEMY CADET Y{(soldier.academySemestersCompleted / 2) + 1}{top10}]</color>";
                }
                else if (soldier.isSpecialForcesCandidate)
                {
                    status = $"<color=#FF8800>[SF CANDIDATE T{soldier.specialForcesTrainingSemesters + 1}/2]</color>";
                }
                else if (soldier.isInCombatDeployment)
                {
                    string sf = soldier.isSpecialForces ? " [SPEC-OPS]" : "";
                    string ext = soldier.hasExtendedTourPayBonus ? " [+10% PAY]" : (soldier.isVoluntaryExtendedTour ? " [EXTENDED]" : "");
                    status = $"<color=#FF4444>[FRONTLINE{sf}{ext}]</color>";
                }
                else
                {
                    string sf = soldier.isSpecialForces ? " [SF BASE]" : "";
                    status = $"<color=#44FF44>[GARRISON{sf}]</color>";
                }

                ageAndCalendarLabel.text = $"Age {soldier.age:F1} | Year {year} S{sem} {status}";
            }

            // Rank & Origin
            if (rankLabel != null)
            {
                rankLabel.text = soldier.GetRankDisplayName();
            }

            if (nobleTitleLabel != null)
            {
                nobleTitleLabel.text = $"Origin: {soldier.origin} ({soldier.originTrait})";
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
                    monthlyPayLabel.text = "<color=#00FFAA>$0 / mo (Basic Training • 100% State Funded)</color>";
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
                    long loanDeduction = SalaryCalculator.CalculateMonthlyLoanDeduction(soldier);
                    string bonus = soldier.hasExtendedTourPayBonus ? " <color=#FFD700>(+10% Combat Bonus)</color>" : "";
                    string sfTag = soldier.isSpecialForces ? " <color=#FF8844>(+$800 SF)</color>" : "";
                    string loanTag = loanDeduction > 0 ? $" <color=#FF8866>(-${loanDeduction:N0} Loan)</color>" : "";
                    monthlyPayLabel.text = $"+${net:N0} / mo (40% Tax Deducted){bonus}{sfTag}{loanTag}";
                }
            }

            // Cultivation & Talent: Hidden to focus on authentic military simulation
            if (powerTierLabel != null) powerTierLabel.gameObject.SetActive(false);
            if (cultivationProgressBar != null) cultivationProgressBar.gameObject.SetActive(false);
            if (talentLabel != null) talentLabel.gameObject.SetActive(false);

            // Center Tour Advance Button label update
            if (advanceSemesterButton != null)
            {
                var btnText = advanceSemesterButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = soldier.isDead 
                        ? "VIEW DOSSIER" 
                        : (soldier.isRetired ? "VETERAN TOUR (+)" : "ADVANCE TOUR (+)");
                }
            }

            // Stat Sliders & Clean Percentage Texts
            if (healthSlider != null) healthSlider.value = soldier.health / 100f;
            if (healthBarText != null) healthBarText.text = $"Health: {soldier.health:F0}%";

            if (fitnessSlider != null) fitnessSlider.value = soldier.fitness / 100f;
            if (fitnessBarText != null) fitnessBarText.text = $"Readiness: {soldier.fitness:F0}%";

            if (combatSkillSlider != null) combatSkillSlider.value = soldier.combatSkill / 100f;
            if (combatBarText != null) combatBarText.text = $"Combat: {soldier.combatSkill:F0}%";

            if (intellectSlider != null) intellectSlider.value = soldier.intellect / 100f;
            if (intellectBarText != null) intellectBarText.text = $"Discipline: {soldier.intellect:F0}%";

            if (meritSlider != null) meritSlider.value = soldier.meritRating / 100f;
            if (influenceSlider != null) influenceSlider.value = soldier.politicalInfluence / 100f;
        }

        // ==========================================
        // 5 Core Button Click Handlers
        // ==========================================

        public void OnLifeButtonClicked()
        {
            AudioManager.PlayUiClick();
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
            AudioManager.PlayUiClick();
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
            AudioManager.PlayTurnAdvance();
            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                if (GameManager.Instance.currentSoldier.isDead)
                {
                    if (careerDossierModal != null)
                    {
                        careerDossierModal.OpenDossier(GameManager.Instance.currentSoldier);
                    }
                    else if (CareerDossierModal.Instance != null)
                    {
                        CareerDossierModal.Instance.OpenDossier(GameManager.Instance.currentSoldier);
                    }
                    return;
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceTurn();
            }
        }

        public void OnRelationshipsButtonClicked()
        {
            AudioManager.PlayUiClick();
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
            AudioManager.PlayUiClick();
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
            AudioManager.PlayUiClick();
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
            AudioManager.PlayUiClick();
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
            if (Camera.main != null) Camera.main.backgroundColor = p.backgroundColor;

            if (headerPanelImage != null) headerPanelImage.color = p.panelBackground;
            if (subHeaderPanelImage != null) subHeaderPanelImage.color = p.cardBackground;
            if (feedContainerImage != null) feedContainerImage.color = p.subCardBackground;
            if (statsPanelImage != null) statsPanelImage.color = p.cardBackground;
            if (bottomBarImage != null) bottomBarImage.color = p.panelBackground;

            if (nameLabel != null) nameLabel.color = p.textPrimary;
            if (ageAndCalendarLabel != null) ageAndCalendarLabel.color = p.textSecondary;
            if (rankLabel != null) rankLabel.color = p.accentGold;
            if (nobleTitleLabel != null) nobleTitleLabel.color = p.accentCoyote;
            if (cashBalanceLabel != null) cashBalanceLabel.color = p.accentEmerald;
            if (cmpBalanceLabel != null) cmpBalanceLabel.color = p.accentGold;
            if (monthlyPayLabel != null) monthlyPayLabel.color = p.textSecondary;
            if (powerTierLabel != null) powerTierLabel.color = p.textPrimary;

            // Top Buttons
            if (settingsButton != null)
            {
                var img = settingsButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = settingsButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }
            if (returnToMenuButton != null)
            {
                var img = returnToMenuButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = returnToMenuButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }

            // Stat Gauge Sliders & Text
            ApplySliderTheme(healthSlider, p.healthBarColor, p.barBackgroundColor);
            if (healthBarText != null) healthBarText.color = p.textPrimary;

            ApplySliderTheme(fitnessSlider, p.fitnessBarColor, p.barBackgroundColor);
            if (fitnessBarText != null) fitnessBarText.color = p.textPrimary;

            ApplySliderTheme(combatSkillSlider, p.combatBarColor, p.barBackgroundColor);
            if (combatBarText != null) combatBarText.color = p.textPrimary;

            ApplySliderTheme(intellectSlider, p.intellectBarColor, p.barBackgroundColor);
            if (intellectBarText != null) intellectBarText.color = p.textPrimary;

            ApplySliderTheme(meritSlider, p.accentGold, p.barBackgroundColor);
            ApplySliderTheme(influenceSlider, p.accentCobalt, p.barBackgroundColor);

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

        private void ApplySliderTheme(Slider slider, Color fillColor, Color bgColor)
        {
            if (slider == null) return;

            if (slider.fillRect != null)
            {
                var fillImg = slider.fillRect.GetComponent<Image>();
                if (fillImg != null) fillImg.color = fillColor;
            }

            Transform bgTransform = slider.transform.Find("Background");
            if (bgTransform != null)
            {
                var bgImg = bgTransform.GetComponent<Image>();
                if (bgImg != null) bgImg.color = bgColor;
            }
        }
    }
}
