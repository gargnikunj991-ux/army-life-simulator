using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class MilitaryCareerEngine
    {
        /// <summary>
        /// Evaluates mandatory retirement / "up-or-out" policies.
        /// A 70-year-old cannot remain as an active Tier-2 frontline grunt!
        /// </summary>
        public static void EvaluateCareerRetirement(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired) return;

            // Guardians (Tier 7+) have expanded lifespans and serve as perpetual strategic pillars
            if (soldier.powerTier >= 7) return;

            int rankOrder = (soldier.currentRank != null) ? soldier.currentRank.rankOrder : 1;
            bool forceRetire = false;
            string retireReason = "";

            // 1. Junior Enlisted (E-1 to E-3): Mandatory retirement at age 38
            if (rankOrder <= 3 && soldier.age >= 38.0f)
            {
                forceRetire = true;
                retireReason = "You reached the mandatory age ceiling for junior enlisted grunts (Age 38) without earning an NCO promotion.";
            }
            // 2. Junior NCOs (E-4 to E-6): Mandatory retirement at age 48
            else if (rankOrder <= 6 && soldier.age >= 48.0f)
            {
                forceRetire = true;
                retireReason = "You reached the statutory service limit for non-commissioned officers (Age 48).";
            }
            // 3. Senior NCOs / Junior Officers (E-7 to E-9, O-1 to O-4): Mandatory retirement at age 58
            else if (rankOrder <= 13 && soldier.age >= 58.0f)
            {
                forceRetire = true;
                retireReason = "You concluded your 30-year military commission at age 58.";
            }
            // 4. Senior Officers / Colonels (O-5 to O-6): Mandatory retirement at age 65
            else if (soldier.age >= 65.0f && soldier.powerTier < 7)
            {
                forceRetire = true;
                retireReason = "You reached mandatory civilian retirement at age 65.";
            }

            // 5. Medical Invalidation (Meridian damage > 80%)
            if (soldier.meridianDamage >= 80f && rankOrder < 10)
            {
                forceRetire = true;
                retireReason = "The Medical Evaluation Board deemed your shattered meridians unfit for active combat duty.";
            }

            if (forceRetire)
            {
                soldier.isRetired = true;
                soldier.isInCombatDeployment = false;

                GameEvents.OnFeedEventLogged?.Invoke("RETIREMENT",
                    $"<color=#FFD700><b>[HONORABLE RETIREMENT & VETERAN PENSION]</b></color> {retireReason} " +
                    $"You were honorably discharged from active frontline service at age {soldier.age:F1}. " +
                    $"You now collect a veteran pension and return to guide your family estate.");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            }
        }

        /// <summary>
        /// Simulates frontline combat mortality and casualty risks during deployment tours.
        /// Low-tier grunts face brutal mortality against enemy heavy firepower!
        /// </summary>
        public static void ProcessCombatDeploymentTour(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired || !soldier.isInCombatDeployment) return;

            // Combat Power Score: Power Tier * 25 + Combat Skill * 0.5 + Fitness * 0.3
            float combatPower = (soldier.powerTier * 28f) + (soldier.combatSkill * 0.45f) + (soldier.fitness * 0.25f);
            float casualtyRoll = Random.Range(0f, 100f);

            // High risk for low combat power (e.g. Tier 1 grunt vs enemy artillery)
            float mortalityThreshold = Mathf.Clamp(18f - (combatPower * 0.12f), 1.0f, 25f);
            float injuryThreshold = Mathf.Clamp(40f - (combatPower * 0.20f), 5.0f, 50f);

            if (casualtyRoll < mortalityThreshold)
            {
                // KIA (KILLED IN ACTION)
                soldier.isDead = true;
                soldier.health = 0f;
                soldier.deathReason = "Killed in Action defending an ancient energy fortress against enemy vanguard shock-troops";

                GameEvents.OnFeedEventLogged?.Invoke("DEATH",
                    $"<color=#FF0000><b>[KILLED IN ACTION (KIA)]</b></color> An enemy heavy energy barrage breached your defensive line in Sector 4! " +
                    $"You fought bravely to the last breath, but were killed in action at age {soldier.age:F1}. " +
                    $"The Empire awarded you the Purple Heart and Valerian Star posthumously.");

                GameEvents.OnImperialExecutionTriggered?.Invoke(soldier.deathReason);
            }
            else if (casualtyRoll < injuryThreshold)
            {
                // BATTLEFIELD INJURY
                soldier.health = Mathf.Max(20f, soldier.health - 35f);
                soldier.meridianDamage = Mathf.Min(100f, soldier.meridianDamage + 25f);
                soldier.fitness = Mathf.Max(25f, soldier.fitness - 15f);

                GameEvents.OnFeedEventLogged?.Invoke("CASUALTY",
                    "<color=#FF4444>[BATTLEFIELD CASUALTY]</color> You were wounded during an enemy counter-attack! " +
                    "Shrapnel pierced your shoulder and bruised your energy meridians. Health and Fitness reduced.");
            }
            else
            {
                // GLORIOUS COMBAT VICTORY!
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + Random.Range(3f, 8f));
                soldier.meritRating = Mathf.Min(100f, soldier.meritRating + Random.Range(5f, 12f));
                soldier.combatMeritPoints += Random.Range(350, 850);
                soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 8f);

                GameEvents.OnFeedEventLogged?.Invoke("COMBAT",
                    $"<color=#00FFAA><b>[MISSION ACCOMPLISHED]</b></color> Your unit repelled the enemy assault on the energy well! " +
                    $"Combat Skill increased, earned +{Random.Range(350, 850)} CMP, and received high commendations from Battalion Command.");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }
    }
}
