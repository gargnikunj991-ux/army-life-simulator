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
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
        }

        private void OnDisable()
        {
            if (bodyTemperingButton != null) bodyTemperingButton.onClick.RemoveListener(OnBodyTemperingClicked);
            if (studyTacticsButton != null) studyTacticsButton.onClick.RemoveListener(OnStudyTacticsClicked);
            if (familyTimeButton != null) familyTimeButton.onClick.RemoveListener(OnFamilyTimeClicked);
            if (siblingSparButton != null) siblingSparButton.onClick.RemoveListener(OnSiblingSparClicked);
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
                    ? $"\n<color=#FFD700>Mock Exam (Age 17.5): <b>{s.mockExamScore}/100</b> ({s.mockExamProjectedAcademy}) | Prep Boost: <b>+{s.examPreparationBonus} pts</b></color>"
                    : (s.age >= 16.0f 
                        ? $"\n<color=#00D4FF>National Mock Exam: Slated for Age 17.5 | Current Prep Bonus: <b>+{s.examPreparationBonus} pts</b></color>" 
                        : "");

                youthStatsSummaryText.text = 
                    $"Fitness: <b>{s.fitness:F0}/100</b>  |  Combat Skill: <b>{s.combatSkill:F0}/100</b>\n" +
                    $"Tactical Intellect: <b>{s.intellect:F0}/100</b>  |  Foundational Tempering: <b>{s.foundationalTempering:F0}/100</b>\n" +
                    $"Parent Relation: <b>{s.parentRelationship:F0}/100</b>  |  Sibling Rivalry Standing: <b>+{s.clanSuccessionScore}</b>" +
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
    }
}
