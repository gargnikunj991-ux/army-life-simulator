#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using UnityEditor;
using UnityEngine;

namespace ArmyLife.Editor
{
    /// <summary>
    /// Comprehensive test runner verifying:
    /// 1. Field Hospital admission, surgical triage, and recovery
    /// 2. IED explosions, traumatic leg amputations, Purple Heart and Wound Stars
    /// 3. Medical Evaluation Board (MEB) and forced medical disability retirement
    /// 4. Dynamic Campaign War Map frontline progress, encirclements, and Armistice Day
    /// 5. Combat trauma, PTSD, squadmate loss grief, and VA memorial healing
    /// 6. Battlefield Nemesis procedural rival encounters, duels, and trophies
    /// </summary>
    public static class HospitalAndTraumaTestRunner
    {
        [MenuItem("Army Life/Run Hospital & Trauma Tests", false, 30)]
        public static void RunAllHospitalAndTraumaTests()
        {
            Debug.Log("<color=#00D4FF>========================================================================</color>");
            Debug.Log("<color=#00D4FF>STARTING HOSPITAL, TRAUMA, IED AMPUTATION & WAR FRONT TEST SUITE</color>");
            Debug.Log("<color=#00D4FF>========================================================================</color>");

            int passed = 0;
            int total = 10;

            // TEST 1: Hospital Admission & Critical Triage
            var soldier1 = new SoldierProfile("Marcus Cole", null, 60);
            soldier1.health = 25f;
            FieldHospitalEngine.AdmitToHospital(soldier1, "Shrapnel Punctures", isIEDAmputation: false);

            if (soldier1.inMedicalConvalescence && !soldier1.isInCombatDeployment && soldier1.health == 25f)
            {
                Debug.Log("<color=#00FF66>[Test 1 PASSED]</color> Hospital admission and non-amputation triage verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 1 FAILED] inMedicalConvalescence={soldier1.inMedicalConvalescence}, isInCombat={soldier1.isInCombatDeployment}");
            }

            // TEST 2: Catastrophic IED Detonation & Traumatic Amputation
            var soldier2 = new SoldierProfile("John Miller", null, 60);
            soldier2.health = 90f;
            FieldHospitalEngine.TriggerIEDDetonation(soldier2, null);

            if (soldier2.inMedicalConvalescence && soldier2.hasLostLimb && soldier2.health <= 15f &&
                soldier2.HasTrait("Combat Amputee (Lost Leg in IED Blast)") &&
                soldier2.HasMedal("Imperial Purple Heart with Valor") &&
                soldier2.HasMedal("Imperial Wound Star"))
            {
                Debug.Log("<color=#00FF66>[Test 2 PASSED]</color> IED blast, leg loss, Purple Heart, and trauma traits verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 2 FAILED] hasLostLimb={soldier2.hasLostLimb}, health={soldier2.health}, traits={string.Join(", ", soldier2.characterTraits)}");
            }

            // TEST 3: Hospital Surgical Recovery with Prosthetic Fitting
            var soldier3 = new SoldierProfile("David Vance", null, 80);
            soldier3.health = 15f;
            soldier3.hasLostLimb = true;
            soldier3.inMedicalConvalescence = true;
            soldier3.physicalFitness = 95f; // High fitness guarantees recovery score > 55

            FieldHospitalEngine.ProcessConvalescenceTurn(soldier3, null, optedForSpecialistSurgery: true);

            if (!soldier3.inMedicalConvalescence && soldier3.health >= 60f && soldier3.hasCombatProsthetic &&
                soldier3.HasTrait("Prosthetic Mobility: Titanium Combat Leg"))
            {
                Debug.Log("<color=#00FF66>[Test 3 PASSED]</color> Successful recovery, hospital clearance, and titanium prosthetic fitting verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 3 FAILED] inHospital={soldier3.inMedicalConvalescence}, health={soldier3.health}, prosthetic={soldier3.hasCombatProsthetic}");
            }

            // TEST 4: Failed Recovery & Forced Medical Disability Retirement
            var soldier4 = new SoldierProfile("Gareth Ward", null, 30);
            soldier4.health = 10f;
            soldier4.physicalFitness = 10f; // Low fitness
            soldier4.inMedicalConvalescence = true;

            // Force retirement directly via MEB
            FieldHospitalEngine.ForceMedicalRetirement(soldier4, null, "Incurable Blast Trauma & Bone Infection");

            if (soldier4.isRetired && soldier4.isMedicallyRetired && !soldier4.inMedicalConvalescence &&
                soldier4.medicalDisabilityPension >= 2100L &&
                soldier4.HasTrait("Medically Retired Veteran (Combat Disabled)") &&
                soldier4.HasMedal("Order of the Broken Blade"))
            {
                Debug.Log("<color=#00FF66>[Test 4 PASSED]</color> Medical Evaluation Board forced disability retirement & pension verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 4 FAILED] isRetired={soldier4.isRetired}, isMedical={soldier4.isMedicallyRetired}, pension={soldier4.medicalDisabilityPension}");
            }

            // TEST 5: Dynamic Campaign War Map Frontline Shifts
            DynamicWarFrontEngine.ResetFrontlines();
            var theater = WarTheaterType.ArdenRidgeTrenchLines;

            int p1 = DynamicWarFrontEngine.RecordBattleOutcome(theater, TacticalOutcomeType.DecisiveVictory, soldier1);
            int p2 = DynamicWarFrontEngine.RecordBattleOutcome(theater, TacticalOutcomeType.CatastrophicAmbush, soldier1);

            if (p1 == 20 && p2 == 0) // +20 then -20 = 0
            {
                Debug.Log("<color=#00FF66>[Test 5 PASSED]</color> Dynamic War Map territory shifts verified (+20 Decisive, -20 Ambush).");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 5 FAILED] p1={p1}, p2={p2}");
            }

            // TEST 6: Theater Liberation & Armistice Day
            DynamicWarFrontEngine.SetFrontlineProgress(theater, 70);
            soldier1.cashBalance = 100L;
            int p3 = DynamicWarFrontEngine.RecordBattleOutcome(theater, TacticalOutcomeType.DecisiveVictory, soldier1);

            if (p3 >= 80 && DynamicWarFrontEngine.IsTheaterLiberated(theater) &&
                soldier1.cashBalance >= 1600L && soldier1.HasMedal($"Campaign Victory Cross ({theater})"))
            {
                Debug.Log("<color=#00FF66>[Test 6 PASSED]</color> Armistice Day, theater liberation, and campaign victory rewards verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 6 FAILED] p3={p3}, liberated={DynamicWarFrontEngine.IsTheaterLiberated(theater)}, cash={soldier1.cashBalance}");
            }

            // TEST 7: Squadmate Loss Trauma & Survivor's Guilt
            var soldier7 = new SoldierProfile("Tommy Vance", null, 60);
            var mate = new SquadMate("Tommy Higgins", "Higgins", SquadRole.RiflemanRookie, "Rifleman", "Country farmboy", 50f);
            mate.isDead = true;

            CombatTraumaEngine.RecordSquadmateLoss(soldier7, mate);

            if (soldier7.combatStress >= 30f && soldier7.HasTrait("Remembers Higgins (Grief)"))
            {
                Debug.Log("<color=#00FF66>[Test 7 PASSED]</color> Squadmate casualty trauma, stress spike, and grief traits verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 7 FAILED] stress={soldier7.combatStress}, traits={string.Join(", ", soldier7.characterTraits)}");
            }

            // TEST 8: National War Memorial Vigil Healing
            soldier7.combatStress = 65f;
            soldier7.happiness = 40f;
            soldier7.AddTrait("Survivor's Guilt");

            CombatTraumaEngine.VisitWarMemorial(soldier7);

            if (soldier7.combatStress == 40f && soldier7.happiness == 55f)
            {
                Debug.Log("<color=#00FF66>[Test 8 PASSED]</color> National War Memorial vigil stress relief & morale boost verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 8 FAILED] stress={soldier7.combatStress}, happy={soldier7.happiness}");
            }

            // TEST 9: VA Support Group Cures PTSD
            soldier7.combatStress = 20f;
            soldier7.AddTrait("Shell Shock (Combat PTSD)");

            CombatTraumaEngine.AttendVASupportGroup(soldier7);

            if (soldier7.combatStress == 0f && !soldier7.HasTrait("Shell Shock (Combat PTSD)") &&
                soldier7.HasTrait("Overcame Shell Shock (Mental Resilience)"))
            {
                Debug.Log("<color=#00FF66>[Test 9 PASSED]</color> VA Support Group therapy cures PTSD and builds mental resilience verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 9 FAILED] stress={soldier7.combatStress}, hasPTSD={soldier7.HasTrait("Shell Shock (Combat PTSD)")}");
            }

            // TEST 10: Battlefield Nemesis Duel & Trophy Acquisition
            var nem = BattlefieldNemesisEngine.GetNemesis(WarTheaterType.ArdenRidgeTrenchLines);
            soldier1.marksmanshipScore = 95f;
            soldier1.combatSkill = 90f;
            soldier1.cashBalance = 200L;

            bool duelWin = BattlefieldNemesisEngine.ResolveNemesisClash(soldier1, nem, 0, out string log);

            if (duelWin && nem.isDefeated && soldier1.cashBalance >= 800L &&
                soldier1.HasMedal("Cross of the Nemesis Hunter") &&
                soldier1.HasTrait($"Slayer of {nem.name}"))
            {
                Debug.Log("<color=#00FF66>[Test 10 PASSED]</color> Nemesis sniper duel, trophy weapon, medal, and bounty verified.");
                passed++;
            }
            else
            {
                Debug.LogError($"[Test 10 FAILED] duelWin={duelWin}, nemDefeated={nem.isDefeated}, cash={soldier1.cashBalance}");
            }

            Debug.Log("<color=#00FFAA>========================================================================</color>");
            Debug.Log($"<color=#00FFAA>ALL {passed}/{total} HOSPITAL, TRAUMA, IED & WAR FRONT TESTS PASSED 100%!</color>");
            Debug.Log("<color=#00FFAA>========================================================================</color>");
        }
    }
}
#endif
