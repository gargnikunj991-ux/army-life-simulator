using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public enum TacticalMissionType
    {
        TrenchBreachAssault,              // Breaching enemy trench parapets, cutting razor wire, clearing bunkers
        HighValueReconPatrol,             // Long-range stealth recon behind enemy lines mapping hostile positions
        ForwardArtilleryObservation,      // Directing devastating high-explosive battery fire from an exposed OP
        CasualtyEvacuation,               // Under-Fire CASEVAC extracting dying soldiers from no-man's land
        ConvoyOverwatchAndDefense,        // Escorting armored supply lines and defending against coordinated ambushes
        NightSabotageRaid                 // Infiltrating enemy command hubs or munitions dumps under cover of dark
    }

    public enum TacticalOutcomeType
    {
        DecisiveVictory,                  // Flawless operational triumph, enemy redoubt annihilated
        TacticalSuccess,                  // Primary objective secured with acceptable friendly friction
        PyrrhicVictory,                   // Objective taken at high cost of wounds and physical exhaustion
        TacticalWithdrawal,               // Orderly fighting retreat preserving unit cohesion
        CatastrophicAmbush                // Trapped in a prepared kill-zone, heavy wounds and chaos
    }

    public enum TacticalChoiceOrder
    {
        DefaultOrder,
        AggressiveBreach,
        ReconnaissanceFlank,
        HeavySuppression,
        EmergencyCASEVAC,
        ArtilleryStrikeCall,
        OrderlyWithdrawal
    }

    [Serializable]
    public class TacticalMission
    {
        public TacticalMissionType missionType;
        public string missionName;
        public string objectiveDescription;
        public WarTheaterInfo theater;
        public int missionDifficulty;             // 1 (Low) to 5 (Lethal)
        public float baseEnemyThreat;             // 40f to 95f
        public SquadRole synergyRole;             // Crucial specialist role
        public string recommendedDoctrine;        // Doctrine that aligns with mission
        public long baseHazardBounty;             // Imperial hazard cash bonus

        public TacticalMission() {}

        public TacticalMission(
            TacticalMissionType type, 
            string name, 
            string desc, 
            WarTheaterInfo theaterInfo, 
            int difficulty, 
            float enemyThreat, 
            SquadRole role, 
            string doctrine, 
            long bounty)
        {
            this.missionType = type;
            this.missionName = name;
            this.objectiveDescription = desc;
            this.theater = theaterInfo;
            this.missionDifficulty = difficulty;
            this.baseEnemyThreat = enemyThreat;
            this.synergyRole = role;
            this.recommendedDoctrine = doctrine;
            this.baseHazardBounty = bounty;
        }
    }

    [Serializable]
    public class CombatOperationOutcome
    {
        public TacticalOutcomeType outcomeType;
        public TacticalMission mission;
        public bool isSuccess;                    // True for DecisiveVictory, TacticalSuccess, PyrrhicVictory
        public string headline;
        public string reportSummary;
        public int casualties;
        public float playerDamageTaken;
        public bool playerWounded;
        public List<string> combatHonors = new List<string>();
        public long cashHazardBounty;
        public long theaterHazardPay;
        public long totalCashAwarded => cashHazardBounty + theaterHazardPay;
        public int promotionPoints;
        public float meritDelta;
        public float combatSkillDelta;
        public float squadMoraleDelta;
        public float squadFatigueDelta;
        public List<string> squadmateReports = new List<string>();
        public string roleSynergySummary;

        public CombatOperationOutcome()
        {
            combatHonors = new List<string>();
            squadmateReports = new List<string>();
        }
    }

    /// <summary>
    /// Milestone: Tactical Combat Operations & Squad Deployment Engine.
    /// Manages semiannual deployment missions, tactical choices, squad role synergies
    /// (Scout recon, Gunner suppression, Medic triage, Leader doctrine),
    /// outcome resolution, combat honors (Silver Cross, Bronze Star, Wound Star, Valor Citation),
    /// hazard pay bounties, and squadmate morale/fatigue/injury tracking.
    /// </summary>
    public static class CombatOperationsEngine
    {
        #region Mission Generation

        /// <summary>
        /// Generates an operational combat mission tailored to the active war theater and squad standing.
        /// </summary>
        public static TacticalMission GenerateMission(
            WarTheaterInfo theater = null, 
            SoldierProfile soldier = null, 
            TacticalMissionType? forcedMissionType = null)
        {
            if (theater == null)
            {
                var theaterType = soldier != null ? soldier.currentTheater : WarTheaterType.ArdenRidgeTrenchLines;
                theater = WarTheaterInfo.GetInfo(theaterType);
            }

            TacticalMissionType chosenType;
            if (forcedMissionType.HasValue)
            {
                chosenType = forcedMissionType.Value;
            }
            else
            {
                // Select realistic mission weighted by theater environment
                switch (theater.theaterType)
                {
                    case WarTheaterType.ArdenRidgeTrenchLines:
                    case WarTheaterType.WesternMudTrenches:
                        chosenType = (UnityEngine.Random.value < 0.50f) 
                            ? TacticalMissionType.TrenchBreachAssault 
                            : (UnityEngine.Random.value < 0.50f ? TacticalMissionType.ForwardArtilleryObservation : TacticalMissionType.CasualtyEvacuation);
                        break;

                    case WarTheaterType.PortIronAmphibiousFront:
                    case WarTheaterType.PacificJungleIslands:
                        chosenType = (UnityEngine.Random.value < 0.45f) 
                            ? TacticalMissionType.TrenchBreachAssault 
                            : (UnityEngine.Random.value < 0.50f ? TacticalMissionType.CasualtyEvacuation : TacticalMissionType.NightSabotageRaid);
                        break;

                    case WarTheaterType.KaelenValleyUrbanZone:
                    case WarTheaterType.UrbanCitadel:
                        chosenType = (UnityEngine.Random.value < 0.40f) 
                            ? TacticalMissionType.NightSabotageRaid 
                            : (UnityEngine.Random.value < 0.50f ? TacticalMissionType.ConvoyOverwatchAndDefense : TacticalMissionType.HighValueReconPatrol);
                        break;

                    case WarTheaterType.BadlandsSteppeRecon:
                        chosenType = (UnityEngine.Random.value < 0.50f) 
                            ? TacticalMissionType.HighValueReconPatrol 
                            : (UnityEngine.Random.value < 0.50f ? TacticalMissionType.ConvoyOverwatchAndDefense : TacticalMissionType.ForwardArtilleryObservation);
                        break;

                    case WarTheaterType.NorthernTaigaOutpost:
                    case WarTheaterType.EasternFrozenSteppe:
                    default:
                        chosenType = (UnityEngine.Random.value < 0.40f) 
                            ? TacticalMissionType.HighValueReconPatrol 
                            : (UnityEngine.Random.value < 0.50f ? TacticalMissionType.ForwardArtilleryObservation : TacticalMissionType.CasualtyEvacuation);
                        break;
                }
            }

            return BuildMissionData(chosenType, theater);
        }

        private static TacticalMission BuildMissionData(TacticalMissionType type, WarTheaterInfo theater)
        {
            string name;
            string desc;
            SquadRole role;
            string doctrine;
            int diff = theater.dangerLevel;
            float threat = 50f + (diff * 8f);
            long bounty = 450L + (diff * 50L);

            switch (type)
            {
                case TacticalMissionType.TrenchBreachAssault:
                    name = "Trench Breach & Redoubt Assault";
                    desc = $"Assault fortified hostile parapets in {theater.theaterName}, neutralise machine-gun revetments, and breach barbed wire defenses.";
                    role = SquadRole.HeavyGunner;
                    doctrine = "Trench Breach Assault";
                    bounty += 100L;
                    break;

                case TacticalMissionType.HighValueReconPatrol:
                    name = "High-Value Reconnaissance Patrol";
                    desc = $"Infiltrate hostile forward screen in {theater.theaterName} to map troop concentrations, fuel dumps, and artillery redoubts.";
                    role = SquadRole.LeadScout;
                    doctrine = "High-Value Recon Patrol";
                    break;

                case TacticalMissionType.ForwardArtilleryObservation:
                    name = "Forward Artillery Observation Post (OP)";
                    desc = $"Maintain an exposed forward OP under direct fire in {theater.theaterName} to direct divisional artillery batteries onto enemy armor.";
                    role = SquadRole.HeavyGunner;
                    doctrine = "Forward Artillery Observation";
                    bounty += 50L;
                    break;

                case TacticalMissionType.CasualtyEvacuation:
                    name = "Under-Fire CASEVAC & Extraction";
                    desc = $"Sustain heavy incoming mortar fire while extracting pinned-down casualties from no-man's land in {theater.theaterName}.";
                    role = SquadRole.CombatMedic;
                    doctrine = "Under-Fire CASEVAC";
                    bounty += 120L;
                    break;

                case TacticalMissionType.ConvoyOverwatchAndDefense:
                    name = "Convoy Overwatch & Ambush Defense";
                    desc = $"Provide tactical security for armored ammunition convoys through hostile defiles in {theater.theaterName}, repelling coordinated ambushes.";
                    role = SquadRole.HeavyGunner;
                    doctrine = "Convoy Overwatch & Ambush Defense";
                    break;

                case TacticalMissionType.NightSabotageRaid:
                default:
                    name = "Night Infiltration Sabotage Raid";
                    desc = $"Infiltrate behind hostile wire under darkness in {theater.theaterName} to detonate satchel charges against enemy communications towers.";
                    role = SquadRole.LeadScout;
                    doctrine = "Night Sabotage Raid";
                    bounty += 150L;
                    break;
            }

            return new TacticalMission(type, name, desc, theater, diff, threat, role, doctrine, bounty);
        }

        #endregion

        #region Tactical Execution & Squad Synergy Resolution

        /// <summary>
        /// Executes a tactical combat operation with squadmate synergies, doctrine matching,
        /// combat honors, cash hazard bounties, and squad morale/fatigue updates.
        /// </summary>
        public static CombatOperationOutcome ExecuteMission(
            SoldierProfile soldier, 
            TacticalMission mission, 
            TacticalChoiceOrder orderChoice = TacticalChoiceOrder.DefaultOrder, 
            WarTheaterInfo theater = null, 
            TacticalOutcomeType? forceOutcome = null, 
            float? forceRoll = null)
        {
            if (soldier == null)
            {
                var err = new CombatOperationOutcome();
                err.headline = "No Commander Assigned";
                err.reportSummary = "Operation aborted: No soldier profile available.";
                return err;
            }

            soldier.InitializeSquadMatesIfEmpty();

            if (theater == null)
            {
                theater = mission?.theater ?? WarTheaterInfo.GetInfo(soldier.currentTheater);
            }
            if (mission == null)
            {
                mission = GenerateMission(theater, soldier);
            }

            var outcome = new CombatOperationOutcome();
            outcome.mission = mission;
            outcome.theaterHazardPay = theater.hazardPayRate;

            // 1. Identify Squadmates & Roles
            var scout = soldier.squadMates.Find(m => m.role == SquadRole.LeadScout && !m.isDead);
            var gunner = soldier.squadMates.Find(m => m.role == SquadRole.HeavyGunner && !m.isDead);
            var medic = soldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);
            var rookie = soldier.squadMates.Find(m => m.role == SquadRole.RiflemanRookie && !m.isDead);

            bool scoutAlert = (scout != null && scout.relationship >= 40f && scout.fatigue < 80f);
            bool gunnerReady = (gunner != null && gunner.relationship >= 40f && gunner.fatigue < 80f);
            bool medicReady = (medic != null && medic.relationship >= 40f && medic.fatigue < 80f);

            // 2. Compute Base Tactical Power & Leader Standing
            float leaderPower = (soldier.combatSkill * 0.25f) + (soldier.intellect * 0.20f) + (soldier.fitness * 0.15f);
            if (SquadmateCommandEngine.IsNCOOrOfficer(soldier))
            {
                leaderPower += 10f; // Command authority bonus
            }

            // Gear protective & tactical perks
            if (soldier.hasSteelHelmet) leaderPower += 4f;
            if (soldier.hasBallisticVest) leaderPower += 6f;
            if (soldier.hasServiceSidearm) leaderPower += 5f;
            if (soldier.hasLuckyWatch) leaderPower += 5f;

            // 3. Squad Role Synergies
            float synergyBonus = 0f;
            List<string> synergyNotes = new List<string>();

            // Special Forces Operator Elite Tactical Bonus & Morale Inspiration
            if (soldier.isSpecialForces)
            {
                leaderPower += 20f; // Elite commando lethality
                synergyNotes.Add("<color=#FFD700><b>Special Forces Operator Aura:</b> Your lethal precision and elite training inspire unwavering courage in the ranks! (+20 Tactical Power)</color>");
            }

            // Military Occupational Specialty (MOS) Perks
            switch (soldier.militaryMOS)
            {
                case MilitaryMOS.InfantryRifleman11B:
                    leaderPower += 12f;
                    synergyNotes.Add("<color=#00FFAA><b>11B Infantry Rifleman:</b> Direct aggressive shock-assault tactics and trench combat mastery (+12 Power).</color>");
                    break;
                case MilitaryMOS.CombatMedic68W:
                    leaderPower += 8f;
                    synergyNotes.Add("<color=#00FFAA><b>68W Combat Medic:</b> Rapid field hemorrhage control and squad survival trauma protocol (+8 Power).</color>");
                    break;
                case MilitaryMOS.ScoutSniper19D:
                    leaderPower += 15f;
                    synergyNotes.Add("<color=#00FFAA><b>19D Scout Sniper:</b> Long-range optic overwatch neutralizing enemy machine-gun nests (+15 Power).</color>");
                    break;
                case MilitaryMOS.CombatEngineer12B:
                    leaderPower += 14f;
                    synergyNotes.Add("<color=#00FFAA><b>12B Combat Engineer:</b> Explosive obstacle clearing and anti-IED route clearance (+14 Power).</color>");
                    break;
                case MilitaryMOS.FighterPilotAce:
                    leaderPower += 22f;
                    synergyNotes.Add("<color=#00FFAA><b>Fighter Ace:</b> Precision close air support (CAS) strikes decimating enemy fortified lines (+22 Power).</color>");
                    break;
                case MilitaryMOS.PararescuePJ:
                    leaderPower += 18f;
                    synergyNotes.Add("<color=#00FFAA><b>Pararescue PJ:</b> Elite combat search and rescue (CSAR) extracting wounded under heavy fire (+18 Power).</color>");
                    break;
            }

            // Scout Synergy
            if (scoutAlert)
            {
                if (mission.missionType == TacticalMissionType.HighValueReconPatrol || mission.missionType == TacticalMissionType.NightSabotageRaid)
                {
                    synergyBonus += 25f;
                    synergyNotes.Add($"Lead Scout {scout.nickname} navigated undetected corridors and reconnoitered enemy tripwires (+25 Synergy)");
                }
                else if (mission.missionType == TacticalMissionType.ConvoyOverwatchAndDefense)
                {
                    synergyBonus += 20f;
                    synergyNotes.Add($"Lead Scout {scout.nickname} identified concealed roadside IED markers early (+20 Synergy)");
                }
                else
                {
                    synergyBonus += 10f;
                    synergyNotes.Add($"Lead Scout {scout.nickname} maintained vigilant forward point (+10 Synergy)");
                }
            }

            // Gunner Synergy
            if (gunnerReady)
            {
                if (mission.missionType == TacticalMissionType.TrenchBreachAssault || mission.missionType == TacticalMissionType.ConvoyOverwatchAndDefense)
                {
                    synergyBonus += 25f;
                    synergyNotes.Add($"Heavy Gunner {gunner.nickname} unleashed relentless cyclic suppressive fire, locking down enemy bunkers (+25 Synergy)");
                }
                else if (mission.missionType == TacticalMissionType.ForwardArtilleryObservation)
                {
                    synergyBonus += 20f;
                    synergyNotes.Add($"Heavy Gunner {gunner.nickname} kept hostile counter-sniper parties pinned down (+20 Synergy)");
                }
                else
                {
                    synergyBonus += 10f;
                    synergyNotes.Add($"Heavy Gunner {gunner.nickname} maintained defensive crossfire arcs (+10 Synergy)");
                }
            }

            // Medic Synergy
            if (medicReady)
            {
                if (mission.missionType == TacticalMissionType.CasualtyEvacuation)
                {
                    synergyBonus += 30f;
                    synergyNotes.Add($"Combat Medic {medic.nickname} executed rapid combat trauma triage under direct mortar fire (+30 Synergy)");
                }
                else
                {
                    synergyBonus += 12f;
                    synergyNotes.Add($"Combat Medic {medic.nickname} was stationed on the frontline, bolstering fireteam morale (+12 Synergy)");
                }
            }

            // 4. Doctrine Match Bonus
            float doctrineBonus = 0f;
            if (theater.IsRecommendedDoctrine(mission.recommendedDoctrine))
            {
                doctrineBonus += 12f;
                synergyNotes.Add($"Tactical plan leveraged theater-recommended doctrine '{mission.recommendedDoctrine}' (+12 Doctrine Alignment)");
            }

            // 5. Tactical Order Choice Impact
            float orderModifier = 0f;
            switch (orderChoice)
            {
                case TacticalChoiceOrder.AggressiveBreach:
                    orderModifier += gunnerReady ? 15f : -5f;
                    break;
                case TacticalChoiceOrder.ReconnaissanceFlank:
                    orderModifier += scoutAlert ? 15f : -5f;
                    break;
                case TacticalChoiceOrder.HeavySuppression:
                    orderModifier += gunnerReady ? 15f : 0f;
                    break;
                case TacticalChoiceOrder.EmergencyCASEVAC:
                    orderModifier += medicReady ? 20f : -10f;
                    break;
                case TacticalChoiceOrder.ArtilleryStrikeCall:
                    orderModifier += (soldier.intellect >= 50f) ? 15f : 0f;
                    break;
                case TacticalChoiceOrder.OrderlyWithdrawal:
                    orderModifier += 10f; // Low risk, safe execution
                    break;
                case TacticalChoiceOrder.DefaultOrder:
                default:
                    orderModifier += 5f;
                    break;
            }

            // 6. Hazard & Enemy Threat Penalties
            float theaterPenalty = (theater.dangerLevel * 6f) + ((theater.casualtyRateModifier - 1.0f) * 20f);
            float baseScore = 50f + leaderPower + synergyBonus + doctrineBonus + orderModifier - (theaterPenalty * 0.8f);

            // Compute randomized score (or forced)
            float randomScore;
            if (forceRoll.HasValue)
            {
                randomScore = forceRoll.Value;
            }
            else
            {
                float variance = UnityEngine.Random.Range(-15f, 15f);
                randomScore = Mathf.Clamp(baseScore + variance, 5f, 98f);
            }

            // Determine Outcome Type
            TacticalOutcomeType outcomeType;
            if (forceOutcome.HasValue)
            {
                outcomeType = forceOutcome.Value;
            }
            else
            {
                if (randomScore >= 80f) outcomeType = TacticalOutcomeType.DecisiveVictory;
                else if (randomScore >= 55f) outcomeType = TacticalOutcomeType.TacticalSuccess;
                else if (randomScore >= 38f) outcomeType = TacticalOutcomeType.PyrrhicVictory;
                else if (randomScore >= 22f) outcomeType = TacticalOutcomeType.TacticalWithdrawal;
                else
                {
                    // If Scout alert, prevent Catastrophic Ambush and convert to fighting withdrawal!
                    if (scoutAlert)
                    {
                        outcomeType = TacticalOutcomeType.TacticalWithdrawal;
                        synergyNotes.Add($"[AMBUSH AVERTED] Lead Scout {scout.nickname} detected the hostile kill-zone 200m out, averting catastrophic ambush into an orderly withdrawal!");
                    }
                    else
                    {
                        outcomeType = TacticalOutcomeType.CatastrophicAmbush;
                    }
                }
            }

            outcome.outcomeType = outcomeType;
            outcome.roleSynergySummary = string.Join("; ", synergyNotes);

            // 7. Calculate Results, Combat Honors, Bounties & Casualties
            ResolveOutcomeDetails(soldier, mission, theater, outcome, scoutAlert, gunnerReady, medicReady, medic);

            // 8. Update Squadmate Welfare: Morale, Fatigue, Injuries
            UpdateSquadReadiness(soldier, outcome, scoutAlert, medicReady);

            // 9. Narrative Feed Dispatch
            DispatchFeedEvents(soldier, outcome, theater);

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return outcome;
        }

        private static void ResolveOutcomeDetails(
            SoldierProfile soldier, 
            TacticalMission mission, 
            WarTheaterInfo theater, 
            CombatOperationOutcome outcome, 
            bool scoutAlert, 
            bool gunnerReady, 
            bool medicReady, 
            SquadMate medic)
        {
            switch (outcome.outcomeType)
            {
                case TacticalOutcomeType.DecisiveVictory:
                    outcome.isSuccess = true;
                    outcome.headline = $"Decisive Victory: {mission.missionName}";
                    outcome.reportSummary = $"Hostile fortified line shattered in {theater.theaterName}! All operational objectives conquered with overwhelming fireteam synergy.";
                    outcome.cashHazardBounty = mission.baseHazardBounty * 2L;
                    outcome.promotionPoints = 20;
                    outcome.meritDelta = 22f;
                    outcome.combatSkillDelta = 4.5f;
                    outcome.squadMoraleDelta = 15f;
                    outcome.squadFatigueDelta = scoutAlert ? 10f : 15f;
                    outcome.casualties = 0;
                    outcome.playerDamageTaken = 0f;
                    break;

                case TacticalOutcomeType.TacticalSuccess:
                    outcome.isSuccess = true;
                    outcome.headline = $"Tactical Success: {mission.missionName}";
                    outcome.reportSummary = $"Primary mission objective secured in {theater.theaterName}. Hostile counter-attack repelled with minimal friction.";
                    outcome.cashHazardBounty = mission.baseHazardBounty;
                    outcome.promotionPoints = 12;
                    outcome.meritDelta = 14f;
                    outcome.combatSkillDelta = 3.0f;
                    outcome.squadMoraleDelta = 8f;
                    outcome.squadFatigueDelta = scoutAlert ? 14f : 20f;
                    outcome.casualties = gunnerReady ? 0 : 1;
                    outcome.playerDamageTaken = 5f;
                    break;

                case TacticalOutcomeType.PyrrhicVictory:
                    outcome.isSuccess = true;
                    outcome.headline = $"Pyrrhic Victory: Heavy Losses at {mission.missionName}";
                    outcome.reportSummary = $"Redoubt taken at severe physical cost in {theater.theaterName}. Fireteam pushed through relentless artillery barrages.";
                    outcome.cashHazardBounty = (long)(mission.baseHazardBounty * 0.60f);
                    outcome.promotionPoints = 6;
                    outcome.meritDelta = 6f;
                    outcome.combatSkillDelta = 2.5f;
                    outcome.squadMoraleDelta = -8f;
                    outcome.squadFatigueDelta = 30f;
                    outcome.casualties = medicReady ? 1 : 2;
                    outcome.playerDamageTaken = soldier.hasBallisticVest ? 18f : 28f;
                    outcome.playerWounded = true;
                    break;

                case TacticalOutcomeType.TacticalWithdrawal:
                    outcome.isSuccess = false;
                    outcome.headline = $"Tactical Withdrawal: {mission.missionName}";
                    outcome.reportSummary = $"Facing insurmountable odds in {theater.theaterName}, unit executed an orderly fighting retreat, preserving squad strength.";
                    outcome.cashHazardBounty = (long)(mission.baseHazardBounty * 0.35f);
                    outcome.promotionPoints = 3;
                    outcome.meritDelta = 2f;
                    outcome.combatSkillDelta = 1.5f;
                    outcome.squadMoraleDelta = -2f;
                    outcome.squadFatigueDelta = 16f;
                    outcome.casualties = 0;
                    outcome.playerDamageTaken = 6f;
                    break;

                case TacticalOutcomeType.CatastrophicAmbush:
                default:
                    outcome.isSuccess = false;
                    outcome.headline = $"Catastrophic Ambush in {theater.theaterName}";
                    outcome.reportSummary = $"Squad was enveloped in an interlocking crossfire corridor! Heavy shrapnel and confusion forced emergency evacuation.";
                    outcome.cashHazardBounty = 0L;
                    outcome.promotionPoints = 0;
                    outcome.meritDelta = -10f;
                    outcome.combatSkillDelta = 1.0f;
                    outcome.squadMoraleDelta = -20f;
                    outcome.squadFatigueDelta = 35f;
                    outcome.casualties = medicReady ? 2 : 4;
                    outcome.playerDamageTaken = soldier.hasBallisticVest ? 28f : 42f;
                    outcome.playerWounded = true;
                    break;
            }

            // Evaluate authentic Military Medal Hierarchy & strict valor criteria
            var evaluatedHonors = MedalRackEngine.EvaluateBattlefieldMedalAwards(soldier, outcome, mission, theater);
            foreach (var h in evaluatedHonors)
            {
                if (!outcome.combatHonors.Contains(h))
                {
                    outcome.combatHonors.Add(h);
                }
            }

            // Special Forces Operator Battlefield Aura:
            // Elite presence inspires squad courage, boosts merit, and protects squadmates from fatal casualties
            if (soldier.isSpecialForces)
            {
                outcome.squadMoraleDelta += 10f;
                outcome.combatSkillDelta += 1.5f;
                outcome.meritDelta += 5f;
                if (outcome.casualties > 0)
                {
                    outcome.casualties = Mathf.Max(0, outcome.casualties - 1);
                    outcome.squadmateReports.Add("<color=#FFD700><b>Special Forces Overwatch:</b> Your precision rifle overwatch suppressed an enemy heavy machine gun bunker, saving squadmates from fatal crossfire.</color>");
                }
            }

            // Apply player damage and health updates
            if (outcome.playerDamageTaken > 0f)
            {
                soldier.health = Mathf.Max(0f, soldier.health - outcome.playerDamageTaken);
                if (soldier.health <= 0f)
                {
                    // Medic emergency save
                    if (medicReady && medic != null)
                    {
                        soldier.health = 20f;
                        medic.relationship = Mathf.Min(100f, medic.relationship + 15f);
                        medic.battlesSurvived++;
                        outcome.squadmateReports.Add($"Combat Medic {medic.GetDisplayName()} dragged you from death, staunching arterial hemorrhage under fire!");
                    }
                    else if (soldier.fieldMedkitsCount > 0)
                    {
                        soldier.fieldMedkitsCount--;
                        soldier.health = 18f;
                        outcome.squadmateReports.Add("Applied emergency PX field trauma medkit, preventing fatal blood loss (-1 Medkit)!");
                    }
                    else
                    {
                        soldier.isDead = true;
                        soldier.deathReason = $"Killed in Action during {mission.missionName} in {theater.theaterName}";
                        soldier.AddMedal("Imperial Wound Star (Posthumous)");
                        outcome.combatHonors.Add("Imperial Wound Star (Posthumous)");
                    }
                }
                else if (!soldier.isDead && soldier.health < FieldHospitalEngine.CRITICAL_CASUALTY_THRESHOLD && !soldier.inMedicalConvalescence)
                {
                    FieldHospitalEngine.AdmitToHospital(soldier, $"Critical Combat Trauma during {mission.missionName}", isIEDAmputation: false);
                    outcome.squadmateReports.Add("<color=#FF5555><b>Forward Field Hospital Triage:</b> Severe combat trauma! Stretcher-bearers evacuated you to surgical care.</color>");
                }
            }

            // Record Territorial Frontline Momentum on the Campaign War Map
            DynamicWarFrontEngine.RecordBattleOutcome(theater.theaterType, outcome.outcomeType, soldier);

            // Apply financial rewards, merit, combat skill, and promotion points
            soldier.cashBalance += outcome.totalCashAwarded;
            soldier.accumulatedPromotionPoints += outcome.promotionPoints;
            soldier.meritRating = Mathf.Clamp(soldier.meritRating + outcome.meritDelta, 0f, 100f);
            soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + outcome.combatSkillDelta);
        }

        private static void UpdateSquadReadiness(
            SoldierProfile soldier, 
            CombatOperationOutcome outcome, 
            bool scoutAlert, 
            bool medicReady)
        {
            if (soldier.squadMates == null) return;

            // Scout route optimization reduces squad fatigue
            float actualFatigueDelta = outcome.squadFatigueDelta;
            if (scoutAlert)
            {
                actualFatigueDelta = Mathf.Max(5f, actualFatigueDelta - 6f);
            }

            foreach (var mate in soldier.squadMates)
            {
                if (mate.isDead) continue;

                mate.battlesSurvived++;
                mate.morale = Mathf.Clamp(mate.morale + outcome.squadMoraleDelta, 0f, 100f);
                mate.fatigue = Mathf.Clamp(mate.fatigue + actualFatigueDelta, 0f, 100f);

                // Relationship changes
                if (outcome.isSuccess)
                {
                    mate.relationship = Mathf.Min(100f, mate.relationship + UnityEngine.Random.Range(2f, 6f));
                }
                else if (outcome.outcomeType == TacticalOutcomeType.CatastrophicAmbush)
                {
                    mate.relationship = Mathf.Max(0f, mate.relationship - 3f);
                }

                outcome.squadmateReports.Add($"{mate.GetDisplayName()} ({mate.roleTitle}): Morale {mate.morale:F0}%, Fatigue {mate.fatigue:F0}%");
            }

            // Squadmate injury resolution during severe engagements
            if (outcome.outcomeType == TacticalOutcomeType.PyrrhicVictory || outcome.outcomeType == TacticalOutcomeType.CatastrophicAmbush)
            {
                var aliveMates = soldier.squadMates.FindAll(m => !m.isDead);
                if (aliveMates.Count > 0)
                {
                    var injuredMate = aliveMates[UnityEngine.Random.Range(0, aliveMates.Count)];
                    injuredMate.isInjured = true;

                    if (medicReady)
                    {
                        injuredMate.injuryStatus = "Stabilized Under Fire (Light Shrapnel)";
                        outcome.squadmateReports.Add($"<color=#00FFAA>[TRIAGE]</color> {injuredMate.GetDisplayName()} took shrapnel, but was stabilized immediately by Doc.");
                    }
                    else
                    {
                        injuredMate.injuryStatus = "Severe Shrapnel Trauma";
                        outcome.squadmateReports.Add($"<color=#FF4444>[CASUALTY]</color> {injuredMate.GetDisplayName()} sustained severe shrapnel wounds!");
                    }
                }
            }
        }

        private static void DispatchFeedEvents(
            SoldierProfile soldier, 
            CombatOperationOutcome outcome, 
            WarTheaterInfo theater)
        {
            string colorTag = outcome.isSuccess ? "#00FFAA" : (outcome.outcomeType == TacticalOutcomeType.TacticalWithdrawal ? "#FFCC00" : "#FF3333");
            string honorText = outcome.combatHonors.Count > 0 ? $" Honors Awarded: <b>{string.Join(", ", outcome.combatHonors)}</b>." : "";

            string feedMsg = $"<color={colorTag}><b>[COMBAT OP • {outcome.outcomeType.ToString().ToUpper()}]</b></color> " +
                $"{outcome.headline} in <b>{theater.theaterName}</b>! {outcome.reportSummary}{honorText} " +
                $"Casualties: {outcome.casualties}. Bounty Earned: <b>+${outcome.totalCashAwarded:N0}</b> (Hazard Bonus + Pay). Promotion Pts: +{outcome.promotionPoints}.";

            GameEvents.OnFeedEventLogged?.Invoke("COMBAT OPERATIONS", feedMsg);

            if (!string.IsNullOrEmpty(outcome.roleSynergySummary))
            {
                GameEvents.OnFeedEventLogged?.Invoke("SQUAD SYNERGY",
                    $"<color=#00D4FF><b>[FIRETEAM SYNERGY]</b></color> {outcome.roleSynergySummary}");
            }
        }

        #endregion

        #region Semester Deployment Tour Integration

        /// <summary>
        /// Fully processes an active 6-month deployment combat tour with mission generation,
        /// tactical resolution, hazard compensation, and fireteam updates.
        /// </summary>
        public static CombatOperationOutcome ProcessSemesterCombatTour(
            SoldierProfile soldier, 
            WarTheaterInfo theater = null, 
            TacticalMissionType? forcedMission = null, 
            TacticalChoiceOrder? forcedChoice = null, 
            TacticalOutcomeType? forceOutcome = null)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired) return null;

            soldier.InitializeSquadMatesIfEmpty();

            if (theater == null)
            {
                theater = WarTheaterInfo.GetInfo(soldier.currentTheater);
            }

            soldier.toursInCurrentTheater++;

            var mission = GenerateMission(theater, soldier, forcedMission);
            var choice = forcedChoice.HasValue ? forcedChoice.Value : TacticalChoiceOrder.DefaultOrder;

            var outcome = ExecuteMission(soldier, mission, choice, theater, forceOutcome);

            // Check for high-danger overseas deployment "Dear John" letter event
            RomanceAndCourtshipEngine.CheckForDearJohnLetter(soldier, theater);

            return outcome;
        }

        #endregion
    }
}
