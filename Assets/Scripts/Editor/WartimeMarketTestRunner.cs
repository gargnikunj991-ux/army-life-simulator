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
    /// Automated test runner verifying macroeconomic shocks, dynamic inflation,
    /// supply crunch adjustments, dividend surges, MP crackdown heat multipliers,
    /// and semester duration lifecycle return to equilibrium.
    /// </summary>
    public static class WartimeMarketTestRunner
    {
        [MenuItem("Army Life/Run Wartime Market & Inflation Test", false, 21)]
        public static void RunWartimeMarketTest()
        {
            Debug.Log("<color=#00D4FF>========== STARTING WARTIME MARKET & INFLATION TESTS ==========</color>");

            // Disable random shock rolls during deterministic verification
            WartimeMarketEngine.AllowRandomShocks = false;

            // ==================== TEST 1: DEFAULT MARKET EQUILIBRIUM ====================
            Debug.Log("[Test 1] Verifying default market conditions...");
            WartimeMarketEngine.ResetToNormal();

            AssertCondition(WartimeMarketEngine.CurrentState.activeShock == MacroEconomicShock.NormalStagnant,
                "Initial shock should be NormalStagnant.");
            AssertCondition(WartimeMarketEngine.CurrentState.shockDurationRemaining == 0,
                "Initial shock duration remaining should be 0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.priceInflationMultiplier, 1.0f),
                "Default priceInflationMultiplier must be 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.medicalCostMultiplier, 1.0f),
                "Default medicalCostMultiplier must be 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.livingCostMultiplier, 1.0f),
                "Default livingCostMultiplier must be 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.warBondDividendMultiplier, 1.0f),
                "Default warBondDividendMultiplier must be 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.businessRevenueMultiplier, 1.0f),
                "Default businessRevenueMultiplier must be 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.crimeHeatMultiplier, 1.0f),
                "Default crimeHeatMultiplier must be 1.0.");

            AssertCondition(WartimeMarketEngine.GetAdjustedPrice(500L, GearCategory.Armor) == 500L,
                "Default gear price should match base price.");
            AssertCondition(WartimeMarketEngine.GetAdjustedPrice(300L, GearCategory.Medical) == 300L,
                "Default medical price should match base price.");
            AssertCondition(WartimeMarketEngine.GetAdjustedHousingCost(HousingTier.BarracksOrSlums, MilitaryBranch.Army) == 250L,
                "Default Army barracks cost should match base $250.");
            AssertCondition(WartimeMarketEngine.GetAdjustedBondDividend(30L) == 30L,
                "Default War Bond dividend should match base $30.");
            AssertCondition(WartimeMarketEngine.GetAdjustedBusinessRevenue(800L) == 800L,
                "Default business revenue should match base $800.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.GetAdjustedCrimeHeat(20f), 20f),
                "Default crime heat should be unadjusted.");
            Debug.Log("<color=#00FF66>[Test 1 PASSED]</color> Default market equilibrium confirmed.");

            // ==================== TEST 2: STRATEGIC BLOCKADE SUPPLY CRUNCH ====================
            Debug.Log("[Test 2] Verifying StrategicBlockadeSupplyCrunch shock...");
            WartimeMarketEngine.ApplyShock(MacroEconomicShock.StrategicBlockadeSupplyCrunch, 2);

            AssertCondition(WartimeMarketEngine.CurrentState.activeShock == MacroEconomicShock.StrategicBlockadeSupplyCrunch,
                "Active shock should be StrategicBlockadeSupplyCrunch.");
            AssertCondition(WartimeMarketEngine.CurrentState.shockDurationRemaining == 2,
                "Shock duration remaining should be 2 semesters.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.priceInflationMultiplier, 1.75f),
                "StrategicBlockadeSupplyCrunch PX gear price multiplier must be 1.75 (+75%).");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.medicalCostMultiplier, 2.00f),
                "StrategicBlockadeSupplyCrunch medical kits cost multiplier must be 2.00 (+100%).");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.livingCostMultiplier, 1.30f),
                "StrategicBlockadeSupplyCrunch quarters living costs multiplier must be 1.30 (+30%).");

            // PX gear price scaling (+75%)
            long baseArmorCost = 1000L;
            long adjustedArmorCost = WartimeMarketEngine.GetAdjustedPrice(baseArmorCost, GearCategory.Armor);
            AssertCondition(adjustedArmorCost == 1750L,
                $"Expected adjusted armor cost 1750, got {adjustedArmorCost}.");

            long baseWeaponCost = 200L;
            long adjustedWeaponCost = WartimeMarketEngine.GetAdjustedPrice(baseWeaponCost, GearCategory.Weapon);
            AssertCondition(adjustedWeaponCost == 350L,
                $"Expected adjusted weapon cost 350, got {adjustedWeaponCost}.");

            // Medical consumables scaling (+100%)
            long baseMedCost = 500L;
            long adjustedMedCost = WartimeMarketEngine.GetAdjustedPrice(baseMedCost, GearCategory.Medical);
            AssertCondition(adjustedMedCost == 1000L,
                $"Expected adjusted medical cost 1000 (+100%), got {adjustedMedCost}.");

            // Living costs scaling (+30%)
            long baseHousingArmy = EconomyData.GetHousingSemesterCost(HousingTier.BarracksOrSlums, MilitaryBranch.Army); // 250
            long adjustedHousingArmy = WartimeMarketEngine.GetAdjustedHousingCost(HousingTier.BarracksOrSlums, MilitaryBranch.Army);
            AssertCondition(adjustedHousingArmy == 325L,
                $"Expected adjusted barracks cost 325 (+30%), got {adjustedHousingArmy}.");

            long baseVillaArmy = EconomyData.GetHousingSemesterCost(HousingTier.ComfortableHome, MilitaryBranch.Army); // 1800
            long adjustedVillaArmy = WartimeMarketEngine.GetAdjustedHousingCost(HousingTier.ComfortableHome, MilitaryBranch.Army);
            AssertCondition(adjustedVillaArmy == 2340L,
                $"Expected adjusted villa cost 2340 (+30%), got {adjustedVillaArmy}.");
            Debug.Log("<color=#00FF66>[Test 2 PASSED]</color> StrategicBlockadeSupplyCrunch prices and living costs verified.");

            // ==================== TEST 3: FRONTLINE VICTORY BOOM ====================
            Debug.Log("[Test 3] Verifying FrontlineVictoryBoom shock...");
            WartimeMarketEngine.ApplyShock(MacroEconomicShock.FrontlineVictoryBoom, 2);

            AssertCondition(WartimeMarketEngine.CurrentState.activeShock == MacroEconomicShock.FrontlineVictoryBoom,
                "Active shock should be FrontlineVictoryBoom.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.warBondDividendMultiplier, 1.50f),
                "War Bond dividend multiplier must be 1.50 (+50%).");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.tradeGuildDividendMultiplier, 1.50f),
                "Trade Guild dividend multiplier must be 1.50 (+50%).");

            long baseBondDiv = 30L;
            long adjustedBondDiv = WartimeMarketEngine.GetAdjustedBondDividend(baseBondDiv);
            AssertCondition(adjustedBondDiv == 45L,
                $"Expected adjusted bond dividend 45 (+50%), got {adjustedBondDiv}.");

            long tenBondsDiv = 300L;
            long adjustedTenBondsDiv = WartimeMarketEngine.GetAdjustedBondDividend(tenBondsDiv);
            AssertCondition(adjustedTenBondsDiv == 450L,
                $"Expected adjusted 10-bond dividend 450 (+50%), got {adjustedTenBondsDiv}.");

            long baseGuildDiv = 60L;
            long adjustedGuildDiv = WartimeMarketEngine.GetAdjustedTradeGuildDividend(baseGuildDiv);
            AssertCondition(adjustedGuildDiv == 90L,
                $"Expected adjusted guild dividend 90 (+50%), got {adjustedGuildDiv}.");

            // Frontline victory morale boost (+10%)
            var soldier = new SoldierProfile() { happiness = 60f };
            WartimeMarketEngine.AdvanceMarketSemester(soldier);
            AssertCondition(Mathf.Approximately(soldier.happiness, 70f),
                $"Expected soldier happiness boosted from 60 to 70 (+10%), got {soldier.happiness}.");
            Debug.Log("<color=#00FF66>[Test 3 PASSED]</color> FrontlineVictoryBoom bond dividends and troop morale verified.");

            // ==================== TEST 4: TREASURY AUDIT & MP CRACKDOWN ====================
            Debug.Log("[Test 4] Verifying TreasuryAuditAndMPCrackdown shock...");
            WartimeMarketEngine.ApplyShock(MacroEconomicShock.TreasuryAuditAndMPCrackdown, 2);

            AssertCondition(WartimeMarketEngine.CurrentState.activeShock == MacroEconomicShock.TreasuryAuditAndMPCrackdown,
                "Active shock should be TreasuryAuditAndMPCrackdown.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.crimeHeatMultiplier, 2.0f),
                "Crime heat multiplier must be 2.0 (doubled).");

            float pettyCrimeHeat = 10f;
            float adjustedPettyHeat = WartimeMarketEngine.GetAdjustedCrimeHeat(pettyCrimeHeat);
            AssertCondition(Mathf.Approximately(adjustedPettyHeat, 20f),
                $"Expected adjusted crime heat 20f, got {adjustedPettyHeat}.");

            float majorHeistHeat = 40f;
            float adjustedHeistHeat = WartimeMarketEngine.GetAdjustedCrimeHeat(majorHeistHeat);
            AssertCondition(Mathf.Approximately(adjustedHeistHeat, 80f),
                $"Expected adjusted crime heat 80f, got {adjustedHeistHeat}.");
            Debug.Log("<color=#00FF66>[Test 4 PASSED]</color> TreasuryAuditAndMPCrackdown doubled law enforcement heat verified.");

            // ==================== TEST 5: EXTENDED SHOCK SUITE ====================
            Debug.Log("[Test 5] Verifying Hyperinflation, Arms Requisition Boom, and Demobilization Recession...");

            // 5a. WartimeHyperinflation (Living costs +50%, business raw material expenses +30%)
            WartimeMarketEngine.ApplyShock(MacroEconomicShock.WartimeHyperinflation, 2);
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.livingCostMultiplier, 1.50f),
                "Hyperinflation livingCostMultiplier must be 1.50 (+50%).");
            long hyperBarracksCost = WartimeMarketEngine.GetAdjustedHousingCost(HousingTier.BarracksOrSlums, MilitaryBranch.Army);
            AssertCondition(hyperBarracksCost == 375L,
                $"Expected hyperinflation barracks cost 375 (250 * 1.5), got {hyperBarracksCost}.");

            // 5b. ArmsIndustryRequisitionBoom (Workshop and Security agency revenues +60%, combat bounties +25%)
            WartimeMarketEngine.ApplyShock(MacroEconomicShock.ArmsIndustryRequisitionBoom, 2);
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.businessRevenueMultiplier, 1.60f),
                "Arms requisition boom business revenue multiplier must be 1.60 (+60%).");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.combatBountyMultiplier, 1.25f),
                "Arms requisition boom combat bounty multiplier must be 1.25 (+25%).");
            long workshopRev = WartimeMarketEngine.GetAdjustedBusinessRevenue(CommercialBusiness.Workshop, 800L);
            AssertCondition(workshopRev == 1280L,
                $"Expected adjusted workshop revenue 1280 (800 * 1.6), got {workshopRev}.");
            long securityRev = WartimeMarketEngine.GetAdjustedBusinessRevenue(CommercialBusiness.PrivateSecurityAgency, 10000L);
            AssertCondition(securityRev == 16000L,
                $"Expected adjusted security agency revenue 16000 (10000 * 1.6), got {securityRev}.");
            long bounty = WartimeMarketEngine.GetAdjustedCombatBounty(4000L);
            AssertCondition(bounty == 5000L,
                $"Expected adjusted combat bounty 5000 (4000 * 1.25), got {bounty}.");

            // 5c. DemobilizationRecession (Civilian salaries cut 20%, trade revenues drop 30%)
            WartimeMarketEngine.ApplyShock(MacroEconomicShock.DemobilizationRecession, 2);
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.civilianSalaryMultiplier, 0.80f),
                "Demobilization recession civilian salary multiplier must be 0.80 (-20%).");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.businessRevenueMultiplier, 0.70f),
                "Demobilization recession trade/business multiplier must be 0.70 (-30%).");
            long civilSalary = WartimeMarketEngine.GetAdjustedCivilianSalary(1000L);
            AssertCondition(civilSalary == 800L,
                $"Expected adjusted civilian salary 800 (1000 * 0.80), got {civilSalary}.");
            long tradeRev = WartimeMarketEngine.GetAdjustedBusinessRevenue(1000L);
            AssertCondition(tradeRev == 700L,
                $"Expected adjusted trade revenue 700 (1000 * 0.70), got {tradeRev}.");
            Debug.Log("<color=#00FF66>[Test 5 PASSED]</color> Extended shocks verified.");

            // ==================== TEST 6: SEMESTER ADVANCEMENT & EQUILIBRIUM RETURN ====================
            Debug.Log("[Test 6] Verifying semester duration countdown and return to equilibrium...");
            WartimeMarketEngine.ApplyShock(MacroEconomicShock.FrontlineVictoryBoom, 2);
            AssertCondition(WartimeMarketEngine.CurrentState.activeShock == MacroEconomicShock.FrontlineVictoryBoom,
                "Shock should be FrontlineVictoryBoom.");
            AssertCondition(WartimeMarketEngine.CurrentState.shockDurationRemaining == 2,
                "Duration should be 2.");

            // Advance 1st semester -> 1 remaining
            WartimeMarketEngine.AdvanceMarketSemester(null);
            AssertCondition(WartimeMarketEngine.CurrentState.activeShock == MacroEconomicShock.FrontlineVictoryBoom,
                "Shock should remain active after 1st advance.");
            AssertCondition(WartimeMarketEngine.CurrentState.shockDurationRemaining == 1,
                "Duration should decrement to 1.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.warBondDividendMultiplier, 1.50f),
                "Dividend multiplier should remain 1.50 during active shock.");

            // Advance 2nd semester -> Expires back to NormalStagnant
            WartimeMarketEngine.AdvanceMarketSemester(null);
            AssertCondition(WartimeMarketEngine.CurrentState.activeShock == MacroEconomicShock.NormalStagnant,
                "Shock should expire to NormalStagnant.");
            AssertCondition(WartimeMarketEngine.CurrentState.shockDurationRemaining == 0,
                "Duration remaining should be 0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.priceInflationMultiplier, 1.0f),
                "Price inflation multiplier should return to 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.warBondDividendMultiplier, 1.0f),
                "War Bond dividend multiplier should return to 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.crimeHeatMultiplier, 1.0f),
                "Crime heat multiplier should return to 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.livingCostMultiplier, 1.0f),
                "Living cost multiplier should return to 1.0.");
            AssertCondition(Mathf.Approximately(WartimeMarketEngine.CurrentState.businessRevenueMultiplier, 1.0f),
                "Business revenue multiplier should return to 1.0.");
            AssertCondition(WartimeMarketEngine.GetAdjustedPrice(1000L, GearCategory.Medical) == 1000L,
                "Medical gear price should return to baseline.");
            AssertCondition(WartimeMarketEngine.GetAdjustedBondDividend(30L) == 30L,
                "War Bond dividend should return to baseline.");
            Debug.Log("<color=#00FF66>[Test 6 PASSED]</color> Semester countdown and return to equilibrium verified.");

            // ==================== TEST 7: GAMEEVENTS FEED LOGGING ====================
            Debug.Log("[Test 7] Verifying GameEvents.OnFeedEventLogged output...");
            bool feedLogged = false;
            string lastTag = string.Empty;
            string lastMessage = string.Empty;

            Action<string, string> handler = (tag, message) =>
            {
                feedLogged = true;
                lastTag = tag;
                lastMessage = message;
            };

            GameEvents.OnFeedEventLogged += handler;
            try
            {
                WartimeMarketEngine.ApplyShock(MacroEconomicShock.TreasuryAuditAndMPCrackdown, 3);
                AssertCondition(feedLogged, "Feed event should have been logged on ApplyShock.");
                AssertCondition(lastTag == "MARKET", $"Feed tag should be 'MARKET', got '{lastTag}'.");
                AssertCondition(lastMessage.Contains("TREASURY AUDIT"), "Headline message should contain shock title.");
            }
            finally
            {
                GameEvents.OnFeedEventLogged -= handler;
            }
            Debug.Log("<color=#00FF66>[Test 7 PASSED]</color> Narrative feed logging verified.");

            // Reset back to equilibrium and re-enable random shocks
            WartimeMarketEngine.ResetToNormal();
            WartimeMarketEngine.AllowRandomShocks = true;

            Debug.Log("<color=#00FFAA>========== ALL WARTIME MARKET & INFLATION TESTS PASSED 100%! ==========</color>");
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
