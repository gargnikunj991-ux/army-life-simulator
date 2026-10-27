using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Combat Trauma, PTSD, Survivor's Guilt & Veteran Reintegration Engine:
    /// Tracks psychological combat stress (0 to 100).
    /// Squadmate deaths and severe trauma cause shell shock, flashbacks, and survivor's guilt.
    /// Soldiers heal through unit memorial visits, VA counseling, and comrade reunions.
    /// </summary>
    public static class CombatTraumaEngine
    {
        public const float PTSD_THRESHOLD = 60f;
        public const float CRITICAL_STRESS_THRESHOLD = 85f;

        /// <summary>
        /// Records combat stress from artillery concussions, wounds, and close calls.
        /// </summary>
        public static void RecordTraumaEvent(SoldierProfile soldier, float stressAmount, string reason)
        {
            if (soldier == null || soldier.isDead) return;

            soldier.combatStress = Mathf.Clamp(soldier.combatStress + stressAmount, 0f, 100f);

            if (soldier.combatStress >= PTSD_THRESHOLD && !soldier.HasTrait("Shell Shock (Combat PTSD)"))
            {
                soldier.AddTrait("Shell Shock (Combat PTSD)");
                AudioManager.PlayHeartbeat();

                GameEvents.OnFeedEventLogged?.Invoke("TRAUMA",
                    $"<color=#FF5555><b>[COMBAT TRAUMA: SHELL SHOCK]</b></color> " +
                    $"Relentless artillery concussion and close calls have taken their toll ({reason}). " +
                    $"Hands tremble under quiet moments; sudden sounds trigger defensive reflex (PTSD trait acquired).");
            }
        }

        /// <summary>
        /// Records the agonizing death of a fireteam comrade, inducing grief and survivor's guilt.
        /// </summary>
        public static void RecordSquadmateLoss(SoldierProfile soldier, SquadMate fallenMate)
        {
            if (soldier == null || fallenMate == null) return;

            soldier.combatStress = Mathf.Clamp(soldier.combatStress + 30f, 0f, 100f);
            soldier.happiness = Mathf.Max(10f, soldier.happiness - 25f);
            soldier.AddTrait($"Remembers {fallenMate.nickname} (Grief)");

            if (soldier.combatStress >= 50f && !soldier.HasTrait("Survivor's Guilt"))
            {
                soldier.AddTrait("Survivor's Guilt");
            }

            AudioManager.PlayTaps();

            GameEvents.OnFeedEventLogged?.Invoke("GRIEF",
                $"<color=#FF4444><b>[SURVIVOR'S GUILT]</b></color> You watched {fallenMate.GetDisplayName()} fall in combat. " +
                $"Their laughter in the barracks and empty cot in the tent haunt your sleep. Why did they die while you lived?");
        }

        /// <summary>
        /// Visiting the National War Memorial to pay solemn tribute to fallen comrades.
        /// </summary>
        public static void VisitWarMemorial(SoldierProfile soldier)
        {
            if (soldier == null) return;

            soldier.combatStress = Mathf.Max(0f, soldier.combatStress - 25f);
            soldier.happiness = Mathf.Min(100f, soldier.happiness + 15f);

            if (soldier.combatStress < 30f && soldier.HasTrait("Survivor's Guilt"))
            {
                soldier.characterTraits.Remove("Survivor's Guilt");
                soldier.AddTrait("Honored Fallen Comrades (Inner Peace)");
            }

            AudioManager.PlayTaps();

            GameEvents.OnFeedEventLogged?.Invoke("MEMORIAL",
                "<color=#00FFAA><b>[WAR MEMORIAL VIGIL]</b></color> You stood before the black granite wall etched with the names of fallen comrades. " +
                "Placing a spent brass casing and a poppy flower beneath their names, a solemn peace quiets the guilt (-25 Combat Stress, +15 Morale).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        /// <summary>
        /// Attending Veteran Affairs Psychological Therapy & Peer Support Group.
        /// </summary>
        public static void AttendVASupportGroup(SoldierProfile soldier)
        {
            if (soldier == null) return;

            soldier.combatStress = Mathf.Max(0f, soldier.combatStress - 35f);
            soldier.happiness = Mathf.Min(100f, soldier.happiness + 10f);

            if (soldier.combatStress < 25f && soldier.HasTrait("Shell Shock (Combat PTSD)"))
            {
                soldier.characterTraits.Remove("Shell Shock (Combat PTSD)");
                soldier.AddTrait("Overcame Shell Shock (Mental Resilience)");
            }

            GameEvents.OnFeedEventLogged?.Invoke("VA THERAPY",
                "<color=#00FFAA><b>[VETERAN PEER SUPPORT CIRCLE]</b></color> Sitting with combat veterans who survived the same artillery barrages, " +
                "you spoke openly of sleepless nights and trench nightmares. The shared bond lightens the psychological weight (-35 Combat Stress).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        /// <summary>
        /// Evaluates spontaneous peacetime PTSD triggers (thunderstorms, car backfires, fireworks).
        /// </summary>
        public static bool CheckPeacetimeFlashback(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.combatStress < PTSD_THRESHOLD) return false;

            int roll = UnityEngine.Random.Range(1, 101);
            if (roll <= 35) // 35% chance when PTSD is active
            {
                AudioManager.PlayHeartbeat();
                soldier.happiness = Mathf.Max(15f, soldier.happiness - 12f);

                GameEvents.OnFeedEventLogged?.Invoke("FLASHBACK",
                    "<color=#FF8800><b>[COMBAT FLASHBACK TRIGGER]</b></color> A delivery truck backfired sharply outside your window. " +
                    "In a split-second panic, your instincts threw you flat onto the floor seeking trench cover before realizing you were safe in garrison. (-12 Morale).");
                return true;
            }
            return false;
        }
    }
}
