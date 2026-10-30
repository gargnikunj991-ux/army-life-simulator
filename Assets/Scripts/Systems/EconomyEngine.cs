using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Core Economy Engine driving the 3-phase simulation economy:
    /// 1. Military Life: Rank base pay, flight pay (+15%), cultivation stipends, hazard/SF pay, continuous tour bonuses, bounties/plunder, 40% War Tax, academy debt.
    /// 2. Civilian Life: Grassroots to college professional wages, 50% Municipal Tax, college debt amortization, housing tiers, commercial businesses.
    /// 3. Post-Military Veteran Life: Formula-based scaled lifetime pension, VA disability allowance, GI Bill, defense consulting, dynastic inheritance.
    /// </summary>
    public static class EconomyEngine
    {
        /// <summary>
        /// Calculates the formula-based scaled lifetime pension for an honorably retired soldier.
        /// Formula: (finalRankBaseMonthlyPay * (yearsOfService * 0.025f)) + (woundStarsCount * 300L).
        /// Minimum guaranteed pension is $1,500/month.
        /// </summary>
        public static long CalculateScaledPension(SoldierProfile soldier)
        {
            if (soldier == null) return EconomyData.MINIMUM_VETERAN_PENSION;

            long finalRankBaseMonthlyPay = 600L;
            if (soldier.currentRank != null)
            {
                finalRankBaseMonthlyPay = soldier.currentRank.baseMonthlySalary;
            }

            float yearsOfService = soldier.totalSemestersLived * 0.5f;
            int woundStars = soldier.GetWoundStarsCount();

            long basePension = (long)(finalRankBaseMonthlyPay * (yearsOfService * EconomyData.PENSION_RATE_PER_YEAR));
            long woundBonus = woundStars * EconomyData.VA_DISABILITY_PER_WOUND_STAR;
            long totalPension = basePension + woundBonus;

            return Math.Max(EconomyData.MINIMUM_VETERAN_PENSION, totalPension);
        }

        /// <summary>
        /// Calculates monthly Veterans Affairs (VA) disability compensation based on earned wound stars.
        /// </summary>
        public static long CalculateDisabilityAllowance(SoldierProfile soldier)
        {
            if (soldier == null) return 0L;
            return soldier.GetWoundStarsCount() * EconomyData.VA_DISABILITY_PER_WOUND_STAR;
        }

        /// <summary>
        /// Calculates the total housing/living costs for a soldier over a 6-month semester.
        /// </summary>
        public static long CalculateSemesterHousingCost(SoldierProfile soldier)
        {
            if (soldier == null) return 0L;
            return (soldier.careerTrack == CareerTrack.MilitaryRecruitBasic)
                ? 0L
                : WartimeMarketEngine.GetAdjustedHousingCost(soldier.housingTier, soldier.militaryBranch);
        }

        /// <summary>
        /// Primary turn-cycle financial calculation executed every 6-month semester.
        /// Processes income, taxation, debt interest/amortization, investments, businesses, combat plunder, and housing upkeep.
        /// </summary>
        public static void ProcessSemesterEconomy(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return;

            // Dependent Youth Students receive room & board but no salary
            if (soldier.isYouthPhase)
            {
                soldier.businessRevenueLastSemester = 0L;
                soldier.investmentDividendsLastSemester = 0L;
                return;
            }

            // Incarcerated convicts have frozen salaries
            if (soldier.isImprisoned || soldier.militaryBrigSemestersRemaining > 0)
            {
                soldier.businessRevenueLastSemester = 0L;
                soldier.investmentDividendsLastSemester = 0L;
                return;
            }

            // 1. Accrue loan interest
            AccrueLoanInterests(soldier);

            // 2. Compute Gross Income & Deductions for the 6-Month Semester
            long monthlyGross = SalaryCalculator.CalculateGrossMonthlyPay(soldier);
            long monthlyTax = SalaryCalculator.CalculateMonthlyTax(soldier);
            long monthlyLoanDeduction = SalaryCalculator.CalculateMonthlyLoanDeduction(soldier);

            long semesterGrossSalary = monthlyGross * 6L;
            long semesterTax = monthlyTax * 6L;
            long semesterAcademyLoan = monthlyLoanDeduction * 6L;

            if (semesterAcademyLoan > soldier.academyTuitionDebt)
            {
                semesterAcademyLoan = soldier.academyTuitionDebt;
            }
            soldier.academyTuitionDebt -= semesterAcademyLoan;

            // 3. College student debt amortization (if civilian or graduate)
            long semesterCollegeLoan = 0L;
            if (soldier.collegeTuitionDebt > 0 && soldier.careerTrack != CareerTrack.CivilianCollege)
            {
                long monthlyTakeHome = Math.Max(0L, monthlyGross - monthlyTax);
                long monthlyCollegePay = (long)(monthlyTakeHome * 0.15f); // 15% debt repayment
                semesterCollegeLoan = Math.Min(soldier.collegeTuitionDebt, monthlyCollegePay * 6L);
                soldier.collegeTuitionDebt -= semesterCollegeLoan;
            }

            long netTakeHomeSalary = Math.Max(0L, semesterGrossSalary - semesterTax - semesterAcademyLoan - semesterCollegeLoan);

            // 4. Investment Dividends (Adjusted dynamically by WartimeMarketEngine)
            long baseBondDiv = soldier.warBondsOwned * EconomyData.WAR_BOND_SEMESTER_DIVIDEND;
            long baseGuildDiv = soldier.tradeGuildShares * EconomyData.TRADE_GUILD_SHARE_SEMESTER_DIVIDEND;
            long bondDividends = WartimeMarketEngine.GetAdjustedBondDividend(baseBondDiv);
            long guildDividends = WartimeMarketEngine.GetAdjustedTradeGuildDividend(baseGuildDiv);
            long totalDividends = bondDividends + guildDividends;
            soldier.investmentDividendsLastSemester = totalDividends;

            // 5. Commercial Business Net Revenue
            long businessRevenue = CalculateSemesterBusinessRevenue(soldier);
            soldier.businessRevenueLastSemester = businessRevenue;

            // 6. Frontline Combat Hazard Pay & Operational Stipends
            long semesterCombatBounties = 0L;
            if (soldier.isInCombatDeployment && !soldier.isCivilian && !soldier.isRetired)
            {
                if (soldier.militaryBranch == MilitaryBranch.Army)
                {
                    // Realistic monthly frontline combat hazard pay ($200-$400/mo = $1,200-$2,400/semester)
                    semesterCombatBounties = (long)(UnityEngine.Random.Range(1200L, 2400L) + (soldier.combatSkill * 5f));
                    if (soldier.isSpecialForces)
                    {
                        // Special Forces direct action hazardous duty stipend (+$350/mo = +$2,100/semester)
                        semesterCombatBounties += (long)UnityEngine.Random.Range(1800L, 2500L);
                    }
                    semesterCombatBounties = WartimeMarketEngine.GetAdjustedCombatBounty(semesterCombatBounties);
                    soldier.combatBountiesEarned += semesterCombatBounties;
                }
                else
                {
                    // Air Force: Combat flight duty hazard pay ($150-$300/mo = $900-$1,800/semester)
                    if (soldier.isFlightDuty && UnityEngine.Random.value < 0.60f)
                    {
                        semesterCombatBounties = (long)UnityEngine.Random.Range(900L, 1800L);
                        semesterCombatBounties = WartimeMarketEngine.GetAdjustedCombatBounty(semesterCombatBounties);
                        soldier.combatBountiesEarned += semesterCombatBounties;
                    }
                }
            }

            // 7. Housing & Dining Living Costs (Adjusted dynamically by WartimeMarketEngine)
            // While on active combat deployment, theater forward operating bases and combat rations are 100% military subsidized ($0).
            long housingUpkeep = 0L;
            long diningUpkeep = 0L;
            if (!soldier.isInCombatDeployment && !soldier.isYouthPhase && soldier.careerTrack != CareerTrack.MilitaryRecruitBasic)
            {
                housingUpkeep = CalculateSemesterHousingCost(soldier);
                diningUpkeep = EconomyData.GetDiningSemesterCost(soldier.garrisonDiningTier);
            }
            soldier.totalHousingExpensesPaid += housingUpkeep;
            soldier.totalFoodExpensesPaid += diningUpkeep;

            // Apply Dining Plan benefits during garrison
            if (!soldier.isInCombatDeployment && !soldier.isDead)
            {
                switch (soldier.garrisonDiningTier)
                {
                    case DiningPlanTier.CommissaryCooking:
                        soldier.fitness = Math.Min(100f, soldier.fitness + 2f);
                        soldier.health = Math.Min(100f, soldier.health + 2f);
                        soldier.happiness = Math.Min(100f, soldier.happiness + 2f);
                        break;
                    case DiningPlanTier.HighProteinAthletic:
                        soldier.fitness = Math.Min(100f, soldier.fitness + 4f);
                        soldier.health = Math.Min(100f, soldier.health + 4f);
                        soldier.combatReadiness = Math.Min(100f, soldier.combatReadiness + 3f);
                        break;
                    case DiningPlanTier.OfficersClubDining:
                        soldier.happiness = Math.Min(100f, soldier.happiness + 6f);
                        soldier.squadRespect = Math.Min(100f, soldier.squadRespect + 3f);
                        break;
                    case DiningPlanTier.PrivateChefExecutive:
                        soldier.happiness = Math.Min(100f, soldier.happiness + 10f);
                        soldier.health = Math.Min(100f, soldier.health + 5f);
                        soldier.squadRespect = Math.Min(100f, soldier.squadRespect + 5f);
                        break;
                }
            }

            // Air Force base housing living condition advantage: +15% Morale, +10% Health recovery
            if (soldier.militaryBranch == MilitaryBranch.AirForce && !soldier.isCivilian && !soldier.isRetired)
            {
                soldier.happiness = Math.Min(100f, soldier.happiness + EconomyData.AIR_FORCE_MORALE_BOOST);
                soldier.health = Math.Min(100f, soldier.health + EconomyData.AIR_FORCE_HEALTH_BOOST);

                if (soldier.isFlightDuty)
                {
                    int flightHours = UnityEngine.Random.Range(65, 120);
                    soldier.flightHoursLogged += flightHours;
                }
            }

            // 8. Child Heir Education Tutor Spending
            long tutorExpense = 0L;
            if (soldier.childHeir != null && soldier.heirHasPrivateTutor)
            {
                tutorExpense = EconomyData.PRIVATE_TUTOR_SEMESTER_COST;
                soldier.totalEducationExpensesPaid += tutorExpense;
                soldier.childHeir.examScoreBonus += 6;
                soldier.childHeir.hasPrivateTutors = true;
            }

            // 9. Total Net Cash Flow (including War Memoir Royalties if published)
            long totalInflow = netTakeHomeSalary + totalDividends + businessRevenue + semesterCombatBounties + soldier.warMemoirRoyaltiesPerSemester;
            long totalOutflow = housingUpkeep + diningUpkeep + tutorExpense;
            long netCashFlow = totalInflow - totalOutflow;

            soldier.cashBalance = Math.Max(0L, soldier.cashBalance + netCashFlow);

            // 10. Generate Feed Log
            LogSemesterFinancialFeed(
                soldier,
                monthlyGross,
                semesterGrossSalary,
                semesterTax,
                netTakeHomeSalary,
                semesterAcademyLoan,
                semesterCollegeLoan,
                totalDividends,
                businessRevenue,
                semesterCombatBounties,
                housingUpkeep,
                diningUpkeep,
                tutorExpense,
                netCashFlow
            );
        }

        private static void AccrueLoanInterests(SoldierProfile soldier)
        {
            if (soldier.academyTuitionDebt > 0)
            {
                long academyInterest = (long)(soldier.academyTuitionDebt * EconomyData.ACADEMY_LOAN_SEMESTER_INTEREST);
                soldier.academyTuitionDebt += academyInterest;
            }

            if (soldier.collegeTuitionDebt > 0)
            {
                long collegeInterest = (long)(soldier.collegeTuitionDebt * EconomyData.COLLEGE_LOAN_SEMESTER_INTEREST);
                soldier.collegeTuitionDebt += collegeInterest;
            }
        }

        public static long CalculateSemesterBusinessRevenue(SoldierProfile soldier)
        {
            if (soldier.ownedBusiness == CommercialBusiness.None) return 0L;

            long baseRevenue = EconomyData.GetBusinessBaseSemesterRevenue(soldier.ownedBusiness);
            float efficiency = 1.0f;

            switch (soldier.ownedBusiness)
            {
                case CommercialBusiness.Workshop:
                    efficiency = 1.0f + ((soldier.intellect + soldier.fitness) / 2f - 50f) * 0.005f;
                    break;
                case CommercialBusiness.Tavern:
                    efficiency = 1.0f + (soldier.parentRelationship - 50f) * 0.004f;
                    break;
                case CommercialBusiness.MerchantCaravan:
                    efficiency = 1.0f + (soldier.intellect - 50f) * 0.008f;
                    break;
                case CommercialBusiness.PrivateSecurityAgency:
                    efficiency = 1.0f + ((soldier.combatSkill + soldier.leadership) / 2f - 50f) * 0.010f;
                    break;
                case CommercialBusiness.DefenseMunitionsConglomerate:
                    efficiency = 1.0f + ((soldier.intellect + soldier.combatSkill) / 2f - 50f) * 0.012f;
                    break;
                case CommercialBusiness.ImperialShippingCartel:
                    efficiency = 1.0f + ((soldier.intellect + soldier.politicalInfluence) / 2f - 50f) * 0.015f;
                    break;
                case CommercialBusiness.CentralBankingHouseAndMint:
                    efficiency = 1.0f + (soldier.politicalInfluence - 50f) * 0.020f;
                    break;
            }

            efficiency = Mathf.Clamp(efficiency, 0.60f, 2.20f);
            long unadjustedRev = (long)(baseRevenue * efficiency);
            return WartimeMarketEngine.GetAdjustedBusinessRevenue(soldier.ownedBusiness, unadjustedRev);
        }

        private static void LogSemesterFinancialFeed(
            SoldierProfile soldier,
            long monthlyGross,
            long semesterGrossSalary,
            long semesterTax,
            long netSalary,
            long academyLoan,
            long collegeLoan,
            long dividends,
            long businessRevenue,
            long combatBounties,
            long housingCost,
            long diningCost,
            long tutorCost,
            long netCashFlow)
        {
            string phaseTitle;
            string taxTag;

            if (soldier.isRetired)
            {
                phaseTitle = "VETERAN PENSION & ASSET DIVIDENDS";
                taxTag = "<color=#00FFAA>Pension Tax-Exempt</color>";
            }
            else if (soldier.isCivilian)
            {
                phaseTitle = $"CIVILIAN PAYROLL ({soldier.civilianJobTitle.ToUpper()})";
                taxTag = $"<color=#FF8866>Municipal Tax (50%): -${semesterTax:N0}</color>";
            }
            else
            {
                string branchTag = (soldier.militaryBranch == MilitaryBranch.AirForce) ? "AIR FORCE" : "IMPERIAL ARMY";
                string flightTag = soldier.isFlightDuty ? " (Flight Pay +15%)" : "";
                phaseTitle = $"{branchTag} COMPENSATION{flightTag}";
                int taxPercent = Mathf.RoundToInt(DifficultySettings.WarTaxRate * 100f);
                taxTag = (semesterTax > 0)
                    ? $"<color=#FF6666>War Tax ({taxPercent}%): -${semesterTax:N0}</color>"
                    : "<color=#00FFAA>Cadet Stipend (Tax Exempt)</color>";
            }

            string dividendStr = dividends > 0
                ? $" | <color=#FFD700>Dividends: +${dividends:N0}</color> ({soldier.warBondsOwned} Bonds, {soldier.tradeGuildShares} Shares)"
                : "";

            string businessStr = businessRevenue > 0
                ? $" | <color=#00FFAA>{EconomyData.GetBusinessDisplayName(soldier.ownedBusiness)}: +${businessRevenue:N0}</color>"
                : "";

            string bountyStr = combatBounties > 0
                ? $" | <color=#FF4444>Combat Bounties & Plunder: +${combatBounties:N0}</color>"
                : "";

            string housingStr = housingCost > 0
                ? $" | <color=#FFBB88>{EconomyData.GetHousingDisplayName(soldier.housingTier, soldier.militaryBranch)}: -${housingCost:N0}</color>"
                : (soldier.isInCombatDeployment ? " | <color=#00FFAA>Theater FOB Quarters: Subsidized ($0)</color>" : "");

            string diningStr = diningCost > 0
                ? $" | <color=#FFCC66>{EconomyData.GetDiningDisplayName(soldier.garrisonDiningTier)}: -${diningCost:N0}</color>"
                : (soldier.isInCombatDeployment ? " | <color=#00FFAA>Combat MRE Rations: Subsidized ($0)</color>" : "");

            string loanStr = (academyLoan > 0 || collegeLoan > 0)
                ? $" | <color=#FF8866>Debt Payments: -${academyLoan + collegeLoan:N0}</color>"
                : "";

            string tutorStr = tutorCost > 0
                ? $" | <color=#00D4FF>Heir Private Tutors: -${tutorCost:N0}</color>"
                : "";

            string netStr = netCashFlow >= 0
                ? $"<color=#00FFAA>+${netCashFlow:N0}</color>"
                : $"<color=#FF4444>-${Math.Abs(netCashFlow):N0}</color>";

            GameEvents.OnFeedEventLogged?.Invoke("PAYDAY",
                $"<b>[{phaseTitle}]</b> Base: ${monthlyGross:N0}/mo (Semester: ${semesterGrossSalary:N0}) | {taxTag}{dividendStr}{businessStr}{bountyStr}{housingStr}{diningStr}{loanStr}{tutorStr} | " +
                $"<b>Semester Net: {netStr}</b>. Cash Reserves: <b>${soldier.cashBalance:N0}</b>.");
        }

        // ==================== REAL ESTATE SPENDING ====================

        public static bool BuyFamilyHouse(SoldierProfile soldier, FamilyHouseTier tier)
        {
            if (soldier == null || tier == FamilyHouseTier.None) return false;
            if (soldier.familyHouseTier >= tier)
            {
                GameEvents.OnFeedEventLogged?.Invoke("REAL ESTATE",
                    $"<color=#00AAFF>[ALREADY OWNED]</color> Your family already resides in {EconomyData.GetFamilyHouseDisplayName(soldier.familyHouseTier)} or better.");
                return false;
            }

            long cost = EconomyData.GetFamilyHouseCost(tier);
            if (soldier.cashBalance < cost)
            {
                GameEvents.OnFeedEventLogged?.Invoke("REAL ESTATE",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> You need ${cost:N0} to purchase {EconomyData.GetFamilyHouseDisplayName(tier)} (Bank: ${soldier.cashBalance:N0}).");
                return false;
            }

            soldier.cashBalance -= cost;
            soldier.familyHouseTier = tier;

            switch (tier)
            {
                case FamilyHouseTier.RuralHomestead:
                    soldier.familyLivingCondition = Math.Min(100f, soldier.familyLivingCondition + 25f);
                    soldier.parentRelationship = Math.Min(100f, soldier.parentRelationship + 20f);
                    soldier.clanFavor = Math.Min(100f, soldier.clanFavor + 15f);
                    break;
                case FamilyHouseTier.CityTownhouse:
                    soldier.familyLivingCondition = Math.Min(100f, soldier.familyLivingCondition + 45f);
                    soldier.parentRelationship = Math.Min(100f, soldier.parentRelationship + 35f);
                    soldier.clanFavor = Math.Min(100f, soldier.clanFavor + 30f);
                    soldier.politicalInfluence = Math.Min(100f, soldier.politicalInfluence + 10f);
                    break;
                case FamilyHouseTier.AncestralEstate:
                    soldier.familyLivingCondition = 100f;
                    soldier.parentRelationship = 100f;
                    soldier.clanFavor = 100f;
                    soldier.clanSuccessionScore += 40;
                    soldier.politicalInfluence = Math.Min(100f, soldier.politicalInfluence + 25f);
                    break;
                case FamilyHouseTier.DucalPalaceCitadel:
                    soldier.familyLivingCondition = 100f;
                    soldier.parentRelationship = 100f;
                    soldier.clanFavor = 100f;
                    soldier.clanSuccessionScore += 100;
                    soldier.politicalInfluence = Math.Min(100f, soldier.politicalInfluence + 50f);
                    break;
                case FamilyHouseTier.SovereignImperialChateau:
                    soldier.familyLivingCondition = 100f;
                    soldier.parentRelationship = 100f;
                    soldier.clanFavor = 100f;
                    soldier.clanSuccessionScore += 250;
                    soldier.politicalInfluence = 100f;
                    break;
                case FamilyHouseTier.ImperialDynasticSeat:
                    soldier.familyLivingCondition = 100f;
                    soldier.parentRelationship = 100f;
                    soldier.clanFavor = 100f;
                    soldier.clanSuccessionScore += 1000;
                    soldier.politicalInfluence = 100f;
                    break;
            }

            GameEvents.OnFeedEventLogged?.Invoke("REAL ESTATE",
                $"<color=#FFD700><b>[FAMILY REAL ESTATE ACQUIRED • {EconomyData.GetFamilyHouseDisplayName(tier).ToUpper()}]</b></color> " +
                $"You purchased a permanent family estate for <b>${cost:N0}</b>! " +
                $"{EconomyData.GetFamilyHouseDescription(tier)} (Family Living Condition: {soldier.familyLivingCondition:F0}/100).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        public static bool UpgradeHousingTier(SoldierProfile soldier, HousingTier tier)
        {
            if (soldier == null) return false;
            if (soldier.housingTier == tier)
            {
                GameEvents.OnFeedEventLogged?.Invoke("HOUSING",
                    $"<color=#00AAFF>[CURRENT RESIDENCE]</color> You already live in {EconomyData.GetHousingDisplayName(tier, soldier.militaryBranch)}.");
                return false;
            }

            soldier.housingTier = tier;
            long semCost = EconomyData.GetHousingSemesterCost(tier, soldier.militaryBranch);

            if (tier >= HousingTier.ComfortableHome)
            {
                soldier.happiness = Math.Min(100f, soldier.happiness + 15f);
                soldier.health = Math.Min(100f, soldier.health + 10f);
            }

            GameEvents.OnFeedEventLogged?.Invoke("HOUSING",
                $"<color=#00FFAA><b>[RESIDENCE RELOCATION • {EconomyData.GetHousingDisplayName(tier, soldier.militaryBranch).ToUpper()}]</b></color> " +
                $"You moved into <b>{EconomyData.GetHousingDisplayName(tier, soldier.militaryBranch)}</b>! " +
                $"Semester Upkeep: <b>${semCost:N0}/sem</b>. {EconomyData.GetHousingDescription(tier, soldier.militaryBranch)}");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        // ==================== HOSPITAL & HEALTHCARE SPENDING ====================

        public static bool TreatAtImperialHospital(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return false;

            if (soldier.cashBalance < EconomyData.IMPERIAL_HOSPITAL_TREATMENT_COST)
            {
                GameEvents.OnFeedEventLogged?.Invoke("HOSPITAL",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Imperial specialist treatment costs ${EconomyData.IMPERIAL_HOSPITAL_TREATMENT_COST:N0} (Bank: ${soldier.cashBalance:N0}).");
                return false;
            }

            soldier.cashBalance -= EconomyData.IMPERIAL_HOSPITAL_TREATMENT_COST;
            soldier.totalMedicalExpensesPaid += EconomyData.IMPERIAL_HOSPITAL_TREATMENT_COST;

            soldier.health = Math.Min(100f, soldier.health + 45f);
            soldier.combatReadiness = Math.Min(100f, soldier.combatReadiness + 20f);
            soldier.fitness = Math.Min(100f, soldier.fitness + 10f);

            GameEvents.OnFeedEventLogged?.Invoke("HOSPITAL",
                $"<color=#00FFAA><b>[MILITARY SPECIALIST SURGERY]</b></color> Military trauma surgeons repaired combat wounds and administered intensive rehabilitation (-${EconomyData.IMPERIAL_HOSPITAL_TREATMENT_COST:N0}). " +
                $"Health restored to <b>{soldier.health:F0}/100</b>! Combat readiness restored to <b>{soldier.combatReadiness:F0}/100</b>.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        public static bool TreatFamilyMedicalEmergency(SoldierProfile soldier)
        {
            if (soldier == null) return false;

            if (soldier.cashBalance < EconomyData.FAMILY_MEDICAL_EMERGENCY_COST)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY CLINIC",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Urgent parent medicine costs ${EconomyData.FAMILY_MEDICAL_EMERGENCY_COST:N0} (Bank: ${soldier.cashBalance:N0}).");
                return false;
            }

            soldier.cashBalance -= EconomyData.FAMILY_MEDICAL_EMERGENCY_COST;
            soldier.totalMedicalExpensesPaid += EconomyData.FAMILY_MEDICAL_EMERGENCY_COST;
            soldier.parentRelationship = Math.Min(100f, soldier.parentRelationship + 25f);
            soldier.clanFavor = Math.Min(100f, soldier.clanFavor + 15f);

            GameEvents.OnFeedEventLogged?.Invoke("FAMILY CLINIC",
                $"<color=#00FFAA><b>[FAMILY ILLNESS CURED]</b></color> You hired top imperial physicians to cure your parents' debilitating winter illness (-${EconomyData.FAMILY_MEDICAL_EMERGENCY_COST:N0}). " +
                "Your parents wept tears of gratitude. (+25 Parent Relationship, +15 Clan Favor).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        // ==================== CHILDREN & EDUCATION SPENDING ====================

        public static bool SetHeirPrivateTutor(SoldierProfile soldier, bool enabled)
        {
            if (soldier == null || soldier.childHeir == null)
            {
                GameEvents.OnFeedEventLogged?.Invoke("EDUCATION",
                    "<color=#FF8800>[NO HEIR]</color> You do not have a child heir to enroll in private tutoring.");
                return false;
            }

            soldier.heirHasPrivateTutor = enabled;
            if (enabled)
            {
                GameEvents.OnFeedEventLogged?.Invoke("EDUCATION",
                    $"<color=#FFD700><b>[ELITE PRIVATE TUTOR ENGAGED • {soldier.childHeir.childName}]</b></color> " +
                    $"You retained veteran scholars and combat instructors for {soldier.childHeir.childName} " +
                    $"(${EconomyData.PRIVATE_TUTOR_SEMESTER_COST:N0}/semester). Their national exam readiness will compound every term!");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("EDUCATION",
                    $"<color=#00AAFF>[TUTORING PAUSED]</color> Private tutoring services for {soldier.childHeir.childName} have been concluded.");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        // ==================== COMMERCIAL BUSINESS & INVESTMENTS ====================

        public static bool PurchaseBusiness(SoldierProfile soldier, CommercialBusiness business)
        {
            if (soldier == null || business == CommercialBusiness.None) return false;
            if (soldier.ownedBusiness == business)
            {
                GameEvents.OnFeedEventLogged?.Invoke("COMMERCE",
                    $"<color=#00AAFF>[ALREADY OWNED]</color> You already own this commercial enterprise: {EconomyData.GetBusinessDisplayName(business)}.");
                return false;
            }

            long cost = EconomyData.GetBusinessPurchaseCost(business);
            if (soldier.cashBalance < cost)
            {
                GameEvents.OnFeedEventLogged?.Invoke("COMMERCE",
                    $"<color=#FF4444>[INSUFFICIENT CAPITAL]</color> Acquiring {EconomyData.GetBusinessDisplayName(business)} requires ${cost:N0} (Bank: ${soldier.cashBalance:N0}).");
                return false;
            }

            soldier.cashBalance -= cost;
            soldier.ownedBusiness = business;
            soldier.totalBusinessInvestment += cost;

            GameEvents.OnFeedEventLogged?.Invoke("COMMERCE",
                $"<color=#FFD700><b>[COMMERCIAL ENTERPRISE ACQUIRED • {EconomyData.GetBusinessDisplayName(business).ToUpper()}]</b></color> " +
                $"You acquired full ownership of {EconomyData.GetBusinessDisplayName(business)} for <b>${cost:N0}</b>! " +
                $"{EconomyData.GetBusinessDescription(business)} Profits will be distributed every semester.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        public static bool PurchaseWarBonds(SoldierProfile soldier, int count = 1)
        {
            if (soldier == null || count <= 0) return false;

            long totalCost = count * EconomyData.WAR_BOND_PRICE;
            if (soldier.cashBalance < totalCost)
            {
                GameEvents.OnFeedEventLogged?.Invoke("INVESTMENT",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Purchasing {count} War Bonds costs ${totalCost:N0} (Bank: ${soldier.cashBalance:N0}).");
                return false;
            }

            soldier.cashBalance -= totalCost;
            soldier.warBondsOwned += count;
            soldier.meritRating = Math.Min(100f, soldier.meritRating + (count * 6f));
            soldier.politicalInfluence = Math.Min(100f, soldier.politicalInfluence + (count * 4f));

            GameEvents.OnFeedEventLogged?.Invoke("INVESTMENT",
                $"<color=#FFD700><b>[WAR BONDS PURCHASED]</b></color> You subscribed to <b>{count} Imperial Victory War Bonds</b> (-${totalCost:N0}). " +
                $"Total Bonds Owned: <b>{soldier.warBondsOwned}</b>. Annual Yield: 12% (${soldier.warBondsOwned * EconomyData.WAR_BOND_SEMESTER_DIVIDEND:N0}/semester dividends).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        public static bool PurchaseGuildShares(SoldierProfile soldier, int count = 1)
        {
            if (soldier == null || count <= 0) return false;

            long totalCost = count * EconomyData.TRADE_GUILD_SHARE_PRICE;
            if (soldier.cashBalance < totalCost)
            {
                GameEvents.OnFeedEventLogged?.Invoke("INVESTMENT",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Purchasing {count} Trade Guild Shares costs ${totalCost:N0} (Bank: ${soldier.cashBalance:N0}).");
                return false;
            }

            soldier.cashBalance -= totalCost;
            soldier.tradeGuildShares += count;

            GameEvents.OnFeedEventLogged?.Invoke("INVESTMENT",
                $"<color=#00FFAA><b>[GUILD SHARES PURCHASED]</b></color> You acquired <b>{count} Merchant Guild Equity Shares</b> (-${totalCost:N0}). " +
                $"Total Guild Shares: <b>{soldier.tradeGuildShares}</b>. Pays <b>${soldier.tradeGuildShares * EconomyData.TRADE_GUILD_SHARE_SEMESTER_DIVIDEND:N0}/semester</b> in commercial trading dividends.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        public static bool BuySquadRounds(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return false;

            if (soldier.cashBalance < EconomyData.SQUAD_ROUNDS_COST)
            {
                GameEvents.OnFeedEventLogged?.Invoke("BARRACKS",
                    $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> Buying a round for the squad costs ${EconomyData.SQUAD_ROUNDS_COST:N0}.");
                return false;
            }

            soldier.cashBalance -= EconomyData.SQUAD_ROUNDS_COST;
            soldier.squadRespect = Math.Min(100f, soldier.squadRespect + 8f);
            soldier.happiness = Math.Min(100f, soldier.happiness + 10f);

            if (soldier.squadMates != null)
            {
                foreach (var mate in soldier.squadMates)
                {
                    if (mate != null && !mate.isDead)
                    {
                        mate.relationship = Math.Min(100f, mate.relationship + 12f);
                    }
                }
            }

            GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                $"<color=#00FFAA><b>[ROUNDS BOUGHT AT CANTEEN]</b></color> You bought an evening round of ales and roast meat for the whole squad (-${EconomyData.SQUAD_ROUNDS_COST:N0})! " +
                "The grunts raised their mugs to you and swore their allegiance under fire (+Squad Respect, +Comrade Bonds).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        public static void TransferDynasticInheritance(SoldierProfile predecessor, SoldierProfile heir)
        {
            if (predecessor == null || heir == null) return;

            heir.familyHouseTier = predecessor.familyHouseTier;
            heir.housingTier = (predecessor.housingTier > HousingTier.ModestApartment)
                ? HousingTier.ModestApartment
                : predecessor.housingTier;

            long inheritedCash = predecessor.cashBalance / 2L;
            heir.cashBalance += inheritedCash;
            heir.inheritedFamilyWealth = inheritedCash;

            heir.warBondsOwned = predecessor.warBondsOwned;
            heir.tradeGuildShares = predecessor.tradeGuildShares;
            heir.ownedBusiness = predecessor.ownedBusiness;

            if (predecessor.earnedMedals != null)
            {
                heir.inheritedMedals = new System.Collections.Generic.List<string>(predecessor.earnedMedals);
            }

            if (predecessor.childHeir != null && predecessor.childHeir.examScoreBonus > 0)
            {
                heir.examPreparationBonus += predecessor.childHeir.examScoreBonus;
                heir.intellect = Math.Min(100f, heir.intellect + (predecessor.childHeir.examScoreBonus * 0.5f));
            }

            GameEvents.OnFeedEventLogged?.Invoke("DYNASTY",
                $"<color=#FFD700><b>[DYNASTIC INHERITANCE EXECUTED]</b></color> " +
                $"Heir <b>{heir.characterName}</b> inherited ${inheritedCash:N0} cash reserves, {predecessor.warBondsOwned} War Bonds, " +
                $"{predecessor.tradeGuildShares} Guild Shares, and {EconomyData.GetBusinessDisplayName(predecessor.ownedBusiness)}!");
        }
    }
}
