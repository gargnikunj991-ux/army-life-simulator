using UnityEngine;

namespace ArmyLife.Data
{
    public enum TierStratum
    {
        MortalInitiate,     // Tiers 1-3: Standard enhanced human biology
        MortalPeak,         // Tiers 4-6: The Spoken Limit of mortal biology (Grandmaster)
        NationalGuardian,   // Tiers 7-10: Strategic deterrents known only to top houses
        PrimordialSect      // Tiers 11-12: Hidden Ancient Sect Realms (500 & 1,000 year lifespans)
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
}
