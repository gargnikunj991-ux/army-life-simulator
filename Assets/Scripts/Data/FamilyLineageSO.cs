using UnityEngine;

namespace ArmyLife.Data
{
    [CreateAssetMenu(fileName = "Lineage_", menuName = "ArmyLife/Family Lineage")]
    public class FamilyLineageSO : ScriptableObject
    {
        [Header("Father Archetype")]
        public string archetypeName;        // e.g. "4-Star General Dynasty", "Civilian Clean Slate"
        public string fatherTitle;          // e.g. "General Vance, Chief of Army Staff"
        [TextArea(2, 4)]
        public string description;

        [Header("Starting Hereditary Status")]
        public NobleRank startingNobleRank;
        [Range(0, 100)]
        public int startingInfluence;       // Political leverage with High Command
        [Range(0, 100)]
        public int startingRespect;         // Respect among grunts and NCOs
        [Range(0, 100)]
        public int startingClanFavor;       // Standing with family elders
        public long startingFamilyWealth;

        [Header("Pre-Enlistment Military Preparation")]
        [Tooltip("Legacy sons receive preparatory military coaching before enlistment.")]
        public int startingPowerTier;       // Readiness baseline
        [Range(0f, 100f)]
        public float startingFoundationalTempering; // Physical conditioning before youth academy

        [Header("Academy Fast-Track Privileges")]
        public bool hasWestPointFastTrack;
        public bool hasSecretBranchInvitation;
    }
}
