using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class TalentTestingApparatus
    {
        /// <summary>
        /// Commoners cannot view their personal talent until reaching rank E-4 (Corporal/Specialist)
        /// to requisition the Army Resonance Chamber. Nobles view talent at birth.
        /// </summary>
        public static bool CanTestSelf(SoldierProfile soldier)
        {
            if (soldier == null) return false;
            if (soldier.nobleTitle > NobleRank.Commoner) return true; // Noble ancestral privilege
            if (soldier.currentRank == null) return false;
            return soldier.currentRank.unlocksSelfTalentTesting;       // E-4 clearance check
        }

        /// <summary>
        /// Commoners cannot test family members until reaching rank E-8 (Master Sergeant).
        /// </summary>
        public static bool CanTestFamily(SoldierProfile soldier)
        {
            if (soldier == null) return false;
            if (soldier.nobleTitle > NobleRank.Commoner) return true;
            if (soldier.currentRank == null) return false;
            return soldier.currentRank.unlocksFamilyTalentTesting;     // E-8 command privilege
        }

        public static bool PerformSelfApparatusTest(SoldierProfile soldier)
        {
            if (!CanTestSelf(soldier))
            {
                GameEvents.OnFeedEventLogged?.Invoke("DISCIPLINE", 
                    "<color=#FF4444>[CLEARANCE DENIED]</color> Military Regulation restricts testing apparatus access to NCOs (Rank E-4+). Your request was rejected.");
                return false;
            }

            soldier.isPersonalTalentRevealed = true;

            string talentClassification;
            if (soldier.talentScore >= 180) talentClassification = "<color=#FFD700>HEAVENLY TRANSCENDENT (S-Rank)</color>";
            else if (soldier.talentScore >= 150) talentClassification = "<color=#AA00FF>GUARDIAN SEED (A-Rank)</color>";
            else if (soldier.talentScore >= 100) talentClassification = "<color=#00AAFF>MILITARY PRODIGY (B-Rank)</color>";
            else if (soldier.talentScore >= 50) talentClassification = "<color=#00FF88>STANDARD SOLDIER (C-Rank)</color>";
            else talentClassification = "<color=#888888>MARROWLESS (D-Rank)</color>";

            GameEvents.OnFeedEventLogged?.Invoke("TESTING", 
                $"<b>[RESONANCE CHAMBER RESULTS]</b> Your spiritual-biological talent was officially measured at: <b>{soldier.talentScore} / 200</b> ({talentClassification})!");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        /// <summary>
        /// Generates a child's talent based on parental genetics with mutations.
        /// </summary>
        public static int RollGeneticChildTalent(int fatherTalent, int motherTalent)
        {
            float average = (fatherTalent + motherTalent) / 2f;
            float roll = Random.Range(0f, 100f);

            if (roll < 7.5f)
            {
                // Heavenly Atavism (Super Mutation): +40 to +70 spike
                int atavism = Mathf.RoundToInt(average + Random.Range(40, 71));
                return Mathf.Clamp(atavism, 0, 200);
            }
            else if (roll < 15.0f)
            {
                // Meridian Degeneration: -40 to -70 drop
                int drop = Mathf.RoundToInt(average - Random.Range(40, 71));
                return Mathf.Clamp(drop, 0, 200);
            }
            else if (roll < 30.0f)
            {
                // Moderate Shift: +/- 20
                return Mathf.Clamp(Mathf.RoundToInt(average + Random.Range(-20, 21)), 0, 200);
            }
            else
            {
                // Standard Inheritance (70%): +/- 10
                return Mathf.Clamp(Mathf.RoundToInt(average + Random.Range(-10, 11)), 0, 200);
            }
        }
    }
}
