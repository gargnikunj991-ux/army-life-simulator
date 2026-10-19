using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class YouthActivitiesModal : MonoBehaviour
    {
        public static YouthActivitiesModal Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject modalRoot;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI youthStatsSummaryText;

        [Header("Activity Buttons")]
        public Button bodyTemperingButton;
        public Button studyTacticsButton;
        public Button cadetFiringRangeButton;
        public Button clanLibraryButton;
        public Button oddJobsButton;
        public Button apothecaryButton;
        public Button familyTimeButton;
        public Button siblingSparButton;
        public Button askAllowanceButton;
        public Button takeMockExamButton;
        public Button askFatherExamHelpButton;
        public Button closeButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null) modalRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (bodyTemperingButton != null) bodyTemperingButton.onClick.AddListener(OnBodyTemperingClicked);
            if (studyTacticsButton != null) studyTacticsButton.onClick.AddListener(OnStudyTacticsClicked);
            if (cadetFiringRangeButton != null) cadetFiringRangeButton.onClick.AddListener(OnCadetFiringRangeClicked);
            if (clanLibraryButton != null) clanLibraryButton.onClick.AddListener(OnClanMartialLibraryClicked);
            if (oddJobsButton != null) oddJobsButton.onClick.AddListener(OnOddJobsClicked);
            if (apothecaryButton != null) apothecaryButton.onClick.AddListener(OnApothecaryClicked);
            if (familyTimeButton != null) familyTimeButton.onClick.AddListener(OnFamilyTimeClicked);
            if (siblingSparButton != null) siblingSparButton.onClick.AddListener(OnSiblingSparClicked);
            if (askAllowanceButton != null) askAllowanceButton.onClick.AddListener(OnAskAllowanceClicked);
            if (takeMockExamButton != null) takeMockExamButton.onClick.AddListener(OnTakeMockExamClicked);
            if (askFatherExamHelpButton != null) askFatherExamHelpButton.onClick.AddListener(OnAskFatherExamHelpClicked);
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
        }

        private void OnDisable()
        {
            if (bodyTemperingButton != null) bodyTemperingButton.onClick.RemoveListener(OnBodyTemperingClicked);
            if (studyTacticsButton != null) studyTacticsButton.onClick.RemoveListener(OnStudyTacticsClicked);
            if (cadetFiringRangeButton != null) cadetFiringRangeButton.onClick.RemoveListener(OnCadetFiringRangeClicked);
            if (clanLibraryButton != null) clanLibraryButton.onClick.RemoveListener(OnClanMartialLibraryClicked);
            if (oddJobsButton != null) oddJobsButton.onClick.RemoveListener(OnOddJobsClicked);
            if (apothecaryButton != null) apothecaryButton.onClick.RemoveListener(OnApothecaryClicked);
            if (familyTimeButton != null) familyTimeButton.onClick.RemoveListener(OnFamilyTimeClicked);
            if (siblingSparButton != null) siblingSparButton.onClick.RemoveListener(OnSiblingSparClicked);
            if (askAllowanceButton != null) askAllowanceButton.onClick.RemoveListener(OnAskAllowanceClicked);
            if (takeMockExamButton != null) takeMockExamButton.onClick.RemoveListener(OnTakeMockExamClicked);
            if (askFatherExamHelpButton != null) askFatherExamHelpButton.onClick.RemoveListener(OnAskFatherExamHelpClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }
            RefreshStatsDisplay();
        }

        public void CloseModal()
        {
            if (modalRoot != null && modalRoot != gameObject)
            {
                modalRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void RefreshStatsDisplay()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (youthStatsSummaryText != null)
            {
                string ageNotice = (s.age < 6.0f) 
                    ? "\n<color=#FF8800><b>BitLife Age Restriction:</b> You are under 6 years old! Most academy activities are locked until Primary School (Age 6.0).</color>"
                    : "";

                string examLine = s.tookMockExam
                    ? $"\n<color=#FFD700><b>National Mock Exam (Age 17.5):</b> {s.mockExamScore}/750 ({s.mockExamProjectedAcademy}) | Prep Boost: +{s.examPreparationBonus * 2.0f:F0} pts</color>"
                    : (s.age >= 16.0f 
                        ? $"\n<color=#00D4FF><b>National Mock Exam:</b> Slated for Age 17.5 | Current Prep Bonus: +{s.examPreparationBonus * 2.0f:F0} pts</color>" 
                        : "");

                youthStatsSummaryText.text = 
                    $"<b>❤️ Health:</b> {s.health:F0}%   |   <b>🏃 Fitness:</b> {s.fitness:F0}%   |   <b>⚔️ Combat:</b> {s.combatSkill:F0}%\n" +
                    $"<b>🧠 Intellect:</b> {s.intellect:F0}%   |   <b>🏡 Family Favor:</b> {s.parentRelationship:F0}%   |   <b>💰 Pocket Money:</b> ${s.cashBalance:N0}\n" +
                    $"<b>🥋 Foundational Tempering:</b> {s.foundationalTempering:F0}/100   |   <b>⚔️ Lucian Standing:</b> +{s.clanSuccessionScore} Favor" +
                    ageNotice + examLine;
            }

            // Update button label tags for clear BitLife-style age gates
            UpdateButtonLabel(studyTacticsButton, "📚 STUDY STRATEGY & TACTICS", s.age >= 6.0f ? "(+Intellect)" : "<color=#FF6666>[Unlocks at Age 6]</color>");
            UpdateButtonLabel(clanLibraryButton, "📜 CLAN MARTIAL ARCHIVE", s.age >= 6.0f ? "(+Qi Insight)" : "<color=#FF6666>[Unlocks at Age 6]</color>");
            UpdateButtonLabel(cadetFiringRangeButton, "🎯 CADET FIRING RANGE", s.age >= 12.0f ? "(+Combat Skill)" : "<color=#FF6666>[Unlocks at Age 12]</color>");
            UpdateButtonLabel(oddJobsButton, "💼 DISTRICT ODD JOBS", s.age >= 12.0f ? "(+Earn Cash)" : "<color=#FF6666>[Unlocks at Age 12]</color>");
            UpdateButtonLabel(apothecaryButton, "🏥 APOTHECARY BONE BROTH", s.age >= 4.0f ? "(-$30, +Health)" : "<color=#FF6666>[Unlocks at Age 4]</color>");
            UpdateButtonLabel(takeMockExamButton, "📝 TAKE MOCK MILITARY EXAM", s.age >= 17.5f ? "(Ready)" : "<color=#FF6666>[Age 17.5 Only]</color>");
            UpdateButtonLabel(askFatherExamHelpButton, "👑 ASK FATHER FOR EXAM HELP", s.age >= 16.0f ? "(Available)" : "<color=#FF6666>[Unlocks at Age 16]</color>");
        }

        private void UpdateButtonLabel(Button btn, string prefix, string suffix)
        {
            if (btn == null) return;
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = $"{prefix} {suffix}";
            }
        }

        private void OnBodyTemperingClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 3.0f)
                {
                    s.fitness = Mathf.Min(100f, s.fitness + 2f);
                    s.health = Mathf.Min(100f, s.health + 2f);
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#00FFAA>[TODDLER CRAWLING]</color> You crawl and wobble vigorously across the courtyard tiles! (+2 Fitness, +2 Health).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                }
                else if (s.age < 6.0f)
                {
                    s.fitness = Mathf.Min(100f, s.fitness + 3f);
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#00FFAA>[COURTYARD PLAY]</color> You chase courtyard birds and run laps around the stone lanterns (+3 Fitness).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                }
                else
                {
                    ChildhoodLifeEngine.PerformBodyTempering(s);
                }
                RefreshStatsDisplay();
            }
        }

        private void OnStudyTacticsClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 6.0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#FF8800><b>[TOO YOUNG FOR SCHOOL]</b></color> You are only Age <b>{s.age:F1}</b>! " +
                        $"Like in BitLife, structured tactical schooling begins at <b>Age 6.0</b> (Primary School). Go play with wooden toys or spend time with Mother!");
                }
                else
                {
                    ChildhoodLifeEngine.PerformStudyTactics(s);
                }
                RefreshStatsDisplay();
            }
        }

        private void OnCadetFiringRangeClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 12.0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#FF8800><b>[WEAPON RESTRICTION]</b></color> Live energy carbine qualification is strictly restricted to cadets aged <b>12 and older</b>! " +
                        $"You are currently only Age <b>{s.age:F1}</b>. The range sergeant waves you away.");
                }
                else
                {
                    ChildhoodLifeEngine.PerformCadetFiringRange(s);
                }
                RefreshStatsDisplay();
            }
        }

        private void OnClanMartialLibraryClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 6.0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#FF8800><b>[FORBIDDEN ARCHIVE]</b></color> The clan guards block the iron doors: " +
                        $"<i>'Toddlers cannot read ancient meridian charts! Come back when you turn 6.'</i> (Unlocks at Age 6.0).");
                }
                else
                {
                    ChildhoodLifeEngine.PerformClanMartialLibrary(s);
                }
                RefreshStatsDisplay();
            }
        }

        private void OnOddJobsClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 12.0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#FF8800><b>[CHILD LABOR RESTRICTION]</b></color> Imperial labor laws forbid hiring children under Age <b>12.0</b>! " +
                        $"You are currently only Age <b>{s.age:F1}</b>. Ask your parents for an allowance instead.");
                }
                else
                {
                    ChildhoodLifeEngine.PerformStreetErrandsAndJobs(s);
                }
                RefreshStatsDisplay();
            }
        }

        private void OnApothecaryClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 4.0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#FF8800><b>[DIGESTION RESTRICTION]</b></color> An infant's stomach cannot digest concentrated celestial marrow herbs! " +
                        $"Mother feeds you sweet honey and warm milk instead (+2 Health).");
                    s.health = Mathf.Min(100f, s.health + 2f);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                }
                else
                {
                    ChildhoodLifeEngine.PerformApothecaryNourishment(s);
                }
                RefreshStatsDisplay();
            }
        }

        private void OnFamilyTimeClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                ChildhoodLifeEngine.PerformFamilyBonding(s);
                RefreshStatsDisplay();
            }
        }

        private void OnSiblingSparClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 4.0f)
                {
                    s.siblingRelationship = Mathf.Min(100f, s.siblingRelationship + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#FF8844>[FAMILY PLAY]</color> You and your brother {s.siblingRivalName} playfully roll around on the rug with baby toys (+5 Sibling Bond).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                }
                else
                {
                    ChildhoodLifeEngine.PerformSiblingSparring(s);
                }
                RefreshStatsDisplay();
            }
        }

        private void OnAskAllowanceClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                s.parentRelationship = Mathf.Min(100f, s.parentRelationship + 4f);
                int allowance = (s.nobleTitle > NobleRank.Commoner) ? 100 : 35;
                s.cashBalance += allowance;
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<color=#FFD700>[ALLOWANCE RECEIVED]</color> You asked your parents for pocket money. Father smiles and hands you <b>+${allowance}</b>! (Parent Relationship: {s.parentRelationship:F0}/100, Pocket Money: ${s.cashBalance:N0})");
                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                RefreshStatsDisplay();
            }
        }

        private void OnTakeMockExamClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 17.5f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACADEMY", 
                        $"[MOCK EXAM] The National Vanguard Mock Exam is officially administered at Age 17.5. You are currently Age {s.age:F1}. Continue studying tactics and tempering your body!");
                }
                else
                {
                    ChildhoodLifeEngine.ConductMockExam(s);
                }
                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                RefreshStatsDisplay();
            }
        }

        private void OnAskFatherExamHelpClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 16.0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                        $"[FAMILY] Father gently ruffles your hair: <i>'Focus on your school lessons and growing healthy, child. When you turn 16, our family will prepare for the National Military Exam.'</i>");
                }
                else
                {
                    ChildhoodLifeEngine.RequestFatherExamIntervention(s);
                }
                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                RefreshStatsDisplay();
            }
        }
    }
}
