using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class SalaryCalculator
    {
        public const float ImperialWarTaxRate = 0.40f;   // 40% Imperial Wartime Tax for active military
        public const float CivilMunicipalTaxRate = 0.50f; // 50% Civil Municipal Tax for civilian jobs

        /// <summary>
        /// Calculates the gross monthly compensation before taxation across Military, Civilian, and Veteran phases.
        /// </summary>
        public static long CalculateGrossMonthlyPay(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return 0;
            if (soldier.isYouthPhase) return 0; // Dependent students receive no monthly salary

            // Imprisoned convicts receive no civilian or military payroll
            if (soldier.isImprisoned || soldier.militaryBrigSemestersRemaining > 0) return 0L;

            // 1. Post-Military Honorable Retirement & Veteran Careers
            if (soldier.isRetired)
            {
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

            // 3. Military Training Schools
            if (soldier.careerTrack == CareerTrack.MilitaryRecruitBasic)
            {
                return 0L; // Basic training (6 months): no pay, no salary
            }
            if (soldier.careerTrack == CareerTrack.MilitaryTechSchool)
            {
                // Technical school (2 years): starts to pay after 1 year (2 semesters)
                return (soldier.militarySchoolSemestersCompleted >= 2) ? 200L : 0L;
            }
            if (soldier.careerTrack == CareerTrack.MilitaryMedicalSchool)
            {
                // Medical school (3 years): starts to pay after 1 year (2 semesters)
                return (soldier.militarySchoolSemestersCompleted >= 2) ? 250L : 0L;
            }
            if (soldier.careerTrack == CareerTrack.MilitaryWarAcademy)
            {
                // War academy: pays from the start
                return 300L;
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

            // Combat Readiness & Special Duty Stipend
            total += (soldier.powerTier * 150L);

            // Feudal Noble Aristocratic Allowance
            switch (soldier.nobleTitle)
            {
                case NobleRank.Baron:     total += 2000L; break;
                case NobleRank.Viscount:  total += 3500L; break;
                case NobleRank.Count:     total += 5000L; break;
                case NobleRank.Marquis:   total += 10000L; break;
                case NobleRank.GrandDuke: total += 25000L; break;
                case NobleRank.RoyalClan: total += 50000L; break;
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
    }
}
