using System;
using System.Collections.Generic;
using System.IO;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class LifePersistenceManager
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "saved_lives.json");

        public static LifeSaveContainer LoadContainer()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var container = JsonUtility.FromJson<LifeSaveContainer>(json);
                    if (container != null && container.lives != null)
                    {
                        return container;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LifePersistenceManager] Error reading saved lives: {ex.Message}");
            }

            return new LifeSaveContainer();
        }

        public static void SaveContainer(LifeSaveContainer container)
        {
            try
            {
                string dir = Path.GetDirectoryName(SaveFilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonUtility.ToJson(container, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LifePersistenceManager] Error writing saved lives: {ex.Message}");
            }
        }

        public static int GetSavedLivesCount()
        {
            return LoadContainer().lives.Count;
        }

        public static List<SavedLifeData> GetAllLives()
        {
            return LoadContainer().lives;
        }

        public static SavedLifeData GetActiveOrMostRecentLife()
        {
            var container = LoadContainer();
            if (container.lives.Count == 0) return null;

            if (!string.IsNullOrEmpty(container.activeLifeId))
            {
                var active = container.lives.Find(l => l.lifeId == container.activeLifeId);
                if (active != null) return active;
            }

            return container.lives[container.lives.Count - 1];
        }

        public static void SaveSoldierLife(SoldierProfile soldier)
        {
            if (soldier == null) return;

            if (string.IsNullOrEmpty(soldier.lifeId))
            {
                soldier.lifeId = Guid.NewGuid().ToString("N");
            }

            var container = LoadContainer();
            var existing = container.lives.Find(l => l.lifeId == soldier.lifeId);

            if (existing == null)
            {
                existing = new SavedLifeData { lifeId = soldier.lifeId };
                container.lives.Add(existing);
            }

            existing.characterName = soldier.characterName;
            existing.firstName = soldier.firstName;
            existing.lastName = soldier.lastName;
            existing.gender = soldier.gender;
            existing.birthCountry = soldier.birthCountry;
            existing.hairStyle = soldier.hairStyle;
            existing.hairColor = soldier.hairColor;
            existing.eyeColor = soldier.eyeColor;
            existing.skinTone = soldier.skinTone;
            existing.looks = soldier.looks;
            existing.age = soldier.age;
            existing.totalSemestersLived = soldier.totalSemestersLived;
            existing.lineageArchetypeName = soldier.lineage != null ? soldier.lineage.archetypeName : "Civilian Clean Slate";
            existing.nobleTitle = soldier.nobleTitle;
            existing.rankPaygrade = soldier.currentRank != null ? soldier.currentRank.paygrade : "E-1";
            existing.rankName = soldier.currentRank != null ? soldier.currentRank.rankName : "Private Recruit";
            existing.physicalFitness = soldier.physicalFitness;
            existing.marksmanshipScore = soldier.marksmanshipScore;
            existing.tacticalIntellect = soldier.tacticalIntellect;
            existing.discipline = soldier.discipline;
            existing.combatReadiness = soldier.combatReadiness;
            existing.cashBalance = soldier.cashBalance;
            existing.combatMeritPoints = soldier.combatMeritPoints;
            existing.health = soldier.health;
            existing.fitness = soldier.physicalFitness;
            existing.combatSkill = soldier.combatSkill;
            existing.isDead = soldier.isDead;
            existing.deathReason = soldier.deathReason;
            existing.isRetired = soldier.isRetired;
            existing.examScore = soldier.examScore;
            existing.mockExamScore = soldier.mockExamScore;
            existing.lastPlayedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            existing.serializedProfileJson = JsonUtility.ToJson(soldier);

            container.activeLifeId = soldier.lifeId;
            SaveContainer(container);

            Debug.Log($"[LifePersistenceManager] Life '{soldier.characterName}' (ID: {soldier.lifeId}) saved successfully.");
        }

        public static SoldierProfile LoadSoldierLife(string lifeId, GameManager gm)
        {
            var container = LoadContainer();
            var entry = container.lives.Find(l => l.lifeId == lifeId);
            if (entry == null)
            {
                Debug.LogWarning($"[LifePersistenceManager] Could not find life ID: {lifeId}");
                return null;
            }

            SoldierProfile soldier = JsonUtility.FromJson<SoldierProfile>(entry.serializedProfileJson);
            if (soldier == null)
            {
                soldier = new SoldierProfile(entry.characterName, null, 50)
                {
                    lifeId = entry.lifeId,
                    age = entry.age,
                    totalSemestersLived = entry.totalSemestersLived,
                    nobleTitle = entry.nobleTitle,
                    physicalFitness = entry.physicalFitness > 0f ? entry.physicalFitness : (entry.fitness > 0f ? entry.fitness : 50f),
                    marksmanshipScore = entry.marksmanshipScore > 0f ? entry.marksmanshipScore : 50f,
                    tacticalIntellect = entry.tacticalIntellect > 0f ? entry.tacticalIntellect : 50f,
                    discipline = entry.discipline > 0f ? entry.discipline : 60f,
                    combatReadiness = entry.combatReadiness > 0f ? entry.combatReadiness : 50f,
                    cashBalance = entry.cashBalance,
                    combatMeritPoints = entry.combatMeritPoints,
                    health = entry.health,
                    combatSkill = entry.combatSkill,
                    isDead = entry.isDead,
                    deathReason = entry.deathReason,
                    isRetired = entry.isRetired,
                    gender = entry.gender ?? "Male",
                    birthCountry = entry.birthCountry ?? "The Valerian Empire",
                    hairStyle = entry.hairStyle ?? "Military Buzzcut",
                    hairColor = entry.hairColor ?? "Jet Black",
                    eyeColor = entry.eyeColor ?? "Steel Grey",
                    skinTone = entry.skinTone ?? "Tan",
                    looks = entry.looks > 0 ? entry.looks : 80f
                };
            }

            // Re-link ScriptableObject references from GameManager databases
            if (gm != null)
            {
                if (gm.familyLineageDatabase != null && gm.familyLineageDatabase.Length > 0)
                {
                    foreach (var lineage in gm.familyLineageDatabase)
                    {
                        if (lineage != null && lineage.archetypeName == entry.lineageArchetypeName)
                        {
                            soldier.lineage = lineage;
                            break;
                        }
                    }

                    if (soldier.lineage == null)
                    {
                        soldier.lineage = gm.familyLineageDatabase[0];
                    }
                }

                if (gm.militaryRankDatabase != null && gm.militaryRankDatabase.Length > 0)
                {
                    foreach (var rank in gm.militaryRankDatabase)
                    {
                        if (rank != null && (rank.paygrade == entry.rankPaygrade || rank.rankName == entry.rankName))
                        {
                            soldier.currentRank = rank;
                            break;
                        }
                    }

                    if (soldier.currentRank == null)
                    {
                        soldier.currentRank = gm.militaryRankDatabase[0];
                    }
                }
            }

            container.activeLifeId = lifeId;
            SaveContainer(container);

            return soldier;
        }

        public static void DeleteLife(string lifeId)
        {
            var container = LoadContainer();
            container.lives.RemoveAll(l => l.lifeId == lifeId);
            if (container.activeLifeId == lifeId)
            {
                container.activeLifeId = container.lives.Count > 0 ? container.lives[0].lifeId : null;
            }
            SaveContainer(container);
            Debug.Log($"[LifePersistenceManager] Life {lifeId} deleted.");
        }
    }
}
