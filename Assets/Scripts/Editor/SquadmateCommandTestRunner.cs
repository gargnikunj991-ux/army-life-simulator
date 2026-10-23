#if UNITY_EDITOR
using System;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using UnityEditor;
using UnityEngine;

namespace ArmyLife.Editor
{
    /// <summary>
    /// Automated test runner verifying Milestone 1: Squadmate Comrades & Tactical Command Engine
    /// (The "Band of Brothers" Pillar).
    /// </summary>
    public static class SquadmateCommandTestRunner
    {
        [MenuItem("Army Life/Run Squadmates & Tactical Command Test", false, 22)]
        public static void RunSquadmatesAndCommandTest()
        {
            Debug.Log("<color=#00D4FF>========== STARTING SQUADMATES & TACTICAL COMMAND TESTS ==========</color>");

            // ==================== TEST 1: SQUAD INITIALIZATION ====================
            Debug.Log("[Test 1] Verifying squadmate initialization (Doc, Deadeye, Anvil, Rookie)...");
            var soldier = new SoldierProfile("Sergeant Marcus Vance", null, 120);
            soldier.InitializeSquadMatesIfEmpty();

            AssertCondition(soldier.squadMates != null, "Squadmates list must not be null.");
            AssertCondition(soldier.squadMates.Count == 4, $"Expected 4 starting squadmates, got {soldier.squadMates.Count}.");

            var doc = soldier.squadMates.Find(m => m.role == SquadRole.CombatMedic);
            var deadeye = soldier.squadMates.Find(m => m.role == SquadRole.LeadScout);
            var anvil = soldier.squadMates.Find(m => m.role == SquadRole.HeavyGunner);
            var rookie = soldier.squadMates.Find(m => m.role == SquadRole.RiflemanRookie);

            AssertCondition(doc != null && doc.nickname == "Doc", "Combat Medic 'Doc' must be present.");
            AssertCondition(deadeye != null && deadeye.nickname == "Deadeye", "Lead Scout 'Deadeye' must be present.");
            AssertCondition(anvil != null && anvil.nickname == "Anvil", "Heavy Gunner 'Anvil' must be present.");
            AssertCondition(rookie != null && rookie.nickname == "Rookie", "Rifleman Rookie 'Rookie' must be present.");

            AssertCondition(!doc.isDead && !deadeye.isDead && !anvil.isDead && !rookie.isDead,
                "All squadmates must start alive.");
            AssertCondition(deadeye.relationship > 50f, "Deadeye starting relationship should be > 50f.");

            Debug.Log("<color=#00FF66>[Test 1 PASSED]</color> 4-man fireteam squadmates successfully initialized.");

            // ==================== TEST 2: CAMARADERIE & SHARING CIGARETTES ====================
            Debug.Log("[Test 2] Verifying sharing cigarettes with squadmates...");
            soldier.cigarettesCount = 3;
            soldier.happiness = 60f;
            float rookieRelBefore = rookie.relationship;

            SquadmateCommandEngine.ShareCigarettesWithSquad(soldier, rookie);

            AssertCondition(soldier.cigarettesCount == 2,
                $"Cigarettes count should decrease by 1. Expected 2, got {soldier.cigarettesCount}.");
            AssertCondition(Mathf.Approximately(rookie.relationship, rookieRelBefore + 15f),
                $"Rookie relationship should increase by +15. Expected {rookieRelBefore + 15f}, got {rookie.relationship}.");
            AssertCondition(soldier.happiness > 60f,
                $"Soldier morale/happiness should increase from smoke break. Got {soldier.happiness}.");

            Debug.Log("<color=#00FF66>[Test 2 PASSED]</color> Sharing cigarettes boosts relationship (+15) and morale.");

            // ==================== TEST 3: HEROIC FIRST AID & TRAUMA BANDAGING ====================
            Debug.Log("[Test 3] Verifying applying emergency trauma bandages to wounded squadmate...");
            soldier.fieldMedkitsCount = 2;
            float docRelBefore = doc.relationship;

            SquadmateCommandEngine.BandageSquadmate(soldier, doc);

            AssertCondition(soldier.fieldMedkitsCount == 1,
                $"Field medkits count should decrease by 1. Expected 1, got {soldier.fieldMedkitsCount}.");
            AssertCondition(Mathf.Approximately(doc.relationship, docRelBefore + 30f),
                $"Doc relationship should increase by +30f. Expected {docRelBefore + 30f}, got {doc.relationship}.");
            AssertCondition(soldier.HasTrait("Life Saver"),
                "Soldier must acquire the 'Life Saver' story trait.");
            AssertCondition(soldier.HasMedal("Imperial Life Saving Cross"),
                "Soldier must be decorated with the Imperial Life Saving Cross.");

            Debug.Log("<color=#00FF66>[Test 3 PASSED]</color> Bandaging wounded comrade grants +30 loyalty and Life Saver decoration.");

            // ==================== TEST 4: COMMAND AUTHORITY ENFORCEMENT ====================
            Debug.Log("[Test 4] Verifying command authority (E-5+ Sergeant / O-1+ Officer)...");
            var privateSoldier = new SoldierProfile("Private Terry", null, 60);
            privateSoldier.currentRank = ScriptableObject.CreateInstance<MilitaryRankSO>();
            privateSoldier.currentRank.rankName = "Private Recruit";
            privateSoldier.currentRank.paygrade = "E-1";
            privateSoldier.currentRank.type = PaygradeType.Enlisted;
            privateSoldier.currentRank.rankOrder = 1;

            var theater = new WarTheater(WarTheaterType.WesternMudTrenches, "Sector 7 Mud Trenches", 1.0f, "Mud & Barbed Wire");

            var deniedOutcome = SquadmateCommandEngine.ExecuteTacticalCommand(privateSoldier, TacticalCommandOrder.FrontalAssault, theater);
            AssertCondition(!deniedOutcome.success, "Tactical command must fail for E-1 Private without command authority.");
            AssertCondition(deniedOutcome.headline.Contains("Denied") || deniedOutcome.headline.Contains("Authority"),
                "Outcome headline must indicate authority denial.");

            // Promote to Sergeant (E-5)
            soldier.currentRank = ScriptableObject.CreateInstance<MilitaryRankSO>();
            soldier.currentRank.rankName = "Sergeant";
            soldier.currentRank.paygrade = "E-5";
            soldier.currentRank.type = PaygradeType.Enlisted;
            soldier.currentRank.rankOrder = 5;

            AssertCondition(SquadmateCommandEngine.IsNCOOrOfficer(soldier),
                "Sergeant (E-5) must be recognized as an NCO with command authority.");

            // Check Officer (O-1)
            var officer = new SoldierProfile("Lieutenant Vance", null, 140);
            officer.currentRank = ScriptableObject.CreateInstance<MilitaryRankSO>();
            officer.currentRank.rankName = "2nd Lieutenant";
            officer.currentRank.paygrade = "O-1";
            officer.currentRank.type = PaygradeType.Officer;
            officer.currentRank.rankOrder = 10;

            AssertCondition(SquadmateCommandEngine.IsNCOOrOfficer(officer),
                "2nd Lieutenant (O-1) must be recognized as an Officer with command authority.");

            Debug.Log("<color=#00FF66>[Test 4 PASSED]</color> Command authority properly restricted to NCOs (E-5+) and Officers (O-1+).");

            // ==================== TEST 5: TACTICAL COMMAND - SCOUT FLANK MANEUVER ====================
            Debug.Log("[Test 5] Verifying ScoutFlankManeuver with high relationship Lead Scout (+25% bonus)...");
            AssertCondition(deadeye.relationship > 50f, "Deadeye relationship must be > 50 for flank synergy.");

            float skillBefore = soldier.combatSkill;
            float meritBefore = soldier.meritRating;

            var flankOutcome = SquadmateCommandEngine.ExecuteTacticalCommand(
                soldier,
                TacticalCommandOrder.ScoutFlankManeuver,
                theater,
                forceSuccess: true
            );

            AssertCondition(flankOutcome.success, "Scout flank maneuver should succeed.");
            AssertCondition(flankOutcome.combatSkillChange > 0f, "Combat skill change should be positive on success.");
            AssertCondition(flankOutcome.meritChange > 0f, "Merit change should be positive on success.");
            AssertCondition(soldier.combatSkill > skillBefore, "Leader combat skill should increase.");
            AssertCondition(soldier.meritRating > meritBefore, "Leader merit rating should increase.");
            AssertCondition(!string.IsNullOrEmpty(flankOutcome.headline), "Outcome headline must not be empty.");
            AssertCondition(flankOutcome.feedMessage.Contains("Deadeye") || flankOutcome.feedMessage.Contains("25%"),
                "Feed message should highlight Lead Scout recon bonus.");

            Debug.Log("<color=#00FF66>[Test 5 PASSED]</color> ScoutFlankManeuver executed with Deadeye recon synergy.");

            // ==================== TEST 6: TACTICAL COMMAND - HEAVY SUPPRESSION PIN ====================
            Debug.Log("[Test 6] Verifying HeavySuppressionPin casualty reduction from Heavy Gunner Anvil...");
            AssertCondition(!anvil.isDead, "Heavy Gunner Anvil must be alive for suppression test.");

            var suppressOutcome = SquadmateCommandEngine.ExecuteTacticalCommand(
                soldier,
                TacticalCommandOrder.HeavySuppressionPin,
                theater,
                forceSuccess: true
            );

            AssertCondition(suppressOutcome.success, "Heavy suppression pin should succeed.");
            AssertCondition(suppressOutcome.casualties <= 1,
                $"Heavy gunner suppression combined with medic should keep casualties to 0 or 1. Got {suppressOutcome.casualties}.");
            AssertCondition(suppressOutcome.feedMessage.Contains("Anvil") || suppressOutcome.feedMessage.Contains("suppression"),
                "Feed message should credit Heavy Gunner suppression.");

            Debug.Log("<color=#00FF66>[Test 6 PASSED]</color> HeavySuppressionPin significantly mitigates subordinate casualties.");

            // ==================== TEST 7: TACTICAL COMMAND - DANGER CLOSE ARTILLERY ====================
            Debug.Log("[Test 7] Verifying DangerCloseArtilleryCall...");
            var artilleryOutcome = SquadmateCommandEngine.ExecuteTacticalCommand(
                soldier,
                TacticalCommandOrder.DangerCloseArtilleryCall,
                theater,
                forceSuccess: true
            );

            AssertCondition(artilleryOutcome.success, "Danger close artillery fire mission should succeed.");
            AssertCondition(artilleryOutcome.meritChange >= 25f,
                $"Artillery call should yield high merit reward (>=25). Got {artilleryOutcome.meritChange}.");
            AssertCondition(artilleryOutcome.headline.Contains("Artillery"),
                "Artillery headline should mention artillery.");

            Debug.Log("<color=#00FF66>[Test 7 PASSED]</color> DangerCloseArtilleryCall successfully executes with high merit reward.");

            // ==================== TEST 8: COMBAT MEDIC HALVES TROOP WOUND MORTALITY ====================
            Debug.Log("[Test 8] Verifying Combat Medic Doc halves subordinate casualties...");
            // Test with doc alive
            var outcomeWithMedic = SquadmateCommandEngine.ExecuteTacticalCommand(
                soldier,
                TacticalCommandOrder.FrontalAssault,
                theater,
                forceSuccess: true,
                overrideSubordinateCasualties: 4
            );
            // 4 raw casualties halved by medic -> 2
            AssertCondition(outcomeWithMedic.casualties == 2,
                $"With Combat Medic alive, 4 casualties should be halved to 2. Got {outcomeWithMedic.casualties}.");

            Debug.Log("<color=#00FF66>[Test 8 PASSED]</color> Combat Medic triage halves subordinate casualty mortality.");

            // ==================== TEST 9: SQUADMATE COMBAT LOSS WITH RESCUE INTERVENTION ====================
            Debug.Log("[Test 9] Verifying squadmate rescue during frontline combat...");
            int initialKIA = soldier.squadMatesKIA;

            // Combat loss evaluation where comrades can assist
            SquadmateCommandEngine.CheckSquadmateCombatLoss(soldier, theater, forceLoss: true, forcedMate: rookie, allowAssistance: true);

            AssertCondition(soldier.squadMatesKIA == initialKIA,
                "Rookie should have been rescued by Doc/Gunner/Scout, preventing KIA.");
            AssertCondition(!rookie.isDead, "Rookie should still be alive following squadmate rescue.");

            Debug.Log("<color=#00FF66>[Test 9 PASSED]</color> Squadmate rescue intervention successfully protects comrade from death.");

            // ==================== TEST 10: SQUADMATE KIA TRAGEDY & RECRUIT REPLACEMENT ====================
            Debug.Log("[Test 10] Verifying squadmate KIA tragedy and replacement recruit...");
            float moraleBeforeKIA = soldier.happiness;

            // Force KIA without assistance to simulate unpreventable tragedy
            SquadmateCommandEngine.CheckSquadmateCombatLoss(soldier, theater, forceLoss: true, forcedMate: rookie, allowAssistance: false);

            AssertCondition(soldier.squadMatesKIA == initialKIA + 1,
                $"squadMatesKIA should increment by 1. Expected {initialKIA + 1}, got {soldier.squadMatesKIA}.");
            AssertCondition(rookie.isDead, "Rookie must be marked as dead following KIA tragedy.");
            AssertCondition(!string.IsNullOrEmpty(rookie.deathReason), "Death reason must be recorded.");
            AssertCondition(soldier.happiness < moraleBeforeKIA,
                "Player morale should decrease following the loss of a brother in arms.");

            // Recruit replacement to fill the KIA vacancy
            Debug.Log("[Test 10b] Recruiting replacement recruit to fill Rifleman vacancy...");
            SquadmateCommandEngine.RecruitReplacement(soldier, SquadRole.RiflemanRookie);

            var replacementRookie = soldier.squadMates.Find(m => m.role == SquadRole.RiflemanRookie && !m.isDead);
            AssertCondition(replacementRookie != null, "Replacement Rifleman recruit must be added and alive.");
            AssertCondition(soldier.squadMates.Count == 4,
                $"Squad count must return to 4 after replacement. Got {soldier.squadMates.Count}.");

            // Verify all 4 roles are active and alive
            AssertCondition(soldier.squadMates.Exists(m => m.role == SquadRole.CombatMedic && !m.isDead), "Doc must be alive.");
            AssertCondition(soldier.squadMates.Exists(m => m.role == SquadRole.LeadScout && !m.isDead), "Deadeye must be alive.");
            AssertCondition(soldier.squadMates.Exists(m => m.role == SquadRole.HeavyGunner && !m.isDead), "Anvil must be alive.");
            AssertCondition(soldier.squadMates.Exists(m => m.role == SquadRole.RiflemanRookie && !m.isDead), "Replacement rookie must be alive.");

            Debug.Log("<color=#00FF66>[Test 10 PASSED]</color> Squadmate KIA tragedy and recruit replacement workflow verified.");

            // ==================== TEST 11: NARRATIVE FEED & STATS EVENT LOGGING ====================
            Debug.Log("[Test 11] Verifying GameEvents feed and stats dispatch...");
            bool feedReceived = false;
            string receivedTag = string.Empty;
            string receivedMessage = string.Empty;

            Action<string, string> feedHandler = (tag, msg) =>
            {
                feedReceived = true;
                receivedTag = tag;
                receivedMessage = msg;
            };

            GameEvents.OnFeedEventLogged += feedHandler;
            try
            {
                SquadmateCommandEngine.ShareCigarettesWithSquad(soldier, replacementRookie);
                AssertCondition(feedReceived, "GameEvents.OnFeedEventLogged should fire when sharing cigarettes.");
                AssertCondition(receivedTag == "CAMARADERIE", $"Expected feed tag 'CAMARADERIE', got '{receivedTag}'.");
            }
            finally
            {
                GameEvents.OnFeedEventLogged -= feedHandler;
            }

            Debug.Log("<color=#00FF66>[Test 11 PASSED]</color> GameEvents narrative feed integration confirmed.");

            Debug.Log("<color=#00FFAA>========== ALL SQUADMATES & TACTICAL COMMAND TESTS PASSED 100%! ==========</color>");
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
