using ArmyLife.Data;

namespace ArmyLife.Systems
{
    public static class SalaryCalculator
    {
        public const float ImperialWarTaxRate = 0.50f; // 50% Imperial Wartime Tax

        /// <summary>
        /// Calculates the gross monthly compensation before taxation.
        /// </summary>
        public static long CalculateGrossMonthlyPay(SoldierProfile soldier)
        {
            if (soldier == null) return 0;

            long total = 0;

            // 1. Rank Base Pay
            if (soldier.currentRank != null)
            {
                total += soldier.currentRank.baseMonthlySalary;
            }
            else
            {
                total += 600; // Recruit / unranked
            }

            // 2. Power Tier Cultivation Stipend ($750 per tier)
            total += (soldier.powerTier * 750L);

            // 3. Feudal Noble Aristocratic Allowance
            switch (soldier.nobleTitle)
            {
                case NobleRank.Baron:     total += 2000L; break;
                case NobleRank.Viscount:  total += 3500L; break;
                case NobleRank.Count:     total += 5000L; break;
                case NobleRank.Marquis:   total += 10000L; break;
                case NobleRank.GrandDuke: total += 25000L; break;
                case NobleRank.RoyalClan: total += 50000L; break;
            }

            // 4. Technical School Graduate Certification Pay
            if (soldier.isTechnicalSchoolGraduate)
            {
                total += 450L; // Specialist Technical Stipend
                total += 350L; // Cyber & Systems Maintenance Pay
            }

            // 5. Performance Merit Bonus
            if (soldier.meritRating >= 90f) total += 1000L;
            else if (soldier.meritRating >= 75f) total += 400L;

            // 6. Combat Deployment Hazard Pay
            if (soldier.isInCombatDeployment) total += 450L;

            return total;
        }

        /// <summary>
        /// Calculates the net monthly take-home pay after deducting the 50% Imperial War Tax.
        /// </summary>
        public static long CalculateNetMonthlyPay(SoldierProfile soldier)
        {
            long gross = CalculateGrossMonthlyPay(soldier);
            long tax = (long)(gross * ImperialWarTaxRate);
            return gross - tax;
        }

        public static long CalculateMonthlyTax(SoldierProfile soldier)
        {
            long gross = CalculateGrossMonthlyPay(soldier);
            return (long)(gross * ImperialWarTaxRate);
        }
    }
}
