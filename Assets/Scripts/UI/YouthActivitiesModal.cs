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
                string examLine = s.tookMockExam
                    ? $"\n<color=#FFD700><b>National Mock Exam (Age 17.5):</b> {s.mockExamScore}/100 ({s.mockExamProjectedAcademy}) | Prep Boost: +{s.examPreparationBonus} pts</color>"
                    : (s.age >= 16.0f 
                        ? $"\n<color=#00D4FF><b>National Mock Exam:</b> Slated for Age 17.5 | Current Prep Bonus: +{s.examPreparationBonus} pts</color>" 
                        : "");

                youthStatsSummaryText.text = 
                    $"<b>❤️ Health:</b> {s.health:F0}%   |   <b>🏃 Fitness:</b> {s.fitness:F0}%   |   <b>⚔️ Combat:</b> {s.combatSkill:F0}%\n" +
                    $"<b>🧠 Intellect:</b> {s.intellect:F0}%   |   <b>🏡 Family Favor:</b> {s.parentRelationship:F0}%   |   <b>💰 Pocket Money:</b> ${s.cashBalance:N0}\n" +
                    $"<b>🥋 Foundational Tempering:</b> {s.foundationalTempering:F0}/100   |   <b>⚔️ Lucian Standing:</b> +{s.clanSuccessionScore} Favor" +
                    examLine;
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
                        $"<color=#00FFAA>[EARLY ACTIVITY]</color> You crawl and wobble vigorously across the courtyard tiles! (+2 Fitness, +2 Health).");
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
                if (s.age < 2.0f)
                {
                    s.intellect = Mathf.Min(100f, s.intellect + 2f);
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#00AAFF>[EARLY ACTIVITY]</color> You stare intently and chew on the corner of a brightly illustrated military picture book! (+2 Intellect).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
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
                ChildhoodLifeEngine.PerformCadetFiringRange(s);
                RefreshStatsDisplay();
            }
        }

        private void OnClanMartialLibraryClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                if (s.age < 5.0f)
                {
                    s.intellect = Mathf.Min(100f, s.intellect + 2f);
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#00D4FF>[EARLY ACTIVITY]</color> You flip curiously through picture scrolls of ancient heroes in the study room (+2 Intellect).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
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
                if (s.age < 4.0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                        $"<color=#FFD700>[TODDLER PLAY]</color> You are too young to work district odd jobs! Mother gives you a warm sweet dumpling instead.");
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
                ChildhoodLifeEngine.PerformApothecaryNourishment(s);
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
                        $"<color=#FF8844>[FAMILY PLAY]</color> You and your brother {s.siblingRivalName} playfully roll around on the rug with wooden toy swords (+5 Sibling Bond).");
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
