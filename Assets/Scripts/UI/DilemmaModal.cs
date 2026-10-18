using System;
using ArmyLife.Core;
using ArmyLife.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class DilemmaModal : MonoBehaviour
    {
        public static DilemmaModal Instance { get; private set; }

        [Header("UI Panel Elements")]
        public GameObject modalRoot;
        public TextMeshProUGUI categoryBadgeText;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;

        [Header("Choice A")]
        public Button choiceAButton;
        public TextMeshProUGUI choiceAText;

        [Header("Choice B")]
        public Button choiceBButton;
        public TextMeshProUGUI choiceBText;

        [Header("Choice C")]
        public Button choiceCButton;
        public TextMeshProUGUI choiceCText;

        private Action onChoiceASelected;
        private Action onChoiceBSelected;
        private Action onChoiceCSelected;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnChildhoodDilemmaTriggered += DisplayDilemma;

            if (choiceAButton != null) choiceAButton.onClick.AddListener(OnChoiceAClicked);
            if (choiceBButton != null) choiceBButton.onClick.AddListener(OnChoiceBClicked);
            if (choiceCButton != null) choiceCButton.onClick.AddListener(OnChoiceCClicked);
        }

        private void OnDisable()
        {
            GameEvents.OnChildhoodDilemmaTriggered -= DisplayDilemma;

            if (choiceAButton != null) choiceAButton.onClick.RemoveListener(OnChoiceAClicked);
            if (choiceBButton != null) choiceBButton.onClick.RemoveListener(OnChoiceBClicked);
            if (choiceCButton != null) choiceCButton.onClick.RemoveListener(OnChoiceCClicked);
        }

        public void DisplayDilemma(
            string category, 
            string title, 
            string description, 
            string choiceALabel, 
            string choiceBLabel, 
            string choiceCLabel, 
            Action actionA, 
            Action actionB, 
            Action actionC)
        {
            onChoiceASelected = actionA;
            onChoiceBSelected = actionB;
            onChoiceCSelected = actionC;

            if (categoryBadgeText != null) categoryBadgeText.text = $"<color=#00D4FF><b>[{category.ToUpper()}]</b></color>";
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;

            if (choiceAText != null) choiceAText.text = choiceALabel;
            if (choiceBText != null) choiceBText.text = choiceBLabel;

            if (choiceCButton != null)
            {
                bool hasChoiceC = !string.IsNullOrEmpty(choiceCLabel);
                choiceCButton.gameObject.SetActive(hasChoiceC);
                if (hasChoiceC && choiceCText != null)
                {
                    choiceCText.text = choiceCLabel;
                }
            }

            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }
        }

        private void OnChoiceAClicked()
        {
            onChoiceASelected?.Invoke();
            CloseAndRefresh();
        }

        private void OnChoiceBClicked()
        {
            onChoiceBSelected?.Invoke();
            CloseAndRefresh();
        }

        private void OnChoiceCClicked()
        {
            onChoiceCSelected?.Invoke();
            CloseAndRefresh();
        }

        private void CloseAndRefresh()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }

            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                GameEvents.OnSoldierStatsUpdated?.Invoke(GameManager.Instance.currentSoldier);
            }
        }
    }
}
