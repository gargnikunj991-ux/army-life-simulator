using System;
using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Macroeconomic shocks affecting the wartime empire economy.
    /// Drives dynamic inflation, supply bottlenecks, victory dividend surges, and MP crackdowns.
    /// </summary>
    public enum MacroEconomicShock
    {
        NormalStagnant = 0,                 // Standard peacetime/stalemate baseline economy
        FrontlineVictoryBoom = 1,           // Bonds yield +50%, trade guild shares dividend +50%, morale +10%
        StrategicBlockadeSupplyCrunch = 2,  // PX gear costs +75%, medical kits cost +100%, living costs +30%
        WartimeHyperinflation = 3,          // Living costs +50%, business raw material expenses +30%, cash purchasing power drops
        ArmsIndustryRequisitionBoom = 4,    // Workshop and Security agency revenues +60%, combat bounties +25%
        TreasuryAuditAndMPCrackdown = 5,    // Law enforcement heat from crimes doubles, MP raid frequency high
        DemobilizationRecession = 6         // Civilian salaries cut 20%, trade revenues drop 30%
    }

    /// <summary>
    /// Represents the current macroeconomic condition of the empire, including active shocks,
    /// headline dispatches, remaining duration, and financial/price multipliers.
    /// </summary>
    [Serializable]
    public class EconomicMarketState
    {
        [Header("Active Shock Status")]
        public MacroEconomicShock activeShock = MacroEconomicShock.NormalStagnant;
        public int shockDurationRemaining = 0; // Duration remaining in semesters (6-month turns)

        [Header("Narrative Headlines")]
        public string headlineText = "Imperial Markets Stable: Standard Guild & PX Parity";
        public string descriptiveNarrative = "Trade routes flow steadily through imperial checkpoints. Standard prices and yields apply across military PX and civilian markets.";

        [Header("Required Multipliers")]
        public float priceInflationMultiplier = 1.0f;       // General PX gear and equipment cost multiplier
        public float warBondDividendMultiplier = 1.0f;      // War Bond payout yield multiplier
        public float businessRevenueMultiplier = 1.0f;      // Commercial enterprise revenue multiplier
        public float crimeHeatMultiplier = 1.0f;            // Law enforcement heat multiplier
        public float medicalCostMultiplier = 1.0f;          // Medical kit and trauma supplies multiplier

        [Header("Extended Wartime Multipliers")]
        public float livingCostMultiplier = 1.0f;           // Housing quarters upkeep multiplier
        public float tradeGuildDividendMultiplier = 1.0f;   // Trade Guild stock yield multiplier
        public float combatBountyMultiplier = 1.0f;         // Frontline plunder and tactical bounty multiplier
        public float civilianSalaryMultiplier = 1.0f;       // Civilian job salary multiplier

        public EconomicMarketState()
        {
            ResetToNormal();
        }

        /// <summary>
        /// Resets the macroeconomic environment back to stable equilibrium.
        /// </summary>
        public void ResetToNormal()
        {
            activeShock = MacroEconomicShock.NormalStagnant;
            shockDurationRemaining = 0;
            headlineText = "Imperial Markets Stable: Standard Guild & PX Parity";
            descriptiveNarrative = "Trade routes flow steadily through imperial checkpoints. Standard prices and yields apply across military PX and civilian markets.";
            priceInflationMultiplier = 1.0f;
            warBondDividendMultiplier = 1.0f;
            businessRevenueMultiplier = 1.0f;
            crimeHeatMultiplier = 1.0f;
            medicalCostMultiplier = 1.0f;
            livingCostMultiplier = 1.0f;
            tradeGuildDividendMultiplier = 1.0f;
            combatBountyMultiplier = 1.0f;
            civilianSalaryMultiplier = 1.0f;
        }

        /// <summary>
        /// Configures multipliers, headlines, and narrative descriptions for a given shock.
        /// </summary>
        public void ConfigureShock(MacroEconomicShock shock, int durationSemesters)
        {
            activeShock = shock;
            shockDurationRemaining = Math.Max(1, durationSemesters);

            switch (shock)
            {
                case MacroEconomicShock.NormalStagnant:
                    ResetToNormal();
                    break;

                case MacroEconomicShock.FrontlineVictoryBoom:
                    headlineText = "VICTORY AT THE FRONTLINE: Imperial Breakthrough Triggers Economic Boom!";
                    descriptiveNarrative = "Allied armies smash enemy lines and capture industrial redoubts! War Bonds and Guild dividends surge +50%, while frontline troop morale skyrockets.";
                    priceInflationMultiplier = 1.0f;
                    warBondDividendMultiplier = 1.50f;     // Bonds yield +50%
                    businessRevenueMultiplier = 1.20f;
                    crimeHeatMultiplier = 1.0f;
                    medicalCostMultiplier = 1.0f;
                    livingCostMultiplier = 1.0f;
                    tradeGuildDividendMultiplier = 1.50f;  // Trade guild shares dividend +50%
                    combatBountyMultiplier = 1.25f;
                    civilianSalaryMultiplier = 1.10f;
                    break;

                case MacroEconomicShock.StrategicBlockadeSupplyCrunch:
                    headlineText = "STRATEGIC BLOCKADE DECLARED: Sea Lanes Severed, Severe Supply Crunch!";
                    descriptiveNarrative = "Hostile raiders and naval ironclads intercept vital cargo shipping. PX tactical gear costs surge +75%, trauma medical kits double (+100%), and quarters living costs increase +30%!";
                    priceInflationMultiplier = 1.75f;      // PX gear costs +75%
                    warBondDividendMultiplier = 1.0f;
                    businessRevenueMultiplier = 0.90f;
                    crimeHeatMultiplier = 1.0f;
                    medicalCostMultiplier = 2.00f;         // Medical kits cost +100%
                    livingCostMultiplier = 1.30f;          // Living costs +30%
                    tradeGuildDividendMultiplier = 0.85f;
                    combatBountyMultiplier = 1.0f;
                    civilianSalaryMultiplier = 1.0f;
                    break;

                case MacroEconomicShock.WartimeHyperinflation:
                    headlineText = "WARTIME HYPERINFLATION: Imperial Currency Weakens, Living Costs Surge!";
                    descriptiveNarrative = "Runaway military expenditures trigger unbacked currency printing. Base living expenses jump +50%, commercial materials rise +30%, and cash purchasing power evaporates.";
                    priceInflationMultiplier = 1.50f;
                    warBondDividendMultiplier = 1.0f;
                    businessRevenueMultiplier = 0.70f;     // Business raw material expenses +30% reduces net margin
                    crimeHeatMultiplier = 1.0f;
                    medicalCostMultiplier = 1.50f;
                    livingCostMultiplier = 1.50f;          // Living costs +50%
                    tradeGuildDividendMultiplier = 0.80f;
                    combatBountyMultiplier = 1.0f;
                    civilianSalaryMultiplier = 0.90f;
                    break;

                case MacroEconomicShock.ArmsIndustryRequisitionBoom:
                    headlineText = "ARMS REQUISITION ORDERS: Defense Ministry Mobilizes Imperial Industry!";
                    descriptiveNarrative = "Imperial arsenals issue sweeping procurement contracts. Machine workshops and private security outfits reap +60% revenue windfalls, and soldier combat bounties rise +25%!";
                    priceInflationMultiplier = 1.10f;
                    warBondDividendMultiplier = 1.15f;
                    businessRevenueMultiplier = 1.60f;     // Workshop and Security agency revenues +60%
                    crimeHeatMultiplier = 1.0f;
                    medicalCostMultiplier = 1.0f;
                    livingCostMultiplier = 1.05f;
                    tradeGuildDividendMultiplier = 1.25f;
                    combatBountyMultiplier = 1.25f;        // Combat bounties +25%
                    civilianSalaryMultiplier = 1.05f;
                    break;

                case MacroEconomicShock.TreasuryAuditAndMPCrackdown:
                    headlineText = "TREASURY AUDIT & MP CRACKDOWN: Provost Marshals Sweep Garrisons!";
                    descriptiveNarrative = "Special inquisitors and military police stage surprise depot inspections. Criminal heat generation doubles and black-market syndicates face sweeping arrests!";
                    priceInflationMultiplier = 1.0f;
                    warBondDividendMultiplier = 1.0f;
                    businessRevenueMultiplier = 0.95f;
                    crimeHeatMultiplier = 2.0f;            // Law enforcement heat from crimes doubles
                    medicalCostMultiplier = 1.0f;
                    livingCostMultiplier = 1.0f;
                    tradeGuildDividendMultiplier = 1.0f;
                    combatBountyMultiplier = 1.0f;
                    civilianSalaryMultiplier = 1.0f;
                    break;

                case MacroEconomicShock.DemobilizationRecession:
                    headlineText = "POST-WAR DEMOBILIZATION RECESSION: Municipal Budgets & Commerce Slump!";
                    descriptiveNarrative = "Sudden armistice and troop drawdowns trigger severe economic headwinds. Civilian salaries are slashed by 20% and trade caravan revenues collapse by 30%!";
                    priceInflationMultiplier = 0.90f;
                    warBondDividendMultiplier = 0.85f;
                    businessRevenueMultiplier = 0.70f;     // Trade revenues drop 30%
                    crimeHeatMultiplier = 1.20f;
                    medicalCostMultiplier = 1.0f;
                    livingCostMultiplier = 0.95f;
                    tradeGuildDividendMultiplier = 0.70f;
                    combatBountyMultiplier = 0.75f;
                    civilianSalaryMultiplier = 0.80f;      // Civilian salaries cut 20%
                    break;
            }
        }
    }
}
