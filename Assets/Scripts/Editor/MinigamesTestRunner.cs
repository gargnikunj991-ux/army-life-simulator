#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using ArmyLife.UI;
using UnityEditor;
using UnityEngine;

namespace ArmyLife.Editor
{
    /// <summary>
    /// Comprehensive automated test runner for Interactive Tactical Minigames & MOS Specialization Engine:
    /// 1. Papers, Please style Checkpoint & Guard Duty Document/Cargo Inspection
    /// 2. Firing Range Marksmanship Qualification (50m-300m timed targets & Shoot/No-Shoot)
    /// 3. Barracks Craps / 2d6 Dice Gambling Ring
    /// 4. Specialized Military Occupational Specialties (MOS) Career System
    /// 5. Tactical Minigames Modal UI Lifecycle
    /// </summary>
    public static class MinigamesTestRunner
    {
        [MenuItem("Army Life/Run Tactical Minigames Tests", false, 25)]
        public static void RunAllMinigamesTests()
        {
            Debug.Log("<color=#00D4FF>========================================================================</color>");
            Debug.Log("<color=#00D4FF>====== STARTING INTERACTIVE TACTICAL MINIGAMES & MOS ENGINE TESTS ======</color>");
            Debug.Log("<color=#00D4FF>========================================================================</color>");

            // ==================== TEST 1: CHECKPOINT CASE GENERATION ====================
            Debug.Log("[Test 1] Verifying procedural checkpoint vehicle cases and passport integrity...");
            var cleanCase = CheckpointInspectionEngine.GenerateInspectionCase(seed: 101, discrepancyProbability: 0.0f);
            AssertCondition(cleanCase != null, "Clean case must not be null.");
            AssertCondition(!cleanCase.HasAnyDiscrepancy, "Discrepancy probability 0.0f must produce a clean case.");
            AssertCondition(cleanCase.transitPassport != null, "Transit passport must not be null.");
            AssertCondition(!cleanCase.transitPassport.isExpired, "Clean case passport must not be expired.");
            AssertCondition(cleanCase.transitPassport.officialSealValid, "Clean case imperial seal must be valid.");
            AssertCondition(cleanCase.cargoManifest != null, "Cargo manifest must not be null.");
            AssertCondition(cleanCase.cargoManifest.actualHiddenCargo == HiddenCargoType.None, "Clean case cargo must be None.");
            Debug.Log("<color=#00FF66>[Test 1 PASSED]</color> Procedural clean checkpoint case validated.");

            // ==================== TEST 2: CHECKPOINT DISCREPANCIES & CONTRABAND ====================
            Debug.Log("[Test 2] Verifying contraband types, counterfeit passports, and wanted bulletins...");
            bool foundWeapons = false;
            bool foundCounterfeit = false;
            bool foundWanted = false;

            for (int s = 1; s <= 50; s++)
            {
                var testCase = CheckpointInspectionEngine.GenerateInspectionCase(seed: s * 17, discrepancyProbability: 1.0f);
                AssertCondition(testCase.HasAnyDiscrepancy, $"Seed {s * 17} with 1.0 prob must have a discrepancy.");

                if (testCase.cargoManifest.actualHiddenCargo == HiddenCargoType.Weapons) foundWeapons = true;
                if (testCase.isCounterfeitPassport) foundCounterfeit = true;
                if (testCase.isWantedInfiltrator)
                {
                    foundWanted = true;
                    AssertCondition(!string.IsNullOrEmpty(testCase.wantedBulletinDescription), "Wanted infiltrator must have bulletin description.");
                }
            }

            AssertCondition(foundWeapons, "Procedural generation must generate Weapons contraband.");
            AssertCondition(foundCounterfeit, "Procedural generation must generate Counterfeit/Expired passports.");
            AssertCondition(foundWanted, "Procedural generation must generate Wanted infiltrators.");
            Debug.Log("<color=#00FF66>[Test 2 PASSED]</color> Contraband diversity and security flags confirmed.");

            // ==================== TEST 3: CHECKPOINT DECISION - APPROVE CLEAN VEHICLE ====================
            Debug.Log("[Test 3] Verifying player approving clean vehicle (Smooth traffic)...");
            var soldier = new SoldierProfile("Sergeant Alan Cole", null, 100);
            soldier.discipline = 50f;
            soldier.coFavor = 50f;

            var approveCleanResult = CheckpointInspectionEngine.ProcessPlayerDecision(cleanCase, CheckpointDecision.ApprovePass, soldier);
            AssertCondition(approveCleanResult.success, "Approving clean vehicle must be successful.");
            AssertCondition(approveCleanResult.disciplineDelta > 0f, "Discipline must increase for approving clean vehicle.");
            AssertCondition(soldier.discipline > 50f, $"Soldier discipline should increase from 50. Got {soldier.discipline}.");
            Debug.Log("<color=#00FF66>[Test 3 PASSED]</color> Clean approval rewarded discipline.");

            // ==================== TEST 4: CHECKPOINT DECISION - APPROVE WEAPONS SMUGGLER ====================
            Debug.Log("[Test 4] Verifying security breach when waving weapons smuggler through gate...");
            var weaponsCase = new CheckpointVehicleCase
            {
                caseId = "CASE-TEST-WEAPONS",
                civilianName = "Boris Stanislav",
                vehicleType = VehicleType.MilitarySupplyTruck,
                licensePlate = "VAL-999-01",
                transitPassport = new TransitPassport("Boris Stanislav", "ID-VAL-1111", "Aethelgard Imperial Central", "Year 2028, Sem 1", false, true, "Transport", "Signed"),
                cargoManifest = new CargoManifest("Flour Sacks", 1500f, "Frontline Depot", HiddenCargoType.Weapons, "Submachine guns and stick grenades"),
                isCounterfeitPassport = false,
                isWantedInfiltrator = false
            };

            soldier.discipline = 60f;
            soldier.coFavor = 60f;
            var weaponsApproved = CheckpointInspectionEngine.ProcessPlayerDecision(weaponsCase, CheckpointDecision.ApprovePass, soldier);
            AssertCondition(!weaponsApproved.success, "Approving weapons smuggler must fail.");
            AssertCondition(weaponsApproved.isInsurgentAttack, "Approving weapons smuggler must trigger insurgent attack flag.");
            AssertCondition(weaponsApproved.disciplineDelta <= -15f, "Discipline must suffer severe deduction.");
            AssertCondition(soldier.discipline < 60f, "Soldier discipline must decrease after attack.");
            Debug.Log("<color=#00FF66>[Test 4 PASSED]</color> Catastrophic security breach triggered appropriately.");

            // ==================== TEST 5: CHECKPOINT DECISION - SEARCH CARGO & SEIZE CONTRABAND ====================
            Debug.Log("[Test 5] Verifying search cargo seizing black-market contraband and awarding cash bounty...");
            soldier.cashBalance = 1000L;
            soldier.accumulatedPromotionPoints = 10;

            var opticsCase = new CheckpointVehicleCase
            {
                caseId = "CASE-TEST-OPTICS",
                civilianName = "Klaus Brandt",
                vehicleType = VehicleType.RedCrossAmbulance,
                licensePlate = "VAL-555-22",
                transitPassport = new TransitPassport("Klaus Brandt", "ID-VAL-2222", "Port Iron Maritime District", "Year 2028, Sem 1", false, true, "Medical Evac", "Signed"),
                cargoManifest = new CargoManifest("Bandages", 800f, "Hospital", HiddenCargoType.BlackMarketOptics, "Stolen Imperial Army night prism optics"),
                isCounterfeitPassport = false,
                isWantedInfiltrator = false
            };

            var searchResult = CheckpointInspectionEngine.ProcessPlayerDecision(opticsCase, CheckpointDecision.SearchCargo, soldier);
            AssertCondition(searchResult.success, "Searching cargo with contraband must succeed.");
            AssertCondition(searchResult.contrabandFound, "Contraband must be flagged as discovered.");
            AssertCondition(searchResult.cashBountyDelta > 0L, "Confiscation cash bounty must be awarded.");
            AssertCondition(soldier.cashBalance > 1000L, $"Soldier cash should increase from bounty. Got {soldier.cashBalance}.");
            AssertCondition(soldier.accumulatedPromotionPoints > 10, "Promotion points must be awarded for seizing optics.");
            Debug.Log("<color=#00FF66>[Test 5 PASSED]</color> Contraband seized, promotion points and cash bounty awarded.");

            // ==================== TEST 6: CHECKPOINT DECISION - DETAIN WANTED INFILTRATOR ====================
            Debug.Log("[Test 6] Verifying arresting wanted infiltrator (Major intelligence coup)...");
            var spyCase = new CheckpointVehicleCase
            {
                caseId = "CASE-TEST-SPY",
                civilianName = "Gregor Vasili",
                vehicleType = VehicleType.BlackMarketSedan,
                licensePlate = "VAL-777-33",
                transitPassport = new TransitPassport("Gregor Vasili", "ID-VAL-3333", "Kaelen Frontier Province", "Year 2028, Sem 1", false, true, "Trade", "Signed"),
                cargoManifest = new CargoManifest("Luggage", 300f, "Capital", HiddenCargoType.None, "Clean"),
                isCounterfeitPassport = false,
                isWantedInfiltrator = true,
                wantedBulletinDescription = "ALERT #104: Red Hand partisan insurgent operative"
            };

            soldier.accumulatedPromotionPoints = 0;
            var spyArrestResult = CheckpointInspectionEngine.ProcessPlayerDecision(spyCase, CheckpointDecision.DetainDriver, soldier);
            AssertCondition(spyArrestResult.success, "Detaining wanted infiltrator must succeed.");
            AssertCondition(spyArrestResult.wantedAgentCaptured, "Wanted agent must be marked as captured.");
            AssertCondition(spyArrestResult.promotionPointsDelta >= 20, $"Expected >= 20 promotion points, got {spyArrestResult.promotionPointsDelta}.");
            AssertCondition(spyArrestResult.cashBountyDelta >= 500L, $"Expected >= $500 bounty, got {spyArrestResult.cashBountyDelta}.");
            Debug.Log("<color=#00FF66>[Test 6 PASSED]</color> Wanted infiltrator captured for major intelligence coup.");

            // ==================== TEST 7: FIRING RANGE POPUP TARGET GENERATION ====================
            Debug.Log("[Test 7] Verifying firing range trial target generation (50m, 100m, 200m, 300m & friendlies)...");
            var targets = FiringRangeEngine.GenerateTrialTargets(trialCount: 10, seed: 42);
            AssertCondition(targets != null && targets.Count == 10, $"Expected 10 trial targets, got {targets?.Count ?? 0}.");

            bool hasFriendly = false;
            bool hasHostile = false;
            bool hasLongRange = false;

            foreach (var t in targets)
            {
                if (t.IsFriendly) hasFriendly = true;
                if (t.IsHostile) hasHostile = true;
                if (t.distanceMeters >= 300f) hasLongRange = true;
            }

            AssertCondition(hasFriendly, "Target course must include friendly shoot/no-shoot targets.");
            AssertCondition(hasHostile, "Target course must include hostile targets.");
            AssertCondition(hasLongRange, "Target course must include 300m long range targets.");

            int maxScore = FiringRangeEngine.CalculateMaxScore(targets);
            AssertCondition(maxScore > 150, $"Max score should exceed 150, got {maxScore}.");
            Debug.Log("<color=#00FF66>[Test 7 PASSED]</color> 10-target firing range course verified.");

            // ==================== TEST 8: FIRING RANGE TIMING & SHOOT / NO-SHOOT ACCURACY ====================
            Debug.Log("[Test 8] Verifying shot evaluation, reaction timing, and friendly target fire discipline...");
            var friendlyTarget = new TargetPopup(4, 100f, 2.5f, TargetType.FriendlyCivilian, 10, "Friendly Civilian");

            // Holding fire on friendly -> Success & points
            var holdFireResult = FiringRangeEngine.EvaluateShot(friendlyTarget, soldier, playerFired: false);
            AssertCondition(holdFireResult.pointsAwarded == 10, $"Holding fire on friendly should award +10 pts, got {holdFireResult.pointsAwarded}.");
            AssertCondition(!holdFireResult.wasFriendlyFiredUpon, "Friendly should not be marked as fired upon.");

            // Firing on friendly -> Severe violation (-25 pts)
            var friendlyFireResult = FiringRangeEngine.EvaluateShot(friendlyTarget, soldier, playerFired: true, customReactionTime: 0.5f);
            AssertCondition(friendlyFireResult.wasFriendlyFiredUpon, "Friendly must be flagged as fired upon.");
            AssertCondition(friendlyFireResult.pointsAwarded == -25, $"Firing on friendly should result in -25 penalty, got {friendlyFireResult.pointsAwarded}.");

            // Hostile hit within time window
            var hostileTarget = new TargetPopup(1, 50f, 2.0f, TargetType.HostileSilhouette, 20, "50m Hostile");
            var cleanHit = FiringRangeEngine.EvaluateShot(hostileTarget, soldier, playerFired: true, customReactionTime: 0.4f);
            AssertCondition(cleanHit.targetHit, "Fast shot at 50m must hit.");
            AssertCondition(cleanHit.pointsAwarded == 20, $"Hostile hit must award 20 pts, got {cleanHit.pointsAwarded}.");

            // Hostile timeout (Reaction too slow)
            var lateShot = FiringRangeEngine.EvaluateShot(hostileTarget, soldier, playerFired: true, customReactionTime: 3.5f);
            AssertCondition(!lateShot.targetHit, "Late shot past popup window must miss.");
            AssertCondition(lateShot.pointsAwarded == 0, "Late shot must award 0 points.");
            Debug.Log("<color=#00FF66>[Test 8 PASSED]</color> Reaction timing and shoot/no-shoot logic confirmed.");

            // ==================== TEST 9: FIRING RANGE QUALIFICATION TIERS & MEDALS ====================
            Debug.Log("[Test 9] Verifying qualification tiers (Unqualified, Marksman, Sharpshooter, Expert)...");

            var expertResult = FiringRangeEngine.CalculateQualification(190, 200);
            AssertCondition(expertResult.tier == QualificationTier.Expert, "190/200 (95%) must be Expert.");
            AssertCondition(expertResult.badgeAwarded == "Expert Marksmanship Badge", "Expert tier must award Expert Marksmanship Badge.");
            AssertCondition(expertResult.cashPrizeBonus == 300L, "Expert cash prize should be $300.");
            AssertCondition(expertResult.promotionPointsAwarded == 15, "Expert promotion points should be 15.");

            var sharpshooterResult = FiringRangeEngine.CalculateQualification(160, 200);
            AssertCondition(sharpshooterResult.tier == QualificationTier.Sharpshooter, "160/200 (80%) must be Sharpshooter.");
            AssertCondition(sharpshooterResult.promotionPointsAwarded == 10, "Sharpshooter promotion points should be 10.");

            var marksmanResult = FiringRangeEngine.CalculateQualification(120, 200);
            AssertCondition(marksmanResult.tier == QualificationTier.Marksman, "120/200 (60%) must be Marksman.");
            AssertCondition(marksmanResult.promotionPointsAwarded == 5, "Marksman promotion points should be 5.");

            var failResult = FiringRangeEngine.CalculateQualification(60, 200);
            AssertCondition(failResult.tier == QualificationTier.Unqualified, "60/200 (30%) must be Unqualified.");
            AssertCondition(failResult.badgeAwarded == "None", "Unqualified tier should award no badge.");
            AssertCondition(failResult.promotionPointsAwarded == 0, "Unqualified tier should award 0 promotion points.");
            Debug.Log("<color=#00FF66>[Test 9 PASSED]</color> All 4 qualification tiers verified.");

            // ==================== TEST 10: FULL FIRING RANGE ROUND EXECUTION ====================
            Debug.Log("[Test 10] Verifying full automated qualification round applied to SoldierProfile...");
            var expertSoldier = new SoldierProfile("Corporal Hawk", null, 120);
            expertSoldier.combatSkill = 90f;
            expertSoldier.marksmanshipScore = 90f;
            expertSoldier.physicalFitness = 90f;
            expertSoldier.accumulatedPromotionPoints = 0;
            expertSoldier.cashBalance = 500L;

            var roundResult = FiringRangeEngine.RunFullQualificationRound(expertSoldier, seed: 10);
            AssertCondition(roundResult != null, "Round result must not be null.");
            AssertCondition(roundResult.trials.Count == 10, $"Expected 10 trials recorded, got {roundResult.trials.Count}.");
            AssertCondition(roundResult.scorePercentage >= 50f, "High-skill soldier should at least qualify.");
            AssertCondition(expertSoldier.accumulatedPromotionPoints >= 5, "Soldier should have received promotion points.");
            AssertCondition(expertSoldier.cashBalance > 500L, "Soldier should have received qualification cash prize.");
            Debug.Log("<color=#00FF66>[Test 10 PASSED]</color> Full round executed and service record updated.");

            // ==================== TEST 11: BARRACKS CRAPS & DICE GAMBLING MATH ====================
            Debug.Log("[Test 11] Verifying Barracks Craps (Natural 7/11, Boxcars jackpot, Showdown & Push)...");
            var gambler = new SoldierProfile("Private Lucky", null, 100);
            gambler.cashBalance = 1000L;
            gambler.happiness = 50f;
            gambler.InitializeSquadMatesIfEmpty();

            // Test Natural 7 (Instant win, 1.5x profit)
            var nat7 = BarracksDiceEngine.PlayDiceRoll(gambler, wagerAmount: 100, "Doc",
                fixedPlayerD1: 3, fixedPlayerD2: 4, fixedOpponentD1: 6, fixedOpponentD2: 6);
            AssertCondition(nat7.won, "Natural 7 must be a win.");
            AssertCondition(nat7.isNatural7Or11, "Must be flagged as natural 7/11.");
            AssertCondition(nat7.netProfit == 150L, $"Expected $150 net profit on $100 wager, got {nat7.netProfit}.");
            AssertCondition(gambler.cashBalance == 1150L, $"Expected cash 1150, got {gambler.cashBalance}.");

            // Test Boxcars 12 (Critical High-Roller Jackpot, 2.5x profit)
            var boxcars = BarracksDiceEngine.PlayDiceRoll(gambler, wagerAmount: 100, "Anvil",
                fixedPlayerD1: 6, fixedPlayerD2: 6, fixedOpponentD1: 5, fixedOpponentD2: 5);
            AssertCondition(boxcars.won, "Boxcars must be a win.");
            AssertCondition(boxcars.isJackpotBoxcarsOrSnakeEyes, "Must be flagged as Boxcars jackpot.");
            AssertCondition(boxcars.netProfit == 250L, $"Expected $250 net profit on $100 wager, got {boxcars.netProfit}.");
            AssertCondition(gambler.cashBalance == 1400L, $"Expected cash 1400, got {gambler.cashBalance}.");

            // Test Defeat (Player 4 vs Opponent 9)
            var defeat = BarracksDiceEngine.PlayDiceRoll(gambler, wagerAmount: 100, "Rookie",
                fixedPlayerD1: 1, fixedPlayerD2: 3, fixedOpponentD1: 4, fixedOpponentD2: 5);
            AssertCondition(!defeat.won, "Player 4 vs Opponent 9 must lose.");
            AssertCondition(defeat.netProfit == -100L, $"Expected -$100 net profit, got {defeat.netProfit}.");
            AssertCondition(gambler.cashBalance == 1300L, $"Expected cash 1300, got {gambler.cashBalance}.");

            // Test Tie / Push (Player 8 vs Opponent 8)
            var tie = BarracksDiceEngine.PlayDiceRoll(gambler, wagerAmount: 100, "Deadeye",
                fixedPlayerD1: 4, fixedPlayerD2: 4, fixedOpponentD1: 5, fixedOpponentD2: 3);
            AssertCondition(!tie.won && tie.isTie, "Ties must be flagged as isTie.");
            AssertCondition(tie.netProfit == 0L, "Ties must yield 0 net profit.");
            AssertCondition(gambler.cashBalance == 1300L, "Ties must leave cash balance unchanged.");
            Debug.Log("<color=#00FF66>[Test 11 PASSED]</color> Barracks Craps gambling math verified 100%.");

            // ==================== TEST 12: MOS CAREER SPECIALIZATIONS ====================
            Debug.Log("[Test 12] Verifying MOS Specializations (Line Infantry, Medic, Sniper/Recon, Combat Engineer)...");
            var allMOS = MOSRegistry.GetAllMOS();
            AssertCondition(allMOS != null && allMOS.Count == 4, $"Expected 4 MOS tracks, got {allMOS?.Count ?? 0}.");

            var medic = MOSRegistry.GetMOS(MOSSpecialization.CombatMedic);
            AssertCondition(medic.triageBonus >= 30f, "Combat Medic must possess high triage bonus.");
            AssertCondition(medic.mosCode == "68W", "Medic code should be 68W.");

            var sniper = MOSRegistry.GetMOS(MOSSpecialization.SniperRecon);
            AssertCondition(sniper.marksmanshipBonus >= 20f, "Sniper/Recon must possess high marksmanship bonus.");
            AssertCondition(sniper.mosCode == "19D", "Sniper code should be 19D.");

            var engineer = MOSRegistry.GetMOS(MOSSpecialization.CombatEngineer);
            AssertCondition(engineer.checkpointSearchBonus >= 30f, "Combat Engineer must possess high search bonus.");
            AssertCondition(engineer.demolitionBonus >= 30f, "Combat Engineer must possess high demolition bonus.");

            var infantry = MOSRegistry.GetMOS(MOSSpecialization.LineInfantry);
            AssertCondition(infantry.mosCode == "11B", "Line Infantry code should be 11B.");

            // Assign MOS to soldier and test trait synchronization
            var mosSoldier = new SoldierProfile("Specialist Briggs", null, 100);
            MOSRegistry.AssignMOS(mosSoldier, MOSSpecialization.CombatEngineer);
            AssertCondition(MOSRegistry.GetSoldierMOS(mosSoldier) == MOSSpecialization.CombatEngineer, "Soldier MOS must be Combat Engineer.");
            AssertCondition(mosSoldier.HasTrait("MOS: Combat Engineer & Sapper"), "Soldier must have MOS trait.");
            AssertCondition(mosSoldier.HasTrait("Sapper Breach Instinct"), "Soldier must have Sapper perk trait.");
            Debug.Log("<color=#00FF66>[Test 12 PASSED]</color> MOS Specialization registry and traits verified.");

            // ==================== TEST 13: TACTICAL MINIGAMES MODAL LIFECYCLE ====================
            Debug.Log("[Test 13] Verifying Tactical Minigames Modal lifecycle and tab navigation...");
            var modalObj = new GameObject("TacticalMinigamesModal_TestInstance");
            try
            {
                var modal = modalObj.AddComponent<TacticalMinigamesModal>();
                AssertCondition(modal != null, "Modal component must not be null.");

                modal.OpenModal(MinigameTab.Checkpoint);
                AssertCondition(modalObj.activeSelf, "Modal GameObject should be active after OpenModal.");

                modal.SwitchTab(MinigameTab.FiringRange);
                modal.SwitchTab(MinigameTab.BarracksDice);
                modal.CloseModal();
                Debug.Log("<color=#00FF66>[Test 13 PASSED]</color> Modal UI lifecycle and navigation verified.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(modalObj);
            }

            Debug.Log("<color=#00FFAA>========================================================================</color>");
            Debug.Log("<color=#00FFAA>=== ALL TACTICAL MINIGAMES & MOS ENGINE TESTS PASSED 100% SUCCEEDED! ===</color>");
            Debug.Log("<color=#00FFAA>========================================================================</color>");
        }

        /// <summary>
        /// Batchmode runner entry point for automated Unity execution from CLI.
        /// </summary>
        public static void RunTestBatch()
        {
            try
            {
                RunAllMinigamesTests();
                Debug.Log("[BATCH COMPLETE] All Tactical Minigames & MOS tests passed with zero failures.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BATCH FAILED] Minigames test runner threw an unhandled exception: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }

        private static void AssertCondition(bool condition, string failureMessage)
        {
            if (!condition)
            {
                Debug.LogError($"<color=#FF4444>[TEST ASSERTION FAILED]</color> {failureMessage}");
                throw new Exception($"Minigames Test Assertion Failed: {failureMessage}");
            }
        }
    }
}
#endif
