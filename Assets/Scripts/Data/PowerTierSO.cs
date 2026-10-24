using System;
using UnityEngine;

namespace ArmyLife.Data
{
    public enum TierStratum
    {
        PreTierMortal,      // Pre-qualification conditioning stages
        MortalInitiate,     // Basic active duty tiers
        MortalPeak,         // Senior enlisted / non-commissioned officer tiers
        NationalGuardian,   // High command strategic tiers
        PrimordialSect      // Legacy classified tiers
    }

    public enum PowerSubTier
    {
        Early = 0,
        Mid = 1,
        Late = 2,
        Peak = 3
    }

    /// <summary>
    /// Deprecated cultivation tier definition, retained for asset and database compatibility.
    /// Grounded military progression relies on MilitaryRankSO, Paygrades, and MilitaryBranch roles.
    /// </summary>
    [Obsolete("PowerTierSO is deprecated in pure military sim. Career progression is managed via MilitaryRankSO.")]
    [CreateAssetMenu(fileName = "PowerTier_", menuName = "ArmyLife/Power Tier (Deprecated)")]
    public class PowerTierSO : ScriptableObject
    {
        [Header("Tier Identity")]
        public int tierLevel;               // 1 to 12
        public string designation;          // e.g. "Line Infantry Specialist"
        public TierStratum stratum;

        [Header("Lifespan & Biological Limits")]
        public int maxLifespanYears;

        [Header("Monthly Financial & Resource Metrics")]
        public int monthlySalaryBonus;
        public int monthlyCashBurn;
        public int monthlyCMPBurn;

        [Header("Bottleneck Breakthrough")]
        [Range(0f, 1f)]
        public float breakthroughDifficulty;

        [Header("Information Censorship & Fog")]
        public bool isHiddenFromPublic;
        public bool isHiddenFromTopHouses;

        private void OnValidate()
        {
            if (monthlySalaryBonus == 0 && tierLevel > 0)
            {
                monthlySalaryBonus = tierLevel * 750;
            }
        }
    }

    /// <summary>
    /// Military readiness and conditioning level descriptor.
    /// Supersedes fantasy cultivation knowledge and qi breakthrough calculations.
    /// </summary>
    public static class PowerTierKnowledgeSystem
    {
        public static string GetPreTierStageName(int stage)
        {
            switch (stage)
            {
                case 1: return "Basic Conditioning";
                case 2: return "Infantry Tactics";
                case 3: return "Field Exercises";
                case 4: return "Combat Readiness";
                default: return "Basic Conditioning";
            }
        }

        public static string GetSubTierName(PowerSubTier subTier)
        {
            switch (subTier)
            {
                case PowerSubTier.Early: return "Class 1";
                case PowerSubTier.Mid: return "Class 2";
                case PowerSubTier.Late: return "Class 3";
                case PowerSubTier.Peak: return "Senior";
                default: return "Class 1";
            }
        }

        public static string GetTierDesignation(int tier)
        {
            switch (tier)
            {
                case 1: return "Line Infantry Recruit";
                case 2: return "Infantry Rifleman";
                case 3: return "Squad Specialist";
                case 4: return "Veteran Fireteam Leader";
                case 5: return "Squad Leader";
                case 6: return "Platoon Sergeant";
                case 7: return "First Sergeant";
                case 8: return "Sergeant Major";
                case 9: return "Command Sergeant Major";
                case 10: return "Chief of Enlisted Staff";
                case 11: return "Master War Specialist";
                case 12: return "Marshal Special Advisor";
                default: return "Active Duty Soldier";
            }
        }

        public static bool IsNationalGuardian(int tier)
        {
            return tier >= 7;
        }

        public static int GetPerceivedMaxTier(int viewerTier, bool unlockedHidden)
        {
            if (unlockedHidden) return 12;
            if (viewerTier >= 4) return 10;
            return 6;
        }

        public static float GetClimbingDifficultyFactor(int tier, PowerSubTier subTier)
        {
            if (tier <= 0) return 1.0f;
            float tierFactor = 1.0f + (tier * 0.40f);
            float subTierFactor = 1.0f + ((int)subTier * 0.30f);
            return tierFactor * subTierFactor;
        }

        public static float GetSubTierAdvancementDifficulty(int tier, PowerSubTier currentSubTier)
        {
            float tierBase = tier * 0.035f;
            switch (currentSubTier)
            {
                case PowerSubTier.Early: return 0.22f + tierBase;
                case PowerSubTier.Mid:   return 0.34f + tierBase;
                case PowerSubTier.Late:  return 0.46f + tierBase;
                default:                 return 0.50f + tierBase;
            }
        }

        public static string FormatSoldierPowerTier(SoldierProfile soldier)
        {
            if (soldier == null) return "Civilian";

            if (soldier.currentRank != null)
            {
                return $"{soldier.currentRank.rankName} ({soldier.currentRank.paygrade})";
            }

            if (soldier.powerTier == 0)
            {
                int stage = Mathf.Clamp(soldier.preTierStage, 1, 4);
                return $"Conditioning Stage {stage}/4 ({GetPreTierStageName(stage)})";
            }

            int perceivedMax = GetPerceivedMaxTier(soldier.powerTier, soldier.unlockedTiers11And12);
            string subTierStr = GetSubTierName(soldier.subTier);

            return $"Readiness Level {soldier.powerTier} ({subTierStr})/{perceivedMax} ({GetTierDesignation(soldier.powerTier)})";
        }
    }
}
