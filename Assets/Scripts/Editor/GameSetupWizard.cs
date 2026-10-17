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
            EnsureDirectoryExists($"{DataRoot}/PowerTiers");
            EnsureDirectoryExists($"{DataRoot}/Ranks");
            EnsureDirectoryExists($"{DataRoot}/Lineages");

            GeneratePowerTiers();
            GenerateMilitaryRanks();
            GenerateFamilyLineages();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Army Life Simulator", 
                "Successfully generated all 12 Power Tiers, Military Ranks (E-1 to O-10), and Family Lineages inside Assets/GameData/!", 
                "OK");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void GeneratePowerTiers()
        {
            // Tier 1 to 12 Definitions
            CreatePowerTier(1, "Iron Initiate", TierStratum.MortalInitiate, 85, 200, 50, 0.10f, false, false);
            CreatePowerTier(2, "Bronze Vanguard", TierStratum.MortalInitiate, 90, 450, 120, 0.15f, false, false);
            CreatePowerTier(3, "Silver Centurion", TierStratum.MortalInitiate, 95, 800, 250, 0.20f, false, false);
            CreatePowerTier(4, "Gold Striker", TierStratum.MortalPeak, 115, 1500, 500, 0.30f, false, false);
            CreatePowerTier(5, "Obsidian Champion", TierStratum.MortalPeak, 125, 2800, 1000, 0.40f, false, false);
            CreatePowerTier(6, "Peak Grandmaster", TierStratum.MortalPeak, 140, 4500, 1800, 0.50f, false, false); // Spoken limit
            CreatePowerTier(7, "Astral Guardian", TierStratum.NationalGuardian, 220, 8000, 3500, 0.65f, true, false);
            CreatePowerTier(8, "Dreadnought Warlord", TierStratum.NationalGuardian, 260, 14000, 6000, 0.75f, true, false);
            CreatePowerTier(9, "Sovereign Pillar", TierStratum.NationalGuardian, 300, 22000, 10000, 0.85f, true, false);
            CreatePowerTier(10, "Supreme Guardian", TierStratum.NationalGuardian, 350, 35000, 18000, 0.95f, true, false); // Elite house known ceiling
            CreatePowerTier(11, "Primordial Immortal", TierStratum.PrimordialSect, 500, 60000, 30000, 0.98f, true, true);  // 500 Years
            CreatePowerTier(12, "Apex Divinity", TierStratum.PrimordialSect, 1000, 100000, 50000, 1.00f, true, true);       // 1,000 Years
        }

        private static void CreatePowerTier(int level, string title, TierStratum stratum, int lifespan, int cashBurn, int cmpBurn, float difficulty, bool hidePublic, bool hideNobles)
        {
            string path = $"{DataRoot}/PowerTiers/Tier_{level}_{title.Replace(" ", "")}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PowerTierSO>(path);
            var asset = existing != null ? existing : ScriptableObject.CreateInstance<PowerTierSO>();

            asset.tierLevel = level;
            asset.designation = title;
            asset.stratum = stratum;
            asset.maxLifespanYears = lifespan;
            asset.monthlySalaryBonus = level * 750;
            asset.monthlyCashBurn = cashBurn;
            asset.monthlyCMPBurn = cmpBurn;
            asset.breakthroughDifficulty = difficulty;
            asset.isHiddenFromPublic = hidePublic;
            asset.isHiddenFromTopHouses = hideNobles;

            if (existing == null) AssetDatabase.CreateAsset(asset, path);
            else EditorUtility.SetDirty(asset);
        }

        private static void GenerateMilitaryRanks()
        {
            CreateRank("Private Recruit", "E-1", PaygradeType.Enlisted, 1, 1800, 0, 40, false, false);
            CreateRank("Private Second Class", "E-2", PaygradeType.Enlisted, 2, 2050, 150, 45, false, false);
            CreateRank("Private First Class", "E-3", PaygradeType.Enlisted, 3, 2300, 300, 50, false, false);
            CreateRank("Corporal", "E-4", PaygradeType.Enlisted, 4, 2700, 500, 60, true, false); // UNLOCKS SELF TALENT TESTING
            CreateRank("Sergeant", "E-5", PaygradeType.Enlisted, 5, 3200, 750, 65, true, false);
            CreateRank("Staff Sergeant", "E-6", PaygradeType.Enlisted, 6, 3750, 1050, 70, true, false);
            CreateRank("Sergeant First Class", "E-7", PaygradeType.Enlisted, 7, 4400, 1400, 75, true, false);
            CreateRank("Master Sergeant", "E-8", PaygradeType.Enlisted, 8, 5300, 1800, 80, true, true); // UNLOCKS FAMILY TALENT TESTING
            CreateRank("Command Sgt Major", "E-9", PaygradeType.Enlisted, 9, 6400, 2300, 85, true, true);

            CreateRank("2nd Lieutenant", "O-1", PaygradeType.Officer, 10, 3800, 400, 60, true, false);
            CreateRank("1st Lieutenant", "O-2", PaygradeType.Officer, 11, 4700, 600, 65, true, false);
            CreateRank("Captain", "O-3", PaygradeType.Officer, 12, 6200, 1100, 70, true, false);
            CreateRank("Major", "O-4", PaygradeType.Officer, 13, 7400, 1700, 75, true, true);
            CreateRank("Colonel", "O-6", PaygradeType.Officer, 15, 10500, 3200, 80, true, true);
            CreateRank("General (Guardian Pillar)", "O-10", PaygradeType.Officer, 19, 18000, 9000, 90, true, true);
        }

        private static void CreateRank(string title, string paygrade, PaygradeType type, int order, int salary, int merit, int fitness, bool unlockSelf, bool unlockFam)
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
            asset.unlocksSelfTalentTesting = unlockSelf;
            asset.unlocksFamilyTalentTesting = unlockFam;

            if (existing == null) AssetDatabase.CreateAsset(asset, path);
            else EditorUtility.SetDirty(asset);
        }

        private static void GenerateFamilyLineages()
        {
            CreateLineage("4-Star General Dynasty", "General Vance, Chief of Army Staff", NobleRank.GrandDuke, 85, 90, 75, 50000, 2, 50f, true, true,
                "Born with a silver spoon and iron connections. High Command protects you from minor infractions, but expectations are suffocating.");

            CreateLineage("Special Forces Legend", "Major Vance (Medal of Honor)", NobleRank.Count, 50, 85, 60, 20000, 1, 35f, false, true,
                "Your father breached enemy bunkers single-handedly. Soldiers instantly respect you, but expect you to perform miracles.");

            CreateLineage("Command Sgt Major Backbone", "CSM Miller, 75th Rangers", NobleRank.Baron, 35, 75, 50, 10000, 1, 20f, false, false,
                "Enlisted royalty. You know every rule loophole in the book and have the grunts' unwavering trust.");

            CreateLineage("Civilian Clean Slate", "Civilian Farmer / Working Class", NobleRank.Commoner, 0, 40, 50, 2000, 0, 0f, false, false,
                "Zero political pull, zero family serums. Everything must be earned through blood, sweat, and frontline merit.");
        }

        private static void CreateLineage(string name, string father, NobleRank rank, int influence, int respect, int clanFavor, long wealth, int powerTier, float tempering, bool westPoint, bool secretBranch, string desc)
        {
            string path = $"{DataRoot}/Lineages/Lineage_{name.Replace(" ", "")}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<FamilyLineageSO>(path);
            var asset = existing != null ? existing : ScriptableObject.CreateInstance<FamilyLineageSO>();

            asset.archetypeName = name;
            asset.fatherTitle = father;
            asset.startingNobleRank = rank;
            asset.startingInfluence = influence;
            asset.startingRespect = respect;
            asset.startingClanFavor = clanFavor;
            asset.startingFamilyWealth = wealth;
            asset.startingPowerTier = powerTier;
            asset.startingFoundationalTempering = tempering;
            asset.hasWestPointFastTrack = westPoint;
            asset.hasSecretBranchInvitation = secretBranch;
            asset.description = desc;

            if (existing == null) AssetDatabase.CreateAsset(asset, path);
            else EditorUtility.SetDirty(asset);
        }
    }
}
#endif
