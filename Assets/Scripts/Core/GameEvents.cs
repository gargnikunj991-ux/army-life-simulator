using System;
using ArmyLife.Data;

namespace ArmyLife.Core
{
    /// <summary>
    /// Global decoupled event bus for Army Life Simulator.
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
        public static Action OnThreePromotionsReached;           // Triggers Family Marriage
        public static Action<int> OnChildBorn;                   // (ChildTalent) New child born
        public static Action<NobleRank> OnNobleTitleInherited;   // Legacy compatibility

        // Imperial Court-Martial / Dishonorable Discharge
        public static Action<string> OnImperialExecutionTriggered; // (Reason for military execution / court-martial)

        // Interactive Military Dilemma Popups
        public static Action<string, string, string, string, string, string, Action, Action, Action> OnMilitaryDilemmaTriggered;
        public static Action<string, string, string, string, string, string, Action, Action, Action> OnChildhoodDilemmaTriggered
        {
            get => OnMilitaryDilemmaTriggered;
            set => OnMilitaryDilemmaTriggered = value;
        }
    }
}
