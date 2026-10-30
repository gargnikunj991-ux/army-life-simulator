using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    /// <summary>
    /// Official Military Service Jacket & Personnel Dossier.
    /// Presents soldier credentials, physical vitals, military classification, and combat service history.
    /// </summary>
    public class LifeModal : MonoBehaviour
    {
        public static LifeModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header Identity")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI characterBioText;
        public Button closeButton;

        [Header("Scroll Hierarchy")]
        public ScrollRect lifeScrollRect;
        public RectTransform contentContainer;

        [Header("Section 1: Physical Status & Vital Signs")]
        public TextMeshProUGUI physicalStatusText;
        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        public Slider fitnessSlider;
        public TextMeshProUGUI fitnessText;

        [Header("Section 2: Military Standing & Classification")]
        public TextMeshProUGUI powerRealmText; // Military Rank & Unit Classification
        public Slider qiProgressSlider;        // Combat Readiness Index Slider
        public TextMeshProUGUI qiProgressText;
        public TextMeshProUGUI talentScoreText; // ASVAB / Technical Aptitude

        [Header("Section 3: Military Service Jacket")]
        public TextMeshProUGUI militaryRecordText;
        public Slider combatSkillSlider;
        public TextMeshProUGUI combatSkillText;
        public Slider intellectSlider;
        public TextMeshProUGUI intellectText;

        [Header("Section 4: Family Heritage & Standing")]
        public TextMeshProUGUI dynasticStatusText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            EnsureUIHierarchy();

            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            EnsureUIHierarchy();
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            ThemeManager.OnThemeChanged += OnThemeChanged;

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            ApplyThemePalette(p);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void EnsureUIHierarchy()
        {
            if (lifeScrollRect == null)
            {
                lifeScrollRect = GetComponentInChildren<ScrollRect>(true);
            }

            if (lifeScrollRect != null)
            {
                lifeScrollRect.horizontal = false;
                lifeScrollRect.vertical = true;
                lifeScrollRect.scrollSensitivity = 45f;
                lifeScrollRect.movementType = ScrollRect.MovementType.Clamped;

                if (contentContainer == null && lifeScrollRect.content != null)
                {
                    contentContainer = lifeScrollRect.content;
                }
            }

            if (modalRoot == null)
            {
                var overlay = transform.Find("LifeOverlay");
                modalRoot = (overlay != null) ? overlay.gameObject : gameObject;
            }
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            EnsureUIHierarchy();
            RefreshUI();

            Canvas.ForceUpdateCanvases();
            if (contentContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
            }
            if (lifeScrollRect != null)
            {
                lifeScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void RefreshUI()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            int curYear = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2026;
            int birthYear = curYear - Mathf.FloorToInt(s.age);

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);

            string goldHex = ColorUtility.ToHtmlStringRGB(p.accentGold);
            string greenHex = ColorUtility.ToHtmlStringRGB(p.accentEmerald);
            string redHex = ColorUtility.ToHtmlStringRGB(p.accentCrimson);
            string blueHex = ColorUtility.ToHtmlStringRGB(p.accentCobalt);

            if (titleText != null)
            {
                titleText.text = $"MILITARY PERSONNEL DOSSIER: <b>{s.characterName.ToUpper()}</b>";
            }

            // SECTION 1: Personal Bio & Enlistment Record
            if (characterBioText != null)
            {
                string statusTag = s.isDead ? $"<color=#{redHex}>[K.I.A. / DECEASED]</color>" : 
                                   s.isRetired ? $"<color=#{goldHex}>[HONORABLE RETIRED VETERAN]</color>" :
                                   s.isInCombatDeployment ? $"<color=#{redHex}>[ACTIVE FRONTLINE DEPLOYMENT]</color>" :
                                   $"<color=#{greenHex}>[ACTIVE GARRISON SERVICE]</color>";

                string originDesc = s.lineage != null ? s.lineage.archetypeName : s.origin.ToString();

                characterBioText.text = 
                    $"<color=#{goldHex}><b>[SECTION 1: RECRUIT IDENTIFICATION & ORIGIN]</b></color>\n" +
                    $"<b>Service Status:</b> {statusTag}   |   <b>Age:</b> {s.age:F1} (Born {birthYear})\n" +
                    $"<b>Branch & Homeland:</b> {s.militaryBranch} • {s.birthCity ?? "Capital"}, {s.birthCountry ?? "Valerian Empire"}\n" +
                    $"<b>Heritage Background:</b> {originDesc} ({s.originTrait})";
            }

            // SECTION 2: Physical Fitness & Combat Readiness
            if (physicalStatusText != null)
            {
                physicalStatusText.text = 
                    $"<color=#{goldHex}><b>[SECTION 2: PHYSICAL FITNESS & VITALS]</b></color>\n" +
                    $"<b>Combat Readiness Index:</b> <color=#{greenHex}>{s.combatReadiness:F0}%</color>   |   <b>Discipline:</b> {s.discipline:F0}%\n" +
                    $"<b>Combat Stress Level:</b> <color=#{redHex}>{s.combatStress:F0}%</color>   |   <b>Morale:</b> {s.happiness:F0}%";
            }

            if (healthSlider != null) healthSlider.value = s.health / 100f;
            if (healthText != null) healthText.text = $"Vital Health: {s.health:F0}%";

            if (fitnessSlider != null) fitnessSlider.value = s.fitness / 100f;
            if (fitnessText != null) fitnessText.text = $"Physical Stamina (PT): {s.fitness:F0}%";

            // SECTION 3: Military Standing & Department Classification
            if (powerRealmText != null)
            {
                string rankStr = s.GetRankDisplayName();
                string sfTag = s.isSpecialForces ? " • [Special Operations]" : "";
                powerRealmText.text = 
                    $"<color=#{goldHex}><b>[SECTION 3: MILITARY CLASSIFICATION & STANDING]</b></color>\n" +
                    $"<b>Current Rank:</b> <color=#{goldHex}><b>{rankStr}</b></color>{sfTag}\n" +
                    $"<b>Assigned Post:</b> {s.militaryBranch} Forces • {s.assignedDepartment ?? "Frontline Division"}";
            }

            if (qiProgressSlider != null)
            {
                qiProgressSlider.gameObject.SetActive(true);
                qiProgressSlider.value = s.combatReadiness / 100f;
            }
            if (qiProgressText != null)
            {
                qiProgressText.gameObject.SetActive(true);
                qiProgressText.text = $"Readiness Rating: {s.combatReadiness:F0}%";
            }

            if (talentScoreText != null)
            {
                talentScoreText.text = $"<b>Technical & Tactical Aptitude (AFQT):</b> <color=#{goldHex}><b>{s.tacticalIntellect:F0}/100</b></color>";
            }

            // SECTION 4: Combat Skills & Compensation
            if (militaryRecordText != null)
            {
                long netPay = SalaryCalculator.CalculateNetMonthlyPay(s);
                militaryRecordText.text = 
                    $"<color=#{goldHex}><b>[SECTION 4: SERVICE COMPENSATION & COMBAT STATS]</b></color>\n" +
                    $"<b>Monthly Net Pay:</b> +${netPay:N0}/mo   |   <b>Bank Balance:</b> ${s.cashBalance:N0}\n" +
                    $"<b>Combat Merit Points:</b> {s.combatMeritPoints} CMP   |   <b>Wound Stars:</b> {s.GetWoundStarsCount()}";
            }

            if (combatSkillSlider != null) combatSkillSlider.value = s.combatSkill / 100f;
            if (combatSkillText != null) combatSkillText.text = $"Combat Skill: {s.combatSkill:F0}%";

            if (intellectSlider != null) intellectSlider.value = s.intellect / 100f;
            if (intellectText != null) intellectText.text = $"Tactical Intellect: {s.intellect:F0}%";

            // SECTION 5: Family & Squad Connections
            if (dynasticStatusText != null)
            {
                dynasticStatusText.text = 
                    $"<color=#{goldHex}><b>[SECTION 5: FIRETEAM & FAMILY RELATIONS]</b></color>\n" +
                    $"<b>Squad Cohesion:</b> {s.squadRespect:F0}%   |   <b>Commanding Officer Favor:</b> {s.coFavor:F0}%\n" +
                    $"<b>Family Remittance Bond:</b> {s.parentRelationship:F0}%";
            }
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (modalBackdropImage != null)
                modalBackdropImage.color = new Color(0f, 0f, 0f, 0.88f);
            if (dialogPanelImage != null)
                dialogPanelImage.color = p.cardBackground;
            if (titleText != null)
                titleText.color = p.accentGold;
        }
    }
}
