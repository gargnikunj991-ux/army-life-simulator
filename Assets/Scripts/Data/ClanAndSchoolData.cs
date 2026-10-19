using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyLife.Data
{
    public enum ClanRelationType
    {
        Father,
        Mother,
        OlderBrother,   // Rival Heir (Lucian)
        YoungerSister,  // Affectionate younger sibling (Seraphina)
        ClanElder,      // Grandfather / Patriarch
        BranchCousin    // Scheming branch house relative (Corvin)
    }

    [Serializable]
    public class ClanMember
    {
        public string name;
        public ClanRelationType relationType;
        public string roleTitle;
        public int age;
        public float relationship; // 0 to 100 (Varied! Not everyone likes you!)
        public NobleRank nobleRank;
        public int powerTier;
        public string personalityTrait; // "Strict", "Affectionate", "Competitive", "Arrogant", "Vindictive", "Wise"
        public float generosity; // 0 to 100 (affects pocket money / allowance)

        public ClanMember(string name, ClanRelationType relationType, string roleTitle, int age, float relationship, NobleRank nobleRank, int powerTier, string personalityTrait, float generosity)
        {
            this.name = name;
            this.relationType = relationType;
            this.roleTitle = roleTitle;
            this.age = age;
            this.relationship = Mathf.Clamp(relationship, 0f, 100f);
            this.nobleRank = nobleRank;
            this.powerTier = powerTier;
            this.personalityTrait = personalityTrait;
            this.generosity = generosity;
        }
    }

    [Serializable]
    public class SchoolPeer
    {
        public string name;
        public string role;
        public string gender;
        public float relationship; // 0 to 100
        public bool isFriend;
        public bool isCrush;
        public string personalityTrait;

        public SchoolPeer(string name, string role, string gender, float relationship, string personalityTrait)
        {
            this.name = name;
            this.role = role;
            this.gender = gender;
            this.relationship = Mathf.Clamp(relationship, 0f, 100f);
            this.personalityTrait = personalityTrait;
            this.isFriend = false;
            this.isCrush = false;
        }
    }

    public enum SchoolLevel
    {
        None = 0,
        PrimarySchool = 1, // Ages 6.0 - 11.5
        MiddleSchool = 2,  // Ages 12.0 - 14.5
        YouthAcademy = 3   // Ages 15.0 - 17.5
    }

    public enum SchoolTier
    {
        City3rd = 0,         // Lowest City School (Passing 50 marks is difficult)
        City2nd = 1,         // Mid City School
        City1st = 2,         // Highest City School (Top municipal scholars)
        State3rd = 3,        // Lowest State School (Provincial entry)
        State2nd = 4,        // Mid State School (Rigorous martial training)
        State1st = 5,        // Highest State School (Difficult to score 75; 85-90 rare)
        ImperialPinnacle = 6 // Solitary Imperial Academy in Imperial Capital (95+ legendary)
    }

    public static class SchoolHelper
    {
        public static string GetSchoolName(SchoolLevel level, SchoolTier tier, string city, string state)
        {
            string cityName = !string.IsNullOrEmpty(city) ? city : "District";
            string stateName = !string.IsNullOrEmpty(state) ? state : "Provincial";

            if (tier == SchoolTier.ImperialPinnacle)
            {
                switch (level)
                {
                    case SchoolLevel.PrimarySchool: return "The Imperial Crown Primary School";
                    case SchoolLevel.MiddleSchool: return "The Imperial Crown Middle School";
                    case SchoolLevel.YouthAcademy: return "The Grand Imperial Youth Academy";
                    default: return "The Grand Imperial Academy";
                }
            }

            string levelSuffix;
            switch (level)
            {
                case SchoolLevel.PrimarySchool: levelSuffix = "Primary School"; break;
                case SchoolLevel.MiddleSchool: levelSuffix = "Middle School"; break;
                case SchoolLevel.YouthAcademy: levelSuffix = "Youth Academy"; break;
                default: levelSuffix = "Academy"; break;
            }

            switch (tier)
            {
                case SchoolTier.City3rd: return $"{cityName} 3rd {levelSuffix}";
                case SchoolTier.City2nd: return $"{cityName} 2nd {levelSuffix}";
                case SchoolTier.City1st: return $"{cityName} 1st {levelSuffix}";
                case SchoolTier.State3rd: return $"{stateName} 3rd State {levelSuffix}";
                case SchoolTier.State2nd: return $"{stateName} 2nd State {levelSuffix}";
                case SchoolTier.State1st: return $"{stateName} 1st State {levelSuffix}";
                default: return $"{cityName} 3rd {levelSuffix}";
            }
        }

        public static string GetTierTitle(SchoolTier tier)
        {
            switch (tier)
            {
                case SchoolTier.City3rd: return "City Tier 3 (Grassroots)";
                case SchoolTier.City2nd: return "City Tier 2 (Standard)";
                case SchoolTier.City1st: return "City Tier 1 (Municipal Pinnacle)";
                case SchoolTier.State3rd: return "State Tier 3 (Provincial Core)";
                case SchoolTier.State2nd: return "State Tier 2 (Provincial Elite)";
                case SchoolTier.State1st: return "State Tier 1 (Provincial Pinnacle)";
                case SchoolTier.ImperialPinnacle: return "Imperial Pinnacle (Valeria Magna)";
                default: return "City Tier 3";
            }
        }

        public static string GetTierDescription(SchoolTier tier)
        {
            switch (tier)
            {
                case SchoolTier.City3rd:
                    return "City Tier 3 (Grassroots) • Difficult to cross 50 marks. Score >= 50% to earn promotion to City 2nd.";
                case SchoolTier.City2nd:
                    return "City Tier 2 (Standard) • Moderate academic rigor. Score >= 55% to earn transfer to City 1st.";
                case SchoolTier.City1st:
                    return "City Tier 1 (Municipal Pinnacle) • Top city scholars. Score >= 60% to earn promotion to State-level schooling.";
                case SchoolTier.State3rd:
                    return "State Tier 3 (Provincial Core) • Regional competition across all 20 cities. Score >= 68% to advance.";
                case SchoolTier.State2nd:
                    return "State Tier 2 (Provincial Elite) • Rigorous standards. Difficult to score 75 marks. Score >= 75% for State 1st.";
                case SchoolTier.State1st:
                    return "State Tier 1 (Provincial Pinnacle) • The highest academy in the state. Scores of 85-90 only appear each year. Top scholar (>= 85%) dispatched to Imperial Academy.";
                case SchoolTier.ImperialPinnacle:
                    return "Imperial Pinnacle (Valeria Magna) • The solitary Crown Academy. The best students across all states gather here. Difficult to score 95+ (Legendary).";
                default:
                    return "";
            }
        }

        public static float CalculateTargetPerformance(SchoolTier tier, float intellect, float foundationalTempering)
        {
            float normInt = Mathf.Clamp01(intellect / 100f);
            float normTemp = Mathf.Clamp01(foundationalTempering / 100f);
            float aptitude = Mathf.Clamp01((normInt * 0.85f) + (normTemp * 0.15f));

            switch (tier)
            {
                case SchoolTier.City3rd:
                    // Difficult to cross 50. Average student: ~42 marks. Dedicated student: ~50-54 marks.
                    return Mathf.Lerp(30f, 56f, aptitude);
                case SchoolTier.City2nd:
                    // Standard City. Average: ~48 marks. Dedicated: ~55-60 marks.
                    return Mathf.Lerp(35f, 62f, aptitude);
                case SchoolTier.City1st:
                    // Municipal Pinnacle. Average: ~54 marks. Dedicated: ~60-66 marks.
                    return Mathf.Lerp(40f, 68f, aptitude);
                case SchoolTier.State3rd:
                    // State Core. Average: ~63 marks. Dedicated: ~68-74 marks.
                    return Mathf.Lerp(50f, 76f, aptitude);
                case SchoolTier.State2nd:
                    // State Elite. Difficult to score 75. Average: ~69 marks. Dedicated: ~75-81 marks.
                    return Mathf.Lerp(56f, 82f, aptitude);
                case SchoolTier.State1st:
                    // State Pinnacle. 85-90 only appear for top prodigies. Average: ~78 marks. Top scholar: ~85-90 marks.
                    return Mathf.Lerp(64f, 92f, aptitude);
                case SchoolTier.ImperialPinnacle:
                    // Imperial Solitary Crown Academy. Average: ~87 marks. 95+ is difficult / legendary!
                    return Mathf.Lerp(76f, 98f, aptitude);
                default:
                    return Mathf.Lerp(30f, 60f, aptitude);
            }
        }

        public static float GetTierDifficultyModifier(SchoolTier tier)
        {
            switch (tier)
            {
                case SchoolTier.City3rd: return 0.70f;
                case SchoolTier.City2nd: return 0.76f;
                case SchoolTier.City1st: return 0.82f;
                case SchoolTier.State3rd: return 0.86f;
                case SchoolTier.State2nd: return 0.90f;
                case SchoolTier.State1st: return 0.93f;
                case SchoolTier.ImperialPinnacle: return 0.97f;
                default: return 0.70f;
            }
        }

        public static float GetPromotionThreshold(SchoolTier tier)
        {
            switch (tier)
            {
                case SchoolTier.City3rd: return 50f; // Cross 50 in City 3rd to promote to City 2nd
                case SchoolTier.City2nd: return 55f; // Cross 55 in City 2nd to promote to City 1st
                case SchoolTier.City1st: return 60f; // Cross 60 in City 1st to promote to State 3rd
                case SchoolTier.State3rd: return 68f; // Cross 68 in State 3rd to promote to State 2nd
                case SchoolTier.State2nd: return 75f; // Difficult to cross 75 in State 2nd to promote to State 1st
                case SchoolTier.State1st: return 85f; // Top student crossing 85 in State 1st earns Imperial Academy dispatch!
                default: return 95f;                 // Difficult to score 95+ in Imperial Academy
            }
        }
    }
}
