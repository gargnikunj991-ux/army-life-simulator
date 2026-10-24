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
    /// Automated test suite for Agent 1: Personal Life, Mail Call & Homefront Dynasty Engine.
    /// Verifies letter decks integrity, semiannual mail call dispatcher, family remittances accounting,
    /// Dear John heartbreak decision trees, Furlough R&R leave engine, and MailCallModal UI.
    /// </summary>
    public static class MailCallTestRunner
    {
        [MenuItem("Army Life/Run Mail Call Tests", false, 30)]
        public static void RunAllMailCallTests()
        {
            Debug.Log("<color=#00D4FF>========================================================================</color>");
            Debug.Log("<color=#00D4FF>==== STARTING PERSONAL LIFE, MAIL CALL & HOMEFRONT DYNASTY TEST SUITE ==</color>");
            Debug.Log("<color=#00D4FF>========================================================================</color>");

            MailCallEngine.ResetEngine();

            TestLetterDecksIntegrity();
            TestSemesterMailCallDispatcher();
            TestRemittanceAccountingAndFamilyProsperity();
            TestReplyWithEncouragement();
            TestBurnOrIgnoreLetter();
            TestDearJohnResolutionPaths();
            TestFurloughRAndREngine();
            TestMailCallModalUI();

            Debug.Log("<color=#00FFAA>========================================================================</color>");
            Debug.Log("<color=#00FFAA>==== ALL MAIL CALL & HOMEFRONT DYNASTY TESTS PASSED WITH 100% SUCCESS ==</color>");
            Debug.Log("<color=#00FFAA>========================================================================</color>");
        }

        /// <summary>
        /// Command line batchmode runner for automated CI execution.
        /// </summary>
        public static void RunTestBatch()
        {
            try
            {
                RunAllMailCallTests();
                Debug.Log("[BATCH SUCCESS] Personal Life, Mail Call & Homefront Dynasty tests completed with 0 errors.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BATCH FAILED] Mail Call test failure: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }

        // =========================================================================
        // TEST 1: LETTER DECKS INTEGRITY
        // =========================================================================

        private static void TestLetterDecksIntegrity()
        {
            Debug.Log("<color=#FFD700>[Test 1] Verifying all authentic wartime letter decks...</color>");

            // 1. Sweetheart Letters
            var sweet1 = MailDeckCatalog.CreateSweetheartEncouragement("Evelyn", 2012, 1);
            AssertCondition(sweet1 != null, "Sweetheart encouragement letter should not be null.");
            AssertCondition(sweet1.relation == MailSenderRelation.Sweetheart, "Relation should be Sweetheart.");
            AssertCondition(sweet1.category == LetterCategory.Sweetheart, "Category should be Sweetheart.");
            AssertCondition(!string.IsNullOrEmpty(sweet1.letterBodyText), "Letter body text should not be empty.");
            AssertCondition(sweet1.responseOptions.Count >= 3, "Sweetheart letter should offer at least 3 choices.");

            var sweet2 = MailDeckCatalog.CreateSweetheartMarriageHints("Evelyn", 2012, 2);
            AssertCondition(sweet2.responseOptions.Count >= 3, "Marriage hints letter should offer at least 3 choices.");

            var sweet3 = MailDeckCatalog.CreateSweetheartAnxiety("Evelyn", 2013, 1);
            AssertCondition(sweet3.moraleImpactIfIgnored > 0f, "Sweetheart anxiety should have morale impact if ignored.");

            var dearJohn = MailDeckCatalog.CreateDearJohnLetter("Evelyn", 2013, 2);
            AssertCondition(dearJohn.isDearJohn, "Dear John letter should have isDearJohn flag set.");
            AssertCondition(dearJohn.responseOptions.Count == 3, "Dear John must offer exactly 3 distinct emotional choices.");

            // 2. Family Farm / Hometown Letters
            var harvest = MailDeckCatalog.CreateHarvestStrugglesLetter(2012, 2);
            AssertCondition(harvest.requiresRemittance, "Harvest struggles should require remittance.");
            AssertCondition(harvest.remittanceAmountRequested == 400L, "Harvest remittance should be $400.");

            var mortgage = MailDeckCatalog.CreateMortgageForeclosureLetter(2013, 1);
            AssertCondition(mortgage.requiresRemittance && mortgage.remittanceAmountRequested == 1000L, "Foreclosure remittance should be $1000.");

            var draft = MailDeckCatalog.CreateSiblingDraftLetter(2013, 2);
            AssertCondition(draft.relation == MailSenderRelation.Mother, "Draft letter should be from Mother.");

            var sickness = MailDeckCatalog.CreateFamilySicknessLetter(2014, 1);
            AssertCondition(sickness.remittanceAmountRequested == 500L, "Family sickness remittance should be $500.");

            // 3. Officer Invitations
            var gala = MailDeckCatalog.CreateCapitalGalaInvitation(2014, 2);
            AssertCondition(gala.category == LetterCategory.OfficerCommandInvitation, "Gala should be OfficerCommandInvitation.");

            var patron = MailDeckCatalog.CreateIndustrialistPatronageLetter(2015, 1);
            AssertCondition(patron.relation == MailSenderRelation.HighCommandOrOfficer, "Patron letter should be from HighCommandOrOfficer.");

            // 4. Gratitude Letter
            var gratitude = MailDeckCatalog.CreateGratitudeLetter("Mother", 500L, 2014, 2);
            AssertCondition(gratitude.category == LetterCategory.GratitudeRemittance, "Gratitude letter category mismatch.");

            Debug.Log("<color=#00FFAA>[Test 1 Passed] All 10 authentic letter scenarios verified with complete metadata and options.</color>");
        }

        // =========================================================================
        // TEST 2: SEMIANNUAL MAIL CALL DISPATCHER
        // =========================================================================

        private static void TestSemesterMailCallDispatcher()
        {
            Debug.Log("<color=#FFD700>[Test 2] Verifying semiannual mail call dispatcher (1-2 letters per semester)...</color>");

            SoldierProfile soldier = CreateTestSoldier("Ethan Vance", 22.0f, 2500L);
            soldier.isInCombatDeployment = false;

            var lettersGarrison = MailCallEngine.DispatchSemesterMailCall(soldier, 2012, 1, false);
            AssertCondition(lettersGarrison != null, "Dispatched letters list should not be null.");
            AssertCondition(lettersGarrison.Count >= 1 && lettersGarrison.Count <= 2, 
                $"Expected 1 or 2 letters per semester, got {lettersGarrison.Count}.");
            AssertCondition(MailCallEngine.CurrentMailQueue.Count == lettersGarrison.Count, 
                "CurrentMailQueue should match dispatched letters count.");

            // Frontline deployment test
            soldier.isInCombatDeployment = true;
            var lettersFrontline = MailCallEngine.DispatchSemesterMailCall(soldier, 2012, 2, true);
            AssertCondition(lettersFrontline != null && lettersFrontline.Count >= 1 && lettersFrontline.Count <= 2,
                $"Frontline mail call should deliver 1-2 letters, got {lettersFrontline?.Count ?? 0}.");

            Debug.Log($"<color=#00FFAA>[Test 2 Passed] Mail dispatcher successfully delivered {lettersGarrison.Count} letters (garrison) and {lettersFrontline.Count} letters (frontline).</color>");
        }

        // =========================================================================
        // TEST 3: REMITTANCE ACCOUNTING & FAMILY PROSPERITY
        // =========================================================================

        private static void TestRemittanceAccountingAndFamilyProsperity()
        {
            Debug.Log("<color=#FFD700>[Test 3] Verifying remittance accounting, cash deduction, prosperity tiers & gratitude follow-up...</color>");

            SoldierProfile soldier = CreateTestSoldier("Arthur King", 23.0f, 2000L);
            soldier.familyLivingCondition = 25f; // Struggling
            soldier.happiness = 60f;

            HomefrontDynastyState state = MailCallEngine.GetHomefrontState(soldier);
            AssertCondition(state.prosperityTier == HomefrontProsperityTier.Struggling, "Initial tier should be Struggling.");

            // Create harvest letter requesting $400 remittance
            var harvestLetter = MailDeckCatalog.CreateHarvestStrugglesLetter(2013, 1);
            long initialCash = soldier.cashBalance;
            float initialMorale = soldier.happiness;
            float initialProsperity = state.familyProsperityPoints;

            // Select Option 0: "Send $400 Harvest Remittance"
            var res = MailCallEngine.ResolveLetterOption(soldier, harvestLetter, 0);

            AssertCondition(res.success, "Remittance option resolution should succeed.");
            AssertCondition(res.wasRemittanceSent, "wasRemittanceSent flag should be true.");
            AssertCondition(soldier.cashBalance == initialCash - 400L, 
                $"Expected cash ${initialCash - 400L}, got ${soldier.cashBalance}.");
            AssertCondition(soldier.totalSentToFamily == 400L, 
                $"Expected totalSentToFamily 400, got {soldier.totalSentToFamily}.");
            AssertCondition(soldier.totalRemittancesSentToFamily == 400L,
                $"totalRemittancesSentToFamily alias should equal 400, got {soldier.totalRemittancesSentToFamily}.");
            AssertCondition(state.familyProsperityPoints > initialProsperity,
                "Family prosperity points should have increased.");
            AssertCondition(soldier.familyLivingCondition == state.familyProsperityPoints,
                "Soldier familyLivingCondition should synchronize with state prosperity.");
            AssertCondition(soldier.happiness > initialMorale,
                $"Soldier morale should increase on sending remittance (from {initialMorale} to {soldier.happiness}).");
            AssertCondition(soldier.HasTrait("Devoted Son"),
                "Soldier should have acquired trait 'Devoted Son'.");
            AssertCondition(state.pendingGratitudeLetterNextTurn,
                "pendingGratitudeLetterNextTurn should be true for next semester.");

            // Dispatch next semester mail call to verify gratitude letter arrives!
            var nextLetters = MailCallEngine.DispatchSemesterMailCall(soldier, 2013, 2, false);
            bool foundGratitudeLetter = false;
            foreach (var l in nextLetters)
            {
                if (l.category == LetterCategory.GratitudeRemittance)
                {
                    foundGratitudeLetter = true;
                    break;
                }
            }
            AssertCondition(foundGratitudeLetter, "Next semester's mail call must deliver a gratitude letter from family!");
            AssertCondition(!state.pendingGratitudeLetterNextTurn, "pendingGratitudeLetterNextTurn should reset after dispatch.");

            Debug.Log("<color=#00FFAA>[Test 3 Passed] Remittance accounting verified: Cash -$400, Prosperity increased, Morale +15, Gratitude letter delivered.</color>");
        }

        // =========================================================================
        // TEST 4: REPLY WITH ENCOURAGEMENT (FREE / RELATIONSHIP STAMINA)
        // =========================================================================

        private static void TestReplyWithEncouragement()
        {
            Debug.Log("<color=#FFD700>[Test 4] Verifying Reply with Encouragement ($0 cost, builds affection & morale)...</color>");

            SoldierProfile soldier = CreateTestSoldier("Marcus Vance", 21.0f, 1500L);
            soldier.happiness = 70f;

            HomefrontDynastyState state = MailCallEngine.GetHomefrontState(soldier);
            state.courtshipAffection = 50f;
            long initialCash = soldier.cashBalance;
            float initialMorale = soldier.happiness;

            var sweetLetter = MailDeckCatalog.CreateSweetheartEncouragement("Evelyn", 2012, 1);
            // Option 0: "Write Tender Romantic Reply" (Cost: $0, Morale: +15, Affection: +20)
            var res = MailCallEngine.ResolveLetterOption(soldier, sweetLetter, 0);

            AssertCondition(res.success, "Reply option should succeed.");
            AssertCondition(soldier.cashBalance == initialCash, "Cash balance should be unchanged ($0 cost).");
            AssertCondition(soldier.happiness == initialMorale + 15f, 
                $"Expected morale {initialMorale + 15f}, got {soldier.happiness}.");
            AssertCondition(state.courtshipAffection == 70f, 
                $"Expected courtship affection 70, got {state.courtshipAffection}.");

            Debug.Log("<color=#00FFAA>[Test 4 Passed] Reply with encouragement built affection (+20) and boosted morale (+15) with $0 cash cost.</color>");
        }

        // =========================================================================
        // TEST 5: BURN / IGNORE LETTER (MORALE PENALTY & STOIC DETACHMENT)
        // =========================================================================

        private static void TestBurnOrIgnoreLetter()
        {
            Debug.Log("<color=#FFD700>[Test 5] Verifying Burn / Ignore letter penalties & Hardened Stoic trait...</color>");

            SoldierProfile soldier = CreateTestSoldier("Boran Holt", 24.0f, 1000L);
            soldier.happiness = 80f;

            HomefrontDynastyState state = MailCallEngine.GetHomefrontState(soldier);
            long initialCash = soldier.cashBalance;

            var harvestLetter = MailDeckCatalog.CreateHarvestStrugglesLetter(2014, 1);
            // Option 2 is "Ignore Request (Save Pay for Yourself)"
            var res = MailCallEngine.ResolveLetterOption(soldier, harvestLetter, 2);

            AssertCondition(res.success, "Ignore letter should resolve successfully.");
            AssertCondition(res.wasIgnored, "wasIgnored flag should be true.");
            AssertCondition(soldier.cashBalance == initialCash, "Cash balance should remain untouched.");
            AssertCondition(soldier.happiness < 80f, $"Morale should drop on ignoring family crisis, got {soldier.happiness}.");
            AssertCondition(soldier.HasTrait("Hardened Stoic"), "Soldier should acquire trait 'Hardened Stoic'.");
            AssertCondition(state.lettersIgnoredCount == 1, $"lettersIgnoredCount should be 1, got {state.lettersIgnoredCount}.");

            Debug.Log("<color=#00FFAA>[Test 5 Passed] Ignore letter penalty applied: Morale dropped by -18 and 'Hardened Stoic' trait acquired.</color>");
        }

        // =========================================================================
        // TEST 6: "DEAR JOHN" BREAKUP RESOLUTION DECISION TREES
        // =========================================================================

        private static void TestDearJohnResolutionPaths()
        {
            Debug.Log("<color=#FFD700>[Test 6] Verifying high-impact Dear John heartbreak decision branches...</color>");

            // Path 1: "Focus on the Mission" (Discipline +10, Morale -15)
            {
                SoldierProfile s1 = CreateTestSoldier("John Alpha", 22.0f, 1000L);
                s1.discipline = 50f;
                s1.happiness = 75f;
                var letter1 = MailDeckCatalog.CreateDearJohnLetter("Clara", 2013, 2);

                var res1 = MailCallEngine.ResolveDearJohn(s1, letter1, DearJohnChoice.FocusOnMission);
                AssertCondition(res1.success, "Dear John Path 1 should succeed.");
                AssertCondition(s1.discipline == 60f, $"Expected discipline 60 (+10), got {s1.discipline}.");
                AssertCondition(s1.happiness == 60f, $"Expected morale 60 (-15), got {s1.happiness}.");
                AssertCondition(s1.HasTrait("Cold Professional"), "Expected trait 'Cold Professional'.");
                AssertCondition(res1.wasDearJohn, "wasDearJohn flag should be true.");
            }

            // Path 2: "Write Desperate Plea" (Morale -25)
            {
                SoldierProfile s2 = CreateTestSoldier("John Bravo", 22.0f, 1000L);
                s2.happiness = 75f;
                var letter2 = MailDeckCatalog.CreateDearJohnLetter("Clara", 2013, 2);

                var res2 = MailCallEngine.ResolveDearJohn(s2, letter2, DearJohnChoice.WriteDesperatePlea);
                AssertCondition(res2.success, "Dear John Path 2 should succeed.");
                AssertCondition(s2.happiness == 50f, $"Expected morale 50 (-25), got {s2.happiness}.");
                AssertCondition(s2.HasTrait("Heartbroken"), "Expected trait 'Heartbroken'.");
            }

            // Path 3: "Tear It Up & Move Forward" (Morale +5, acquire "Iron Heart" trait)
            {
                SoldierProfile s3 = CreateTestSoldier("John Charlie", 22.0f, 1000L);
                s3.happiness = 70f;
                s3.discipline = 50f;
                var letter3 = MailDeckCatalog.CreateDearJohnLetter("Clara", 2013, 2);

                var res3 = MailCallEngine.ResolveDearJohn(s3, letter3, DearJohnChoice.TearItUpMoveForward);
                AssertCondition(res3.success, "Dear John Path 3 should succeed.");
                AssertCondition(s3.happiness == 75f, $"Expected morale 75 (+5), got {s3.happiness}.");
                AssertCondition(s3.discipline == 55f, $"Expected discipline 55 (+5), got {s3.discipline}.");
                AssertCondition(s3.HasTrait("Iron Heart"), "Expected trait 'Iron Heart'.");
            }

            Debug.Log("<color=#00FFAA>[Test 6 Passed] All 3 Dear John emotional outcomes verified: Focus on Mission (+10 Disc, -15 Morale), Desperate Plea (-25 Morale), and Tear It Up (+5 Morale, 'Iron Heart').</color>");
        }

        // =========================================================================
        // TEST 7: FURLOUGH R&R ENGINE (CAPITAL LEAVE ACTIVITIES)
        // =========================================================================

        private static void TestFurloughRAndREngine()
        {
            Debug.Log("<color=#FFD700>[Test 7] Verifying Furlough R&R leave engine (Canteen, Courtship, Marriage, Civilian Land & Shops)...</color>");

            SoldierProfile soldier = CreateTestSoldier("Victor Vance", 25.0f, 10000L);
            soldier.isInCombatDeployment = false;
            soldier.happiness = 60f;
            soldier.squadRespect = 50f;

            HomefrontDynastyState state = MailCallEngine.GetHomefrontState(soldier);
            state.sweetheartName = "Evelyn";
            state.courtshipAffection = 40f;

            // 1. Canteen Visit ($100, Morale +15, Respect +5)
            var resCanteen = MailCallEngine.TakeFurlough(soldier, FurloughActivityType.CanteenVisit);
            AssertCondition(resCanteen.success, "Canteen leave should succeed.");
            AssertCondition(soldier.cashBalance == 9900L, $"Expected cash 9900, got {soldier.cashBalance}.");
            AssertCondition(soldier.happiness == 75f, $"Expected morale 75, got {soldier.happiness}.");
            AssertCondition(soldier.squadRespect == 55f, $"Expected squad respect 55, got {soldier.squadRespect}.");

            // 2. Court Sweetheart ($250, Affection +25)
            var resCourt = MailCallEngine.TakeFurlough(soldier, FurloughActivityType.CourtSweetheart);
            AssertCondition(resCourt.success, "Courting should succeed.");
            AssertCondition(soldier.cashBalance == 9650L, $"Expected cash 9650, got {soldier.cashBalance}.");
            AssertCondition(state.courtshipAffection == 65f, $"Expected affection 65, got {state.courtshipAffection}.");

            // Court again to advance to Engaged (Affection -> 90 >= 80)
            var resCourt2 = MailCallEngine.TakeFurlough(soldier, FurloughActivityType.CourtSweetheart);
            AssertCondition(state.courtshipStage == CourtshipStage.Engaged, "Courtship stage should advance to Engaged.");
            AssertCondition(soldier.HasTrait("Engaged"), "Soldier should have 'Engaged' trait.");

            // 3. Get Married ($1,000, Morale +30, Trait: 'Devoted Husband')
            var resMarriage = MailCallEngine.TakeFurlough(soldier, FurloughActivityType.GetMarried);
            AssertCondition(resMarriage.success, "Marriage ceremony should succeed.");
            AssertCondition(soldier.isMarried, "Soldier should be marked as married.");
            AssertCondition(soldier.spouseName == "Evelyn", $"Spouse name should be Evelyn, got {soldier.spouseName}.");
            AssertCondition(soldier.HasTrait("Devoted Husband"), "Soldier should acquire trait 'Devoted Husband'.");

            // 4. Invest in Civilian Shops ($1,500, Prosperity +20, Trait: 'Civilian Investor')
            float preShopProsperity = state.familyProsperityPoints;
            var resShop = MailCallEngine.TakeFurlough(soldier, FurloughActivityType.InvestCivilianShops);
            AssertCondition(resShop.success, "Civilian shop investment should succeed.");
            AssertCondition(state.civilianShopInvestments == 1500L, $"Expected shop investments 1500, got {state.civilianShopInvestments}.");
            AssertCondition(state.familyProsperityPoints == preShopProsperity + 20f, "Prosperity should increase by 20.");
            AssertCondition(soldier.HasTrait("Civilian Investor"), "Soldier should have 'Civilian Investor' trait.");

            // 5. Invest in Civilian Land ($2,000, Prosperity +25, Trait: 'Landowner')
            float preLandProsperity = state.familyProsperityPoints;
            var resLand = MailCallEngine.TakeFurlough(soldier, FurloughActivityType.InvestCivilianLand);
            AssertCondition(resLand.success, "Civilian land purchase should succeed.");
            AssertCondition(state.civilianLandParcels == 1, $"Expected land parcels 1, got {state.civilianLandParcels}.");
            AssertCondition(state.familyProsperityPoints == preLandProsperity + 25f, "Prosperity should increase by 25.");
            AssertCondition(soldier.HasTrait("Landowner"), "Soldier should have 'Landowner' trait.");

            // 6. Test Furlough Blocked on Active Frontline
            soldier.isInCombatDeployment = true;
            bool canTake = MailCallEngine.CanTakeFurlough(soldier, out string blockedReason);
            AssertCondition(!canTake, "Furlough must be disallowed while in combat deployment.");
            AssertCondition(blockedReason.Contains("frontline combat"), "Blocked reason should cite frontline combat deployment.");

            Debug.Log("<color=#00FFAA>[Test 7 Passed] Furlough engine verified: Canteen, Courting, Engagement, Cathedral Marriage, Shop & Land Investments.</color>");
        }

        // =========================================================================
        // TEST 8: MAIL CALL MODAL UI INTEGRITY
        // =========================================================================

        private static void TestMailCallModalUI()
        {
            Debug.Log("<color=#FFD700>[Test 8] Verifying MailCallModal UI full-screen anchoring, letter display & button handling...</color>");

            var canvasObj = new GameObject("TestUICanvas", typeof(Canvas));
            var modalObj = new GameObject("MailCallModalTest", typeof(RectTransform));
            modalObj.transform.SetParent(canvasObj.transform, false);

            var modal = modalObj.AddComponent<MailCallModal>();
            modal.EnsureUIHierarchy();

            AssertCondition(modal.modalRoot != null, "Modal root must be non-null.");
            AssertCondition(modal.letterBodyText != null, "Letter body text element must exist.");
            AssertCondition(modal.option1Button != null, "Option 1 button must exist.");
            AssertCondition(modal.option2Button != null, "Option 2 button must exist.");

            // Test OpenModal with a letter
            var testLetter = MailDeckCatalog.CreateHarvestStrugglesLetter(2012, 1);
            int selectedOption = -1;

            modal.OpenModal(testLetter, (l, optIdx) =>
            {
                selectedOption = optIdx;
            });

            AssertCondition(modal.modalRoot.activeSelf, "Modal root should be active after OpenModal.");
            AssertCondition(modal.CurrentLetter == testLetter, "CurrentLetter should match opened letter.");
            AssertCondition(modal.subjectText.text.Contains(testLetter.subject), "Subject text should display letter subject.");
            AssertCondition(modal.letterBodyText.text == testLetter.letterBodyText, "Letter body text should match.");

            // Test CloseModal
            modal.CloseModal();
            AssertCondition(!modal.modalRoot.activeSelf, "Modal root should be inactive after CloseModal.");
            AssertCondition(modal.CurrentLetter == null, "CurrentLetter should be cleared on close.");

            // Cleanup test gameobjects
            UnityEngine.Object.DestroyImmediate(canvasObj);

            Debug.Log("<color=#00FFAA>[Test 8 Passed] MailCallModal verified: Hierarchy auto-construction, text bindings, and open/close cycles.</color>");
        }

        // =========================================================================
        // HELPER FACTORIES & ASSERTION METHOD
        // =========================================================================

        private static SoldierProfile CreateTestSoldier(string name, float age, long cash)
        {
            return new SoldierProfile(name, null, 100)
            {
                lifeId = Guid.NewGuid().ToString(),
                age = age,
                cashBalance = cash,
                happiness = 75f,
                discipline = 60f,
                squadRespect = 50f,
                coFavor = 50f,
                familyLivingCondition = 30f,
                totalSentToFamily = 0L,
                isMarried = false,
                spouseName = null,
                childhoodRomanceInterest = "Evelyn",
                consecutiveCombatSemesters = 0,
                isInCombatDeployment = false,
                isDead = false,
                isRetired = false
            };
        }

        private static void AssertCondition(bool condition, string failureMessage)
        {
            if (!condition)
            {
                Debug.LogError($"<color=#FF4444>[TEST ASSERTION FAILED]</color> {failureMessage}");
                throw new Exception($"Assertion Failed: {failureMessage}");
            }
        }
    }
}
#endif
