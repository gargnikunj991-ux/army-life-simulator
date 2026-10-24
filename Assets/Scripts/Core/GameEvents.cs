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

        // Military Qualification Events (Deprecated Cultivation Hooks)
        [Obsolete("Deprecated in pure military sim.")]
        public static Action OnSecretCaveDiscovered;
        [Obsolete("Deprecated in pure military sim.")]
        public static Action<int> OnPowerTierBreakthrough;       // (NewReadinessTier)

        // Imperial Retribution
        public static Action<string> OnImperialExecutionTriggered; // (Reason for royal decapitation)

        // Interactive Dilemma Popups (Childhood & Military Career)
        public static Action<string, string, string, string, string, string, Action, Action, Action> OnChildhoodDilemmaTriggered;
    }
}
