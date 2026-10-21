using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class PowerCultivationEngine
    {
        /// <summary>
        /// Simulates 6 months of continuous cultivation resource burn and meridian absorption.
        /// Meridian damage severely penalizes absorption speed.
        /// </summary>
        /// <summary>
        /// Simulates 6 months of continuous cultivation resource burn and meridian absorption.
        /// Meridian damage severely penalizes absorption speed.
        /// Climbing difficulty increases with both Tier and Sub-Tier (Early -> Mid -> Late -> Peak).
        /// </summary>
        public static void ProcessCultivationTraining(SoldierProfile soldier)
        {
            ProcessSemesterCultivation(soldier, null);
        }

        public static void ProcessSemesterCultivation(SoldierProfile soldier, PowerTierSO currentTierSO)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired) return;
            if (soldier.powerTier >= 12 && soldier.subTier == PowerSubTier.Peak) return; // Apex divinity absolute pinnacle

            // Knowledge lock: Tiers 11 and 12 require unsealing the Secret Cave
            if (soldier.powerTier >= 10 && !soldier.unlockedTiers11And12)
            {
                return;
            }

            int baseMonthlyCost = (currentTierSO != null) ? currentTierSO.monthlyCashBurn : (soldier.powerTier == 0 ? 50 : 100 * soldier.powerTier);
            float subTierCostMultiplier = (soldier.powerTier > 0) ? (1.0f + ((int)soldier.subTier * 0.25f)) : (1.0f + ((soldier.preTierStage - 1) * 0.15f));
            int sixMonthCost = (int)(baseMonthlyCost * 6 * subTierCostMultiplier);

            if (soldier.cashBalance >= sixMonthCost)
            {
                soldier.cashBalance -= sixMonthCost;

                // Meridian damage penalty: high damage slows or halts cultivation
                float damageMultiplier = Mathf.Clamp01(1f - (soldier.meridianDamage / 100f));
                float talentMultiplier = Mathf.Clamp(soldier.talentScore / 100f, 0.4f, 3.5f);
                float fitnessFactor = Mathf.Clamp(soldier.fitness / 100f, 0.5f, 1.2f);

                // Climbing difficulty factor: scales with both major tier and sub-tier!
                float climbingFactor = (soldier.powerTier == 0)
                    ? (1.0f + ((soldier.preTierStage - 1) * 0.25f))
                    : PowerTierKnowledgeSystem.GetClimbingDifficultyFactor(soldier.powerTier, soldier.subTier);

                float baseProgress = 22f / climbingFactor;
                float progressGained = baseProgress * talentMultiplier * fitnessFactor * damageMultiplier;
                soldier.tierProgressPercent += progressGained;

                if (soldier.tierProgressPercent >= 100f)
                {
                    soldier.tierProgressPercent = 100f;
                    AttemptBreakthrough(soldier, currentTierSO);
                }
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("CULTIVATION",
                    "<color=#FF8800>[INSUFFICIENT RESOURCES]</color> You could not afford military-grade serums for this semester. Cultivation progress halted.");
            }
        }

        /// <summary>
        /// Bottleneck breakthrough trial.
        /// Handles Pre-Tier stages, Sub-Tier advancement (Early -> Mid -> Late -> Peak),
        /// and Major Realm Breakthroughs (Peak -> Next Tier Early).
        /// Repeated major failures cause meridian tears, crippling, and even violent death!
        /// </summary>
        public static bool AttemptBreakthrough(SoldierProfile soldier, PowerTierSO currentTierSO)
        {
            if (soldier == null || soldier.isDead) return false;

            // 1. Pre-Tier Mortal Stages (0 to Tier 1)
            if (soldier.powerTier == 0)
            {
                float stageDifficulty = 0.18f + ((soldier.preTierStage - 1) * 0.08f);
                float talentBonus = (soldier.talentScore / 200f) * 0.35f;
                float fitnessBonus = (soldier.fitness / 100f) * 0.15f;
                float meridianPenalty = (soldier.meridianDamage / 100f) * 0.30f;

                float successChance = Mathf.Clamp(0.50f + talentBonus + fitnessBonus - stageDifficulty - meridianPenalty, 0.05f, 0.95f);
                float roll = Random.Range(0f, 1f);

                if (roll <= successChance)
                {
                    soldier.tierProgressPercent = 0f;
                    soldier.failedBreakthroughCount = 0;
                    soldier.meridianDamage = Mathf.Max(0f, soldier.meridianDamage - 15f);

                    if (soldier.preTierStage < 4)
                    {
                        soldier.preTierStage++;
                        soldier.fitness = Mathf.Min(100f, soldier.fitness + 3f);
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2f);
                        GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                            $"<color=#00FFAA><b>[PRE-TIER STAGE ADVANCEMENT]</b></color> You completed <b>{PowerTierKnowledgeSystem.GetPreTierStageName(soldier.preTierStage - 1)}</b> and advanced to <b>{PowerTierKnowledgeSystem.GetPreTierStageName(soldier.preTierStage)}</b> (Stage {soldier.preTierStage}/4)! (+3 Fitness, +2 Combat)");
                    }
                    else
                    {
                        // Completes Stage 4 -> Transcends to Tier 1 (Early)!
                        soldier.powerTier = 1;
                        soldier.subTier = PowerSubTier.Early;
                        soldier.fitness = Mathf.Min(100f, soldier.fitness + 6f);
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 6f);
                        soldier.health = 100f;
                        GameEvents.OnPowerTierBreakthrough?.Invoke(1);
                        GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                            $"<color=#FFD700>==================================================\n" +
                            $"<b>[SUPER-SOLDIER TRANSCENDENCE • TIER 1 (MK I) ACHIEVED!]</b>\n" +
                            $"Having completed all 4 preparatory sub-dermal conditioning stages, your augmentations activated! You stepped into <b>Tier 1 Mk I ({PowerTierKnowledgeSystem.GetTierDesignation(1)})</b>!\n" +
                            $"==================================================</color>");
                    }

                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    return true;
                }
                else
                {
                    soldier.meridianDamage += 15f;
                    soldier.tierProgressPercent = 50f;
                    soldier.health = Mathf.Max(25f, soldier.health - 20f);
                    GameEvents.OnFeedEventLogged?.Invoke("BOTTLENECK",
                        $"<color=#FF6600>[SUB-DERMAL FRAMEWORK STRAIN]</color> Serum integration resisted! Stage {soldier.preTierStage} conditioning failed (+15% Neural Strain). Progress reset to 50%.");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    return false;
                }
            }

            // 2. Sub-Tier Advancement (Mk I -> Mk II -> Mk III -> Apex) within current Tier
            if (soldier.subTier < PowerSubTier.Peak)
            {
                float subDifficulty = PowerTierKnowledgeSystem.GetSubTierAdvancementDifficulty(soldier.powerTier, soldier.subTier);
                float talentBonus = (soldier.talentScore / 200f) * 0.35f;
                float fitnessBonus = (soldier.fitness / 100f) * 0.15f;
                float meridianPenalty = (soldier.meridianDamage / 100f) * 0.30f;

                float successChance = Mathf.Clamp(0.48f + talentBonus + fitnessBonus - subDifficulty - meridianPenalty, 0.05f, 0.92f);
                float roll = Random.Range(0f, 1f);

                if (roll <= successChance)
                {
                    PowerSubTier oldSubTier = soldier.subTier;
                    soldier.subTier = (PowerSubTier)((int)soldier.subTier + 1);
                    soldier.tierProgressPercent = 0f;
                    soldier.failedBreakthroughCount = 0;
                    soldier.meridianDamage = Mathf.Max(0f, soldier.meridianDamage - 10f);
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 2f);
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2f);

                    string guardianTag = PowerTierKnowledgeSystem.IsNationalGuardian(soldier.powerTier) ? " <color=#FFD700>[NATIONAL GUARDIAN]</color>" : "";
                    GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                        $"<color=#00FFAA><b>[AUGMENTATION UPGRADE]</b></color> Implant matrix synchronized! Advanced from <b>Tier {soldier.powerTier} ({PowerTierKnowledgeSystem.GetSubTierName(oldSubTier)})</b> to <b>Tier {soldier.powerTier} ({PowerTierKnowledgeSystem.GetSubTierName(soldier.subTier)}) • {PowerTierKnowledgeSystem.GetTierDesignation(soldier.powerTier)}</b>{guardianTag} (+2 Fitness, +2 Combat)!");

                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    return true;
                }
                else
                {
                    soldier.meridianDamage += 15f;
                    soldier.tierProgressPercent = 45f;
                    soldier.health = Mathf.Max(20f, soldier.health - 25f);
                    GameEvents.OnFeedEventLogged?.Invoke("BOTTLENECK",
                        $"<color=#FF8800>[IMPLANT CALIBRATION FAULT]</color> Neural feedback surged while attempting to calibrate to <b>{PowerTierKnowledgeSystem.GetSubTierName(soldier.subTier + 1)}</b>! Neural strain (+15% Toxicity). Progress set back to 45%.");

                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    return false;
                }
            }

            // 3. Major Realm Bottleneck Breakthrough (Peak -> Next Tier Early)
            if (soldier.powerTier >= 12)
            {
                soldier.tierProgressPercent = 100f;
                return true; // Already at absolute pinnacle
            }

            float majorDifficulty = (currentTierSO != null) ? currentTierSO.breakthroughDifficulty : (0.35f + soldier.powerTier * 0.05f);
            float majorTalentBonus = (soldier.talentScore / 200f) * 0.35f;
            float majorFitnessBonus = (soldier.fitness / 100f) * 0.15f;
            float majorMeridianPenalty = (soldier.meridianDamage / 100f) * 0.30f;

            float majorSuccessChance = Mathf.Clamp(0.35f + majorTalentBonus + majorFitnessBonus - majorDifficulty - majorMeridianPenalty, 0.05f, 0.88f);
            float majorRoll = Random.Range(0f, 1f);

            if (majorRoll <= majorSuccessChance)
            {
                soldier.powerTier++;
                soldier.subTier = PowerSubTier.Early; // Reset to Early in new tier!
                soldier.tierProgressPercent = 0f;
                soldier.failedBreakthroughCount = 0;
                soldier.meridianDamage = Mathf.Max(0f, soldier.meridianDamage - 25f);
                soldier.health = 100f;
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 6f);
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 6f);

                GameEvents.OnPowerTierBreakthrough?.Invoke(soldier.powerTier);

                if (soldier.powerTier == 7)
                {
                    soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 30f);
                    soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 25f);

                    GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                        $"<color=#FFD700>==================================================\n" +
                        $"<b>[NATIONAL GUARDIAN ASCENSION • TIER 7 (EARLY)]</b>\n" +
                        $"You have shattered the spoken mortal barrier! The Imperial Throne and High Command officially register you as a revered <b>NATIONAL GUARDIAN OF THE EMPIRE</b>!\n" +
                        $"Your name is inscribed upon the Great Monument of Valeria (+30 Merit, +25 Clan Favor).\n" +
                        $"==================================================</color>");
                }
                else
                {
                    string guardianTag = PowerTierKnowledgeSystem.IsNationalGuardian(soldier.powerTier) ? " <color=#FFD700>[NATIONAL GUARDIAN]</color>" : "";
                    GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                        $"<color=#00FFAA><b>[MAJOR TIER BREAKTHROUGH SUCCESS!]</b></color> Your core condensed into a new realm! You have successfully stepped into <b>Tier {soldier.powerTier} (Early) • {PowerTierKnowledgeSystem.GetTierDesignation(soldier.powerTier)}</b>!{guardianTag}");
                }

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }
            else
            {
                // Major Failure: Escalating Consequences
                soldier.failedBreakthroughCount++;

                if (soldier.failedBreakthroughCount == 1)
                {
                    soldier.tierProgressPercent = 35f;
                    soldier.meridianDamage += 30f;
                    soldier.health = Mathf.Max(25f, soldier.health - 35f);

                    GameEvents.OnFeedEventLogged?.Invoke("BOTTLENECK",
                        $"<color=#FF6600>[CALIBRATION FAULT - NEURAL STRAIN]</color> Augmentation barrier resisted! " +
                        $"Your neural pathways suffered severe strain (+30% Toxicity). Calibration progress set back to 35%.");
                }
                else if (soldier.failedBreakthroughCount == 2)
                {
                    soldier.tierProgressPercent = 15f;
                    soldier.meridianDamage += 45f;
                    soldier.fitness = Mathf.Max(20f, soldier.fitness - 20f);
                    soldier.health = Mathf.Max(10f, soldier.health - 45f);

                    GameEvents.OnFeedEventLogged?.Invoke("BOTTLENECK",
                        "<color=#FF2200><b>[CRITICAL REJECTION - NEURAL OVERHEAT!]</b></color> A violent electrical feedback surge ruptured your spinal implants! " +
                        "Neural toxicity critical. Another consecutive failure will be FATAL!");
                }
                else
                {
                    // 3rd+ Failure: FATAL ENERGY EXPLOSION OR PERMANENT CRIPPLING
                    float deathRoll = Random.Range(0f, 1f);
                    if (deathRoll < 0.65f || soldier.health <= 30f)
                    {
                        soldier.isDead = true;
                        soldier.health = 0f;
                        soldier.deathReason = $"Fatal Neural Core Overload attempting Tier {soldier.powerTier + 1} Augmentation";

                        GameEvents.OnFeedEventLogged?.Invoke("DEATH",
                            $"<color=#FF0000><b>[FATAL NEURAL CORE OVERLOAD]</b></color> Unable to stabilize the aetheric combat serum, your bio-cybernetic framework suffered catastrophic thermal meltdown! " +
                            $"You died at age {soldier.age:F1} during a high-risk augmentation trial.");

                        GameEvents.OnImperialExecutionTriggered?.Invoke(soldier.deathReason);
                    }
                    else
                    {
                        soldier.failedBreakthroughCount = 0;
                        if (soldier.powerTier > 1)
                        {
                            soldier.powerTier--;
                            soldier.subTier = PowerSubTier.Peak;
                        }
                        else
                        {
                            soldier.powerTier = 0;
                            soldier.preTierStage = 4;
                        }
                        soldier.meridianDamage = 90f;
                        soldier.fitness = 25f;
                        soldier.tierProgressPercent = 0f;

                        GameEvents.OnFeedEventLogged?.Invoke("CRIPPLED",
                            "<color=#AA0000><b>[AUGMENTATION COLLAPSE]</b></color> Your implant framework fractured! " +
                            "To save your life, military cyber-surgeons purged the burning modules. " +
                            $"You dropped down to Tier {soldier.powerTier} with 90% neural pathway scarring!");
                    }
                }

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return false;
            }
        }

        public static bool CheckSecretCaveAnomaly(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.hasDiscoveredSecretCave) return false;
            if (soldier.powerTier < 10) return false;

            // 1 in 100 chance during frontline tour
            if (Random.Range(0f, 100f) > 1.0f) return false;

            if (soldier.talentScore >= 180 && !soldier.hasCorruptedFoundation && soldier.meridianDamage < 20f)
            {
                soldier.hasDiscoveredSecretCave = true;
                soldier.unlockedTiers11And12 = true;

                GameEvents.OnSecretCaveDiscovered?.Invoke();
                GameEvents.OnFeedEventLogged?.Invoke("SECRET VAULT",
                    "<color=#FFD700><b>[CLASSIFIED PROTOTYPE VAULT UNSEALED]</b></color> Deep in the bombarded ruins of Sector 9, you breached an uncharted pre-war black-ops bunker! " +
                    "Recognizing your pristine neural pathways and 180+ Genetic Aptitude, the automated defense matrix authorized classified Singularity Operative blueprints, unlocking <b>Tier 11 (500-Year Life Extension)</b> and <b>Tier 12 (1,000-Year Life Extension)</b>!");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }

            return false;
        }
    }
}
