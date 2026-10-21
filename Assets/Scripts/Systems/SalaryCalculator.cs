using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class SalaryCalculator
    {
        public const float ImperialWarTaxRate = 0.40f;   // 40% Imperial Wartime Tax for active military
        public const float CivilMunicipalTaxRate = 0.50f; // 50% Civil Municipal Tax for civilian jobs

        /// <summary>
        /// Calculates the gross monthly compensation before taxation.
        /// </summary>
        public static long CalculateGrossMonthlyPay(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return 0;
            if (soldier.isYouthPhase) return 0; // Dependent students receive no monthly salary

            // 1. Civilian Employment Tracks
            if (soldier.careerTrack == CareerTrack.CivilianWork || soldier.careerTrack == CareerTrack.CivilianProfessional)
            {
                return soldier.civilianSalaryPerMonth > 0 ? soldier.civilianSalaryPerMonth : 150L;
            }
            if (soldier.careerTrack == CareerTrack.CivilianCollege)
            {
                return 0L; // Full-time college student
            }

            // 2. Military Training Schools
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

            // 3. Active Military Duty
            long total = 0;

            // Rank Base Pay
            if (soldier.currentRank != null)
            {
                total += soldier.currentRank.baseMonthlySalary;
            }
            else
            {
                total += 600; // Recruit / unranked
            }

            // Power Tier Cultivation Stipend ($750 per tier)
            total += (soldier.powerTier * 750L);

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

            if (soldier.isCivilian)
            {
                long civilTax = (long)(gross * CivilMunicipalTaxRate);
                return gross - civilTax;
            }

            if (soldier.careerTrack == CareerTrack.MilitaryTechSchool || 
                soldier.careerTrack == CareerTrack.MilitaryMedicalSchool || 
                soldier.careerTrack == CareerTrack.MilitaryWarAcademy)
            {
                return gross; // Cadet stipends are exempt from wartime income tax
            }

            long tax = (long)(gross * ImperialWarTaxRate);
            return gross - tax;
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
            long gross = CalculateGrossMonthlyPay(soldier);
            if (gross <= 0) return 0;

            if (soldier.isCivilian)
            {
                return (long)(gross * CivilMunicipalTaxRate);
            }

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
