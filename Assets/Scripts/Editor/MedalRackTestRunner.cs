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
    /// Automated test runner verifying Agent 2 Module:
    /// Visual Uniform, Medal Ribbon Rack, Historical Citations Dossier & Regimental Memorial Wall.
    /// </summary>
    public static class MedalRackTestRunner
    {
        [MenuItem("Army Life/Run Medal Rack Tests", false, 24)]
        public static void RunAllTests()
        {
            Debug.Log("<color=#00D4FF>========================================================================</color>");
            Debug.Log("<color=#00D4FF>==== STARTING MEDAL RIBBON RACK & MEMORIAL WALL TEST SUITE (AGENT 2) ===</color>");
            Debug.Log("<color=#00D4FF>========================================================================</color>");

            // ==================== TEST 1: REGISTRY DATA INTEGRITY ====================
            Debug.Log("[Test 1] Verifying all authentic military decorations in MedalRegistry...");
            var allMedals = MedalRegistry.GetAllOfficialMedals();
            AssertCondition(allMedals != null && allMedals.Count >= 15,
                $"Expected at least 15 official decorations, got {allMedals?.Count ?? 0}.");

            // Verify core highest valor medals
            var grandStar = MedalRegistry.GetDefinition("Grand Imperial Star");
            AssertCondition(grandStar.precedenceOrder == 1, $"Grand Imperial Star precedence should be 1, got {grandStar.precedenceOrder}.");
            AssertCondition(grandStar.imperialBounty == 75000L, $"Grand Imperial Star bounty should be $75k, got {grandStar.imperialBounty}.");
            AssertCondition(grandStar.GetColors().Length >= 3, "Grand Imperial Star should have multi-stripe ribbon colors.");

            var dsc = MedalRegistry.GetDefinition("Distinguished Service Cross");
            AssertCondition(dsc.precedenceOrder == 2, $"DSC precedence should be 2, got {dsc.precedenceOrder}.");

            var silverCross = MedalRegistry.GetDefinition("Silver Cross");
            AssertCondition(silverCross.precedenceOrder == 3, $"Silver Cross precedence should be 3, got {silverCross.precedenceOrder}.");
            AssertCondition(silverCross.imperialBounty == 15000L, $"Silver Cross bounty should be $15,000, got {silverCross.imperialBounty}.");

            var bronzeStar = MedalRegistry.GetDefinition("Bronze Star");
            AssertCondition(bronzeStar.precedenceOrder == 4, $"Bronze Star precedence should be 4, got {bronzeStar.precedenceOrder}.");

            var woundStar = MedalRegistry.GetDefinition("Imperial Wound Star");
            AssertCondition(woundStar.precedenceOrder == 5, $"Imperial Wound Star precedence should be 5, got {woundStar.precedenceOrder}.");
            AssertCondition(woundStar.category == MedalCategory.WoundsAndCasualties, "Wound star category mismatch.");

            var valorCitation = MedalRegistry.GetDefinition("Valor Citation");
            AssertCondition(valorCitation.precedenceOrder == 6, $"Valor Citation precedence should be 6, got {valorCitation.precedenceOrder}.");

            // Verify all 5 authentic campaign medals
            var arden = MedalRegistry.GetDefinition("Arden Ridge Campaign");
            var portIron = MedalRegistry.GetDefinition("Port Iron Beachhead");
            var kaelen = MedalRegistry.GetDefinition("Kaelen Urban Pacification");
            var badlands = MedalRegistry.GetDefinition("Badlands Steppe Patrol");
            var taiga = MedalRegistry.GetDefinition("Northern Taiga Winter Defense");

            AssertCondition(arden != null && arden.category == MedalCategory.CampaignAndTheaters, "Arden Ridge campaign medal missing.");
            AssertCondition(portIron != null && portIron.category == MedalCategory.CampaignAndTheaters, "Port Iron beachhead medal missing.");
            AssertCondition(kaelen != null && kaelen.category == MedalCategory.CampaignAndTheaters, "Kaelen urban medal missing.");
            AssertCondition(badlands != null && badlands.category == MedalCategory.CampaignAndTheaters, "Badlands patrol medal missing.");
            AssertCondition(taiga != null && taiga.category == MedalCategory.CampaignAndTheaters, "Taiga winter defense medal missing.");

            // Verify service conduct & qualification
            var goodConduct = MedalRegistry.GetDefinition("Valerian Merit Ribbon");
            var longevity = MedalRegistry.GetDefinition("Longevity Service Hash");
            var cib = MedalRegistry.GetDefinition("Combat Infantry Badge");

            AssertCondition(goodConduct != null, "Good conduct ribbon missing.");
            AssertCondition(longevity != null && longevity.precedenceOrder == 31, "Longevity hash definition missing or order wrong.");
            AssertCondition(cib != null && cib.precedenceOrder == 32, "CIB definition missing.");

            Debug.Log("<color=#00FF66>[Test 1 PASSED]</color> MedalRegistry integrity and full decoration catalog verified.");

            // ==================== TEST 2: FUZZY ALIAS NORMALIZATION ====================
            Debug.Log("[Test 2] Verifying fuzzy alias matching and robustness...");
            AssertCondition(MedalRegistry.GetDefinition("Medal of Honor").medalId == "MEDAL_OF_HONOR", "Medal of Honor alias failed.");
            AssertCondition(MedalRegistry.GetDefinition("Silver Vanguard Cross").medalId == "SILVER_CROSS", "Silver Vanguard alias failed.");
            AssertCondition(MedalRegistry.GetDefinition("Imperial Purple Heart").medalId == "IMPERIAL_WOUND_STAR", "Purple Heart alias failed.");
            AssertCondition(MedalRegistry.GetDefinition("Imperial Wound Star (Posthumous)").medalId == "IMPERIAL_WOUND_STAR", "Posthumous wound star alias failed.");
            AssertCondition(MedalRegistry.GetDefinition("Special Operations Valor Cross (Posthumous)").medalId == "SPECIAL_OPS_CROSS", "Special ops posthumous alias failed.");
            AssertCondition(MedalRegistry.GetDefinition("Celestial Valkyrie Wings").medalId == "DISTINGUISHED_FLYING_CROSS", "Valkyrie Wings alias failed.");

            // Graceful fallback for completely unknown custom medal
            var customFallback = MedalRegistry.GetDefinition("Unknown Foreign Decoration");
            AssertCondition(customFallback != null, "Fallback medal should never be null.");
            AssertCondition(customFallback.displayName == "Unknown Foreign Decoration", "Fallback medal should preserve display name.");

            Debug.Log("<color=#00FF66>[Test 2 PASSED]</color> Fuzzy medal alias matching confirmed.");

            // ==================== TEST 3: RIBBON RACK PRECEDENCE SORTING ====================
            Debug.Log("[Test 3] Verifying ribbon rack precedence sorting and mounting rows...");
            var soldier = new SoldierProfile("Sergeant Marcus Vance", null, 250);
            soldier.earnedMedals = new List<string>
            {
                "Bronze Star",                  // Order 4
                "Grand Imperial Star",          // Order 1
                "Arden Ridge Campaign",         // Order 20
                "Silver Cross",                 // Order 3
                "Imperial Wound Star"           // Order 5
            };

            var sortedRibbons = MedalRackEngine.GetSortedRibbons(soldier);
            AssertCondition(sortedRibbons.Count == 5, $"Expected 5 sorted ribbons, got {sortedRibbons.Count}.");
            AssertCondition(sortedRibbons[0].medalId == "MEDAL_OF_HONOR", "Precedence #1 (Grand Star) must be index 0.");
            AssertCondition(sortedRibbons[1].medalId == "SILVER_CROSS", "Precedence #3 (Silver Cross) must be index 1.");
            AssertCondition(sortedRibbons[2].medalId == "BRONZE_STAR", "Precedence #4 (Bronze Star) must be index 2.");
            AssertCondition(sortedRibbons[3].medalId == "IMPERIAL_WOUND_STAR", "Precedence #5 (Wound Star) must be index 3.");
            AssertCondition(sortedRibbons[4].medalId == "ARDEN_RIDGE_CAMPAIGN", "Precedence #20 (Campaign) must be index 4.");

            // Verify mounting rows (5 medals -> Row 0 has 2 ribbons centered, Row 1 has 3 ribbons)
            var rows = MedalRackEngine.BuildRibbonRack(soldier);
            AssertCondition(rows.Count == 2, $"Expected 2 mounted rows for 5 ribbons, got {rows.Count}.");
            AssertCondition(rows[0].ribbons.Count == 2, $"Top row must contain remainder 2 ribbons, got {rows[0].ribbons.Count}.");
            AssertCondition(rows[1].ribbons.Count == 3, $"Bottom row must contain 3 ribbons, got {rows[1].ribbons.Count}.");
            AssertCondition(rows[0].ribbons[0].medalId == "MEDAL_OF_HONOR", "Top-left ribbon must be highest valor honor.");

            // Verify 4 medals mounting: Row 0 has 1 ribbon, Row 1 has 3 ribbons
            soldier.earnedMedals.RemoveAt(4);
            var rows4 = MedalRackEngine.BuildRibbonRack(soldier);
            AssertCondition(rows4.Count == 2, "4 medals should mount in 2 rows.");
            AssertCondition(rows4[0].ribbons.Count == 1, $"Top row for 4 medals should have 1 ribbon, got {rows4[0].ribbons.Count}.");
            AssertCondition(rows4[1].ribbons.Count == 3, $"Bottom row for 4 medals should have 3 ribbons, got {rows4[1].ribbons.Count}.");

            Debug.Log("<color=#00FF66>[Test 3 PASSED]</color> Standard military 3-per-row ribbon rack mounting verified.");

            // ==================== TEST 4: HISTORICAL CITATION GENERATION ====================
            Debug.Log("[Test 4] Verifying historical parchment citation generation...");
            soldier.totalSemestersLived = 8; // Year 944, Semester 1
            soldier.currentTheater = WarTheaterType.ArdenRidgeTrenchLines;

            var citation = MedalRackEngine.GenerateCitation(soldier, "Silver Cross");
            AssertCondition(citation != null, "Citation must not be null.");
            AssertCondition(citation.recipientName == "Sergeant Marcus Vance", "Recipient name mismatch.");
            AssertCondition(citation.theaterOfAction == "Arden Ridge Trench Lines", $"Expected Arden Ridge, got {citation.theaterOfAction}.");
            AssertCondition(citation.dateAwarded.Contains("Imperial Year 944"), $"Date should mention Imperial Year 944, got {citation.dateAwarded}.");
            AssertCondition(citation.commandingOfficerName.Contains("Julian Vance"), "CO name mismatch for Imperial Army.");
            AssertCondition(citation.citationText.Contains("gallantry in action"), "Citation text missing key phrase.");
            AssertCondition(citation.imperialSealCode.StartsWith("SEAL-ORD-"), "Imperial seal code missing or malformed.");

            var allCitations = MedalRackEngine.GetAllCitations(soldier);
            var expectedRibbons = MedalRackEngine.GetSortedRibbons(soldier);
            AssertCondition(allCitations.Count == expectedRibbons.Count, 
                $"Expected {expectedRibbons.Count} citations matching mounted decorations, got {allCitations.Count}.");
            AssertCondition(allCitations[0].precedence < allCitations[allCitations.Count - 1].precedence,
                "Citations must be ordered by military precedence.");

            Debug.Log("<color=#00FF66>[Test 4 PASSED]</color> Historical citations dossier and authentic seals confirmed.");

            // ==================== TEST 5: LONGEVITY HASHES & WOUND STARS ====================
            Debug.Log("[Test 5] Verifying longevity hashes and wound stars calculations...");
            soldier.totalSemestersLived = 18; // 9 full years active duty -> 18 / 6 = 3 hashes
            int hashes = MedalRackEngine.CalculateLongevityHashes(soldier);
            AssertCondition(hashes == 3, $"Expected 3 longevity service hashes for 18 semesters, got {hashes}.");

            soldier.woundStarsCount = 2;
            int wounds = MedalRackEngine.CalculateWoundStars(soldier);
            AssertCondition(wounds == 2, $"Expected 2 wound stars, got {wounds}.");

            Debug.Log("<color=#00FF66>[Test 5 PASSED]</color> Longevity hashes and wound stars calculation verified.");

            // ==================== TEST 6: REGIMENTAL MEMORIAL WALL CASUALTY RECORDING ====================
            Debug.Log("[Test 6] Verifying Regimental Memorial Wall synchronization with fallen squadmates...");
            MedalRackEngine.ClearMemorialRegistry();
            soldier.lifeId = "TEST_HERO_LIFE_001";
            soldier.squadMates = new List<SquadMate>
            {
                new SquadMate("Raymond Miller", "Doc", SquadRole.CombatMedic, "Squad Combat Medic", "Warm-Hearted Medic"),
                new SquadMate("Lucas Cole", "Deadeye", SquadRole.LeadScout, "Lead Recon Scout", "Cynical Veteran"),
                new SquadMate("Karl Braun", "Anvil", SquadRole.HeavyGunner, "Heavy Gunner", "Stoic Ironclad"),
                new SquadMate("Thomas Gray", "Rookie", SquadRole.RiflemanRookie, "Rifleman Recruit", "Nervous Farmboy")
            };

            // Kill Doc during a combat mission
            var doc = soldier.squadMates[0];
            doc.isDead = true;
            doc.deathReason = "Direct mortar shrapnel during frontline triage.";

            var memorials = MedalRackEngine.GetMemorialWall(soldier);
            AssertCondition(memorials.Count == 1, $"Expected 1 fallen hero on Memorial Wall, got {memorials.Count}.");

            var docMemorial = memorials[0];
            AssertCondition(docMemorial.soldierName == "Raymond Miller", "Memorial name mismatch.");
            AssertCondition(docMemorial.nickname == "Doc", "Memorial nickname mismatch.");
            AssertCondition(docMemorial.role == SquadRole.CombatMedic, "Memorial role mismatch.");
            AssertCondition(docMemorial.rank == "Corporal", $"Expected Corporal rank for Medic, got {docMemorial.rank}.");
            AssertCondition(docMemorial.causeOfDeath.Contains("mortar"), "Cause of death missing mortar.");
            AssertCondition(docMemorial.finalWords.Contains("Patch up"), "Final words mismatch for Doc.");
            AssertCondition(docMemorial.posthumousMedal.Contains("Imperial Life Saving Cross"), "Posthumous medal mismatch for Doc.");
            AssertCondition(docMemorial.wreathLaid == false, "Wreath should not be laid initially.");

            Debug.Log("<color=#00FF66>[Test 6 PASSED]</color> Regimental Memorial Wall KIA synchronization confirmed.");

            // ==================== TEST 7: MEMORIAL WREATH TRIBUTE & MORALE ====================
            Debug.Log("[Test 7] Verifying laying memorial wreath, paying respects, and morale boosts...");
            soldier.cashBalance = 200L;
            soldier.happiness = 50f;
            soldier.squadRespect = 60f;

            bool feedFired = false;
            string feedTagReceived = "";
            Action<string, string> feedObserver = (tag, msg) =>
            {
                feedFired = true;
                feedTagReceived = tag;
            };

            GameEvents.OnFeedEventLogged += feedObserver;
            try
            {
                bool tributeSuccess = MedalRackEngine.LayWreathAtMemorial(soldier, docMemorial);
                AssertCondition(tributeSuccess, "LayWreathAtMemorial should return true.");
                AssertCondition(docMemorial.wreathLaid == true, "Wreath laid flag must be true.");
                AssertCondition(docMemorial.tributesPaidCount == 1, $"Tributes paid count should be 1, got {docMemorial.tributesPaidCount}.");
                AssertCondition(soldier.cashBalance == 150L, $"Cash should reduce by $50. Expected 150, got {soldier.cashBalance}.");
                AssertCondition(soldier.happiness == 65f, $"Happiness should boost by +15. Expected 65, got {soldier.happiness}.");
                AssertCondition(soldier.squadRespect == 70f, $"Squad respect should boost by +10. Expected 70, got {soldier.squadRespect}.");
                AssertCondition(feedFired && feedTagReceived == "MEMORIAL", "Memorial event feed must be dispatched.");
            }
            finally
            {
                GameEvents.OnFeedEventLogged -= feedObserver;
            }

            Debug.Log("<color=#00FF66>[Test 7 PASSED]</color> Laying memorial wreath mechanics and morale impact verified.");

            // ==================== TEST 8: MODAL UI INTEGRATION & TAB NAVIGATION ====================
            Debug.Log("[Test 8] Verifying MedalRibbonRackModal runtime hierarchy and tab navigation...");
            var modalObj = new GameObject("TestMedalRibbonRackModal");
            var modal = modalObj.AddComponent<MedalRibbonRackModal>();

            try
            {
                modal.OpenModal(soldier);
                AssertCondition(modal.modalRoot != null && modal.modalRoot.activeSelf, "Modal root must be active upon OpenModal.");

                // Test Tab 0: Ribbon Rack
                modal.SwitchTab(0);
                AssertCondition(modal.tabRackPanel != null && modal.tabRackPanel.activeSelf, "Tab 1 (Ribbon Rack) must be active.");
                AssertCondition(modal.tabCitationsPanel != null && !modal.tabCitationsPanel.activeSelf, "Tab 2 must be inactive.");
                AssertCondition(modal.tabMemorialPanel != null && !modal.tabMemorialPanel.activeSelf, "Tab 3 must be inactive.");

                // Test Selecting a medal
                var silverDef = MedalRegistry.GetDefinition("Silver Cross");
                modal.SelectMedal(silverDef);
                AssertCondition(modal.selectedRibbonTitleText != null && modal.selectedRibbonTitleText.text.Contains("SILVER CROSS"),
                    "Selected medal title text must update.");

                // Test Tab 1: Citations Dossier
                modal.SwitchTab(1);
                AssertCondition(modal.tabCitationsPanel != null && modal.tabCitationsPanel.activeSelf, "Tab 2 (Citations) must be active.");
                AssertCondition(modal.tabRackPanel != null && !modal.tabRackPanel.activeSelf, "Tab 1 must be inactive.");

                // Test Tab 2: Memorial Wall
                modal.SwitchTab(2);
                AssertCondition(modal.tabMemorialPanel != null && modal.tabMemorialPanel.activeSelf, "Tab 3 (Memorial) must be active.");

                // Test CloseModal
                modal.CloseModal();
                AssertCondition(!modal.modalRoot.activeSelf, "Modal root must be inactive after CloseModal.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(modalObj);
            }

            Debug.Log("<color=#00FF66>[Test 8 PASSED]</color> Modal UI hierarchy and multi-tab navigation verified.");

            Debug.Log("<color=#00FFAA>========================================================================</color>");
            Debug.Log("<color=#00FFAA>==== ALL MEDAL RACK & MEMORIAL WALL TESTS (AGENT 2) PASSED 100%! ===</color>");
            Debug.Log("<color=#00FFAA>========================================================================</color>");
        }

        /// <summary>
        /// Command line batchmode runner for automated CI execution.
        /// </summary>
        public static void RunTestBatch()
        {
            try
            {
                RunAllTests();
                Debug.Log("[BATCH COMPLETE] All Medal Ribbon Rack & Memorial Wall tests passed with zero failures.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BATCH FAILED] Medal Rack test suite failed with exception: {ex.Message}\n{ex.StackTrace}");
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
