using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Grounded Armed Services Vocational Aptitude Battery (ASVAB) and Military Entrance Processing Station (MEPS) evaluation system.
    /// Completely supersedes legacy fantasy talent crystal and resonance chamber apparatuses.
    /// Evaluates recruits on arithmetic reasoning, paragraph comprehension, mechanical comprehension, and tactical aptitude.
    /// </summary>
    public static class TalentTestingApparatus
    {
        /// <summary>
        /// Any enlisted soldier or cadet may view or verify their official MEPS / ASVAB score on record.
        /// </summary>
        public static bool CanTestSelf(SoldierProfile soldier)
        {
            if (soldier == null) return false;
            return true;
        }

        /// <summary>
        /// Family members can take standard academic / military aptitude assessments.
        /// </summary>
        public static bool CanTestFamily(SoldierProfile soldier)
        {
            if (soldier == null) return false;
            return true;
        }

        /// <summary>
        /// Conducts or reviews the official Armed Forces Qualification Test (AFQT) / MEPS vocational evaluation.
        /// </summary>
        public static bool PerformSelfApparatusTest(SoldierProfile soldier)
        {
            if (soldier == null) return false;

            soldier.isPersonalTalentRevealed = true;

            int afqtScore = Mathf.Clamp(Mathf.RoundToInt((soldier.tacticalIntellect * 0.45f) + (soldier.discipline * 0.30f) + (soldier.marksmanshipScore * 0.25f)), 31, 99);
            soldier.talentScore = afqtScore;

            string categoryDesc;
            if (afqtScore >= 93)
                categoryDesc = "<color=#FFD700>Category I (Top 10% • Intelligence, Special Forces & Officer Candidate Qualified)</color>";
            else if (afqtScore >= 65)
                categoryDesc = "<color=#00D4FF>Category II (Above Average • Signals, Combat Engineer & Technical Specialist)</color>";
            else if (afqtScore >= 50)
                categoryDesc = "<color=#00FFAA>Category III-A (Average • Line Infantry & Armored Reconnaissance)</color>";
            else
                categoryDesc = "<color=#CCCCCC>Category III-B (Minimum Enlisted Qualifying Standard)</color>";

            GameEvents.OnFeedEventLogged?.Invoke("TESTING",
                $"<b>[MEPS / ASVAB VOCATIONAL ASSESSMENT]</b> Official Armed Forces Qualification Test (AFQT) certified at: " +
                $"<b>{afqtScore} / 99</b> ({categoryDesc})! Record placed in permanent service jacket.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        /// <summary>
        /// Evaluates child inherited cognitive and vocational aptitude based on parental background.
        /// Uses standard natural distribution without fantasy atavisms or meridian anomalies.
        /// </summary>
        public static int RollGeneticChildTalent(int parent1Aptitude, int parent2Aptitude)
        {
            float average = (parent1Aptitude + parent2Aptitude) / 2f;
            float roll = UnityEngine.Random.Range(-12f, 12f);
            return Mathf.Clamp(Mathf.RoundToInt(average + roll), 35, 99);
        }
    }
}
