using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Firing Range Marksmanship Qualification Engine.
    /// Manages 10-target timed trials across 50m, 100m, 200m, and 300m lanes,
    /// evaluates trigger reaction timing, marksman accuracy, shoot/no-shoot decision making,
    /// and awards military qualification badges (Marksman, Sharpshooter, Expert).
    /// </summary>
    public static class FiringRangeEngine
    {
        private static readonly System.Random rng = new System.Random();

        /// <summary>
        /// Generates a standardized 10-target military rifle qualification course.
        /// Includes standard hostile silhouettes, high-value hostile officers, and no-shoot friendly/hostage targets.
        /// </summary>
        public static List<TargetPopup> GenerateTrialTargets(int trialCount = 10, int seed = 0)
        {
            var rand = (seed != 0) ? new System.Random(seed) : rng;
            var list = new List<TargetPopup>();

            // Default distances and windows
            float[] distances = { 50f, 100f, 200f, 300f };

            for (int i = 0; i < trialCount; i++)
            {
                int targetId = i + 1;
                float dist;
                float window;
                TargetType type;
                int points;
                string desc;

                // Design trial distribution:
                // Target 4 or 8: Friendly civilian / hostage (Shoot/No-Shoot test)
                if (i == 3)
                {
                    dist = 100f;
                    window = 2.5f;
                    type = TargetType.FriendlyCivilian;
                    points = 10; // Awarded for HOLDING fire; -25 penalty if shot!
                    desc = "Friendly non-combatant civilian holding field ration crates. [DO NOT FIRE!]";
                }
                else if (i == 7)
                {
                    dist = 200f;
                    window = 2.2f;
                    type = TargetType.FriendlyHostage;
                    points = 15; // Awarded for HOLDING fire; -25 penalty if shot!
                    desc = "Friendly allied prisoner positioned in crossfire. [DO NOT FIRE!]";
                }
                else if (i == 9)
                {
                    // Target 10: High value enemy officer at 300m
                    dist = 300f;
                    window = 3.0f;
                    type = TargetType.HostileOfficer;
                    points = 40;
                    desc = "Enemy field commander wearing officer peaked cap at long distance.";
                }
                else
                {
                    // Standard hostile popups
                    int distIndex = rand.Next(0, distances.Length);
                    dist = distances[distIndex];

                    if (dist <= 50f)
                    {
                        window = 1.4f;
                        points = 10;
                        desc = "Close-range enemy infantry popup silhouette (50m).";
                    }
                    else if (dist <= 100f)
                    {
                        window = 2.0f;
                        points = 20;
                        desc = "Mid-range hostile rifleman popup silhouette (100m).";
                    }
                    else if (dist <= 200f)
                    {
                        window = 2.6f;
                        points = 30;
                        desc = "Long-range hostile machine-gunner silhouette (200m).";
                    }
                    else
                    {
                        window = 3.2f;
                        points = 35;
                        desc = "Extreme-range concealed enemy sniper silhouette (300m).";
                    }

                    type = (i == 5) ? TargetType.HostileOfficer : TargetType.HostileSilhouette;
                    if (type == TargetType.HostileOfficer) points += 10;
                }

                list.Add(new TargetPopup(targetId, dist, window, type, points, desc));
            }

            return list;
        }

        /// <summary>
        /// Calculates the maximum theoretical score achievable on a given set of targets.
        /// </summary>
        public static int CalculateMaxScore(List<TargetPopup> targets)
        {
            if (targets == null || targets.Count == 0) return 200;
            int sum = 0;
            foreach (var t in targets)
            {
                sum += t.pointValue;
            }
            return sum;
        }

        /// <summary>
        /// Evaluates a single target shot or hold-fire action.
        /// </summary>
        public static FiringTrialResult EvaluateShot(
            TargetPopup target,
            SoldierProfile soldier,
            bool playerFired,
            float customReactionTime = -1f)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var result = new FiringTrialResult
            {
                target = target,
                heldFire = !playerFired
            };

            // CASE 1: FRIENDLY TARGET
            if (target.IsFriendly)
            {
                if (playerFired)
                {
                    // Friendly fire violation!
                    result.targetHit = true;
                    result.wasFriendlyFiredUpon = true;
                    result.pointsAwarded = -25; // Severe penalty
                    result.reactionTimeSeconds = (customReactionTime > 0f) ? customReactionTime : 0.8f;
                    result.feedback = "FRIENDLY FIRE VIOLATION! You fired upon a non-combatant friendly silhouette!";
                }
                else
                {
                    // Successfully held fire! Discipline demonstrated
                    result.targetHit = false;
                    result.wasFriendlyFiredUpon = false;
                    result.pointsAwarded = target.pointValue; // +10 or +15 points
                    result.reactionTimeSeconds = 0f;
                    result.feedback = "TARGET IDENTIFIED: FRIENDLY NON-COMBATANT. Fire held with discipline.";
                }
                return result;
            }

            // CASE 2: HOSTILE TARGET, BUT PLAYER HELD FIRE (TIMEOUT / HESITATION)
            if (!playerFired)
            {
                result.targetHit = false;
                result.pointsAwarded = 0;
                result.reactionTimeSeconds = target.popupWindowSeconds;
                result.feedback = "TARGET MISSED: Weapon not fired before target retracted into trench.";
                return result;
            }

            // CASE 3: HOSTILE TARGET, SHOT WAS FIRED
            // Determine reaction time
            float reactionTime;
            if (customReactionTime > 0f)
            {
                reactionTime = customReactionTime;
            }
            else
            {
                // Simulated reaction time: base 1.5s - fitness and combat skill modifiers
                float fitnessMod = (soldier != null) ? (soldier.physicalFitness / 100f) * 0.4f : 0.2f;
                float combatMod = (soldier != null) ? (soldier.combatSkill / 100f) * 0.4f : 0.2f;
                float mosMod = 0f;

                if (soldier != null)
                {
                    var mos = MOSRegistry.GetSoldierMOS(soldier);
                    if (mos == MOSSpecialization.SniperRecon) mosMod = 0.35f;
                }

                float simulatedBase = UnityEngine.Random.Range(0.7f, 1.4f);
                reactionTime = Mathf.Max(0.35f, simulatedBase - (fitnessMod + combatMod + mosMod));
            }

            result.reactionTimeSeconds = reactionTime;

            // Check if reaction was too late (target already dropped)
            if (reactionTime > target.popupWindowSeconds)
            {
                result.targetHit = false;
                result.pointsAwarded = 0;
                result.feedback = $"TRIGGER PULLED TOO LATE: Reaction ({reactionTime:F2}s) exceeded popup window ({target.popupWindowSeconds:F1}s).";
                return result;
            }

            // Determine accuracy / hit probability
            // Base hit chance decreases with distance
            float baseAccuracy = 1.0f - (target.distanceMeters / 600f); // 50m = 0.92, 100m = 0.83, 200m = 0.67, 300m = 0.50
            if (soldier != null)
            {
                baseAccuracy += (soldier.marksmanshipScore / 100f) * 0.35f;
                baseAccuracy += (soldier.combatSkill / 100f) * 0.15f;

                // MOS Sniper bonus
                var mos = MOSRegistry.GetSoldierMOS(soldier);
                var def = MOSRegistry.GetMOS(mos);
                baseAccuracy += def.marksmanshipBonus * 0.01f; // +25% for Sniper
            }

            // High reaction speed slightly improves point accuracy
            float speedBonus = Mathf.Clamp01((target.popupWindowSeconds - reactionTime) / target.popupWindowSeconds);
            baseAccuracy += speedBonus * 0.10f;

            bool isHit = UnityEngine.Random.value <= baseAccuracy || baseAccuracy >= 0.98f;

            if (isHit)
            {
                result.targetHit = true;
                result.pointsAwarded = target.pointValue;
                result.feedback = $"BULLSEYE HIT! Target at {target.distanceMeters:F0}m struck cleanly in {reactionTime:F2}s (+{target.pointValue} pts).";
            }
            else
            {
                result.targetHit = false;
                result.pointsAwarded = 0;
                result.feedback = $"ROUND WIDE! Bullet kicked up dirt behind {target.distanceMeters:F0}m lane (0 pts).";
            }

            return result;
        }

        /// <summary>
        /// Calculates qualification tier and rewards based on points achieved.
        /// Qualification Tiers:
        /// - Unqualified: < 50%
        /// - Marksman: 50% - 74%
        /// - Sharpshooter: 75% - 89%
        /// - Expert: 90% - 100%
        /// </summary>
        public static FiringRangeQualificationResult CalculateQualification(
            int totalScore,
            int maxPossibleScore,
            List<FiringTrialResult> trials = null)
        {
            if (maxPossibleScore <= 0) maxPossibleScore = 200;

            float scorePercentage = Mathf.Clamp01((float)totalScore / maxPossibleScore) * 100f;
            QualificationTier tier;
            string badge;
            long cashPrize;
            int promoPoints;
            float statGain;
            string summary;

            if (scorePercentage >= 90f)
            {
                tier = QualificationTier.Expert;
                badge = "Expert Marksmanship Badge";
                cashPrize = 300L;
                promoPoints = 15;
                statGain = 6f;
                summary = "OUTSTANDING EXPERT QUALIFICATION! Flawless target recognition, rapid grouping, and steel composure under timer.";
            }
            else if (scorePercentage >= 75f)
            {
                tier = QualificationTier.Sharpshooter;
                badge = "Sharpshooter Qualification Badge";
                cashPrize = 150L;
                promoPoints = 10;
                statGain = 4f;
                summary = "SHARPSHOOTER QUALIFICATION CONFIRMED! Reliable engagement across 200m and 300m lanes with controlled trigger cadence.";
            }
            else if (scorePercentage >= 50f)
            {
                tier = QualificationTier.Marksman;
                badge = "Marksman Qualification Badge";
                cashPrize = 50L;
                promoPoints = 5;
                statGain = 2.5f;
                summary = "STANDARD MARKSMAN QUALIFIED. Passed baseline garrison rifle marksmanship qualification requirements.";
            }
            else
            {
                tier = QualificationTier.Unqualified;
                badge = "None";
                cashPrize = 0L;
                promoPoints = 0;
                statGain = 1f;
                summary = "UNQUALIFIED (FAILURE). Score fell below imperial garrison standards. Ordered to report for remedial rifle drills.";
            }

            return new FiringRangeQualificationResult
            {
                totalScore = totalScore,
                maxPossibleScore = maxPossibleScore,
                scorePercentage = scorePercentage,
                tier = tier,
                badgeAwarded = badge,
                cashPrizeBonus = cashPrize,
                promotionPointsAwarded = promoPoints,
                marksmanshipScoreGained = statGain,
                trials = trials ?? new List<FiringTrialResult>(),
                certificateSummary = summary
            };
        }

        /// <summary>
        /// Simulates a complete 10-target qualification course for a soldier.
        /// </summary>
        public static FiringRangeQualificationResult RunFullQualificationRound(
            SoldierProfile soldier,
            List<TargetPopup> targets = null,
            int seed = 0)
        {
            if (targets == null || targets.Count == 0)
            {
                targets = GenerateTrialTargets(10, seed);
            }

            int totalScore = 0;
            int maxScore = CalculateMaxScore(targets);
            var trialResults = new List<FiringTrialResult>();

            foreach (var target in targets)
            {
                // Intelligent AI / player decision: do not fire on friendly civilian / hostage
                bool shouldFire = target.IsHostile;
                var trial = EvaluateShot(target, soldier, playerFired: shouldFire);
                trialResults.Add(trial);
                totalScore += trial.pointsAwarded;
            }

            // Prevent negative total score
            if (totalScore < 0) totalScore = 0;

            var qualResult = CalculateQualification(totalScore, maxScore, trialResults);

            // Apply awards to soldier if provided
            if (soldier != null)
            {
                soldier.marksmanshipScore = Mathf.Clamp(soldier.marksmanshipScore + qualResult.marksmanshipScoreGained, 0f, 100f);
                soldier.accumulatedPromotionPoints += qualResult.promotionPointsAwarded;
                soldier.cashBalance += qualResult.cashPrizeBonus;

                if (qualResult.badgeAwarded != "None")
                {
                    soldier.AddMedal(qualResult.badgeAwarded);
                }

                string colorTag = qualResult.tier switch
                {
                    QualificationTier.Expert => "#00FFAA",
                    QualificationTier.Sharpshooter => "#00D4FF",
                    QualificationTier.Marksman => "#FFD700",
                    _ => "#FF4444"
                };

                GameEvents.OnFeedEventLogged?.Invoke("MARKSMANSHIP",
                    $"<color={colorTag}><b>[FIRING RANGE • {qualResult.tier.ToString().ToUpper()}]</b></color> Score: <b>{qualResult.totalScore}/{qualResult.maxPossibleScore} ({qualResult.scorePercentage:F0}%)</b>. " +
                    $"Badge: <b>{qualResult.badgeAwarded}</b> | Cash: <b>+${qualResult.cashPrizeBonus:N0}</b> | Promotion: <b>+{qualResult.promotionPointsAwarded} pts</b>.");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            }

            return qualResult;
        }
    }
}
