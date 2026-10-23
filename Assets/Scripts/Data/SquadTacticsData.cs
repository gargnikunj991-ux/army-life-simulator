using System;
using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Tactical orders that an NCO (Sergeant E-5+) or Commissioned Officer (O-1+) can execute.
    /// </summary>
    public enum TacticalCommandOrder
    {
        FrontalAssault,
        ScoutFlankManeuver,
        HeavySuppressionPin,
        DangerCloseArtilleryCall,
        OrderlyFightingRetreat,
        HoldDefenseToTheLast
    }

    /// <summary>
    /// Data structure representing the resolution of a tactical command order.
    /// </summary>
    [Serializable]
    public class CommandOutcome
    {
        public bool success;
        public int casualties;
        public float combatSkillChange;
        public float meritChange;
        public string headline;
        public string feedMessage;

        // Convenient properties for flexible access
        public bool Success { get => success; set => success = value; }
        public bool isSuccess { get => success; set => success = value; }
        public int CasualtyCount { get => casualties; set => casualties = value; }
        public int casualtyCount { get => casualties; set => casualties = value; }
        public int subordinateCasualties { get => casualties; set => casualties = value; }
        public float CombatSkillChange { get => combatSkillChange; set => combatSkillChange = value; }
        public float combatSkillDelta { get => combatSkillChange; set => combatSkillChange = value; }
        public float MeritChange { get => meritChange; set => meritChange = value; }
        public float meritDelta { get => meritChange; set => meritChange = value; }
        public string HeadlineText { get => headline; set => headline = value; }
        public string headlineText { get => headline; set => headline = value; }
        public string FeedMessage { get => feedMessage; set => feedMessage = value; }

        public CommandOutcome() {}

        public CommandOutcome(bool success, int casualties, float combatSkillChange, float meritChange, string headline, string feedMessage)
        {
            this.success = success;
            this.casualties = casualties;
            this.combatSkillChange = combatSkillChange;
            this.meritChange = meritChange;
            this.headline = headline;
            this.feedMessage = feedMessage;
        }
    }

    /// <summary>
    /// Representation of a military theater of operations for tactical maneuvers and casualty evaluation.
    /// Bridges seamlessly with existing WarTheaterType and WarTheaterInfo.
    /// </summary>
    [Serializable]
    public class WarTheater
    {
        public WarTheaterType theaterType = WarTheaterType.WesternMudTrenches;
        public string theaterName = "The Western Front";
        public float hazardRating = 1.0f;
        public string environmentDescription = "Mud and trenches";

        public WarTheater() {}

        public WarTheater(WarTheaterType type, string name, float hazard = 1.0f, string desc = "")
        {
            this.theaterType = type;
            this.theaterName = name;
            this.hazardRating = hazard;
            this.environmentDescription = desc;
        }

        public static WarTheater FromType(WarTheaterType type)
        {
            var info = WarTheaterInfo.GetInfo(type);
            return new WarTheater(type, info.theaterName, info.baseHazardModifier, info.description);
        }

        public static implicit operator WarTheater(WarTheaterInfo info)
        {
            if (info == null) return null;
            return new WarTheater(info.theaterType, info.theaterName, info.baseHazardModifier, info.description);
        }

        public static implicit operator WarTheater(WarTheaterType type)
        {
            return FromType(type);
        }

        public static implicit operator WarTheaterInfo(WarTheater theater)
        {
            if (theater == null) return null;
            return WarTheaterInfo.GetInfo(theater.theaterType);
        }
    }
}
