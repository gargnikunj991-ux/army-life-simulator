using System.Collections.Generic;
using ArmyLife.Data;
using ArmyLife.Systems;
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

            // Direct Enlistment: Start at Age 18.0 in Year 2026 as Private Recruit (E-1)!
            currentSoldier.age = 18.0f;
            currentSoldier.totalSemestersLived = 0;
            currentSoldier.careerTrack = CareerTrack.MilitaryEnlisted;
            currentSoldier.currentRank = GetRankByPaygrade("E-1");

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

            // Save new life into persistent storage
            LifePersistenceManager.SaveSoldierLife(currentSoldier);

            string imperialBadge = isImperial ? " <color=#FFD700>[IMPERIAL CROWN DOMAIN]</color>" : "";
            GameEvents.OnFeedEventLogged?.Invoke("ENLISTMENT",
                $"<color=#00FFAA><b>[ENLISTMENT DAY • AGE 18.0]</b></color> You stood before the military commission in <b>{city}</b>, province of <b>{state}</b> ({birthCountry}){imperialBadge}, raised your right hand, and swore the oath of service! " +
                $"Assigned to active duty as a <b>{currentSoldier.currentRank?.rankName ?? "Private Recruit"} (E-1)</b>. " +
                $"Background Origin: <b>{origin}</b> (Trait: <i>{currentSoldier.originTrait}</i>). " +
                $"Appearance: <i>{hairStyle}, {hairColor} hair, {eyeColor} eyes, {skinTone} ({looks:F0}% Looks)</i>.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
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
                GameEvents.OnFeedEventLogged?.Invoke("SERVICE END",
                    $"<color=#FF4444><b>[SERVICE RECORD CONCLUDED]</b></color> {currentSoldier.characterName} has fallen in the line of duty ({currentSoldier.deathReason}). Review your career dossier or enlist a new recruit from the main menu.");
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

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

            // 2. War Academy / Special Forces Training vs Frontline Active Duty
            if (currentSoldier.careerTrack == CareerTrack.MilitaryWarAcademy)
            {
                currentSoldier.consecutiveCombatSemesters = 0;
                currentSoldier.isInCombatDeployment = false;
                CivilAndMilitaryCareerEngine.AdvanceCareerTurn(currentSoldier, this);
            }
            else if (currentSoldier.isSpecialForcesCandidate)
            {
                currentSoldier.consecutiveCombatSemesters = 0;
                currentSoldier.isInCombatDeployment = false;
                currentSoldier.specialForcesTrainingSemesters++;

                currentSoldier.combatSkill = Mathf.Min(100f, currentSoldier.combatSkill + Random.Range(8f, 14f));
                currentSoldier.fitness = Mathf.Min(100f, currentSoldier.fitness + Random.Range(6f, 10f));
                currentSoldier.discipline = Mathf.Min(100f, currentSoldier.discipline + 6f);

                if (currentSoldier.specialForcesTrainingSemesters == 1)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                        "<color=#FF8800><b>[SPECIAL FORCES QUALIFICATION • PHASE 1 (SELECTION & HELL DRILLS)]</b></color> " +
                        "You endured 6 months of brutal mountain survival, counter-terror tactics, and live-fire room clearing! (+Combat Skill, +Fitness). Final qualification term ahead.");
                }
                else if (currentSoldier.specialForcesTrainingSemesters >= 2)
                {
                    currentSoldier.isSpecialForcesCandidate = false;
                    currentSoldier.isSpecialForces = true;

                    string sfName = currentSoldier.specialForcesBranchName;
                    string sfRole = currentSoldier.specialForcesRoleTitle;
                    string sfMedal = (currentSoldier.militaryBranch == MilitaryBranch.AirForce) 
                        ? "Celestial Valkyrie Wings" 
                        : "Iron Vanguard Black Beret";

                    currentSoldier.AddTrait(sfRole);
                    currentSoldier.AddMedal(sfMedal);

                    GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                        $"<color=#FFD700><b>[SPECIAL FORCES GRADUATION • {sfName.ToUpper()}]</b></color> " +
                        $"You conquered the grueling 1-year selection course! Inducted into <b>{sfName}</b> as a <b>{sfRole}</b> and awarded the <i>{sfMedal}</i>! " +
                        "You now draw an exclusive <b>+$800/mo Elite Hazard Stipend</b>, but will deploy on the deadliest classified black-ops missions!");
                }
            }
            else if (currentSoldier.isInCombatDeployment)
            {
                currentSoldier.consecutiveCombatSemesters++;
                string extensionBadge = currentSoldier.isVoluntaryExtendedTour ? " <color=#FF8800>[VOLUNTARY EXTENDED TOUR]</color>" : "";
                string payBonusBadge = currentSoldier.hasExtendedTourPayBonus ? " <color=#FFD700>[+10% VETERAN COMBAT PAY ACTIVE]</color>" : "";

                var theater = WarTheaterInfo.GetInfo(currentSoldier.currentTheater);
                currentSoldier.toursInCurrentTheater++;

                // Protective Gear & Squad Scout bonuses
                float gearProtection = 0f;
                if (currentSoldier.hasSteelHelmet) gearProtection += 5f;
                if (currentSoldier.hasBallisticVest) gearProtection += 8f;
                if (currentSoldier.hasLuckyWatch) gearProtection += 4f;
                var scout = currentSoldier.squadMates.Find(m => m.role == SquadRole.LeadScout && !m.isDead);
                if (scout != null && scout.relationship >= 50f) gearProtection += 4f;

                if (currentSoldier.isSpecialForces)
                {
                    // Special Forces Covert Black-Ops Mission: High Pay, Higher Lethality Risk, Rapid Experience Scaling
                    GameEvents.OnFeedEventLogged?.Invoke("SPEC-OPS",
                        $"<color=#FF3333><b>[SPECIAL OPERATIONS OP • {theater.theaterName.ToUpper()}]</b></color>{extensionBadge}{payBonusBadge} " +
                        $"Covert penetration raid. Weather: <i>{theater.weatherAndEnvironment}</i>. Special hazard pay <b>+$800/mo</b> active.");

                    int rankOrder = currentSoldier.currentRank != null ? currentSoldier.currentRank.rankOrder : 1;
                    float rankExpBonus = rankOrder * 1.5f;
                    float skillMitigation = currentSoldier.combatSkill * 0.35f;
                    float sfCasualtyThreshold = Mathf.Clamp((48f * theater.baseHazardModifier) - rankExpBonus - skillMitigation - gearProtection, 10f, 55f);

                    float sfRoll = Random.value * 100f;
                    if (sfRoll < sfCasualtyThreshold)
                    {
                        float woundDmg = Random.Range(20f, 45f);
                        currentSoldier.health = Mathf.Max(0f, currentSoldier.health - woundDmg);

                        if (currentSoldier.health <= 0f)
                        {
                            var medic = currentSoldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);
                            if (medic != null && medic.relationship >= 40f && Random.value < 0.65f)
                            {
                                currentSoldier.health = 20f;
                                medic.relationship = Mathf.Min(100f, medic.relationship + 15f);
                                medic.battlesSurvived++;
                                currentSoldier.AddMedal("Imperial Wound Star");
                                GameEvents.OnFeedEventLogged?.Invoke("MEDIC RESCUE",
                                    $"<color=#00FFAA><b>[HEROIC MEDIC RESCUE • BROTHERS IN ARMS]</b></color> You collapsed from a fatal bullet wound! " +
                                    $"{medic.GetDisplayName()} charged through enemy fire, dragged you behind cover, and applied arterial tourniquets! " +
                                    "Your life was saved! (Health restored to 20%).");
                            }
                            else if (currentSoldier.fieldMedkitsCount > 0)
                            {
                                currentSoldier.fieldMedkitsCount--;
                                currentSoldier.health = 18f;
                                currentSoldier.AddMedal("Imperial Wound Star");
                                GameEvents.OnFeedEventLogged?.Invoke("FIELD TRAUMA",
                                    "<color=#00FFAA><b>[EMERGENCY MEDKIT CONSUMED]</b></color> Facing fatal blood loss, you rammed your emergency PX trauma syrette into the wound (-1 Medkit). You survived with 18% Health!");
                            }
                            else
                            {
                                currentSoldier.isDead = true;
                                currentSoldier.deathReason = $"Killed in Action (KIA) during a high-stakes Special Forces night assault in {theater.theaterName}";
                                currentSoldier.AddMedal("Special Operations Valor Cross (Posthumous)");
                                GameEvents.OnFeedEventLogged?.Invoke("KIA",
                                    $"<color=#FF0000><b>[KILLED IN ACTION • SPECIAL FORCES]</b></color> Ambushed behind enemy lines in {theater.theaterName}. You sacrificed your life so your team could escape. Posthumously awarded the <b>Special Operations Valor Cross</b>.");
                                LifePersistenceManager.SaveSoldierLife(currentSoldier);
                                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                                return;
                            }
                        }
                        else
                        {
                            currentSoldier.AddMedal("Imperial Wound Star");
                            GameEvents.OnFeedEventLogged?.Invoke("CASUALTY",
                                $"<color=#FF6644>[SPEC-OPS CASUALTY]</color> Wounded by enemy fire during an extraction (-{woundDmg:F0} Health). Awarded <b>Imperial Wound Star</b>.");
                        }
                    }
                    else
                    {
                        currentSoldier.specialForcesMissionsSurvived++;
                        float expGain = Random.Range(7f, 13f) + (rankOrder * 0.8f);
                        currentSoldier.combatSkill = Mathf.Min(100f, currentSoldier.combatSkill + expGain);
                        currentSoldier.meritRating = Mathf.Min(100f, currentSoldier.meritRating + Random.Range(5f, 10f));

                        if (currentSoldier.specialForcesMissionsSurvived == 3)
                        {
                            currentSoldier.AddMedal("Distinguished Commando Ribbon");
                        }
                        else if (currentSoldier.specialForcesMissionsSurvived == 6)
                        {
                            currentSoldier.AddMedal("Silver Star with V Device (Valor)");
                        }
                        else if (currentSoldier.specialForcesMissionsSurvived == 10)
                        {
                            currentSoldier.AddMedal("Imperial Legion of Merit");
                        }

                        GameEvents.OnFeedEventLogged?.Invoke("SPEC-OPS SUCCESS",
                            $"<color=#00FFAA><b>[SPECIAL OPERATIONS OBJECTIVE CONQUERED]</b></color> Target neutralized in {theater.theaterName}! " +
                            $"Missions Survived: <b>{currentSoldier.specialForcesMissionsSurvived}</b>. Tactical Experience: <b>+{expGain:F1} Combat Skill</b>, +Merit.");
                    }
                }
                else
                {
                    // Standard Frontline Operational Tour
                    GameEvents.OnFeedEventLogged?.Invoke("DEPLOYMENT",
                        $"<color=#FF4444><b>[OPERATIONAL TOUR • {theater.theaterName.ToUpper()}]</b></color>{extensionBadge}{payBonusBadge} " +
                        $"Your unit is holding the frontline (Continuous: <b>{currentSoldier.consecutiveCombatSemesters * 0.5f:F1} yrs</b> | Theater Tours: {currentSoldier.toursInCurrentTheater}). Earned <b>+$450 Hazard Combat Pay</b>.");

                    float survivalRoll = Random.value * 100f;
                    float casualtyThreshold = Mathf.Clamp((35f * theater.baseHazardModifier) - (currentSoldier.combatSkill * 0.25f) - gearProtection, 6f, 38f);

                    if (survivalRoll < casualtyThreshold)
                    {
                        float woundDmg = Random.Range(15f, 35f);
                        currentSoldier.health = Mathf.Max(0f, currentSoldier.health - woundDmg);

                        if (currentSoldier.health <= 0f)
                        {
                            var medic = currentSoldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);
                            if (medic != null && medic.relationship >= 40f && Random.value < 0.65f)
                            {
                                currentSoldier.health = 20f;
                                medic.relationship = Mathf.Min(100f, medic.relationship + 15f);
                                medic.battlesSurvived++;
                                currentSoldier.AddMedal("Imperial Wound Star");
                                GameEvents.OnFeedEventLogged?.Invoke("MEDIC RESCUE",
                                    $"<color=#00FFAA><b>[HEROIC MEDIC RESCUE • BROTHERS IN ARMS]</b></color> A high-explosive burst shattered your position! " +
                                    $"{medic.GetDisplayName()} charged through the smoke, dragged you behind sandbags, and stabilized your pulse! " +
                                    "Your life was saved! (Health restored to 20%).");
                            }
                            else if (currentSoldier.fieldMedkitsCount > 0)
                            {
                                currentSoldier.fieldMedkitsCount--;
                                currentSoldier.health = 18f;
                                currentSoldier.AddMedal("Imperial Wound Star");
                                GameEvents.OnFeedEventLogged?.Invoke("FIELD TRAUMA",
                                    "<color=#00FFAA><b>[EMERGENCY MEDKIT CONSUMED]</b></color> You slammed your emergency trauma syrette into your wound (-1 Medkit). You survived with 18% Health!");
                            }
                            else
                            {
                                currentSoldier.isDead = true;
                                currentSoldier.deathReason = $"Killed in Action (KIA) during heavy fighting in {theater.theaterName}";
                                currentSoldier.AddMedal("Imperial Wound Star (Posthumous)");
                                GameEvents.OnFeedEventLogged?.Invoke("KIA",
                                    $"<color=#FF0000><b>[KILLED IN ACTION]</b></color> You gave your life for the nation in {theater.theaterName}. Awarded the <b>Imperial Wound Star</b> posthumously.");
                                LifePersistenceManager.SaveSoldierLife(currentSoldier);
                                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                                return;
                            }
                        }
                        else
                        {
                            currentSoldier.AddMedal("Imperial Wound Star");
                            GameEvents.OnFeedEventLogged?.Invoke("CASUALTY",
                                $"<color=#FF6644>[COMBAT CASUALTY]</color> Shrapnel struck your torso in {theater.theaterName}! (-{woundDmg:F0} Health). Field medics extracted you safely. Awarded <b>Imperial Wound Star</b>.");
                        }
                    }
                    else
                    {
                        currentSoldier.combatSkill = Mathf.Min(100f, currentSoldier.combatSkill + Random.Range(3f, 7f));
                        if (!currentSoldier.HasMedal("Combat Infantry Badge"))
                        {
                            currentSoldier.AddMedal("Combat Infantry Badge");
                        }
                    }
                }

                // Squadmates battle experience and bonding
                foreach (var mate in currentSoldier.squadMates)
                {
                    if (!mate.isDead)
                    {
                        mate.battlesSurvived++;
                        mate.relationship = Mathf.Min(100f, mate.relationship + Random.Range(2f, 5f));
                    }
                }
            }
            else
            {
                currentSoldier.consecutiveCombatSemesters = 0;

                // Garrison Duty: PT Drills & Recovery
                currentSoldier.fitness = Mathf.Min(100f, currentSoldier.fitness + Random.Range(3f, 6f));
                currentSoldier.health = Mathf.Min(100f, currentSoldier.health + 12f);
                GameEvents.OnFeedEventLogged?.Invoke("GARRISON",
                    $"<color=#00FFAA><b>[GARRISON DUTY & RECOVERY]</b></color> Stationed at Fort Valerius for refit, physical conditioning, and marksmanship drills. (+Health, +Fitness).");
            }

            // 3. Roll High-Stakes Tactical / Barracks Dilemma
            MilitaryDilemmaEngine.EvaluateAdultSemesterDilemma(currentSoldier, this);
            if (currentSoldier.isDead)
            {
                LifePersistenceManager.SaveSoldierLife(currentSoldier);
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

            // 4. Payday, US-Style Living Costs, Investments & Crime Heat Cycle
            EconomyEngine.ProcessSemesterEconomy(currentSoldier);
            CrimeEngine.ProcessSemesterHeatAndRaids(currentSoldier);

            // 5. Military Promotion Board Evaluation (Active Enlisted and Officers)
            if (currentSoldier.careerTrack != CareerTrack.MilitaryWarAcademy && !currentSoldier.isSpecialForcesCandidate)
            {
                PromotionBoardEngine.EvaluatePromotionBoard(currentSoldier, militaryRankDatabase);
            }

            // 5b. Annual Longevity Pay Step (Awarded each full year of service, unless promoted that year)
            if (currentSoldier.totalSemestersLived > 0 && currentSoldier.totalSemestersLived % 2 == 0)
            {
                if (currentSoldier.wasPromotedThisYear)
                {
                    // Soldier was promoted during this service year: promotion already provided rank pay jump
                    currentSoldier.wasPromotedThisYear = false;
                }
                else if (!currentSoldier.isRetired && !currentSoldier.isCivilian && currentSoldier.careerTrack != CareerTrack.MilitaryWarAcademy && !currentSoldier.isYouthPhase)
                {
                    // Soldier served a full year without promotion: award annual military longevity step raise!
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

            // 6. Check Victory Condition: 4-Star General (O-10)
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

            // 7. Check 20-Year Career Milestone (Honorable Retirement Eligibility)
            if (currentSoldier.totalSemestersLived >= 40 && !currentSoldier.HasTrait("20-Year Veteran"))
            {
                currentSoldier.AddTrait("20-Year Veteran");
                currentSoldier.AddMedal("Distinguished Service Cross");
                GameEvents.OnFeedEventLogged?.Invoke("CAREER MILESTONE",
                    "<color=#FFD700><b>[20 YEARS OF SERVICE REACHED]</b></color> You have achieved 20 years of active military duty! " +
                    "You are now eligible to take <b>Honorable Retirement</b> with a full lifetime military pension ($1,500/mo)!");
            }

            // 8. Post-Service Veteran Career Turn
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

            // 9. Generational Child Aging
            if (currentSoldier.childHeir != null)
            {
                currentSoldier.childHeir.age += 0.5f;
                if (Mathf.Approximately(currentSoldier.childHeir.age, 18.0f))
                {
                    GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                        $"<color=#00FFAA><b>[CHILD TURNS 18 • MILITARY ELIGIBLE]</b></color> Your child, <b>{currentSoldier.childHeir.childName}</b>, has reached Age 18 and is now eligible to enlist as a <b>Military Legacy</b> recruit!");
                }
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
                    "<color=#FFD700><b>[CO RECOMMENDATION APPROVED • SPECIAL FORCES RECRUITMENT]</b></color> " +
                    "Your Commanding Officer signed your Special Forces candidate packet: " +
                    "<i>'This soldier has the exceptional combat lethality and discipline required for covert operations.'</i> " +
                    "You have been transferred directly to the <b>1-Year Special Forces Qualification Course</b>!");
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

            if (currentSoldier.careerTrack != CareerTrack.MilitaryWarAcademy || currentSoldier.academySemestersCompleted < 2)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    "<color=#FF8800>[NOT ELIGIBLE]</color> Special Forces recruitment from the Academy is only available to Year 2 cadets.");
                return;
            }

            if (!currentSoldier.isAcademyTop10Percent)
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                    $"<color=#FF8800>[NOT QUALIFIED]</color> Requires Top 10% class standing (Score >= 90%). Current: {currentSoldier.academyPerformanceScore:F1}%.");
                return;
            }

            currentSoldier.isSpecialForcesCandidate = true;
            currentSoldier.specialForcesTrainingSemesters = 0;

            GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                "<color=#FFD700><b>[SPECIAL FORCES SELECTION ENTERED • TOP 10% CADET]</b></color> " +
                "You accepted the invitation from the Special Operations Group! " +
                "You entered the grueling 1-Year Special Forces Qualification Course alongside your officer tactical development.");

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
                    mate.relationship = Mathf.Min(100f, mate.relationship + 15f);
                    currentSoldier.happiness = Mathf.Min(100f, currentSoldier.happiness + 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                        $"<color=#00FFAA><b>[SMOKE BREAK • BROTHERS IN ARMS]</b></color> You shared a smoke with {mate.GetDisplayName()} ({mate.roleTitle}). " +
                        $"He spoke of his home and praised your loyalty. (+15 Relationship, +10 Happiness).");
                    break;

                case "BunkBanter":
                    mate.relationship = Mathf.Min(100f, mate.relationship + 8f);
                    currentSoldier.happiness = Mathf.Min(100f, currentSoldier.happiness + 8f);
                    GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                        $"<color=#00FFAA>[BARRACKS CARDS]</color> You played cards and joked with {mate.GetDisplayName()}. (+8 Relationship).");
                    break;

                case "TacticalDrill":
                    mate.relationship = Mathf.Min(100f, mate.relationship + 5f);
                    currentSoldier.combatSkill = Mathf.Min(100f, currentSoldier.combatSkill + 2f);
                    currentSoldier.fitness = Mathf.Min(100f, currentSoldier.fitness + 2f);
                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                        $"<color=#00D4FF>[DRILL PARTNERS]</color> You ran room-clearing and weapon malfunction drills with {mate.GetDisplayName()}. (+2 Combat, +2 Fitness).");
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
    }
}
