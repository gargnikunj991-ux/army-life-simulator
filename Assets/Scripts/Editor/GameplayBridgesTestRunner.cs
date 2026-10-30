#if UNITY_EDITOR
using System;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using UnityEditor;
using UnityEngine;

namespace ArmyLife.Editor
{
    public static class GameplayBridgesTestRunner
    {
        [MenuItem("Army Life/Run Gameplay Bridges & Age Limits Test", false, 30)]
        public static void RunTests()
        {
            Debug.Log("<color=#00D4FF>========================================================================</color>");
            Debug.Log("<color=#00D4FF>====== STARTING GAMEPLAY BRIDGES & AGE RESTRICTION TEST SUITE ======</color>");
            Debug.Log("<color=#00D4FF>========================================================================</color>");

            var gm = GameObject.FindAnyObjectByType<GameManager>();
            bool createdGm = false;
            if (gm == null)
            {
                var gmObj = new GameObject("GameManager_BridgeTest");
                gm = gmObj.AddComponent<GameManager>();
                createdGm = true;
            }

            try
            {
                // TEST 1: WAR ACADEMY AGE RESTRICTION (MAX AGE 35)
                Debug.Log("[Test 1] Testing War Academy age restriction (Max Age: 35)...");
                var oldCadet = new SoldierProfile("Major Oldman", null, 120);
                oldCadet.age = 36.0f;
                oldCadet.intellect = 75f;
                oldCadet.fitness = 75f;
                oldCadet.cashBalance = 150000L;
                gm.currentSoldier = oldCadet;

                gm.ApplyForOfficerAcademy(takeLoan: false);
                AssertCondition(oldCadet.careerTrack != CareerTrack.MilitaryWarAcademy,
                    "Soldier aged 36 should have been REJECTED from the War Academy due to Max Age 35 rule!");

                var youngCadet = new SoldierProfile("Young Aspirant", null, 120);
                youngCadet.age = 22.0f;
                youngCadet.intellect = 75f;
                youngCadet.fitness = 75f;
                youngCadet.meritRating = 80f;
                youngCadet.cashBalance = 150000L;
                gm.currentSoldier = youngCadet;

                gm.ApplyForOfficerAcademy(takeLoan: false);
                AssertCondition(youngCadet.careerTrack == CareerTrack.MilitaryWarAcademy,
                    "Soldier aged 22 with qualifying stats and tuition should have been ADMITTED to the War Academy!");
                Debug.Log("<color=#00FF66>✓ Test 1 Passed: War Academy strictly enforces candidate admission <= 35 years old.</color>");

                // TEST 2: SPECIAL FORCES SELECTION AGE RESTRICTION (MAX AGE 30)
                Debug.Log("[Test 2] Testing Special Forces selection age restriction (Max Age: 30)...");
                var veteranSoldier = new SoldierProfile("Veteran Sergeant", null, 130);
                veteranSoldier.age = 31.0f;
                veteranSoldier.coFavor = 85f;
                veteranSoldier.meritRating = 80f;
                veteranSoldier.fitness = 85f;
                veteranSoldier.combatSkill = 70f;
                gm.currentSoldier = veteranSoldier;

                gm.RequestCOSpecialForcesRecommendation();
                AssertCondition(!veteranSoldier.isSpecialForcesCandidate,
                    "Soldier aged 31 must be REJECTED from Special Forces selection due to Max Age 30 rule!");

                var primeSoldier = new SoldierProfile("Prime Commando", null, 130);
                primeSoldier.age = 25.0f;
                primeSoldier.coFavor = 85f;
                primeSoldier.meritRating = 80f;
                primeSoldier.fitness = 85f;
                primeSoldier.combatSkill = 70f;
                gm.currentSoldier = primeSoldier;

                gm.RequestCOSpecialForcesRecommendation();
                AssertCondition(primeSoldier.isSpecialForcesCandidate,
                    "Soldier aged 25 with qualifying standing must be APPROVED for the 1-year Special Forces Q-Course!");
                Debug.Log("<color=#00FF66>✓ Test 2 Passed: Special Forces Selection strictly enforces candidate age <= 30 years old.</color>");

                // TEST 3: NATURAL DEPLOYMENT ROTATION
                Debug.Log("[Test 3] Testing Natural Military Deployment Rotation (Frontline Combat <-> Garrison Refit)...");
                var activeSoldier = new SoldierProfile("Ethan Vance", null, 110);
                activeSoldier.age = 19.0f;
                activeSoldier.careerTrack = CareerTrack.MilitaryEnlisted;
                activeSoldier.isInCombatDeployment = true;
                activeSoldier.isVoluntaryExtendedTour = false;
                activeSoldier.currentTheater = WarTheaterType.ArdenRidgeTrenchLines;
                activeSoldier.InitializeSquadMatesIfEmpty();
                gm.currentSoldier = activeSoldier;

                // Simulate mission resolution
                var theater = WarTheaterInfo.GetInfo(activeSoldier.currentTheater);
                var mission = CombatOperationsEngine.GenerateMission(theater, activeSoldier);
                gm.ResolveCombatOperation(mission, TacticalChoiceOrder.AggressiveBreach, theater);

                AssertCondition(!activeSoldier.isInCombatDeployment,
                    "Upon concluding standard combat tour, soldier must rotate to Garrison Refit (isInCombatDeployment = false)!");
                AssertCondition(activeSoldier.consecutiveCombatSemesters == 0,
                    "Consecutive combat semesters must reset upon rotating back to garrison.");

                // Now advance turn in Garrison: must refit and mobilize for next tour!
                gm.AdvanceTurn();
                AssertCondition(activeSoldier.isInCombatDeployment,
                    "After completing garrison refit, High Command must issue mobilization orders deploying the unit back to the frontline!");
                Debug.Log("<color=#00FF66>✓ Test 3 Passed: Authentic military deployment cycle (Combat <-> Garrison) rotates naturally.</color>");

                // TEST 4: COMMAND AUTHORITY SCALING
                Debug.Log("[Test 4] Testing Command Authority scaling (E-1 Grunt vs E-5 NCO / O-1 Officer)...");
                var recruit = new SoldierProfile("Recruit Private", null, 80);
                recruit.currentRank = new MilitaryRankSO { rankName = "Private", paygrade = "E-2", rankOrder = 2, type = PaygradeType.Enlisted };
                AssertCondition(!SquadmateCommandEngine.IsNCOOrOfficer(recruit),
                    "E-2 Private must NOT hold tactical platoon/squad command authority.");

                var sergeant = new SoldierProfile("Sgt Miller", null, 110);
                sergeant.currentRank = new MilitaryRankSO { rankName = "Sergeant", paygrade = "E-5", rankOrder = 5, type = PaygradeType.Enlisted };
                AssertCondition(SquadmateCommandEngine.IsNCOOrOfficer(sergeant),
                    "E-5 Sergeant MUST hold tactical squad command authority.");

                var lieutenant = new SoldierProfile("Lt Cross", null, 120);
                lieutenant.currentRank = new MilitaryRankSO { rankName = "2nd Lieutenant", paygrade = "O-1", rankOrder = 1, type = PaygradeType.Officer };
                AssertCondition(SquadmateCommandEngine.IsNCOOrOfficer(lieutenant),
                    "O-1 Lieutenant MUST hold commissioned tactical command authority.");
                Debug.Log("<color=#00FF66>✓ Test 4 Passed: Command authority accurately gates tactical doctrines by rank.</color>");

                // TEST 5: SQUADMATE CAMARADERIE INTERACTIONS
                Debug.Log("[Test 5] Testing Squadmate fireteam interactions (PX Cigarettes & Field Trauma)...");
                var squadLeader = new SoldierProfile("Leader Logan", null, 100);
                squadLeader.InitializeSquadMatesIfEmpty();
                squadLeader.cigarettesCount = 5;
                squadLeader.fieldMedkitsCount = 3;

                var medic = squadLeader.squadMates.Find(m => m.role == SquadRole.CombatMedic);
                float prevDocRel = medic.relationship;
                SquadmateCommandEngine.ShareCigarettesWithSquad(squadLeader, medic);
                AssertCondition(medic.relationship > prevDocRel, "Sharing cigarettes must increase squadmate loyalty/relationship.");
                AssertCondition(squadLeader.cigarettesCount == 4, "Sharing cigarettes must consume 1 cigarette pack.");

                medic.isDead = true;
                medic.deathReason = "Shrapnel";
                SquadmateCommandEngine.BandageSquadmate(squadLeader, medic);
                AssertCondition(!medic.isDead, "Applying field trauma medkit must revive dying comrade.");
                AssertCondition(squadLeader.fieldMedkitsCount == 2, "Bandaging squadmate must consume 1 medkit.");
                AssertCondition(squadLeader.HasTrait("Life Saver"), "Heroic save must award 'Life Saver' story trait.");
                Debug.Log("<color=#00FF66>✓ Test 5 Passed: Fireteam interactions function seamlessly.</color>");

                // TEST 6: WARTIME MARKET ENGINE DYNAMIC PX PRICING
                Debug.Log("[Test 6] Testing WartimeMarketEngine dynamic PX pricing under macroeconomic shocks...");
                WartimeMarketEngine.ResetToNormal();
                long normalMedkit = WartimeMarketEngine.GetAdjustedPrice(150L, GearCategory.Medical);
                long normalArmor = WartimeMarketEngine.GetAdjustedPrice(500L, GearCategory.Armor);
                AssertCondition(normalMedkit == 150L, "Normal medkit price should be $150.");
                AssertCondition(normalArmor == 500L, "Normal armor price should be $500.");

                WartimeMarketEngine.ApplyShock(MacroEconomicShock.StrategicBlockadeSupplyCrunch, 2);
                long blockadeMedkit = WartimeMarketEngine.GetAdjustedPrice(150L, GearCategory.Medical);
                long blockadeArmor = WartimeMarketEngine.GetAdjustedPrice(500L, GearCategory.Armor);
                AssertCondition(blockadeMedkit == 300L, $"Blockade medkit price should be $300 (+100%), got {blockadeMedkit}.");
                AssertCondition(blockadeArmor == 875L, $"Blockade armor price should be $875 (+75%), got {blockadeArmor}.");
                WartimeMarketEngine.ResetToNormal();
                Debug.Log("<color=#00FF66>✓ Test 6 Passed: Wartime market dynamic pricing accurately surges during blockades.</color>");

                // TEST 7: ECONOMIC DILEMMA DECK INTEGRATION
                Debug.Log("[Test 7] Testing EconomicDilemmaDeck integration into adult life...");
                var testSoldier = new SoldierProfile("Dilemma Test", null, 100);
                testSoldier.age = 22f;
                testSoldier.cashBalance = 1000L;
                EconomicDilemmaDeck.AutoResolveInHeadlessMode = true;
                bool dilemmaTriggered = EconomicDilemmaDeck.TryTriggerEconomicDilemma(testSoldier);
                AssertCondition(dilemmaTriggered, "EconomicDilemmaDeck must successfully select and trigger an eligible dilemma.");
                Debug.Log("<color=#00FF66>✓ Test 7 Passed: 15 high-stakes economic & military graft dilemmas trigger seamlessly.</color>");

                // TEST 8: MAIL CALL DISPATCH & QUEUE
                Debug.Log("[Test 8] Testing MailCallEngine dispatch and letter queue...");
                MailCallEngine.ResetEngine();
                var letters = MailCallEngine.DispatchSemesterMailCall(testSoldier, 2026, 1, false);
                AssertCondition(letters != null && letters.Count > 0, "MailCallEngine must dispatch at least one letter per semester.");
                AssertCondition(MailCallEngine.CurrentMailQueue.Count == letters.Count, "CurrentMailQueue must reflect dispatched letters.");
                Debug.Log("<color=#00FF66>✓ Test 8 Passed: Field post courier delivers letters and care packages.</color>");

                // TEST 9: SQUADMATE REPLACEMENT DEPOT
                Debug.Log("[Test 9] Testing KIA squadmate replacement depot...");
                var fireteamLead = new SoldierProfile("Fireteam Cap", null, 100);
                fireteamLead.InitializeSquadMatesIfEmpty();
                var gunner = fireteamLead.squadMates.Find(m => m.role == SquadRole.HeavyGunner);
                gunner.isDead = true;
                gunner.deathReason = "Mortar Barrage";

                var replacement = SquadmateCommandEngine.AddReplacement(fireteamLead, SquadRole.HeavyGunner);
                AssertCondition(replacement != null, "Replacement recruit must be instantiated.");
                AssertCondition(!replacement.isDead, "Replacement recruit must be alive.");
                AssertCondition(fireteamLead.squadMates.FindAll(m => m.role == SquadRole.HeavyGunner).Count == 1, 
                    "Only 1 squadmate should occupy the HeavyGunner role slot after replacement.");
                Debug.Log("<color=#00FF66>✓ Test 9 Passed: Replacement depot replenishes KIA fireteam vacancies.</color>");

                // TEST 10: ECONOMY ENGINE MACRO SHOCK INTEGRATION
                Debug.Log("[Test 10] Testing EconomyEngine dynamic dividend adjustment under War Boom...");
                var investor = new SoldierProfile("Bond Holder", null, 100);
                investor.warBondsOwned = 10; // Base: 10 * $30 = $300
                investor.tradeGuildShares = 5; // Base: 5 * $60 = $300
                investor.housingTier = HousingTier.BarracksOrSlums;
                WartimeMarketEngine.ResetToNormal();

                EconomyEngine.ProcessSemesterEconomy(investor);
                long normalDiv = investor.investmentDividendsLastSemester;
                AssertCondition(normalDiv == 600L, $"Normal dividends for 10 bonds + 5 shares must be $600, got {normalDiv}.");

                WartimeMarketEngine.ApplyShock(MacroEconomicShock.FrontlineVictoryBoom, 2); // War bond & guild dividends: 1.5x
                EconomyEngine.ProcessSemesterEconomy(investor);
                long boomDiv = investor.investmentDividendsLastSemester;
                AssertCondition(boomDiv == 900L, $"Victory boom dividends must yield 1.5x on war bonds ($450) + 1.5x on shares ($450) = $900, got {boomDiv}.");
                WartimeMarketEngine.ResetToNormal();
                Debug.Log("<color=#00FF66>✓ Test 10 Passed: Macroeconomic shocks dynamically modulate financial yields.</color>");

                // TEST 11: ROMANCE, DATING, PROPOSING & ARCH-OF-SABERS WEDDING
                Debug.Log("[Test 11] Testing BitLife Romance, Dating, Engagement & Wedding loop...");
                var romantic = new SoldierProfile("Sergeant Lover", null, 100);
                romantic.cashBalance = 5000L;
                var partner = RomanceAndCourtshipEngine.GenerateProspectivePartner(romantic);
                AssertCondition(partner != null && !string.IsNullOrEmpty(partner.name), "Prospective partner must be generated with valid name.");
                
                // Dating
                float preDateAffection = partner.affection;
                bool dateSuccess = RomanceAndCourtshipEngine.TakeOnDate(romantic, partner, "MilitaryBall");
                AssertCondition(dateSuccess && partner.affection > preDateAffection, "Gala date must increase partner affection.");
                
                // Ring Purchase & Proposal
                partner.affection = 80f;
                bool proposalSuccess = RomanceAndCourtshipEngine.ProposeMarriage(romantic, partner, "Imperial Diamond Solitaire", 2000L);
                AssertCondition(proposalSuccess && partner.stage == CourtshipStage.Engaged, "Diamond ring proposal must be accepted into Engagement.");
                
                // Arch-of-Sabers Wedding Ceremony
                bool weddingSuccess = RomanceAndCourtshipEngine.HoldWeddingCeremony(romantic, partner, "CathedralMilitary");
                AssertCondition(weddingSuccess && romantic.isMarried && romantic.spouseName == partner.name, "Arch-of-Sabers wedding must register spouse and isMarried.");
                AssertCondition(romantic.clanMembers.Exists(m => m.relationType == ClanRelationType.Spouse), "Spouse must be present in clan members roster.");
                Debug.Log("<color=#00FF66>✓ Test 11 Passed: Romance, dating, ring proposal, and military wedding verified.</color>");

                // TEST 12: MILITARY OCCUPATIONAL SPECIALTY (MOS) COMBAT PERKS
                Debug.Log("[Test 12] Testing MOS specializations and battlefield tactical perks...");
                var medicSoldier = new SoldierProfile("Doc Taylor", null, 100);
                medicSoldier.militaryMOS = MilitaryMOS.CombatMedic68W;
                var testTheater = WarTheaterInfo.GetInfo(WarTheaterType.ArdenRidgeTrenchLines);
                var opOutcome = CombatOperationsEngine.ProcessSemesterCombatTour(medicSoldier, testTheater, TacticalMissionType.TrenchBreachAssault, TacticalChoiceOrder.HeavySuppression);
                AssertCondition(opOutcome != null, "Combat operation with specialized MOS must successfully resolve.");
                AssertCondition(opOutcome.roleSynergySummary.Contains("68W Combat Medic"), "Combat outcome must acknowledge 68W Combat Medic operational perks.");
                Debug.Log("<color=#00FF66>✓ Test 12 Passed: MOS specializations inject unique operational battlefield advantages.</color>");

                // TEST 13: THE DEAR JOHN LETTER & IRON HEART TRAIT
                Debug.Log("[Test 13] Testing Dear John breakup letter and player response choices...");
                var deployedSoldier = new SoldierProfile("Heartbroken Soldier", null, 100);
                deployedSoldier.isMarried = true;
                deployedSoldier.spouseName = "Faithless Spouse";
                RomanceAndCourtshipEngine.ResolveDearJohnChoice(deployedSoldier, DearJohnChoice.TearItUpMoveForward);
                AssertCondition(!deployedSoldier.isMarried, "Dear John breakup must divorce/separate soldier.");
                AssertCondition(deployedSoldier.HasTrait("Iron Heart"), "Tearing up Dear John letter must award 'Iron Heart' resilience trait.");
                Debug.Log("<color=#00FF66>✓ Test 13 Passed: Dear John letter resolution and emotional hardening verified.</color>");

                // TEST 14: PUBLISHED WAR MEMOIR ROYALTIES & CIVIC OFFICE
                Debug.Log("[Test 14] Testing War Memoir publishing and semester royalty payouts...");
                var veteranAuthor = new SoldierProfile("Veteran Author", null, 100);
                veteranAuthor.totalSemestersLived = 20; // 10 years served
                veteranAuthor.AddMedal("Imperial Wound Star (Purple Heart)");
                veteranAuthor.publishedMemoirsCount = 1;
                veteranAuthor.publishedMemoirTitle = "Blood in the Arden";
                veteranAuthor.warMemoirRoyaltiesPerSemester = 1500L;
                veteranAuthor.cashBalance = 2000L;
                
                EconomyEngine.ProcessSemesterEconomy(veteranAuthor);
                AssertCondition(veteranAuthor.cashBalance >= 3500L, "Semester economy must disburse war memoir royalties.");
                Debug.Log("<color=#00FF66>✓ Test 14 Passed: War memoir royalties and veteran legacy financial yield verified.</color>");

                Debug.Log("<color=#00FF66>========================================================================</color>");
                Debug.Log("<color=#00FF66>====== ALL GAMEPLAY BRIDGES & AGE RESTRICTION TESTS PASSED (100%) ======</color>");
                Debug.Log("<color=#00FF66>========================================================================</color>");
            }
            finally
            {
                if (createdGm && gm != null)
                {
                    UnityEngine.Object.DestroyImmediate(gm.gameObject);
                }
            }
        }

        private static void AssertCondition(bool condition, string failureMessage)
        {
            if (!condition)
            {
                Debug.LogError($"[TEST FAILURE] {failureMessage}");
                throw new Exception($"Assertion Failed: {failureMessage}");
            }
        }
    }
}
#endif
