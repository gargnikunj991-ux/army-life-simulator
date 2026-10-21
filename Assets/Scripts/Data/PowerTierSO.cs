using UnityEngine;

namespace ArmyLife.Data
{
    public enum TierStratum
    {
        PreTierMortal,      // Pre-Tier 1: 4 Preparatory Stages (Cellular Conditioning, Neural Calibration, Bio-Resonance Sync, Sub-Dermal Framework)
        MortalInitiate,     // Tiers 1-3: Enhanced Operative Classes (Iron Vanguard, Bronze Strike, Silver Centurion)
        MortalPeak,         // Tiers 4-6: Titan Warframe Augments (Gold Vanguard, Platinum Frame, Apex Titan)
        NationalGuardian,   // Tiers 7-10: Strategic Dreadnought Super-Soldiers (Dreadnought, Orbital Sovereign, Imperial Pillar)
        PrimordialSect      // Tiers 11-12: Classified Pre-War Prototypes (Singularity Operative, Transcendent Warmaster Frame)
    }

    public enum PowerSubTier
    {
        Early = 0,
        Mid = 1,
        Late = 2,
        Peak = 3
    }

    [CreateAssetMenu(fileName = "PowerTier_", menuName = "ArmyLife/Power Tier")]
    public class PowerTierSO : ScriptableObject
    {
        [Header("Tier Identity")]
        public int tierLevel;               // 1 to 12
        public string designation;          // e.g. "Iron Initiate", "Primordial Immortal"
        public TierStratum stratum;

        [Header("Lifespan & Biological Limits")]
        [Tooltip("Maximum biological lifespan achieved upon reaching this tier.")]
        public int maxLifespanYears;        // Tier 11 = 500, Tier 12 = 1000

        [Header("Monthly Financial & Resource Metrics")]
        [Tooltip("Automatic monthly merit salary stipend ($750 * Tier Level).")]
        public int monthlySalaryBonus;
        [Tooltip("Monthly cost in Cash required to sustain steady cultivation progress.")]
        public int monthlyCashBurn;
        [Tooltip("Alternative monthly cost in Combat Merit Points (CMP).")]
        public int monthlyCMPBurn;

        [Header("Bottleneck Breakthrough")]
        [Range(0f, 1f)]
        public float breakthroughDifficulty; // Higher = harder trial / lower base success rate

        [Header("Information Censorship & Fog")]
        [Tooltip("If true, general grunts and civilians believe this tier cannot be achieved.")]
        public bool isHiddenFromPublic;
        [Tooltip("If true, even children of 4-Star Generals and top houses are unaware this tier exists.")]
        public bool isHiddenFromTopHouses;  // True for Tiers 11 and 12

        private void OnValidate()
        {
            if (monthlySalaryBonus == 0 && tierLevel > 0)
            {
                monthlySalaryBonus = tierLevel * 750;
            }
        }
    }

    public static class PowerTierKnowledgeSystem
    {
        public static string GetPreTierStageName(int stage)
        {
            switch (stage)
            {
                case 1: return "Cellular Conditioning";
                case 2: return "Neural Calibration";
                case 3: return "Bio-Resonance Integration";
                case 4: return "Sub-Dermal Framework";
                default: return "Cellular Conditioning";
            }
        }

        public static string GetSubTierName(PowerSubTier subTier)
        {
            switch (subTier)
            {
                case PowerSubTier.Early: return "Mk I";
                case PowerSubTier.Mid: return "Mk II";
                case PowerSubTier.Late: return "Mk III";
                case PowerSubTier.Peak: return "Apex";
                default: return "Mk I";
            }
        }

        public static string GetTierDesignation(int tier)
        {
            switch (tier)
            {
                case 1: return "Iron Vanguard Operator";
                case 2: return "Bronze Strike Commando";
                case 3: return "Silver Centurion Frame";
                case 4: return "Gold Vanguard Commander";
                case 5: return "Platinum Warframe";
                case 6: return "Titan Dreadnought (Peak Augmented)";
                case 7: return "Dreadnought Class";
                case 8: return "Orbital Sovereign Frame";
                case 9: return "Imperial Aegis Pillar";
                case 10: return "Apex Strategic Deterrent";
                case 11: return "Singularity Operative (Classified)";
                case 12: return "Transcendent Warmaster (Classified)";
                default: return "Standard Human";
            }
        }

        public static bool IsNationalGuardian(int tier)
        {
            return tier >= 7;
        }

        /// <summary>
        /// Returns the perceived max tier based on knowledge censorship:
        /// - Soldiers below Tier 4 only know up to Tier 6 (The Spoken Mortal Limit).
        /// - Soldiers Tier 4 to 10 know up to Tier 10 (National Guardian apex).
        /// - Tiers 11 and 12 are hidden from everyone unless unlocked via the Secret Cave.
        /// </summary>
        public static int GetPerceivedMaxTier(int viewerTier, bool unlockedHidden)
        {
            if (unlockedHidden) return 12;
            if (viewerTier >= 4) return 10;
            return 6;
        }

        /// <summary>
        /// Returns the climbing difficulty multiplier based on major tier and sub-tier.
        /// Higher tiers and higher sub-tiers (Early -> Mid -> Late -> Peak) require exponentially denser Qi.
        /// </summary>
        public static float GetClimbingDifficultyFactor(int tier, PowerSubTier subTier)
        {
            if (tier <= 0) return 1.0f;
            float tierFactor = 1.0f + (tier * 0.40f);
            float subTierFactor = 1.0f + ((int)subTier * 0.30f);
            return tierFactor * subTierFactor;
        }

        /// <summary>
        /// Returns the bottleneck trial difficulty for sub-tier advancement within a tier.
        /// Early -> Mid (0.22+) < Mid -> Late (0.34+) < Late -> Peak (0.46+).
        /// </summary>
        public static float GetSubTierAdvancementDifficulty(int tier, PowerSubTier currentSubTier)
        {
            float tierBase = tier * 0.035f;
            switch (currentSubTier)
            {
                case PowerSubTier.Early: return 0.22f + tierBase; // Advancing to Mid
                case PowerSubTier.Mid:   return 0.34f + tierBase; // Advancing to Late
                case PowerSubTier.Late:  return 0.46f + tierBase; // Advancing to Peak
                default:                 return 0.50f + tierBase;
            }
        }

        /// <summary>
        /// Formats the power tier display string for a soldier, respecting knowledge fog and sub-tiers.
        /// </summary>
        public static string FormatSoldierPowerTier(SoldierProfile soldier)
        {
            if (soldier == null) return "Mortal";

            if (soldier.powerTier == 0)
            {
                int stage = Mathf.Clamp(soldier.preTierStage, 1, 4);
                return $"Pre-Tier Stage {stage}/4 ({GetPreTierStageName(stage)})";
            }

            int perceivedMax = GetPerceivedMaxTier(soldier.powerTier, soldier.unlockedTiers11And12);
            string subTierStr = GetSubTierName(soldier.subTier);

            if (soldier.powerTier >= 7)
            {
                string hiddenTag = soldier.powerTier >= 11 ? " [HIDDEN REALM]" : "";
                return $"<color=#FFD700>Tier {soldier.powerTier} ({subTierStr})/{perceivedMax} • National Guardian ({GetTierDesignation(soldier.powerTier)}){hiddenTag}</color>";
            }
            else
            {
                return $"Tier {soldier.powerTier} ({subTierStr})/{perceivedMax} ({GetTierDesignation(soldier.powerTier)})";
            }
        }
    }
}
