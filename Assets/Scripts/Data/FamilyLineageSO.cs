using UnityEngine;

namespace ArmyLife.Data
{
    [CreateAssetMenu(fileName = "Lineage_", menuName = "ArmyLife/Family Lineage")]
    public class FamilyLineageSO : ScriptableObject
    {
        [Header("Family Background Archetype")]
        public string archetypeName;        // e.g. "4-Star General Dynasty", "Working Class Enlistee"
        public string fatherTitle;          // e.g. "General Vance, Chief of Army Staff"
        [TextArea(2, 4)]
        public string description;

        [Header("Starting Heritage & Standing")]
        [Range(0, 100)]
        public int startingInfluence;       // Political leverage with High Command
        [Range(0, 100)]
        public int startingRespect;         // Respect among soldiers and NCOs
        public long startingFamilyWealth;

        [Header("Academy & Branch Privileges")]
        public bool hasWestPointFastTrack;
        public bool hasSecretBranchInvitation;
    }
}
