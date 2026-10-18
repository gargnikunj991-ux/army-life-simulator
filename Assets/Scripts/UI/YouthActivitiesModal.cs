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
            if (familyTimeButton != null) familyTimeButton.onClick.RemoveListener(OnFamilyTimeClicked);
            if (siblingSparButton != null) siblingSparButton.onClick.RemoveListener(OnSiblingSparClicked);
            if (askAllowanceButton != null) askAllowanceButton.onClick.RemoveListener(OnAskAllowanceClicked);
            if (takeMockExamButton != null) takeMockExamButton.onClick.RemoveListener(OnTakeMockExamClicked);
            if (askFatherExamHelpButton != null) askFatherExamHelpButton.onClick.RemoveListener(OnAskFatherExamHelpClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
        }

        public void OpenModal()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
                RefreshStatsDisplay();
            }
        }

        public void CloseModal()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
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
                ChildhoodLifeEngine.PerformBodyTempering(s);
                RefreshStatsDisplay();
            }
        }

        private void OnStudyTacticsClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                ChildhoodLifeEngine.PerformStudyTactics(s);
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
                ChildhoodLifeEngine.PerformSiblingSparring(s);
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
                ChildhoodLifeEngine.RequestFatherExamIntervention(s);
                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                RefreshStatsDisplay();
            }
        }
    }
}
