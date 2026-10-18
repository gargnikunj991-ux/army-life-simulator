using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class PromotionBoardEngine
    {
        /// <summary>
        /// Evaluates military promotion points and convenes Promotion Boards every semester.
        /// Commoner soldiers climb through PT fitness, combat tours, NCO reviews, and time-in-service!
        /// Cultivation is NOT required for enlisted promotions!
        /// </summary>
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
            soldier.accumulatedPromotionPoints += semesterPoints;

            // 2. Find next rank in progression
            MilitaryRankSO currentRank = soldier.currentRank;
            int currentOrder = (currentRank != null) ? currentRank.rankOrder : 0;
            MilitaryRankSO nextRank = GetNextRank(rankDatabase, currentOrder);

            if (nextRank == null) return; // Reached highest military rank (General)

            // 3. Board Criteria Check
            bool meetsPoints = soldier.accumulatedPromotionPoints >= nextRank.requiredMeritPoints;
            bool meetsFitness = soldier.fitness >= nextRank.requiredFitness;

            if (meetsPoints && meetsFitness)
            {
                // CONVENE PROMOTION BOARD & PROMOTE!
                GameEvents.OnFeedEventLogged?.Invoke("PROMOTION BOARD",
                    $"<color=#00FFAA><b>[PROMOTION BOARD CONVENED]</b></color> The Battalion Promotion Board evaluated your service record: " +
                    $"<b>{soldier.accumulatedPromotionPoints} / {nextRank.requiredMeritPoints} Points</b>! " +
                    $"You passed the board and have been recommended for promotion to {nextRank.rankName} ({nextRank.paygrade})!");

                GameManager.Instance.PromoteSoldier(nextRank);
            }
            else if (meetsPoints && !meetsFitness)
            {
                GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                    $"<color=#FFB300><b>[PROMOTION BOARD DEFERRED]</b></color> You have enough merit points (<b>{soldier.accumulatedPromotionPoints} / {nextRank.requiredMeritPoints}</b>), " +
                    $"but your <b>Physical Fitness ({soldier.fitness:F0} / {nextRank.requiredFitness} required)</b> failed the military PT standard! " +
                    $"Improve your fitness to pass the board for {nextRank.rankName} ({nextRank.paygrade}).");
            }
            else
            {
                int remaining = Mathf.Max(0, nextRank.requiredMeritPoints - soldier.accumulatedPromotionPoints);
                string fitnessNote = (!meetsFitness) ? $" [Need {nextRank.requiredFitness} Fitness, currently {soldier.fitness:F0}]" : "";
                GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                    $"[PROMOTION BOARD RECORD] Earned +{semesterPoints} promotion points this semester. " +
                    $"Current Total: <b>{soldier.accumulatedPromotionPoints} / {nextRank.requiredMeritPoints} Points</b> for {nextRank.rankName} ({nextRank.paygrade}). " +
                    $"({remaining} points needed for board{fitnessNote}).");
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
