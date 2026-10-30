using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Feudal-military aristocratic hierarchy within the Valerian Empire.
    /// Determines family private army size, Royal Court attendance, and monthly noble stipends.
    /// </summary>
    public enum NobleRank
    {
        Commoner = 0,   // Standard grunts and civilians; pure merit climbers
        Baron = 1,      // Minor military nobility; company/battalion commanders
        Viscount = 2,   // Junior aristocracy; enhanced clan resource quotas
        Count = 3,      // Regiment commanders; Tier 6/7 martial masters
        Marquis = 4,    // Division commanders; high-tier vault access
        GrandDuke = 5,  // Corps commanders; commands private Ducal Guard & attends Royal Court
        RoyalClan = 6   // Imperial Sovereigns; supreme military house & commanders-in-chief
    }

    public enum PaygradeType
    {
        Enlisted,   // E-1 to E-9
        Warrant,    // WO-1 to CW-5
        Officer     // O-1 to O-10
    }

    [CreateAssetMenu(fileName = "Rank_", menuName = "ArmyLife/Military Rank")]
    public class MilitaryRankSO : ScriptableObject
    {
        [Header("Rank Identity")]
        public string rankName;             // e.g. "Private First Class", "Corporal", "Master Sergeant"
        public string paygrade;             // e.g. "E-1", "E-4", "E-8", "O-1", "O-10"
        public PaygradeType type;
        public int rankOrder;               // Numeric sorting index (1 = E-1, 4 = E-4, 8 = E-8, etc.)

        [Header("Financial Compensation")]
        [Tooltip("Standard military monthly base pay.")]
        public int baseMonthlySalary;

        [Header("Promotion Requirements")]
        public int requiredMeritPoints;
        public int requiredFitness;
        public int requiredCombatSkill;
    }
}
