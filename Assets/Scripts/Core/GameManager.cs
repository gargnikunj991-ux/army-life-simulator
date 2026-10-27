using System.Collections.Generic;
using ArmyLife.Data;
using ArmyLife.Systems;
using ArmyLife.UI;
using UnityEngine;

namespace ArmyLife.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Active Character State")]
        public SoldierProfile currentSoldier;

        [Header("Data Registries")]
        public PowerTierSO[] powerTierDatabase;
        public MilitaryRankSO[] militaryRankDatabase;
        public FamilyLineageSO[] familyLineageDatabase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Do not force-spawn Ethan Vance if no soldier is loaded.
            // OpeningSceneController will handle showing the menu or character creation.
            if (currentSoldier != null && !currentSoldier.hasAnsweredWartimeCall && currentSoldier.age <= 18.5f && !currentSoldier.isDead && !currentSoldier.isRetired)
            {
                TriggerWartimeMobilizationPopup();
            }
        }

        public void ResumeLife(string lifeId)
        {
            SoldierProfile loaded = LifePersistenceManager.LoadSoldierLife(lifeId, this);
            if (loaded != null)
            {
                currentSoldier = loaded;

                // Sync simulation calendar to character's age!
                if (TimeCycleManager.Instance != null)
                {
                    TimeCycleManager.Instance.currentYear = 2008 + Mathf.FloorToInt(currentSoldier.age);
                    TimeCycleManager.Instance.currentSemester = ((currentSoldier.age - Mathf.Floor(currentSoldier.age)) >= 0.25f) ? 2 : 1;
                }

                GameEvents.OnFeedEventLogged?.Invoke("RESUME",
                    $"<color=#00D4FF><b>[SERVICE RECORD RESUMED]</b></color> Welcome back, <b>{currentSoldier.characterName}</b>! " +
                    $"{(currentSoldier.isYouthPhase ? "[Youth Phase]" : $"Rank: {(currentSoldier.currentRank != null ? currentSoldier.currentRank.rankName : "Private")}")} | " +
                    $"Age: <b>{currentSoldier.age:F1}</b> | Year: <b>{(TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2026)}</b>.");

                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
            }
        }

        public MilitaryRankSO GetRankByPaygrade(string paygrade)
        {
            if (militaryRankDatabase == null) return null;
            foreach (var r in militaryRankDatabase)
            {
                if (r != null && r.paygrade == paygrade) return r;
            }
            return militaryRankDatabase.Length > 0 ? militaryRankDatabase[0] : null;
        }

        public FamilyLineageSO RollBirthLineage()
        {
            if (familyLineageDatabase == null || familyLineageDatabase.Length == 0) return null;

            int roll = Random.Range(1, 101); // 1 to 100

            string targetArchetype;
            if (roll <= 80) // 80% Commoner (8 out of 10)
            {
                targetArchetype = "Civilian Clean Slate";
            }
            else if (roll <= 94) // 14% Military Baron House
            {
                targetArchetype = "Command Sgt Major Backbone";
            }
            else if (roll <= 99) // 5% Special Forces Viscount House
            {
                targetArchetype = "Special Forces Legend";
            }
            else // 1% Grand Duke Dynasty (Ultra-Rare!)
            {
                targetArchetype = "4-Star General Dynasty";
            }

            foreach (var lin in familyLineageDatabase)
            {
                if (lin != null && lin.archetypeName == targetArchetype)
                {
                    return lin;
                }
            }

            return familyLineageDatabase[0];
        }

        public void StartNewLife(
            string soldierName, 
            FamilyLineageSO lineage = null, 
            string gender = "Male", 
            string birthCountry = "The Valerian Empire", 
            string hairStyle = "Military Buzzcut",
            string hairColor = "Jet Black",
            string eyeColor = "Steel Grey",
            string skinTone = "Tan",
            float looks = 80f,
            int? explicitTalent = null,
            SoldierOrigin origin = SoldierOrigin.Farmhand)
        {
            if (lineage == null)
            {
                lineage = RollBirthLineage();
            }

            int talent = explicitTalent ?? Random.Range(40, 185);

            currentSoldier = new SoldierProfile(soldierName, lineage, talent)
            {
                gender = gender,
                birthCountry = birthCountry,
                hairStyle = hairStyle,
                hairColor = hairColor,
                eyeColor = eyeColor,
                skinTone = skinTone,
                looks = looks,
                origin = origin
            };

            // Split first and last name
            string[] parts = soldierName.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0) currentSoldier.firstName = parts[0];
            if (parts.Length > 1) currentSoldier.lastName = parts[parts.Length - 1];

            // Roll realistic birth state and city from continental geography
            var (state, city, isImperial) = WorldGeographyDatabase.RollBirthLocation(birthCountry, currentSoldier.nobleTitle);
            currentSoldier.birthState = state;
            currentSoldier.birthCity = city;
            currentSoldier.isBornInImperialState = isImperial;

            // Direct Enlistment: Start at Age 18.0 in Year 2026!
            currentSoldier.age = 18.0f;
            currentSoldier.totalSemestersLived = 0;
            currentSoldier.careerTrack = CareerTrack.MilitaryRecruitBasic;
            currentSoldier.currentRank = null; // Unranked recruit in Basic Training
            currentSoldier.militarySchoolSemestersCompleted = 0;
            currentSoldier.hasAnsweredWartimeCall = false;
            currentSoldier.academyTuitionDebt = 0;
            currentSoldier.collegeTuitionDebt = 0; // All basic training expenses 100% covered by the government during wartime emergency

            // Apply Background Origin Bonuses
            switch (origin)
            {
                case SoldierOrigin.Farmhand:
                    currentSoldier.originTrait = "Country Grit";
                    currentSoldier.health = 100f;
                    currentSoldier.fitness = 85f;
                    currentSoldier.combatSkill = 35f;
                    currentSoldier.meritRating = 50f;
                    currentSoldier.intellect = 50f;
                    currentSoldier.cashBalance = 250L;
                    currentSoldier.AddTrait("Country Grit");
                    break;
                case SoldierOrigin.StreetTough:
                    currentSoldier.originTrait = "Street Brawler";
                    currentSoldier.health = 95f;
                    currentSoldier.fitness = 75f;
                    currentSoldier.combatSkill = 45f;
                    currentSoldier.meritRating = 45f;
                    currentSoldier.intellect = 50f;
                    currentSoldier.coFavor = 40f;
                    currentSoldier.cashBalance = 350L;
                    currentSoldier.AddTrait("Street Brawler");
                    break;
                case SoldierOrigin.WorkingMechanic:
                    currentSoldier.originTrait = "Field Craftsman";
                    currentSoldier.health = 95f;
                    currentSoldier.fitness = 65f;
                    currentSoldier.combatSkill = 35f;
                    currentSoldier.meritRating = 55f;
                    currentSoldier.intellect = 80f;
                    currentSoldier.cashBalance = 450L;
                    currentSoldier.AddTrait("Field Craftsman");
                    break;
                case SoldierOrigin.MilitaryLegacy:
                    currentSoldier.originTrait = "Soldier's Son";
                    currentSoldier.health = 100f;
                    currentSoldier.fitness = 80f;
                    currentSoldier.combatSkill = 40f;
                    currentSoldier.meritRating = 60f;
                    currentSoldier.intellect = 70f;
                    currentSoldier.squadRespect = 65f;
                    currentSoldier.coFavor = 65f;
                    currentSoldier.cashBalance = 500L;
                    currentSoldier.AddTrait("Soldier's Son");
                    break;
            }

            // Clock starts at Year 2026 Semester 1 (Garrison / Boot Camp)
            if (TimeCycleManager.Instance != null)
            {
                TimeCycleManager.Instance.currentYear = 2026;
                TimeCycleManager.Instance.currentSemester = 1;
            }

            // Clear any lingering feed from prior lives
            var feed = Object.FindAnyObjectByType<ArmyLife.UI.BitLifeFeedView>(FindObjectsInactive.Include);
            if (feed != null)
            {
                feed.ClearFeed();
            }

            // Initialize persistent fireteam squadmates
            currentSoldier.InitializeSquadMatesIfEmpty();
            currentSoldier.ResetSemesterActionPoints();

            // Save new life into persistent storage
            LifePersistenceManager.SaveSoldierLife(currentSoldier);

            string imperialBadge = isImperial ? " <color=#FFD700>[IMPERIAL CROWN DOMAIN]</color>" : "";
            GameEvents.OnFeedEventLogged?.Invoke("ENLISTMENT",
                $"<color=#00FFAA><b>[ENLISTMENT DAY • AGE 18.0]</b></color> You stood before the military commission in <b>{city}</b>, province of <b>{state}</b> ({birthCountry}){imperialBadge}, raised your right hand, and swore the oath of service to your country! " +
                $"Background Origin: <b>{origin}</b> (Trait: <i>{currentSoldier.originTrait}</i>). " +
                $"Appearance: <i>{hairStyle}, {hairColor} hair, {eyeColor} eyes, {skinTone} ({looks:F0}% Looks)</i>.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);

            // Trigger the Wartime Mobilization Call to Arms popup
            TriggerWartimeMobilizationPopup();
        }

        /// <summary>
        /// Advances the game by 6 months (1 tour) with frontline deployments, tactical dilemmas, and promotion boards.
        /// </summary>
        public void AdvanceTurn()
        {
            if (currentSoldier == null) return;

            // 0. If soldier is dead, stop and prompt
            if (currentSoldier.isDead)
            {
                AudioManager.PlayTaps();
                GameEvents.OnFeedEventLogged?.Invoke("SERVICE END",
                    $"<color=#FF4444><b>[SERVICE RECORD CONCLUDED]</b></color> {currentSoldier.characterName} has fallen in the line of duty ({currentSoldier.deathReason}). Review your career dossier or enlist a new recruit from the main menu.");
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

            AudioManager.PlayTurnAdvance();

            // 1. Advance Calendar Clock (+0.5 yrs / 6 months)
            if (TimeCycleManager.Instance != null)
            {
                TimeCycleManager.Instance.AdvanceSemester(currentSoldier);
            }
            else
            {
                currentSoldier.age += 0.5f;
                currentSoldier.totalSemestersLived++;
            }
            currentSoldier.timeInCurrentRankSemesters++;
            currentSoldier.ResetSemesterActionPoints();

            // 1.5. Medical Convalescence Ward Check: In hospital recovering from trauma / wounds
            if (currentSoldier.inMedicalConvalescence)
            {
                currentSoldier.consecutiveCombatSemesters = 0;
                currentSoldier.isInCombatDeployment = false;
                FieldHospitalEngine.ProcessConvalescenceTurn(currentSoldier, this, false);
                FinishTurnPostActions();
                return;
            }

            // 1.6. Check if Retired or Medically Discharged
            if (currentSoldier.isRetired)
            {
                currentSoldier.consecutiveCombatSemesters = 0;
                currentSoldier.isInCombatDeployment = false;
                CombatTraumaEngine.CheckPeacetimeFlashback(currentSoldier);
                FinishTurnPostActions();
                return;
            }

            // 2. War Academy / Basic Training / Technical School vs Frontline Active Duty
            if (currentSoldier.careerTrack == CareerTrack.MilitaryWarAcademy ||
                currentSoldier.careerTrack == CareerTrack.MilitaryRecruitBasic ||
                currentSoldier.careerTrack == CareerTrack.MilitaryTechSchool)
            {
                currentSoldier.consecutiveCombatSemesters = 0;
                currentSoldier.isInCombatDeployment = false;
                CivilAndMilitaryCareerEngine.AdvanceCareerTurn(currentSoldier, this);
            }
            else if (currentSoldier.isSpecialForcesCandidate)
            {
                ProcessSpecialForcesCandidateProgression();
                return;
            }
            else if (currentSoldier.isInCombatDeployment)
            {
                TriggerCombatTourBriefing();
                return;
            }
            else
            {
                currentSoldier.consecutiveCombatSemesters = 0;

                // Garrison Duty: PT Drills & Recovery
                currentSoldier.fitness = Mathf.Min(100f, currentSoldier.fitness + Random.Range(3f, 6f));
                currentSoldier.health = Mathf.Min(100f, currentSoldier.health + 12f);
                GameEvents.OnFeedEventLogged?.Invoke("GARRISON",
                    $"<color=#00FFAA><b>[GARRISON DUTY & REFIT]</b></color> Stationed at Fort Valerius for refit, physical conditioning, and marksmanship drills. (+Health, +Fitness).");

                // Roll High-Stakes Tactical / Barracks Dilemma
                MilitaryDilemmaEngine.EvaluateAdultSemesterDilemma(currentSoldier, this);

                // Natural Deployment Mobilization:
                // After 1 semester of garrison refit (or immediately if volunteer extended tour is set), mobilize to the frontline!
                currentSoldier.garrisonSemestersInRow++;
                if (currentSoldier.garrisonSemestersInRow >= 1 || currentSoldier.isVoluntaryExtendedTour)
                {
                    currentSoldier.isInCombatDeployment = true;
                    currentSoldier.garrisonSemestersInRow = 0;
                    currentSoldier.currentTheater = RollNextWarTheater();
                    var nextTheater = WarTheaterInfo.GetInfo(currentSoldier.currentTheater);
                    GameEvents.OnFeedEventLogged?.Invoke("DEPLOYMENT ORDERS",
                        $"<color=#FFD700><b>[MOBILIZATION ORDER • HIGH COMMAND]</b></color> Your garrison refit has concluded. " +
                        $"High Command has ordered your regiment to deploy to the frontlines of <b>{nextTheater.theaterName}</b> next tour! " +
                        $"(Danger: {nextTheater.dangerLevel}/5 | Hazard Pay: +${nextTheater.hazardPayRate}/mo). Prepare your gear and inspect your squad!");
                }

                FinishTurnPostActions();
                return;
            }

            FinishTurnPostActions();
        }

        public WarTheaterType RollNextWarTheater()
        {
            WarTheaterType[] frontlines = new WarTheaterType[]
            {
                WarTheaterType.ArdenRidgeTrenchLines,
                WarTheaterType.PortIronAmphibiousFront,
                WarTheaterType.KaelenValleyUrbanZone,
                WarTheaterType.BadlandsSteppeRecon,
                WarTheaterType.NorthernTaigaOutpost,
                WarTheaterType.PacificJungleIslands
            };
            int idx = Random.Range(0, frontlines.Length);
            return frontlines[idx];
        }

        /// <summary>
        /// Displays the interactive Tactical Combat Mission Briefing for active combat tours.
        /// Unlocks the full Tactical Combat Operations Engine, doctrine selection, and fireteam synergies.
        /// </summary>
        public void TriggerCombatTourBriefing()
        {
            if (currentSoldier == null || currentSoldier.isDead) return;

            currentSoldier.consecutiveCombatSemesters++;
            currentSoldier.toursInCurrentTheater++;
            var theater = WarTheaterInfo.GetInfo(currentSoldier.currentTheater);
            var mission = CombatOperationsEngine.GenerateMission(theater, currentSoldier);

            currentSoldier.InitializeSquadMatesIfEmpty();
            var scout = currentSoldier.squadMates.Find(m => m.role == SquadRole.LeadScout && !m.isDead);
            var gunner = currentSoldier.squadMates.Find(m => m.role == SquadRole.HeavyGunner && !m.isDead);
            var medic = currentSoldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);

            bool scoutAlert = (scout != null && scout.relationship >= 40f && scout.fatigue < 80f);
            bool gunnerReady = (gunner != null && gunner.relationship >= 40f && gunner.fatigue < 80f);
            bool medicReady = (medic != null && medic.relationship >= 40f && medic.fatigue < 80f);

            bool isCommander = SquadmateCommandEngine.IsNCOOrOfficer(currentSoldier);
            string extBadge = currentSoldier.isVoluntaryExtendedTour ? " <color=#FF8800>[VOLUNTARY EXTENSION]</color>" : "";
            string bonusBadge = currentSoldier.hasExtendedTourPayBonus ? " <color=#FFD700>[+10% VETERAN COMBAT PAY]</color>" : "";

            string category = $"COMBAT PATROL • PHASE 1/3: MOVEMENT TO CONTACT";
            string title = $"APPROACH TO {theater.theaterName.ToUpper()}";

            string scoutStatus = scout == null ? "KIA / Absent" : (scoutAlert ? "<color=#00FFAA>Vigilant & Alert</color>" : "<color=#FFAA00>Exhausted / Strained</color>");
            string gunnerStatus = gunner == null ? "KIA / Absent" : (gunnerReady ? "<color=#00FFAA>Heavy Gun Ready</color>" : "<color=#FFAA00>Fatigued / Low Ammo</color>");
            string medicStatus = medic == null ? "KIA / Absent" : (medicReady ? "<color=#00FFAA>Trauma Webbing Ready</color>" : "<color=#FFAA00>Medkits Low</color>");

            string description =
                $"<b>OPERATION:</b> {mission.missionName.ToUpper()}\n" +
                $"<b>OBJECTIVE:</b> {mission.objectiveDescription}\n\n" +
                $"<b>THEATER:</b> {theater.theaterName} (Danger: {theater.dangerLevel}/5, Weather: <i>{theater.weatherAndEnvironment}</i>){extBadge}{bonusBadge}\n\n" +
                $"<b>HEADQUARTERS SITREP:</b> Regimental HQ advises target redoubt is lightly garrisoned. Direct orders: Advance immediately down the main approach road without delay.\n\n" +
                $"<b>SCOUT'S FIELD WHISPER:</b> Lead Scout <b>{scout?.nickname ?? "Miller"}</b> ({scoutStatus}) urgently whispers:\n" +
                $"<i>\"Sir! Binoculars spot fresh berms and electrical command wires along that road! That's a buried 155mm IED pressure-plate ambush! If we advance down that road we'll be blown apart. We have to divert through the marsh creek!\"</i>\n\n" +
                $"<b>FIRETEAM COMBAT READINESS:</b>\n" +
                $"• Heavy Gunner: <b>{gunner?.nickname ?? "Anvil"}</b> ({gunnerStatus}) | Combat Medic: <b>{medic?.nickname ?? "Doc"}</b> ({medicStatus})\n\n" +
                $"<i>Phase 1: Make your critical tactical approach choice:</i>";

            string choiceA = "📻 OBEY HQ ORDERS: MAIN ROAD ADVANCE\n<size=75%>Follow battalion orders (+10 CO Standing, but high risk of catastrophic IED detonation!)</size>";
            string choiceB = "🥾 TRUST SCOUT'S FIELD WHISPER\n<size=75%>Divert through the marsh creek defiles (+20 Tactical Advantage, bypasses IED, saves squad)</size>";
            string choiceC = "💥 CALL ARTILLERY PRE-BOMBARDMENT\n<size=75%>Blanket road with high-explosive battery fire to detonate buried IEDs from a distance</size>";

            var dilemma = DilemmaModal.Instance ?? Object.FindAnyObjectByType<DilemmaModal>(FindObjectsInactive.Include);
            if (dilemma != null)
            {
                dilemma.DisplayDilemma(
                    category,
                    title,
                    description,
                    choiceA,
                    choiceB,
                    choiceC,
                    () => ResolveCombatPhase1(mission, 0, theater),
                    () => ResolveCombatPhase1(mission, 1, theater),
                    () => ResolveCombatPhase1(mission, 2, theater)
                );
            }
            else
            {
                ResolveCombatOperation(mission, TacticalChoiceOrder.DefaultOrder, theater);
            }
        }

        private void ResolveCombatPhase1(TacticalMission mission, int approachChoice, WarTheaterInfo theater)
        {
            if (currentSoldier == null) return;

            int phase1Bonus = 0;
            var scout = currentSoldier.squadMates.Find(m => m.role == SquadRole.LeadScout && !m.isDead);
            bool scoutAlert = (scout != null && scout.relationship >= 40f && scout.fatigue < 80f);

            if (approachChoice == 0) // Obey HQ Orders
            {
                currentSoldier.coFavor = Mathf.Min(100f, currentSoldier.coFavor + 10f);
                int iedRoll = UnityEngine.Random.Range(1, 101);
                if (iedRoll <= 60) // Catastrophic IED Detonation!
                {
                    FieldHospitalEngine.TriggerIEDDetonation(currentSoldier, this);
                    FinishTurnPostActions();
                    return;
                }
                else
                {
                    phase1Bonus = 8;
                    GameEvents.OnFeedEventLogged?.Invoke("APPROACH",
                        "<color=#00D4FF>[ROAD ADVANCE]</color> Through pure fortune, the squad cleared the main road without triggering suspected pressure plates (+8 Tactical Advantage, +10 CO Standing).");
                }
            }
            else if (approachChoice == 1) // Trust Scout
            {
                if (scoutAlert)
                {
                    phase1Bonus = 20;
                    currentSoldier.meritRating = Mathf.Min(100f, currentSoldier.meritRating + 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("APPROACH",
                        $"<color=#00FFAA><b>[SCOUT WHISPER TRUSTED]</b></color> Lead Scout {scout.nickname} guided the fireteam safely through the marsh defile, completely bypassing the buried IED kill zone! You caught the enemy outpost unawares (+20 Tactical Advantage, +10 Merit).");
                }
                else
                {
                    phase1Bonus = 12;
                    GameEvents.OnFeedEventLogged?.Invoke("APPROACH",
                        "<color=#00D4FF>[FLANK ADVANCE]</color> Fireteam successfully skirted the paved road and flanked the enemy perimeter (+12 Tactical Advantage).");
                }
            }
            else // Artillery Pre-Bombardment
            {
                phase1Bonus = 15;
                AudioManager.PlayArtilleryExplosion();
                GameEvents.OnFeedEventLogged?.Invoke("APPROACH",
                    "<color=#FFD700><b>[PRE-BOMBARDMENT BARRAGE]</b></color> 155mm battery shells pulverized the road berms, detonating buried IEDs in massive secondary eruptions! The route was blasted clear (+15 Tactical Advantage).");
            }

            // Transition to Phase 2: The Main Clash & Redoubt Breach
            TriggerCombatPhase2(mission, phase1Bonus, theater);
        }

        private void TriggerCombatPhase2(TacticalMission mission, int phase1Bonus, WarTheaterInfo theater)
        {
            if (currentSoldier == null || currentSoldier.isDead) return;

            bool isCommander = SquadmateCommandEngine.IsNCOOrOfficer(currentSoldier);
            var gunner = currentSoldier.squadMates.Find(m => m.role == SquadRole.HeavyGunner && !m.isDead);
            bool gunnerReady = (gunner != null && gunner.relationship >= 40f && gunner.fatigue < 80f);
            string gunnerStatus = gunner == null ? "KIA / Absent" : (gunnerReady ? "<color=#00FFAA>Heavy Gun Ready</color>" : "<color=#FFAA00>Fatigued / Low Ammo</color>");

            NemesisProfile nemesis = BattlefieldNemesisEngine.GetNemesis(theater.theaterType);
            bool nemesisActive = BattlefieldNemesisEngine.RollNemesisAppearance(theater.theaterType, currentSoldier);

            string nemesisAlert = nemesisActive
                ? $"\n\n<color=#FF3333><b>⚠️ INFAMOUS THEATER RIVAL ENGAGED: {nemesis.name.ToUpper()} ({nemesis.title.ToUpper()})</b></color>\n" +
                  $"<i>{nemesis.backstory} Confront them for a +${nemesis.bounty:N0} bounty and the Cross of the Nemesis Hunter!</i>"
                : "";

            string category = $"COMBAT PATROL • PHASE 2/3: THE CLASH & ASSAULT";
            string title = nemesisActive ? $"SHOWDOWN WITH {nemesis.name.ToUpper()}" : "BREACHING THE REDOUBT";
            string description =
                $"<b>THEATER:</b> {theater.theaterName}\n" +
                $"<b>SITUATION:</b> Hostile heavy machine guns open up from reinforced bunkers! Shrapnel lacerates the mud. Your squad is at the wire redoubt.\n\n" +
                $"• Heavy Gunner: <b>{gunner?.nickname ?? "Anvil"}</b> ({gunnerStatus}) [Cyclic Suppression]\n" +
                $"• Tactical Approach Advantage: <b>+{phase1Bonus} Pts</b>{nemesisAlert}\n\n" +
                $"<i>{(isCommander ? "Commander: Issue your assault command to the fireteam!" : "Soldier: Select your frontline assault action!")}</i>";

            string choiceA, choiceB, choiceC;
            TacticalChoiceOrder orderA, orderB, orderC;

            if (isCommander)
            {
                choiceA = "⚔️ AGGRESSIVE BREACH ASSAULT\n<size=75%>Direct Gunner suppression & storm redoubt with grenades</size>";
                orderA = TacticalChoiceOrder.AggressiveBreach;

                choiceB = "💥 HEAVY CYCLIC SUPPRESSION PIN\n<size=75%>Lay continuous machine gun crossfire to silence bunker slits</size>";
                orderB = TacticalChoiceOrder.HeavySuppression;

                choiceC = "📞 DANGER-CLOSE ARTILLERY STRIKE\n<size=75%>Coordinate 155mm battery barrage onto redoubt roof</size>";
                orderC = TacticalChoiceOrder.ArtilleryStrikeCall;
            }
            else
            {
                choiceA = "🪖 LEAD POINT BREACH ASSAULT\n<size=75%>Aggressive breach through wire with grenades (High Risk / High Merit)</size>";
                orderA = TacticalChoiceOrder.AggressiveBreach;

                choiceB = "💥 COVERING MACHINE GUN SUPPRESSION\n<size=75%>Coordinate with Heavy Gunner to maintain suppressing crossfire</size>";
                orderB = TacticalChoiceOrder.HeavySuppression;

                choiceC = "🎯 ACCURATE RIFLE OVERWATCH\n<size=75%>Pick off enemy counter-snipers and bunker gunners</size>";
                orderC = TacticalChoiceOrder.ReconnaissanceFlank;
            }

            var dilemma = DilemmaModal.Instance ?? Object.FindAnyObjectByType<DilemmaModal>(FindObjectsInactive.Include);
            if (dilemma != null)
            {
                dilemma.DisplayDilemma(
                    category,
                    title,
                    description,
                    choiceA,
                    choiceB,
                    choiceC,
                    () => ResolveCombatPhase2(mission, orderA, phase1Bonus, theater),
                    () => ResolveCombatPhase2(mission, orderB, phase1Bonus, theater),
                    () => ResolveCombatPhase2(mission, orderC, phase1Bonus, theater)
                );
            }
            else
            {
                ResolveCombatOperation(mission, TacticalChoiceOrder.DefaultOrder, theater);
            }
        }

        private void ResolveCombatPhase2(TacticalMission mission, TacticalChoiceOrder assaultOrder, int phase1Bonus, WarTheaterInfo theater)
        {
            if (currentSoldier == null) return;

            var nemesis = BattlefieldNemesisEngine.GetNemesis(theater.theaterType);
            if (nemesis != null && !nemesis.isDefeated && UnityEngine.Random.value < 0.35f)
            {
                int clashChoice = (assaultOrder == TacticalChoiceOrder.ArtilleryStrikeCall) ? 2 : (assaultOrder == TacticalChoiceOrder.HeavySuppression ? 1 : 0);
                BattlefieldNemesisEngine.ResolveNemesisClash(currentSoldier, nemesis, clashChoice, out string _);
            }

            GameEvents.OnFeedEventLogged?.Invoke("ASSAULT",
                $"<color=#FFD700><b>[BATTLEFIELD CLASH]</b></color> You committed the fireteam to <b>{assaultOrder}</b>! " +
                "Machine guns roared, grenade detonations shattered the concrete revetment, and the hostile strongpoint was breached!");

            // Transition to Phase 3: Extraction Corridor Crisis
            TriggerCombatPhase3(mission, assaultOrder, phase1Bonus, theater);
        }

        private void TriggerCombatPhase3(TacticalMission mission, TacticalChoiceOrder assaultOrder, int phase1Bonus, WarTheaterInfo theater)
        {
            if (currentSoldier == null || currentSoldier.isDead) return;

            var medic = currentSoldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);
            bool medicReady = (medic != null && medic.relationship >= 40f && medic.fatigue < 80f);
            string medicStatus = medic == null ? "KIA / Absent" : (medicReady ? "<color=#00FFAA>Trauma Webbing Ready</color>" : "<color=#FFAA00>Medkits Low</color>");

            string category = $"COMBAT PATROL • PHASE 3/3: CRISIS & EXTRACTION";
            string title = $"CRISIS AT THE EXTRACTION LINE";
            string description =
                $"<b>THEATER:</b> {theater.theaterName}\n" +
                $"<b>SITUATION:</b> The bunker is burning, but an urgent crisis erupts! Enemy mortar barrages hammer the withdrawal corridor, and a squad brother is pinned beneath fallen timbers in no-man's land!\n\n" +
                $"• Combat Medic: <b>{medic?.nickname ?? "Doc"}</b> ({medicStatus})\n" +
                $"• Available Medkits: <b>{currentSoldier.fieldMedkitsCount}</b>\n\n" +
                $"<i>Make your critical command decision under enemy fire:</i>";

            string choiceA = "🩸 HEROIC CASUALTY RESCUE (CASEVAC)\n<size=75%>Drag your pinned brother to safety under mortar fire! (Heroic valor, saves comrade)</size>";
            string choiceB = "🛡️ DEFENSIVE OVERWATCH WITHDRAWAL\n<size=75%>Establish leap-frog firing line preserving squad cohesion and minimal losses</size>";
            string choiceC = "📦 RAID HOSTILE CIPHER VAULT\n<size=75%>Seize enemy operational maps & ciphers before detonating bunker (+Merit & Bounty)</size>";

            var dilemma = DilemmaModal.Instance ?? Object.FindAnyObjectByType<DilemmaModal>(FindObjectsInactive.Include);
            if (dilemma != null)
            {
                dilemma.DisplayDilemma(
                    category,
                    title,
                    description,
                    choiceA,
                    choiceB,
                    choiceC,
                    () => ResolveCombatPhase3(mission, assaultOrder, 0, phase1Bonus, theater),
                    () => ResolveCombatPhase3(mission, assaultOrder, 1, phase1Bonus, theater),
                    () => ResolveCombatPhase3(mission, assaultOrder, 2, phase1Bonus, theater)
                );
            }
            else
            {
                ResolveCombatOperation(mission, assaultOrder, theater);
            }
        }

        private void ResolveCombatPhase3(TacticalMission mission, TacticalChoiceOrder assaultOrder, int extractionChoice, int totalBonus, WarTheaterInfo theater)
        {
            if (currentSoldier == null) return;

            TacticalChoiceOrder finalOrder = assaultOrder;

            if (extractionChoice == 0) // CASEVAC Heroic Rescue
            {
                finalOrder = TacticalChoiceOrder.EmergencyCASEVAC;
                currentSoldier.squadRespect = Mathf.Min(100f, currentSoldier.squadRespect + 20f);
                currentSoldier.AddTrait("Life Saver");
                foreach (var m in currentSoldier.squadMates)
                {
                    if (m != null && !m.isDead) m.relationship = Mathf.Min(100f, m.relationship + 15f);
                }
                GameEvents.OnFeedEventLogged?.Invoke("HEROISM",
                    "<color=#00FFAA><b>[HEROIC RESCUE UNDER FIRE]</b></color> You sprinted through erupting mortar geysers, lifted the timber, and dragged your brother to the casualty collection point! (+20 Squad Respect, 'Life Saver' trait awarded).");
            }
            else if (extractionChoice == 1) // Safe Defensive Withdrawal
            {
                finalOrder = assaultOrder;
                GameEvents.OnFeedEventLogged?.Invoke("TACTICS",
                    "<color=#00D4FF>[DISCIPLINED WITHDRAWAL]</color> Fireteam executed an orderly fighting retreat, preserving squad strength and weapons.");
            }
            else // Raid Cipher Vault
            {
                currentSoldier.meritRating = Mathf.Min(100f, currentSoldier.meritRating + 15f);
                currentSoldier.combatMeritPoints += 300;
                currentSoldier.cashBalance += 400L;
                GameEvents.OnFeedEventLogged?.Invoke("INTELLIGENCE",
                    "<color=#FFD700><b>[ENEMY CIPHERS SEIZED]</b></color> You breached the command safe and secured classified operational maps and codebooks! (+15 Merit, +300 CMP, +$400 Intel Bounty).");
            }

            // Resolve the operational outcome via the Combat Engine
            ResolveCombatOperation(mission, finalOrder, theater);
        }

        public void ResolveCombatOperation(TacticalMission mission, TacticalChoiceOrder order, WarTheaterInfo theater)
        {
            if (currentSoldier == null) return;

            var outcome = CombatOperationsEngine.ExecuteMission(currentSoldier, mission, order, theater);

            // Tour rotation handling
            if (!currentSoldier.isVoluntaryExtendedTour)
            {
                currentSoldier.isInCombatDeployment = false;
                currentSoldier.consecutiveCombatSemesters = 0;
                GameEvents.OnFeedEventLogged?.Invoke("ROTATION",
                    $"<color=#00FFAA><b>[TOUR COMPLETED • ROTATING TO GARRISON]</b></color> Operational objectives completed in {theater.theaterName}! " +
                    "Your unit was relieved by incoming fresh reserves. Rotated back to Fort Valerius for garrison rest, refit, and marksmanship drills.");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("ROTATION",
                    $"<color=#FF8800><b>[CONTINUOUS FRONT CONTINUED]</b></color> You remained at the frontline per your voluntary tour extension!");
            }

            FinishTurnPostActions();
        }

        public void FinishTurnPostActions()
        {
            if (currentSoldier == null) return;

            if (currentSoldier.isDead)
            {
                LifePersistenceManager.SaveSoldierLife(currentSoldier);
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

            // Payday, US-Style Living Costs, Investments & Crime Heat Cycle
            EconomyEngine.ProcessSemesterEconomy(currentSoldier);
            CrimeEngine.ProcessSemesterHeatAndRaids(currentSoldier);

            // Military Promotion Board Evaluation (Active Enlisted and Officers)
            if (currentSoldier.careerTrack != CareerTrack.MilitaryWarAcademy && !currentSoldier.isSpecialForcesCandidate)
            {
                PromotionBoardEngine.EvaluatePromotionBoard(currentSoldier, militaryRankDatabase);
            }

            // Annual Longevity Pay Step (Awarded each full year of service, unless promoted that year)
            if (currentSoldier.totalSemestersLived > 0 && currentSoldier.totalSemestersLived % 2 == 0)
            {
                if (currentSoldier.wasPromotedThisYear)
                {
                    currentSoldier.wasPromotedThisYear = false;
                }
                else if (!currentSoldier.isRetired && !currentSoldier.isCivilian && currentSoldier.careerTrack != CareerTrack.MilitaryWarAcademy && !currentSoldier.isYouthPhase)
                {
                    long baseSalary = (currentSoldier.currentRank != null) ? currentSoldier.currentRank.baseMonthlySalary : 600L;
                    long annualRaise = System.Math.Max(150L, (long)(baseSalary * 0.05f));
                    currentSoldier.longevityMonthlyPayBonus += annualRaise;
                    currentSoldier.annualLongevityRaisesEarned++;

                    GameEvents.OnFeedEventLogged?.Invoke("PAYDAY",
                        $"<color=#00FFAA><b>[ANNUAL SERVICE PAY RAISE]</b></color> You completed a full year of military service without rank promotion. " +
                        $"Imperial regulations awarded you an annual longevity pay raise of <b>+${annualRaise:N0}/mo</b>! " +
                        $"(Total Accumulated Longevity Pay: +${currentSoldier.longevityMonthlyPayBonus:N0}/mo).");
                }
            }

            // Check Victory Condition: 4-Star General (O-10)
            if (currentSoldier.currentRank != null && (currentSoldier.currentRank.paygrade == "O-10" || currentSoldier.currentRank.rankName.Contains("General")))
            {
                if (!currentSoldier.HasTrait("Supreme Allied Commander"))
                {
                    currentSoldier.AddTrait("Supreme Allied Commander");
                    currentSoldier.AddMedal("Grand Imperial Star");
                    GameEvents.OnFeedEventLogged?.Invoke("SUPREME VICTORY",
                        "<color=#FFD700><b>[SUPREME VICTORY ACHIEVED!]</b></color> You have ascended to <b>General of the Army (O-10)</b>! " +
                        "As Supreme Commander, you command the entire armed forces. You have conquered the pinnacle of military service!");
                }
            }

            // Check 20-Year Career Milestone (Honorable Retirement Eligibility)
            if (currentSoldier.totalSemestersLived >= 40 && !currentSoldier.HasTrait("20-Year Veteran"))
            {
                currentSoldier.AddTrait("20-Year Veteran");
                currentSoldier.AddMedal("Distinguished Service Cross");
                GameEvents.OnFeedEventLogged?.Invoke("CAREER MILESTONE",
                    "<color=#FFD700><b>[20 YEARS OF SERVICE REACHED]</b></color> You have achieved 20 years of active military duty! " +
                    "You are now eligible to take <b>Honorable Retirement</b> with a full lifetime military pension ($1,500/mo)!");
            }

            // Post-Service Veteran Career Turn
            if (currentSoldier.isRetired && currentSoldier.veteranCareer != VeteranCareer.None)
            {
                switch (currentSoldier.veteranCareer)
                {
                    case VeteranCareer.UniversityGI:
                        currentSoldier.cashBalance += (500L * 6);
                        currentSoldier.intellect = Mathf.Min(100f, currentSoldier.intellect + 6f);
                        GameEvents.OnFeedEventLogged?.Invoke("GI BILL",
                            $"<color=#00FFAA>[GI BILL TERM]</color> You attended university lectures on military scholarship (+${500 * 6:N0} GI Stipend, +6 Intellect).");
                        break;
                    case VeteranCareer.DefenseContractor:
                        currentSoldier.cashBalance += (2500L * 6);
                        GameEvents.OnFeedEventLogged?.Invoke("CONTRACTOR",
                            $"<color=#FFD700>[DEFENSE CONTRACTOR]</color> Consulted for military arms procurement (+${2500 * 6:N0} Consulting Pay).");
                        break;
                    case VeteranCareer.CityMayor:
                        currentSoldier.cashBalance += (4000L * 6);
                        currentSoldier.politicalInfluencePoints += 15;
                        GameEvents.OnFeedEventLogged?.Invoke("CIVIC OFFICE",
                            $"<color=#FFD700>[MAYORAL GOVERNANCE]</color> Governed municipal infrastructure with veteran military discipline (+${4000 * 6:N0} Mayoral Pay).");
                        break;
                    case VeteranCareer.RegionalGovernor:
                        currentSoldier.cashBalance += (8000L * 6);
                        currentSoldier.politicalInfluencePoints += 30;
                        GameEvents.OnFeedEventLogged?.Invoke("GOVERNOR",
                            $"<color=#FFD700>[PROVINCIAL GOVERNOR]</color> Administered regional wartime logistics and security (+${8000 * 6:N0} Governor Pay).");
                        break;
                    case VeteranCareer.MinisterOfDefense:
                        currentSoldier.cashBalance += (18000L * 6);
                        currentSoldier.politicalInfluencePoints += 50;
                        GameEvents.OnFeedEventLogged?.Invoke("CABINET",
                            $"<color=#FFD700>[MINISTER OF DEFENSE]</color> Directed national defense policy and armed forces budget (+${18000 * 6:N0} Cabinet Pay).");
                        break;
                }
            }

            // BitLife-Style Dynamic Family Turn: Sibling births, family aging, and life events
            currentSoldier.ProcessFamilySemesterTurn();

            // Generational Child Aging
            if (currentSoldier.childHeir != null)
            {
                currentSoldier.childHeir.age += 0.5f;
                if (Mathf.Approximately(currentSoldier.childHeir.age, 18.0f))
                {
                    GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                        $"<color=#00FFAA><b>[CHILD TURNS 18 • MILITARY ELIGIBLE]</b></color> Your child, <b>{currentSoldier.childHeir.childName}</b>, has reached Age 18 and is now eligible to enlist as a <b>Military Legacy</b> recruit!");
                }
            }

            // Trigger Dramatic Semester Dispatch Modal (BitLife-style cinematic turn milestone)
            if (!currentSoldier.isDead && !currentSoldier.isYouthPhase && SemesterDispatchModal.Instance != null)
            {
                long basePay = SalaryCalculator.CalculateGrossMonthlyPay(currentSoldier) * 6L;
                long hazard = currentSoldier.combatBountiesEarned;
                long livingCosts = EconomyEngine.CalculateSemesterHousingCost(currentSoldier);
                SemesterDispatchModal.Instance.DisplayDispatch(currentSoldier, basePay, hazard, livingCosts);
            }

            // Auto-save soldier profile to persistent storage
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void RetireSoldierHonorably()
        {
            if (currentSoldier == null || currentSoldier.isDead || currentSoldier.isRetired) return;
            currentSoldier.isRetired = true;
            currentSoldier.calculatedPensionPerMonth = EconomyEngine.CalculateScaledPension(currentSoldier);
            currentSoldier.AddTrait("Honorable Veteran");
            GameEvents.OnFeedEventLogged?.Invoke("RETIREMENT",
                $"<color=#00FFAA><b>[HONORABLE RETIREMENT CEREMONY]</b></color> After {currentSoldier.totalSemestersLived * 0.5f:F1} years of faithful military service, " +
                $"you were honorably discharged with full military honors! Awarded lifetime military pension of <b>${currentSoldier.calculatedPensionPerMonth:N0}/mo</b>.");
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        /// <summary>
        /// Displays the Wartime Mobilization: National Call to Arms proclamation popup.
        /// Announces that the country is at war, calls upon the recruit for help to serve their country,
        /// covers 100% of all training expenses, and offers the choice to join the Army or Air Force.
        /// </summary>
        public void TriggerWartimeMobilizationPopup()
        {
            if (currentSoldier == null) return;

            AudioManager.PlayAlertPing();
            AudioManager.PlayRadioSquelch();

            string country = !string.IsNullOrWhiteSpace(currentSoldier.birthCountry) 
                ? currentSoldier.birthCountry 
                : "the Valerian Empire";

            string category = "WARTIME MOBILIZATION • NATIONAL CALL TO ARMS";
            string title = "YOUR COUNTRY IS AT WAR!";
            string description = 
                $"<color=#FF4444><b>EMERGENCY MOBILIZATION ORDER FROM HIGH COMMAND:</b></color>\n\n" +
                $"War has officially erupted across the frontiers of <b>{country}</b>! " +
                $"Hostile forces are invading, and the Supreme Defense Council has issued an emergency <b>National Call to Arms</b>. " +
                $"Your homeland is under direct threat and is calling upon you for help—it is your solemn duty to step forward and serve your nation!\n\n" +
                $"<color=#00FFAA><b>100% STATE-COVERED TRAINING EXPENSES:</b></color>\n" +
                $"Because your nation requires every capable citizen during total war, <b>100% of all basic training expenses, armory gear, uniforms, ammunition drills, rations, and barracks quarters are covered by the government</b> ($0 expense to you).\n\n" +
                $"You will report immediately for military Basic Training. From recruit drills, you will forge your military career—with an open pipeline to rise through the ranks, even reaching commissioned officer rank as an <b>Ensign (O-1) / 2nd Lieutenant</b> and leading your forces to victory!\n\n" +
                $"<b>Which branch will you answer the call to join?</b>";

            string choiceArmy = "🪖 JOIN THE ARMY\n<size=75%>Ground Forces & Infantry • Basic Combat Training (100% State Funded • Path to 2nd Lt)</size>";
            string choiceAirForce = "✈️ JOIN THE AIR FORCE\n<size=75%>Aviation & Flight Wing • Basic Flight Training (100% State Funded • Path to Ensign)</size>";

            var dilemma = DilemmaModal.Instance;
            if (dilemma == null)
            {
                dilemma = Object.FindAnyObjectByType<DilemmaModal>(FindObjectsInactive.Include);
            }

            if (dilemma != null)
            {
                dilemma.DisplayDilemma(
                    category,
                    title,
                    description,
                    choiceArmy,
                    choiceAirForce,
                    null,
                    () => EnlistInWartimeBranch(MilitaryBranch.Army),
                    () => EnlistInWartimeBranch(MilitaryBranch.AirForce),
                    null
                );
            }
            else
            {
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    category,
                    title,
                    description,
                    choiceArmy,
                    choiceAirForce,
                    null,
                    () => EnlistInWartimeBranch(MilitaryBranch.Army),
                    () => EnlistInWartimeBranch(MilitaryBranch.AirForce),
                    null
                );
            }
        }

        /// <summary>
        /// Handles wartime branch enlistment into Army or Air Force with 100% covered basic training.
        /// </summary>
        public void EnlistInWartimeBranch(MilitaryBranch branch)
        {
            if (currentSoldier == null) return;

            AudioManager.PlayTurnAdvance();
            AudioManager.PlayDrillWhistle();

            currentSoldier.hasAnsweredWartimeCall = true;
            currentSoldier.militaryBranch = branch;
            currentSoldier.careerTrack = CareerTrack.MilitaryRecruitBasic;
            currentSoldier.militarySchoolSemestersCompleted = 0;
            currentSoldier.currentRank = null; // Unranked recruit in Basic Training
            currentSoldier.isInCombatDeployment = false;
            currentSoldier.academyTuitionDebt = 0;
            currentSoldier.collegeTuitionDebt = 0; // 100% government covered

            if (branch == MilitaryBranch.Army)
            {
                currentSoldier.airForceRole = AirForceRole.None;
                currentSoldier.assignedDepartment = "Infantry Recruit Depot (Fort Ironclad)";
                currentSoldier.fitness = Mathf.Max(currentSoldier.fitness, 70f);
                currentSoldier.combatSkill = Mathf.Max(currentSoldier.combatSkill, 35f);

                GameEvents.OnFeedEventLogged?.Invoke("CALL TO ARMS",
                    $"<color=#FF8844><b>[WARTIME MOBILIZATION • ENLISTED IN THE ARMY]</b></color> " +
                    $"You answered your country's emergency call to arms and enlisted in the <b>Imperial Army Ground Forces</b>! " +
                    $"<b>All training expenses, tactical equipment, and barracks rations are 100% paid by the government ($0 expense)</b>. " +
                    $"You reported to Fort Ironclad for 6 months of grueling Basic Combat Training drills. " +
                    $"Endure recruit drills to earn your Private (E-1) stripes, with pathways open to Officer Commissioning (2nd Lieutenant O-1) and General Command!");
            }
            else
            {
                currentSoldier.airForceRole = AirForceRole.FlightDuty;
                currentSoldier.assignedDepartment = "Imperial Air Force Recruit Depot (Aviation Wing)";
                currentSoldier.intellect = Mathf.Max(currentSoldier.intellect, 65f);
                currentSoldier.fitness = Mathf.Max(currentSoldier.fitness, 60f);

                GameEvents.OnFeedEventLogged?.Invoke("CALL TO ARMS",
                    $"<color=#00D4FF><b>[WARTIME MOBILIZATION • ENLISTED IN THE AIR FORCE]</b></color> " +
                    $"You answered your country's emergency call to arms and enlisted in the <b>Imperial Air Force Aviation Corps</b>! " +
                    $"<b>All flight simulators, aviation ground instruction, and flight suits are 100% paid by the government ($0 expense)</b>. " +
                    $"You reported to the Air Recruit Depot for 1 year of aviation and survival conditioning. " +
                    $"Graduate to earn your Airman (E-1) wings with flight pay (+15% Aviation Bonus), with pathways open to Officer Commissioning (Ensign / Flight Lieutenant O-1)!");
            }

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void EnlistInAirForce(AirForceRole role)
        {
            if (currentSoldier == null) return;
            currentSoldier.militaryBranch = MilitaryBranch.AirForce;
            currentSoldier.airForceRole = role;
            currentSoldier.careerTrack = CareerTrack.MilitaryEnlisted;
            currentSoldier.currentRank = GetRankByPaygrade("E-1");
            currentSoldier.assignedDepartment = (role == AirForceRole.FlightDuty)
                ? "1st Tactical Aviation Wing (Flight Duty)"
                : "Air Base Ground Logistics & Avionics";
            currentSoldier.isInCombatDeployment = false;

            string flightBonusNotice = (role == AirForceRole.FlightDuty)
                ? " <b>Aviation Flight Duty active (+15% Flight Pay Bonus)!</b>"
                : " Serving as ground technical crew.";

            GameEvents.OnFeedEventLogged?.Invoke("ENLISTMENT",
                $"<color=#00D4FF><b>[ENLISTED IN THE IMPERIAL AIR FORCE]</b></color> You joined the Imperial Air Force as <b>{currentSoldier.assignedDepartment}</b>!{flightBonusNotice} " +
                "Air Force base quarters provide superior living conditions (+15% Morale, +10% Health recovery).");

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void ToggleExtendedCombatTour()
        {
            if (currentSoldier == null || currentSoldier.isDead || currentSoldier.isRetired) return;

            currentSoldier.isVoluntaryExtendedTour = !currentSoldier.isVoluntaryExtendedTour;

            if (currentSoldier.isVoluntaryExtendedTour)
            {
                currentSoldier.isInCombatDeployment = true;
                GameEvents.OnFeedEventLogged?.Invoke("EXTENDED TOUR",
                    "<color=#FF4444><b>[VOLUNTEERED FOR EXTENDED COMBAT TOUR]</b></color> You signed an official voluntary tour extension to remain deployed on the frontline longer than 6 months! " +
                    "If you stay deployed continuously for longer than 1 year, your total pay will automatically increase by <b>+10%</b>!");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("EXTENDED TOUR",
                    "<color=#00FFAA><b>[TOUR EXTENSION CONCLUDED]</b></color> You requested standard rotation back to garrison base for rest and refit upon your next relief order.");
            }

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void ApplyForOfficerAcademy(bool takeLoan = false, float loanRepaymentPercent = 0.15f, MilitaryBranch? branchOverride = null)
        {
            if (currentSoldier == null || currentSoldier.isDead || currentSoldier.isRetired) return;

            if (branchOverride.HasValue)
            {
                currentSoldier.militaryBranch = branchOverride.Value;
            }

            long tuitionCost = (currentSoldier.militaryBranch == MilitaryBranch.AirForce)
                ? EconomyData.AIRFORCE_ACADEMY_TUITION_COST
                : EconomyData.ARMY_ACADEMY_TUITION_COST;
            int programYears = (currentSoldier.militaryBranch == MilitaryBranch.AirForce)
                ? (EconomyData.AIRFORCE_ACADEMY_SEMESTERS / 2)
                : (EconomyData.ARMY_WAR_ACADEMY_SEMESTERS / 2);
            int programSemesters = (currentSoldier.militaryBranch == MilitaryBranch.AirForce)
                ? EconomyData.AIRFORCE_ACADEMY_SEMESTERS
                : EconomyData.ARMY_WAR_ACADEMY_SEMESTERS;
            string academyName = (currentSoldier.militaryBranch == MilitaryBranch.AirForce)
                ? "Imperial Flight Academy"
                : "Imperial War Academy";
            string sfTargetName = currentSoldier.specialForcesBranchName;

            if (currentSoldier.isOfficer)
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    "<color=#00AAFF>[ALREADY COMMISSIONED]</color> You already hold an active officer commission!");
                return;
            }
            if (currentSoldier.age > 35.0f)
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#FF8800><b>[AGE LIMIT EXCEEDED • MAX AGE 35]</b></color> Imperial War Academy regulations strictly cap candidate admission at <b>Age 35</b>. You are {currentSoldier.age:F1} years old and ineligible for cadet commissioning.");
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }
            if (currentSoldier.careerTrack == CareerTrack.MilitaryWarAcademy)
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#00AAFF>[ALREADY ENROLLED]</color> You are already attending the {programYears}-Year {academyName} (Semester {currentSoldier.academySemestersCompleted + 1}/{programSemesters})!");
                return;
            }

            // Admission Standards: Discipline (intellect) >= 55% and Physical Readiness (fitness) >= 55%
            bool meetsDiscipline = currentSoldier.intellect >= 55f;
            bool meetsFitness = currentSoldier.fitness >= 55f;

            // Limited seats check: Competitive Entrance Board (Quota)
            float applicantScore = (currentSoldier.intellect * 0.40f) + (currentSoldier.fitness * 0.35f) + (currentSoldier.meritRating * 0.25f);
            if (currentSoldier.origin == SoldierOrigin.MilitaryLegacy || currentSoldier.nobleTitle > NobleRank.Commoner)
            {
                applicantScore += 5f; // Legacy consideration bonus
            }
            bool seatsAvailable = applicantScore >= 55f;

            if (!meetsDiscipline || !meetsFitness || !seatsAvailable)
            {
                string reason = "";
                if (!meetsDiscipline) reason += $"Discipline/Tactics ({currentSoldier.intellect:F0}/55% min) ";
                if (!meetsFitness) reason += $"Physical Fitness ({currentSoldier.fitness:F0}/55% min) ";
                if (!seatsAvailable) reason += $"Competitive Seat Quota ({applicantScore:F0}/55 score required for limited admission seats) ";

                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#FF8800><b>[ACADEMY ADMISSION REJECTED • LIMITED SEATS]</b></color> The {academyName} Selection Board rejected your application. " +
                    $"Deficiencies: <b>{reason}</b>. Improve your stats and reapply next term!");
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

            // Tuition Payment check
            if (!takeLoan)
            {
                if (currentSoldier.cashBalance < tuitionCost)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                        $"<color=#FF8800><b>[INSUFFICIENT FUNDS]</b></color> You do not have ${tuitionCost:N0} cash for upfront tuition (Bank: ${currentSoldier.cashBalance:N0}). " +
                        "You can apply using the 9% Military Education Loan with salary deduction instead!");
                    return;
                }
                currentSoldier.cashBalance -= tuitionCost;
                currentSoldier.academyTuitionDebt = 0;
            }
            else
            {
                currentSoldier.academyTuitionDebt = tuitionCost;
                currentSoldier.academyLoanDeductionPercent = Mathf.Clamp(loanRepaymentPercent, 0.10f, 0.80f);
            }

            // Enroll in Academy
            currentSoldier.careerTrack = CareerTrack.MilitaryWarAcademy;
            currentSoldier.academySemestersCompleted = 0;
            currentSoldier.militarySchoolSemestersCompleted = 0;
            currentSoldier.assignedDepartment = academyName;
            currentSoldier.isInCombatDeployment = false;

            string paymentMethod = (!takeLoan) 
                ? $"Paid ${tuitionCost:N0} cash upfront" 
                : $"Approved for ${tuitionCost:N0} Education Loan at 9% annual interest ({currentSoldier.academyLoanDeductionPercent * 100f:F0}% monthly salary deduction)";

            GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                $"<color=#FFD700><b>[{academyName.ToUpper()} ADMISSION GRANTED • {programYears}-YEAR OFFICER PROGRAM]</b></color> " +
                $"You won a coveted seat at {academyName}! " +
                $"Tuition: <b>{paymentMethod}</b>. {programSemesters} semesters ({programYears} years) of tactical command training commence. " +
                $"Cadets in the <b>Top 10%</b> qualify for elite {sfTargetName} selection. Graduates receive direct O-1 commissions!");

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void ApplyForOfficerAcademy()
        {
            if (currentSoldier == null) return;
            long tuitionCost = (currentSoldier.militaryBranch == MilitaryBranch.AirForce)
                ? EconomyData.AIRFORCE_ACADEMY_TUITION_COST
                : EconomyData.ARMY_ACADEMY_TUITION_COST;
            bool canAffordCash = currentSoldier.cashBalance >= tuitionCost;
            ApplyForOfficerAcademy(takeLoan: !canAffordCash, loanRepaymentPercent: 0.15f);
        }

        public void RequestCOSpecialForcesRecommendation()
        {
            if (currentSoldier == null || currentSoldier.isDead || currentSoldier.isRetired) return;

            if (currentSoldier.isSpecialForces)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    "<color=#00FFAA>[ALREADY SPECIAL FORCES]</color> You are already a qualified Special Forces Operator!");
                return;
            }
            if (currentSoldier.age > 30.0f)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    $"<color=#FF8800><b>[AGE LIMIT EXCEEDED • MAX AGE 30]</b></color> Special Forces Selection regulations mandate candidates must be <b>Age 30 or younger</b>. You are {currentSoldier.age:F1} years old and ineligible for Special Forces selection.");
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }
            if (currentSoldier.isSpecialForcesCandidate)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    $"<color=#FF8800>[TRAINING IN PROGRESS]</color> You are currently undergoing the Special Forces Qualification Course (Term {currentSoldier.specialForcesTrainingSemesters + 1}/2)!");
                return;
            }

            // Enlisted criteria: CO favor >= 70, Merit >= 60, Fitness >= 60, Combat >= 45
            bool meetsCO = currentSoldier.coFavor >= 70f;
            bool meetsMerit = currentSoldier.meritRating >= 60f;
            bool meetsFit = currentSoldier.fitness >= 60f;
            bool meetsCombat = currentSoldier.combatSkill >= 45f;

            if (meetsCO && meetsMerit && meetsFit && meetsCombat)
            {
                currentSoldier.hasCOSpecialForcesRecommendation = true;
                currentSoldier.isSpecialForcesCandidate = true;
                currentSoldier.specialForcesTrainingSemesters = 0;
                currentSoldier.isInCombatDeployment = false;

                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    "<color=#FFD700><b>[CO RECOMMENDATION APPROVED • SELECTION ORDERS ISSUED]</b></color> " +
                    "Your Commanding Officer signed your Special Forces candidate packet: " +
                    "<i>'This soldier has the exceptional combat lethality and discipline required for covert operations.'</i> " +
                    "You have been issued official orders to attend **Special Forces Assessment & Selection (SFAS)**! You must now face Phase 1: The Crucible and 70% attrition Hell Week.");
            }
            else
            {
                string missing = "";
                if (!meetsCO) missing += $"CO Favor ({currentSoldier.coFavor:F0}/70% min) ";
                if (!meetsMerit) missing += $"Merit ({currentSoldier.meritRating:F0}/60% min) ";
                if (!meetsFit) missing += $"Fitness ({currentSoldier.fitness:F0}/60% min) ";
                if (!meetsCombat) missing += $"Combat Skill ({currentSoldier.combatSkill:F0}/45% min) ";

                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    $"<color=#FF8800><b>[CO RECOMMENDATION DENIED]</b></color> Your CO reviewed your request and shook his head: " +
                    $"<i>'Not ready for the black ops group yet.'</i> Deficiencies: <b>{missing}</b>. Prove yourself on the frontlines first.");
            }

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void JoinSpecialForcesFromAcademy()
        {
            if (currentSoldier == null || currentSoldier.isDead || currentSoldier.isRetired) return;

            bool isEligibleCadet = (currentSoldier.careerTrack == CareerTrack.MilitaryWarAcademy && currentSoldier.academySemestersCompleted >= 2);
            bool isEligibleGraduate = currentSoldier.graduatedWarAcademy;

            if (currentSoldier.age > 30.0f)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    $"<color=#FF8800><b>[AGE LIMIT EXCEEDED • MAX AGE 30]</b></color> Special Forces Selection regulations mandate candidates must be <b>Age 30 or younger</b>. You are {currentSoldier.age:F1} years old and ineligible for Special Forces selection.");
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

            if (!isEligibleCadet && !isEligibleGraduate)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    "<color=#FF8800>[NOT ELIGIBLE]</color> Special Forces recruitment from the Academy requires Year 2 cadet standing or Academy graduation in the Top 10%.");
                return;
            }

            if (!currentSoldier.isAcademyTop10Percent && currentSoldier.academyPerformanceScore < 90f)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    $"<color=#FF8800>[NOT QUALIFIED]</color> Requires Top 10% class standing (Score >= 90%). Current: {currentSoldier.academyPerformanceScore:F1}%.");
                return;
            }

            currentSoldier.isSpecialForcesCandidate = true;
            currentSoldier.specialForcesTrainingSemesters = 0;
            currentSoldier.isInCombatDeployment = false;

            string branchSfName = currentSoldier.specialForcesBranchName;
            GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                $"<color=#FFD700><b>[SPECIAL FORCES SELECTION ORDERS ISSUED • {branchSfName.ToUpper()}]</b></color> " +
                $"You accepted the invitation to attend Selection for {branchSfName}! " +
                "You reported to the Hoffman Special Warfare Training Center as a candidate. You must now face Phase 1: The Crucible and 70% attrition Hell Week!");

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        /// <summary>
        /// Manages the grueling 2-phase Special Forces Selection and Qualification pipeline.
        /// Phase 1: SFAS Assessment & The Crucible (Hell Week & Attrition Test)
        /// Phase 2: Q-Course Tactical Final & SERE Survival (Earning the Beret & Wings)
        /// </summary>
        public void ProcessSpecialForcesCandidateProgression()
        {
            if (currentSoldier == null || !currentSoldier.isSpecialForcesCandidate) return;

            currentSoldier.consecutiveCombatSemesters = 0;
            currentSoldier.isInCombatDeployment = false;

            if (currentSoldier.specialForcesTrainingSemesters == 0)
            {
                TriggerCrucibleSelectionDilemma();
            }
            else
            {
                CompleteSpecialForcesQCourse();
            }
        }

        /// <summary>
        /// Triggers the interactive Crucible Dilemma testing candidate physical grit and determination.
        /// </summary>
        public void TriggerCrucibleSelectionDilemma()
        {
            if (currentSoldier == null) return;

            string branchSfName = currentSoldier.specialForcesBranchName;
            string cat = $"SPECIAL FORCES SELECTION • {branchSfName.ToUpper()}";
            string title = "THE CRUCIBLE: 40-MILE RUCK & THE BRASS BELL";
            string desc =
                $"<color=#FF8844><b>DAY 19 • SPECIAL FORCES ASSESSMENT & SELECTION (SFAS):</b></color>\n\n" +
                $"For three weeks, candidates have been marched with 75-lb rucks, navigated freezing mountain swamps, and slept under two hours a night. " +
                $"Crucible attrition has already eliminated over <b>65% of your class</b>.\n\n" +
                $"On the final 40-mile mountain march across Hoffman Ridge, your feet are bloody raw blisters and your body screams to collapse. " +
                $"The Cadre Instructor stands beside the Brass Bell on the trailside:\n" +
                $"<i>'Just ring the bell three times, candidate. Warm shower, hot chow, clean sheets, and no shame. Your conventional unit is waiting for you.'</i>\n\n" +
                $"What is your decision?";

            string choiceEndure = "⚔️ REFUSE TO QUIT: DRIVE INTO THE NIGHT!\n<size=75%>(Endure the agony • Requires Fitness ≥ 65 & Discipline ≥ 55)</size>";
            string choiceDOR = "🔔 RING THE BRASS BELL (DROP ON REQUEST)\n<size=75%>(Withdraw with honor to regular conventional forces)</size>";

            var dilemma = DilemmaModal.Instance ?? Object.FindAnyObjectByType<DilemmaModal>(FindObjectsInactive.Include);
            if (dilemma != null)
            {
                AudioManager.PlayHeartbeat();
                dilemma.DisplayDilemma(
                    cat,
                    title,
                    desc,
                    choiceEndure,
                    choiceDOR,
                    null,
                    () => ResolveCrucibleTrial(true),
                    () => ResolveCrucibleTrial(false),
                    null
                );
            }
            else
            {
                // Headless/batchmode fallback
                ResolveCrucibleTrial(true);
            }
        }

        /// <summary>
        /// Resolves the candidate's Crucible trial: evaluates grit vs physical collapse.
        /// </summary>
        public void ResolveCrucibleTrial(bool refuseToQuit)
        {
            if (currentSoldier == null) return;

            string branchSfName = currentSoldier.specialForcesBranchName;

            if (!refuseToQuit)
            {
                // Voluntary Drop on Request (DOR)
                AudioManager.PlayAlertPing();
                currentSoldier.isSpecialForcesCandidate = false;
                currentSoldier.isSpecialForces = false;
                currentSoldier.specialForcesWashoutCount++;

                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES SELECTION",
                    $"<color=#888888><b>[VOLUNTARY WITHDRAWAL • DROPPED ON REQUEST]</b></color> You stepped forward and rang the brass bell three times. The cadre took your candidate ruck. Reassigned with dignity to regular conventional military service.");
                LifePersistenceManager.SaveSoldierLife(currentSoldier);
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

            // Evaluate grit based on Physical Fitness, Discipline, and Combat Skill
            float gritScore = (currentSoldier.fitness * 0.40f) + (currentSoldier.discipline * 0.35f) + (currentSoldier.combatSkill * 0.25f) + Random.Range(-5f, 10f);

            if (gritScore >= 56f && currentSoldier.health > 25f)
            {
                // SURVIVED THE CRUCIBLE!
                AudioManager.PlayBugleFanfare();
                currentSoldier.specialForcesTrainingSemesters = 1;
                currentSoldier.fitness = Mathf.Min(100f, currentSoldier.fitness + 10f);
                currentSoldier.combatSkill = Mathf.Min(100f, currentSoldier.combatSkill + 12f);
                currentSoldier.discipline = Mathf.Min(100f, currentSoldier.discipline + 12f);
                currentSoldier.squadRespect = Mathf.Min(100f, currentSoldier.squadRespect + 20f);

                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES SELECTION",
                    $"<color=#00FFAA><b>[CRUCIBLE CONQUERED • 70% ATTRITION SURVIVED]</b></color> " +
                    $"You gritted your teeth through the freezing darkness, refusing to ring the bell! Out of 130 hopefuls, only 34 made it past the Crucible. " +
                    $"You have been officially selected to advance to Phase 2: Advanced Weapons, Foreign Tactics & SERE Survival!");
            }
            else
            {
                // WASHOUT DUE TO PHYSICAL COLLAPSE
                AudioManager.PlayAlertPing();
                currentSoldier.isSpecialForcesCandidate = false;
                currentSoldier.isSpecialForces = false;
                currentSoldier.specialForcesWashoutCount++;
                currentSoldier.fitness = Mathf.Max(30f, currentSoldier.fitness - 5f);

                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES SELECTION",
                    $"<color=#FF8800><b>[SELECTION ATTRITION • MEDICALLY DROPPED]</b></color> " +
                    $"On Mile 34, severe hypothermia and torn ligaments collapsed your legs. The cadre medical team pulled you from the course for your own safety. " +
                    $"Like 70% of candidates who attempt Selection, you washed out of the Crucible. Reassigned with honor to conventional forces. You may rebuild your conditioning and re-apply later!");
            }

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        /// <summary>
        /// Evaluates Phase 2 of the Special Forces Qualification Course.
        /// </summary>
        public void CompleteSpecialForcesQCourse()
        {
            if (currentSoldier == null) return;

            float qScore = (currentSoldier.intellect * 0.35f) + (currentSoldier.combatSkill * 0.35f) + (currentSoldier.fitness * 0.30f) + Random.Range(0f, 10f);

            if (qScore >= 50f)
            {
                // GRADUATED AS SPECIAL FORCES OPERATOR!
                AudioManager.PlayBugleFanfare();
                currentSoldier.isSpecialForcesCandidate = false;
                currentSoldier.isSpecialForces = true;

                string sfName = currentSoldier.specialForcesBranchName;
                string sfRole = currentSoldier.specialForcesRoleTitle;
                string sfMedal = (currentSoldier.militaryBranch == MilitaryBranch.AirForce) 
                    ? "Celestial Valkyrie Wings" 
                    : "Iron Vanguard Black Beret";

                currentSoldier.AddTrait(sfRole);
                currentSoldier.AddTrait("Special Forces Operator");
                currentSoldier.AddMedal(sfMedal);

                // Immense Respect & Social Standing surge
                currentSoldier.squadRespect = Mathf.Min(100f, currentSoldier.squadRespect + 35f);
                currentSoldier.meritRating = Mathf.Min(100f, currentSoldier.meritRating + 25f);
                currentSoldier.politicalInfluence = Mathf.Min(100f, currentSoldier.politicalInfluence + 20f);
                currentSoldier.coFavor = Mathf.Min(100f, currentSoldier.coFavor + 20f);

                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES GRADUATION",
                    $"<color=#FFD700><b>[SPECIAL FORCES GRADUATION • EARNED THE BERET & WINGS]</b></color> " +
                    $"Against crushing 75% selection attrition, you survived the Crucible and completed the entire Qualification Course! " +
                    $"Inducted into <b>{sfName}</b> as an elite <b>{sfRole}</b> and pinned with the <i>{sfMedal}</i>!\n" +
                    $"<b>Special Forces Prestige Aura Activated:</b> Regular soldiers, squadmates, and officers look up to you with legendary respect (+35 Squad Respect, +25 Merit Rating, +20 CO Favor)! " +
                    $"You now draw an exclusive <b>+$800/mo Elite Combat Stipend</b>.");
            }
            else
            {
                // FAILED FINAL TACTICAL PHASE
                AudioManager.PlayAlertPing();
                currentSoldier.isSpecialForcesCandidate = false;
                currentSoldier.isSpecialForces = false;
                currentSoldier.specialForcesWashoutCount++;

                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES SELECTION",
                    $"<color=#FF8800><b>[Q-COURSE ATTRITION • FAILED FINAL TACTICAL PHASE]</b></color> " +
                    $"You were compromised during the final unconventional warfare exercise and failed to meet tactical qualification benchmarks. " +
                    $"Reassigned with honor to conventional military service.");
            }

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void SetAcademyLoanDeductionPercent(float percent)
        {
            if (currentSoldier == null) return;
            currentSoldier.academyLoanDeductionPercent = Mathf.Clamp(percent, 0.10f, 0.80f);
            GameEvents.OnFeedEventLogged?.Invoke("FINANCE",
                $"<color=#00FFAA>[LOAN TERMS UPDATED]</color> Academy tuition loan monthly salary deduction set to <b>{currentSoldier.academyLoanDeductionPercent * 100f:F0}%</b> (Min 10%).");
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        /// <summary>
        /// Sends money home to improve the family's living conditions during the wartime economy.
        /// </summary>
        public void SendMoneyToFamily(long amount)
        {
            if (currentSoldier == null || currentSoldier.cashBalance < amount || amount <= 0)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    "<color=#FF8800>[INSUFFICIENT FUNDS]</color> You do not have enough cash to send this remittance home.");
                return;
            }

            currentSoldier.cashBalance -= amount;
            currentSoldier.totalSentToFamily += amount;

            // Improve family living condition (0-100)
            float improvement = (amount / 300f);
            currentSoldier.familyLivingCondition = Mathf.Min(100f, currentSoldier.familyLivingCondition + improvement);
            currentSoldier.clanFavor = Mathf.Min(100f, currentSoldier.clanFavor + 6f);

            string tierStr;
            if (currentSoldier.familyLivingCondition >= 80f) tierStr = "<color=#FFD700>PROMINENT LOCAL NOBILITY</color>";
            else if (currentSoldier.familyLivingCondition >= 55f) tierStr = "<color=#00FFAA>AFFLUENT & COMFORTABLE</color>";
            else if (currentSoldier.familyLivingCondition >= 35f) tierStr = "<color=#00AAFF>STABLE WORKING CLASS</color>";
            else tierStr = "<color=#FF8844>IMPOVERISHED WARTIME HOUSEHOLD</color>";

            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                $"<color=#00FFAA><b>[FAMILY REMITTANCE SENT]</b></color> You sent <b>${amount:N0}</b> home to your family! " +
                $"Your parents bought proper medicine and nutrition for your younger siblings. " +
                $"Family Living Condition: <b>{tierStr} ({currentSoldier.familyLivingCondition:F0}/100)</b>.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void PromoteSoldier(MilitaryRankSO nextRank)
        {
            if (currentSoldier == null || nextRank == null || currentSoldier.isRetired || currentSoldier.isDead) return;

            AudioManager.PlayBugleFanfare();

            currentSoldier.currentRank = nextRank;
            currentSoldier.totalPromotionsEarned++;
            currentSoldier.accumulatedPromotionPoints = 0;
            currentSoldier.timeInCurrentRankSemesters = 0;
            currentSoldier.wasPromotedThisYear = true;
            currentSoldier.longevityMonthlyPayBonus = 0L;

            GameEvents.OnFeedEventLogged?.Invoke("PROMOTION",
                $"<color=#00FFAA><b>[PROMOTION APPROVED!]</b></color> You have been promoted to <b>{nextRank.rankName} ({nextRank.paygrade})</b>! " +
                $"New Base Salary: ${nextRank.baseMonthlySalary:N0}/mo.");

            if (nextRank.unlocksSelfTalentTesting && !currentSoldier.isPersonalTalentRevealed)
            {
                currentSoldier.isPersonalTalentRevealed = true;
                GameEvents.OnFeedEventLogged?.Invoke("CLEARANCE",
                    "<color=#00AAFF>[NCO CLEARANCE GRANTED]</color> Reaching rank E-4 certifies you as a Non-Commissioned Officer (NCO) with full service jacket review clearance!");
            }

            if (currentSoldier.totalPromotionsEarned == 3 && !currentSoldier.isMarried)
            {
                GameEvents.OnThreePromotionsReached?.Invoke();
                GameEvents.OnFeedEventLogged?.Invoke("MARRIAGE",
                    "<color=#FF88FF><b>[FAMILY ARRANGED MARRIAGE]</b></color> Having proven your military capability with 3 promotions, your parents have formally arranged a marriage alliance for you!");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public PowerTierSO GetPowerTierSO(int tierLevel)
        {
            if (powerTierDatabase == null) return null;
            foreach (var tier in powerTierDatabase)
            {
                if (tier != null && tier.tierLevel == tierLevel) return tier;
            }
            return null;
        }

        public FamilyLineageSO GetDefaultLineage()
        {
            if (familyLineageDatabase != null && familyLineageDatabase.Length > 0)
            {
                return familyLineageDatabase[0];
            }
            return null;
        }

        public void BuyGearItem(MilitaryGearItem item)
        {
            if (currentSoldier == null || item == null) return;
            if (currentSoldier.cashBalance < item.price)
            {
                GameEvents.OnFeedEventLogged?.Invoke("PX STORE",
                    $"<color=#FF8800>[INSUFFICIENT FUNDS]</color> You need ${item.price:N0} to purchase {item.itemName} (Balance: ${currentSoldier.cashBalance:N0}).");
                return;
            }

            currentSoldier.cashBalance -= item.price;
            currentSoldier.AddGear(item);

            GameEvents.OnFeedEventLogged?.Invoke("PX STORE",
                $"<color=#00FFAA><b>[PX QUARTERMASTER PURCHASE]</b></color> You purchased <b>{item.itemName}</b> for <b>${item.price:N0}</b>! " +
                $"{item.description}");

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void TransferWarTheater(WarTheaterType newTheater)
        {
            if (currentSoldier == null || currentSoldier.isDead || currentSoldier.isRetired) return;
            if (currentSoldier.currentTheater == newTheater) return;

            currentSoldier.currentTheater = newTheater;
            currentSoldier.toursInCurrentTheater = 0;
            var info = WarTheaterInfo.GetInfo(newTheater);

            GameEvents.OnFeedEventLogged?.Invoke("THEATER TRANSFER",
                $"<color=#FFD700><b>[THEATER TRANSFER APPROVED]</b></color> High Command reassigned your unit to <b>{info.theaterName}</b>! " +
                $"Environment: <i>{info.weatherAndEnvironment}</i>. {info.description}");

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void InteractWithSquadMate(SquadMate mate, string action)
        {
            if (currentSoldier == null || mate == null || mate.isDead) return;

            switch (action)
            {
                case "ShareCigarettes":
                    if (currentSoldier.cigarettesCount <= 0)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("BARRACKS",
                            "<color=#FF8800>[OUT OF CIGARETTES]</color> You don't have any cigarette packs left. Visit the PX store to buy more!");
                        return;
                    }
                    currentSoldier.cigarettesCount--;

                    if (currentSoldier.isSpecialForces)
                    {
                        mate.relationship = Mathf.Min(100f, mate.relationship + 20f);
                        currentSoldier.happiness = Mathf.Min(100f, currentSoldier.happiness + 12f);
                        currentSoldier.squadRespect = Mathf.Min(100f, currentSoldier.squadRespect + 5f);
                        GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                            $"<color=#FFD700><b>[SMOKE BREAK • OPERATOR REVERENCE]</b></color> You shared a smoke with {mate.GetDisplayName()} ({mate.roleTitle}). " +
                            $"He took the cigarette with visible awe: <i>'Always an honor to stand watch with an Operator. Every regular in the platoon sleeps easier knowing the Beret has our back.'</i> (+20 Relationship, +12 Morale, +5 Squad Respect).");
                    }
                    else
                    {
                        mate.relationship = Mathf.Min(100f, mate.relationship + 15f);
                        currentSoldier.happiness = Mathf.Min(100f, currentSoldier.happiness + 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                            $"<color=#00FFAA><b>[SMOKE BREAK • BROTHERS IN ARMS]</b></color> You shared a smoke with {mate.GetDisplayName()} ({mate.roleTitle}). " +
                            $"He spoke of home and the legend of Special Forces: <i>'Half the boys in the company want to try out for SF selection, but only men of pure iron survive the Crucible.'</i> (+15 Relationship, +10 Morale).");
                    }
                    break;

                case "BunkBanter":
                    if (currentSoldier.isSpecialForces)
                    {
                        mate.relationship = Mathf.Min(100f, mate.relationship + 12f);
                        currentSoldier.happiness = Mathf.Min(100f, currentSoldier.happiness + 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                            $"<color=#FFD700>[BARRACKS CARDS • SPECIAL FORCES LORE]</color> You played cards with {mate.GetDisplayName()}. " +
                            "Squadmates from neighboring bunks gathered around to hear stories about covert night raids and surviving the Hoffman Selection Ridge. (+12 Relationship, +10 Morale).");
                    }
                    else
                    {
                        mate.relationship = Mathf.Min(100f, mate.relationship + 8f);
                        currentSoldier.happiness = Mathf.Min(100f, currentSoldier.happiness + 8f);
                        GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                            $"<color=#00FFAA>[BARRACKS CARDS]</color> You played cards and joked with {mate.GetDisplayName()} in the mess area. (+8 Relationship, +8 Morale).");
                    }
                    break;

                case "TacticalDrill":
                    if (currentSoldier.isSpecialForces)
                    {
                        mate.relationship = Mathf.Min(100f, mate.relationship + 8f);
                        currentSoldier.combatSkill = Mathf.Min(100f, currentSoldier.combatSkill + 4f);
                        currentSoldier.fitness = Mathf.Min(100f, currentSoldier.fitness + 3f);
                        currentSoldier.squadRespect = Mathf.Min(100f, currentSoldier.squadRespect + 5f);
                        GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                            $"<color=#00FFAA>[OPERATOR COMBAT MASTERCLASS]</color> You drilled {mate.GetDisplayName()} on high-speed CQB room breaches and transition drills. " +
                            $"He wiped sweat from his face with admiration: <i>'I've learned more from you in one afternoon than six months of garrison!'</i> (+4 Combat, +3 Fitness, +5 Respect).");
                    }
                    else
                    {
                        mate.relationship = Mathf.Min(100f, mate.relationship + 5f);
                        currentSoldier.combatSkill = Mathf.Min(100f, currentSoldier.combatSkill + 2f);
                        currentSoldier.fitness = Mathf.Min(100f, currentSoldier.fitness + 2f);
                        GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                            $"<color=#00D4FF>[DRILL PARTNERS]</color> You ran room-clearing and weapon malfunction drills with {mate.GetDisplayName()}. (+2 Combat, +2 Fitness).");
                    }
                    break;
            }

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void StartVeteranCareer(VeteranCareer career)
        {
            if (currentSoldier == null || !currentSoldier.isRetired) return;

            currentSoldier.veteranCareer = career;
            string title;
            switch (career)
            {
                case VeteranCareer.UniversityGI: title = "GI Bill University Scholar"; break;
                case VeteranCareer.DefenseContractor: title = "Private Defense Contractor"; break;
                case VeteranCareer.CityMayor: title = "Elected City Mayor"; break;
                case VeteranCareer.RegionalGovernor: title = "Elected Provincial Governor"; break;
                case VeteranCareer.MinisterOfDefense: title = "National Minister of Defense"; break;
                default: title = "Retired Veteran"; break;
            }

            GameEvents.OnFeedEventLogged?.Invoke("CIVILIAN LIFE",
                $"<color=#FFD700><b>[POST-SERVICE CAREER BEGUN]</b></color> You inaugurated your civilian career as <b>{title}</b>! " +
                "Your military decorations and lifetime service inspire the nation.");

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void ContinueAsChildHeir()
        {
            if (currentSoldier == null || currentSoldier.childHeir == null) return;
            if (!currentSoldier.childHeir.isEligibleForEnlistment)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<color=#FF8800>[NOT YET AGE 18]</color> Your child {currentSoldier.childHeir.childName} is currently Age {currentSoldier.childHeir.age:F1}. They must reach Age 18 to enlist.");
                return;
            }

            string heirName = currentSoldier.childHeir.childName;
            string famName = currentSoldier.lastName;
            string gender = currentSoldier.childHeir.gender;
            long inheritedCash = currentSoldier.cashBalance / 2;
            int nextGen = currentSoldier.generationNumber + 1;
            List<string> medalsToInherit = new List<string>(currentSoldier.earnedMedals);

            StartNewLife(
                soldierName: $"{heirName} {famName}",
                lineage: null,
                gender: gender,
                birthCountry: currentSoldier.birthCountry,
                hairStyle: "Military Buzzcut",
                hairColor: "Dark Brown",
                eyeColor: "Steel Grey",
                skinTone: "Tan",
                looks: 85f,
                explicitTalent: null,
                origin: SoldierOrigin.MilitaryLegacy
            );

            currentSoldier.generationNumber = nextGen;
            currentSoldier.cashBalance += inheritedCash;
            currentSoldier.inheritedFamilyWealth = inheritedCash;
            currentSoldier.inheritedMedals = medalsToInherit;

            GameEvents.OnFeedEventLogged?.Invoke("GENERATIONAL DYNASTY",
                $"<color=#FFD700><b>[GENERATIONAL LEGACY • GENERATION {nextGen}]</b></color> " +
                $"As the child of a distinguished war veteran, <b>{currentSoldier.characterName}</b> stood before the recruitment officer on Enlistment Day! " +
                $"Inherited <b>${inheritedCash:N0}</b> family savings and your parent's combat service history (+Military Legacy traits).");

            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        #region Aviation & Flight School Engine
        public bool ApplyForAirForceFlightSchool()
        {
            if (currentSoldier == null) return false;
            bool result = AirForceFlightSchoolEngine.ApplyForFlightSchool(currentSoldier, this);
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
            return result;
        }

        public void GraduateFlightSchool(PilotSpecialization specialization)
        {
            if (currentSoldier == null) return;
            AirForceFlightSchoolEngine.GraduateFlightSchool(currentSoldier, specialization, this);
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public AviationSortieOutcome FlyAviationSortie(AviationDilemmaType? forcedDilemma = null, int choice = 0)
        {
            if (currentSoldier == null) return new AviationSortieOutcome();
            var outcome = AirForceFlightSchoolEngine.ExecuteAviationSortie(currentSoldier, forcedDilemma, choice, this);
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
            return outcome;
        }

        public bool AttendTopGunWeaponsSchool()
        {
            if (currentSoldier == null) return false;
            bool res = AirForceFlightSchoolEngine.EnrollInTopGunSchool(currentSoldier, this);
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
            return res;
        }
        #endregion

        #region Military Justice & Court-Martial (UCMJ)
        public bool ImposeArticle15(MilitaryInfractionType infraction)
        {
            if (currentSoldier == null) return false;
            bool res = MilitaryJusticeEngine.ImposeArticle15(currentSoldier, infraction, this);
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
            return res;
        }

        public CourtMartialTrialResult ConductCourtMartial(MilitaryInfractionType charge, bool hireEliteJagCounsel = false)
        {
            if (currentSoldier == null) return new CourtMartialTrialResult();
            var res = MilitaryJusticeEngine.ConductCourtMartial(currentSoldier, charge, hireEliteJagCounsel, this);
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
            return res;
        }

        public void ExecuteDischarge(DischargeCharacterization characterization, string reason)
        {
            if (currentSoldier == null) return;
            MilitaryJusticeEngine.ExecuteDischarge(currentSoldier, characterization, reason, this);
            LifePersistenceManager.SaveSoldierLife(currentSoldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }
        #endregion
    }
}
