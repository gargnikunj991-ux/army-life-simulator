using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class SalaryCalculator
    {
        public static float ImperialWarTaxRate => DifficultySettings.WarTaxRate;   // Scaled by Game Difficulty (Story: 20%, Realistic: 40%, Ironclad: 50%)
        public const float CivilMunicipalTaxRate = 0.50f; // 50% Civil Municipal Tax for civilian jobs

        /// <summary>
        /// Calculates the gross monthly compensation before taxation across Military, Civilian, and Veteran phases.
        /// </summary>
        public static long CalculateGrossMonthlyPay(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return 0;
            if (soldier.isYouthPhase) return 0; // Dependent students receive no monthly salary

            // Imprisoned convicts receive no civilian or military payroll
            if (soldier.isImprisoned || soldier.militaryBrigSemestersRemaining > 0 || soldier.isFortLeavenworthInmate) return 0L;

            // 1. Post-Military Honorable Retirement & Veteran Careers
            if (soldier.isRetired)
            {
                // Dishonorable Discharge or Bad Conduct Discharge forfeits all VA benefits and military pension
                if (soldier.dischargeCharacterization == DischargeCharacterization.DishonorableDischarge ||
                    soldier.dischargeCharacterization == DischargeCharacterization.BadConductDischarge)
                {
                    return 0L;
                }

                long pension = soldier.calculatedPensionPerMonth > 0 
                    ? soldier.calculatedPensionPerMonth 
                    : (soldier.veteranPensionPerMonth > 0 ? soldier.veteranPensionPerMonth : 1500L);

                long disability = soldier.vaDisabilityAllowancePerMonth > 0
                    ? soldier.vaDisabilityAllowancePerMonth
                    : EconomyEngine.CalculateDisabilityAllowance(soldier);

                long veteranCareerWage = 0L;
                switch (soldier.veteranCareer)
                {
                    case VeteranCareer.UniversityGI:       veteranCareerWage = 500L; break;   // GI Bill Education Stipend
                    case VeteranCareer.DefenseContractor:  veteranCareerWage = 2500L; break;  // Security Consulting Pay
                    case VeteranCareer.CityMayor:          veteranCareerWage = 4000L; break;  // Municipal Governance
                    case VeteranCareer.RegionalGovernor:   veteranCareerWage = 8000L; break;  // Provincial Leadership
                    case VeteranCareer.MinisterOfDefense:  veteranCareerWage = 18000L; break; // Supreme Cabinet Salary
                }

                return pension + disability + veteranCareerWage;
            }

            // 2. Civilian Employment Tracks
            if (soldier.careerTrack == CareerTrack.CivilianWork || soldier.careerTrack == CareerTrack.CivilianProfessional)
            {
                long baseCiv = soldier.civilianSalaryPerMonth > 0 ? soldier.civilianSalaryPerMonth : 150L;
                return WartimeMarketEngine.GetAdjustedCivilianSalary(baseCiv);
            }
            if (soldier.careerTrack == CareerTrack.CivilianCollege)
            {
                return 0L; // Full-time college student
            }

            // 3. Military Training Schools & Pipelines
            if (soldier.careerTrack == CareerTrack.MilitaryRecruitBasic || (soldier.currentRank != null && soldier.currentRank.paygrade == "E-1"))
            {
                // In basic combat training (first 6 months), recruits do NOT receive a cash salary!
                // 100% of lodging, food rations, uniform, armory gear, and drill expenses are state-covered.
                // Cash pay begins upon graduating to active line Private (E-2).
                return 0L;
            }
            if (soldier.careerTrack == CareerTrack.MilitaryTechSchool)
            {
                long techPay = (soldier.currentRank != null) ? soldier.currentRank.baseMonthlySalary : 1800L;
                return techPay;
            }
            if (soldier.careerTrack == CareerTrack.MilitaryMedicalSchool)
            {
                long medPay = (soldier.currentRank != null) ? soldier.currentRank.baseMonthlySalary : 2050L;
                return medPay;
            }
            if (soldier.careerTrack == CareerTrack.MilitaryWarAcademy)
            {
                // War academy cadet monthly stipend
                return 1200L;
            }

            // 4. Active Military Duty
            long total = 0;

            // Rank Base Pay & Annual Longevity Step Raises (Non-Promotion Years)
            long basePay = (soldier.currentRank != null) ? soldier.currentRank.baseMonthlySalary : 600L;
            total += basePay;
            total += soldier.longevityMonthlyPayBonus;

            // Air Force Flight Duty Aviation Pay (+15% Aviation Flight Pay Bonus on base salary)
            if (soldier.isFlightDuty)
            {
                long aviationBonus = (long)(basePay * EconomyData.AVIATION_FLIGHT_PAY_BONUS);
                total += aviationBonus;
            }

            // Technical School Graduate Certification Pay
            if (soldier.isTechnicalSchoolGraduate)
            {
                total += 450L; // Specialist Technical Stipend
                total += 350L; // Cyber & Systems Maintenance Pay
            }

            // Performance Merit Bonus
            if (soldier.meritRating >= 90f) total += 1000L;
            else if (soldier.meritRating >= 75f) total += 400L;

            // Combat Deployment Hazard Pay
            if (soldier.isInCombatDeployment) total += 450L;

            // Special Forces Operator Stipend (Higher pay for elite operators)
            if (soldier.isSpecialForces) total += 800L;

            // Extended Frontline Combat Tour Veteran Bonus:
            // If the soldier stays continuously on the battlefield longer than 1 year (>= 2 semesters),
            // total pay increases by 10%, independent of any other pay increases.
            if (soldier.hasExtendedTourPayBonus && soldier.isInCombatDeployment)
            {
                long extendedCombatBonus = (long)(total * 0.10f);
                total += extendedCombatBonus;
            }

            // Article 15 (UCMJ) Non-Judicial Punishment: Forfeiture of 1/2 pay
            if (soldier.hasForfeitedPayThisSemester)
            {
                total = total / 2;
            }

            return total;
        }

        /// <summary>
        /// Calculates the monthly academy tuition loan deduction (minimum 10% of take-home pay).
        /// </summary>
        public static long CalculateMonthlyLoanDeduction(SoldierProfile soldier)
        {
            if (soldier == null || soldier.academyTuitionDebt <= 0) return 0L;
            long netBeforeLoan = CalculateNetMonthlyPayBeforeLoan(soldier);
            if (netBeforeLoan <= 0) return 0L;

            float rate = Mathf.Clamp(soldier.academyLoanDeductionPercent, 0.10f, 0.80f); // Minimum 10%
            long deduction = (long)(netBeforeLoan * rate);
            return System.Math.Min(deduction, soldier.academyTuitionDebt);
        }

        public static long CalculateNetMonthlyPayBeforeLoan(SoldierProfile soldier)
        {
            long gross = CalculateGrossMonthlyPay(soldier);
            if (gross <= 0) return 0L;

            long tax = CalculateMonthlyTax(soldier);
            return System.Math.Max(0L, gross - tax);
        }

        /// <summary>
        /// Calculates the net monthly take-home pay after tax and academy loan deductions.
        /// </summary>
        public static long CalculateNetMonthlyPay(SoldierProfile soldier)
        {
            long net = CalculateNetMonthlyPayBeforeLoan(soldier);
            long loanPayment = CalculateMonthlyLoanDeduction(soldier);
            return System.Math.Max(0L, net - loanPayment);
        }

        public static long CalculateMonthlyTax(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.isYouthPhase) return 0;
            if (soldier.isImprisoned || soldier.militaryBrigSemestersRemaining > 0) return 0;

            // Retired veterans: Military pension and VA disability are 100% tax-exempt!
            // Only civilian post-service employment wages are subject to municipal tax.
            if (soldier.isRetired)
            {
                long careerWage = 0L;
                switch (soldier.veteranCareer)
                {
                    case VeteranCareer.DefenseContractor: careerWage = 2500L; break;
                    case VeteranCareer.CityMayor:         careerWage = 4000L; break;
                    case VeteranCareer.RegionalGovernor:  careerWage = 8000L; break;
                    case VeteranCareer.MinisterOfDefense: careerWage = 18000L; break;
                    default:                              careerWage = 0L; break; // GI Bill is tax-exempt scholarship
                }
                return (long)(careerWage * CivilMunicipalTaxRate);
            }

            long gross = CalculateGrossMonthlyPay(soldier);
            if (gross <= 0) return 0;

            if (soldier.isCivilian)
            {
                return (long)(gross * CivilMunicipalTaxRate);
            }

            // Cadet educational stipends are exempt from wartime income tax
            if (soldier.careerTrack == CareerTrack.MilitaryTechSchool || 
                soldier.careerTrack == CareerTrack.MilitaryMedicalSchool || 
                soldier.careerTrack == CareerTrack.MilitaryWarAcademy)
            {
                return 0;
            }

            return (long)(gross * ImperialWarTaxRate);
        }

        /// <summary>
        /// Calculates the next longevity pay rise percentage based on raises earned so far:
        /// Starts at 10.0%, decreases by 0.5% with each rise until 5.0%, then stops entirely (returns 0%).
        /// Scale: Rise 1 = 10.0%, Rise 2 = 9.5%, ... Rise 11 = 5.0%, then stops.
        /// </summary>
        public static float GetNextPayRisePercentage(int currentRaisesCount)
        {
            float rate = 0.10f - (currentRaisesCount * 0.005f);
            if (rate < 0.05f - 0.0001f) return 0f; // Stopped after 5% tier is exhausted
            return rate;
        }

        /// <summary>
        /// Evaluates whether a soldier qualifies for an annual or multi-year longevity pay rise.
        /// Restricts E-4 to every 2 years, E-6+ to every 3 years with high merit (>=80) and squad respect (>=65).
        /// </summary>
        public static bool CanEarnPayRise(SoldierProfile soldier, out string reason)
        {
            reason = "";
            if (soldier == null || soldier.isDead || soldier.isRetired || soldier.isCivilian || soldier.isYouthPhase)
            {
                reason = "Ineligible service status.";
                return false;
            }

            if (soldier.hasReachedPayRaiseCeiling || GetNextPayRisePercentage(soldier.annualLongevityRaisesEarned) <= 0f)
            {
                reason = "Maximum career longevity pay rise ceiling reached.";
                return false;
            }

            // Clean conduct check: no active Article 15 forfeiture or brig restriction
            if (soldier.hasForfeitedPayThisSemester || soldier.isBarracksRestricted || soldier.isImprisoned || soldier.militaryBrigSemestersRemaining > 0)
            {
                reason = "Withheld due to active UCMJ non-judicial punishment or barracks restriction.";
                return false;
            }

            string pg = soldier.currentRank != null ? soldier.currentRank.paygrade.ToUpper() : "E-1";

            // 1. Junior Enlisted (E-1 to E-3): Eligible every 1 full year (2 semesters)
            if (pg.StartsWith("E-1") || pg.StartsWith("E-2") || pg.StartsWith("E-3"))
            {
                if (soldier.semestersSinceLastPayRaise < 2)
                {
                    reason = "Junior enlisted longevity raises require 1 full year of service.";
                    return false;
                }
                return true;
            }

            // 2. Corporal / Sergeant (E-4, E-5): Not given each year - requires 2 full years (4 semesters) + Merit >= 60
            if (pg.StartsWith("E-4") || pg.StartsWith("E-5"))
            {
                if (soldier.semestersSinceLastPayRaise < 4)
                {
                    float servedYears = soldier.semestersSinceLastPayRaise * 0.5f;
                    reason = $"E-4 and E-5 pay raises are awarded every 2 years (served {servedYears:F1}/2.0 yrs).";
                    return false;
                }
                if (soldier.meritRating < 60f)
                {
                    reason = $"Requires at least 60% Merit Rating (current: {soldier.meritRating:F0}%).";
                    return false;
                }
                return true;
            }

            // 3. Senior NCOs and Officers (E-6+ and Officers): Highly difficult - requires 3 full years (6 semesters) + Merit >= 80 + Squad Respect >= 65
            if (soldier.semestersSinceLastPayRaise < 6)
            {
                float servedYears = soldier.semestersSinceLastPayRaise * 0.5f;
                reason = $"Senior grade pay raises (E-6+) are awarded every 3 years (served {servedYears:F1}/3.0 yrs).";
                return false;
            }
            if (soldier.meritRating < 80f)
            {
                reason = $"Senior pay step requires superior Merit Rating (current: {soldier.meritRating:F0}% / 80% min).";
                return false;
            }
            if (soldier.squadRespect < 65f)
            {
                reason = $"Senior pay step requires high Squad Respect (current: {soldier.squadRespect:F0}% / 65% min).";
                return false;
            }

            return true;
        }
    }
}
