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
        public Slider fitnessSlider;
        public Slider combatSkillSlider;
        public Slider meritSlider;
        public Slider influenceSlider;

        [Header("Action Buttons")]
        public Button advanceSemesterButton;
        public Button testTalentButton;
        public Button sendMoneyHomeButton;

        private void OnEnable()
        {
            GameEvents.OnSoldierStatsUpdated += UpdateHUD;
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
        }
                testTalentButton.onClick.RemoveListener(OnTestTalentClicked);
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
                int year = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2026;
                int sem = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentSemester : 1;
                
                string status;
                if (soldier.isDead) status = "<color=#FF0000>[DEAD]</color>";
                else if (soldier.isRetired) status = "<color=#FFFF44>[VETERAN PENSION]</color>";
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
                else
                {
                    rankStr = (soldier.currentRank != null) 
                        ? $"{soldier.currentRank.rankName} ({soldier.currentRank.paygrade})" 
                        : "Recruit";
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

                    if (soldier.powerTier >= 11 && !soldier.unlockedTiers11And12)
                    {
                        powerTierLabel.text = $"Tier VI (Peak Mortal Limit){damageWarning}";
                    }
                    else
                    {
                        powerTierLabel.text = $"Power Tier: {soldier.powerTier} / 12{damageWarning}";
                    }
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
                    btnText.text = soldier.isDead ? "REINCARNATE (NEW LIFE)" : "ADVANCE 6 MONTHS (+)";
                }
            }

            // Test Talent Button Visibility
            if (testTalentButton != null)
            {
                bool canTest = TalentTestingApparatus.CanTestSelf(soldier) && !soldier.isPersonalTalentRevealed;
                testTalentButton.gameObject.SetActive(canTest);
            }

            // Stat Sliders
            if (healthSlider != null) healthSlider.value = soldier.health / 100f;
            if (fitnessSlider != null) fitnessSlider.value = soldier.fitness / 100f;
            if (combatSkillSlider != null) combatSkillSlider.value = soldier.combatSkill / 100f;
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
                TalentTestingApparatus.PerformSelfApparatusTest(GameManager.Instance.currentSoldier);
            }
        }

        private void OnSendMoneyHomeClicked()
        {
            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                GameManager.Instance.SendMoneyToFamily(400);
            }
        }
    }
}
