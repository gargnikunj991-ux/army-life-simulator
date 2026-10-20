using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class FinanceModal : MonoBehaviour
    {
        public static FinanceModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header")]
        public TextMeshProUGUI titleText;
        public Button closeButton;

        [Header("Liquid Wealth")]
        public TextMeshProUGUI cashBalanceText;
        public TextMeshProUGUI cmpBalanceText;

        [Header("Monthly Income Breakdown (Wartime Economy)")]
        public TextMeshProUGUI monthlyBreakdownText;

        [Header("Family Economic Status & Remittances")]
        public TextMeshProUGUI familyWealthStatusText;
        public Button send200Button;
        public Button send500Button;
        public Button send1000Button;

        [Header("Imperial Logistics & War Bonds")]
        public Button buyWarBondButton;
        public Button emergencyAdvanceButton;
        public TextMeshProUGUI warBondStatusText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            if (send200Button != null) send200Button.onClick.AddListener(() => SendRemittance(200));
            if (send500Button != null) send500Button.onClick.AddListener(() => SendRemittance(500));
            if (send1000Button != null) send1000Button.onClick.AddListener(() => SendRemittance(1000));
            if (buyWarBondButton != null) buyWarBondButton.onClick.AddListener(OnBuyWarBondClicked);
            if (emergencyAdvanceButton != null) emergencyAdvanceButton.onClick.AddListener(OnEmergencyAdvanceClicked);

            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            if (send200Button != null) send200Button.onClick.RemoveAllListeners();
            if (send500Button != null) send500Button.onClick.RemoveAllListeners();
            if (send1000Button != null) send1000Button.onClick.RemoveAllListeners();
            if (buyWarBondButton != null) buyWarBondButton.onClick.RemoveListener(OnBuyWarBondClicked);
            if (emergencyAdvanceButton != null) emergencyAdvanceButton.onClick.RemoveListener(OnEmergencyAdvanceClicked);

            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

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
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            // Liquid balances
            if (cashBalanceText != null)
            {
                cashBalanceText.text = $"Liquid Cash: <color=#00FFAA>${s.cashBalance:N0}</color>";
            }

            if (cmpBalanceText != null)
            {
                cmpBalanceText.text = $"Combat Merit Points: <color=#FF88FF>{s.combatMeritPoints:N0} CMP</color>";
            }

            // Monthly breakdown
            if (monthlyBreakdownText != null)
            {
                if (s.isDead)
                {
                    monthlyBreakdownText.text = "<b>Monthly Earnings:</b> $0 (Fallen Hero)";
                }
                else if (s.isYouthPhase)
                {
                    monthlyBreakdownText.text = 
                        "<b>Current Income Status:</b> Dependent Student / Cadet\n" +
                        "<i>Cadets receive free academy meals and academic boarding, but no monthly salary until official enlistment or graduation.</i>";
                }
                else if (s.careerTrack == CareerTrack.CivilianCollege)
                {
                    long debt = s.collegeTuitionDebt;
                    monthlyBreakdownText.text = 
                        $"<b>Imperial College Undergraduate:</b> $0 / mo (Full-Time Degree Student)\n" +
                        $"<b>Semester Tuition:</b> $12,500 / sem ($100,000 for 4 Years / 8 Semesters)\n" +
                        $"<color={(debt > 0 ? "#FF6666" : "#00FFAA")}><b>Outstanding Student Loan Debt:</b> ${debt:N0}</color>\n" +
                        $"<i>Degrees confer professional civil positions starting at $500+/mo upon graduation.</i>";
                }
                else if (s.isCivilian)
                {
                    long gross = s.civilianSalaryPerMonth;
                    long net = SalaryCalculator.CalculateNetMonthlyPay(s);
                    long tax = gross - net;
                    string debtTag = s.collegeTuitionDebt > 0 ? $"\n<color=#FF6666><b>Student Tuition Debt Remaining:</b> ${s.collegeTuitionDebt:N0}</color>" : "";

                    monthlyBreakdownText.text = 
                        $"<b>Gross Civilian Wage ({s.civilianJobTitle}):</b> ${gross:N0} / mo\n" +
                        $"<color=#FF8866><b>Municipal Civil Income Tax (50%):</b> -${tax:N0} / mo</color>\n" +
                        $"<color=#00FF88><b>Net Take-Home Pay:</b> +${net:N0} / mo</color>{debtTag}\n" +
                        $"<i>Civilian citizens pay 50% municipal tax towards public infrastructure and municipal governance.</i>";
                }
                else if (s.careerTrack == CareerTrack.MilitaryRecruitBasic)
                {
                    monthlyBreakdownText.text = 
                        "<b>Basic Combat Training Recruit:</b> $0 / mo (Unpaid Basic Drills)\n" +
                        "<i>Recruits receive standard field rations and boot camp gear. Pay begins upon passing out as Private (E-1) in 6 months.</i>";
                }
                else if (s.careerTrack == CareerTrack.MilitaryTechSchool || s.careerTrack == CareerTrack.MilitaryMedicalSchool || s.careerTrack == CareerTrack.MilitaryWarAcademy)
                {
                    long pay = SalaryCalculator.CalculateGrossMonthlyPay(s);
                    string payNote = pay > 0 ? $"+${pay:N0} / mo (Tax-Exempt Cadet Stipend)" : "$0 / mo (Year 1 Training Phase)";
                    monthlyBreakdownText.text = 
                        $"<b>Academy Cadet Status:</b> {s.assignedDepartment}\n" +
                        $"<b>Cadet Monthly Stipend:</b> {payNote}\n" +
                        $"<i>Cadet educational stipends are fully exempt from the 40% Imperial War Tax during training.</i>";
                }
                else if (s.isRetired)
                {
                    monthlyBreakdownText.text = 
                        "<b>Imperial Veteran Pension:</b> +$1,500 / month\n" +
                        "<i>Exempt from standard military hazard deductions. Paid directly from Imperial Treasury.</i>";
                }
                else
                {
                    long gross = SalaryCalculator.CalculateGrossMonthlyPay(s);
                    long net = SalaryCalculator.CalculateNetMonthlyPay(s);
                    long tax = gross - net;

                    string tourBonus = s.isInCombatDeployment ? " (+$600 Combat Hazard Pay included)" : "";

                    monthlyBreakdownText.text = 
                        $"<b>Gross Military Salary:</b> ${gross:N0} / mo{tourBonus}\n" +
                        $"<color=#FF6666><b>Mandatory Imperial War Tax (40%):</b> -${tax:N0} / mo</color>\n" +
                        $"<color=#00FF88><b>Net Take-Home Pay:</b> +${net:N0} / mo</color>\n" +
                        $"<i>Imperial Decree: 40% of all imperial soldiers' pay is deducted to fund frontline munitions, heavy artillery, and continental defense arrays.</i>";
                }
            }

            // Family Status
            if (familyWealthStatusText != null)
            {
                string familyTier;
                if (s.familyLineage != null && s.familyLineage.startingNobleRank > NobleRank.Commoner)
                {
                    familyTier = "Hereditary Aristocracy (Affluent)";
                }
                else if (s.totalRemittancesSentToFamily >= 3000)
                {
                    familyTier = "Comfortable (Lifted by Remittances)";
                }
                else if (s.totalRemittancesSentToFamily >= 1000)
                {
                    familyTier = "Modest Working Class";
                }
                else
                {
                    familyTier = "Impoverished Wartime Civilian Class";
                }

                familyWealthStatusText.text = 
                    $"<b>Family House:</b> House {s.lastName}   |   <b>Economic Tier:</b> {familyTier}\n" +
                    $"<b>Total Remittances Delivered:</b> ${s.totalRemittancesSentToFamily:N0}   |   <b>Parent Relationship:</b> {s.parentRelationship:F0}%\n" +
                    $"<i>Sending money home lifts your family out of wartime rationing and grants massive Clan Favor & Parent Approval.</i>";
            }

            // War Bonds / Student Loan Status
            if (warBondStatusText != null)
            {
                if (s.collegeTuitionDebt > 0)
                {
                    warBondStatusText.text = 
                        $"<b>Imperial College Student Loan:</b> Outstanding balance of <b>${s.collegeTuitionDebt:N0}</b>. " +
                        $"Use your earnings or family remittances to eliminate debt burden.";
                }
                else
                {
                    warBondStatusText.text = 
                        "<b>Imperial Defense War Bonds:</b> Purchase a $500 Certificate of Valor. Yields dividends and boosts imperial court prestige.";
                }
            }

            // Apply Theme Palette
            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private void SendRemittance(int amount)
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.cashBalance < amount)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> You do not have ${amount:N0} to send home (Current cash: ${s.cashBalance:N0}).");
                return;
            }

            s.cashBalance -= amount;
            s.totalRemittancesSentToFamily += amount;
            s.parentRelationship = Mathf.Min(100f, s.parentRelationship + (amount >= 1000 ? 15f : (amount >= 500 ? 10f : 5f)));
            s.clanFavor = Mathf.Min(100f, s.clanFavor + (amount >= 1000 ? 8f : 4f));
            s.clanSuccessionScore += (amount >= 1000 ? 6 : 2);

            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                $"<color=#00FFAA>[REMITTANCE DISPATCHED]</color> You sent <b>${amount:N0}</b> to your parents. " +
                $"They were deeply moved by your filial devotion. (Relationship +{(amount >= 1000 ? 15 : (amount >= 500 ? 10 : 5))}%, Clan Favor +{(amount >= 1000 ? 8 : 4)}%).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        private void OnBuyWarBondClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.cashBalance < 500)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Imperial War Bonds require a minimum investment of $500.");
                return;
            }

            s.cashBalance -= 500;
            s.meritRating = Mathf.Min(100f, s.meritRating + 8f);
            s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 5f);
            s.happiness = Mathf.Min(100f, s.happiness + 5f);

            GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                "<color=#FFD700>[WAR BOND PURCHASED]</color> You invested <b>$500</b> into Imperial Victory War Bonds! " +
                "The Imperial High Command recognized your patriotic support. (+8 Merit, +5 Political Influence).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        private void OnEmergencyAdvanceClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.isYouthPhase)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    "<color=#FF8844>[LOAN REJECTED]</color> Cadets are not eligible for military payroll advances.");
                return;
            }

            s.cashBalance += 300;
            s.meritRating = Mathf.Max(0f, s.meritRating - 4f);

            GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                "<color=#00D4FF>[PAY ADVANCE APPROVED]</color> You received an emergency advance of <b>$300</b> from the battalion paymaster. " +
                "Your merit rating slipped slightly (-4 Merit).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (titleText != null) titleText.color = p.textPrimary;
            if (cashBalanceText != null) cashBalanceText.color = p.textPrimary;
            if (cmpBalanceText != null) cmpBalanceText.color = p.textPrimary;
            if (monthlyBreakdownText != null) monthlyBreakdownText.color = p.textSecondary;
            if (familyWealthStatusText != null) familyWealthStatusText.color = p.textSecondary;
            if (warBondStatusText != null) warBondStatusText.color = p.textSecondary;
        }
    }
}
