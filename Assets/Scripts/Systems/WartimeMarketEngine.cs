using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Wartime Market & Inflation Engine.
    /// Drives dynamic macro shocks, PX military gear price surges, housing cost inflations,
    /// War Bond yield windfalls, business revenue fluctuations, and law enforcement heat multipliers.
    /// </summary>
    public class WartimeMarketEngine
    {
        private static WartimeMarketEngine s_instance;
        public static WartimeMarketEngine Instance => s_instance ?? (s_instance = new WartimeMarketEngine());

        private static EconomicMarketState s_currentState = new EconomicMarketState();
        
        /// <summary>
        /// Global static access to current economic market state.
        /// </summary>
        public static EconomicMarketState CurrentState
        {
            get => s_currentState ?? (s_currentState = new EconomicMarketState());
            set => s_currentState = value;
        }

        public static EconomicMarketState State => CurrentState;

        /// <summary>
        /// When true, AdvanceMarketSemester may roll random macroeconomic shocks when stagnant.
        /// Can be disabled during deterministic unit tests.
        /// </summary>
        public static bool AllowRandomShocks { get; set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSubscribeToEvents()
        {
            GameEvents.OnSemesterAdvanced -= HandleSemesterAdvanced;
            GameEvents.OnSemesterAdvanced += HandleSemesterAdvanced;
        }

        private static void HandleSemesterAdvanced(int year, int semester, bool isDeployment)
        {
            SoldierProfile activeSoldier = (GameManager.Instance != null) ? GameManager.Instance.currentSoldier : null;
            AdvanceMarketSemester(activeSoldier);
        }

        /// <summary>
        /// Resets the market state to default stable equilibrium.
        /// </summary>
        public static void ResetToNormal()
        {
            CurrentState.ResetToNormal();
        }

        /// <summary>
        /// Explicitly applies a macroeconomic shock for scripted events or unit tests.
        /// </summary>
        public static void ApplyShock(MacroEconomicShock shock, int durationSemesters = 2)
        {
            if (shock == MacroEconomicShock.NormalStagnant)
            {
                ResetToNormal();
                return;
            }

            CurrentState.ConfigureShock(shock, durationSemesters);

            GameEvents.OnFeedEventLogged?.Invoke("MARKET",
                $"<color=#FF8844><b>[MARKET SHOCK ACTIVATED]</b></color> <b>{CurrentState.headlineText}</b>\n{CurrentState.descriptiveNarrative} (Duration: {durationSemesters} semesters)");
        }

        /// <summary>
        /// Advances macroeconomic conditions by one semester (6 months).
        /// Decrements active shock durations, handles returns to equilibrium, or rolls new market events.
        /// </summary>
        public static void AdvanceMarketSemester(SoldierProfile soldier)
        {
            if (CurrentState.activeShock != MacroEconomicShock.NormalStagnant)
            {
                CurrentState.shockDurationRemaining--;

                if (CurrentState.shockDurationRemaining <= 0)
                {
                    MacroEconomicShock expiredShock = CurrentState.activeShock;
                    ResetToNormal();
                    GameEvents.OnFeedEventLogged?.Invoke("MARKET",
                        $"<color=#00FFAA><b>[MARKET NORMALIZED]</b></color> The <b>{expiredShock}</b> conditions have subsided. Imperial prices, living expenses, and investment yields have returned to baseline parity.");
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("MARKET",
                        $"<color=#FFD700><b>[MARKET CONDITIONS]</b></color> <b>{CurrentState.headlineText}</b> ({CurrentState.shockDurationRemaining} semester(s) remaining).");

                    // Passive shock effects on the active soldier
                    if (soldier != null && CurrentState.activeShock == MacroEconomicShock.FrontlineVictoryBoom)
                    {
                        soldier.happiness = Mathf.Min(100f, soldier.happiness + 10f); // Frontline victory morale +10%
                    }
                }
            }
            else
            {
                // Currently stagnant equilibrium; roll for random shock if permitted
                if (AllowRandomShocks && UnityEngine.Random.value < 0.35f)
                {
                    MacroEconomicShock[] availableShocks = new MacroEconomicShock[]
                    {
                        MacroEconomicShock.FrontlineVictoryBoom,
                        MacroEconomicShock.StrategicBlockadeSupplyCrunch,
                        MacroEconomicShock.WartimeHyperinflation,
                        MacroEconomicShock.ArmsIndustryRequisitionBoom,
                        MacroEconomicShock.TreasuryAuditAndMPCrackdown,
                        MacroEconomicShock.DemobilizationRecession
                    };

                    MacroEconomicShock rolled = availableShocks[UnityEngine.Random.Range(0, availableShocks.Length)];
                    int duration = UnityEngine.Random.Range(2, 4); // 2-3 semesters
                    ApplyShock(rolled, duration);

                    if (soldier != null && rolled == MacroEconomicShock.FrontlineVictoryBoom)
                    {
                        soldier.happiness = Mathf.Min(100f, soldier.happiness + 10f);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the market-adjusted purchase price for PX gear and medical consumables.
        /// Under StrategicBlockadeSupplyCrunch, medical kits cost +100% while standard gear costs +75%.
        /// </summary>
        public static long GetAdjustedPrice(long basePrice, GearCategory category)
        {
            float multiplier = (category == GearCategory.Medical)
                ? CurrentState.medicalCostMultiplier
                : CurrentState.priceInflationMultiplier;

            return Math.Max(1L, (long)Math.Round(basePrice * multiplier));
        }

        /// <summary>
        /// Calculates the market-adjusted housing upkeep cost based on branch, tier, and macroeconomic inflation.
        /// </summary>
        public static long GetAdjustedHousingCost(HousingTier tier, MilitaryBranch branch)
        {
            long baseCost = EconomyData.GetHousingSemesterCost(tier, branch);
            return Math.Max(0L, (long)Math.Round(baseCost * CurrentState.livingCostMultiplier));
        }

        /// <summary>
        /// Calculates the market-adjusted semi-annual dividend return on Imperial War Bonds.
        /// Under FrontlineVictoryBoom, bond dividends yield +50%.
        /// </summary>
        public static long GetAdjustedBondDividend(long baseDividend)
        {
            return Math.Max(0L, (long)Math.Round(baseDividend * CurrentState.warBondDividendMultiplier));
        }

        /// <summary>
        /// Calculates the market-adjusted semi-annual dividend return on Trade Guild Shares.
        /// </summary>
        public static long GetAdjustedTradeGuildDividend(long baseDividend)
        {
            return Math.Max(0L, (long)Math.Round(baseDividend * CurrentState.tradeGuildDividendMultiplier));
        }

        /// <summary>
        /// Calculates the market-adjusted commercial enterprise semester revenue.
        /// Under ArmsIndustryRequisitionBoom, workshops and security firms enjoy +60% surges.
        /// Under DemobilizationRecession, commerce drops -30%.
        /// </summary>
        public static long GetAdjustedBusinessRevenue(long baseRevenue)
        {
            return Math.Max(0L, (long)Math.Round(baseRevenue * CurrentState.businessRevenueMultiplier));
        }

        /// <summary>
        /// Overload providing industry-specific revenue adjustments for player businesses.
        /// </summary>
        public static long GetAdjustedBusinessRevenue(CommercialBusiness business, long baseRevenue)
        {
            if (CurrentState.activeShock == MacroEconomicShock.ArmsIndustryRequisitionBoom)
            {
                if (business == CommercialBusiness.Workshop || business == CommercialBusiness.PrivateSecurityAgency || business == CommercialBusiness.DefenseMunitionsConglomerate)
                {
                    return Math.Max(0L, (long)Math.Round(baseRevenue * 1.60f));
                }
            }

            return GetAdjustedBusinessRevenue(baseRevenue);
        }

        /// <summary>
        /// Calculates the market-adjusted criminal heat generated by illicit operations.
        /// Under TreasuryAuditAndMPCrackdown, law enforcement heat from crimes doubles (2.0x).
        /// </summary>
        public static float GetAdjustedCrimeHeat(float baseHeat)
        {
            return baseHeat * CurrentState.crimeHeatMultiplier;
        }

        /// <summary>
        /// Calculates the market-adjusted frontline combat tour and war plunder bounties.
        /// </summary>
        public static long GetAdjustedCombatBounty(long baseBounty)
        {
            return Math.Max(0L, (long)Math.Round(baseBounty * CurrentState.combatBountyMultiplier));
        }

        /// <summary>
        /// Calculates the market-adjusted civilian career salary.
        /// Under DemobilizationRecession, civilian wages take an immediate -20% austerity reduction.
        /// </summary>
        public static long GetAdjustedCivilianSalary(long baseSalary)
        {
            return Math.Max(0L, (long)Math.Round(baseSalary * CurrentState.civilianSalaryMultiplier));
        }
    }
}
