using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyLife.Data
{
    [Serializable]
    public class SavedLifeData
    {
        public string lifeId;
        public string characterName;
        public string firstName;
        public string lastName;
        public string gender;
        public string birthCountry;
        public string hairStyle;
        public string hairColor;
        public string eyeColor;
        public string skinTone;
        public float looks;
        public float age;
        public int totalSemestersLived;
        public string lineageArchetypeName;
        public NobleRank nobleTitle;
        public string rankPaygrade;
        public string rankName;
        public int powerTier;
        public int talentScore;
        public bool isPersonalTalentRevealed;
        public long cashBalance;
        public long combatMeritPoints;
        public float health;
        public float fitness;
        public float combatSkill;
        public bool isDead;
        public string deathReason;
        public bool isRetired;
        public int examScore;
        public int mockExamScore;
        public string lastPlayedDate;

        [TextArea(3, 10)]
        public string serializedProfileJson;
    }

    [Serializable]
    public class LifeSaveContainer
    {
        public List<SavedLifeData> lives = new List<SavedLifeData>();
        public string activeLifeId;
    }
}
