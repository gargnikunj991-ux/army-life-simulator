using System;
using System.Collections.Generic;
using ArmyLife.Core;
using UnityEngine;

namespace ArmyLife.Data
{
    // =========================================================================
    // SECTION 1: PAPERS, PLEASE CHECKPOINT INSPECTION DATA STRUCTURES
    // =========================================================================

    public enum VehicleType
    {
        FarmCart,                   // Peasant mule cart with agricultural produce
        MilitarySupplyTruck,        // Standard 6x6 Imperial Logistics transport
        BlackMarketSedan,           // Sleek civilian luxury coupe with tinted glass
        RedCrossAmbulance,          // Field hospital van with medical emblems
        CivilianCourierVan,         // Parcel dispatch van for provincial delivery
        ArmoredPersonnelCarrier     // Re-routed armored patrol vehicle
    }

    public enum HiddenCargoType
    {
        None,                       // Clean cargo, matches manifest
        Weapons,                    // Clandestine rifles, machine carbines & stick grenades
        ContrabandMoonshine,        // 120-proof illicit barracks alcohol & narcotics
        BlackMarketOptics,          // Stolen imperial sniper scopes & night prism optics
        FalseCompartment,           // Hidden double-floor smuggling compartment
        Refugees                    // Displaced non-combatant civilians fleeing combat zone
    }

    public enum CheckpointDecision
    {
        ApprovePass,                // Stamp pass & open gate
        SearchCargo,                // Order thorough search of vehicle cargo bed
        DetainDriver                // Signal MP guard detail to arrest driver
    }

    [Serializable]
    public class TransitPassport
    {
        public string civilianName;
        public string photoId;
        public string issuedDistrict;
        public string expirationDate;
        public bool isExpired;
        public bool officialSealValid;
        public string travelPurpose;
        public string signature;

        public TransitPassport() { }

        public TransitPassport(string name, string id, string district, string expiry, bool expired, bool sealValid, string purpose, string sig)
        {
            civilianName = name;
            photoId = id;
            issuedDistrict = district;
            expirationDate = expiry;
            isExpired = expired;
            officialSealValid = sealValid;
            travelPurpose = purpose;
            signature = sig;
        }
    }

    [Serializable]
    public class CargoManifest
    {
        public string declaredCargo;
        public float manifestWeightKg;
        public string destination;
        public HiddenCargoType actualHiddenCargo;
        public string hiddenCargoDescription;

        public CargoManifest() { }

        public CargoManifest(string declared, float weight, string dest, HiddenCargoType actual, string hiddenDesc)
        {
            declaredCargo = declared;
            manifestWeightKg = weight;
            destination = dest;
            actualHiddenCargo = actual;
            hiddenCargoDescription = hiddenDesc;
        }
    }

    [Serializable]
    public class CheckpointVehicleCase
    {
        public string caseId;
        public string civilianName;
        public VehicleType vehicleType;
        public string licensePlate;
        public TransitPassport transitPassport;
        public CargoManifest cargoManifest;
        public bool isCounterfeitPassport;
        public bool isWantedInfiltrator;
        public string wantedBulletinDescription;
        public string vehicleVisualNotes;

        public bool HasAnyDiscrepancy =>
            isCounterfeitPassport ||
            isWantedInfiltrator ||
            (transitPassport != null && (transitPassport.isExpired || !transitPassport.officialSealValid)) ||
            (cargoManifest != null && cargoManifest.actualHiddenCargo != HiddenCargoType.None);

        public bool HasContraband => cargoManifest != null && cargoManifest.actualHiddenCargo != HiddenCargoType.None;
    }

    [Serializable]
    public class CheckpointInspectionResult
    {
        public CheckpointDecision decision;
        public bool success;
        public string headline;
        public string narrative;
        public bool contrabandFound;
        public bool wantedAgentCaptured;
        public bool isInsurgentAttack;
        public int promotionPointsDelta;
        public long cashBountyDelta;
        public float disciplineDelta;
        public float coFavorDelta;
        public float squadRespectDelta;
        public string confiscatedItemDescription;
    }

    // =========================================================================
    // SECTION 2: FIRING RANGE MARKSMANSHIP QUALIFICATION DATA STRUCTURES
    // =========================================================================

    public enum TargetType
    {
        HostileSilhouette,      // Standard pop-up enemy silhouette (200-300m)
        HostileOfficer,         // High-value officer target with peaked cap
        FriendlyCivilian,       // Non-combatant civilian holding packages (NO-SHOOT)
        FriendlyHostage         // Friendly prisoner in front of hostile (NO-SHOOT / PRECISION)
    }

    public enum QualificationTier
    {
        Unqualified,            // Score < 50% - Remedial training ordered
        Marksman,               // Score 50% - 74% - Standard marksman qualification
        Sharpshooter,           // Score 75% - 89% - Superior marksmanship badge
        Expert                  // Score 90% - 100% - Expert Infantryman Marksmanship Badge
    }

    [Serializable]
    public class TargetPopup
    {
        public int targetId;
        public float distanceMeters;        // 50m, 100m, 200m, 300m
        public float popupWindowSeconds;    // Time target stays exposed
        public TargetType targetType;
        public int pointValue;              // Points for clean hit (negative for friendly)
        public string description;

        public bool IsHostile => targetType == TargetType.HostileSilhouette || targetType == TargetType.HostileOfficer;
        public bool IsFriendly => targetType == TargetType.FriendlyCivilian || targetType == TargetType.FriendlyHostage;

        public TargetPopup() { }

        public TargetPopup(int id, float distance, float windowSeconds, TargetType type, int points, string desc)
        {
            targetId = id;
            distanceMeters = distance;
            popupWindowSeconds = windowSeconds;
            targetType = type;
            pointValue = points;
            description = desc;
        }
    }

    [Serializable]
    public class FiringTrialResult
    {
        public TargetPopup target;
        public bool targetHit;
        public bool heldFire;
        public float reactionTimeSeconds;
        public bool wasFriendlyFiredUpon;
        public int pointsAwarded;
        public string feedback;
    }

    [Serializable]
    public class FiringRangeQualificationResult
    {
        public int totalScore;
        public int maxPossibleScore;
        public float scorePercentage;
        public QualificationTier tier;
        public string badgeAwarded;
        public long cashPrizeBonus;
        public int promotionPointsAwarded;
        public float marksmanshipScoreGained;
        public List<FiringTrialResult> trials = new List<FiringTrialResult>();
        public string certificateSummary;
    }

    // =========================================================================
    // SECTION 3: BARRACKS DICE & CRAPS GAMBLING DATA STRUCTURES
    // =========================================================================

    [Serializable]
    public class DiceRollGame
    {
        public long wagerAmount;
        public int playerDie1;
        public int playerDie2;
        public int playerSum => playerDie1 + playerDie2;

        public int opponentDie1;
        public int opponentDie2;
        public int opponentSum => opponentDie1 + opponentDie2;

        public float winMultiplier;
        public string opponentName;
        public string opponentNickname;
    }

    [Serializable]
    public class DiceGameResult
    {
        public long wagerAmount;
        public int playerDie1;
        public int playerDie2;
        public int playerSum;
        public int opponentDie1;
        public int opponentDie2;
        public int opponentSum;
        public long payout;
        public long netProfit;
        public bool won;
        public bool isTie;
        public bool isNatural7Or11;
        public bool isJackpotBoxcarsOrSnakeEyes;
        public string headline;
        public string narrative;
        public float happinessDelta;
        public float squadmateLoyaltyDelta;
    }

    // =========================================================================
    // SECTION 4: MOS CAREER SPECIALIZATIONS ENGINE DATA
    // =========================================================================

    public enum MOSSpecialization
    {
        LineInfantry,           // Standard frontline rifleman (Assault, weapon discipline, suppressive fire)
        CombatMedic,            // Frontline emergency triage (Casualty stabilization, trauma kit efficiency)
        SniperRecon,            // Scout sniper (Marksmanship bonus, spotter instincts, extended range accuracy)
        CombatEngineer          // Demolition & fortifications (Mine clearing, obstacle breach, inspection bonus)
    }

    [Serializable]
    public class MOSDefinition
    {
        public MOSSpecialization specialization;
        public string mosCode;              // e.g. "11B", "68W", "19D", "12B"
        public string title;                // e.g. "Infantry Rifleman", "Combat Medic Specialist"
        public string description;
        public string primaryAttribute;
        public float marksmanshipBonus;     // Bonus to range accuracy
        public float triageBonus;           // Bonus to saving squadmates
        public float demolitionBonus;       // Bonus in assault / obstacle breaches
        public float checkpointSearchBonus; // Bonus to detecting hidden compartments at checkpoints
        public long monthlySalaryBonus;
        public string specialtyPerkName;
        public string specialtyPerkDescription;

        public MOSDefinition(
            MOSSpecialization spec,
            string code,
            string titleName,
            string desc,
            string primaryAttr,
            float marksBonus,
            float triBonus,
            float demoBonus,
            float searchBonus,
            long salaryBonus,
            string perkName,
            string perkDesc)
        {
            specialization = spec;
            mosCode = code;
            title = titleName;
            description = desc;
            primaryAttribute = primaryAttr;
            marksmanshipBonus = marksBonus;
            triageBonus = triBonus;
            demolitionBonus = demoBonus;
            checkpointSearchBonus = searchBonus;
            monthlySalaryBonus = salaryBonus;
            specialtyPerkName = perkName;
            specialtyPerkDescription = perkDesc;
        }
    }

    public static class MOSRegistry
    {
        private static readonly Dictionary<MOSSpecialization, MOSDefinition> registry =
            new Dictionary<MOSSpecialization, MOSDefinition>
            {
                {
                    MOSSpecialization.LineInfantry,
                    new MOSDefinition(
                        MOSSpecialization.LineInfantry,
                        "11B",
                        "Line Infantry Specialist",
                        "The backbone of the Imperial Army. Master of small arms, trench clearing, and disciplined volley fire under direct artillery bombardment.",
                        "Combat Readiness & Grit",
                        marksBonus: 5f,
                        triBonus: 0f,
                        demoBonus: 10f,
                        searchBonus: 5f,
                        salaryBonus: 100L,
                        "Fix Bayonets",
                        "+15% bonus morale and shock resistance during frontline assaults."
                    )
                },
                {
                    MOSSpecialization.CombatMedic,
                    new MOSDefinition(
                        MOSSpecialization.CombatMedic,
                        "68W",
                        "Combat Field Medic",
                        "Emergency frontline caregiver trained to administer tourniquets, plasma, and morphine amidst gunfire. Saves wounded comrades from fatal trauma.",
                        "Triage & Composure",
                        marksBonus: 0f,
                        triBonus: 35f,
                        demoBonus: 0f,
                        searchBonus: 15f,
                        salaryBonus: 250L,
                        "Angel in the Shrapnel",
                        "+50% chance to save critically injured squadmates; +30 loyalty boost from bandaging."
                    )
                },
                {
                    MOSSpecialization.SniperRecon,
                    new MOSDefinition(
                        MOSSpecialization.SniperRecon,
                        "19D",
                        "Sniper / Reconnaissance Scout",
                        "Concealed scout trained in long-range precision marksmanship, perimeter surveillance, and spotter identification of hostile command elements.",
                        "Precision Marksmanship & Vigilance",
                        marksBonus: 25f,
                        triBonus: 0f,
                        demoBonus: 5f,
                        searchBonus: 20f,
                        salaryBonus: 300L,
                        "Deadeye Eagle",
                        "+25% marksmanship bonus on the firing range and eliminates long-range target penalties."
                    )
                },
                {
                    MOSSpecialization.CombatEngineer,
                    new MOSDefinition(
                        MOSSpecialization.CombatEngineer,
                        "12B",
                        "Combat Engineer & Sapper",
                        "Demolition expert tasked with laying minefields, breaching reinforced concrete bunkers, and detecting concealed vehicle compartments.",
                        "Tactical Intellect & Explosives",
                        marksBonus: 0f,
                        triBonus: 5f,
                        demoBonus: 35f,
                        searchBonus: 35f,
                        salaryBonus: 280L,
                        "Sapper Breach Instinct",
                        "+35% detection bonus when searching false compartments at security checkpoints."
                    )
                }
            };

        public static MOSDefinition GetMOS(MOSSpecialization spec)
        {
            if (registry.TryGetValue(spec, out var def))
            {
                return def;
            }
            return registry[MOSSpecialization.LineInfantry];
        }

        public static List<MOSDefinition> GetAllMOS()
        {
            return new List<MOSDefinition>(registry.Values);
        }

        public static MOSSpecialization GetSoldierMOS(SoldierProfile soldier)
        {
            if (soldier == null) return MOSSpecialization.LineInfantry;

            if (soldier.HasTrait("MOS: Combat Field Medic") || soldier.assignedDepartment == "Combat Medic")
                return MOSSpecialization.CombatMedic;
            if (soldier.HasTrait("MOS: Sniper / Reconnaissance Scout") || soldier.assignedDepartment == "Sniper Recon")
                return MOSSpecialization.SniperRecon;
            if (soldier.HasTrait("MOS: Combat Engineer & Sapper") || soldier.assignedDepartment == "Combat Engineer")
                return MOSSpecialization.CombatEngineer;

            return MOSSpecialization.LineInfantry;
        }

        public static void AssignMOS(SoldierProfile soldier, MOSSpecialization mos)
        {
            if (soldier == null) return;

            var def = GetMOS(mos);
            soldier.assignedDepartment = def.title;

            // Remove other MOS traits to maintain clean state
            if (soldier.characterTraits != null)
            {
                soldier.characterTraits.RemoveAll(t => t.StartsWith("MOS: "));
            }

            soldier.AddTrait($"MOS: {def.title}");
            soldier.AddTrait(def.specialtyPerkName);

            GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                $"<color=#00D4FF><b>[MOS SPECIALIZATION ASSIGNED]</b></color> You completed specialized schooling and were assigned MOS <b>{def.mosCode} — {def.title}</b>! Perk unlocked: <b>{def.specialtyPerkName}</b>.");
        }
    }

    // =========================================================================
    // SECTION 5: BARRACKS CRAPS ENGINE
    // =========================================================================

    public static class BarracksDiceEngine
    {
        private static readonly System.Random rng = new System.Random();

        /// <summary>
        /// Executes a round of barracks street craps / 2d6 dice gambling.
        /// </summary>
        public static DiceGameResult PlayDiceRoll(
            SoldierProfile soldier,
            long wagerAmount,
            string squadmateName = "Deadeye",
            int fixedPlayerD1 = 0,
            int fixedPlayerD2 = 0,
            int fixedOpponentD1 = 0,
            int fixedOpponentD2 = 0)
        {
            if (wagerAmount <= 0) wagerAmount = 20;

            // Roll dice (or use fixed values for deterministic testing)
            int p1 = (fixedPlayerD1 >= 1 && fixedPlayerD1 <= 6) ? fixedPlayerD1 : rng.Next(1, 7);
            int p2 = (fixedPlayerD2 >= 1 && fixedPlayerD2 <= 6) ? fixedPlayerD2 : rng.Next(1, 7);
            int pSum = p1 + p2;

            int o1 = (fixedOpponentD1 >= 1 && fixedOpponentD1 <= 6) ? fixedOpponentD1 : rng.Next(1, 7);
            int o2 = (fixedOpponentD2 >= 1 && fixedOpponentD2 <= 6) ? fixedOpponentD2 : rng.Next(1, 7);
            int oSum = o1 + o2;

            var result = new DiceGameResult
            {
                wagerAmount = wagerAmount,
                playerDie1 = p1,
                playerDie2 = p2,
                playerSum = pSum,
                opponentDie1 = o1,
                opponentDie2 = o2,
                opponentSum = oSum
            };

            // Rule 1: Natural 7 or 11 -> Instant Win (1.5x profit)
            if (pSum == 7 || pSum == 11)
            {
                result.won = true;
                result.isNatural7Or11 = true;
                result.netProfit = (long)(wagerAmount * 1.5f);
                result.payout = wagerAmount + result.netProfit;
                result.headline = pSum == 7 ? "NATURAL SEVEN! INSTANT WIN!" : "NATURAL ELEVEN! INSTANT WIN!";
                result.narrative = $"You rolled a ({p1}, {p2}) totaling {pSum}! The barracks crowd roars as {squadmateName} tosses over the pot.";
                result.happinessDelta = 10f;
                result.squadmateLoyaltyDelta = 5f;
            }
            // Rule 2: Snake Eyes (1,1) or Boxcars (6,6) -> High Roller Critical Jackpot (2.5x profit)
            else if (pSum == 2 || pSum == 12)
            {
                result.won = true;
                result.isJackpotBoxcarsOrSnakeEyes = true;
                result.netProfit = (long)(wagerAmount * 2.5f);
                result.payout = wagerAmount + result.netProfit;
                result.headline = pSum == 12 ? "BOXCARS (12)! HIGH-ROLLER JACKPOT!" : "SNAKE EYES (2)! LUCKY DOUBLE ROLL!";
                result.narrative = $"Incredible roll! Double {(pSum == 12 ? "sixes" : "ones")} struck the wooden footlocker! You collect a massive 2.5x pot payout.";
                result.happinessDelta = 15f;
                result.squadmateLoyaltyDelta = 8f;
            }
            // Rule 3: High roll showdown vs Opponent
            else if (pSum > oSum)
            {
                result.won = true;
                result.netProfit = wagerAmount;
                result.payout = wagerAmount * 2;
                result.headline = "VICTORY! HIGHER ROLL!";
                result.narrative = $"Your total of {pSum} beats {squadmateName}'s {oSum}. You sweep the coins off the footlocker.";
                result.happinessDelta = 8f;
                result.squadmateLoyaltyDelta = 4f;
            }
            else if (pSum == oSum)
            {
                result.won = false;
                result.isTie = true;
                result.netProfit = 0;
                result.payout = wagerAmount; // Wager returned
                result.headline = "STALEMATE ROLL (PUSH)";
                result.narrative = $"Both you and {squadmateName} rolled a total of {pSum}! The wager is pushed back to the center.";
                result.happinessDelta = 2f;
                result.squadmateLoyaltyDelta = 2f;
            }
            else
            {
                result.won = false;
                result.netProfit = -wagerAmount;
                result.payout = 0;
                result.headline = "DEFEAT! LOW ROLL";
                result.narrative = $"Your {pSum} falls short of {squadmateName}'s {oSum}. {squadmateName} chuckles and rakes in your ${wagerAmount:N0} bet.";
                result.happinessDelta = -5f;
                result.squadmateLoyaltyDelta = 3f; // Still bonded over gambling
            }

            // Apply to SoldierProfile if provided
            if (soldier != null)
            {
                soldier.cashBalance += result.netProfit;
                soldier.happiness = Mathf.Clamp(soldier.happiness + result.happinessDelta, 0f, 100f);

                // Update squadmate relationship if found
                if (soldier.squadMates != null && soldier.squadMates.Count > 0)
                {
                    var mate = soldier.squadMates.Find(m => m.nickname == squadmateName || (m.name != null && m.name.Contains(squadmateName)));
                    if (mate != null)
                    {
                        mate.relationship = Mathf.Clamp(mate.relationship + result.squadmateLoyaltyDelta, 0f, 100f);
                    }
                }

                GameEvents.OnFeedEventLogged?.Invoke("GAMBLING",
                    $"<color=#FFD700><b>[BARRACKS DICE • {result.headline}]</b></color> Wager: ${wagerAmount:N0} vs {squadmateName}. " +
                    $"You rolled <b>{pSum}</b> ({p1}, {p2}) vs Opponent <b>{oSum}</b> ({o1}, {o2}). Net: <b>{(result.netProfit >= 0 ? "+" : "")}${result.netProfit:N0}</b>.");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            }

            return result;
        }
    }
}
