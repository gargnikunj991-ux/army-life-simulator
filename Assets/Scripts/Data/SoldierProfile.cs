using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyLife.Data
{
    public enum CareerTrack
    {
        YouthStudent,           // Ages 0 to 18.0
        // Military Tracks
        MilitaryRecruitBasic,   // 6 months (1 semester), UNPAID ($0), passes out as E-1 Private
        MilitaryEnlisted,       // E-1 to E-9 active duty
        MilitaryOfficer,        // O-1 to O-10 active duty
        MilitaryTechSchool,     // 2 years (4 semesters). Year 1 unpaid ($0), Year 2 paid $200/mo. Passes out as E-3 Specialist with tech stipends!
        MilitaryMedicalSchool,  // 3 years (6 semesters). Year 1 unpaid ($0), Years 2-3 paid $250/mo. Passes out as Combat Medic Officer!
        MilitaryWarAcademy,     // 4 years (8 semesters). PAID FROM DAY 1 ($300/mo). MUST have Tier 1 Cultivation to pass out!
        // Civilian Tracks
        CivilianWork,           // Direct entry jobs $150/mo to $300/mo
        CivilianCollege,        // 4 years (8 semesters). Costs $100,000 for 4 years ($12,500/sem).
        CivilianProfessional    // Post-college degree careers starting at $500+/mo up to position!
    }

    [Serializable]
    public class SoldierProfile
    {
        [Header("Identity & Background")]
        public string lifeId;
        public string characterName;
        public string firstName;
        public string lastName;
        public string gender = "Male";
        public string birthCountry = "The Valerian Empire";
        public string birthState = "The Imperial Crownlands (Aethelgard)";
        public string birthCity = "Valeria Magna";
        public bool isBornInImperialState = true;
        public float age;                       // e.g. 18.0, 18.5, 19.0
        public int totalSemestersLived;         // Number of 6-month turns survived
        public bool isYouthPhase => age < 18.0f;
        public bool isRetired;                  // True if honorably discharged / retired to veteran life
        public bool isDead;                     // True if KIA, executed, or killed by breakthrough backlash
        public string deathReason;

        [Header("Appearance & Customization (BitLife style)")]
        public string hairStyle = "Military Buzzcut";
        public string hairColor = "Jet Black";
        public string eyeColor = "Steel Grey";
        public string skinTone = "Tan";
        [Range(0f, 100f)] public float looks = 80f;

        [Header("Lineage & Aristocracy")]
        public FamilyLineageSO lineage;
        public NobleRank nobleTitle;
        public bool isClanHead;                 // True if player won Clan Head seat (inherits Duke/Marquis title)
        public int clanSuccessionScore;         // Evaluated against siblings by Clan Elders

        [Header("Military Standing")]
        public MilitaryRankSO currentRank;
        public int totalPromotionsEarned;       // At 3 promotions -> Triggers Arranged Marriage!
        public int accumulatedPromotionPoints;  // Earned through PT tests, combat tours, and merit
        public string assignedDepartment;       // "Infantry", "Technical & Cyber", "Combat Medic", "Secret Branch"
        public bool isTechnicalSchoolGraduate;  // Grants guaranteed Tech ($450) + Cyber ($350) stipends
        public bool isInCombatDeployment;       // True during frontline tours (+Hazard Pay)
        public int medalsEarnedCount;

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

        [Header("Power Cultivation (4 Pre-Tier Stages + 12 Tiers with 4 Sub-Tiers)")]
        [Range(1, 4)]
        public int preTierStage = 1;            // 1 to 4: Body Tempering, Meridian Awakening, Qi Circulation, Foundational Core
        [Range(0, 12)]
        public int powerTier;                   // 0 to 12 (0 = Pre-Tier Stages 1-4)
        public PowerSubTier subTier = PowerSubTier.Early; // Early, Mid, Late, Peak within each tier
        [Range(0f, 100f)]
        public float tierProgressPercent;       // Progress towards next sub-tier or major tier breakthrough
        public int failedBreakthroughCount;     // Consecutive bottleneck failures (3 = fatal / crippling!)
        [Range(0f, 100f)]
        public float meridianDamage;            // 0 = healthy; 50+ = severe cultivation penalty
        public bool hasDiscoveredSecretCave;    // True if unsealed Ancient Cultivation Sect Cave
        public bool unlockedTiers11And12;       // Secret Transcendent Realms (500 & 1,000 year lifespans)
        public bool hasCorruptedFoundation;     // If true, locked out of Secret Cave

        [Header("Talent (0 to 200 Scale)")]
        [Range(0, 200)]
        public int talentScore;                 // Multiplies cultivation absorption and breakthrough chance
        public bool isPersonalTalentRevealed;   // Revealed at birth for Nobles; revealed at E-4 for Commoners
        public bool isFamilyTalentRevealed;     // Revealed at E-8 (Master Sergeant) clearance

        [Header("Core Attributes (0-100)")]
        [Range(0f, 100f)] public float health = 100f;
        [Range(0f, 100f)] public float fitness = 50f;
        [Range(0f, 100f)] public float combatSkill = 30f;
        [Range(0f, 100f)] public float meritRating = 50f;
        [Range(0f, 100f)] public float politicalInfluence = 10f;
        [Range(0f, 100f)] public float clanFavor = 50f;
        [Range(0f, 100f)] public float squadRespect = 50f;
        [Range(0f, 100f)] public float coFavor = 50f;
        [Range(0f, 100f)] public float happiness = 80f;

        [Header("Knowledge & Family Well-Being")]
        public bool knowsAboutCultivation;      // False for commoners; unlocked at E-4 or noble birth
        public float familyLivingCondition = 20f; // 0=Impoverished, 40=Modest, 70=Comfortable, 100=Prominent
        public long totalSentToFamily;
        public long totalRemittancesSentToFamily { get => totalSentToFamily; set => totalSentToFamily = value; }

        [Header("Treasury & Military Currency")]
        public long cashBalance;
        public long combatMeritPoints;          // Restricted military currency for serums & prototypes

        [Header("Military Career Tracking")]
        public int timeInCurrentRankSemesters;
        public int promotionPoints { get => accumulatedPromotionPoints; set => accumulatedPromotionPoints = value; }
        public FamilyLineageSO familyLineage { get => lineage; set => lineage = value; }
        public string birthProvince { get => birthState; set => birthState = value; }
        public SchoolLevel currentSchoolLevel { get => schoolLevel; set => schoolLevel = value; }
        public SchoolTier currentSchoolTier { get => schoolTier; set => schoolTier = value; }
        public void InitializePeers() => InitializeSchoolPeersIfEligible();

        [Header("Dynasty, Marriage & Succession")]
        public bool isMarried;
        public string spouseName;
        public NobleRank spouseNobleRank;
        public int spouseTalent;
        public int childrenCount;
        public bool hasRejectedChildSoulTransfer; // If true, locked until death -> reincarnate as youngest descendant

        public SoldierProfile()
        {
            health = 100f;
            fitness = 50f;
            combatSkill = 30f;
            meritRating = 50f;
            politicalInfluence = 10f;
            clanFavor = 50f;
            squadRespect = 50f;
            coFavor = 50f;
        }

        [Header("Childhood & Youth Development (Ages 0 to 18)")]
        [Range(0f, 100f)] public float intellect = 50f;
        [Range(0f, 100f)] public float foundationalTempering = 10f;
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

        public void InitializeDefaultClanMembers()
        {
            if (clanMembers == null) clanMembers = new List<ClanMember>();
            if (clanMembers.Count > 0) return;

            string famName = !string.IsNullOrEmpty(lastName) ? lastName : "Vance";
            NobleRank rank = (lineage != null) ? lineage.startingNobleRank : NobleRank.Commoner;

            clanMembers.Add(new ClanMember($"Lord Julian {famName}", ClanRelationType.Father, "Father (Imperial Veteran)", 42, 65f, rank, 2, "Strict & Honorable", 60f));
            clanMembers.Add(new ClanMember($"Lady Eleanor {famName}", ClanRelationType.Mother, "Mother", 40, 85f, rank, 1, "Warm & Devoted", 85f));
            clanMembers.Add(new ClanMember($"Lucian {famName}", ClanRelationType.OlderBrother, "Older Brother (Rival Heir)", 2, 35f, rank, 1, "Jealous & Competitive", 15f));
            clanMembers.Add(new ClanMember($"Seraphina {famName}", ClanRelationType.YoungerSister, "Younger Sister", 1, 70f, rank, 0, "Affectionate", 50f));

            if (rank > NobleRank.Commoner)
            {
                clanMembers.Add(new ClanMember($"Grand Duke Raymond {famName}", ClanRelationType.ClanElder, "Clan Patriarch & Elder", 112, 40f, rank, 5, "Austere & Demanding", 40f));
                clanMembers.Add(new ClanMember($"Corvin {famName}", ClanRelationType.BranchCousin, "Branch House Cousin", 3, 25f, NobleRank.Baron, 1, "Scheming & Resentful", 10f));
            }
            else
            {
                clanMembers.Add(new ClanMember($"Uncle Douglas {famName}", ClanRelationType.ClanElder, "Clan Elder (Retired Sergeant)", 68, 50f, NobleRank.Commoner, 1, "Gruff & Honest", 35f));
                clanMembers.Add(new ClanMember($"Corvin {famName}", ClanRelationType.BranchCousin, "Cousin (Street Brawler)", 3, 30f, NobleRank.Commoner, 0, "Envious", 15f));
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

        public SoldierProfile(string name, FamilyLineageSO initialLineage, int rolledTalent)
        {
            characterName = name;
            lineage = initialLineage;
            age = 0.0f; // Born as a baby into the world!
            totalSemestersLived = 0;
            talentScore = rolledTalent;
            isRetired = false;
            isDead = false;
            failedBreakthroughCount = 0;
            meridianDamage = 0f;
            intellect = 50f;
            foundationalTempering = 10f;
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

                // Noble houses have ancestral testing stones to reveal talent at birth
                isPersonalTalentRevealed = (nobleTitle > NobleRank.Commoner);
            }
            else
            {
                nobleTitle = NobleRank.Commoner;
                isPersonalTalentRevealed = false;
            }

            InitializeDefaultClanMembers();
            InitializeSchoolPeersIfEligible();
        }
    }
}
