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
        public static void ProcessSemesterCultivation(SoldierProfile soldier, PowerTierSO currentTierSO)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired) return;
            if (soldier.powerTier >= 12) return; // Apex divinity

            // Knowledge lock: Tiers 11 and 12 require unsealing the Secret Cave
            if (soldier.powerTier >= 10 && !soldier.unlockedTiers11And12)
            {
                return;
            }

            int sixMonthCost = (currentTierSO != null) ? (currentTierSO.monthlyCashBurn * 6) : 600;

            if (soldier.cashBalance >= sixMonthCost)
            {
                soldier.cashBalance -= sixMonthCost;

                // Meridian damage penalty: high damage slows or halts cultivation
                float damageMultiplier = Mathf.Clamp01(1f - (soldier.meridianDamage / 100f));
                float talentMultiplier = Mathf.Clamp(soldier.talentScore / 100f, 0.4f, 3.5f);
                float fitnessFactor = Mathf.Clamp(soldier.fitness / 100f, 0.5f, 1.2f);

                float progressGained = 18f * talentMultiplier * fitnessFactor * damageMultiplier;
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
        /// High-stakes bottleneck breakthrough trial.
        /// Repeated failures cause meridian tears, crippling, and even violent death!
        /// </summary>
        public static bool AttemptBreakthrough(SoldierProfile soldier, PowerTierSO currentTierSO)
        {
            if (soldier == null || soldier.isDead) return false;

            float difficulty = (currentTierSO != null) ? currentTierSO.breakthroughDifficulty : 0.35f;
            float talentBonus = (soldier.talentScore / 200f) * 0.35f;
            float fitnessBonus = (soldier.fitness / 100f) * 0.15f;
            float meridianPenalty = (soldier.meridianDamage / 100f) * 0.30f;

            float successChance = Mathf.Clamp(0.40f + talentBonus + fitnessBonus - difficulty - meridianPenalty, 0.05f, 0.95f);
            float roll = Random.Range(0f, 1f);

            if (roll <= successChance)
            {
                // SUCCESS
                soldier.tierProgressPercent = 0f;
                soldier.failedBreakthroughCount = 0;
                soldier.meridianDamage = Mathf.Max(0f, soldier.meridianDamage - 20f); // Breakthrough cleanses impurities
                soldier.health = 100f;

                if (soldier.powerTier == 0)
                {
                    // Pre-Tier 1: 4 Preparatory Mortal Stages
                    if (soldier.preTierStage < 4)
                    {
                        soldier.preTierStage++;
                        GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                            $"<color=#00FFAA><b>[PRE-TIER STAGE ADVANCEMENT]</b></color> You completed <b>{PowerTierKnowledgeSystem.GetPreTierStageName(soldier.preTierStage - 1)}</b> and advanced to <b>{PowerTierKnowledgeSystem.GetPreTierStageName(soldier.preTierStage)}</b> (Stage {soldier.preTierStage}/4)!");
                    }
                    else
                    {
                        // Completes Stage 4 -> Steps into Tier 1!
                        soldier.powerTier = 1;
                        GameEvents.OnPowerTierBreakthrough?.Invoke(1);
                        GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                            $"<color=#FFD700>==================================================\n" +
                            $"<b>[MORTAL TRANSCENDENCE • TIER 1 ACHIEVED!]</b>\n" +
                            $"Having mastered all 4 preparatory stages, your meridian core ignited! You have officially stepped into <b>Tier 1 ({PowerTierKnowledgeSystem.GetTierDesignation(1)})</b>!\n" +
                            $"==================================================</color>");
                    }
                }
                else
                {
                    soldier.powerTier++;
                    GameEvents.OnPowerTierBreakthrough?.Invoke(soldier.powerTier);

                    if (soldier.powerTier == 7)
                    {
                        soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 30f);
                        soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 25f);

                        GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                            $"<color=#FFD700>==================================================\n" +
                            $"<b>[NATIONAL GUARDIAN ASCENSION • TIER 7]</b>\n" +
                            $"You have shattered the spoken mortal barrier! The Imperial Throne and High Command officially register you as a revered <b>NATIONAL GUARDIAN OF THE EMPIRE</b>!\n" +
                            $"Your name is inscribed upon the Great Monument of Valeria (+30 Merit, +25 Clan Favor).\n" +
                            $"==================================================</color>");
                    }
                    else
                    {
                        string guardianTag = PowerTierKnowledgeSystem.IsNationalGuardian(soldier.powerTier) ? " <color=#FFD700>[NATIONAL GUARDIAN]</color>" : "";
                        GameEvents.OnFeedEventLogged?.Invoke("BREAKTHROUGH",
                            $"<color=#00FFAA><b>[TIER BREAKTHROUGH SUCCESS!]</b></color> Your core condensed and your life aura surged! You have successfully stepped into <b>Tier {soldier.powerTier} ({PowerTierKnowledgeSystem.GetTierDesignation(soldier.powerTier)})</b>!{guardianTag}");
                    }
                }

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }
            else
            {
                // FAILURE WITH BRUTAL ESCALATING CONSEQUENCES
                soldier.failedBreakthroughCount++;

                if (soldier.failedBreakthroughCount == 1)
                {
                    // 1st Failure: Meridian Strain
                    soldier.tierProgressPercent = 40f;
                    soldier.meridianDamage += 30f;
                    soldier.health = Mathf.Max(25f, soldier.health - 35f);

                    GameEvents.OnFeedEventLogged?.Invoke("BOTTLENECK",
                        "<color=#FF6600>[BREAKTHROUGH FAILED - MERIDIAN STRAIN]</color> Atmospheric energy recoiled! " +
                        "Your meridians suffered heavy strain (Damage +30%). Progress set back to 40%.");
                }
                else if (soldier.failedBreakthroughCount == 2)
                {
                    // 2nd Failure: Severe Meridian Tear
                    soldier.tierProgressPercent = 15f;
                    soldier.meridianDamage += 45f;
                    soldier.fitness = Mathf.Max(20f, soldier.fitness - 20f);
                    soldier.health = Mathf.Max(10f, soldier.health - 45f);

                    GameEvents.OnFeedEventLogged?.Invoke("BOTTLENECK",
                        "<color=#FF2200><b>[SEVERE RECOIL - MERIDIANS TORN!]</b></color> A violent energy storm erupted inside your chest! " +
                        "Your meridians ruptured. Another consecutive failure will be FATAL!");
                }
                else
                {
                    // 3rd+ Failure: FATAL ENERGY EXPLOSION OR PERMANENT CRIPPLING
                    float deathRoll = Random.Range(0f, 1f);
                    if (deathRoll < 0.65f || soldier.health <= 30f)
                    {
                        // FATAL DETONATION
                        soldier.isDead = true;
                        soldier.health = 0f;
                        soldier.deathReason = $"Violent Energy Explosion attempting Tier {soldier.powerTier + 1} breakthrough";

                        GameEvents.OnFeedEventLogged?.Invoke("DEATH",
                            $"<color=#FF0000><b>[FATAL MERIDIAN DETONATION]</b></color> Unable to contain the raging celestial power, your core violently exploded! " +
                            $"You died at age {soldier.age:F1} during a desperate breakthrough attempt.");

                        GameEvents.OnImperialExecutionTriggered?.Invoke(soldier.deathReason);
                    }
                    else
                    {
                        // PERMANENT CRIPPLING: Drops a Tier
                        soldier.failedBreakthroughCount = 0;
                        soldier.powerTier = Mathf.Max(0, soldier.powerTier - 1);
                        soldier.meridianDamage = 90f;
                        soldier.fitness = 25f;
                        soldier.tierProgressPercent = 0f;

                        GameEvents.OnFeedEventLogged?.Invoke("CRIPPLED",
                            "<color=#AA0000><b>[CULTIVATION COLLAPSED]</b></color> Your foundation fractured! " +
                            "To save your life, the military hospital burned away your damaged channels. " +
                            $"You permanently dropped to Tier {soldier.powerTier} with 90% meridian scarring!");
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
                GameEvents.OnFeedEventLogged?.Invoke("SECRET CAVE",
                    "<color=#FFD700><b>[ANCIENT SECT REVELATION]</b></color> In the ruins of Sector 9, you breached the Primordial Sect Sanctuary! " +
                    "Recognizing your pristine meridians and 180+ Heavenly Talent, the ancient spirit array granted you the Primordial Manual, unlocking <b>Tier 11 (500 Years)</b> and <b>Tier 12 (1,000 Years)</b>!");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }

            return false;
        }
    }
}
