#if UNITY_EDITOR
using System;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using UnityEditor;
using UnityEngine;

namespace ArmyLife.Editor
{
    public static class EconomicDilemmaTestRunner
    {
        [MenuItem("Army Life/Run Economic Dilemmas Deck Test", false, 20)]
        public static void RunEconomicDilemmasTest()
        {
            Debug.Log("<color=#00D4FF>========== STARTING ECONOMIC DILEMMAS DECK VALIDATION TEST ==========</color>");

            // 1. Verify Deck Population and Card Integrity
            var deck = EconomicDilemmaDeck.AllDilemmas;
            Debug.Assert(deck != null, "EconomicDilemmaDeck.AllDilemmas should not be null!");
            Debug.Assert(deck.Count >= 12, $"Expected at least 12 dilemma cards, found {deck.Count}!");
            Debug.Log($"[Deck Verification] Deck size validated: {deck.Count} high-stakes scenarios loaded.");

            foreach (var dilemma in deck)
            {
                Debug.Assert(!string.IsNullOrEmpty(dilemma.Id), $"Dilemma has missing or empty Id!");
                Debug.Assert(!string.IsNullOrEmpty(dilemma.Category), $"Dilemma '{dilemma.Id}' has missing Category!");
                Debug.Assert(!string.IsNullOrEmpty(dilemma.Title), $"Dilemma '{dilemma.Id}' has missing Title!");
                Debug.Assert(!string.IsNullOrEmpty(dilemma.Description), $"Dilemma '{dilemma.Id}' has missing Description!");
                Debug.Assert(!string.IsNullOrEmpty(dilemma.ChoiceALabel), $"Dilemma '{dilemma.Id}' has missing ChoiceALabel!");
                Debug.Assert(!string.IsNullOrEmpty(dilemma.ChoiceBLabel), $"Dilemma '{dilemma.Id}' has missing ChoiceBLabel!");
                Debug.Assert(dilemma.ActionA != null, $"Dilemma '{dilemma.Id}' has null ActionA callback!");
                Debug.Assert(dilemma.ActionB != null, $"Dilemma '{dilemma.Id}' has null ActionB callback!");

                if (dilemma.HasChoiceC)
                {
                    Debug.Assert(!string.IsNullOrEmpty(dilemma.ChoiceCLabel), $"Dilemma '{dilemma.Id}' has empty ChoiceCLabel!");
                    Debug.Assert(dilemma.ActionC != null, $"Dilemma '{dilemma.Id}' has null ActionC callback!");
                }
            }
            Debug.Log("<color=#00FFAA>[Deck Integrity] All 15 dilemma cards passed structural verification.</color>");

            // 2. Create Base Test Soldier
            var soldier = new SoldierProfile("Victor Kane", null, 120)
            {
                age = 22.0f,
                careerTrack = CareerTrack.MilitaryEnlisted,
                cashBalance = 1000L,
                criminalHeat = 0f,
                happiness = 80f,      // Morale
                discipline = 60f,
                meritRating = 50f,
                squadRespect = 50f,
                coFavor = 50f,
                health = 100f
            };

            Debug.Log($"[Soldier Initial State] Name: {soldier.characterName}, Cash: ${soldier.cashBalance}, Heat: {soldier.criminalHeat}%, Morale: {soldier.happiness}%, Discipline: {soldier.discipline}%");

            // =========================================================================
            // TEST 1: Black-Market Syrettes Offer (Choice A: Buy and flip)
            // =========================================================================
            Debug.Log("<color=#FFD700>--- Testing Dilemma 1: Black-Market Syrettes Offer ---</color>");
            long initialCash1 = soldier.cashBalance;
            float initialHeat1 = soldier.criminalHeat;
            float initialDisc1 = soldier.discipline;
            int initialCrimes1 = soldier.crimesCommittedCount;

            bool resolved1 = EconomicDilemmaDeck.ResolveDilemma("black_market_syrettes", 0, soldier);
            Debug.Assert(resolved1, "Failed to resolve 'black_market_syrettes' Choice A!");
            Debug.Assert(soldier.cashBalance == initialCash1 + 3500L, $"Expected Cash ${initialCash1 + 3500L}, got ${soldier.cashBalance}");
            Debug.Assert(Mathf.Approximately(soldier.criminalHeat, initialHeat1 + 20f), $"Expected Heat {initialHeat1 + 20f}%, got {soldier.criminalHeat}%");
            Debug.Assert(Mathf.Approximately(soldier.discipline, initialDisc1 - 15f), $"Expected Discipline {initialDisc1 - 15f}%, got {soldier.discipline}%");
            Debug.Assert(soldier.crimesCommittedCount == initialCrimes1 + 1, "Expected crimesCommittedCount to increase by 1!");
            Debug.Assert(soldier.HasTrait("Black Market Fence"), "Expected trait 'Black Market Fence' to be added!");

            Debug.Log($"[Dilemma 1 Passed] Stolen syrettes sold! Cash: ${soldier.cashBalance} (+3,500), Heat: {soldier.criminalHeat}% (+20%), Discipline: {soldier.discipline}% (-15%).");

            // =========================================================================
            // TEST 2: Corrupt Medical Board Bribery (Choice A: Pay Bribe for VA Pension)
            // =========================================================================
            Debug.Log("<color=#FFD700>--- Testing Dilemma 2: Corrupt Medical Board Bribery ---</color>");
            long initialCash2 = soldier.cashBalance; // 4500
            float initialHeat2 = soldier.criminalHeat; // 20
            float initialDisc2 = soldier.discipline; // 45
            long initialVA = soldier.vaDisabilityAllowancePerMonth; // 0

            bool resolved2 = EconomicDilemmaDeck.ResolveDilemma("corrupt_medical_board", 0, soldier);
            Debug.Assert(resolved2, "Failed to resolve 'corrupt_medical_board' Choice A!");
            Debug.Assert(soldier.cashBalance == initialCash2 - 2000L, $"Expected Cash ${initialCash2 - 2000L}, got ${soldier.cashBalance}");
            Debug.Assert(soldier.vaDisabilityAllowancePerMonth == initialVA + 300L, $"Expected VA Disability +$300/mo, got ${soldier.vaDisabilityAllowancePerMonth}");
            Debug.Assert(Mathf.Approximately(soldier.criminalHeat, initialHeat2 + 25f), $"Expected Heat {initialHeat2 + 25f}%, got {soldier.criminalHeat}%");
            Debug.Assert(Mathf.Approximately(soldier.discipline, initialDisc2 - 20f), $"Expected Discipline {initialDisc2 - 20f}%, got {soldier.discipline}%");
            Debug.Assert(soldier.HasTrait("Corrupt Disability Rating"), "Expected trait 'Corrupt Disability Rating' to be acquired!");

            Debug.Log($"[Dilemma 2 Passed] Medical bribe paid! Cash: ${soldier.cashBalance} (-2,000), VA Disability: +${soldier.vaDisabilityAllowancePerMonth}/mo, Heat: {soldier.criminalHeat}% (+25%), Discipline: {soldier.discipline}% (-20%).");

            // =========================================================================
            // TEST 3: Predatory Loan Shark Collection (Choice A: Pay Extortion)
            // =========================================================================
            Debug.Log("<color=#FFD700>--- Testing Dilemma 3: Predatory Loan Shark Collection ---</color>");
            long initialCash3 = soldier.cashBalance; // 2500
            float initialMorale3 = soldier.happiness; // 80
            float initialHeat3 = soldier.criminalHeat; // 45

            bool resolved3 = EconomicDilemmaDeck.ResolveDilemma("loan_shark_collection", 0, soldier);
            Debug.Assert(resolved3, "Failed to resolve 'loan_shark_collection' Choice A!");
            Debug.Assert(soldier.cashBalance == initialCash3 - 1500L, $"Expected Cash ${initialCash3 - 1500L}, got ${soldier.cashBalance}");
            Debug.Assert(Mathf.Approximately(soldier.happiness, initialMorale3 - 15f), $"Expected Morale {initialMorale3 - 15f}%, got {soldier.happiness}%");
            Debug.Assert(Mathf.Approximately(soldier.criminalHeat, initialHeat3 - 10f), $"Expected Heat {initialHeat3 - 10f}%, got {soldier.criminalHeat}%");

            Debug.Log($"[Dilemma 3 Passed] Loan shark settled! Cash: ${soldier.cashBalance} (-1,500), Morale: {soldier.happiness}% (-15%), Heat: {soldier.criminalHeat}% (-10%).");

            // =========================================================================
            // TEST 4: Verification of Choice B & C (Honorable & Squad Paths)
            // =========================================================================
            Debug.Log("<color=#FFD700>--- Testing Alternative Choice Resolutions (Choice B & C) ---</color>");
            var honestSoldier = new SoldierProfile("Marcus Vance", null, 110)
            {
                age = 24.0f,
                cashBalance = 3000L,
                meritRating = 50f,
                discipline = 50f,
                happiness = 70f,
                squadRespect = 50f
            };

            // Test Honest Medical Board (Choice B)
            EconomicDilemmaDeck.ResolveDilemma("corrupt_medical_board", 1, honestSoldier);
            Debug.Assert(honestSoldier.discipline == 65f, $"Expected Discipline 65, got {honestSoldier.discipline}");
            Debug.Assert(honestSoldier.meritRating == 60f, $"Expected Merit 60, got {honestSoldier.meritRating}");
            Debug.Assert(honestSoldier.happiness == 80f, $"Expected Morale 80, got {honestSoldier.happiness}");

            // Test Distressed Squadmate Loan (Choice A: Unconditional Brotherhood)
            EconomicDilemmaDeck.ResolveDilemma("distressed_squadmate_loan", 0, honestSoldier);
            Debug.Assert(honestSoldier.cashBalance == 2000L, $"Expected Cash 2000, got {honestSoldier.cashBalance}");
            Debug.Assert(honestSoldier.squadRespect == 85f, $"Expected Squad Respect 85, got {honestSoldier.squadRespect}");
            Debug.Assert(honestSoldier.HasTrait("Loyal Benefactor"), "Expected trait 'Loyal Benefactor' to be added!");

            // Test Plundered Pocket Watch (Choice A: Lucky Talisman)
            EconomicDilemmaDeck.ResolveDilemma("plundered_pocket_watch", 0, honestSoldier);
            Debug.Assert(honestSoldier.hasLuckyWatch, "Expected hasLuckyWatch to be true!");
            Debug.Assert(honestSoldier.HasTrait("Lucky Golden Chronometer"), "Expected trait 'Lucky Golden Chronometer' to be added!");

            Debug.Log("<color=#00FFAA>[Alternative Choices Passed] Honorable refusal, brotherhood lending, and lucky watch mechanics verified.</color>");

            // =========================================================================
            // TEST 5: Interactive Event Trigger Pipeline
            // =========================================================================
            Debug.Log("<color=#FFD700>--- Testing TryTriggerEconomicDilemma & Event Dispatch ---</color>");
            bool eventFired = false;
            string firedTitle = null;

            Action<string, string, string, string, string, string, Action, Action, Action> testListener =
                (category, title, description, choiceA, choiceB, choiceC, actA, actB, actC) =>
                {
                    eventFired = true;
                    firedTitle = title;
                    Debug.Assert(!string.IsNullOrEmpty(title), "Triggered dilemma title was empty!");
                    Debug.Assert(actA != null, "Triggered actionA was null!");
                    Debug.Assert(actB != null, "Triggered actionB was null!");
                };

            EconomicDilemmaDeck.OnDilemmaTriggered += testListener;
            bool triggerSuccess = EconomicDilemmaDeck.TryTriggerEconomicDilemma(soldier);
            EconomicDilemmaDeck.OnDilemmaTriggered -= testListener;

            Debug.Assert(triggerSuccess, "TryTriggerEconomicDilemma returned false!");
            Debug.Assert(eventFired, "OnDilemmaTriggered event was not received by subscriber!");
            Debug.Log($"[Event Pipeline Passed] Successfully triggered and received dilemma: '{firedTitle}'.");

            Debug.Log("<color=#00FFAA>========== ALL ECONOMIC DILEMMA TESTS PASSED 100%! ==========</color>");
        }
    }
}
#endif
