using System;
using ArmyLife.Data;

namespace ArmyLife.Core
{
    /// <summary>
    /// Global decoupled event bus.
    /// UI screens and subsystem managers subscribe here without tight coupling.
    /// </summary>
    public static class GameEvents
    {
        // State & Turn Events
        public static Action<SoldierProfile> OnSoldierStatsUpdated;
        public static Action<int, int, bool> OnSemesterAdvanced; // (Year, Semester [1 or 2], IsDeploymentTour)

        // Narrative Feed & Timeline
        public static Action<string, string> OnFeedEventLogged; // (CategoryTag, Message)

        // Dynastic & Marriage Milestones
        public static Action OnThreePromotionsReached;           // Triggers Arranged Marriage
        public static Action<int> OnChildBorn;                   // (ChildTalent) Triggers Soul Transfer Prompt
        public static Action<NobleRank> OnNobleTitleInherited;   // Player becomes Duke / Clan Head

        // Cultivation & Ancient Sect Events
        public static Action OnSecretCaveDiscovered;             // 1-in-100 anomaly trigger
        public static Action<int> OnPowerTierBreakthrough;       // (NewTierLevel)

        // Imperial Retribution
        public static Action<string> OnImperialExecutionTriggered; // (Reason for royal decapitation)
    }
}
