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
        public YouthActivitiesModal youthActivitiesModal;
        public DilemmaModal dilemmaModal;
        public ClanModal clanModal;
        public SchoolModal schoolModal;

        [Header("Action Buttons")]
        public Button advanceSemesterButton;
        public Button testTalentButton;
        public Button sendMoneyHomeButton;
        public Button returnToMenuButton;
        public Button clanButton;
        public Button schoolButton;

        private void OnEnable()
        {
            GameEvents.OnSoldierStatsUpdated += UpdateHUD;
            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                UpdateHUD(GameManager.Instance.currentSoldier);
            }

            if (advanceSemesterButton != null)
            {
                advanceSemesterButton.onClick.AddListener(OnAdvanceSemesterClicked);
            }
            if (testTalentButton != null)
            {
                testTalentButton.onClick.AddListener(OnTestTalentClicked);
            }
            if (sendMoneyHomeButton != null)
            {
                sendMoneyHomeButton.onClick.AddListener(OnSendMoneyHomeClicked);
            }
            if (clanButton != null)
            {
                clanButton.onClick.AddListener(OnClanButtonClicked);
            }
            if (schoolButton != null)
            {
                schoolButton.onClick.AddListener(OnSchoolButtonClicked);
            }
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
            }
        }

        private void OnDisable()
        {
            GameEvents.OnSoldierStatsUpdated -= UpdateHUD;
            if (advanceSemesterButton != null)
            {
                advanceSemesterButton.onClick.RemoveListener(OnAdvanceSemesterClicked);
            }
            if (testTalentButton != null)
            {
                testTalentButton.onClick.RemoveListener(OnTestTalentClicked);
            }
            if (sendMoneyHomeButton != null)
            {
                sendMoneyHomeButton.onClick.RemoveListener(OnSendMoneyHomeClicked);
            }
            if (clanButton != null)
            {
                clanButton.onClick.RemoveListener(OnClanButtonClicked);
            }
            if (schoolButton != null)
            {
                schoolButton.onClick.RemoveListener(OnSchoolButtonClicked);
            }
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                UpdateHUD(GameManager.Instance.currentSoldier);
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

            // Finances (Reflecting 50% Imperial War Tax)
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
                else if (soldier.isRetired)
                {
                    monthlyPayLabel.text = "+$1,500 / mo (Pension)";
                }
                else
                {
                    long net = SalaryCalculator.CalculateNetMonthlyPay(soldier);
                    monthlyPayLabel.text = $"+${net:N0} / mo (50% Tax Deducted)";
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

            // Talent Display: COMPLETELY HIDDEN UNTIL TESTED AT E-4 (OR NOBLE BIRTH)!
            if (talentLabel != null)
            {
                talentLabel.gameObject.SetActive(soldier.isPersonalTalentRevealed);
                if (soldier.isPersonalTalentRevealed)
                {
                    talentLabel.text = $"Talent: {soldier.talentScore} / 200";
                }
            }

            // Button label updates
            if (advanceSemesterButton != null)
            {
                var btnText = advanceSemesterButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = soldier.isDead 
                        ? "REINCARNATE (NEW LIFE)" 
                        : (soldier.isYouthPhase ? "GROW UP (+6 MO)" : "ADVANCE (+6 MO)");
                }
            }

            // Action 2: "ACTIVITIES" during youth; "SEND $400 HOME" during military duty
            if (sendMoneyHomeButton != null)
            {
                var sendText = sendMoneyHomeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (sendText != null)
                {
                    sendText.text = soldier.isYouthPhase ? "📋 ACTIVITIES" : "💸 SEND $400 HOME";
                }
            }

            // Action 3: "TEST TALENT (E-4)" (Hidden during youth phase!)
            if (testTalentButton != null)
            {
                if (soldier.isYouthPhase)
                {
                    testTalentButton.gameObject.SetActive(false);
                }
                else
                {
                    bool canTest = TalentTestingApparatus.CanTestSelf(soldier) && !soldier.isPersonalTalentRevealed;
                    testTalentButton.gameObject.SetActive(canTest);
                    var testText = testTalentButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (testText != null) testText.text = "TEST (E-4)";
                }
            }

            // Stat Sliders & Percentage Texts
            if (healthSlider != null) healthSlider.value = soldier.health / 100f;
            if (healthBarText != null) healthBarText.text = $"❤️ Health: {soldier.health:F0}%";

            if (fitnessSlider != null) fitnessSlider.value = soldier.fitness / 100f;
            if (fitnessBarText != null) fitnessBarText.text = $"🏃 Fitness: {soldier.fitness:F0}%";

            if (combatSkillSlider != null) combatSkillSlider.value = soldier.combatSkill / 100f;
            if (combatBarText != null) combatBarText.text = $"⚔️ Combat: {soldier.combatSkill:F0}%";

            if (intellectSlider != null) intellectSlider.value = soldier.intellect / 100f;
            if (intellectBarText != null) intellectBarText.text = $"🧠 Intellect: {soldier.intellect:F0}%";

            if (meritSlider != null) meritSlider.value = soldier.meritRating / 100f;
            if (influenceSlider != null) influenceSlider.value = soldier.politicalInfluence / 100f;
        }

        private void OnAdvanceSemesterClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceTurn();
            }
        }

        private void OnTestTalentClicked()
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

        private void OnSendMoneyHomeClicked()
        {
            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                var soldier = GameManager.Instance.currentSoldier;
                if (soldier.isYouthPhase)
                {
                    // During Youth Phase, this button opens the BitLife-style Activities Menu!
                    if (youthActivitiesModal != null)
                    {
                        youthActivitiesModal.OpenModal();
                    }
                    else if (YouthActivitiesModal.Instance != null)
                    {
                        YouthActivitiesModal.Instance.OpenModal();
                    }
                    else
                    {
                        var found = FindAnyObjectByType<YouthActivitiesModal>(FindObjectsInactive.Include);
                        if (found != null)
                        {
                            found.OpenModal();
                        }
                    }
                }
                else
                {
                    GameManager.Instance.SendMoneyToFamily(400);
                }
            }
        }

        private void OnReturnToMenuClicked()
        {
            if (OpeningSceneController.Instance != null)
            {
                OpeningSceneController.Instance.ShowOpeningScreen();
            }
        }

        public void OnClanButtonClicked()
        {
            if (clanModal != null)
            {
                clanModal.OpenModal();
            }
            else if (ClanModal.Instance != null)
            {
                ClanModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<ClanModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }

        public void OnSchoolButtonClicked()
        {
            if (schoolModal != null)
            {
                schoolModal.OpenModal();
            }
            else if (SchoolModal.Instance != null)
            {
                SchoolModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<SchoolModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }
    }
}
