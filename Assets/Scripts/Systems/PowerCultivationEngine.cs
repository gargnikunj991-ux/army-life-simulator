using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Grounded military physical conditioning, tactical drills, and physical fitness test (APFT) engine.
    /// Completely replaces fantasy cultivation loops, Qi, and meridians with realistic military calisthenics,
    /// obstacle courses, marksmanship drills, and endurance training.
    /// </summary>
    public static class MilitaryPhysicalConditioningEngine
    {
        public const int SEMESTER_CONDITIONING_SUPPLEMENTS_COST = 50;

        /// <summary>
        /// Simulates 6 months of active military physical conditioning, PT sessions, and drill training.
        /// </summary>
        public static void ProcessPhysicalConditioning(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired) return;

            if (soldier.cashBalance >= SEMESTER_CONDITIONING_SUPPLEMENTS_COST)
            {
                soldier.cashBalance -= SEMESTER_CONDITIONING_SUPPLEMENTS_COST;
                soldier.physicalFitness = Mathf.Min(100f, soldier.physicalFitness + 4f);
                soldier.combatReadiness = Mathf.Min(100f, soldier.combatReadiness + 5f);
                soldier.discipline = Mathf.Min(100f, soldier.discipline + 2f);
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2f);
                soldier.health = Mathf.Min(100f, soldier.health + 3f);

                GameEvents.OnFeedEventLogged?.Invoke("CONDITIONING",
                    $"<color=#00FFAA><b>[MILITARY PHYSICAL CONDITIONING]</b></color> You maintained strict physical drill discipline: " +
                    $"morning ruck marches, calisthenics, and obstacle runs. " +
                    $"(Fitness: <b>{soldier.physicalFitness:F0}/100</b>, Readiness: <b>{soldier.combatReadiness:F0}/100</b>).");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            }
            else
            {
                // Can't afford extra gym / nutrition gear, but still trains on standard mess rations
                soldier.physicalFitness = Mathf.Min(100f, soldier.physicalFitness + 1.5f);
                soldier.combatReadiness = Mathf.Min(100f, soldier.combatReadiness + 2f);
                GameEvents.OnFeedEventLogged?.Invoke("CONDITIONING",
                    "<b>[BARRACKS CALISTHENICS]</b> Lacking extra funds for athletic supplements, you completed standard bodyweight drills in the barracks courtyard.");
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            }
        }

        /// <summary>
        /// Conducts a formal Army Physical Fitness Test (APFT) or tactical qualification trial.
        /// </summary>
        public static bool ConductFitnessQualificationTest(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return false;

            float baseScore = (soldier.physicalFitness * 0.5f) + (soldier.discipline * 0.3f) + (soldier.combatReadiness * 0.2f);
            float roll = UnityEngine.Random.Range(0f, 100f);

            if (roll < baseScore)
            {
                soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 5f);
                soldier.promotionPoints += 5;
                soldier.physicalFitness = Mathf.Min(100f, soldier.physicalFitness + 3f);
                soldier.combatReadiness = Mathf.Min(100f, soldier.combatReadiness + 4f);

                GameEvents.OnFeedEventLogged?.Invoke("QUALIFICATION",
                    $"<color=#00FFAA><b>[PT TEST EXCELLENCE]</b></color> You passed the Army Physical Fitness Test with flying colors! " +
                    $"(+5 Merit, +5 Promotion Points, Fitness: <b>{soldier.physicalFitness:F0}/100</b>).");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }
            else
            {
                soldier.health = Mathf.Max(20f, soldier.health - 5f);
                soldier.discipline = Mathf.Min(100f, soldier.discipline + 1f);

                GameEvents.OnFeedEventLogged?.Invoke("QUALIFICATION",
                    "<color=#FF8800><b>[REMEDIAL PT REQUIRED]</b></color> You struggled on the timed 2-mile run during the APFT. " +
                    "Your squad leader assigned 2 weeks of remedial physical drill.");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return false;
            }
        }
    }

    /// <summary>
    /// Deprecated cultivation wrapper retained for backwards compatibility with legacy callers.
    /// Redirects internally to grounded military conditioning and qualification systems.
    /// </summary>
    [Obsolete("PowerCultivationEngine is deprecated. Use MilitaryPhysicalConditioningEngine.")]
    public static class PowerCultivationEngine
    {
        public static void ProcessCultivationTraining(SoldierProfile soldier)
        {
            MilitaryPhysicalConditioningEngine.ProcessPhysicalConditioning(soldier);
        }

        public static void ProcessSemesterCultivation(SoldierProfile soldier, PowerTierSO currentTierSO)
        {
            MilitaryPhysicalConditioningEngine.ProcessPhysicalConditioning(soldier);
        }

        public static bool AttemptBreakthrough(SoldierProfile soldier, PowerTierSO currentTierSO)
        {
            if (soldier == null || soldier.isDead) return false;

            // In legacy pre-tier stages (0 to 1)
            if (soldier.powerTier == 0)
            {
                soldier.tierProgressPercent = 0f;
                soldier.failedBreakthroughCount = 0;
                soldier.meridianDamage = 0f;

                if (soldier.preTierStage < 4)
                {
                    soldier.preTierStage++;
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 3f);
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2f);
                    soldier.combatReadiness = Mathf.Min(100f, soldier.combatReadiness + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("QUALIFICATION",
                        $"<color=#00FFAA><b>[PHYSICAL CONDITIONING ADVANCEMENT]</b></color> Completed Stage {soldier.preTierStage - 1} conditioning and advanced to Stage {soldier.preTierStage}/4! (+3 Fitness, +2 Combat).");
                }
                else
                {
                    soldier.powerTier = 1;
                    soldier.subTier = PowerSubTier.Early;
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 6f);
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 6f);
                    soldier.combatReadiness = Mathf.Min(100f, soldier.combatReadiness + 10f);
                    soldier.health = 100f;
                    GameEvents.OnPowerTierBreakthrough?.Invoke(1);
                    GameEvents.OnFeedEventLogged?.Invoke("QUALIFICATION",
                        $"<color=#FFD700>==================================================\n" +
                        $"<b>[ACTIVE DUTY READINESS • FULL QUALIFICATION ACHIEVED]</b>\n" +
                        $"Having completed all preparatory physical conditioning stages, you are certified for active field operations!\n" +
                        $"==================================================</color>");
                }

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }

            // Sub-Tier Advancement
            if (soldier.subTier < PowerSubTier.Peak)
            {
                soldier.subTier = (PowerSubTier)((int)soldier.subTier + 1);
                soldier.tierProgressPercent = 0f;
                soldier.failedBreakthroughCount = 0;
                soldier.meridianDamage = 0f;
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 2f);
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2f);
                soldier.combatReadiness = Mathf.Min(100f, soldier.combatReadiness + 5f);

                GameEvents.OnFeedEventLogged?.Invoke("QUALIFICATION",
                    $"<color=#00FFAA><b>[COMBAT READINESS ADVANCEMENT]</b></color> Advanced to <b>Tier {soldier.powerTier} ({PowerTierKnowledgeSystem.GetSubTierName(soldier.subTier)})</b> (+2 Fitness, +2 Combat)!");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }

            // Major Tier Advancement
            soldier.powerTier++;
            soldier.subTier = PowerSubTier.Early;
            soldier.tierProgressPercent = 0f;
            soldier.failedBreakthroughCount = 0;
            soldier.meridianDamage = 0f;
            soldier.health = 100f;
            soldier.fitness = Mathf.Min(100f, soldier.fitness + 5f);
            soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
            soldier.combatReadiness = Mathf.Min(100f, soldier.combatReadiness + 10f);

            GameEvents.OnPowerTierBreakthrough?.Invoke(soldier.powerTier);
            GameEvents.OnFeedEventLogged?.Invoke("QUALIFICATION",
                $"<color=#00FFAA><b>[ELITE QUALIFICATION ACHIEVED]</b></color> You passed advanced military evaluation for Tier {soldier.powerTier} standing!");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        public static bool CheckSecretCaveAnomaly(SoldierProfile soldier)
        {
            // Grounded sim: No secret cultivation caves
            return false;
        }
    }
}
