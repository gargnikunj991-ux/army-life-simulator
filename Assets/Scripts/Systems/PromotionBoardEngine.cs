using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class PromotionBoardEngine
    {
        /// <summary>
        /// Evaluates military promotion points and convenes Promotion Boards every semester.
        /// Commoner soldiers climb through PT fitness, marksmanship qualification, combat tours, NCO reviews, and time-in-service!
        /// </summary>
        public static void EvaluateBoardPromotion(SoldierProfile soldier, MilitaryRankSO[] rankDatabase = null)
        {
            if (rankDatabase == null && Core.GameManager.Instance != null)
            {
                rankDatabase = Core.GameManager.Instance.militaryRankDatabase;
            }
            EvaluatePromotionBoard(soldier, rankDatabase);
        }

        public static void EvaluatePromotionBoard(SoldierProfile soldier, MilitaryRankSO[] rankDatabase)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired || soldier.isYouthPhase) return;
            if (rankDatabase == null || rankDatabase.Length == 0) return;

            // 1. Earn promotion points from military service this semester
            float ptPoints = (soldier.fitness / 100f) * 45f;         // 0-45 pts from physical fitness (PT test)
            float combatPoints = (soldier.combatSkill / 100f) * 55f; // 0-55 pts from weapon qualification
            float meritPoints = (soldier.meritRating / 100f) * 70f;  // 0-70 pts from official NCO evaluation
            float timePoints = 35f;                                  // 35 pts for 6 months time-in-grade
            float influenceBonus = (soldier.politicalInfluence / 100f) * 25f; // Lineage leverage

            int semesterPoints = Mathf.RoundToInt(ptPoints + combatPoints + meritPoints + timePoints + influenceBonus);
            
            // Special Forces operators gain accelerated tactical experience as they rank up
            if (soldier.isSpecialForces)
            {
                semesterPoints = Mathf.RoundToInt(semesterPoints * 1.30f);
            }

            soldier.accumulatedPromotionPoints += semesterPoints;

            // 2. Find next rank in progression
            MilitaryRankSO currentRank = soldier.currentRank;
            int currentOrder = (currentRank != null) ? currentRank.rankOrder : 0;
            MilitaryRankSO nextRank = GetNextRank(rankDatabase, currentOrder);

            if (nextRank == null) return; // Reached highest military rank (General)

            // Enlisted cannot cross to Officer without attending the 2-Year War Academy
            if (!soldier.isOfficer && nextRank.type == PaygradeType.Officer)
            {
                GameEvents.OnFeedEventLogged?.Invoke("CAREER APEX",
                    $"<color=#00FFAA><b>[ENLISTED CAREER APEX • {currentRank?.rankName?.ToUpper() ?? "COMMAND SERGEANT MAJOR"}]</b></color> " +
                    "You have reached the apex of the enlisted paygrades! To earn an Officer Commission (O-1+), " +
                    "regulations require completing the 2-Year Imperial War Academy.");
                return;
            }

            // 3. Time-in-Grade and Board Criteria Check
            int reqTime = (currentRank != null) ? GetRequiredTimeInGradeSemesters(currentRank.paygrade) : 1;
            bool meetsTimeInGrade = soldier.timeInCurrentRankSemesters >= reqTime;
            bool meetsPoints = soldier.accumulatedPromotionPoints >= nextRank.requiredMeritPoints;
            bool meetsFitness = soldier.fitness >= nextRank.requiredFitness;

            if (meetsPoints && meetsFitness && meetsTimeInGrade)
            {
                // CONVENE PROMOTION BOARD & PROMOTE!
                GameEvents.OnFeedEventLogged?.Invoke("PROMOTION BOARD",
                    $"<color=#00FFAA><b>[PROMOTION BOARD CONVENED]</b></color> The Battalion Promotion Board evaluated your service record: " +
                    $"<b>{soldier.accumulatedPromotionPoints} / {nextRank.requiredMeritPoints} Points</b> | Time-in-Grade: <b>{soldier.timeInCurrentRankSemesters * 0.5f:F1} yrs</b>! " +
                    $"You passed the board and have been recommended for promotion to {nextRank.rankName} ({nextRank.paygrade})! Promotion points reset to 0.");

                // Promotion points reset after each promotion!
                soldier.accumulatedPromotionPoints = 0;
                soldier.timeInCurrentRankSemesters = 0;

                GameManager.Instance.PromoteSoldier(nextRank);
            }
            else if (meetsPoints && meetsFitness && !meetsTimeInGrade)
            {
                float servedYears = soldier.timeInCurrentRankSemesters * 0.5f;
                float reqYears = reqTime * 0.5f;
                GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                    $"<color=#FFB300><b>[PROMOTION BOARD DEFERRED • TIME-IN-GRADE]</b></color> You have enough merit points and fitness, " +
                    $"but military regulations require at least <b>{reqYears:F1} years ({reqTime} semesters)</b> of service in rank {currentRank.rankName} ({currentRank.paygrade}) before board review. " +
                    $"(Currently served: {servedYears:F1} years).");
            }
            else if (meetsPoints && !meetsFitness)
            {
                GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                    $"<color=#FFB300><b>[PROMOTION BOARD DEFERRED • PT STANDARD]</b></color> You have enough merit points (<b>{soldier.accumulatedPromotionPoints} / {nextRank.requiredMeritPoints}</b>), " +
                    $"but your <b>Physical Fitness ({soldier.fitness:F0} / {nextRank.requiredFitness} required)</b> failed the military PT standard! " +
                    $"Improve your fitness to pass the board for {nextRank.rankName} ({nextRank.paygrade}).");
            }
            else
            {
                int remaining = Mathf.Max(0, nextRank.requiredMeritPoints - soldier.accumulatedPromotionPoints);
                string fitnessNote = (!meetsFitness) ? $" [Need {nextRank.requiredFitness} Fitness, currently {soldier.fitness:F0}]" : "";
                string timeNote = (!meetsTimeInGrade) ? $" [Served {soldier.timeInCurrentRankSemesters * 0.5f:F1}/{reqTime * 0.5f:F1} yrs in rank]" : "";
                GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                    $"[PROMOTION BOARD RECORD] Earned +{semesterPoints} promotion points this semester. " +
                    $"Current Total: <b>{soldier.accumulatedPromotionPoints} / {nextRank.requiredMeritPoints} Points</b> for {nextRank.rankName} ({nextRank.paygrade}). " +
                    $"({remaining} points needed for board{fitnessNote}{timeNote}).");
            }
        }

        public static int GetRequiredTimeInGradeSemesters(string currentPaygrade)
        {
            switch (currentPaygrade)
            {
                case "E-1": return 1; // 6 months in E-1 to promote to E-2
                case "E-2": return 2; // 1 year (2 semesters) in E-2 to promote to E-3
                case "E-3": return 3; // 1.5 years (3 semesters) in E-3 to promote to E-4 (Corporal)
                case "E-4": return 4; // 2 years (4 semesters) in E-4 to promote to E-5 (Sergeant)
                case "E-5": return 5; // 2.5 years (5 semesters) in E-5 to promote to E-6 (Staff Sergeant)
                case "E-6": return 6; // 3 years (6 semesters) in E-6 to promote to E-7
                case "E-7": return 6; // 3 years (6 semesters) in E-7 to promote to E-8
                case "E-8": return 6; // 3 years (6 semesters) in E-8 to promote to E-9
                case "O-1": return 3; // 1.5 years (3 semesters) in O-1 to promote to O-2 (1st Lt)
                case "O-2": return 4; // 2 years (4 semesters) in O-2 to promote to O-3 (Captain)
                default:    return 6; // 3 years (6 semesters) for O-3 through O-9
            }
        }

        private static MilitaryRankSO GetNextRank(MilitaryRankSO[] database, int currentOrder)
        {
            MilitaryRankSO bestNext = null;
            int lowestHigherOrder = int.MaxValue;

            foreach (var rank in database)
            {
                if (rank != null && rank.rankOrder > currentOrder && rank.rankOrder < lowestHigherOrder)
                {
                    lowestHigherOrder = rank.rankOrder;
                    bestNext = rank;
                }
            }
            return bestNext;
        }
    }
}
