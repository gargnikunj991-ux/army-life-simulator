using System;
using UnityEngine;

namespace ArmyLife.Data
{
    public enum SquadRole
    {
        CombatMedic,        // Heals and can rescue from fatal wounds
        LeadScout,          // Provides early warning against ambushes & traps
        HeavyGunner,        // Provides suppressing fire, lowering squad casualties
        RiflemanRookie      // Young recruit needing mentorship; high loyalty growth
    }

    [Serializable]
    public class SquadMate
    {
        public string id;
        public string name;
        public string nickname;
        public SquadRole role;
        public string roleTitle;
        [Range(0f, 100f)] public float relationship = 50f;
        public bool isDead;
        public string deathReason;
        public string personalityTrait; // e.g. "Cynical Veteran", "Warm-Hearted Medic", "Nervous Farmboy"
        public string hometown = "Imperial Capital Province";
        public string backstoryAndQuirk = "A reliable soldier forged in the line of duty.";
        public int battlesSurvived;

        [Header("Tactical Readiness & Morale")]
        [Range(0f, 100f)] public float morale = 75f;
        [Range(0f, 100f)] public float fatigue = 15f;
        public bool isInjured = false;
        public string injuryStatus = "Fit for Duty";

        public SquadMate() {}

        public SquadMate(string name, string nickname, SquadRole role, string roleTitle, string trait, float initialRelationship = 50f)
        {
            this.id = Guid.NewGuid().ToString("N").Substring(0, 8);
            this.name = name;
            this.nickname = nickname;
            this.role = role;
            this.roleTitle = roleTitle;
            this.personalityTrait = trait;
            this.relationship = initialRelationship;
            this.isDead = false;
            this.battlesSurvived = 0;
            this.morale = 75f;
            this.fatigue = 15f;
            this.isInjured = false;
            this.injuryStatus = "Fit for Duty";
        }

        public void ApplyRestAndRecovery(float fatigueRecovery = 25f, float moraleRecovery = 15f)
        {
            fatigue = Mathf.Max(0f, fatigue - fatigueRecovery);
            morale = Mathf.Min(100f, morale + moraleRecovery);
            if (isInjured)
            {
                isInjured = false;
                injuryStatus = "Fit for Duty";
            }
        }

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(nickname) ? name : $"{name} \"{nickname}\"";
        }
    }
}
