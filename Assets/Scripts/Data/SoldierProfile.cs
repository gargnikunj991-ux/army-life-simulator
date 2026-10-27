using System;
using System.Collections.Generic;
using UnityEngine;
using ArmyLife.Core;

namespace ArmyLife.Data
{
    public enum SoldierOrigin
    {
        Farmhand,           // +15% Health, +15% Fitness, starting trait: "Country Grit"
        StreetTough,        // +15% Combat Skill, -10% Discipline, starting trait: "Street Brawler"
        WorkingMechanic,    // +15% Intellect (Tactics/Technical), +$200 Cash, starting trait: "Field Craftsman"
        MilitaryLegacy      // +15% CO Favor, +15% Squad Respect, starting trait: "Soldier's Son"
    }

    public enum CareerTrack
    {
        YouthStudent,           // Ages 0 to 18.0
        // Military Tracks
        MilitaryRecruitBasic,   // 6 months (1 semester), UNPAID ($0), passes out as E-1 Private
        MilitaryEnlisted,       // E-1 to E-9 active duty
        MilitaryOfficer,        // O-1 to O-10 active duty
        MilitaryTechSchool,     // 2 years (4 semesters). Year 1 unpaid ($0), Year 2 paid $200/mo. Passes out as E-3 Specialist with tech stipends!
        MilitaryMedicalSchool,  // 3 years (6 semesters). Year 1 unpaid ($0), Years 2-3 paid $250/mo. Passes out as Combat Medic Officer!
        MilitaryWarAcademy,     // 4 years (8 semesters). PAID FROM DAY 1 ($300/mo). Officer cadet commissioning academy!
        // Civilian Tracks
        CivilianWork,           // Direct entry jobs $150/mo to $300/mo
        CivilianCollege,        // 4 years (8 semesters). Costs $100,000 for 4 years ($12,500/sem).
        CivilianProfessional    // Post-college degree careers starting at $500+/mo up to position!
    }

    /// <summary>
    /// Military Occupational Specialty (MOS) defining a soldier's primary battlefield role.
    /// </summary>
    public enum MilitaryMOS
    {
        InfantryRifleman11B = 0,     // 11B: Frontline ground assault, close-quarters combat (+15% Combat Skill)
        CombatMedic68W = 1,          // 68W: Field trauma medicine, saves squadmates from fatal wounds, lower casualty rate
        ScoutSniper19D = 2,          // 19D: Reconnaissance, early ambush warning, Nemesis counter-snipe bonus
        CombatEngineer12B = 3,       // 12B: Sapper, minefield clearing, IED detection & 50% blast reduction
        FighterPilotAce = 4,         // Air Force: Air superiority dogfighting, Close Air Support strafing runs
        PararescuePJ = 5             // Air Force: Combat Search and Rescue (CSAR) behind enemy lines, high valor awards
    }

    [Serializable]
    public partial class SoldierProfile
    {
        [Header("Identity & Background")]
        public string lifeId;
        public string characterName;
        public string firstName;
        public string lastName;
        public SoldierOrigin origin = SoldierOrigin.Farmhand;
        public string originTrait = "Country Grit";
        public string gender = "Male";
        public string birthCountry = "The Valerian Empire";
        public string birthState = "The Imperial Crownlands (Aethelgard)";
        public string birthCity = "Valeria Magna";
        public bool isBornInImperialState = true;
        public float age;                       // e.g. 18.0, 18.5, 19.0
        public int totalSemestersLived;         // Number of 6-month turns survived
        public bool isYouthPhase => age < 18.0f;
        public bool isRetired;                  // True if honorably discharged / retired to veteran life
        public bool isDead;                     // True if KIA, executed, or fatal combat injury
        public string deathReason;

        [Header("Appearance & Customization (BitLife style)")]
        public string hairStyle = "Military Buzzcut";
        public string hairColor = "Jet Black";
        public string eyeColor = "Steel Grey";
        public string skinTone = "Tan";
        [Range(0f, 100f)] public float looks = 80f;

        [Header("Medical Convalescence, Trauma & Amputation")]
        public bool inMedicalConvalescence;
        public int convalescenceSemestersRemaining;
        public bool hasLostLimb;
        public string amputatedLimbDescription = "";
        public bool hasCombatProsthetic;
        public bool isMedicallyRetired;
        public long medicalDisabilityPension = 0;
        public float combatStress = 0f; // 0 to 100 combat psychological stress
        public string activeNemesisName = "";
        public string activeNemesisTitle = "";
        public int activeNemesisBounty = 0;

        [Header("Lineage & Aristocracy")]
        public FamilyLineageSO lineage;
        public NobleRank nobleTitle;
        public bool isClanHead;                 // True if player won Clan Head seat (inherits Duke/Marquis title)
        public int clanSuccessionScore;         // Evaluated against siblings by Clan Elders

        [Header("Military Standing & Branch")]
        public MilitaryBranch militaryBranch = MilitaryBranch.Army;
        public AirForceRole airForceRole = AirForceRole.None;
        public bool hasAnsweredWartimeCall = false; // Set to true after player chooses their service branch
        public bool isFlightDuty => militaryBranch == MilitaryBranch.AirForce && airForceRole == AirForceRole.FlightDuty;
        public int flightHoursLogged = 0;
        public MilitaryRankSO currentRank;
        public int totalPromotionsEarned;       // At 3 promotions -> Triggers Arranged Marriage!
        public int accumulatedPromotionPoints;  // Earned through PT tests, combat tours, and merit
        public string assignedDepartment;       // "Infantry", "Technical & Cyber", "Combat Medic", "Aviation Squadron", "Air Defense"
        public bool isTechnicalSchoolGraduate;  // Grants guaranteed Tech ($450) + Cyber ($350) stipends
        public bool isInCombatDeployment;       // True during frontline tours (+Hazard Pay)
        public bool isVoluntaryExtendedTour;    // If true, soldier volunteers to stay on the battlefield longer than 6 months
        public int consecutiveCombatSemesters;  // Number of continuous 6-month frontline combat semesters
        public int garrisonSemestersInRow;      // Number of continuous 6-month garrison refit semesters
        public bool hasExtendedTourPayBonus => consecutiveCombatSemesters >= 2; // True if continuously deployed >= 1 year (> 2 semesters)
        public bool isOfficer => currentRank != null && currentRank.type == PaygradeType.Officer;
        public int medalsEarnedCount;
        public List<string> characterTraits = new List<string>();
        public List<string> earnedMedals = new List<string>();

        public void AddTrait(string traitName)
        {
            if (characterTraits == null) characterTraits = new List<string>();
            if (!characterTraits.Contains(traitName))
            {
                characterTraits.Add(traitName);
                GameEvents.OnFeedEventLogged?.Invoke("TRAIT", $"<color=#00D4FF><b>[NEW STORY TRAIT]</b></color> You acquired the trait: <b>{traitName}</b>!");
            }
        }

        public bool HasTrait(string traitName)
        {
            return characterTraits != null && characterTraits.Contains(traitName);
        }

        public void AddMedal(string medalName)
        {
            if (earnedMedals == null) earnedMedals = new List<string>();
            if (!earnedMedals.Contains(medalName))
            {
                earnedMedals.Add(medalName);
                medalsEarnedCount = earnedMedals.Count;
                if (medalName != null && medalName.Contains("Wound Star"))
                {
                    woundStarsCount++;
                }

                // Imperial Treasury Medal Cash Bounty
                long bounty = EconomyData.GetMedalCashReward(medalName);
                if (bounty > 0)
                {
                    cashBalance += bounty;
                    GameEvents.OnFeedEventLogged?.Invoke("HONOR",
                        $"<color=#FFD700><b>[MEDAL AWARDED • IMPERIAL BOUNTY]</b></color> You were decorated with the <b>{medalName}</b>! " +
                        $"The Imperial Treasury awarded you an official cash honorarium of <color=#00FFAA><b>+${bounty:N0}</b></color>!");
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("HONOR", $"<color=#FFD700><b>[MEDAL AWARDED]</b></color> You were decorated with the <b>{medalName}</b>!");
                }
            }
        }

        public string specialForcesBranchName => (militaryBranch == MilitaryBranch.AirForce) 
            ? EconomyData.AIRFORCE_SPECIAL_FORCES_NAME 
            : EconomyData.ARMY_SPECIAL_FORCES_NAME;

        public string specialForcesRoleTitle => (militaryBranch == MilitaryBranch.AirForce) 
            ? EconomyData.AIRFORCE_SPECIAL_FORCES_ROLE 
            : EconomyData.ARMY_SPECIAL_FORCES_ROLE;

        /// <summary>
        /// Returns the authentic branch-specific rank title (e.g. Ensign O-1 for Air Force, 2nd Lt O-1 for Army, or Recruit in Basic Training).
        /// </summary>
        public string GetRankDisplayName()
        {
            if (isDead) return "Fallen Hero (KIA)";
            if (isRetired) return $"[RETIRED] {(currentRank != null ? currentRank.rankName : "Veteran")}";

            if (careerTrack == CareerTrack.MilitaryRecruitBasic)
            {
                return (militaryBranch == MilitaryBranch.AirForce)
                    ? "Air Recruit (Basic Flight Training • 100% Covered)"
                    : "Recruit (Basic Combat Training • 100% Covered)";
            }

            if (careerTrack == CareerTrack.MilitaryWarAcademy)
            {
                string branchName = (militaryBranch == MilitaryBranch.AirForce) ? "Flight" : "War";
                return $"Officer Cadet ({branchName} Academy Year {(academySemestersCompleted / 2) + 1})";
            }

            if (isSpecialForcesCandidate)
            {
                return "Special Forces Candidate (Q-Course)";
            }

            if (currentRank == null)
            {
                return (militaryBranch == MilitaryBranch.AirForce) ? "Airman Basic (E-1)" : "Private Recruit (E-1)";
            }

            // Air Force specific designations:
            if (militaryBranch == MilitaryBranch.AirForce)
            {
                string afRank = currentRank.paygrade switch
                {
                    "E-1" => "Airman Basic (E-1)",
                    "E-2" => "Airman (E-2)",
                    "E-3" => "Airman First Class (E-3)",
                    "E-4" => "Senior Airman (E-4)",
                    "E-5" => "Staff Sergeant (E-5)",
                    "E-6" => "Technical Sergeant (E-6)",
                    "E-7" => "Master Sergeant (E-7)",
                    "E-8" => "Senior Master Sergeant (E-8)",
                    "E-9" => "Chief Master Sergeant (E-9)",
                    "O-1" => "Ensign / Flight Lieutenant (O-1)",
                    "O-2" => "1st Lieutenant (O-2)",
                    "O-3" => "Flight Captain (O-3)",
                    "O-4" => "Squadron Leader / Major (O-4)",
                    "O-5" => "Wing Commander / Lt Col (O-5)",
                    "O-6" => "Group Captain / Colonel (O-6)",
                    "O-7" => "Air Commodore / Brig Gen (O-7)",
                    "O-8" => "Air Vice-Marshal (O-8)",
                    "O-9" => "Air Marshal (O-9)",
                    "O-10" => "General of the Air Force (O-10)",
                    _ => $"{currentRank.rankName} ({currentRank.paygrade})"
                };

                return isSpecialForces ? $"[SF VALKYRIE OPERATOR] {afRank}" : afRank;
            }

            // Army designations:
            string armyRank = $"{currentRank.rankName} ({currentRank.paygrade})";
            return isSpecialForces ? $"[SF GREEN BERET OPERATOR] {armyRank}" : armyRank;
        }

        public bool HasMedal(string medalName)
        {
            return earnedMedals != null && earnedMedals.Contains(medalName);
        }

        public int GetWoundStarsCount()
        {
            int count = woundStarsCount;
            if (earnedMedals != null)
            {
                int starCount = 0;
                foreach (var m in earnedMedals)
                {
                    if (m != null && m.Contains("Wound Star")) starCount++;
                }
                if (starCount > count) count = starCount;
            }
            return count;
        }

        [Header("Career Track (Civilian vs Military Education & Employment)")]
        public CareerTrack careerTrack = CareerTrack.YouthStudent;
        public string civilianJobTitle = "Unemployed";
        public long civilianSalaryPerMonth = 0;
        public string collegeDegreeName;
        public int collegeSemestersCompleted;
        public long collegeTuitionDebt;
        public int militarySchoolSemestersCompleted;
        public bool graduatedCollege;
        public bool graduatedWarAcademy;
        public bool graduatedTechSchool;
        public bool graduatedMedicalSchool;
        public bool isCivilian => careerTrack == CareerTrack.CivilianWork || careerTrack == CareerTrack.CivilianCollege || careerTrack == CareerTrack.CivilianProfessional;

        [Header("Dynamic Economy, Housing & Investments")]
        public HousingTier housingTier = HousingTier.BarracksOrSlums;
        public FamilyHouseTier familyHouseTier = FamilyHouseTier.None;
        public int warBondsOwned = 0;
        public int tradeGuildShares = 0;
        public CommercialBusiness ownedBusiness = CommercialBusiness.None;
        public long businessRevenueLastSemester = 0;
        public long investmentDividendsLastSemester = 0;
        public long combatBountiesEarned = 0;
        public long vaDisabilityAllowancePerMonth = 0;
        public long calculatedPensionPerMonth = 0;
        public bool heirHasPrivateTutor = false;
        public bool isHeirTutoringEnrolled { get => heirHasPrivateTutor; set => heirHasPrivateTutor = value; }
        public bool hasBionicProsthetics = false;
        public int woundStarsCount = 0;
        public long totalMedicalExpensesPaid = 0;
        public long totalHousingExpensesPaid = 0;
        public long totalEducationExpensesPaid = 0;
        public long totalBusinessInvestment = 0;

        [Header("Underworld Crime & Law Enforcement Heat")]
        [Range(0f, 100f)] public float criminalHeat = 0f;
        public bool hasCriminalRecord = false;
        public bool isImprisoned = false;
        public int prisonSemestersRemaining = 0;
        public int militaryBrigSemestersRemaining = 0;
        public int crimesCommittedCount = 0;
        public int totalCrimesCommitted { get => crimesCommittedCount; set => crimesCommittedCount = value; }
        public long totalCrimeEarnings = 0;
        public bool committedCrimeThisSemester = false;

        [Header("War Academy (2-Year Officer Program)")]
        public int academySemestersCompleted;           // Exactly 4 semesters = 2 years
        [Range(0f, 100f)]
        public float academyPerformanceScore = 75f;     // 0-100 score; >= 90 is top 10%
        public bool isAcademyTop10Percent => academyPerformanceScore >= 90f;
        public long academyTuitionDebt;                 // Loan for $100,000 tuition
        public float academyLoanDeductionPercent = 0.15f; // Min 10% (0.10f) of monthly salary
        public const float AcademyLoanAnnualInterest = 0.09f; // 9% annual interest
        public const long ACADEMY_TUITION_COST = 100000L;

        [Header("Annual Longevity Pay Step (Raises for Non-Promoted Years)")]
        public bool wasPromotedThisYear = false;        // Tracked over the 2-semester service year
        public long longevityMonthlyPayBonus = 0L;      // Accumulated step increases on monthly gross pay
        public int annualLongevityRaisesEarned = 0;     // Number of annual step raises earned

        [Header("Special Forces Program")]
        public bool isSpecialForces;                    // Qualified elite operator (+$800 pay, higher risk)
        public bool isSpecialForcesCandidate;           // In 1-year (2 semesters) training
        public int specialForcesTrainingSemesters;       // 0 to 2 semesters
        public bool hasCOSpecialForcesRecommendation;    // Direct enlisted recommendation from commanding officer
        public int specialForcesMissionsSurvived;        // Tracks special operations completed
        public int specialForcesWashoutCount = 0;       // Times washed out of selection (can re-try if fitness builds up)

        [Header("Air Force Flight School & Aviation Specialization")]
        public PilotSpecialization pilotSpecialization = PilotSpecialization.None;
        public FlightSchoolStatus flightSchoolStatus = FlightSchoolStatus.None;
        public int flightSchoolSemestersEnrolled = 0;
        public bool isTopGunGraduate = false;
        public int airToAirKills = 0;
        public int antiArmorKills = 0;
        public int combatAirdropsCompleted = 0;
        public int emergencyEjectionsSurvived = 0;
        public bool hasSurvivedSereBehindLines = false;

        [Header("Military Justice & UCMJ")]
        public DischargeCharacterization dischargeCharacterization = DischargeCharacterization.None;
        public List<MilitaryChargeRecord> disciplinaryRecords = new List<MilitaryChargeRecord>();
        public bool isBarracksRestricted = false;
        public int barracksRestrictionDaysRemaining = 0;
        public bool hasForfeitedPayThisSemester = false;
        public bool isFortLeavenworthInmate = false;
        public int leavenworthSemestersRemaining = 0;
        public bool hasFelonyDischargeRecord = false;
        public int article15PunishmentsCount = 0;
        public int courtMartialConvictionsCount = 0;

        [Header("Brothers in Arms (Persistent Squadmates)")]
        public List<SquadMate> squadMates = new List<SquadMate>();
        public int squadMatesKIA;

        [Header("PX Field Gear & Tactical Inventory")]
        public List<MilitaryGearItem> inventory = new List<MilitaryGearItem>();
        public bool hasSteelHelmet;         // -15% wound casualty chance
        public bool hasBallisticVest;        // -20% wound casualty chance
        public bool hasServiceSidearm;       // +10 close-quarters combat skill
        public bool hasLuckyWatch;           // +10% survival roll bonus
        public int fieldMedkitsCount = 1;    // Consumable to save self or comrade
        public int cigarettesCount = 2;      // Consumable to raise morale & bond with squad

        [Header("Theaters of War")]
        public WarTheaterType currentTheater = WarTheaterType.WesternMudTrenches;
        public int toursInCurrentTheater;

        [Header("Post-Service Veteran Life & Politics")]
        public VeteranCareer veteranCareer = VeteranCareer.None;
        public long veteranPensionPerMonth = 1500L;
        public int politicalInfluencePoints;

        [Header("Generational Military Legacy")]
        public int generationNumber = 1;
        public ChildHeir childHeir;
        public List<string> inheritedMedals = new List<string>();
        public long inheritedFamilyWealth;

        public void InitializeSquadMatesIfEmpty()
        {
            if (squadMates == null) squadMates = new List<SquadMate>();
            if (squadMates.Count > 0) return;

            squadMates.Add(new SquadMate("Arthur Harris", "Doc", SquadRole.CombatMedic, "Squad Field Medic", "Compassionate & Calm Under Fire", 70f)
            {
                hometown = "Valerius City Hospital District",
                backstoryAndQuirk = "Former third-year medical resident who volunteered after his brother fell at Port Iron. Always keeps spare morphine and clean bandages tucked in his helmet band."
            });
            squadMates.Add(new SquadMate("Miller Vance", "Deadeye", SquadRole.LeadScout, "Point Scout", "Cynical & Exceptionally Vigilant", 60f)
            {
                hometown = "Blackwood Forest Frontier",
                backstoryAndQuirk = "Raised hunting elk in deep mountain snow. Rarely speaks more than five words at a time, but spots enemy tripwires and snipers before anyone else."
            });
            squadMates.Add(new SquadMate("Boran Kowalski", "Anvil", SquadRole.HeavyGunner, "Machine Gunner", "Gruff & Fearless", 55f)
            {
                hometown = "Ironport Foundry Ward",
                backstoryAndQuirk = "Steel mill blacksmith with iron shoulders. Carries the heavy machine gun like a rifle and dreams of opening an auto-repair shop when the war ends."
            });
            squadMates.Add(new SquadMate("Tommy Higgins", "Rookie", SquadRole.RiflemanRookie, "Junior Rifleman", "Eager, Nervous Farmboy", 65f)
            {
                hometown = "Oakhaven Agrarian Valley",
                backstoryAndQuirk = "18-year-old farmboy who enlisted on his birthday. Keeps a worn photograph of his mother and childhood dog in his breast pocket."
            });
        }

        public bool HasGear(string itemId)
        {
            if (inventory == null) return false;
            foreach (var item in inventory)
            {
                if (item != null && item.id == itemId) return true;
            }
            return false;
        }

        public void AddGear(MilitaryGearItem item)
        {
            if (inventory == null) inventory = new List<MilitaryGearItem>();
            inventory.Add(item);

            if (item.id == "gear_helmet") hasSteelHelmet = true;
            else if (item.id == "gear_vest") hasBallisticVest = true;
            else if (item.id == "gear_sidearm") hasServiceSidearm = true;
            else if (item.id == "gear_watch") hasLuckyWatch = true;
            else if (item.id == "gear_medkit") fieldMedkitsCount += item.count;
            else if (item.id == "gear_cig") cigarettesCount += item.count;
        }

        public void HaveChild(string name = null, string gender = null)
        {
            if (string.IsNullOrEmpty(gender)) gender = UnityEngine.Random.value > 0.5f ? "Male" : "Female";
            if (string.IsNullOrEmpty(name))
            {
                string[] maleNames = { "James", "Alexander", "Victor", "William", "Marcus", "Ethan" };
                string[] femaleNames = { "Eleanor", "Clara", "Sophia", "Victoria", "Evelyn", "Grace" };
                name = (gender == "Male") ? maleNames[UnityEngine.Random.Range(0, maleNames.Length)] : femaleNames[UnityEngine.Random.Range(0, femaleNames.Length)];
            }

            childHeir = new ChildHeir(name, gender, 0.5f);
            childrenCount++;
            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                $"<color=#00FFAA><b>[A CHILD IS BORN!]</b></color> Your spouse gave birth to a healthy baby {gender.ToLower()}, <b>{name} {lastName}</b>! " +
                "You swear to survive the war and build a peaceful world for your child.");
        }

        [Header("Grounded Military Attributes (0-100)")]
        [Range(0f, 100f)] public float health = 100f;
        [Range(0f, 100f)] public float physicalFitness = 50f;
        [Range(0f, 100f)] public float marksmanshipScore = 50f;
        [Range(0f, 100f)] public float tacticalIntellect = 50f;
        [Range(0f, 100f)] public float discipline = 60f;
        [Range(0f, 100f)] public float combatReadiness = 50f;
        [Range(0f, 100f)] public float combatSkill = 30f;
        [Range(0f, 100f)] public float leadership = 50f;
        [Range(0f, 100f)] public float meritRating = 50f;
        [Range(0f, 100f)] public float politicalInfluence = 10f;
        [Range(0f, 100f)] public float clanFavor = 50f;
        [Range(0f, 100f)] public float squadRespect = 50f;
        [Range(0f, 100f)] public float coFavor = 50f;
        [Range(0f, 100f)] public float happiness = 80f;

        [Header("Semester Action Economy (Action Points / Stamina)")]
        public int actionPointsRemaining = 3;
        public const int MAX_GARRISON_ACTION_POINTS = 3;
        public const int MAX_COMBAT_ACTION_POINTS = 1;
        public bool hasTrainedFlightThisSemester = false;

        public void ResetSemesterActionPoints()
        {
            actionPointsRemaining = isInCombatDeployment ? MAX_COMBAT_ACTION_POINTS : MAX_GARRISON_ACTION_POINTS;
            hasTrainedFlightThisSemester = false;
        }

        public bool TryConsumeActionPoint(string actionName = "")
        {
            if (actionPointsRemaining <= 0)
            {
                return false;
            }
            actionPointsRemaining--;
            return true;
        }

        // Backward-compatible attribute aliases
        public float fitness { get => physicalFitness; set => physicalFitness = value; }
        public float intellect { get => tacticalIntellect; set => tacticalIntellect = value; }
        public float foundationalTempering { get => combatReadiness; set => combatReadiness = value; }
        public float morale { get => happiness; set => happiness = value; }

        [Header("Legacy Cultivation Mechanics (Deprecated for Pure Military Sim)")]
        [Obsolete("Pre-tier stages are deprecated; use physicalFitness and combatReadiness.")]
        public int preTierStage = 1;
        [Obsolete("Meridian damage is deprecated; physical injuries are tracked via health/fitness.")]
        public float meridianDamage = 0f;
        [Obsolete("Secret cultivation caves are deprecated in realistic military sim.")]
        public bool hasDiscoveredSecretCave = false;
        [Obsolete("Fantasy talent score is deprecated; military aptitude uses marksmanshipScore/tacticalIntellect.")]
        public int talentScore = 50;
        [Obsolete("Personal talent reveal is deprecated; qualification scores are available upon examination.")]
        public bool isPersonalTalentRevealed = true;
        [Obsolete("Family talent reveal is deprecated.")]
        public bool isFamilyTalentRevealed = true;
        [Obsolete("Cultivation knowledge is deprecated in pure military sim.")]
        public bool knowsAboutCultivation = false;
        [Obsolete("Spouse talent is deprecated.")]
        public int spouseTalent = 0;
        [Obsolete("Power tiers are deprecated; military standing uses MilitaryRankSO and Paygrades.")]
        public int powerTier = 0;
        [Obsolete("Cultivation sub-tiers are deprecated.")]
        public PowerSubTier subTier = PowerSubTier.Early;
        [Obsolete("Tier progress percent is deprecated.")]
        public float tierProgressPercent = 0f;
        [Obsolete("Failed breakthroughs are deprecated.")]
        public int failedBreakthroughCount = 0;
        [Obsolete("Transcendent tiers are deprecated.")]
        public bool unlockedTiers11And12 = false;
        [Obsolete("Corrupted foundation is deprecated.")]
        public bool hasCorruptedFoundation = false;
        [Obsolete("Soul transfer is deprecated.")]
        public bool hasRejectedChildSoulTransfer = false;

        [Header("Knowledge & Family Well-Being")]
        public float familyLivingCondition = 20f; // 0=Impoverished, 40=Modest, 70=Comfortable, 100=Prominent
        public long totalSentToFamily;
        public long totalRemittancesSentToFamily { get => totalSentToFamily; set => totalSentToFamily = value; }
        public long totalRemittancesSent { get => totalSentToFamily; set => totalSentToFamily = value; }

        [Header("Treasury & Military Currency")]
        public long cashBalance;
        public long combatMeritPoints;          // Restricted military currency for equipment & prototypes

        [Header("Military Career Tracking")]
        public int timeInCurrentRankSemesters;
        public int promotionPoints { get => accumulatedPromotionPoints; set => accumulatedPromotionPoints = value; }
        public FamilyLineageSO familyLineage { get => lineage; set => lineage = value; }
        public string birthProvince { get => birthState; set => birthState = value; }
        public SchoolLevel currentSchoolLevel { get => schoolLevel; set => schoolLevel = value; }
        public SchoolTier currentSchoolTier { get => schoolTier; set => schoolTier = value; }
        public void InitializePeers() => InitializeSchoolPeersIfEligible();

        [Header("Military Occupational Specialty (MOS)")]
        public MilitaryMOS militaryMOS = MilitaryMOS.InfantryRifleman11B;

        [Header("Dynasty, Romance, Marriage & Succession")]
        public bool isMarried;
        public string spouseName;
        public NobleRank spouseNobleRank;
        public int childrenCount;
        public string romanticPartnerName = "";
        public CourtshipStage courtshipStage = CourtshipStage.Single;
        public string engagementRing = "";

        [Header("Veteran Affairs, Memoirs & Civic Office")]
        public int publishedMemoirsCount = 0;
        public string publishedMemoirTitle = "";
        public long warMemoirRoyaltiesPerSemester = 0L;
        public string civicOfficeHeld = "None";

        public SoldierProfile()
        {
            health = 100f;
            physicalFitness = 50f;
            marksmanshipScore = 50f;
            tacticalIntellect = 50f;
            discipline = 60f;
            combatReadiness = 50f;
            combatSkill = 30f;
            meritRating = 50f;
            politicalInfluence = 10f;
            clanFavor = 50f;
            squadRespect = 50f;
            coFavor = 50f;
            happiness = 80f;
            leadership = 50f;
            inMedicalConvalescence = false;
            convalescenceSemestersRemaining = 0;
            hasLostLimb = false;
            amputatedLimbDescription = "";
            hasCombatProsthetic = false;
            isMedicallyRetired = false;
            medicalDisabilityPension = 0;
            combatStress = 0f;
            activeNemesisName = "";
            activeNemesisTitle = "";
            activeNemesisBounty = 0;
        }

        [Header("Childhood & Youth Development (Ages 0 to 18)")]
        public int examScore;
        public string siblingRivalName = "Lucian";
        public float parentRelationship = 80f;
        public float siblingRelationship = 50f;

        [Header("Mock Exam & Exam Preparation (Age 17.5)")]
        public bool tookMockExam;
        public int mockExamScore;
        public string mockExamProjectedAcademy = "Recruit Depot";
        public bool receivedFatherExamBoost;
        public int examPreparationBonus;

        [Header("Interactive National Exam Performance (Age 18.0)")]
        public int examPhysicalScore;
        public int examTacticalScore;
        public int examMartialScore;
        public string childhoodRomanceInterest = "Evelyn";
        public bool participatedUndergroundRing;

        [Header("Clan & Dynastic Family (Feudal Relations)")]
        public List<ClanMember> clanMembers = new List<ClanMember>();

        [Header("School & Academy Peers (Ages 6 to 18)")]
        public string schoolName = "Not Enrolled";
        public SchoolLevel schoolLevel = SchoolLevel.None;
        public SchoolTier schoolTier = SchoolTier.City3rd;
        [Range(0f, 100f)] public float schoolPerformance = 70f;
        public float semesterStudyPerformanceGained = 0f; // Max 3.0 pts per semester
        public const float MAX_SEMESTER_STUDY_BOOST = 3.0f;
        [Range(0f, 100f)] public float primarySchoolScore = 65f; // Cumulative primary school performance (Ages 6-11)
        public string primarySchoolGraduationHonors = "Pending";
        public string schoolClub = "None";
        public int timesPromotedSchool;
        public int timesDemotedSchool;
        public bool isImperialScholarshipStudent;
        public float lastPromotionCheckAge;
        public List<SchoolPeer> schoolPeers = new List<SchoolPeer>();

        public static readonly string[] FATHER_FIRST_NAMES = new string[]
        {
            "Marcus", "Julian", "Thomas", "David", "Edward", "Arthur", "Robert", "Victor", "William", "James", "Joseph", "Charles", "Henry", "Walter"
        };

        public static readonly string[] MOTHER_FIRST_NAMES = new string[]
        {
            "Eleanor", "Sarah", "Clara", "Margaret", "Catherine", "Evelyn", "Beatrice", "Martha", "Elizabeth", "Victoria", "Alice", "Diana", "Grace", "Rose"
        };

        public static readonly string[] BROTHER_FIRST_NAMES = new string[]
        {
            "Lucian", "Liam", "Ethan", "Noah", "Oliver", "Lucas", "Alexander", "Nathan", "Gabriel", "Henry", "Caleb", "Leo", "Samuel", "Benjamin"
        };

        public static readonly string[] SISTER_FIRST_NAMES = new string[]
        {
            "Seraphina", "Chloe", "Emma", "Olivia", "Ava", "Sophia", "Isabella", "Grace", "Hannah", "Amelia", "Lily", "Zoe", "Clara", "Mia"
        };

        public static readonly string[] PARENT_PERSONALITIES = new string[]
        {
            "Warm & Devoted", "Strict & Honorable", "Gentle & Caring", "Disciplined & Demanding", "Quiet & Hardworking", "Supportive & Wise"
        };

        public static readonly string[] SIBLING_PERSONALITIES = new string[]
        {
            "Affectionate & Playful", "Competitive & Fiery", "Quiet & Studious", "Mischievous & Energetic", "Supportive & Loyal", "Rebellious & Bold"
        };

        /// <summary>
        /// Generates a procedural, BitLife-style family:
        /// - Variable parents: ~70% both parents, ~15% single mother, ~10% single father, ~5% orphan.
        /// - Variable siblings: 0 to 3 siblings, older or younger, randomized names and personalities.
        /// - No distant feudal elders or scheming cousins—strictly Parents, Siblings, and Friends.
        /// - In deterministic/legacy test mode, ensures standard testing expectations are satisfied.
        /// </summary>
        public void InitializeDefaultClanMembers(bool forceLegacyFixture = false)
        {
            if (clanMembers == null) clanMembers = new List<ClanMember>();
            if (clanMembers.Count > 0)
            {
                SyncImmediateFamilyMembers();
                return;
            }

            string famName = !string.IsNullOrEmpty(lastName) ? lastName : "Vance";
            NobleRank rank = (lineage != null) ? lineage.startingNobleRank : NobleRank.Commoner;
            int playerAge = Mathf.Max(0, Mathf.RoundToInt(age));

            if (forceLegacyFixture)
            {
                clanMembers.Add(new ClanMember($"Julian {famName}", ClanRelationType.Father, "Father", 46, 65f, rank, 2, "Strict & Honorable", 60f));
                clanMembers.Add(new ClanMember($"Eleanor {famName}", ClanRelationType.Mother, "Mother", 44, 85f, rank, 1, "Warm & Devoted", 85f));
                clanMembers.Add(new ClanMember($"Lucian {famName}", ClanRelationType.OlderBrother, "Older Brother (Rival Heir)", 21, 35f, rank, 1, "Jealous & Competitive", 15f));
                clanMembers.Add(new ClanMember($"Seraphina {famName}", ClanRelationType.YoungerSister, "Younger Sister", 15, 70f, rank, 0, "Affectionate", 50f));
                SyncImmediateFamilyMembers();
                return;
            }

            // Procedural BitLife-Style Family Generation:
            // 1. Variable Parentage:
            //    ~70% Both Parents
            //    ~15% Single Mother (Father absent/deceased)
            //    ~10% Single Father (Mother deceased)
            //    ~5% Orphaned
            int parentRoll = UnityEngine.Random.Range(0, 100);
            bool hasMother = parentRoll < 85; // 0..84 have mother
            bool hasFather = (parentRoll < 70) || (parentRoll >= 85 && parentRoll < 95); // 0..69 and 85..94 have father

            int motherAge = 0;
            if (hasMother)
            {
                motherAge = playerAge + UnityEngine.Random.Range(20, 36);
                string mName = MOTHER_FIRST_NAMES[UnityEngine.Random.Range(0, MOTHER_FIRST_NAMES.Length)] + " " + famName;
                string mTrait = PARENT_PERSONALITIES[UnityEngine.Random.Range(0, PARENT_PERSONALITIES.Length)];
                float mRel = UnityEngine.Random.Range(70f, 95f);
                float mGen = UnityEngine.Random.Range(60f, 90f);
                clanMembers.Add(new ClanMember(mName, ClanRelationType.Mother, "Mother", motherAge, mRel, rank, 1, mTrait, mGen));
            }

            if (hasFather)
            {
                int fatherAge = (motherAge > 0 ? motherAge : playerAge + 24) + UnityEngine.Random.Range(-2, 6);
                string fName = FATHER_FIRST_NAMES[UnityEngine.Random.Range(0, FATHER_FIRST_NAMES.Length)] + " " + famName;
                string fTrait = PARENT_PERSONALITIES[UnityEngine.Random.Range(0, PARENT_PERSONALITIES.Length)];
                float fRel = UnityEngine.Random.Range(55f, 90f);
                float fGen = UnityEngine.Random.Range(50f, 85f);
                clanMembers.Add(new ClanMember(fName, ClanRelationType.Father, "Father", fatherAge, fRel, rank, 2, fTrait, fGen));
            }

            // 2. Variable Siblings:
            //    ~30% 0 siblings (only child!)
            //    ~45% 1 sibling
            //    ~20% 2 siblings
            //    ~5% 3 siblings
            int sibRoll = UnityEngine.Random.Range(0, 100);
            int siblingCount = 0;
            if (sibRoll >= 30 && sibRoll < 75) siblingCount = 1;
            else if (sibRoll >= 75 && sibRoll < 95) siblingCount = 2;
            else if (sibRoll >= 95) siblingCount = 3;

            for (int i = 0; i < siblingCount; i++)
            {
                bool isBoy = UnityEngine.Random.value < 0.5f;
                string sFirst = isBoy 
                    ? BROTHER_FIRST_NAMES[UnityEngine.Random.Range(0, BROTHER_FIRST_NAMES.Length)]
                    : SISTER_FIRST_NAMES[UnityEngine.Random.Range(0, SISTER_FIRST_NAMES.Length)];
                string sFullName = $"{sFirst} {famName}";

                // Ensure unique name among siblings
                int attempts = 0;
                while (clanMembers.Exists(m => m.name == sFullName) && attempts < 10)
                {
                    sFirst = isBoy 
                        ? BROTHER_FIRST_NAMES[UnityEngine.Random.Range(0, BROTHER_FIRST_NAMES.Length)]
                        : SISTER_FIRST_NAMES[UnityEngine.Random.Range(0, SISTER_FIRST_NAMES.Length)];
                    sFullName = $"{sFirst} {famName}";
                    attempts++;
                }

                // Sibling age relative to player
                int sAge;
                if (playerAge <= 0)
                {
                    sAge = UnityEngine.Random.Range(1, 9);
                }
                else
                {
                    bool isOlder = UnityEngine.Random.value < 0.5f;
                    if (isOlder)
                    {
                        sAge = playerAge + UnityEngine.Random.Range(1, 7);
                    }
                    else
                    {
                        sAge = Mathf.Max(0, playerAge - UnityEngine.Random.Range(1, 7));
                    }
                }

                ClanRelationType relType;
                string roleTitle;
                if (isBoy)
                {
                    relType = (sAge > playerAge) ? ClanRelationType.OlderBrother : ClanRelationType.Brother;
                    roleTitle = (sAge > playerAge) ? "Older Brother" : (sAge == 0 ? "Baby Brother" : "Younger Brother");
                }
                else
                {
                    relType = (sAge < playerAge) ? ClanRelationType.YoungerSister : ClanRelationType.Sister;
                    roleTitle = (sAge > playerAge) ? "Older Sister" : (sAge == 0 ? "Baby Sister" : "Younger Sister");
                }

                string sTrait = SIBLING_PERSONALITIES[UnityEngine.Random.Range(0, SIBLING_PERSONALITIES.Length)];
                float sRel = UnityEngine.Random.Range(35f, 90f);
                float sGen = UnityEngine.Random.Range(30f, 75f);
                clanMembers.Add(new ClanMember(sFullName, relType, roleTitle, sAge, sRel, rank, 0, sTrait, sGen));
            }

            SyncImmediateFamilyMembers();
            SyncFamilyRelationshipAverages();
        }

        public void SyncFamilyRelationshipAverages()
        {
            if (clanMembers == null || clanMembers.Count == 0) return;
            var parents = clanMembers.FindAll(m => m.IsParent && m.isAlive);
            if (parents.Count > 0)
            {
                float total = 0f;
                foreach (var p in parents) total += p.relationship;
                parentRelationship = total / parents.Count;
            }
            var siblings = clanMembers.FindAll(m => m.IsSibling && m.isAlive);
            if (siblings.Count > 0)
            {
                float total = 0f;
                foreach (var s in siblings) total += s.relationship;
                siblingRelationship = total / siblings.Count;
            }
        }

        /// <summary>
        /// Called every semester turn advance to process BitLife-style dynamic family events:
        /// 1. Family member aging (all living members age with the player).
        /// 2. New sibling birth chance (if mother is alive and in childbearing age).
        /// 3. Old age passing of elderly parents (age 75+).
        /// </summary>
        public void ProcessFamilySemesterTurn()
        {
            if (clanMembers == null) clanMembers = new List<ClanMember>();

            // 1. Advance age of living family members (every full year / 2 semesters)
            if (totalSemestersLived % 2 == 0)
            {
                foreach (var member in clanMembers)
                {
                    if (member.isAlive)
                    {
                        member.age++;
                        // If newborn reaches 1 year, update role from "Baby" to "Younger"
                        if (member.age == 1 && member.roleTitle.Contains("Baby"))
                        {
                            member.roleTitle = member.relationType == ClanRelationType.Brother ? "Younger Brother" : "Younger Sister";
                        }
                    }
                }
            }

            // 2. Check for new sibling birth (BitLife style: "later in the life a sibling is born")
            CheckForNewSiblingBirth();

            // 3. Check for old age passing of elderly parents (age 75+)
            CheckForParentOldAgePassing();

            // 4. Keep spouse and children synced
            SyncImmediateFamilyMembers();
            SyncFamilyRelationshipAverages();
        }

        public bool CheckForNewSiblingBirth()
        {
            if (clanMembers == null) return false;

            // Conditions:
            // - Player is youth/young adult (age < 20)
            // - Mother exists, is alive, and within childbearing age (18 to 43)
            // - Living siblings < 5
            if (age >= 20.0f) return false;

            var mother = clanMembers.Find(m => m.relationType == ClanRelationType.Mother && m.isAlive);
            if (mother == null) return false;
            if (mother.age < 18 || mother.age > 43) return false;

            int livingSiblingCount = clanMembers.FindAll(m => m.IsSibling && m.isAlive).Count;
            if (livingSiblingCount >= 5) return false;

            // Semester chance: ~5% (translates to ~10% annual chance)
            float chance = (mother.age <= 36) ? 0.05f : 0.025f;
            if (UnityEngine.Random.value > chance) return false;

            bool isBoy = UnityEngine.Random.value < 0.5f;
            string famName = !string.IsNullOrEmpty(lastName) ? lastName : "Vance";
            string babyFirst = isBoy 
                ? BROTHER_FIRST_NAMES[UnityEngine.Random.Range(0, BROTHER_FIRST_NAMES.Length)]
                : SISTER_FIRST_NAMES[UnityEngine.Random.Range(0, SISTER_FIRST_NAMES.Length)];
            string fullName = $"{babyFirst} {famName}";

            int attempts = 0;
            while (clanMembers.Exists(m => m.name == fullName) && attempts < 10)
            {
                babyFirst = isBoy 
                    ? BROTHER_FIRST_NAMES[UnityEngine.Random.Range(0, BROTHER_FIRST_NAMES.Length)]
                    : SISTER_FIRST_NAMES[UnityEngine.Random.Range(0, SISTER_FIRST_NAMES.Length)];
                fullName = $"{babyFirst} {famName}";
                attempts++;
            }

            ClanRelationType relType = isBoy ? ClanRelationType.Brother : ClanRelationType.Sister;
            string role = isBoy ? "Baby Brother (Newborn)" : "Baby Sister (Newborn)";
            var baby = new ClanMember(fullName, relType, role, 0, UnityEngine.Random.Range(80f, 95f), nobleTitle, 0, "Adorable & Innocent", 50f);
            clanMembers.Add(baby);

            happiness = Mathf.Min(100f, happiness + 15f);

            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                $"<color=#FF88FF><b>[NEW SIBLING BORN • BITLIFE MOMENT]</b></color> Your mother, <b>{mother.name}</b>, gave birth to a healthy baby {(isBoy ? "boy" : "girl")}, <b>{fullName}</b>! " +
                $"You are now an older sibling to baby {babyFirst} (+15 Happiness)!");

            return true;
        }

        public void CheckForParentOldAgePassing()
        {
            if (clanMembers == null) return;

            foreach (var member in clanMembers)
            {
                if (member.isAlive && member.IsParent && member.age >= 75)
                {
                    float passChance = (member.age >= 85) ? 0.08f : 0.03f;
                    if (UnityEngine.Random.value < passChance)
                    {
                        member.isAlive = false;
                        member.roleTitle = $"{member.roleTitle} (Passed Away at Age {member.age})";
                        long inheritance = (long)UnityEngine.Random.Range(500, 2500);
                        cashBalance += inheritance;
                        combatStress = Mathf.Min(100f, combatStress + 10f);

                        GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                            $"<color=#AAAAAA><b>[REST IN PEACE]</b></color> Your {member.relationType.ToString().ToLower()}, <b>{member.name}</b>, has passed away peacefully of old age at Age {member.age}. " +
                            $"You received a keepsake and <b>${inheritance:N0}</b> inheritance from their estate.");
                    }
                }
            }
        }

        /// <summary>
        /// Manually triggers a new sibling birth for testing or deterministic life simulations.
        /// </summary>
        public ClanMember TriggerSiblingBirthForTest(bool isBrother = true, string customName = "")
        {
            if (clanMembers == null) clanMembers = new List<ClanMember>();
            string famName = !string.IsNullOrEmpty(lastName) ? lastName : "Vance";
            string babyFirst = !string.IsNullOrEmpty(customName) ? customName : (isBrother ? "Liam" : "Chloe");
            string fullName = $"{babyFirst} {famName}";
            ClanRelationType relType = isBrother ? ClanRelationType.Brother : ClanRelationType.Sister;
            string role = isBrother ? "Baby Brother (Newborn)" : "Baby Sister (Newborn)";
            var baby = new ClanMember(fullName, relType, role, 0, 85f, nobleTitle, 0, "Adorable & Innocent", 50f);
            clanMembers.Add(baby);

            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                $"<color=#FF88FF><b>[NEW SIBLING BORN • BITLIFE MOMENT]</b></color> A healthy baby {(isBrother ? "boy" : "girl")}, <b>{fullName}</b>, was born into the family!");

            return baby;
        }

        public void SyncImmediateFamilyMembers()
        {
            if (clanMembers == null) clanMembers = new List<ClanMember>();

            if (isMarried && !string.IsNullOrEmpty(spouseName))
            {
                if (!clanMembers.Exists(m => m.relationType == ClanRelationType.Spouse))
                {
                    clanMembers.Add(new ClanMember(spouseName, ClanRelationType.Spouse, "Beloved Military Spouse", Mathf.Max(18, (int)age), 85f, nobleTitle, 0, "Loving & Steadfast", 75f));
                }
            }

            if (childHeir != null && !string.IsNullOrEmpty(childHeir.childName))
            {
                var existingChild = clanMembers.Find(m => m.relationType == ClanRelationType.Child);
                if (existingChild == null)
                {
                    clanMembers.Add(new ClanMember(childHeir.childName, ClanRelationType.Child, $"Eldest Child (Age {(int)childHeir.age})", (int)childHeir.age, childHeir.relationship, nobleTitle, 0, "Affectionate & Curious", 50f));
                }
                else
                {
                    existingChild.age = (int)childHeir.age;
                    existingChild.roleTitle = $"Eldest Child (Age {(int)childHeir.age})";
                    existingChild.relationship = childHeir.relationship;
                }
            }
        }

        public void InitializeStartingSchoolTier()
        {
            if (isBornInImperialState)
            {
                if (nobleTitle >= NobleRank.Marquis)
                    schoolTier = SchoolTier.ImperialPinnacle;
                else if (nobleTitle >= NobleRank.Baron)
                    schoolTier = SchoolTier.City1st;
                else
                    schoolTier = SchoolTier.City2nd;
            }
            else
            {
                if (nobleTitle >= NobleRank.Viscount)
                    schoolTier = SchoolTier.State3rd;
                else if (nobleTitle >= NobleRank.Baron)
                    schoolTier = SchoolTier.City1st;
                else
                    schoolTier = SchoolTier.City3rd;
            }
        }

        public void InitializeSchoolPeersIfEligible()
        {
            if (schoolPeers == null) schoolPeers = new List<SchoolPeer>();
            if (age < 6.0f)
            {
                schoolLevel = SchoolLevel.None;
                schoolName = "Not Enrolled (Unlocks at Age 6)";
                return;
            }

            // Determine educational stage by age
            if (age < 12.0f)
            {
                schoolLevel = SchoolLevel.PrimarySchool;
            }
            else if (age < 15.0f)
            {
                schoolLevel = SchoolLevel.MiddleSchool;
            }
            else
            {
                schoolLevel = SchoolLevel.YouthAcademy;
            }

            // Update school name based on level, tier, city, and state
            schoolName = SchoolHelper.GetSchoolName(schoolLevel, schoolTier, birthCity, birthState);

            // Preserve existing peer relationship levels across upgrades
            float evelynRel = 55f, boranRel = 25f, darekRel = 65f;
            bool evelynCrush = false, evelynFriend = false, darekFriend = false;
            foreach (var p in schoolPeers)
            {
                if (p.name.Contains("Evelyn")) { evelynRel = p.relationship; evelynCrush = p.isCrush; evelynFriend = p.isFriend; }
                else if (p.name.Contains("Boran")) { boranRel = p.relationship; }
                else if (p.name.Contains("Darek")) { darekRel = p.relationship; darekFriend = p.isFriend; }
            }

            schoolPeers.Clear();

            if (schoolTier == SchoolTier.ImperialPinnacle)
            {
                schoolPeers.Add(new SchoolPeer("Crown Prince Alden", "Imperial Royal Scion", "Male", 40f, "Haughty & Regal"));
                schoolPeers.Add(new SchoolPeer("Evelyn Reed", "Imperial Scholarship Sharpshooter", "Female", evelynRel, "Sharp & Focused") { isCrush = evelynCrush, isFriend = evelynFriend });
                schoolPeers.Add(new SchoolPeer("Lady Lyra Vane", "Grand Duke's Prodigy", "Female", 50f, "Calm & Mystical"));
                schoolPeers.Add(new SchoolPeer("Grand Proctor Thorne", "Supreme Imperial Proctor", "Male", 45f, "Iron Discipline"));
            }
            else if (schoolTier >= SchoolTier.State2nd)
            {
                schoolPeers.Add(new SchoolPeer("Evelyn Reed", "State Top Prodigy", "Female", evelynRel, "Studious & Kind") { isCrush = evelynCrush, isFriend = evelynFriend });
                schoolPeers.Add(new SchoolPeer("Lord Gary", "Arrogant Provincial Baronet", "Male", 30f, "Conceited"));
                schoolPeers.Add(new SchoolPeer("Darek Stone", "Frontline Vanguard Cadet", "Male", darekRel, "Brave & Reliable") { isFriend = darekFriend });
                schoolPeers.Add(new SchoolPeer("Kaelen Voss", "Drone & Cyber Apprentice", "Male", 55f, "Inquisitive"));
            }
            else if (schoolTier >= SchoolTier.City1st)
            {
                schoolPeers.Add(new SchoolPeer("Evelyn Reed", "Quiet Prodigy", "Female", evelynRel, "Studious & Kind") { isCrush = evelynCrush, isFriend = evelynFriend });
                schoolPeers.Add(new SchoolPeer("Boran Drake", "Aggressive Cadet Rival", "Male", boranRel, "Aggressive & Loud"));
                schoolPeers.Add(new SchoolPeer("Darek Stone", "Loyal Seatmate", "Male", darekRel, "Tough & Friendly") { isFriend = darekFriend });
                schoolPeers.Add(new SchoolPeer("Instructor Brant", "Municipal Proctor", "Male", 50f, "Stern"));
            }
            else
            {
                // City 3rd and City 2nd (Grassroots Municipal Schools)
                schoolPeers.Add(new SchoolPeer("Darek Stone", "Scrappy Street Seatmate", "Male", darekRel, "Loyal & Scrappy") { isFriend = darekFriend });
                schoolPeers.Add(new SchoolPeer("Boran Drake", "Courtyard Bully", "Male", boranRel, "Aggressive & Loud"));
                schoolPeers.Add(new SchoolPeer("Milo Finch", "Struggling Desk-mate", "Male", 60f, "Nervous & Earnest"));
                schoolPeers.Add(new SchoolPeer("Instructor Brant", "District Drillmaster", "Male", 45f, "Gruff"));
            }
        }

        public SoldierProfile(string name, FamilyLineageSO initialLineage, int rolledTalent = 50)
        {
            characterName = name;
            lineage = initialLineage;
            age = 18.0f; // Directly enlists at age 18 into active military service!
            totalSemestersLived = 0;
            careerTrack = CareerTrack.MilitaryEnlisted;
            isRetired = false;
            isDead = false;
            inMedicalConvalescence = false;
            convalescenceSemestersRemaining = 0;
            hasLostLimb = false;
            amputatedLimbDescription = "";
            hasCombatProsthetic = false;
            isMedicallyRetired = false;
            medicalDisabilityPension = 0;
            combatStress = 0f;
            activeNemesisName = "";
            activeNemesisTitle = "";
            activeNemesisBounty = 0;
            health = 100f;
            physicalFitness = 75f;
            marksmanshipScore = Mathf.Clamp(rolledTalent / 2f, 30f, 95f);
            tacticalIntellect = 65f;
            discipline = 65f;
            combatReadiness = 60f;
            combatSkill = 35f;
            meritRating = 50f;
            squadRespect = 50f;
            coFavor = 50f;
            cashBalance = 1200L;
            failedBreakthroughCount = 0;
            meridianDamage = 0f;
            talentScore = rolledTalent;
            parentRelationship = 85f;
            siblingRelationship = 50f;

            if (initialLineage != null)
            {
                nobleTitle = initialLineage.startingNobleRank;
                politicalInfluence = initialLineage.startingInfluence;
                squadRespect = initialLineage.startingRespect;
                clanFavor = initialLineage.startingClanFavor;
                cashBalance = initialLineage.startingFamilyWealth;
                powerTier = initialLineage.startingPowerTier;
            }
            else
            {
                nobleTitle = NobleRank.Commoner;
            }

            isPersonalTalentRevealed = true;
            isFamilyTalentRevealed = true;

            InitializeDefaultClanMembers();
            InitializeSchoolPeersIfEligible();
        }
    }
}
