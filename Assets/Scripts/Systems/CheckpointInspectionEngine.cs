using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Checkpoint & Guard Duty Inspection Engine (Papers, Please inspired mechanics).
    /// Generates procedural inspection cases, validates passports, examines cargo manifests,
    /// and evaluates tactical security decisions (Approve, Search, Detain).
    /// </summary>
    public static class CheckpointInspectionEngine
    {
        private static readonly System.Random rng = new System.Random();

        private static readonly string[] CivilianNames =
        {
            "Viktor Corvin", "Gregor Vasili", "Elena Rostova", "Klaus Brandt",
            "Dmitri Volkov", "Mira Vance", "Anton Richter", "Tatyana Morova",
            "Boris Stanislav", "Lukas Lindqvist", "Yelena Vane", "Soren Kael"
        };

        private static readonly string[] ImperialDistricts =
        {
            "Aethelgard Imperial Central",
            "Kaelen Frontier Province",
            "Oakhaven Agricultural Basin",
            "Port Iron Maritime District",
            "Arden Valley Forward Command"
        };

        private static readonly string[] DeclaredCargoList =
        {
            "Agricultural Turnips & Grain Sacks",
            "Standard Army C-Rations & Dried Meat",
            "Machinery Spare Parts & Iron Castings",
            "Field Hospital Linens & First-Aid Packs",
            "Civilian Apparel, Cloth & Personal Luggage",
            "Industrial Machine Lubricant & Kerosene"
        };

        private static readonly string[] WantedBulletins =
        {
            "ALERT #104: Red Hand partisan insurgent operative suspected of forward depot sabotage.",
            "ALERT #219: Master forger moving stolen military radio ciphers across district lines.",
            "ALERT #308: Black-market weapons courier distributing unlicensed machine pistols.",
            "ALERT #415: Hostile infiltrator carrying clandestine maps of garrison trench perimeters."
        };

        /// <summary>
        /// Generates a procedural vehicle inspection case.
        /// </summary>
        /// <param name="seed">Deterministic seed (0 for pseudo-random).</param>
        /// <param name="discrepancyProbability">Probability of having some security discrepancy (0.0f - 1.0f).</param>
        public static CheckpointVehicleCase GenerateInspectionCase(int seed = 0, float discrepancyProbability = 0.5f)
        {
            var rand = (seed != 0) ? new System.Random(seed) : rng;

            string civilianName = CivilianNames[rand.Next(CivilianNames.Length)];
            var vehicleType = (VehicleType)rand.Next(Enum.GetValues(typeof(VehicleType)).Length);
            string licensePlate = $"VAL-{rand.Next(100, 999)}-{rand.Next(10, 99)}";
            string district = ImperialDistricts[rand.Next(ImperialDistricts.Length)];

            bool hasDiscrepancy = rand.NextDouble() < discrepancyProbability;

            bool isCounterfeit = false;
            bool isWanted = false;
            bool isExpired = false;
            bool isSealForged = false;
            var hiddenCargo = HiddenCargoType.None;
            string hiddenDesc = "No hidden compartment detected. Cargo clean.";
            string wantedBulletin = null;

            if (hasDiscrepancy)
            {
                // Choose nature of discrepancy
                int discrepancyRoll = rand.Next(0, 4);

                switch (discrepancyRoll)
                {
                    case 0: // Wanted Infiltrator
                        isWanted = true;
                        wantedBulletin = WantedBulletins[rand.Next(WantedBulletins.Length)];
                        // Infiltrators often carry forged papers or weapons
                        if (rand.NextDouble() < 0.6)
                        {
                            hiddenCargo = HiddenCargoType.Weapons;
                            hiddenDesc = "Concealed crates of automatic carbines and fragmentation grenades!";
                        }
                        break;

                    case 1: // Forged or Expired Passport
                        isCounterfeit = true;
                        if (rand.NextDouble() < 0.5)
                        {
                            isSealForged = true; // Invalid smudged or missing imperial seal
                        }
                        else
                        {
                            isExpired = true; // Expired 6 months ago
                        }
                        break;

                    case 2: // Smuggling Contraband Cargo
                        int cargoRoll = rand.Next(0, 4);
                        if (cargoRoll == 0)
                        {
                            hiddenCargo = HiddenCargoType.Weapons;
                            hiddenDesc = "Submachine guns and munitions packed beneath grain sacks!";
                        }
                        else if (cargoRoll == 1)
                        {
                            hiddenCargo = HiddenCargoType.ContrabandMoonshine;
                            hiddenDesc = "Unlicensed 120-proof barracks moonshine stored inside auxiliary fuel tanks!";
                        }
                        else if (cargoRoll == 2)
                        {
                            hiddenCargo = HiddenCargoType.BlackMarketOptics;
                            hiddenDesc = "Stolen Imperial Army sniper scopes and night prism optics in false crates!";
                        }
                        else
                        {
                            hiddenCargo = HiddenCargoType.FalseCompartment;
                            hiddenDesc = "Welded double-bottom chassis concealing undeclared bullion and encrypted letters!";
                        }
                        break;

                    case 3: // Displaced War Refugees
                        hiddenCargo = HiddenCargoType.Refugees;
                        hiddenDesc = "Unarmed civilian war refugees and children concealed beneath canvas tarpaulins.";
                        break;
                }
            }

            int currentYear = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2026;
            string expiryStr = isExpired ? $"Year {currentYear - 1}, Semester 2" : $"Year {currentYear + 2}, Semester 1";
            string photoId = $"ID-VAL-{rand.Next(1000, 9999)}";

            var passport = new TransitPassport(
                civilianName,
                photoId,
                district,
                expiryStr,
                isExpired,
                !isSealForged,
                "Commercial Transit & Inter-District Trade",
                $"Signed: {civilianName}"
            );

            string declared = DeclaredCargoList[rand.Next(DeclaredCargoList.Length)];
            float declaredWeight = rand.Next(400, 3500);
            string destination = district.Contains("Central") ? "Imperial Capital Depot" : "Frontline Garrison Outpost";

            var manifest = new CargoManifest(
                declared,
                declaredWeight,
                destination,
                hiddenCargo,
                hiddenDesc
            );

            string notes = $"Driver arrived in a {vehicleType}. Engine idling. Awaiting gate clearance.";

            return new CheckpointVehicleCase
            {
                caseId = $"CASE-{rand.Next(10000, 99999)}",
                civilianName = civilianName,
                vehicleType = vehicleType,
                licensePlate = licensePlate,
                transitPassport = passport,
                cargoManifest = manifest,
                isCounterfeitPassport = isCounterfeit || isExpired || isSealForged,
                isWantedInfiltrator = isWanted,
                wantedBulletinDescription = wantedBulletin,
                vehicleVisualNotes = notes
            };
        }

        /// <summary>
        /// Evaluates player decision (ApprovePass, SearchCargo, DetainDriver) and calculates consequences.
        /// </summary>
        public static CheckpointInspectionResult ProcessPlayerDecision(
            CheckpointVehicleCase vehicleCase,
            CheckpointDecision decision,
            SoldierProfile soldier = null)
        {
            if (vehicleCase == null) throw new ArgumentNullException(nameof(vehicleCase));

            var result = new CheckpointInspectionResult
            {
                decision = decision
            };

            switch (decision)
            {
                // =============================================================
                // DECISION 1: APPROVE & LET PASS
                // =============================================================
                case CheckpointDecision.ApprovePass:
                    if (!vehicleCase.HasAnyDiscrepancy)
                    {
                        // Clean vehicle, correct call!
                        result.success = true;
                        result.headline = "CLEAN PASSAGE APPROVED";
                        result.narrative = $"You reviewed {vehicleCase.civilianName}'s credentials and stamped their pass. " +
                                           "Traffic moved efficiently through the perimeter. Command commended your swift gate processing.";
                        result.disciplineDelta = +3f;
                        result.coFavorDelta = +2f;
                        result.squadRespectDelta = +1f;
                        result.promotionPointsDelta = +2;
                    }
                    else if (vehicleCase.cargoManifest != null && vehicleCase.cargoManifest.actualHiddenCargo == HiddenCargoType.Weapons)
                    {
                        // Lethal mistake: weapons allowed through!
                        result.success = false;
                        result.isInsurgentAttack = true;
                        result.headline = "CATASTROPHIC BREACH: WEAPONS SMUGGLED!";
                        result.narrative = "You waved the vehicle through without searching! Hours later, the smuggled automatic weapons and grenades " +
                                           "were detonated in an insurgent mortar strike on the forward fuel depot! The Provost Marshal opened a formal court-martial inquiry.";
                        result.disciplineDelta = -20f;
                        result.coFavorDelta = -25f;
                        result.squadRespectDelta = -15f;
                        result.promotionPointsDelta = -10;
                    }
                    else if (vehicleCase.isWantedInfiltrator)
                    {
                        // Hostile spy slipped past
                        result.success = false;
                        result.headline = "INTELLIGENCE FAILURE: ENEMY SPY LET THROUGH";
                        result.narrative = $"You stamped {vehicleCase.civilianName}'s papers without noticing the Imperial Watch Bulletin! " +
                                           "Counter-intelligence traced the hostile agent to a compromised garrison outpost. You received an official written reprimand.";
                        result.disciplineDelta = -15f;
                        result.coFavorDelta = -20f;
                        result.promotionPointsDelta = -8;
                    }
                    else if (vehicleCase.cargoManifest != null && vehicleCase.cargoManifest.actualHiddenCargo == HiddenCargoType.Refugees)
                    {
                        // Compassionate pass of refugees
                        result.success = true;
                        result.headline = "HUMANITARIAN ACT: REFUGEES SAFELY ESCORTED";
                        result.narrative = "A group of exhausted war refugees slipped into the safe district under your watch. " +
                                           "Though technically an unauthorized transit, local relief workers whispered heartfelt gratitude for your mercy.";
                        result.disciplineDelta = -2f;
                        result.squadRespectDelta = +5f;
                    }
                    else
                    {
                        // Other contraband or forged papers slipped past
                        result.success = false;
                        result.headline = "SECURITY BREACH: CONTRABAND SLIPPED THROUGH";
                        result.narrative = "Military Police at the inner perimeter intercepted the vehicle and found irregular documentation and contraband. " +
                                           "The Sergeant of the Guard docked your duty rating.";
                        result.disciplineDelta = -10f;
                        result.coFavorDelta = -12f;
                        result.promotionPointsDelta = -5;
                    }
                    break;

                // =============================================================
                // DECISION 2: SEARCH CARGO THOROUGHLY
                // =============================================================
                case CheckpointDecision.SearchCargo:
                    // Calculate discovery chance based on soldier attributes and MOS
                    float baseChance = 0.80f;
                    if (soldier != null)
                    {
                        baseChance += (soldier.tacticalIntellect - 50f) * 0.003f;
                        var mos = MOSRegistry.GetSoldierMOS(soldier);
                        var def = MOSRegistry.GetMOS(mos);
                        baseChance += def.checkpointSearchBonus * 0.01f; // Combat Engineer gets +35%, Recon +20%
                    }

                    bool contrabandDiscovered = (vehicleCase.HasContraband) && (UnityEngine.Random.value <= baseChance || baseChance >= 1.0f);

                    if (contrabandDiscovered)
                    {
                        result.success = true;
                        result.contrabandFound = true;
                        result.confiscatedItemDescription = vehicleCase.cargoManifest.hiddenCargoDescription;

                        long bounty = 200L;
                        int promoPoints = 8;

                        switch (vehicleCase.cargoManifest.actualHiddenCargo)
                        {
                            case HiddenCargoType.Weapons:
                                result.headline = "CONTRABAND CACHE SEIZED: WEAPONS SMUGGLER BUSTED!";
                                bounty = 450L;
                                promoPoints = 15;
                                break;
                            case HiddenCargoType.BlackMarketOptics:
                                result.headline = "MILITARY GEAR RECOVERED: NIGHT OPTICS SEIZED!";
                                bounty = 350L;
                                promoPoints = 12;
                                break;
                            case HiddenCargoType.ContrabandMoonshine:
                                result.headline = "ILLICIT ALCOHOL CONFISCATED";
                                bounty = 150L;
                                promoPoints = 6;
                                break;
                            case HiddenCargoType.FalseCompartment:
                                result.headline = "FALSE COMPARTMENT DETECTED & BREACHED";
                                bounty = 300L;
                                promoPoints = 10;
                                break;
                            case HiddenCargoType.Refugees:
                                result.headline = "CLANDESTINE REFUGEES DISCOVERED";
                                result.narrative = "Guards pulled aside canvas tarps to discover fleeing families. They were safely referred to the Red Cross field relief camp.";
                                bounty = 50L;
                                promoPoints = 5;
                                break;
                        }

                        if (vehicleCase.cargoManifest.actualHiddenCargo != HiddenCargoType.Refugees)
                        {
                            result.narrative = $"Your rigorous physical inspection stripped the cargo hold and exposed: {vehicleCase.cargoManifest.hiddenCargoDescription} " +
                                               $"The driver was handcuffed, and the Imperial Treasury awarded an official confiscation bounty of ${bounty:N0}!";
                        }

                        result.cashBountyDelta = bounty;
                        result.promotionPointsDelta = promoPoints;
                        result.disciplineDelta = +8f;
                        result.coFavorDelta = +12f;
                        result.squadRespectDelta = +6f;
                    }
                    else if (vehicleCase.HasContraband)
                    {
                        // Contraband present but missed by rushed inspection
                        result.success = false;
                        result.headline = "INSPECTION CONCLUDED: COMPARTMENT MISSED";
                        result.narrative = "Your guards tapped the tires and peered into crates but failed to find the cunningly masked compartment. " +
                                           "The driver offered a tense smile and hurried past.";
                        result.disciplineDelta = -3f;
                    }
                    else
                    {
                        // Clean vehicle thoroughly searched
                        result.success = true;
                        result.headline = "THOROUGH SEARCH COMPLETED: CARGO VERIFIED CLEAN";
                        result.narrative = "Every sack and crate was unsealed and matched the official manifest. " +
                                           "The driver complained about delays, but your sergeant commended your thorough adherence to garrison security protocols.";
                        result.disciplineDelta = +3f;
                        result.coFavorDelta = +2f;
                    }
                    break;

                // =============================================================
                // DECISION 3: DETAIN / ARREST DRIVER
                // =============================================================
                case CheckpointDecision.DetainDriver:
                    if (vehicleCase.isWantedInfiltrator)
                    {
                        // High-value catch!
                        result.success = true;
                        result.wantedAgentCaptured = true;
                        result.headline = "MAJOR INTELLIGENCE COUP: WANTED INFILTRATOR APPREHENDED!";
                        result.narrative = $"You flagged the driver's resemblance to {vehicleCase.wantedBulletinDescription}! " +
                                           "MP guards instantly subdued the operative, discovering encrypted enemy orders sewn into their uniform. Garrison Commander cited you for outstanding vigilance!";
                        result.promotionPointsDelta = +20;
                        result.cashBountyDelta = 600L;
                        result.disciplineDelta = +15f;
                        result.coFavorDelta = +25f;
                        result.squadRespectDelta = +15f;
                    }
                    else if (vehicleCase.isCounterfeitPassport || vehicleCase.HasContraband)
                    {
                        // Valid criminal arrest
                        result.success = true;
                        result.headline = "CRIMINAL APPREHENDED: ARREST CONFIRMED";
                        result.narrative = $"Driver was taken into custody for {(vehicleCase.isCounterfeitPassport ? "forged identification" : "smuggling unauthorized goods")}. " +
                                           "Provost Marshal took custody of the suspect and commended your vigilance.";
                        result.promotionPointsDelta = +12;
                        result.cashBountyDelta = 250L;
                        result.disciplineDelta = +10f;
                        result.coFavorDelta = +15f;
                        result.squadRespectDelta = +8f;
                    }
                    else
                    {
                        // Innocent civilian arrested! False arrest grievance
                        result.success = false;
                        result.headline = "FALSE ARREST: CIVILIAN GRIEVANCE LODGED";
                        result.narrative = $"The driver turned out to be an honest merchant whose credentials and cargo were 100% legitimate. " +
                                           "The district magistrate lodged a formal complaint against gate harassment, earning you a sharp reprimand from the Company Commander.";
                        result.disciplineDelta = -8f;
                        result.coFavorDelta = -12f;
                        result.squadRespectDelta = -5f;
                        result.promotionPointsDelta = -5;
                    }
                    break;
            }

            // Apply modifications to soldier profile if provided
            if (soldier != null)
            {
                ApplyOutcomeToSoldier(soldier, result);
            }

            return result;
        }

        private static void ApplyOutcomeToSoldier(SoldierProfile soldier, CheckpointInspectionResult result)
        {
            soldier.discipline = Mathf.Clamp(soldier.discipline + result.disciplineDelta, 0f, 100f);
            soldier.coFavor = Mathf.Clamp(soldier.coFavor + result.coFavorDelta, 0f, 100f);
            soldier.squadRespect = Mathf.Clamp(soldier.squadRespect + result.squadRespectDelta, 0f, 100f);
            soldier.accumulatedPromotionPoints = Mathf.Max(0, soldier.accumulatedPromotionPoints + result.promotionPointsDelta);
            soldier.cashBalance += result.cashBountyDelta;

            string colorTag = result.success ? "#00FF66" : "#FF4444";
            GameEvents.OnFeedEventLogged?.Invoke("SECURITY DUTY",
                $"<color={colorTag}><b>[CHECKPOINT INSPECTION • {result.headline}]</b></color> {result.narrative} " +
                $"Discipline: {(result.disciplineDelta >= 0 ? "+" : "")}{result.disciplineDelta:F0} | " +
                $"Promotion: {(result.promotionPointsDelta >= 0 ? "+" : "")}{result.promotionPointsDelta} pts | " +
                $"Bounty: +${result.cashBountyDelta:N0}.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }
    }
}
