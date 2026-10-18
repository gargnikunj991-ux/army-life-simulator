using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Core
{
    public class TimeCycleManager : MonoBehaviour
    {
        public static TimeCycleManager Instance { get; private set; }

        [Header("Simulation Clock")]
        public int currentYear = 2008;
        public int currentSemester = 1;     // 1 = Spring/Summer, 2 = Autumn/Winter
        public bool isDeploymentSemester => currentSemester == 2;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ResetClockForNewborn(int birthYear = 2008)
        {
            currentYear = birthYear;
            currentSemester = 1;
        }

        /// <summary>
        /// Advances the simulation clock by 6 months (+0.5 years).
        /// </summary>
        public void AdvanceSemester(SoldierProfile soldier)
        {
            if (soldier == null) return;

            soldier.age += 0.5f;
            soldier.totalSemestersLived++;

            // Toggle between Semester 1 and Semester 2
            if (currentSemester == 1)
            {
                currentSemester = 2;
                soldier.isInCombatDeployment = !soldier.isYouthPhase; // Deployment tour during adult service
            }
            else
            {
                currentSemester = 1;
                currentYear++;
                soldier.isInCombatDeployment = false; // Base refit & training tour
            }

            string season;
            if (soldier.isYouthPhase)
            {
                season = (currentSemester == 1) ? "Spring-Summer (Academic Term)" : "Autumn-Winter (Winter Term)";
            }
            else
            {
                season = (currentSemester == 1) ? "Spring-Summer (Training & Base Refit)" : "Autumn-Winter (Operational Tour)";
            }

            GameEvents.OnFeedEventLogged?.Invoke("CALENDAR", 
                $"<b>--- Year {currentYear}, Semester {currentSemester} ({season}) | Age {soldier.age:F1} ---</b>");

            GameEvents.OnSemesterAdvanced?.Invoke(currentYear, currentSemester, soldier.isInCombatDeployment);
        }
    }
}
