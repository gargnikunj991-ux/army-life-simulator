using System;
using UnityEngine;

namespace ArmyLife.Data
{
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

        [Header("Power Cultivation (12 Tiers)")]
        [Range(0, 12)]
        public int powerTier;                   // 0 to 12
        [Range(0f, 100f)]
        public float tierProgressPercent;       // Progress towards next tier breakthrough
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

        [Header("Knowledge & Family Well-Being")]
        public bool knowsAboutCultivation;      // False for commoners; unlocked at E-4 or noble birth
        public float familyLivingCondition = 20f; // 0=Impoverished, 40=Modest, 70=Comfortable, 100=Prominent
        public long totalSentToFamily;

        [Header("Treasury & Military Currency")]
        public long cashBalance;
        public long combatMeritPoints;          // Restricted military currency for serums & prototypes

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
        }
    }
}
