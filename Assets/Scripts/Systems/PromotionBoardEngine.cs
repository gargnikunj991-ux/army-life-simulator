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
            float ptPoints = (soldier.fitness / 100f) * 35f;         // 0-35 pts from physical fitness (PT test)
            float combatPoints = (soldier.combatSkill / 100f) * 40f; // 0-40 pts from weapon qualification
            float meritPoints = (soldier.meritRating / 100f) * 50f;  // 0-50 pts from official NCO evaluation
            float timePoints = 25f;                                  // 25 pts for 6 months time-in-grade
            float influenceBonus = (soldier.politicalInfluence / 100f) * 15f; // Lineage leverage

            int semesterPoints = Mathf.RoundToInt(ptPoints + combatPoints + meritPoints + timePoints + influenceBonus);
            
            // Disciplinary consequences: Demerits, Article 15, and Brig records heavily penalize promotion points!
            int demeritCount = (soldier.disciplinaryRecords != null) ? soldier.disciplinaryRecords.Count : 0;
            demeritCount += soldier.article15PunishmentsCount;
            if (demeritCount > 0)
            {
                int demeritPenalty = demeritCount * 60;
                semesterPoints = Mathf.Max(0, semesterPoints - demeritPenalty);
                GameEvents.OnFeedEventLogged?.Invoke("DISCIPLINE",
                    $"<color=#FF5555><b>[PROMOTION BOARD PENALTY]</b></color> UCMJ disciplinary records ({demeritCount} demerits) deducted -{demeritPenalty} promotion points from your board review!");
            }

            // Pay forfeiture or barracks restriction freezes points
            if (soldier.hasForfeitedPayThisSemester || soldier.isBarracksRestricted)
            {
                semesterPoints = 0;
                GameEvents.OnFeedEventLogged?.Invoke("DISCIPLINE",
                    "<color=#FF4444><b>[PROMOTION PACKET FROZEN]</b></color> Active UCMJ non-judicial punishment froze your promotion packet (0 points earned this semester).");
            }

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

            // Check E-5 Automatic CO Recommendation for Officer School:
            // If the soldier is an E-5 Sergeant and made the right choices (clean discipline, high merit, smarts, fitness, respect)
            if (currentRank != null && currentRank.paygrade == "E-5" && !soldier.hasOfficerCandidateSchoolRecommendation)
            {
                bool cleanDiscipline = demeritCount == 0 && !soldier.hasFelonyDischargeRecord;
                bool exemplaryRecord = soldier.meritRating >= 75f && soldier.intellect >= 60f && soldier.fitness >= 65f && soldier.squadRespect >= 60f;
                bool eligibleAge = soldier.age <= 35.0f;

                if (cleanDiscipline && exemplaryRecord && eligibleAge)
                {
                    soldier.hasOfficerCandidateSchoolRecommendation = true;
                    soldier.AddTrait("CO Endorsement: Officer Candidate School (OCS)");
                    GameEvents.OnFeedEventLogged?.Invoke("CO RECOMMENDATION",
                        $"<color=#FFD700><b>[CO RECOMMENDATION • OFFICER CANDIDATE SCHOOL]</b></color> " +
                        $"By virtue of your spotless disciplinary record, tactical intellect, and fireteam leadership as an E-5 Sergeant, " +
                        $"your Company Commander has officially submitted a <b>Command Recommendation for Officer Candidate School (OCS)</b>! " +
                        $"Guaranteed board admission and 50% command tuition sponsorship unlocked.");
                }
            }

            // Check Great Merit: Direct Brevet / Merit Promotion or Exceptional Merit Pay Rise
            bool hasMonumentalMerit = (soldier.meritRating >= 95f && (soldier.combatMeritPoints >= 400 || soldier.HasTopValorMedal())) || soldier.combatMeritPoints >= 800;
            if (hasMonumentalMerit && nextRank != null && !soldier.isOfficer && nextRank.type == PaygradeType.Enlisted)
            {
                soldier.accumulatedPromotionPoints = 0;
                soldier.timeInCurrentRankSemesters = 0;
                GameEvents.OnFeedEventLogged?.Invoke("DIRECT PROMOTION",
                    $"<color=#FFD700><b>[DIRECT MERIT PROMOTION • MONUMENTAL BATTLEFIELD MERIT]</b></color> " +
                    $"In recognition of extraordinary combat merit ({soldier.meritRating:F0}% Merit Rating, {soldier.combatMeritPoints} CMP), " +
                    $"the High Military Council has bypassed all standard board wait times and awarded you a <b>Direct Merit Promotion</b> to {nextRank.rankName} ({nextRank.paygrade})!");

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PromoteSoldier(nextRank);
                }
                return;
            }

            if (nextRank == null) return; // Reached highest military rank (General)

            // Enlisted crossing to Officer:
            if (!soldier.isOfficer && nextRank.type == PaygradeType.Officer)
            {
                // Battlefield Commission (Mustang Officer Pathway)
                bool qualifiesForBattlefieldCommission = (soldier.HasMedal("Silver Cross") || soldier.HasMedal("Bronze Star") || soldier.HasMedal("Distinguished Service Cross") || soldier.HasTrait("Life Saver"))
                    && soldier.meritRating >= 65f
                    && soldier.combatSkill >= 55f
                    && (currentRank != null && currentRank.rankOrder >= 5); // Must be at least E-5 Sergeant

                if (qualifiesForBattlefieldCommission)
                {
                    soldier.AddTrait("Battlefield Commission (Mustang)");
                    GameEvents.OnFeedEventLogged?.Invoke("MUSTANG COMMISSION",
                        $"<color=#FFD700><b>[DIRECT BATTLEFIELD COMMISSION • MUSTANG OFFICER]</b></color> " +
                        $"By decree of High Command, in recognition of extraordinary battlefield gallantry and seasoned combat leadership under fire, " +
                        $"<b>{currentRank.rankName} {soldier.characterName}</b> is directly commissioned as a <b>Second Lieutenant (O-1)</b>! " +
                        $"You take command of a frontline rifle platoon with the deep respect of both enlisted grunts and senior officers.");

                    soldier.accumulatedPromotionPoints = 0;
                    soldier.timeInCurrentRankSemesters = 0;
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.PromoteSoldier(nextRank);
                    }
                    return;
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("CAREER APEX",
                        $"<color=#00FFAA><b>[ENLISTED CAREER APEX • {currentRank?.rankName?.ToUpper() ?? "COMMAND SERGEANT MAJOR"}]</b></color> " +
                        "You have reached the apex of the enlisted paygrades! To earn an Officer Commission (O-1+), " +
                        "regulations require completing the Imperial War Academy, having a CO Recommendation, or earning a Battlefield Commission through valorous medals (Silver Cross / Bronze Star / Life Saver) at E-5+.");
                    return;
                }
            }

            // 3. Time-in-Grade and Board Criteria Check (Scaled by Game Difficulty)
            int reqTime = (currentRank != null) ? GetRequiredTimeInGradeSemesters(currentRank.paygrade) : 1;
            bool meetsTimeInGrade = soldier.timeInCurrentRankSemesters >= reqTime;
            int scaledReqPoints = Mathf.RoundToInt(nextRank.requiredMeritPoints * DifficultySettings.PromotionPointsMultiplier);
            float scaledReqFitness = Mathf.Clamp(nextRank.requiredFitness + DifficultySettings.PromotionFitnessModifier, 35f, 95f);
            bool meetsPoints = soldier.accumulatedPromotionPoints >= scaledReqPoints;
            bool meetsFitness = soldier.fitness >= scaledReqFitness;

            // In Ironclad mode, zero tolerance: any demerit freezes promotion packet completely
            if (DifficultySettings.IsPermadeathForced && soldier.disciplinaryDemerits > 0)
            {
                GameEvents.OnFeedEventLogged?.Invoke("PROMOTION PACKET FROZEN",
                    $"<color=#FF4444><b>[IRONCLAD BOARD • PACKET FROZEN]</b></color> Ironclad regulations enforce zero tolerance on military discipline. " +
                    $"Your {soldier.disciplinaryDemerits} disciplinary demerit(s) disqualified your packet from board review.");
                return;
            }

            // Leadership and Discipline board standards: player choices have consequences!
            bool meetsDisciplineAndRespect = true;
            string leadershipDeficiency = "";
            if (nextRank.paygrade == "E-5")
            {
                if (soldier.squadRespect < 50f) { meetsDisciplineAndRespect = false; leadershipDeficiency += $"Squad Respect ({soldier.squadRespect:F0}/50% min) "; }
                if (soldier.discipline < 50f) { meetsDisciplineAndRespect = false; leadershipDeficiency += $"Discipline ({soldier.discipline:F0}/50% min) "; }
            }
            else if (nextRank.paygrade == "E-6")
            {
                if (soldier.squadRespect < 60f) { meetsDisciplineAndRespect = false; leadershipDeficiency += $"Squad Respect ({soldier.squadRespect:F0}/60% min) "; }
                if (soldier.discipline < 60f) { meetsDisciplineAndRespect = false; leadershipDeficiency += $"Discipline ({soldier.discipline:F0}/60% min) "; }
            }
            else if (nextRank.paygrade.StartsWith("E-7") || nextRank.paygrade.StartsWith("E-8") || nextRank.paygrade.StartsWith("E-9"))
            {
                if (soldier.squadRespect < 65f) { meetsDisciplineAndRespect = false; leadershipDeficiency += $"Squad Respect ({soldier.squadRespect:F0}/65% min) "; }
                if (soldier.discipline < 65f) { meetsDisciplineAndRespect = false; leadershipDeficiency += $"Discipline ({soldier.discipline:F0}/65% min) "; }
            }

            if (meetsPoints && meetsFitness && meetsTimeInGrade && meetsDisciplineAndRespect)
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
            else if (meetsPoints && meetsFitness && meetsTimeInGrade && !meetsDisciplineAndRespect)
            {
                GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                    $"<color=#FF5555><b>[PROMOTION BOARD DENIED • LEADERSHIP REVIEW]</b></color> You had enough merit points and time, " +
                    $"but the board noted serious leadership deficiencies: <b>{leadershipDeficiency}</b>! " +
                    $"Barracks and field choices matter. Improve your squad relations and discipline to earn command authority.");
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
