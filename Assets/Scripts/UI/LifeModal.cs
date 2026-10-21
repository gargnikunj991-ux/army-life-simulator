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

        [Header("Section 1: Body & Vitality")]
        public TextMeshProUGUI physicalStatusText;
        public Slider healthSlider;
        public TextMeshProUGUI healthText;
        public Slider fitnessSlider;
        public TextMeshProUGUI fitnessText;

        [Header("Section 2: Power Cultivation & Meridians")]
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

            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            RefreshUI();
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        public void RefreshUI()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            int curYear = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2008;
            int birthYear = curYear - Mathf.FloorToInt(s.age);

            // Title
            if (titleText != null)
            {
                titleText.text = $"<b>{s.characterName.ToUpper()}</b> - SERVICE RECORD & PROFILE";
            }

            // Bio Summary
            if (characterBioText != null)
            {
                string statusTag = s.isDead ? "<color=#FF4444>[DECEASED]</color>" : 
                                   s.isRetired ? "<color=#FFFF44>[VETERAN PENSION]</color>" :
                                   s.isYouthPhase ? "<color=#00D4FF>[YOUTH PHASE]</color>" : 
                                   s.careerTrack == CareerTrack.CivilianCollege ? "<color=#FFAA00>[COLLEGE STUDENT]</color>" :
                                   s.isCivilian ? "<color=#44DDFF>[CIVILIAN LIFE]</color>" : "<color=#44FF44>[ACTIVE MILITARY]</color>";

                characterBioText.text = 
                    $"<b>Status:</b> {statusTag}   |   <b>Age:</b> {s.age:F1} (Born {birthYear})\n" +
                    $"<b>Homeland:</b> {s.birthProvince ?? "Valeria Magna"}, {s.birthCountry ?? "The Valerian Empire"}\n" +
                    $"<b>Appearance:</b> <i>{s.hairStyle ?? "Short"} {s.hairColor ?? "Black"} hair, {s.eyeColor ?? "Brown"} eyes, {s.gender}</i>";
            }

            // Physical Status & Cyber-Bio Strain
            if (physicalStatusText != null)
            {
                string strainColor = s.meridianDamage > 50f ? "#FF4444" : (s.meridianDamage > 20f ? "#FFAA00" : "#44FF44");
                string prepPhase = s.isYouthPhase ? $"Conditioning Phase {s.preTierStage}/4 ({PowerTierKnowledgeSystem.GetPreTierStageName(s.preTierStage)})" : "Framework Integrated";

                physicalStatusText.text = 
                    $"<b>Neural Strain:</b> <color={strainColor}>{s.meridianDamage:F0}% Toxicity</color>   |   <b>Framework:</b> {prepPhase}\n" +
                    $"<b>Bio-Aptitude Calibration:</b> {s.foundationalTempering:F0}/100   |   <b>Morale:</b> {s.happiness:F0}%";
            }

            if (healthSlider != null) healthSlider.value = s.health / 100f;
            if (healthText != null) healthText.text = $"Health: {s.health:F0}%";

            if (fitnessSlider != null) fitnessSlider.value = s.fitness / 100f;
            if (fitnessText != null) fitnessText.text = $"Fitness: {s.fitness:F0}%";

            // Super-Soldier Augmentation Program
            if (powerRealmText != null)
            {
                bool showAugmentation = s.knowsAboutCultivation || s.nobleTitle > NobleRank.Commoner;
                if (showAugmentation)
                {
                    string augmentStr = PowerTierKnowledgeSystem.FormatSoldierPowerTier(s);
                    powerRealmText.text = $"<b>Augmentation Class:</b> {augmentStr}\n" +
                                          $"<b>Implant Calibration Faults:</b> {s.failedBreakthroughCount}/3 (3rd fault triggers fatal neural overload!)";
                }
                else
                {
                    powerRealmText.text = "<b>Augmentation Program:</b> <i>[Classified by High Command] (Requires Rank E-4 Clearance)</i>";
                }
            }

            if (qiProgressSlider != null)
            {
                bool showAugmentation = s.knowsAboutCultivation || s.nobleTitle > NobleRank.Commoner;
                qiProgressSlider.gameObject.SetActive(showAugmentation);
                qiProgressSlider.value = s.tierProgressPercent / 100f;
            }

            if (qiProgressText != null)
            {
                bool showAugmentation = s.knowsAboutCultivation || s.nobleTitle > NobleRank.Commoner;
                qiProgressText.gameObject.SetActive(showAugmentation);
                qiProgressText.text = $"Serum Infusion Saturation: {s.tierProgressPercent:F1}%";
            }

            if (talentScoreText != null)
            {
                talentScoreText.text = s.isPersonalTalentRevealed 
                    ? $"<b>Genetic Aptitude Rating:</b> <color=#FFD700>{s.talentScore} / 200</color>"
                    : "<b>Genetic Aptitude Rating:</b> <i>[Unscreened] (Requires Rank E-4 Medical Clearance)</i>";
            }

            // Career / Military Record
            if (militaryRecordText != null)
            {
                if (s.isYouthPhase)
                {
                    militaryRecordText.text = 
                        $"<b>Academic Track:</b> {SchoolHelper.GetSchoolName(s.currentSchoolLevel, s.currentSchoolTier, s.birthCity, s.birthProvince)}\n" +
                        $"<b>Mock Exam Score:</b> {(s.tookMockExam ? $"{s.mockExamScore}/750 ({s.mockExamProjectedAcademy})" : "Not Attempted")}\n" +
                        $"<b>Academic Standing:</b> {s.schoolPerformance:F1}%";
                }
                else if (s.isCivilian)
                {
                    string debtInfo = s.collegeTuitionDebt > 0 ? $"   |   Debt: <color=#FF6666>${s.collegeTuitionDebt:N0}</color>" : "";
                    string eduInfo = s.graduatedCollege 
                        ? $"Imperial College Graduate ({s.collegeDegreeName})" 
                        : (s.careerTrack == CareerTrack.CivilianCollege ? $"Attending Imperial College ({s.collegeSemestersCompleted}/8 Semesters)" : "Secondary School Graduate");

                    militaryRecordText.text = 
                        $"<b>Career Track:</b> Civilian Life   |   <b>Occupation:</b> {s.civilianJobTitle}\n" +
                        $"<b>Education:</b> {eduInfo}\n" +
                        $"<b>Monthly Wage:</b> ${s.civilianSalaryPerMonth:N0}/mo (Gross) | ${SalaryCalculator.CalculateNetMonthlyPay(s):N0}/mo (Net){debtInfo}";
                }
                else if (s.careerTrack == CareerTrack.MilitaryRecruitBasic || s.careerTrack == CareerTrack.MilitaryTechSchool || s.careerTrack == CareerTrack.MilitaryMedicalSchool || s.careerTrack == CareerTrack.MilitaryWarAcademy)
                {
                    string schoolName = s.assignedDepartment ?? "Imperial Training Depot";
                    int sem = s.militarySchoolSemestersCompleted;
                    militaryRecordText.text = 
                        $"<b>Training Track:</b> {schoolName}\n" +
                        $"<b>Terms Completed:</b> {sem} semesters   |   <b>Cadet Standing:</b> {s.meritRating:F0}%\n" +
                        $"<b>Monthly Compensation:</b> ${SalaryCalculator.CalculateGrossMonthlyPay(s):N0}/mo";
                }
                else
                {
                    string rankStr = s.currentRank != null ? $"{s.currentRank.rankName} ({s.currentRank.paygrade})" : "Private (E-1)";
                    string tourStr = s.isInCombatDeployment ? "<color=#FF4444>Frontline Combat Tour (Active)</color>" : "Garrison Refit";

                    militaryRecordText.text = 
                        $"<b>Current Rank:</b> {rankStr}   |   <b>Deployment:</b> {tourStr}\n" +
                        $"<b>Combat Merit Points:</b> {s.combatMeritPoints:N0} CMP   |   <b>Promotion Points:</b> {s.promotionPoints:F0} pts\n" +
                        $"<b>Time in Grade:</b> {s.timeInCurrentRankSemesters * 0.5f:F1} years ({s.timeInCurrentRankSemesters} semesters)";
                }
            }

            if (combatSkillSlider != null) combatSkillSlider.value = s.combatSkill / 100f;
            if (combatSkillText != null) combatSkillText.text = $"Combat Skill: {s.combatSkill:F0}%";

            if (intellectSlider != null) intellectSlider.value = s.intellect / 100f;
            if (intellectText != null) intellectText.text = $"Intellect: {s.intellect:F0}%";

            // Dynastic Status, Medals & Story Traits
            if (dynasticStatusText != null)
            {
                string clanStatus = s.isClanHead ? "<color=#FFD700>Clan Patriarch (Duke / Head)</color>" : "Cadet Branch Member";

                string traitsStr = (s.characterTraits != null && s.characterTraits.Count > 0)
                    ? string.Join("  ", s.characterTraits.ConvertAll(t => $"<color=#00D4FF>[{t}]</color>"))
                    : "<i>None acquired yet</i>";

                string medalsStr = (s.earnedMedals != null && s.earnedMedals.Count > 0)
                    ? string.Join("  ", s.earnedMedals.ConvertAll(m => $"<color=#FFD700>🏅 {m}</color>"))
                    : "<i>No military decorations yet</i>";

                dynasticStatusText.text = 
                    $"<b>Noble House:</b> House {s.lastName} ({s.nobleTitle})   |   <b>Succession:</b> {clanStatus} ({s.clanSuccessionScore} pts)\n" +
                    $"<b>Story Traits:</b> {traitsStr}\n" +
                    $"<b>Medals Rack:</b> {medalsStr}";
            }

            // Apply Theme Palette
            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (titleText != null) titleText.color = p.textPrimary;
            if (characterBioText != null) characterBioText.color = p.textSecondary;
            if (physicalStatusText != null) physicalStatusText.color = p.textSecondary;
            if (powerRealmText != null) powerRealmText.color = p.textPrimary;
            if (talentScoreText != null) talentScoreText.color = p.textSecondary;
            if (militaryRecordText != null) militaryRecordText.color = p.textSecondary;
            if (dynasticStatusText != null) dynasticStatusText.color = p.textSecondary;
        }
    }
}
