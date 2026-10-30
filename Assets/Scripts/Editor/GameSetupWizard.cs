#if UNITY_EDITOR
using ArmyLife.Data;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ArmyLife.Editor
{
    public static class GameSetupWizard
    {
        private const string DataRoot = "Assets/GameData";

        [MenuItem("Army Life/Generate All Default Game Data", false, 1)]
        public static void GenerateAllDefaultData()
        {
            EnsureDirectoryExists($"{DataRoot}/Ranks");
            EnsureDirectoryExists($"{DataRoot}/Lineages");

            GenerateMilitaryRanks();
            GenerateFamilyLineages();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Army Life Simulator", 
                    "Successfully generated all Military Ranks (E-1 to O-10) and Family Lineages inside Assets/GameData/!", 
                    "OK");
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void GenerateMilitaryRanks()
        {
            CreateRank("Private Recruit", "E-1", PaygradeType.Enlisted, 1, 0, 0, 40);
            CreateRank("Private Second Class", "E-2", PaygradeType.Enlisted, 2, 2050, 150, 45);
            CreateRank("Private First Class", "E-3", PaygradeType.Enlisted, 3, 2300, 300, 50);
            CreateRank("Corporal", "E-4", PaygradeType.Enlisted, 4, 2700, 500, 60);
            CreateRank("Sergeant", "E-5", PaygradeType.Enlisted, 5, 3200, 750, 65);
            CreateRank("Staff Sergeant", "E-6", PaygradeType.Enlisted, 6, 3750, 1050, 70);
            CreateRank("Sergeant First Class", "E-7", PaygradeType.Enlisted, 7, 4400, 1400, 75);
            CreateRank("Master Sergeant", "E-8", PaygradeType.Enlisted, 8, 5300, 1800, 80);
            CreateRank("Command Sgt Major", "E-9", PaygradeType.Enlisted, 9, 6400, 2300, 85);

            CreateRank("2nd Lieutenant", "O-1", PaygradeType.Officer, 10, 3800, 400, 60);
            CreateRank("1st Lieutenant", "O-2", PaygradeType.Officer, 11, 4700, 600, 65);
            CreateRank("Captain", "O-3", PaygradeType.Officer, 12, 6200, 1100, 70);
            CreateRank("Major", "O-4", PaygradeType.Officer, 13, 7400, 1700, 75);
            CreateRank("Lieutenant Colonel", "O-5", PaygradeType.Officer, 14, 8800, 2400, 78);
            CreateRank("Colonel", "O-6", PaygradeType.Officer, 15, 10500, 3200, 80);
            CreateRank("Brigadier General", "O-7", PaygradeType.Officer, 16, 12500, 4400, 82);
            CreateRank("Major General", "O-8", PaygradeType.Officer, 17, 14500, 5800, 85);
            CreateRank("Lieutenant General", "O-9", PaygradeType.Officer, 18, 16500, 7400, 88);
            CreateRank("General (Guardian Pillar)", "O-10", PaygradeType.Officer, 19, 19000, 9500, 90);
        }

        private static void CreateRank(string title, string paygrade, PaygradeType type, int order, int salary, int merit, int fitness)
        {
            string path = $"{DataRoot}/Ranks/Rank_{paygrade}_{title.Replace(" ", "")}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MilitaryRankSO>(path);
            var asset = existing != null ? existing : ScriptableObject.CreateInstance<MilitaryRankSO>();

            asset.rankName = title;
            asset.paygrade = paygrade;
            asset.type = type;
            asset.rankOrder = order;
            asset.baseMonthlySalary = salary;
            asset.requiredMeritPoints = merit;
            asset.requiredFitness = fitness;

            if (existing == null) AssetDatabase.CreateAsset(asset, path);
            else EditorUtility.SetDirty(asset);
        }

        private static void GenerateFamilyLineages()
        {
            CreateLineage("4-Star General Dynasty", "General Vance, Chief of Army Staff", 85, 90, 50000, true, true,
                "Born with deep military connections and expectations. High Command observes your career closely, but expectations are high.");

            CreateLineage("Special Forces Legend", "Major Vance (Medal of Honor)", 50, 85, 20000, false, true,
                "Your father was a decorated battlefield commander. Soldiers immediately respect your heritage and leadership potential.");

            CreateLineage("Command Sgt Major Backbone", "CSM Miller, 75th Rangers", 35, 75, 10000, false, false,
                "Enlisted heritage through and through. You were raised around barracks discipline and have the grunts' unwavering trust.");

            CreateLineage("Working Class Recruit", "Civilian Blue-Collar Family", 0, 40, 2000, false, false,
                "No military connections or head starts. Your rank, pay, and reputation must all be forged on the frontlines.");
        }

        private static void CreateLineage(string name, string father, int influence, int respect, long wealth, bool westPoint, bool secretBranch, string desc)
        {
            string path = $"{DataRoot}/Lineages/Lineage_{name.Replace(" ", "")}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<FamilyLineageSO>(path);
            var asset = existing != null ? existing : ScriptableObject.CreateInstance<FamilyLineageSO>();

            asset.archetypeName = name;
            asset.fatherTitle = father;
            asset.startingInfluence = influence;
            asset.startingRespect = respect;
            asset.startingFamilyWealth = wealth;
            asset.hasWestPointFastTrack = westPoint;
            asset.hasSecretBranchInvitation = secretBranch;
            asset.description = desc;

            if (existing == null) AssetDatabase.CreateAsset(asset, path);
            else EditorUtility.SetDirty(asset);
        }
    }
}
#endif
