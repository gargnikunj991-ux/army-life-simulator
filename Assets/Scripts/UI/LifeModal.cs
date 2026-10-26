using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
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

        [Header("Section 1: Body & Vitality")]
        public TextMeshProUGUI physicalStatusText;
        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        public Slider fitnessSlider;
        public TextMeshProUGUI fitnessText;

        [Header("Section 2: Military Standing & Classification")]
        public TextMeshProUGUI powerRealmText;
        public Slider qiProgressSlider;
        public TextMeshProUGUI qiProgressText;
        public TextMeshProUGUI talentScoreText;

        [Header("Section 3: Military Service Jacket")]
        public TextMeshProUGUI militaryRecordText;
        public Slider combatSkillSlider;
        public TextMeshProUGUI combatSkillText;
        public Slider intellectSlider;
        public TextMeshProUGUI intellectText;

        [Header("Section 4: Dynasty & Noble Standing")]
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

        private void Update()
        {
            // Direct mouse wheel scroll support across PC platforms
            if (lifeScrollRect != null && modalRoot != null && modalRoot.activeInHierarchy)
            {
                float wheel = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(wheel) > 0.001f)
                {
                    lifeScrollRect.verticalNormalizedPosition = Mathf.Clamp01(lifeScrollRect.verticalNormalizedPosition + wheel * 0.25f);
                }
            }
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

                // Ensure viewport has an Image with raycastTarget enabled to capture mouse wheel & drag events
                if (lifeScrollRect.viewport != null)
                {
                    var vpImg = lifeScrollRect.viewport.GetComponent<Image>();
                    if (vpImg == null)
                    {
                        vpImg = lifeScrollRect.viewport.gameObject.AddComponent<Image>();
                        vpImg.color = Color.clear;
                    }
                    vpImg.raycastTarget = true;
                }

                if (contentContainer == null && lifeScrollRect.content != null)
                {
                    contentContainer = lifeScrollRect.content;
                }
            }

            if (contentContainer != null)
            {
                var vlg = contentContainer.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    vlg.spacing = 14f;
                    vlg.padding = new RectOffset(16, 16, 16, 32);
                    vlg.childControlHeight = true;
                    vlg.childControlWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.childForceExpandWidth = true;
                }

                var csf = contentContainer.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // Strip any fixed preferred heights on text elements so ContentSizeFitter calculates true scroll height
                var textElements = new[] { characterBioText, physicalStatusText, powerRealmText, talentScoreText, militaryRecordText, dynasticStatusText };
                foreach (var t in textElements)
                {
                    if (t != null)
                    {
                        t.enableWordWrapping = true;
                        t.overflowMode = TextOverflowModes.Overflow;
                        var le = t.GetComponent<LayoutElement>();
                        if (le != null)
                        {
                            le.preferredHeight = -1;
                            le.minHeight = -1;
                        }
                    }
                }
            }
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            EnsureUIHierarchy();
            RefreshUI();

            // Force layout recalculation and reset scroll position to the very top
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
            string emeraldHex = ColorUtility.ToHtmlStringRGB(p.accentEmerald);
            string redHex = ColorUtility.ToHtmlStringRGB(p.accentCrimson);
            string blueHex = ColorUtility.ToHtmlStringRGB(p.accentCobalt);
            string coyoteHex = ColorUtility.ToHtmlStringRGB(p.accentCoyote);

            // Title
            if (titleText != null)
            {
                titleText.text = $"<b>{s.characterName.ToUpper()}</b> - SERVICE RECORD & PROFILE";
            }

            // SECTION 1: Personal Bio Summary
            if (characterBioText != null)
            {
                string statusTag = s.isDead ? $"<color=#{redHex}>[K.I.A. / DECEASED]</color>" : 
                                   s.isRetired ? $"<color=#{goldHex}>[VETERAN PENSION]</color>" :
                                   s.isYouthPhase ? $"<color=#{blueHex}>[YOUTH PHASE]</color>" : 
                                   s.careerTrack == CareerTrack.CivilianCollege ? $"<color=#{coyoteHex}>[COLLEGE STUDENT]</color>" :
                                   s.isCivilian ? $"<color=#{blueHex}>[CIVILIAN CITIZEN]</color>" : $"<color=#{emeraldHex}>[ACTIVE MILITARY]</color>";

                characterBioText.text = 
                    $"<color=#{goldHex}><b>[SECTION 1: PERSONAL IDENTITY & STATUS]</b></color>\n" +
                    $"<b>Service Status:</b> {statusTag}   |   <b>Age:</b> {s.age:F1} (Born {birthYear})\n" +
                    $"<b>Homeland:</b> {s.birthProvince ?? "Valeria Magna"}, {s.birthCountry ?? "The Valerian Empire"}\n" +
                    $"<b>Appearance:</b> <i>{s.hairStyle ?? "Short"} {s.hairColor ?? "Black"} hair, {s.eyeColor ?? "Brown"} eyes, {s.gender}</i>";
            }

            // SECTION 2: Physical Status & Readiness
            if (physicalStatusText != null)
            {
                physicalStatusText.text = 
                    $"<color=#{goldHex}><b>[SECTION 2: PHYSICAL FITNESS & COMBAT READINESS]</b></color>\n" +
                    $"<b>Combat Readiness:</b> <color=#{emeraldHex}>{s.combatReadiness:F0}%</color>   |   <b>Discipline:</b> {s.discipline:F0}%\n" +
                    $"<b>Tactical Intellect:</b> {s.tacticalIntellect:F0}/100   |   <b>Morale & Spirits:</b> {s.happiness:F0}%";
            }

            if (healthSlider != null) healthSlider.value = s.health / 100f;
            if (healthText != null) healthText.text = $"Health / Vital Signs: {s.health:F0}%";

            if (fitnessSlider != null) fitnessSlider.value = s.fitness / 100f;
            if (fitnessText != null) fitnessText.text = $"Physical Fitness: {s.fitness:F0}%";

            // SECTION 3: Military Standing & Career Classification
            if (powerRealmText != null)
            {
                string rankStr = s.currentRank != null ? $"{s.currentRank.rankName} ({s.currentRank.paygrade})" : "Civilian / Candidate";
                powerRealmText.text = 
                    $"<color=#{goldHex}><b>[SECTION 3: MILITARY CLASSIFICATION & STANDING]</b></color>\n" +
                    $"<b>Military Rank:</b> <color=#{goldHex}><b>{rankStr}</b></color>\n" +
                    $"<b>Branch & Regimental Assignment:</b> {s.militaryBranch} • {s.assignedDepartment ?? "Infantry"}";
            }

            if (qiProgressSlider != null)
            {
                qiProgressSlider.gameObject.SetActive(true);
                qiProgressSlider.value = s.combatReadiness / 100f;
            }

            if (qiProgressText != null)
            {
                qiProgressText.gameObject.SetActive(true);
                qiProgressText.text = $"Combat Readiness Index: {s.combatReadiness:F0}%";
            }

            if (talentScoreText != null)
            {
                talentScoreText.text = $"<b>MEPS / ASVAB AFQT Rating:</b> <color=#{goldHex}><b>{s.talentScore} / 99</b></color> (Qualified Active Duty)";
            }

            // SECTION 4: Career / Military Record & Compensation
            if (militaryRecordText != null)
            {
                if (s.isYouthPhase)
                {
                    militaryRecordText.text = 
                        $"<color=#{goldHex}><b>[SECTION 4: ACADEMIC & FOUNDATION TRACK]</b></color>\n" +
                        $"<b>Academy:</b> {SchoolHelper.GetSchoolName(s.currentSchoolLevel, s.currentSchoolTier, s.birthCity, s.birthProvince)}\n" +
                        $"<b>Mock Exam Score:</b> {(s.tookMockExam ? $"{s.mockExamScore}/750 ({s.mockExamProjectedAcademy})" : "Not Attempted")}\n" +
                        $"<b>Academic Standing:</b> {s.schoolPerformance:F1}%";
                }
                else if (s.isCivilian)
                {
                    string debtInfo = s.collegeTuitionDebt > 0 ? $"   |   Debt: <color=#{redHex}>${s.collegeTuitionDebt:N0}</color>" : "";
                    string eduInfo = s.graduatedCollege 
                        ? $"Imperial College Graduate ({s.collegeDegreeName})" 
                        : (s.careerTrack == CareerTrack.CivilianCollege ? $"Attending Imperial College ({s.collegeSemestersCompleted}/8 Semesters)" : "Secondary School Graduate");

                    militaryRecordText.text = 
                        $"<color=#{goldHex}><b>[SECTION 4: CIVILIAN OCCUPATION & WAGES]</b></color>\n" +
                        $"<b>Career Track:</b> Civilian Life   |   <b>Occupation:</b> {s.civilianJobTitle}\n" +
                        $"<b>Education:</b> {eduInfo}\n" +
                        $"<b>Monthly Wage:</b> ${s.civilianSalaryPerMonth:N0}/mo (Gross) | ${SalaryCalculator.CalculateNetMonthlyPay(s):N0}/mo (Net){debtInfo}";
                }
                else if (s.careerTrack == CareerTrack.MilitaryRecruitBasic || s.careerTrack == CareerTrack.MilitaryTechSchool || s.careerTrack == CareerTrack.MilitaryMedicalSchool || s.careerTrack == CareerTrack.MilitaryWarAcademy)
                {
                    string schoolName = s.assignedDepartment ?? "Imperial Training Depot";
                    int sem = s.militarySchoolSemestersCompleted;
                    militaryRecordText.text = 
                        $"<color=#{goldHex}><b>[SECTION 4: CADET TRAINING & SERVICE JACKET]</b></color>\n" +
                        $"<b>Training Track:</b> {schoolName}\n" +
                        $"<b>Terms Completed:</b> {sem} semesters   |   <b>Cadet Standing:</b> {s.meritRating:F0}%\n" +
                        $"<b>Monthly Compensation:</b> ${SalaryCalculator.CalculateGrossMonthlyPay(s):N0}/mo";
                }
                else
                {
                    string rankStr = s.currentRank != null ? $"{s.currentRank.rankName} ({s.currentRank.paygrade})" : "Private (E-1)";
                    string tourStr = s.isInCombatDeployment ? $"<color=#{redHex}>Frontline Combat Tour (Active)</color>" : "Garrison Refit";

                    militaryRecordText.text = 
                        $"<color=#{goldHex}><b>[SECTION 4: SERVICE JACKET & ACTIVE DEPLOYMENT]</b></color>\n" +
                        $"<b>Current Rank:</b> {rankStr}   |   <b>Deployment:</b> {tourStr}\n" +
                        $"<b>Combat Merit Points:</b> {s.combatMeritPoints:N0} CMP   |   <b>Promotion Points:</b> {s.promotionPoints:F0} pts\n" +
                        $"<b>Time in Grade:</b> {s.timeInCurrentRankSemesters * 0.5f:F1} years ({s.timeInCurrentRankSemesters} semesters)";
                }
            }

            if (combatSkillSlider != null) combatSkillSlider.value = s.combatSkill / 100f;
            if (combatSkillText != null) combatSkillText.text = $"Combat Skill: {s.combatSkill:F0}%";

            if (intellectSlider != null) intellectSlider.value = s.intellect / 100f;
            if (intellectText != null) intellectText.text = $"Tactical Intellect: {s.intellect:F0}%";

            // SECTION 5: Dynastic Status, Medals & Story Traits
            if (dynasticStatusText != null)
            {
                string clanStatus = s.isClanHead ? $"<color=#{goldHex}>Clan Patriarch (Duke / Head)</color>" : "Cadet Branch Member";

                string traitsStr = (s.characterTraits != null && s.characterTraits.Count > 0)
                    ? string.Join("  ", s.characterTraits.ConvertAll(t => $"<color=#{blueHex}>[{t}]</color>"))
                    : "<i>None acquired yet</i>";

                string medalsStr = (s.earnedMedals != null && s.earnedMedals.Count > 0)
                    ? string.Join("  ", s.earnedMedals.ConvertAll(m => $"<color=#{goldHex}>🏅 {m}</color>"))
                    : "<i>No military decorations recorded yet</i>";

                dynasticStatusText.text = 
                    $"<color=#{goldHex}><b>[SECTION 5: DYNASTIC HOUSE & HONORS RACK]</b></color>\n" +
                    $"<b>Noble House:</b> House {s.lastName} ({s.nobleTitle})   |   <b>Succession:</b> {clanStatus} ({s.clanSuccessionScore} pts)\n" +
                    $"<b>Service Medals Rack:</b>\n{medalsStr}\n" +
                    $"<b>Story Traits:</b> {traitsStr}";
            }

            // Apply Theme Palette
            ApplyThemePalette(p);
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (modalBackdropImage != null)
            {
                modalBackdropImage.color = p.mode == ThemeMode.Light 
                    ? new Color(0.12f, 0.12f, 0.10f, 0.75f) 
                    : new Color(0f, 0f, 0f, 0.90f);
            }
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (titleText != null) titleText.color = p.accentGold;
            if (characterBioText != null) characterBioText.color = p.textPrimary;
            if (physicalStatusText != null) physicalStatusText.color = p.textSecondary;
            if (powerRealmText != null) powerRealmText.color = p.textPrimary;
            if (talentScoreText != null) talentScoreText.color = p.textSecondary;
            if (militaryRecordText != null) militaryRecordText.color = p.textSecondary;
            if (dynasticStatusText != null) dynasticStatusText.color = p.textSecondary;

            if (closeButton != null)
            {
                var img = closeButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }

            if (healthSlider != null && healthSlider.fillRect != null)
            {
                var img = healthSlider.fillRect.GetComponent<Image>();
                if (img != null) img.color = p.healthBarColor;
            }
            if (fitnessSlider != null && fitnessSlider.fillRect != null)
            {
                var img = fitnessSlider.fillRect.GetComponent<Image>();
                if (img != null) img.color = p.fitnessBarColor;
            }
            if (combatSkillSlider != null && combatSkillSlider.fillRect != null)
            {
                var img = combatSkillSlider.fillRect.GetComponent<Image>();
                if (img != null) img.color = p.combatBarColor;
            }
            if (intellectSlider != null && intellectSlider.fillRect != null)
            {
                var img = intellectSlider.fillRect.GetComponent<Image>();
                if (img != null) img.color = p.intellectBarColor;
            }
            if (qiProgressSlider != null && qiProgressSlider.fillRect != null)
            {
                var img = qiProgressSlider.fillRect.GetComponent<Image>();
                if (img != null) img.color = p.accentEmerald;
            }
        }
    }
}
