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
    /// Automated test runner for Combat Theaters, Tactical Missions & Squad Deployment Engine.
    /// Verifies authentic theater traits, mission generation, squad role synergies (Scout, Gunner, Medic, Leader),
    /// tactical outcome spectrum, combat honors (Silver Cross, Bronze Star, Wound Star, Valor Citation),
    /// hazard bounties, and squadmate fatigue/morale/injury lifecycle.
    /// </summary>
    public static class CombatOperationsTestRunner
    {
        [MenuItem("Army Life/Run Combat Operations Test", false, 23)]
        public static void RunCombatOperationsTests()
        {
            Debug.Log("<color=#00D4FF>========================================================================</color>");
            Debug.Log("<color=#00D4FF>====== STARTING COMBAT THEATERS & TACTICAL OPERATIONS TEST SUITE ======</color>");
            Debug.Log("<color=#00D4FF>========================================================================</color>");

            // ==================== TEST 1: COMBAT THEATERS DATA INTEGRITY ====================
            Debug.Log("[Test 1] Verifying all 5 distinct authentic combat theaters...");
            var allTheaters = WarTheaterInfo.GetAllTheaters();
            AssertCondition(allTheaters != null && allTheaters.Count == 5, 
                $"Expected 5 authentic theaters, got {allTheaters?.Count ?? 0}.");

            // 1. Arden Ridge Trench Lines
            var arden = WarTheaterInfo.GetInfo(WarTheaterType.ArdenRidgeTrenchLines);
            AssertCondition(arden.theaterName == "Arden Ridge Trench Lines", "Arden Ridge theaterName mismatch.");
            AssertCondition(arden.dangerLevel == 4, $"Arden Ridge dangerLevel should be 4, got {arden.dangerLevel}.");
            AssertCondition(arden.casualtyRateModifier >= 1.30f, "Arden Ridge casualty modifier should be >= 1.30.");
            AssertCondition(arden.hazardPayRate == 500L, $"Arden Ridge hazardPayRate should be 500, got {arden.hazardPayRate}.");
            AssertCondition(arden.specializedTerrainTrait.Contains("Trench") || arden.specializedTerrainTrait.Contains("Artillery"),
                "Arden Ridge specializedTerrainTrait should mention trench foot or artillery.");
            AssertCondition(arden.operationalDoctrinesRecommended.Contains("Static Trench Defense"),
                "Arden Ridge recommended doctrines should include Static Trench Defense.");

            // 2. Port Iron Amphibious Front
            var portIron = WarTheaterInfo.GetInfo(WarTheaterType.PortIronAmphibiousFront);
            AssertCondition(portIron.theaterName == "Port Iron Amphibious Front", "Port Iron theaterName mismatch.");
            AssertCondition(portIron.dangerLevel == 5, $"Port Iron dangerLevel should be 5, got {portIron.dangerLevel}.");
            AssertCondition(portIron.casualtyRateModifier >= 1.50f, "Port Iron casualty modifier should be >= 1.50.");
            AssertCondition(portIron.hazardPayRate == 750L, $"Port Iron hazardPayRate should be 750, got {portIron.hazardPayRate}.");
            AssertCondition(portIron.specializedTerrainTrait.Contains("Beachhead") || portIron.specializedTerrainTrait.Contains("Sea-Walls"),
                "Port Iron specializedTerrainTrait should mention beachhead or sea-walls.");
            AssertCondition(portIron.operationalDoctrinesRecommended.Contains("Amphibious Breach Assault"),
                "Port Iron recommended doctrines should include Amphibious Breach Assault.");

            // 3. Kaelen Valley Urban Zone
            var kaelen = WarTheaterInfo.GetInfo(WarTheaterType.KaelenValleyUrbanZone);
            AssertCondition(kaelen.theaterName == "Kaelen Valley Urban Zone", "Kaelen Valley theaterName mismatch.");
            AssertCondition(kaelen.dangerLevel == 4, $"Kaelen Valley dangerLevel should be 4, got {kaelen.dangerLevel}.");
            AssertCondition(kaelen.hazardPayRate == 600L, $"Kaelen Valley hazardPayRate should be 600, got {kaelen.hazardPayRate}.");
            AssertCondition(kaelen.specializedTerrainTrait.Contains("CQB") || kaelen.specializedTerrainTrait.Contains("Sniper"),
                "Kaelen Valley specializedTerrainTrait should mention CQB or snipers.");
            AssertCondition(kaelen.operationalDoctrinesRecommended.Contains("Room-Clearing CQB"),
                "Kaelen Valley recommended doctrines should include Room-Clearing CQB.");

            // 4. Badlands Steppe Recon
            var badlands = WarTheaterInfo.GetInfo(WarTheaterType.BadlandsSteppeRecon);
            AssertCondition(badlands.theaterName == "Badlands Steppe Recon", "Badlands theaterName mismatch.");
            AssertCondition(badlands.dangerLevel == 3, $"Badlands dangerLevel should be 3, got {badlands.dangerLevel}.");
            AssertCondition(badlands.hazardPayRate == 450L, $"Badlands hazardPayRate should be 450, got {badlands.hazardPayRate}.");
            AssertCondition(badlands.specializedTerrainTrait.Contains("Heat") || badlands.specializedTerrainTrait.Contains("Desert"),
                "Badlands specializedTerrainTrait should mention heat exhaustion or desert.");
            AssertCondition(badlands.operationalDoctrinesRecommended.Contains("High-Value Recon Patrol"),
                "Badlands recommended doctrines should include High-Value Recon Patrol.");

            // 5. Northern Taiga Outpost
            var taiga = WarTheaterInfo.GetInfo(WarTheaterType.NorthernTaigaOutpost);
            AssertCondition(taiga.theaterName == "Northern Taiga Outpost", "Northern Taiga theaterName mismatch.");
            AssertCondition(taiga.dangerLevel == 3, $"Northern Taiga dangerLevel should be 3, got {taiga.dangerLevel}.");
            AssertCondition(taiga.hazardPayRate == 550L, $"Northern Taiga hazardPayRate should be 550, got {taiga.hazardPayRate}.");
            AssertCondition(taiga.specializedTerrainTrait.Contains("Hypothermia") || taiga.specializedTerrainTrait.Contains("Snowdrifts"),
                "Northern Taiga specializedTerrainTrait should mention hypothermia or snowdrifts.");
            AssertCondition(taiga.operationalDoctrinesRecommended.Contains("Forward Artillery Observation"),
                "Northern Taiga recommended doctrines should include Forward Artillery Observation.");

            // Backward-compatibility aliases check
            var legacyTrench = WarTheaterInfo.GetInfo(WarTheaterType.WesternMudTrenches);
            AssertCondition(legacyTrench.theaterName == "Arden Ridge Trench Lines", "Legacy WesternMudTrenches should map to Arden Ridge.");
            var legacySteppe = WarTheaterInfo.GetInfo(WarTheaterType.EasternFrozenSteppe);
            AssertCondition(legacySteppe.theaterName == "Northern Taiga Outpost", "Legacy EasternFrozenSteppe should map to Northern Taiga.");

            Debug.Log("<color=#00FF66>[Test 1 PASSED]</color> All 5 military combat theaters and legacy aliases verified.");

            // ==================== TEST 2: TACTICAL MISSION GENERATION ACROSS ALL THEATERS ====================
            Debug.Log("[Test 2] Verifying operational mission generation across all theaters and types...");
            var testSoldier = new SoldierProfile("Sergeant Alexander Holt", null, 150);
            testSoldier.InitializeSquadMatesIfEmpty();

            // Verify generation across each theater
            foreach (var th in allTheaters)
            {
                var mission = CombatOperationsEngine.GenerateMission(th, testSoldier);
                AssertCondition(mission != null, $"Mission generation failed for theater {th.theaterName}.");
                AssertCondition(mission.theater == th, "Mission theater reference should match.");
                AssertCondition(mission.missionDifficulty == th.dangerLevel, "Mission difficulty should match theater dangerLevel.");
                AssertCondition(mission.baseHazardBounty > 0, "Mission base hazard bounty must be positive.");
            }

            // Verify all 6 distinct mission types explicitly
            TacticalMissionType[] allMissions = new TacticalMissionType[]
            {
                TacticalMissionType.TrenchBreachAssault,
                TacticalMissionType.HighValueReconPatrol,
                TacticalMissionType.ForwardArtilleryObservation,
                TacticalMissionType.CasualtyEvacuation,
                TacticalMissionType.ConvoyOverwatchAndDefense,
                TacticalMissionType.NightSabotageRaid
            };

            foreach (var mType in allMissions)
            {
                var explicitMission = CombatOperationsEngine.GenerateMission(arden, testSoldier, mType);
                AssertCondition(explicitMission.missionType == mType, $"Expected mission type {mType}, got {explicitMission.missionType}.");
                AssertCondition(!string.IsNullOrEmpty(explicitMission.missionName), "Mission name must not be empty.");
                AssertCondition(!string.IsNullOrEmpty(explicitMission.objectiveDescription), "Mission description must not be empty.");
                AssertCondition(!string.IsNullOrEmpty(explicitMission.recommendedDoctrine), "Mission recommended doctrine must not be empty.");
            }

            Debug.Log("<color=#00FF66>[Test 2 PASSED]</color> All 6 operational mission types generated with valid data.");

            // ==================== TEST 3: SQUAD ROLE SYNERGY (SCOUT, GUNNER, MEDIC, LEADER) ====================
            Debug.Log("[Test 3] Verifying squadmate role synergies (Scout, Gunner, Medic)...");
            var deadeye = testSoldier.squadMates.Find(m => m.role == SquadRole.LeadScout);
            var anvil = testSoldier.squadMates.Find(m => m.role == SquadRole.HeavyGunner);
            var doc = testSoldier.squadMates.Find(m => m.role == SquadRole.CombatMedic);

            AssertCondition(deadeye != null && anvil != null && doc != null, "All 3 core specialists must exist.");

            // 1. Scout Recon Synergy
            var reconMission = CombatOperationsEngine.GenerateMission(badlands, testSoldier, TacticalMissionType.HighValueReconPatrol);
            var reconOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                reconMission, 
                TacticalChoiceOrder.ReconnaissanceFlank, 
                badlands, 
                forceOutcome: TacticalOutcomeType.TacticalSuccess);

            AssertCondition(reconOutcome.roleSynergySummary.Contains("Lead Scout"),
                "Recon patrol with scout should include Lead Scout in synergy summary.");

            // 2. Gunner Cyclic Suppression Synergy
            var trenchMission = CombatOperationsEngine.GenerateMission(arden, testSoldier, TacticalMissionType.TrenchBreachAssault);
            var trenchOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                trenchMission, 
                TacticalChoiceOrder.HeavySuppression, 
                arden, 
                forceOutcome: TacticalOutcomeType.TacticalSuccess);

            AssertCondition(trenchOutcome.roleSynergySummary.Contains("Heavy Gunner"),
                "Trench breach assault with gunner should include Heavy Gunner in synergy summary.");

            // 3. Medic CASEVAC Synergy
            var casevacMission = CombatOperationsEngine.GenerateMission(portIron, testSoldier, TacticalMissionType.CasualtyEvacuation);
            var casevacOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                casevacMission, 
                TacticalChoiceOrder.EmergencyCASEVAC, 
                portIron, 
                forceOutcome: TacticalOutcomeType.TacticalSuccess);

            AssertCondition(casevacOutcome.roleSynergySummary.Contains("Combat Medic"),
                "CASEVAC operation with medic should include Combat Medic in synergy summary.");

            Debug.Log("<color=#00FF66>[Test 3 PASSED]</color> Squad role synergies (Scout, Gunner, Medic) successfully engaged.");

            // ==================== TEST 4: TACTICAL OUTCOME SPECTRUM & COMBAT HONORS ====================
            Debug.Log("[Test 4] Verifying full outcome spectrum (Decisive, Success, Pyrrhic, Withdrawal, Ambush)...");

            // Case A: Decisive Victory
            testSoldier.earnedMedals.Clear();
            var decisiveOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                trenchMission, 
                TacticalChoiceOrder.AggressiveBreach, 
                arden, 
                forceOutcome: TacticalOutcomeType.DecisiveVictory);

            AssertCondition(decisiveOutcome.isSuccess, "Decisive Victory should be marked as success.");
            AssertCondition(decisiveOutcome.promotionPoints == 20, $"Expected 20 promotion points, got {decisiveOutcome.promotionPoints}.");
            AssertCondition(decisiveOutcome.cashHazardBounty == trenchMission.baseHazardBounty * 2L, "Decisive victory should award double base bounty.");
            AssertCondition(testSoldier.HasMedal("Silver Cross") || testSoldier.HasMedal("Distinguished Service Cross"), "Decisive Victory must award the Silver Cross or DSC.");
            AssertCondition(decisiveOutcome.squadMoraleDelta > 0, "Decisive victory should increase squad morale.");

            // Case B: Tactical Success
            var successOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                reconMission, 
                TacticalChoiceOrder.ReconnaissanceFlank, 
                badlands, 
                forceOutcome: TacticalOutcomeType.TacticalSuccess);

            AssertCondition(successOutcome.isSuccess, "Tactical Success should be marked as success.");
            AssertCondition(successOutcome.promotionPoints == 12, $"Expected 12 promotion points, got {successOutcome.promotionPoints}.");
            AssertCondition(testSoldier.HasMedal("Bronze Star"), "Tactical Success must award Bronze Star.");

            // Case C: Pyrrhic Victory
            float healthBeforePyrrhic = testSoldier.health;
            var pyrrhicOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                trenchMission, 
                TacticalChoiceOrder.AggressiveBreach, 
                arden, 
                forceOutcome: TacticalOutcomeType.PyrrhicVictory);

            AssertCondition(pyrrhicOutcome.isSuccess, "Pyrrhic Victory should be marked as success.");
            AssertCondition(pyrrhicOutcome.playerWounded, "Player should be marked as wounded in Pyrrhic Victory.");
            AssertCondition(testSoldier.HasMedal("Imperial Wound Star"), "Pyrrhic Victory wounds must award the Imperial Wound Star.");
            AssertCondition(testSoldier.HasMedal("Valor Citation"), "Pyrrhic Victory must award Valor Citation.");
            AssertCondition(pyrrhicOutcome.squadMoraleDelta < 0, "Pyrrhic victory should decrease squad morale due to heavy casualties.");
            AssertCondition(testSoldier.health < healthBeforePyrrhic, "Player health should decrease from battlefield wounds.");

            // Case D: Tactical Withdrawal
            var withdrawalOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                reconMission, 
                TacticalChoiceOrder.OrderlyWithdrawal, 
                badlands, 
                forceOutcome: TacticalOutcomeType.TacticalWithdrawal);

            AssertCondition(!withdrawalOutcome.isSuccess, "Tactical Withdrawal is not a combat victory.");
            AssertCondition(withdrawalOutcome.promotionPoints == 3, $"Expected 3 promotion points, got {withdrawalOutcome.promotionPoints}.");
            AssertCondition(withdrawalOutcome.cashHazardBounty > 0, "Tactical withdrawal should still award partial hazard compensation.");
            AssertCondition(withdrawalOutcome.casualties == 0, "Orderly withdrawal should preserve troops with 0 casualties.");

            // Case E: Catastrophic Ambush
            var ambushMission = CombatOperationsEngine.GenerateMission(kaelen, testSoldier, TacticalMissionType.ConvoyOverwatchAndDefense);
            var ambushOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                ambushMission, 
                TacticalChoiceOrder.DefaultOrder, 
                kaelen, 
                forceOutcome: TacticalOutcomeType.CatastrophicAmbush);

            AssertCondition(!ambushOutcome.isSuccess, "Catastrophic Ambush must be a failure.");
            AssertCondition(ambushOutcome.cashHazardBounty == 0L, "Catastrophic Ambush awards 0 hazard bounty.");
            AssertCondition(ambushOutcome.promotionPoints == 0, "Catastrophic Ambush awards 0 promotion points.");
            AssertCondition(ambushOutcome.squadMoraleDelta <= -15f, "Catastrophic Ambush should cause massive morale penalty.");
            AssertCondition(testSoldier.HasMedal("Imperial Wound Star"), "Ambush wounds must award Imperial Wound Star.");

            Debug.Log("<color=#00FF66>[Test 4 PASSED]</color> Tactical outcome spectrum and combat honors verified.");

            // ==================== TEST 5: SQUADMATE WELFARE (MORALE, FATIGUE, INJURIES) ====================
            Debug.Log("[Test 5] Verifying squadmate morale, fatigue, and injury tracking...");
            var testMate = testSoldier.squadMates[0];
            testMate.morale = 70f;
            testMate.fatigue = 20f;
            testMate.isInjured = false;

            // Run decisive victory to verify morale boost
            CombatOperationsEngine.ExecuteMission(testSoldier, trenchMission, TacticalChoiceOrder.DefaultOrder, arden, forceOutcome: TacticalOutcomeType.DecisiveVictory);
            AssertCondition(testMate.morale > 70f, $"Squadmate morale should increase on Decisive Victory. Got {testMate.morale}.");
            AssertCondition(testMate.fatigue > 20f, "Squadmate fatigue should increase after a high-intensity battle.");
            AssertCondition(testMate.battlesSurvived > 0, "Squadmate battlesSurvived should increment.");

            // Run ambush or pyrrhic victory to trigger injury
            CombatOperationsEngine.ExecuteMission(testSoldier, trenchMission, TacticalChoiceOrder.DefaultOrder, arden, forceOutcome: TacticalOutcomeType.PyrrhicVictory);
            var injuredMate = testSoldier.squadMates.Find(m => m.isInjured);
            AssertCondition(injuredMate != null, "At least one squadmate should be injured following Pyrrhic Victory.");
            AssertCondition(!string.IsNullOrEmpty(injuredMate.injuryStatus), "Injury status description should not be empty.");

            // Apply rest & recovery
            float fatigueBeforeRest = injuredMate.fatigue;
            injuredMate.ApplyRestAndRecovery(30f, 20f);
            AssertCondition(injuredMate.fatigue < fatigueBeforeRest, "Rest should reduce fatigue.");
            AssertCondition(!injuredMate.isInjured, "Rest & medical treatment should clear isInjured.");
            AssertCondition(injuredMate.injuryStatus.Contains("Fit for Duty"), "Injury status should return to Fit for Duty.");

            Debug.Log("<color=#00FF66>[Test 5 PASSED]</color> Squadmate morale, fatigue, and injury lifecycle verified.");

            // ==================== TEST 6: HAZARD PAY & BOUNTY FINANCIAL ACCOUNTING ====================
            Debug.Log("[Test 6] Verifying hazard pay and bounty accounting...");
            long cashBefore = testSoldier.cashBalance;
            var portIronMission = CombatOperationsEngine.GenerateMission(portIron, testSoldier, TacticalMissionType.TrenchBreachAssault);

            var payOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                portIronMission, 
                TacticalChoiceOrder.HeavySuppression, 
                portIron, 
                forceOutcome: TacticalOutcomeType.TacticalSuccess);

            long expectedTotalCash = payOutcome.cashHazardBounty + portIron.hazardPayRate;
            AssertCondition(payOutcome.theaterHazardPay == portIron.hazardPayRate,
                $"Expected theater hazard pay {portIron.hazardPayRate}, got {payOutcome.theaterHazardPay}.");
            AssertCondition(testSoldier.cashBalance >= cashBefore + expectedTotalCash,
                $"Soldier cash balance should increase by total bounty + hazard pay. Expected at least {cashBefore + expectedTotalCash}, got {testSoldier.cashBalance}.");

            Debug.Log("<color=#00FF66>[Test 6 PASSED]</color> Hazard pay and operational bounty correctly calculated and awarded.");

            // ==================== TEST 7: AMBUSH MITIGATION BY LEAD SCOUT ====================
            Debug.Log("[Test 7] Verifying ambush mitigation by vigilant Lead Scout...");
            deadeye.relationship = 75f;
            deadeye.fatigue = 10f;
            deadeye.isDead = false;

            // Low roll on Convoy Overwatch should trigger scout alert mitigation
            var ambushAvoidMission = CombatOperationsEngine.GenerateMission(badlands, testSoldier, TacticalMissionType.ConvoyOverwatchAndDefense);
            var mitigatedOutcome = CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                ambushAvoidMission, 
                TacticalChoiceOrder.ReconnaissanceFlank, 
                badlands, 
                forceRoll: 10f); // very low roll that would normally be Catastrophic Ambush

            AssertCondition(mitigatedOutcome.outcomeType != TacticalOutcomeType.CatastrophicAmbush,
                "Alert Lead Scout must mitigate low tactical roll, preventing Catastrophic Ambush.");

            Debug.Log("<color=#00FF66>[Test 7 PASSED]</color> Lead Scout early warning successfully averted ambush catastrophe.");

            // ==================== TEST 8: COMBAT MEDIC HEROIC LIFE-SAVING INTERVENTION ====================
            Debug.Log("[Test 8] Verifying Combat Medic emergency trauma life-saving under fire...");
            testSoldier.health = 5f;
            testSoldier.fieldMedkitsCount = 0;
            doc.relationship = 80f;
            doc.fatigue = 10f;
            doc.isDead = false;

            // Force catastrophic ambush where damage exceeds 5 HP
            CombatOperationsEngine.ExecuteMission(
                testSoldier, 
                ambushMission, 
                TacticalChoiceOrder.DefaultOrder, 
                kaelen, 
                forceOutcome: TacticalOutcomeType.CatastrophicAmbush);

            AssertCondition(!testSoldier.isDead, "Combat Medic must save player from fatal wound.");
            AssertCondition(testSoldier.health >= 15f, $"Player health should be stabilized to at least 15f, got {testSoldier.health}.");

            Debug.Log("<color=#00FF66>[Test 8 PASSED]</color> Combat Medic emergency life-saving intervention confirmed.");

            // ==================== TEST 9: FULL 6-MONTH SEMESTER DEPLOYMENT TOUR INTEGRATION ====================
            Debug.Log("[Test 9] Verifying 6-month deployment tour integration...");
            testSoldier.currentTheater = WarTheaterType.NorthernTaigaOutpost;
            testSoldier.toursInCurrentTheater = 0;
            int pointsBeforeTour = testSoldier.accumulatedPromotionPoints;
            float skillBeforeTour = testSoldier.combatSkill;

            var tourOutcome = CombatOperationsEngine.ProcessSemesterCombatTour(
                testSoldier, 
                forceOutcome: TacticalOutcomeType.TacticalSuccess);

            AssertCondition(tourOutcome != null, "Tour outcome should not be null.");
            AssertCondition(tourOutcome.mission.theater.theaterType == WarTheaterType.NorthernTaigaOutpost,
                "Tour mission theater should match soldier's current theater.");
            AssertCondition(testSoldier.toursInCurrentTheater == 1, 
                $"Tours completed in theater should increment to 1, got {testSoldier.toursInCurrentTheater}.");
            AssertCondition(testSoldier.accumulatedPromotionPoints > pointsBeforeTour,
                "Accumulated promotion points should increase after semester tour.");
            AssertCondition(testSoldier.combatSkill > skillBeforeTour,
                "Combat skill should increase after semester tour.");

            Debug.Log("<color=#00FF66>[Test 9 PASSED]</color> 6-month semester combat deployment tour fully operational.");

            // ==================== TEST 10: NARRATIVE FEED INTEGRATION ====================
            Debug.Log("[Test 10] Verifying GameEvents narrative feed integration...");
            bool feedReceived = false;
            string lastFeedTag = string.Empty;
            string lastFeedMsg = string.Empty;

            Action<string, string> feedObserver = (tag, msg) =>
            {
                feedReceived = true;
                lastFeedTag = tag;
                lastFeedMsg = msg;
            };

            GameEvents.OnFeedEventLogged += feedObserver;
            try
            {
                CombatOperationsEngine.ProcessSemesterCombatTour(testSoldier, forceOutcome: TacticalOutcomeType.DecisiveVictory);
                AssertCondition(feedReceived, "GameEvents.OnFeedEventLogged should be fired during combat operations.");
                AssertCondition(lastFeedTag == "COMBAT OPERATIONS" || lastFeedTag == "SQUAD SYNERGY",
                    $"Expected feed tag COMBAT OPERATIONS or SQUAD SYNERGY, got {lastFeedTag}.");
            }
            finally
            {
                GameEvents.OnFeedEventLogged -= feedObserver;
            }

            Debug.Log("<color=#00FF66>[Test 10 PASSED]</color> Narrative feed and combat dispatch confirmed.");

            Debug.Log("<color=#00FFAA>========================================================================</color>");
            Debug.Log("<color=#00FFAA>=== ALL COMBAT THEATERS & OPERATIONS TESTS PASSED 100% SUCCESSFULLY! ===</color>");
            Debug.Log("<color=#00FFAA>========================================================================</color>");
        }

        /// <summary>
        /// Batchmode runner entry point for automated CI / command line execution.
        /// </summary>
        public static void RunTestBatch()
        {
            try
            {
                RunCombatOperationsTests();
                Debug.Log("[BATCH COMPLETE] All Combat Operations tests passed with zero failures.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BATCH FAILED] Combat Operations test threw an unhandled exception: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }

        private static void AssertCondition(bool condition, string failureMessage)
        {
            if (!condition)
            {
                Debug.LogError($"<color=#FF4444>[TEST ASSERTION FAILED]</color> {failureMessage}");
                throw new Exception($"Test Assertion Failed: {failureMessage}");
            }
        }
    }
}
#endif
