using System;
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

        [Header("Garrison Living Quarters & Dining Plan")]
        public Button changeHousingButton;
        public Button changeDiningButton;

        [Header("Expanded Three-Phase Economy Actions (Optional Inspector Bindings)")]
        public Button buyTradeGuildShareButton;
        public Button payDebt1000Button;
        public Button treatSquadButton;
        public Button specialistHospitalButton;
        public TextMeshProUGUI portfolioBreakdownText;
        public TextMeshProUGUI spendingBreakdownText;

        // Dynamic fallback buttons
        private Button _runtimeBuyTradeShareBtn;
        private Button _runtimePayDebtBtn;
        private Button _runtimeTreatSquadBtn;
        private Button _runtimeHospitalBtn;
        private Button _runtimeChangeHousingBtn;
        private Button _runtimeChangeDiningBtn;
        private TextMeshProUGUI _runtimeHousingBtnText;
        private TextMeshProUGUI _runtimeDiningBtnText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null && modalRoot != gameObject) modalRoot.SetActive(false);
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

            if (buyTradeGuildShareButton != null) buyTradeGuildShareButton.onClick.AddListener(OnBuyTradeGuildShareClicked);
            if (payDebt1000Button != null) payDebt1000Button.onClick.AddListener(OnPayDebt1000Clicked);
            if (treatSquadButton != null) treatSquadButton.onClick.AddListener(OnTreatSquadClicked);
            if (specialistHospitalButton != null) specialistHospitalButton.onClick.AddListener(OnSpecialistHospitalClicked);
            if (changeHousingButton != null) changeHousingButton.onClick.AddListener(OnCycleHousingClicked);
            if (changeDiningButton != null) changeDiningButton.onClick.AddListener(OnCycleDiningClicked);

            EnsureDynamicActionButtons();

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

            if (buyTradeGuildShareButton != null) buyTradeGuildShareButton.onClick.RemoveListener(OnBuyTradeGuildShareClicked);
            if (payDebt1000Button != null) payDebt1000Button.onClick.RemoveListener(OnPayDebt1000Clicked);
            if (treatSquadButton != null) treatSquadButton.onClick.RemoveListener(OnTreatSquadClicked);
            if (specialistHospitalButton != null) specialistHospitalButton.onClick.RemoveListener(OnSpecialistHospitalClicked);
            if (changeHousingButton != null) changeHousingButton.onClick.RemoveListener(OnCycleHousingClicked);
            if (changeDiningButton != null) changeDiningButton.onClick.RemoveListener(OnCycleDiningClicked);

            if (_runtimeBuyTradeShareBtn != null) _runtimeBuyTradeShareBtn.onClick.RemoveListener(OnBuyTradeGuildShareClicked);
            if (_runtimePayDebtBtn != null) _runtimePayDebtBtn.onClick.RemoveListener(OnPayDebt1000Clicked);
            if (_runtimeTreatSquadBtn != null) _runtimeTreatSquadBtn.onClick.RemoveListener(OnTreatSquadClicked);
            if (_runtimeHospitalBtn != null) _runtimeHospitalBtn.onClick.RemoveListener(OnSpecialistHospitalClicked);
            if (_runtimeChangeHousingBtn != null) _runtimeChangeHousingBtn.onClick.RemoveListener(OnCycleHousingClicked);
            if (_runtimeChangeDiningBtn != null) _runtimeChangeDiningBtn.onClick.RemoveListener(OnCycleDiningClicked);

            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void OpenModal()
        {
            AudioManager.PlayUiClick();
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

            // Housing Upkeep
            long housingPerSemester = EconomyData.GetHousingUpkeepPerSemester(s.housingTier);
            long housingPerMonth = EconomyData.GetHousingUpkeepPerMonth(s.housingTier);
            string housingName = EconomyData.GetHousingName(s.housingTier);

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
                        "<b><color=#00D4FF>PHASE: DEPENDENT YOUTH & CADET</color></b>\n" +
                        $"<b>Housing / Boarding:</b> {housingName} (Free Military Dependent Quarters)\n" +
                        "<b>Monthly Allowance:</b> $0 / mo (Academic Boarding & Mess Hall rations provided)\n" +
                        "<i>Cadets receive free academy meals, physical training gear, and academic instruction, but no military salary until official enlistment or graduation at Age 18.0.</i>";
                }
                else if (s.careerTrack == CareerTrack.CivilianCollege)
                {
                    long debt = s.collegeTuitionDebt;
                    monthlyBreakdownText.text = 
                        "<b><color=#FFD700>PHASE: IMPERIAL COLLEGE UNDERGRADUATE</color></b>\n" +
                        "<b>Academic Track:</b> Full-Time Degree Student ($0 / mo wage)\n" +
                        "<b>Semester Tuition Cost:</b> $12,500 / semester ($100,000 for 4-Year / 8-Semester Degree)\n" +
                        $"<b>Housing Residence:</b> {housingName} (-${housingPerMonth:N0}/mo | -${housingPerSemester:N0}/sem upkeep)\n" +
                        $"<color={(debt > 0 ? "#FF6666" : "#00FFAA")}><b>Outstanding Student Loan Balance:</b> ${debt:N0}</color>\n" +
                        "<i>Degrees qualify graduates for prestigious civilian guild positions starting at $500+/mo upon graduation.</i>";
                }
                else if (s.isCivilian)
                {
                    long gross = s.civilianSalaryPerMonth > 0 ? s.civilianSalaryPerMonth : 150L;
                    long tax = (long)(gross * SalaryCalculator.CivilMunicipalTaxRate);
                    long netBeforeHousing = gross - tax;
                    long netTakeHome = Math.Max(0L, netBeforeHousing - housingPerMonth);
                    long semesterTakeHome = netTakeHome * 6;

                    string debtTag = s.collegeTuitionDebt > 0 
                        ? $"\n<color=#FF6666><b>Student Tuition Debt Remaining:</b> ${s.collegeTuitionDebt:N0}</color>" 
                        : "";

                    monthlyBreakdownText.text = 
                        $"<b><color=#00FFAA>PHASE: CIVILIAN CITIZEN • {s.civilianJobTitle.ToUpper()}</color></b>\n" +
                        $"<b>Gross Monthly Wage:</b> ${gross:N0} / mo (${gross * 6:N0} / sem)\n" +
                        $"<color=#FF8866><b>Municipal Civil Income Tax (50%):</b> -${tax:N0} / mo (-${tax * 6:N0} / sem)</color>\n" +
                        $"<color=#FFAA44><b>Housing Rent & Upkeep ({housingName}):</b> -${housingPerMonth:N0} / mo (-${housingPerSemester:N0} / sem)</color>\n" +
                        $"<color=#00FF88><b>Net Monthly Take-Home Pay:</b> +${netTakeHome:N0} / mo</color>\n" +
                        $"<color=#00FFAA><b>Estimated Semester Net Cashflow:</b> +${semesterTakeHome:N0} / sem</color>{debtTag}\n" +
                        "<i>Civilian municipal law requires a 50% municipal tax towards city infrastructure, security watch, and public maintenance.</i>";
                }
                else if (s.careerTrack == CareerTrack.MilitaryRecruitBasic)
                {
                    monthlyBreakdownText.text = 
                        "<b><color=#FF8844>PHASE: BASIC COMBAT TRAINING RECRUIT</color></b>\n" +
                        "<b>Compensation:</b> $0 / mo (Unpaid Basic Drill Term)\n" +
                        $"<b>Housing:</b> {housingName} (Military Depot Barracks)\n" +
                        "<i>Recruits receive standard field rations, bunking, and uniform gear. Active salary commences upon passing out as Private (E-1).</i>";
                }
                else if (s.careerTrack == CareerTrack.MilitaryTechSchool || s.careerTrack == CareerTrack.MilitaryMedicalSchool || s.careerTrack == CareerTrack.MilitaryWarAcademy)
                {
                    long pay = SalaryCalculator.CalculateGrossMonthlyPay(s);
                    string payNote = pay > 0 ? $"+${pay:N0} / mo (${pay * 6:N0} / sem, Tax-Exempt Cadet Stipend)" : "$0 / mo (Cadet Trainee)";
                    string loanLine = s.academyTuitionDebt > 0 
                        ? $"\n<color=#FF8866><b>War Academy Tuition Debt:</b> ${s.academyTuitionDebt:N0} (9% Annual Int | {s.academyLoanDeductionPercent * 100f:F0}% deduction rate)</color>" 
                        : "";

                    monthlyBreakdownText.text = 
                        $"<b><color=#00D4FF>PHASE: MILITARY ACADEMY CADET • {s.assignedDepartment.ToUpper()}</color></b>\n" +
                        $"<b>Cadet Monthly Stipend:</b> {payNote}\n" +
                        $"<b>Housing Quarters:</b> {housingName} (Cadet Hall)\n" +
                        $"<color=#FFAA44><b>Quarters Upkeep:</b> -${housingPerMonth:N0} / mo (-${housingPerSemester:N0} / sem)</color>{loanLine}\n" +
                        "<i>Cadet educational stipends are fully exempt from the 40% Imperial War Tax during academy schooling.</i>";
                }
                else if (s.isRetired)
                {
                    long pension = s.calculatedPensionPerMonth > 0 ? s.calculatedPensionPerMonth : 1500L;
                    long careerPay = 0L;
                    string careerTitle = "Honorable Military Veteran";

                    switch (s.veteranCareer)
                    {
                        case VeteranCareer.UniversityGI: careerPay = 500L; careerTitle = "GI Bill University Scholar"; break;
                        case VeteranCareer.DefenseContractor: careerPay = 2500L; careerTitle = "Defense Logistics Contractor"; break;
                        case VeteranCareer.CityMayor: careerPay = 4000L; careerTitle = "Municipal City Mayor"; break;
                        case VeteranCareer.RegionalGovernor: careerPay = 8000L; careerTitle = "Provincial Governor"; break;
                        case VeteranCareer.MinisterOfDefense: careerPay = 18000L; careerTitle = "Minister of Defense (Cabinet Post)"; break;
                    }

                    long disability = s.vaDisabilityAllowancePerMonth;
                    long totalMonthly = pension + careerPay + disability;
                    long netMonthly = Math.Max(0L, totalMonthly - housingPerMonth);
                    long semesterNet = netMonthly * 6;

                    string vaLine = disability > 0 
                        ? $"\n<color=#FFD700><b>VA Frontline Combat Disability Stipend:</b> +${disability:N0} / mo (+${disability * 6:N0} / sem, Tax-Exempt)</color>" 
                        : "";

                    string careerLine = careerPay > 0 
                        ? $"\n<b>Veteran Career Wage ({careerTitle}):</b> +${careerPay:N0} / mo (+${careerPay * 6:N0} / sem)" 
                        : "";

                    monthlyBreakdownText.text = 
                        $"<b><color=#FFD700>PHASE: HONORABLE RETIRED VETERAN • {careerTitle.ToUpper()}</color></b>\n" +
                        $"<b>Lifetime Imperial Veteran Pension:</b> +${pension:N0} / mo (+${pension * 6:N0} / sem){careerLine}{vaLine}\n" +
                        $"<color=#FFAA44><b>Personal Residence ({housingName}):</b> -${housingPerMonth:N0} / mo (-${housingPerSemester:N0} / sem)</color>\n" +
                        $"<color=#00FF88><b>Net Monthly Cashflow:</b> +${netMonthly:N0} / mo</color>\n" +
                        $"<color=#00FFAA><b>Estimated Semester Net Cashflow:</b> +${semesterNet:N0} / sem</color>\n" +
                        "<i>Imperial military pensions and VA disability allowances are fully exempt from wartime taxation.</i>";
                }
                else
                {
                    // Active Military Service
                    long gross = SalaryCalculator.CalculateGrossMonthlyPay(s);
                    long net = SalaryCalculator.CalculateNetMonthlyPay(s);
                    long tax = SalaryCalculator.CalculateMonthlyTax(s);
                    long loanDeduction = SalaryCalculator.CalculateMonthlyLoanDeduction(s);
                    long disability = s.vaDisabilityAllowancePerMonth;

                    long monthlyRent = s.isInCombatDeployment ? 0L : s.GetMonthlyRent();
                    long monthlyFood = s.isInCombatDeployment ? 0L : s.GetMonthlyFood();
                    long livingExpensesMonth = monthlyRent + monthlyFood;
                    long monthlyNetWithHousing = Math.Max(0L, net + disability - livingExpensesMonth);
                    long semesterNetCashflow = monthlyNetWithHousing * 6;

                    string rankName = s.currentRank != null ? s.currentRank.rankName : "Private";
                    string sfLine = s.isSpecialForces ? "\n<color=#FFD700><b>Special Forces Hazard Stipend:</b> +$800 / mo included</color>" : "";
                    string extTourLine = s.hasExtendedTourPayBonus ? "\n<color=#00FFAA><b>Continuous Combat Veteran Bonus:</b> +10% Total Pay included</color>" : "";
                    string tourBonus = s.isInCombatDeployment ? " (+$450 Hazard Pay Active)" : "";
                    string vaLine = disability > 0 ? $"\n<color=#FFD700><b>VA Combat Disability Stipend:</b> +${disability:N0} / mo included (Tax-Exempt)</color>" : "";

                    string loanLine = loanDeduction > 0 
                        ? $"\n<color=#FF8866><b>War Academy Loan Repayment:</b> -${loanDeduction:N0} / mo (-${loanDeduction * 6:N0} / sem) [Debt: ${s.academyTuitionDebt:N0} @ 9% Int]</color>" 
                        : "";

                    string livingCostLine = s.isInCombatDeployment
                        ? "<color=#00FFAA><b>Living Expenses (FOB Quarters & Combat Rations):</b> $0 / mo (100% Subsidized)</color>\n"
                        : $"<color=#FFAA44><b>Garrison Living Expenses (Rent: ${monthlyRent:N0} | Food: ${monthlyFood:N0}):</b> -${livingExpensesMonth:N0} / mo (-${livingExpensesMonth * 6:N0} / sem)</color>\n";

                    monthlyBreakdownText.text = 
                        $"<b><color=#00FFAA>PHASE: ACTIVE DUTY MILITARY • {rankName.ToUpper()}</color></b>\n" +
                        $"<b>Gross Monthly Compensation:</b> ${gross:N0} / mo (${gross * 6:N0} / sem){tourBonus}{sfLine}{extTourLine}{vaLine}\n" +
                        $"<color=#FF6666><b>Mandatory Imperial War Tax (40%):</b> -${tax:N0} / mo (-${tax * 6:N0} / sem)</color>{loanLine}\n" +
                        $"{livingCostLine}" +
                        $"<color=#00FF88><b>Net Monthly Take-Home Pay:</b> +${monthlyNetWithHousing:N0} / mo</color>\n" +
                        $"<color=#00FFAA><b>Estimated Semester Net Cashflow:</b> +${semesterNetCashflow:N0} / sem</color>\n" +
                        "<i>Imperial Decree: 40% of military compensation is deducted for logistics and wartime ammunition reserves.</i>";
                }

                if (WartimeMarketEngine.CurrentState.activeShock != MacroEconomicShock.NormalStagnant)
                {
                    string marketBanner = $"<color=#FF8844><b>[MARKET: {WartimeMarketEngine.CurrentState.headlineText.ToUpper()}]</b></color>\n" +
                                           $"<i>{WartimeMarketEngine.CurrentState.descriptiveNarrative} ({WartimeMarketEngine.CurrentState.shockDurationRemaining} sem remaining)</i>\n\n";
                    monthlyBreakdownText.text = marketBanner + monthlyBreakdownText.text;
                }
            }

            // Investment & Business Portfolio
            long bondDividends = EconomyData.GetWarBondDividendPerSemester(s.warBondsOwned);
            long shareDividends = EconomyData.GetTradeGuildDividendPerSemester(s.tradeGuildShares);
            long totalDividends = bondDividends + shareDividends;

            long bizRevenue = s.businessRevenueLastSemester;
            if (bizRevenue <= 0 && s.ownedBusiness != CommercialBusiness.None)
            {
                bizRevenue = EconomyData.GetBusinessBaseSemesterYield(s.ownedBusiness);
            }

            long totalPassiveIncome = totalDividends + (s.ownedBusiness != CommercialBusiness.None ? bizRevenue : 0L);

            if (warBondStatusText != null)
            {
                string bizStatus = s.ownedBusiness != CommercialBusiness.None
                    ? $"<b>Enterprise Owned:</b> {EconomyData.GetBusinessName(s.ownedBusiness)} (Last Sem Revenue: <color=#00FFAA>+${bizRevenue:N0}</color>)\n"
                    : "<b>Commercial Enterprise:</b> None owned (Workshops, Taverns, and Caravans available in Activities)\n";

                string debtSummary = "";
                if (s.academyTuitionDebt > 0 || s.collegeTuitionDebt > 0)
                {
                    debtSummary = $"<b>Active Liabilities:</b> War Academy Debt: <color=#FF6666>${s.academyTuitionDebt:N0}</color> (9% Int) | College Debt: <color=#FF6666>${s.collegeTuitionDebt:N0}</color>\n";
                }

                warBondStatusText.text = 
                    "<b><color=#00D4FF>INVESTMENT & COMMERCIAL PORTFOLIO:</color></b>\n" +
                    $"<b>Imperial War Bonds:</b> {s.warBondsOwned} owned (${s.warBondsOwned * 500:N0} invested) | Yield: <color=#00FFAA>+${bondDividends:N0} / sem</color> (~7% APY)\n" +
                    $"<b>Trade Guild Shares:</b> {s.tradeGuildShares} shares (${s.tradeGuildShares * 1000:N0} invested) | Yield: <color=#00FFAA>+${shareDividends:N0} / sem</color> (~9% APY)\n" +
                    $"{bizStatus}" +
                    $"{debtSummary}" +
                    $"<color=#FFD700><b>Total Semi-Annual Portfolio Return:</b> +${totalPassiveIncome:N0} / semester</color>";
            }

            // Family Status & Living Conditions
            if (familyWealthStatusText != null)
            {
                string familyHouseStr;
                switch (s.familyHouseTier)
                {
                    case FamilyHouseTier.AncestralEstate:
                        familyHouseStr = "Spacious Suburban Residence & Estate";
                        break;
                    case FamilyHouseTier.CityTownhouse:
                        familyHouseStr = "Capital City Townhouse (High Working Class)";
                        break;
                    case FamilyHouseTier.RuralHomestead:
                        familyHouseStr = "Fertile Rural Family Homestead";
                        break;
                    default:
                        if (s.origin == SoldierOrigin.MilitaryLegacy)
                            familyHouseStr = "Historic Officer Family Residence";
                        else if (s.totalRemittancesSentToFamily >= 3000)
                            familyHouseStr = "Comfortable Townhouse (Lifted by Remittances)";
                        else if (s.totalRemittancesSentToFamily >= 1000)
                            familyHouseStr = "Modest Working-Class Tenement";
                        else
                            familyHouseStr = "Impoverished Wartime Tenement Slums";
                        break;
                }

                string heirTutoringStr = s.isHeirTutoringEnrolled 
                    ? "<color=#00FFAA>Enrolled in Private Elite Tutoring ($1,000/sem)</color>" 
                    : "<color=#94A3B8>Public Military Cadre</color>";

                string bionicStr = s.hasBionicProsthetics 
                    ? " | Bionics: <color=#00D4FF>Equipped</color>" 
                    : "";

                familyWealthStatusText.text = 
                    $"<b>Family Household:</b> {s.lastName} Family   |   <b>Living Standard:</b> {s.familyLivingCondition:F0}/100\n" +
                    $"<b>Family Real Estate:</b> {familyHouseStr}\n" +
                    $"<b>Total Remittances Sent:</b> ${s.totalRemittancesSentToFamily:N0}   |   <b>Parent Approval:</b> {s.parentRelationship:F0}%   |   <b>Family Favor:</b> {s.clanFavor:F0}%\n" +
                    $"<b>Generational Heir Tutoring:</b> {heirTutoringStr}{bionicStr}\n" +
                    "<i>Sending money home elevates family living standards and strengthens parental bonds.</i>";
            }

            // Garrison Living Quarters & Dining Plan Breakdown
            if (spendingBreakdownText != null)
            {
                if (s.isInCombatDeployment)
                {
                    spendingBreakdownText.text = 
                        "<b><color=#00D4FF>[ACTIVE COMBAT DEPLOYMENT • LIVING SUBSIDY]</color></b>\n" +
                        "<b>Theater Quarters:</b> Forward Operating Base (FOB) Quarters (<color=#00FFAA>$0 / mo • 100% Armed Forces Subsidized</color>)\n" +
                        "<b>Combat Dining:</b> Tactical Field Combat Rations (<color=#00FFAA>$0 / mo • 100% Armed Forces Subsidized</color>)\n" +
                        "<i>While deployed in combat operations, all housing, lodging, and nutrition are fully subsidized by the military.</i>";
                }
                else
                {
                    long rentMo = s.GetMonthlyRent();
                    long rentSem = rentMo * 6L;
                    long foodMo = s.GetMonthlyFood();
                    long foodSem = foodMo * 6L;
                    long totalLivingMo = rentMo + foodMo;
                    long totalLivingSem = totalLivingMo * 6L;

                    string housingTitle = EconomyData.GetHousingDisplayName(s.housingTier, s.militaryBranch);
                    string housingDesc = EconomyData.GetHousingDescription(s.housingTier, s.militaryBranch);
                    string diningTitle = EconomyData.GetDiningDisplayName(s.garrisonDiningTier);
                    string diningDesc = EconomyData.GetDiningDescription(s.garrisonDiningTier);

                    spendingBreakdownText.text = 
                        "<b><color=#FFD700>[GARRISON LIVING EXPENSES & LIFESTYLE]</color></b>\n" +
                        $"<b>Living Quarters:</b> {housingTitle} (-${rentMo:N0}/mo | -${rentSem:N0}/sem)\n" +
                        $"<size=17><i>{housingDesc}</i></size>\n" +
                        $"<b>Dining Plan:</b> {diningTitle} (-${foodMo:N0}/mo | -${foodSem:N0}/sem)\n" +
                        $"<size=17><i>{diningDesc}</i></size>\n" +
                        $"<color=#FFAA44><b>Total Monthly Living Expenses:</b> -${totalLivingMo:N0} / mo (-${totalLivingSem:N0} / semester)</color>\n" +
                        "<i>Click buttons below to upgrade/downgrade quarters (rank permitting) or alter meal plan.</i>";
                }
            }

            if (_runtimeHousingBtnText != null)
            {
                _runtimeHousingBtnText.text = $"QUARTERS: {EconomyData.GetHousingDisplayName(s.housingTier, s.militaryBranch)}";
            }
            if (_runtimeDiningBtnText != null)
            {
                _runtimeDiningBtnText.text = $"DINING: {EconomyData.GetDiningDisplayName(s.garrisonDiningTier)}";
            }

            // Apply Theme Palette
            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private void EnsureDynamicActionButtons()
        {
            // If buyTradeGuildShareButton is null and buyWarBondButton exists, create one beside/below it
            if (buyTradeGuildShareButton == null && _runtimeBuyTradeShareBtn == null && buyWarBondButton != null)
            {
                var parent = buyWarBondButton.transform.parent;
                var btnObj = new GameObject("Runtime_BuyTradeGuildShareBtn");
                btnObj.transform.SetParent(parent, false);
                btnObj.transform.SetSiblingIndex(buyWarBondButton.transform.GetSiblingIndex() + 1);

                var img = btnObj.AddComponent<Image>();
                img.color = new Color(0.20f, 0.50f, 0.70f);

                _runtimeBuyTradeShareBtn = btnObj.AddComponent<Button>();
                _runtimeBuyTradeShareBtn.targetGraphic = img;
                _runtimeBuyTradeShareBtn.onClick.AddListener(OnBuyTradeGuildShareClicked);

                var le = btnObj.AddComponent<LayoutElement>();
                le.preferredHeight = 52;
                le.minHeight = 48;

                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                var tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "BUY $1,000 TRADE GUILD SHARE (+9% Div & Influence)";
                tmp.fontSize = 20;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;

                var rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // If pay debt button is missing and war bonds section exists, create a quick debt repayment button if in debt
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null && (s.academyTuitionDebt > 0 || s.collegeTuitionDebt > 0))
            {
                if (payDebt1000Button == null && _runtimePayDebtBtn == null && emergencyAdvanceButton != null)
                {
                    var parent = emergencyAdvanceButton.transform.parent;
                    var btnObj = new GameObject("Runtime_PayDebt1000Btn");
                    btnObj.transform.SetParent(parent, false);
                    btnObj.transform.SetSiblingIndex(emergencyAdvanceButton.transform.GetSiblingIndex() + 1);

                    var img = btnObj.AddComponent<Image>();
                    img.color = new Color(0.65f, 0.35f, 0.20f);

                    _runtimePayDebtBtn = btnObj.AddComponent<Button>();
                    _runtimePayDebtBtn.targetGraphic = img;
                    _runtimePayDebtBtn.onClick.AddListener(OnPayDebt1000Clicked);

                    var le = btnObj.AddComponent<LayoutElement>();
                    le.preferredHeight = 52;
                    le.minHeight = 48;

                    var textObj = new GameObject("Text");
                    textObj.transform.SetParent(btnObj.transform, false);
                    var tmp = textObj.AddComponent<TextMeshProUGUI>();
                    tmp.text = "PAY $1,000 TOWARD DEBT PRINCIPAL";
                    tmp.fontSize = 20;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;

                    var rt = textObj.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
            }

            // Housing Selection Button (Barracks $200/mo up to Command Estate $5,000/mo)
            if (changeHousingButton == null && _runtimeChangeHousingBtn == null && buyWarBondButton != null)
            {
                var parent = buyWarBondButton.transform.parent;
                var btnObj = new GameObject("Runtime_ChangeHousingBtn");
                btnObj.transform.SetParent(parent, false);

                var img = btnObj.AddComponent<Image>();
                img.color = new Color(0.18f, 0.40f, 0.55f);

                _runtimeChangeHousingBtn = btnObj.AddComponent<Button>();
                _runtimeChangeHousingBtn.targetGraphic = img;
                _runtimeChangeHousingBtn.onClick.AddListener(OnCycleHousingClicked);

                var le = btnObj.AddComponent<LayoutElement>();
                le.preferredHeight = 52;
                le.minHeight = 48;

                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                _runtimeHousingBtnText = textObj.AddComponent<TextMeshProUGUI>();
                _runtimeHousingBtnText.text = "CHANGE LIVING QUARTERS ($200 - $5,000/mo)";
                _runtimeHousingBtnText.fontSize = 19;
                _runtimeHousingBtnText.fontStyle = FontStyles.Bold;
                _runtimeHousingBtnText.alignment = TextAlignmentOptions.Center;
                _runtimeHousingBtnText.color = Color.white;

                var rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Dining Plan Selection Button (Mess Hall $100/mo up to Executive Chef $2,000/mo)
            if (changeDiningButton == null && _runtimeChangeDiningBtn == null && buyWarBondButton != null)
            {
                var parent = buyWarBondButton.transform.parent;
                var btnObj = new GameObject("Runtime_ChangeDiningBtn");
                btnObj.transform.SetParent(parent, false);

                var img = btnObj.AddComponent<Image>();
                img.color = new Color(0.22f, 0.48f, 0.32f);

                _runtimeChangeDiningBtn = btnObj.AddComponent<Button>();
                _runtimeChangeDiningBtn.targetGraphic = img;
                _runtimeChangeDiningBtn.onClick.AddListener(OnCycleDiningClicked);

                var le = btnObj.AddComponent<LayoutElement>();
                le.preferredHeight = 52;
                le.minHeight = 48;

                var textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                _runtimeDiningBtnText = textObj.AddComponent<TextMeshProUGUI>();
                _runtimeDiningBtnText.text = "CHANGE GARRISON DINING PLAN ($100 - $2,000/mo)";
                _runtimeDiningBtnText.fontSize = 19;
                _runtimeDiningBtnText.fontStyle = FontStyles.Bold;
                _runtimeDiningBtnText.alignment = TextAlignmentOptions.Center;
                _runtimeDiningBtnText.color = Color.white;

                var rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        public void OnCycleHousingClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.isInCombatDeployment)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    "<color=#FF8844>[HOUSING LOCKED]</color> You are actively deployed in combat operations. Quarters are assigned by theater command ($0/mo).");
                return;
            }

            int current = (int)s.housingTier;
            bool foundNext = false;
            for (int offset = 1; offset < 5; offset++)
            {
                HousingTier candidate = (HousingTier)((current + offset) % 5);
                if (s.IsRankEligibleForHousing(candidate))
                {
                    s.housingTier = candidate;
                    foundNext = true;
                    break;
                }
            }

            if (!foundNext)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    $"<color=#FFAA44>[HOUSING LIMIT]</color> Your current military rank only qualifies for <b>{EconomyData.GetHousingDisplayName(s.housingTier, s.militaryBranch)}</b>. Earn promotion to unlock higher officer quarters!");
            }
            else
            {
                AudioManager.PlayUiClick();
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    $"<color=#00FFAA>[LIVING QUARTERS UPDATED]</color> You moved into <b>{EconomyData.GetHousingDisplayName(s.housingTier, s.militaryBranch)}</b> (Rent: ${s.GetMonthlyRent():N0}/mo | ${s.GetMonthlyRent() * 6:N0}/semester).");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        public void OnCycleDiningClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.isInCombatDeployment)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    "<color=#FF8844>[DINING LOCKED]</color> You are actively deployed in combat operations. Field combat rations are fully subsidized by theater command ($0/mo).");
                return;
            }

            int current = (int)s.garrisonDiningTier;
            s.garrisonDiningTier = (DiningPlanTier)((current + 1) % 5);

            AudioManager.PlayUiClick();
            GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                $"<color=#00FFAA>[DINING PLAN UPDATED]</color> You switched to <b>{EconomyData.GetDiningDisplayName(s.garrisonDiningTier)}</b> (Expense: ${s.GetMonthlyFood():N0}/mo | ${s.GetMonthlyFood() * 6:N0}/semester). {EconomyData.GetDiningDescription(s.garrisonDiningTier)}");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
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
            s.familyLivingCondition = Mathf.Min(100f, s.familyLivingCondition + (amount >= 1000 ? 10f : 4f));
            s.parentRelationship = Mathf.Min(100f, s.parentRelationship + (amount >= 1000 ? 15f : (amount >= 500 ? 10f : 5f)));
            s.clanFavor = Mathf.Min(100f, s.clanFavor + (amount >= 1000 ? 8f : 4f));
            s.clanSuccessionScore += (amount >= 1000 ? 6 : 2);

            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                $"<color=#00FFAA>[REMITTANCE DISPATCHED]</color> You sent <b>${amount:N0}</b> to your parents. " +
                $"They were deeply moved by your filial devotion. (Relationship +{(amount >= 1000 ? 15 : (amount >= 500 ? 10 : 5))}%, Clan Favor +{(amount >= 1000 ? 8 : 4)}%).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        public void OnBuyWarBondClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.cashBalance < EconomyData.WAR_BOND_COST)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Imperial War Bonds require a minimum investment of ${EconomyData.WAR_BOND_COST:N0}.");
                return;
            }

            s.cashBalance -= EconomyData.WAR_BOND_COST;
            s.warBondsOwned++;
            AudioManager.PlayPayday();
            s.meritRating = Mathf.Min(100f, s.meritRating + 8f);
            s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 5f);
            s.happiness = Mathf.Min(100f, s.happiness + 5f);

            GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                $"<color=#FFD700>[WAR BOND PURCHASED]</color> You invested <b>${EconomyData.WAR_BOND_COST:N0}</b> into Imperial Defense War Bonds! " +
                $"Total Owned: <b>{s.warBondsOwned}</b> (Yields +${s.warBondsOwned * 35:N0}/sem). Imperial Command noted your patriotism (+8 Merit, +5 Influence).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        public void OnBuyTradeGuildShareClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.cashBalance < EconomyData.TRADE_GUILD_SHARE_COST)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Trade Guild Shares require ${EconomyData.TRADE_GUILD_SHARE_COST:N0} in liquid cash.");
                return;
            }

            s.cashBalance -= EconomyData.TRADE_GUILD_SHARE_COST;
            s.tradeGuildShares++;
            AudioManager.PlayPayday();
            s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 8f);
            s.happiness = Mathf.Min(100f, s.happiness + 4f);

            GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                $"<color=#00D4FF>[TRADE GUILD SHARE ACQUIRED]</color> You purchased 1 Commercial Guild Share for <b>${EconomyData.TRADE_GUILD_SHARE_COST:N0}</b>! " +
                $"Total Shares: <b>{s.tradeGuildShares}</b> (Yields +${s.tradeGuildShares * 90:N0}/sem dividend). Commercial influence rose (+8 Influence).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        public void OnEmergencyAdvanceClicked()
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

        public void OnPayDebt1000Clicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.cashBalance < 1000)
            {
                GameEvents.OnFeedEventLogged?.Invoke("DEBT",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> You need $1,000 cash for extra principal repayment.");
                return;
            }

            if (s.academyTuitionDebt > 0)
            {
                long pay = Math.Min(1000L, s.academyTuitionDebt);
                s.cashBalance -= pay;
                s.academyTuitionDebt -= pay;
                AudioManager.PlayPayday();
                GameEvents.OnFeedEventLogged?.Invoke("DEBT",
                    $"<color=#00FFAA>[PRINCIPAL REPAID]</color> You paid <b>${pay:N0}</b> towards your War Academy loan! Remaining debt: <b>${s.academyTuitionDebt:N0}</b>.");
            }
            else if (s.collegeTuitionDebt > 0)
            {
                long pay = Math.Min(1000L, s.collegeTuitionDebt);
                s.cashBalance -= pay;
                s.collegeTuitionDebt -= pay;
                AudioManager.PlayPayday();
                GameEvents.OnFeedEventLogged?.Invoke("DEBT",
                    $"<color=#00FFAA>[PRINCIPAL REPAID]</color> You paid <b>${pay:N0}</b> towards your Imperial College student debt! Remaining debt: <b>${s.collegeTuitionDebt:N0}</b>.");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("DEBT",
                    "<color=#00FFAA>[DEBT-FREE]</color> You have no outstanding loan liabilities!");
                return;
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        public void OnTreatSquadClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.cashBalance < EconomyData.SQUAD_MESS_DRINKS_COST)
            {
                GameEvents.OnFeedEventLogged?.Invoke("MESS",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Treating the fireteam costs ${EconomyData.SQUAD_MESS_DRINKS_COST:N0}.");
                return;
            }

            s.cashBalance -= EconomyData.SQUAD_MESS_DRINKS_COST;
            s.squadRespect = Mathf.Min(100f, s.squadRespect + 20f);
            s.happiness = Mathf.Min(100f, s.happiness + 10f);
            AudioManager.PlayPayday();

            if (s.squadMates != null)
            {
                foreach (var mate in s.squadMates)
                {
                    if (mate != null && !mate.isDead)
                    {
                        mate.relationship = Mathf.Min(100f, mate.relationship + 15f);
                    }
                }
            }

            GameEvents.OnFeedEventLogged?.Invoke("MESS",
                $"<color=#00FFAA>[SQUAD DRINKS & FEAST]</color> You treated your fireteam brothers at the mess hall (-${EconomyData.SQUAD_MESS_DRINKS_COST})! " +
                "The soldiers raised a toast to your loyalty (+20 Squad Respect, +15 Comradeship).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
        }

        public void OnSpecialistHospitalClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (s.cashBalance < EconomyData.SPECIALIST_HOSPITAL_CARE_COST)
            {
                GameEvents.OnFeedEventLogged?.Invoke("MEDICAL",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Specialist trauma surgery costs ${EconomyData.SPECIALIST_HOSPITAL_CARE_COST:N0}.");
                return;
            }

            s.cashBalance -= EconomyData.SPECIALIST_HOSPITAL_CARE_COST;
            s.health = Mathf.Min(100f, s.health + 50f);
            s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 30f);

            GameEvents.OnFeedEventLogged?.Invoke("MEDICAL",
                $"<color=#00FFAA>[SPECIALIST CARE]</color> Military trauma surgeons repaired combat injuries and physical exhaustion (+30 Readiness, +50 Health).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            RefreshUI();
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
            if (cashBalanceText != null) cashBalanceText.color = p.accentEmerald;
            if (cmpBalanceText != null) cmpBalanceText.color = p.accentGold;
            if (monthlyBreakdownText != null) monthlyBreakdownText.color = p.textSecondary;
            if (familyWealthStatusText != null) familyWealthStatusText.color = p.textSecondary;
            if (warBondStatusText != null) warBondStatusText.color = p.textSecondary;
            if (portfolioBreakdownText != null) portfolioBreakdownText.color = p.textSecondary;
            if (spendingBreakdownText != null) spendingBreakdownText.color = p.textSecondary;

            StyleFinanceButton(closeButton, p);
            StyleFinanceButton(send200Button, p);
            StyleFinanceButton(send500Button, p);
            StyleFinanceButton(send1000Button, p);
            StyleFinanceButton(buyWarBondButton, p);
            StyleFinanceButton(emergencyAdvanceButton, p);
            StyleFinanceButton(buyTradeGuildShareButton, p);
            StyleFinanceButton(payDebt1000Button, p);
            StyleFinanceButton(treatSquadButton, p);
            StyleFinanceButton(specialistHospitalButton, p);
            StyleFinanceButton(changeHousingButton, p);
            StyleFinanceButton(changeDiningButton, p);
            StyleFinanceButton(_runtimeChangeHousingBtn, p);
            StyleFinanceButton(_runtimeChangeDiningBtn, p);
        }

        private void StyleFinanceButton(Button btn, ThemePalette p)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = p.btnNormalBg;
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.color = p.btnNormalText;
        }
    }
}
