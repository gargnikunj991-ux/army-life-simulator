using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public enum ActivityCategory
    {
        Directory,

        // Military & Veteran Categories
        CareerAndDuty,
        TrainingAndDrills,
        QuartermasterPX,
        BarracksAndSquad,
        MedicalBay,
        RealEstateAndCommerce,
        DynastyAndLegacy,
        UnderworldAndCrimes,

        // Civilian Categories
        CivilianWork,
        CivilianEducation,
        CivilianLifestyle,
        CivilianEnlistment,

        // Youth Categories
        YouthAcademics,
        YouthDrills,
        YouthCadetLife
    }

    public class ActivitiesModal : MonoBehaviour
    {
        public static ActivitiesModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;
        public Button closeButton;

        [Header("Scroll Hierarchy")]
        public ScrollRect activitiesScrollRect;
        public Transform activitiesContainer;

        private ActivityCategory currentCategory = ActivityCategory.Directory;

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
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void Update()
        {
            // Direct mouse wheel scroll support across PC platforms
            if (activitiesScrollRect != null && modalRoot != null && modalRoot.activeInHierarchy)
            {
                float wheel = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(wheel) > 0.001f)
                {
                    activitiesScrollRect.verticalNormalizedPosition = Mathf.Clamp01(activitiesScrollRect.verticalNormalizedPosition + wheel * 0.25f);
                }
            }
        }

        public void EnsureUIHierarchy()
        {
            if (activitiesScrollRect == null)
            {
                activitiesScrollRect = GetComponentInChildren<ScrollRect>(true);
            }

            if (activitiesScrollRect != null)
            {
                activitiesScrollRect.horizontal = false;
                activitiesScrollRect.vertical = true;
                activitiesScrollRect.scrollSensitivity = 45f;
                activitiesScrollRect.movementType = ScrollRect.MovementType.Clamped;

                // Ensure viewport has an Image with raycastTarget enabled to capture mouse drag & wheel
                if (activitiesScrollRect.viewport != null)
                {
                    var vpImg = activitiesScrollRect.viewport.GetComponent<Image>();
                    if (vpImg == null)
                    {
                        vpImg = activitiesScrollRect.viewport.gameObject.AddComponent<Image>();
                        vpImg.color = Color.clear;
                    }
                    vpImg.raycastTarget = true;
                }

                if (activitiesContainer == null && activitiesScrollRect.content != null)
                {
                    activitiesContainer = activitiesScrollRect.content;
                }
            }

            if (activitiesContainer != null)
            {
                var vLayout = activitiesContainer.GetComponent<VerticalLayoutGroup>();
                if (vLayout == null)
                {
                    vLayout = activitiesContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                }
                vLayout.padding = new RectOffset(10, 10, 10, 10);
                vLayout.spacing = 10f;
                vLayout.childControlHeight = true;
                vLayout.childControlWidth = true;
                vLayout.childForceExpandHeight = false;
                vLayout.childForceExpandWidth = true;

                var fitter = activitiesContainer.GetComponent<ContentSizeFitter>();
                if (fitter == null)
                {
                    fitter = activitiesContainer.gameObject.AddComponent<ContentSizeFitter>();
                }
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            EnsureUIHierarchy();
            currentCategory = ActivityCategory.Directory;
            RefreshUI();

            if (activitiesScrollRect != null)
            {
                activitiesScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void CloseModal()
        {
            currentCategory = ActivityCategory.Directory;
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void SetCategory(ActivityCategory category)
        {
            currentCategory = category;
            RefreshUI();

            if (activitiesScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                if (activitiesContainer is RectTransform rt)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                }
                activitiesScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void RefreshUI()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            UpdateHeader(s);

            if (activitiesContainer == null) return;

            // Clear old items
            for (int i = activitiesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(activitiesContainer.GetChild(i).gameObject);
            }

            if (currentCategory == ActivityCategory.Directory)
            {
                if (s.isYouthPhase) BuildYouthDirectory(s);
                else if (s.isCivilian) BuildCivilianDirectory(s);
                else BuildAdultMilitaryDirectory(s);
            }
            else
            {
                // Sub-category drilldown view: Back button + specific category items
                BuildCategoryView(s, currentCategory);
            }

            Canvas.ForceUpdateCanvases();
            if (activitiesContainer is RectTransform containerRt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRt);
            }

            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private void UpdateHeader(SoldierProfile s)
        {
            if (titleText != null)
            {
                if (currentCategory == ActivityCategory.Directory)
                {
                    if (s.isYouthPhase) titleText.text = "ACADEMY & CADET ACTIVITIES";
                    else if (s.isCivilian) titleText.text = "CIVILIAN OPERATIONS & LIFE";
                    else titleText.text = "MILITARY OPERATIONS & BASE ACTIVITIES";
                }
                else
                {
                    titleText.text = GetCategoryTitle(currentCategory);
                }
            }

            if (subtitleText != null)
            {
                if (currentCategory == ActivityCategory.Directory)
                {
                    if (s.isYouthPhase)
                    {
                        subtitleText.text = $"Foundational Tempering: <b>{s.foundationalTempering:F0}/100</b>   |   Intellect: <b>{s.intellect:F0}%</b>   |   Combat: <b>{s.combatSkill:F0}%</b>";
                    }
                    else if (s.isCivilian)
                    {
                        string debtStr = s.collegeTuitionDebt > 0 ? $"   |   Debt: <color=#FF6666>${s.collegeTuitionDebt:N0}</color>" : "";
                        subtitleText.text = $"Civilian Wage: <b>${s.civilianSalaryPerMonth:N0}/mo</b>   |   Cash: <b>${s.cashBalance:N0}</b>{debtStr}";
                    }
                    else
                    {
                        string debtStr = s.academyTuitionDebt > 0 ? $"   |   Debt: <color=#FF6666>${s.academyTuitionDebt:N0}</color>" : "";
                        subtitleText.text = $"Fitness: <b>{s.fitness:F0}%</b>   |   Combat: <b>{s.combatSkill:F0}%</b>   |   Readiness: <b>{s.combatReadiness:F0}%</b>   |   Cash: <b>${s.cashBalance:N0}</b>{debtStr}";
                    }
                }
                else
                {
                    subtitleText.text = GetCategorySubtitle(currentCategory, s);
                }
            }
        }

        private string GetCategoryTitle(ActivityCategory cat)
        {
            switch (cat)
            {
                case ActivityCategory.CareerAndDuty: return "CAREER & DUTY ASSIGNMENTS";
                case ActivityCategory.TrainingAndDrills: return "TRAINING & COMBAT DRILLS";
                case ActivityCategory.QuartermasterPX: return "PX QUARTERMASTER DEPOT";
                case ActivityCategory.BarracksAndSquad: return "BARRACKS & FIRETEAM COMRADES";
                case ActivityCategory.MedicalBay: return "MEDICAL BAY & CLINIC";
                case ActivityCategory.RealEstateAndCommerce: return "REAL ESTATE & COMMERCIAL ASSETS";
                case ActivityCategory.DynastyAndLegacy: return "FAMILY & DYNASTIC SUCCESSION";
                case ActivityCategory.UnderworldAndCrimes: return "BLACK MARKET & UNDERWORLD";

                case ActivityCategory.CivilianWork: return "CIVILIAN EMPLOYMENT & CAREER";
                case ActivityCategory.CivilianEducation: return "IMPERIAL COLLEGE & HIGHER STUDIES";
                case ActivityCategory.CivilianLifestyle: return "CIVILIAN RECREATION & WELLNESS";
                case ActivityCategory.CivilianEnlistment: return "MILITARY ENLISTMENT DEPOT";

                case ActivityCategory.YouthAcademics: return "ACADEMIC LECTURES & ARCHIVES";
                case ActivityCategory.YouthDrills: return "MARTIAL DRILLS & FIRING RANGE";
                case ActivityCategory.YouthCadetLife: return "CADET CLINIC & APPRENTICE LABOR";
                default: return "ACTIVITIES";
            }
        }

        private string GetCategorySubtitle(ActivityCategory cat, SoldierProfile s)
        {
            switch (cat)
            {
                case ActivityCategory.CareerAndDuty:
                    return $"Rank: <b>{(s.currentRank != null ? s.currentRank.rankName : "Recruit")}</b>   |   Merit: <b>{s.meritRating:F0}%</b>   |   Board Points: <b>{s.promotionPoints}</b>";
                case ActivityCategory.TrainingAndDrills:
                    return $"Action Points: <b>{s.actionPointsRemaining} / {(s.isInCombatDeployment ? 1 : 3)}</b>   |   Fitness: <b>{s.fitness:F0}%</b>   |   Combat: <b>{s.combatSkill:F0}%</b>";
                case ActivityCategory.QuartermasterPX:
                    return $"Liquid Cash: <b>${s.cashBalance:N0}</b>   |   Medkits: <b>{s.fieldMedkitsCount}</b>   |   Cigarettes: <b>{s.cigarettesCount}</b>";
                case ActivityCategory.BarracksAndSquad:
                    return $"Morale: <b>{s.happiness:F0}%</b>   |   Squad Size: <b>{s.squadMates.Count} Brothers</b>";
                case ActivityCategory.MedicalBay:
                    return $"Health: <b>{s.health:F0}%</b>   |   Readiness: <b>{s.combatReadiness:F0}%</b>   |   Prosthetics: <b>{(s.hasBionicProsthetics ? "Fitted" : "None")}</b>";
                case ActivityCategory.RealEstateAndCommerce:
                    return $"Personal Quarters: <b>{s.housingTier}</b>   |   Family Estate: <b>{s.familyHouseTier}</b>   |   Business: <b>{s.ownedBusiness}</b>";
                case ActivityCategory.DynastyAndLegacy:
                    return $"Generation: <b>Gen {s.generationNumber}</b>   |   Heir: <b>{(s.childHeir != null ? s.childHeir.childName : "None")}</b>";
                case ActivityCategory.UnderworldAndCrimes:
                    return $"Criminal Heat: <b>{s.criminalHeat:F0}%</b>   |   Crimes Committed: <b>{s.totalCrimesCommitted}</b>";
                default:
                    return $"Cash: <b>${s.cashBalance:N0}</b>   |   Health: <b>{s.health:F0}%</b>";
            }
        }

        // ==================== DIRECTORY BUILDERS (BITLIFE STYLE) ====================

        private void BuildAdultMilitaryDirectory(SoldierProfile s)
        {
            CreateCategoryTile("🎖️", "CAREER & DUTY ASSIGNMENTS",
                "Promotion boards, tour extensions, Officer Academy, war theaters, and veteran ambitions.",
                GetCareerAndDutyCount(s),
                () => SetCategory(ActivityCategory.CareerAndDuty));

            CreateCategoryTile("⚔️", "TRAINING & COMBAT DRILLS",
                "Obstacle courses, live-fire firing range, tactical qualification tests, and flight sorties.",
                GetTrainingAndDrillsCount(s),
                () => SetCategory(ActivityCategory.TrainingAndDrills));

            CreateCategoryTile("🛒", "PX QUARTERMASTER DEPOT",
                "Body armor, weapons, emergency field trauma kits, cigarettes, and lucky keepsakes.",
                GetQuartermasterCount(s),
                () => SetCategory(ActivityCategory.QuartermasterPX));

            CreateCategoryTile("👥", "BARRACKS & FIRETEAM COMRADES",
                "Mess hall, officers club, squad rounds, and fireteam bonds with your brothers-in-arms.",
                GetBarracksAndSquadCount(s),
                () => SetCategory(ActivityCategory.BarracksAndSquad));

            CreateCategoryTile("🏥", "MEDICAL BAY & CLINIC",
                "Trauma bay recovery, specialist surgeries, combat prosthetics, and family care.",
                4,
                () => SetCategory(ActivityCategory.MedicalBay));

            CreateCategoryTile("🏡", "REAL ESTATE & COMMERCIAL ASSETS",
                "Personal quarters upgrades, family ancestral estates, and commercial enterprise acquisitions.",
                GetRealEstateAndCommerceCount(s),
                () => SetCategory(ActivityCategory.RealEstateAndCommerce));

            CreateCategoryTile("👑", "FAMILY & DYNASTIC SUCCESSION",
                "Raise children, enroll them in elite military proctors, and pass the torch to continue your dynasty.",
                GetDynastyAndLegacyCount(s),
                () => SetCategory(ActivityCategory.DynastyAndLegacy));

            CreateCategoryTile("🕶️", "BLACK MARKET & UNDERWORLD",
                "Depot diesel siphoning, armory theft, dice rings, battlefield plunder, and bribery.",
                GetCrimeCount(s),
                () => SetCategory(ActivityCategory.UnderworldAndCrimes));
        }

        private void BuildCivilianDirectory(SoldierProfile s)
        {
            CreateCategoryTile("💼", "CIVILIAN EMPLOYMENT & CAREER",
                "Work overtime shifts, petition for wage increases, and executive performance reviews.",
                2,
                () => SetCategory(ActivityCategory.CivilianWork));

            CreateCategoryTile("🎓", "IMPERIAL COLLEGE & HIGHER STUDIES",
                "Apply for university degrees, research lectures, and manage student tuition debt.",
                GetCivilianEducationCount(s),
                () => SetCategory(ActivityCategory.CivilianEducation));

            CreateCategoryTile("🎖️", "MILITARY ENLISTMENT DEPOT",
                "Leave civilian life and volunteer for recruit boot camp in the Imperial Army or Air Force.",
                2,
                () => SetCategory(ActivityCategory.CivilianEnlistment));

            CreateCategoryTile("🏛️", "RECREATION, WELLNESS & LIFESTYLE",
                "Municipal wellness gym, neighborhood social taverns, and physical conditioning.",
                3,
                () => SetCategory(ActivityCategory.CivilianLifestyle));

            CreateCategoryTile("🏡", "REAL ESTATE & COMMERCIAL ASSETS",
                "Apartment leases, comfortable family villas, ancestral estates, and business empires.",
                GetRealEstateAndCommerceCount(s),
                () => SetCategory(ActivityCategory.RealEstateAndCommerce));

            CreateCategoryTile("🏥", "HOSPITAL & SPECIALIST MEDICAL",
                "Civilian clinic visits, specialist surgery, orthopedic prosthetics, and family care.",
                4,
                () => SetCategory(ActivityCategory.MedicalBay));

            CreateCategoryTile("👑", "FAMILY & DYNASTIC LINEAGE",
                "Start a family, raise children, hire martial tutors, and preserve your bloodline.",
                GetDynastyAndLegacyCount(s),
                () => SetCategory(ActivityCategory.DynastyAndLegacy));

            CreateCategoryTile("🕶️", "UNDERWORLD & SHADOW HUSTLES",
                "Underground fight clubs, contraband couriers, syndicate debt muscle, and heists.",
                GetCrimeCount(s),
                () => SetCategory(ActivityCategory.UnderworldAndCrimes));
        }

        private void BuildYouthDirectory(SoldierProfile s)
        {
            int academicCount = (s.age >= 16.0f) ? 3 : 2;
            CreateCategoryTile("📚", "ACADEMIC STUDIES & ARCHIVES",
                "Attend academy lectures, study military doctrine in the archives, and sit for mock exams.",
                academicCount,
                () => SetCategory(ActivityCategory.YouthAcademics));

            CreateCategoryTile("⚔️", "MARTIAL DRILLS & FIRING RANGE",
                "High-intensity physical conditioning, ruck marches, and target marksmanship.",
                2,
                () => SetCategory(ActivityCategory.YouthDrills));

            CreateCategoryTile("🏥", "CADET CLINIC & APPRENTICE LABOR",
                "Base clinic checkups, restorative vitamins, and part-time delivery jobs for pocket money.",
                2,
                () => SetCategory(ActivityCategory.YouthCadetLife));
        }

        // ==================== SUB-CATEGORY VIEW BUILDER ====================

        private void BuildCategoryView(SoldierProfile s, ActivityCategory cat)
        {
            CreateBackToDirectoryButton();

            switch (cat)
            {
                case ActivityCategory.CareerAndDuty:
                    BuildCareerAndDutyActivities(s);
                    break;
                case ActivityCategory.TrainingAndDrills:
                    BuildTrainingAndDrillsActivities(s);
                    break;
                case ActivityCategory.QuartermasterPX:
                    BuildQuartermasterActivities(s);
                    break;
                case ActivityCategory.BarracksAndSquad:
                    BuildBarracksAndSquadActivities(s);
                    break;
                case ActivityCategory.MedicalBay:
                    BuildMedicalActivities(s);
                    break;
                case ActivityCategory.RealEstateAndCommerce:
                    BuildRealEstateAndCommerceActivities(s);
                    break;
                case ActivityCategory.DynastyAndLegacy:
                    BuildDynastyAndLegacyActivities(s);
                    break;
                case ActivityCategory.UnderworldAndCrimes:
                    BuildCrimeAndUnderworldActivities(s);
                    break;

                case ActivityCategory.CivilianWork:
                    BuildCivilianWorkActivities(s);
                    break;
                case ActivityCategory.CivilianEducation:
                    BuildCivilianEducationActivities(s);
                    break;
                case ActivityCategory.CivilianLifestyle:
                    BuildCivilianLifestyleActivities(s);
                    break;
                case ActivityCategory.CivilianEnlistment:
                    BuildCivilianEnlistmentActivities(s);
                    break;

                case ActivityCategory.YouthAcademics:
                    BuildYouthAcademicsActivities(s);
                    break;
                case ActivityCategory.YouthDrills:
                    BuildYouthDrillsActivities(s);
                    break;
                case ActivityCategory.YouthCadetLife:
                    BuildYouthCadetLifeActivities(s);
                    break;
            }
        }

        // ==================== COUNTER HELPERS FOR DIRECTORY BADGES ====================

        private int GetCareerAndDutyCount(SoldierProfile s)
        {
            int count = 1; // Promotion Board Review
            if (!s.isRetired && s.careerTrack != CareerTrack.MilitaryWarAcademy)
            {
                count++; // Tour Extension
                count++; // Current Theater info
                count += 4; // 4 Transfer options
            }
            if (!s.isOfficer && s.careerTrack != CareerTrack.MilitaryWarAcademy)
            {
                count++; // Officer Academy upfront or loan
                if (s.cashBalance >= ((s.militaryBranch == MilitaryBranch.AirForce) ? EconomyData.AIRFORCE_ACADEMY_TUITION_COST : EconomyData.ARMY_ACADEMY_TUITION_COST)) count++;
            }
            if (s.academyTuitionDebt > 0) count++;
            if (!s.isSpecialForces && !s.isSpecialForcesCandidate) count++;
            else count++; // SF candidate/operator status

            if (s.militaryBranch != MilitaryBranch.AirForce && !s.isRetired && !s.isCivilian) count += 2; // Air force transfer options

            if (!s.isCivilian) count += 2; // Service Dress Uniform & Medal Ribbon Rack inspection + Strategic Campaign War Map

            if (s.isRetired)
            {
                count = (s.veteranCareer == VeteranCareer.None) ? 3 : 1;
                if (s.isRetired && s.veteranCareer == VeteranCareer.None && s.medalsEarnedCount >= 2) count++;
                if (s.isRetired && s.veteranCareer == VeteranCareer.None && s.isOfficer) count++;
                if (s.isRetired && s.veteranCareer == VeteranCareer.None && s.currentRank != null && (s.currentRank.paygrade == "O-10" || s.currentRank.rankName.Contains("General"))) count++;
            }
            return Mathf.Max(1, count);
        }

        private int GetTrainingAndDrillsCount(SoldierProfile s)
        {
            int count = 5; // PT, Firing Range, Combat Drills, Interactive Range Minigame, Checkpoint Minigame
            if (s.militaryBranch == MilitaryBranch.AirForce && !s.isRetired)
            {
                count = (s.isFlightDuty ? 2 : 1) + 2;
            }
            return count;
        }

        private int GetQuartermasterCount(SoldierProfile s)
        {
            int count = 2; // Medkits, Cigarettes
            if (!s.hasSteelHelmet) count++;
            if (!s.hasBallisticVest) count++;
            if (!s.hasServiceSidearm) count++;
            if (!s.hasLuckyWatch) count++;
            return count;
        }

        private int GetBarracksAndSquadCount(SoldierProfile s)
        {
            int count = 1; // Mess hall
            if (s.militaryBranch == MilitaryBranch.AirForce) count++; // Officers club
            if (!s.isCivilian && !s.isRetired) count += 3; // Squad drinks, Barracks Craps, Mail Call
            s.InitializeSquadMatesIfEmpty();
            count += s.squadMates.Count; // One card or interaction per squadmate
            return count;
        }

        private int GetRealEstateAndCommerceCount(SoldierProfile s)
        {
            int count = 1; // Personal Housing
            count++; // Family Housing
            count += (s.ownedBusiness == CommercialBusiness.None) ? 7 : 4; // Commercial enterprises
            return count;
        }

        private int GetDynastyAndLegacyCount(SoldierProfile s)
        {
            int count = 1; // Child Heir status or marriage
            if (s.childHeir != null && !s.childHeir.isEligibleForEnlistment) count++; // Tutors
            return count;
        }

        private int GetCrimeCount(SoldierProfile s)
        {
            int count = 1; // Heat status banner
            if (s.isImprisoned || s.militaryBrigSemestersRemaining > 0) return 1;
            if (!s.isCivilian && !s.isRetired)
            {
                count += 3; // Siphon, armory, craps
                if (s.isInCombatDeployment) count++;
                if (s.isOfficer) count++;
            }
            else if (s.isCivilian)
            {
                count += 4;
                if (s.careerTrack == CareerTrack.CivilianProfessional) count++;
            }
            else if (s.isRetired)
            {
                count += 2;
                if (s.veteranCareer == VeteranCareer.CityMayor || s.veteranCareer == VeteranCareer.RegionalGovernor || s.veteranCareer == VeteranCareer.MinisterOfDefense || s.politicalInfluencePoints >= 20) count++;
            }
            if (s.criminalHeat >= 15f) count++; // Bribe option
            return count;
        }

        private int GetCivilianEducationCount(SoldierProfile s)
        {
            int count = 0;
            if (!s.graduatedCollege && s.careerTrack != CareerTrack.CivilianCollege) count++;
            if (s.careerTrack == CareerTrack.CivilianCollege) count++;
            if (s.collegeTuitionDebt > 0) count++;
            return Mathf.Max(1, count);
        }

        // ==================== YOUTH SUB-CATEGORY IMPLEMENTATIONS ====================

        private void BuildYouthAcademicsActivities(SoldierProfile s)
        {
            CreateActivityCard("Academic Studies & Lectures",
                "Attend lectures at your current academy. Boost academic standing and exam preparedness.",
                new Color(0.20f, 0.45f, 0.75f), () =>
                {
                    if (s.semesterStudyPerformanceGained >= SoldierProfile.MAX_SEMESTER_STUDY_BOOST)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("ACADEMICS",
                            "<color=#FFAA00>[STUDY CAP REACHED]</color> You already completed this semester's study hours! Advance to the next semester to refresh.");
                        return;
                    }
                    float boost = Mathf.Min(4.0f, SoldierProfile.MAX_SEMESTER_STUDY_BOOST - s.semesterStudyPerformanceGained);
                    s.semesterStudyPerformanceGained += boost;
                    s.schoolPerformance = Mathf.Min(100f, s.schoolPerformance + boost);
                    s.intellect = Mathf.Min(100f, s.intellect + 2.0f);
                    GameEvents.OnFeedEventLogged?.Invoke("ACADEMICS",
                        $"<color=#00FFAA>[ACADEMIC STUDY]</color> You attended intensive lectures. Academic Standing +{boost:F1}%, Intellect +2%.");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Military Tactical Archives",
                "Study military doctrine, infantry maneuvers, artillery tables, and command strategy.",
                new Color(0.50f, 0.25f, 0.70f), () =>
                {
                    s.intellect = Mathf.Min(100f, s.intellect + 3f);
                    s.discipline = Mathf.Min(100f, s.discipline + 2f);
                    GameEvents.OnFeedEventLogged?.Invoke("STUDY",
                        "<color=#FFD700>[MILITARY ARCHIVES]</color> You studied advanced infantry manuals and battlefield command doctrines! (+3 Intellect, +2 Discipline).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            if (s.age >= 16.0f)
            {
                CreateActivityCard("Take National Mock Exam",
                    "Simulate the 750-point entrance exam to project your academic academy admission score.",
                    new Color(0.80f, 0.50f, 0.10f), () =>
                    {
                        int score = ChildhoodLifeEngine.CalculateProjectedExamScore(s, out string academy);
                        s.tookMockExam = true;
                        s.mockExamScore = score;
                        s.mockExamProjectedAcademy = academy;
                        GameEvents.OnFeedEventLogged?.Invoke("EXAM",
                            $"<color=#FFD700><b>[MOCK EXAM RESULT]</b></color> You scored <b>{score}/750</b>! Projected Academy: <b>{academy}</b>.");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
        }

        private void BuildYouthDrillsActivities(SoldierProfile s)
        {
            string stageName = PowerTierKnowledgeSystem.GetPreTierStageName(s.preTierStage);
            CreateActivityCard($"Physical Conditioning ({stageName})",
                "Endure high-intensity physical conditioning, calisthenics, and ruck marches to build strength and stamina.",
                new Color(0.70f, 0.35f, 0.15f), () =>
                {
                    s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 6f);
                    s.fitness = Mathf.Min(100f, s.fitness + 3f);
                    s.health = Mathf.Max(20f, s.health - 4f);

                    if (s.combatReadiness >= 25f && s.preTierStage < 2) s.preTierStage = 2;
                    if (s.combatReadiness >= 50f && s.preTierStage < 3) s.preTierStage = 3;
                    if (s.combatReadiness >= 80f && s.preTierStage < 4) s.preTierStage = 4;

                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                        $"<color=#00FFAA>[PHYSICAL CONDITIONING]</color> You completed grueling muscular and endurance drills! (+6 Readiness, +3 Fitness).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Cadet Firing Range",
                "Practice marksmanship with military kinetic rifles and energy carbines.",
                new Color(0.20f, 0.60f, 0.40f), () =>
                {
                    s.combatSkill = Mathf.Min(100f, s.combatSkill + 4.5f);
                    s.happiness = Mathf.Min(100f, s.happiness + 2f);
                    GameEvents.OnFeedEventLogged?.Invoke("CADET",
                        "<color=#00D4FF>[MARKSMANSHIP]</color> You spent hours at the target range scoring bulls-eyes. (+4.5 Combat Skill).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }

        private void BuildYouthCadetLifeActivities(SoldierProfile s)
        {
            CreateActivityCard("Base Medical Clinic",
                "Purchase medical treatment and restorative vitamins ($30) to recover from training fatigue.",
                new Color(0.15f, 0.55f, 0.35f), () =>
                {
                    if (s.cashBalance < 30)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "<color=#FF4444>[POVERTY]</color> You lack $30 for medical treatment.");
                        return;
                    }
                    s.cashBalance -= 30;
                    s.health = Mathf.Min(100f, s.health + 15f);
                    s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL",
                        "<color=#00FFAA>[MEDICAL CARE]</color> Clinic doctors treated your sprains and exhaustion (+5 Readiness, +15 Health).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Cadet Part-Time Labor",
                "Run supply deliveries and clean equipment to earn modest pocket money.",
                new Color(0.35f, 0.40f, 0.45f), () =>
                {
                    s.cashBalance += 40;
                    s.happiness = Mathf.Max(10f, s.happiness - 4f);
                    GameEvents.OnFeedEventLogged?.Invoke("WORK",
                        "<color=#00FFAA>[HARD WORK]</color> You worked odd jobs around the military district and earned <b>+$40</b>.");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }

        // ==================== CIVILIAN SUB-CATEGORY IMPLEMENTATIONS ====================

        private void BuildCivilianWorkActivities(SoldierProfile s)
        {
            long shiftEarnings = (s.careerTrack == CareerTrack.CivilianProfessional) ? 120L : 60L;
            CreateActivityCard("Work Overtime Shift (1 AP)",
                $"Put in additional hours at your civilian workplace to earn immediate income (+${shiftEarnings}, consumes 1 AP).",
                new Color(0.20f, 0.55f, 0.35f), () =>
                {
                    if (!s.TryConsumeActionPoint("Overtime Shift"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester. Advance the semester to recover your stamina!");
                        return;
                    }
                    s.cashBalance += shiftEarnings;
                    s.health = Mathf.Max(15f, s.health - 4f);
                    s.happiness = Mathf.Max(10f, s.happiness - 3f);
                    GameEvents.OnFeedEventLogged?.Invoke("WORK",
                        $"<color=#00FFAA>[OVERTIME WORKED]</color> You completed an extra evening shift and received <b>+${shiftEarnings}</b> ({s.actionPointsRemaining} AP remaining).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            if (s.careerTrack == CareerTrack.CivilianWork)
            {
                CreateActivityCard("Seek Wage Promotion / Better Job",
                    $"Interview for higher-tier civilian responsibilities (Up to ${CivilAndMilitaryCareerEngine.MAX_NON_COLLEGE_PAY}/mo max without degree).",
                    new Color(0.25f, 0.45f, 0.75f), () =>
                    {
                        if (s.civilianSalaryPerMonth >= CivilAndMilitaryCareerEngine.MAX_NON_COLLEGE_PAY)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                                $"<color=#FFAA00>[WAGE CEILING REACHED]</color> Without a 4-year Imperial College degree, non-degree civilian wages cap at <b>${CivilAndMilitaryCareerEngine.MAX_NON_COLLEGE_PAY}/mo</b>. Enroll in College to unlock $500+/mo professional positions!");
                            return;
                        }

                        s.civilianSalaryPerMonth = System.Math.Min(CivilAndMilitaryCareerEngine.MAX_NON_COLLEGE_PAY, s.civilianSalaryPerMonth + 35L);
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                            $"<color=#00FFAA>[WAGE INCREASED]</color> Your employer promoted you to senior clerk/artisan! Monthly Wage: <b>${s.civilianSalaryPerMonth}/mo</b>.");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
            else if (s.careerTrack == CareerTrack.CivilianProfessional)
            {
                CreateActivityCard("Executive Promotion Review",
                    "Submit professional portfolio for departmental advancement and executive compensation.",
                    new Color(0.70f, 0.50f, 0.15f), () =>
                    {
                        s.civilianSalaryPerMonth += 175L;
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER",
                            $"<color=#FFD700>[EXECUTIVE ADVANCEMENT]</color> Your professional performance was lauded by the Guild Directorate! Monthly salary increased to <b>${s.civilianSalaryPerMonth:N0}/mo</b>.");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
        }

        private void BuildCivilianEducationActivities(SoldierProfile s)
        {
            if (s.careerTrack == CareerTrack.CivilianCollege)
            {
                CreateActivityCard("Attend University Lectures & Thesis",
                    $"Study curriculum for {s.collegeDegreeName}. Term: {s.collegeSemestersCompleted}/8. Boosts Intellect.",
                    new Color(0.20f, 0.50f, 0.80f), () =>
                    {
                        s.intellect = Mathf.Min(100f, s.intellect + 3.5f);
                        s.happiness = Mathf.Min(100f, s.happiness + 2f);
                        GameEvents.OnFeedEventLogged?.Invoke("COLLEGE",
                            $"<color=#00D4FF>[ACADEMIC STUDY]</color> You researched advanced texts in {s.collegeDegreeName}. (+3.5 Intellect).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }

            if (s.collegeTuitionDebt > 0)
            {
                CreateActivityCard($"Repay Student Loan Debt ($1,000)",
                    $"Make a principal repayment on your imperial tuition debt (Remaining Debt: ${s.collegeTuitionDebt:N0}).",
                    new Color(0.60f, 0.35f, 0.20f), () =>
                    {
                        long payAmount = System.Math.Min(1000L, s.collegeTuitionDebt);
                        if (s.cashBalance < payAmount)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("DEBT",
                                $"<color=#FF4444>[INSUFFICIENT FUNDS]</color> You need ${payAmount:N0} in cash to make this debt payment.");
                            return;
                        }
                        s.cashBalance -= payAmount;
                        s.collegeTuitionDebt -= payAmount;
                        GameEvents.OnFeedEventLogged?.Invoke("DEBT",
                            $"<color=#00FFAA>[DEBT REPAID]</color> You paid <b>${payAmount:N0}</b> toward your college loans. Remaining debt: <b>${s.collegeTuitionDebt:N0}</b>.");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }

            if (!s.graduatedCollege && s.careerTrack != CareerTrack.CivilianCollege)
            {
                CreateActivityCard("Apply to Imperial College ($100,000 Degree)",
                    "Enroll in a 4-year degree (8 semesters at $12,500/sem) to unlock high-paying $500+/mo professional positions.",
                    new Color(0.20f, 0.45f, 0.75f), () =>
                    {
                        CivilAndMilitaryCareerEngine.EnrollInCollege(s, "Imperial Commerce & Trade");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
        }

        private void BuildCivilianLifestyleActivities(SoldierProfile s)
        {
            CreateActivityCard("Municipal Wellness Gym",
                "Exercise on weights and athletic circuits to maintain physical conditioning.",
                new Color(0.18f, 0.65f, 0.35f), () =>
                {
                    s.fitness = Mathf.Min(100f, s.fitness + 4f);
                    s.health = Mathf.Min(100f, s.health + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("FITNESS",
                        "<color=#00FFAA>[WORKOUT]</color> You completed an intense session at the municipal gym (+4 Fitness, +5 Health).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Physical Conditioning Drills",
                "Perform calisthenics, morning ruck marches, and endurance conditioning.",
                new Color(0.55f, 0.25f, 0.75f), () =>
                {
                    MilitaryPhysicalConditioningEngine.ProcessPhysicalConditioning(s);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("City Tavern & Social Lounge",
                "Unwind with neighborhood acquaintances, listen to bard music, and enjoy good food.",
                new Color(0.30f, 0.55f, 0.45f), () =>
                {
                    s.happiness = Mathf.Min(100f, s.happiness + 15f);
                    GameEvents.OnFeedEventLogged?.Invoke("SOCIAL",
                        "<color=#00FFAA>[RECREATION]</color> You enjoyed a lively evening with local friends in the city tavern (+15% Happiness).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }

        private void BuildCivilianEnlistmentActivities(SoldierProfile s)
        {
            CreateActivityCard("Enlist: Imperial Army (6 Mo Boot Camp)",
                "Leave civilian life to join the frontline Imperial Army. 6-month basic training depot -> E-1 Private. Eligible for frontline combat bounties.",
                new Color(0.70f, 0.35f, 0.15f), () =>
                {
                    CivilAndMilitaryCareerEngine.EnlistAsRecruit(s, GameManager.Instance, MilitaryBranch.Army);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Enlist: Imperial Air Force (1 Yr Boot Camp)",
                "Leave civilian life to join the Imperial Air Force. 1-year aviation training -> E-1 Airman. Superior base quarters (+15% Morale) and flight duty pay (+15%).",
                new Color(0.20f, 0.50f, 0.85f), () =>
                {
                    CivilAndMilitaryCareerEngine.EnlistAsRecruit(s, GameManager.Instance, MilitaryBranch.AirForce, AirForceRole.FlightDuty);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }

        // ==================== MILITARY SUB-CATEGORY IMPLEMENTATIONS ====================

        private void BuildCareerAndDutyActivities(SoldierProfile s)
        {
            // Strategic Campaign War Map Room
            if (!s.isCivilian)
            {
                var curTh = WarTheaterInfo.GetInfo(s.currentTheater);
                int prog = DynamicWarFrontEngine.GetFrontlineProgress(s.currentTheater);
                string sitrep = DynamicWarFrontEngine.GetTheaterStatusText(s.currentTheater);
                CreateActivityCard("Strategic Campaign War Map Room",
                    $"Inspect frontline territory control across all 5 combat theaters. Current: <b>{curTh.theaterName}</b> ({prog:+0;-0;0}%). {sitrep}",
                    new Color(0.18f, 0.45f, 0.65f), () =>
                    {
                        CloseModal();
                        if (StrategicWarMapModal.Instance != null)
                        {
                            StrategicWarMapModal.Instance.OpenModal();
                        }
                        else
                        {
                            var found = FindAnyObjectByType<StrategicWarMapModal>(FindObjectsInactive.Include);
                            if (found != null) found.OpenModal();
                        }
                    });

                // Tactical Firefight Engagement (Interactive Combat Decision Cards)
                if (!s.isRetired && s.health >= 35f)
                {
                    CreateActivityCard("Deploy: Tactical Firefight Engagement",
                        $"Lead your 4-man fireteam into an immediate tactical contact mission in <b>{curTh.theaterName}</b>. Command suppression, airstrikes, or CASEVAC under fire!",
                        new Color(0.75f, 0.20f, 0.20f), () =>
                        {
                            CloseModal();
                            if (TacticalFirefightModal.Instance != null)
                            {
                                TacticalFirefightModal.Instance.LaunchFirefight(s, curTh, RefreshUI);
                            }
                            else
                            {
                                var found = FindAnyObjectByType<TacticalFirefightModal>(FindObjectsInactive.Include);
                                if (found != null) found.LaunchFirefight(s, curTh, RefreshUI);
                            }
                        });
                }

                // MOS Specialization Qualification
                if (!s.isRetired)
                {
                    string mosDesc = s.militaryBranch == MilitaryBranch.AirForce
                        ? $"Current Air Force Role: <b>{s.militaryMOS}</b>. Specialize as Fighter Pilot Ace or Pararescue PJ."
                        : $"Current Army MOS: <b>{s.militaryMOS}</b>. Specialize as 11B Infantry, 68W Combat Medic, 19D Sniper, or 12B Sapper.";

                    CreateActivityCard($"Military Specialty (MOS): {s.militaryMOS}",
                        $"{mosDesc}\nClick to cross-train or certify into a specialized battlefield qualification.",
                        new Color(0.25f, 0.55f, 0.40f), () =>
                        {
                            CycleMOSQualification(s);
                            RefreshUI();
                        });
                }
            }

            // 1. Promotion Board Review
            CreateActivityCard("Promotion Board Submission",
                "Review service jacket and submit official petition for next military paygrade promotion.",
                new Color(0.85f, 0.65f, 0.15f), () =>
                {
                    PromotionBoardEngine.EvaluateBoardPromotion(s, null);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // Military Justice & UCMJ Status
            if (s.isFortLeavenworthInmate)
            {
                CreateActivityCard($"Fort Leavenworth Penal Detail ({s.leavenworthSemestersRemaining} Semesters Remaining)",
                    "Serving hard labor sentence at Fort Leavenworth Military Disciplinary Barracks. Complete your sentence to return to civilian society.",
                    new Color(0.6f, 0.15f, 0.15f), () =>
                    {
                        MilitaryJusticeEngine.ProcessLeavenworthPenalSemester(s, GameManager.Instance);
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
            else if (s.isBarracksRestricted)
            {
                CreateActivityCard($"Article 15 Barracks Restriction ({s.barracksRestrictionDaysRemaining} Days Remaining)",
                    "Restricted to company quarters under commanding officer's Article 15 order. Forfeiture of 1/2 pay in effect.",
                    new Color(0.7f, 0.35f, 0.15f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("RESTRICTION",
                            $"<color=#FF8866>[BARRACKS RESTRICTION]</color> You remain confined to quarters. {s.barracksRestrictionDaysRemaining} days remaining.");
                        RefreshUI();
                    });
            }
            else if (!s.isRetired && !s.isCivilian)
            {
                int offenses = s.disciplinaryRecords != null ? s.disciplinaryRecords.Count : 0;
                CreateActivityCard("JAG Legal Office: UCMJ Disciplinary Review",
                    $"Review military justice jacket. Infractions on record: <b>{offenses}</b> | Article 15 Penalties: <b>{s.article15PunishmentsCount}</b> | Court-Martial Convictions: <b>{s.courtMartialConvictionsCount}</b>.",
                    new Color(0.35f, 0.40f, 0.55f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("JAG REVIEW",
                            $"<color=#00D4FF>[MILITARY LEGAL JACKET]</color> Record: {offenses} infractions, {s.article15PunishmentsCount} Article 15s. Discharge Standing: {s.dischargeCharacterization}.");
                        RefreshUI();
                    });
            }

            // 2. Extended Frontline Deployment
            if (!s.isRetired && s.careerTrack != CareerTrack.MilitaryWarAcademy)
            {
                string extStatus = s.isVoluntaryExtendedTour 
                    ? "<color=#FFD700>[EXTENDED COMBAT ACTIVE]</color>" 
                    : "<color=#94A3B8>[STANDARD 6-MONTH ROTATION]</color>";
                string extBonusStatus = s.hasExtendedTourPayBonus 
                    ? "\n<color=#00FFAA><b>VETERAN COMBAT BONUS: +10% TOTAL PAY ACTIVE!</b></color>" 
                    : (s.isVoluntaryExtendedTour ? $"\n<color=#FFD700>Deployed {s.consecutiveCombatSemesters * 0.5f:F1}/1.0 yrs continuous toward +10% Pay Bonus</color>" : "");

                string extLabel = s.isVoluntaryExtendedTour ? "Revoke Tour Extension (Rotate to Base)" : "Volunteer for Extended Frontline Tour";
                string extDesc = $"Volunteer to remain in active combat longer than standard 6-month rotations. Continuous service past 1 year grants an exclusive <b>+10% Total Pay Increase</b> that stacks with all other earnings! Current: {extStatus}{extBonusStatus}";

                CreateActivityCard(extLabel, extDesc, s.isVoluntaryExtendedTour ? new Color(0.70f, 0.45f, 0.15f) : new Color(0.80f, 0.25f, 0.20f), () =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ToggleExtendedCombatTour();
                    }
                    RefreshUI();
                });
            }

            // 3. Officer Academy Admission
            if (!s.isOfficer && s.careerTrack != CareerTrack.MilitaryWarAcademy)
            {
                long academyTuition = (s.militaryBranch == MilitaryBranch.AirForce)
                    ? EconomyData.AIRFORCE_ACADEMY_TUITION_COST
                    : EconomyData.ARMY_ACADEMY_TUITION_COST;
                int academyYears = (s.militaryBranch == MilitaryBranch.AirForce)
                    ? (EconomyData.AIRFORCE_ACADEMY_SEMESTERS / 2)
                    : (EconomyData.ARMY_WAR_ACADEMY_SEMESTERS / 2);
                string academyName = (s.militaryBranch == MilitaryBranch.AirForce)
                    ? "Imperial Flight Academy"
                    : "Imperial War Academy";
                string sfName = s.specialForcesBranchName;
                string ageNotice = s.age > 35.0f ? " <color=#FF4444>[MAX AGE 35 EXCEEDED]</color>" : " (Max Age: 35)";
                string academyReqs = $"Requires <b>55% Discipline</b> (Current: {s.intellect:F0}%) and <b>55% Physical Readiness</b> (Current: {s.fitness:F0}%). Max Admission Age: <b>35</b> (Current: {s.age:F1}). Limited seats quota. Graduates commissioned directly as <b>O-1 Officer</b>. Top 10% qualify for elite <b>{sfName}</b> selection!";

                if (s.cashBalance >= academyTuition)
                {
                    CreateActivityCard($"Apply for {academyName} (${academyTuition:N0} Cash Upfront){ageNotice}",
                        $"Pay ${academyTuition:N0} tuition upfront in cash for the {academyYears}-year officer program. {academyReqs}",
                        s.age > 35.0f ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.20f, 0.60f, 0.40f), () =>
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.ApplyForOfficerAcademy(takeLoan: false);
                            }
                            RefreshUI();
                        });
                }

                CreateActivityCard($"Apply for {academyName} (9% Loan, {s.academyLoanDeductionPercent * 100f:F0}% Pay Deduction){ageNotice}",
                    $"Finance the ${academyTuition:N0} tuition with a 9% annual interest Education Loan ({academyYears} years). {academyReqs}",
                    s.age > 35.0f ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.20f, 0.50f, 0.85f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.ApplyForOfficerAcademy(takeLoan: true, s.academyLoanDeductionPercent);
                        }
                        RefreshUI();
                    });
            }

            // 4. Academy Loan Repayment Rate Adjustment
            if (s.academyTuitionDebt > 0)
            {
                long monthlyDeduction = SalaryCalculator.CalculateMonthlyLoanDeduction(s);
                string loanDesc = $"Outstanding Debt: <b>${s.academyTuitionDebt:N0}</b> (9% annual interest). " +
                                  $"Current Deduction: <b>{s.academyLoanDeductionPercent * 100f:F0}% of net salary (-${monthlyDeduction:N0}/mo)</b>. " +
                                  $"Minimum 10% required. Click to cycle repayment rate (10%, 15%, 20%, 30%, 50%).";

                CreateActivityCard($"Adjust Academy Loan Deduction ({s.academyLoanDeductionPercent * 100f:F0}%)",
                    loanDesc,
                    new Color(0.85f, 0.55f, 0.15f), () =>
                    {
                        float currentRate = s.academyLoanDeductionPercent;
                        float nextRate = 0.10f;
                        if (Mathf.Approximately(currentRate, 0.10f)) nextRate = 0.15f;
                        else if (Mathf.Approximately(currentRate, 0.15f)) nextRate = 0.20f;
                        else if (Mathf.Approximately(currentRate, 0.20f)) nextRate = 0.30f;
                        else if (Mathf.Approximately(currentRate, 0.30f)) nextRate = 0.50f;
                        else nextRate = 0.10f;

                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.SetAcademyLoanDeductionPercent(nextRate);
                        }
                        RefreshUI();
                    });
            }

            // 5. Special Forces Pathways
            if (!s.isSpecialForces && !s.isSpecialForcesCandidate)
            {
                string sfAgeNotice = s.age > 30.0f ? " <color=#FF4444>[MAX AGE 30 EXCEEDED]</color>" : " (Max Age: 30)";
                bool isCadetEligible = s.careerTrack == CareerTrack.MilitaryWarAcademy && s.academySemestersCompleted >= 2;
                bool isGraduateEligible = s.graduatedWarAcademy;

                // Aspiration card for any active soldier or cadet
                if (!s.isCivilian && !s.isRetired)
                {
                    CreateActivityCard("🏋️ Aspire to Special Forces: Train for Selection",
                        "Like many soldiers in the ranks, you dream of earning the prestigious Special Forces Beret. Run weighted rucks, study terrain maps, and push physical limits to prepare for Selection (+Fitness, +Discipline, +Combat Readiness).",
                        new Color(0.25f, 0.45f, 0.70f), () =>
                        {
                            s.fitness = Mathf.Min(100f, s.fitness + 5f);
                            s.discipline = Mathf.Min(100f, s.discipline + 4f);
                            s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 5f);
                            GameEvents.OnFeedEventLogged?.Invoke("SF ASPIRATION",
                                $"<color=#00D4FF><b>[SPECIAL FORCES SELECTION PREPARATION]</b></color> You strapped on a 65-lb rucksack and ran midnight endurance sprints on the perimeter road. " +
                                $"Comrades in your squad cheered you on: <i>'Go for it! Bring back the Beret for our platoon!'</i> (+5 Fitness, +4 Discipline, +5 Combat Readiness).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                            RefreshUI();
                        });
                }

                if (isCadetEligible || isGraduateEligible)
                {
                    if (s.isAcademyTop10Percent || s.academyPerformanceScore >= 90f)
                    {
                        string cadetOrGrad = isGraduateEligible ? "Top 10% Honors Graduate" : "Top 10% Cadet";
                        CreateActivityCard($"Volunteer for Special Forces Assessment & Selection ({cadetOrGrad}){sfAgeNotice}",
                            $"Your stellar academic score of <b>{s.academyPerformanceScore:F1}%</b> earns you an official invitation to attend Special Forces Selection! Note: Selection is NOT a guaranteed appointment; over 70% wash out during Hell Week. Earning the Beret grants legendary military respect.",
                            s.age > 30.0f ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.75f, 0.20f, 0.25f), () =>
                            {
                                if (GameManager.Instance != null)
                                {
                                    GameManager.Instance.JoinSpecialForcesFromAcademy();
                                }
                                RefreshUI();
                            });
                    }
                    else if (isCadetEligible)
                    {
                        CreateActivityCard($"Special Forces Selection (Top 10% Required){sfAgeNotice}",
                            $"Requires Academy standing >= 90% (Top 10%) and Age <= 30. Current Standing: <b>{s.academyPerformanceScore:F1}%</b>. Train physical readiness and tactics to reach the top decile!",
                            new Color(0.50f, 0.50f, 0.50f), () =>
                            {
                                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                                    $"<color=#FF8800>[TOP 10% REQUIRED]</color> Special Forces recruitment is restricted to the top 10% of cadets (Score >= 90%). Current score: <b>{s.academyPerformanceScore:F1}%</b>.");
                                RefreshUI();
                            });
                    }
                }

                // Active Duty Petition (Both Enlisted and Officers can petition!)
                if (!s.isCivilian && !s.isRetired && s.careerTrack != CareerTrack.MilitaryWarAcademy && s.careerTrack != CareerTrack.MilitaryRecruitBasic)
                {
                    string roleTitle = s.isOfficer ? "Officer Detachment Commander" : "Enlisted Operator";
                    string sfCODesc = $"Petition your Commanding Officer for official orders to attend Special Forces Assessment & Selection (SFAS) ({roleTitle}). " +
                                      $"Candidate Age Limit: <b>30</b> (Current: {s.age:F1}). Requires <b>CO Favor >= 70%</b> and <b>Merit >= 60%</b>. " +
                                      $"Over 70% of candidates wash out during the Crucible; operators who survive Hell Week earn immense social respect and +$800/mo stipend!";

                    CreateActivityCard($"Petition CO for Special Forces Selection Orders ({roleTitle}){sfAgeNotice}",
                        sfCODesc,
                        s.age > 30.0f ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.70f, 0.25f, 0.35f), () =>
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.RequestCOSpecialForcesRecommendation();
                            }
                            RefreshUI();
                        });
                }
            }
            else if (s.isSpecialForcesCandidate)
            {
                string phaseStr = s.specialForcesTrainingSemesters == 0 
                    ? "Phase 1: SFAS Assessment & The Crucible (Hell Week)" 
                    : "Phase 2: Tactical Q-Course, Advanced Weapons & SERE Survival";

                CreateActivityCard($"Special Forces Candidate: {phaseStr}",
                    $"Currently enduring the brutal Selection pipeline. Over 70% of candidates wash out before graduation. Current Phase: <b>{phaseStr}</b>. Advance semester to face the next trials.",
                    new Color(0.85f, 0.45f, 0.15f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                            $"<color=#FFD700>[SF CANDIDATE]</color> You are currently enrolled in <b>{phaseStr}</b>. Steel your body and mind for the trials ahead.");
                        RefreshUI();
                    });
            }
            else if (s.isSpecialForces)
            {
                // Elite operator prestige activities
                CreateActivityCard("⭐ Special Forces Prestige: Mentor Aspiring Soldiers",
                    "Regular soldiers in the barracks look up to you with legendary reverence. Mentor eager soldiers who dream of attempting Special Forces Selection (+Squad Respect, +Merit, +10 Barracks Morale).",
                    new Color(0.95f, 0.75f, 0.20f), () =>
                    {
                        s.squadRespect = Mathf.Min(100f, s.squadRespect + 8f);
                        s.meritRating = Mathf.Min(100f, s.meritRating + 6f);
                        s.happiness = Mathf.Min(100f, s.happiness + 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES PRESTIGE",
                            $"<color=#FFD700><b>[MENTORING ASPIRING OPERATORS]</b></color> A crowd of young soldiers and junior NCOs gathered around your bunk in the barracks. " +
                            $"You shared tactical advice on surviving the Crucible, land navigation, and mental toughness. The men saluted with deep admiration: <i>'Thank you, Operator!'</i> (+8 Squad Respect, +6 Merit, +10 Morale).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });

                CreateActivityCard("⚡ Elite Commando Physical Regimen",
                    "Conduct a 30-mile weighted tactical ruck and combat marksmanship drill. Regular soldiers watching the compound are awed by your elite standard (+Fitness, +Combat Skill, +Aura).",
                    new Color(0.20f, 0.65f, 0.35f), () =>
                    {
                        s.fitness = Mathf.Min(100f, s.fitness + 4f);
                        s.combatSkill = Mathf.Min(100f, s.combatSkill + 3f);
                        s.squadRespect = Mathf.Min(100f, s.squadRespect + 3f);
                        GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES DRILL",
                            $"<color=#00FFAA><b>[ELITE COMMANDO CONDITIONING]</b></color> You completed a grueling 30-mile weighted ruck march in full kit followed by dynamic live-fire target engagement. " +
                            $"Regular troops in the compound stopped and watched in awe at your relentless stamina (+Fitness, +Combat Skill, +3 Squad Respect).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });

                CreateActivityCard("Special Forces Operator Service Record",
                    $"Active Elite Operator. Covert Operations Survived: <b>{s.specialForcesMissionsSurvived}</b>. Drawing <b>+$800/mo Special Operations Hazard Pay</b>. Recognized across the armed forces with supreme social prestige.",
                    new Color(0.15f, 0.55f, 0.35f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                            $"<color=#00FFAA>[SPECIAL FORCES OPERATOR]</color> Service File: {s.specialForcesMissionsSurvived} covert operations survived. Decorated with Special Forces Elite Beret / Wings.");
                        RefreshUI();
                    });
            }

            // 6. Branch Transfers
            if (s.militaryBranch != MilitaryBranch.AirForce && !s.isRetired && !s.isCivilian)
            {
                CreateActivityCard("Branch Transfer: Imperial Air Force (Flight Duty)",
                    "Apply to cross-train as a combat aviator in the 1st Tactical Aviation Wing. Earns an exclusive <b>+15% Aviation Flight Pay Bonus</b> on all military salary! Living conditions in air base quarters are superior (+15% Morale, +10% Health).",
                    new Color(0.20f, 0.55f, 0.85f), () =>
                    {
                        if (GameManager.Instance != null) GameManager.Instance.EnlistInAirForce(AirForceRole.FlightDuty);
                        RefreshUI();
                    });

                CreateActivityCard("Branch Transfer: Imperial Air Force (Ground Duty Staff)",
                    "Transfer to Air Base Ground Logistics, Avionics, and Radar Defense. Standard military base pay, safer deployments, and air-conditioned base housing (+15% Morale).",
                    new Color(0.30f, 0.45f, 0.65f), () =>
                    {
                        if (GameManager.Instance != null) GameManager.Instance.EnlistInAirForce(AirForceRole.GroundDutyStaff);
                        RefreshUI();
                    });
            }

            // 7. Theaters of War
            if (!s.isRetired && s.careerTrack != CareerTrack.MilitaryWarAcademy)
            {
                var curTheater = WarTheaterInfo.GetInfo(s.currentTheater);
                CreateActivityCard($"Theater of War: {curTheater.theaterName}",
                    $"Current Deployment: <b>{curTheater.theaterName}</b> ({curTheater.weatherAndEnvironment}). " +
                    $"Tours Completed Here: <b>{s.toursInCurrentTheater}</b>. {curTheater.description}",
                    new Color(0.60f, 0.20f, 0.20f), () =>
                    {
                        RefreshUI();
                    });

                if (s.currentTheater != WarTheaterType.ArdenRidgeTrenchLines && s.currentTheater != WarTheaterType.WesternMudTrenches)
                {
                    CreateActivityCard("Request Transfer: Arden Ridge Trench Lines",
                        "Request deployment to the Arden Ridge trench network. Heavy artillery, deep mud, and brutal static attrition (Danger Level 4, Hazard Pay $500).",
                        new Color(0.45f, 0.35f, 0.25f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.ArdenRidgeTrenchLines);
                            RefreshUI();
                        });
                }

                if (s.currentTheater != WarTheaterType.PortIronAmphibiousFront && s.currentTheater != WarTheaterType.PacificJungleIslands)
                {
                    CreateActivityCard("Request Transfer: Port Iron Amphibious Front",
                        "Request amphibious assault deployment to Port Iron. Fortified sea-walls, bunker clearing, and deadly crossfire (Danger Level 5, Hazard Pay $750).",
                        new Color(0.20f, 0.45f, 0.55f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.PortIronAmphibiousFront);
                            RefreshUI();
                        });
                }

                if (s.currentTheater != WarTheaterType.KaelenValleyUrbanZone && s.currentTheater != WarTheaterType.UrbanCitadel)
                {
                    CreateActivityCard("Request Transfer: Kaelen Valley Urban Zone",
                        "Request deployment to ruined city canyons. Sniper alleys, room-clearing CQB, and IED ambushes (Danger Level 4, Hazard Pay $600).",
                        new Color(0.50f, 0.25f, 0.45f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.KaelenValleyUrbanZone);
                            RefreshUI();
                        });
                }

                if (s.currentTheater != WarTheaterType.BadlandsSteppeRecon)
                {
                    CreateActivityCard("Request Transfer: Badlands Steppe Recon",
                        "Request deployment to wide arid desert expanse. Mechanized vehicle patrols, heat exhaustion, and long-range skirmishes (Danger Level 3, Hazard Pay $450).",
                        new Color(0.65f, 0.45f, 0.20f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.BadlandsSteppeRecon);
                            RefreshUI();
                        });
                }

                if (s.currentTheater != WarTheaterType.NorthernTaigaOutpost && s.currentTheater != WarTheaterType.EasternFrozenSteppe)
                {
                    CreateActivityCard("Request Transfer: Northern Taiga Outpost",
                        "Request arctic deployment to sub-zero pine frontier. Blizzards, hypothermia, deep snow, and guerrilla mountain insurgency (Danger Level 3, Hazard Pay $550).",
                        new Color(0.25f, 0.50f, 0.65f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.NorthernTaigaOutpost);
                            RefreshUI();
                        });
                }
            }

            // 8. Post-Service Veteran Careers
            if (s.isRetired)
            {
                if (s.veteranCareer == VeteranCareer.None)
                {
                    CreateActivityCard("Post-Service: Attend University on GI Bill ($500/mo)",
                        "Enroll in higher education funded by military veteran benefits. Earns $500/mo GI stipend and boosts intellect.",
                        new Color(0.20f, 0.55f, 0.70f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.StartVeteranCareer(VeteranCareer.UniversityGI);
                            RefreshUI();
                        });

                    CreateActivityCard("Post-Service: Private Defense Logistics ($2,500/mo)",
                        "Consult for arms manufacturing and tactical logistics firms. High civilian salary ($2,500/mo).",
                        new Color(0.65f, 0.45f, 0.15f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.StartVeteranCareer(VeteranCareer.DefenseContractor);
                            RefreshUI();
                        });

                    if (s.medalsEarnedCount >= 2)
                    {
                        CreateActivityCard("Civic Ambition: Run for City Mayor ($4,000/mo)",
                            "Use your military decorations and honorable record to run for Mayor of your hometown. Earns $4,000/mo.",
                            new Color(0.20f, 0.65f, 0.40f), () =>
                            {
                                if (GameManager.Instance != null) GameManager.Instance.StartVeteranCareer(VeteranCareer.CityMayor);
                                RefreshUI();
                            });
                    }

                    if (s.isOfficer)
                    {
                        CreateActivityCard("Civic Ambition: Run for Provincial Governor ($8,000/mo)",
                            "Leverage your commissioned officer command legacy to campaign for Provincial Governor. Earns $8,000/mo.",
                            new Color(0.70f, 0.50f, 0.10f), () =>
                            {
                                if (GameManager.Instance != null) GameManager.Instance.StartVeteranCareer(VeteranCareer.RegionalGovernor);
                                RefreshUI();
                            });
                    }

                    if (s.currentRank != null && (s.currentRank.paygrade == "O-10" || s.currentRank.rankName.Contains("General")))
                    {
                        CreateActivityCard("Cabinet Post: Appointed Minister of Defense ($18,000/mo)",
                            "Assume command of national military readiness and defense procurement as Minister of Defense ($18,000/mo).",
                            new Color(0.85f, 0.20f, 0.25f), () =>
                            {
                                if (GameManager.Instance != null) GameManager.Instance.StartVeteranCareer(VeteranCareer.MinisterOfDefense);
                                RefreshUI();
                            });
                    }
                }
                else
                {
                    CreateActivityCard($"Veteran Career Active: {s.veteranCareer}",
                        $"You are actively serving in civilian society as <b>{s.veteranCareer}</b>. Your veteran pension ($1,500/mo) and career earnings continue every turn.",
                        new Color(0.15f, 0.60f, 0.45f), () =>
                        {
                            RefreshUI();
                        });
                }
            }

            // Dress Uniform & Ribbon Rack Inspection
            if (!s.isCivilian)
            {
                CreateActivityCard("🎖️ Inspect Service Dress Uniform & Ribbon Rack",
                    "Inspect your full service dress uniform, ribbon rack order of precedence, formal parchment citations, and the fallen squadmates memorial wall.",
                    new Color(0.85f, 0.70f, 0.20f), () =>
                    {
                        var rack = MedalRibbonRackModal.Instance ?? Object.FindAnyObjectByType<MedalRibbonRackModal>(FindObjectsInactive.Include);
                        if (rack != null)
                        {
                            CloseModal();
                            rack.OpenModal(s);
                        }
                        else
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("HONORS",
                                $"<color=#FFD700>[SERVICE UNIFORM INSPECTION]</color> Current Rank: <b>{s.GetRankDisplayName()}</b> | Medals: <b>{s.earnedMedals.Count}</b>.");
                            RefreshUI();
                        }
                    });

                CreateActivityCard("📋 Review Official Personnel File (Career Dossier)",
                    "Review your complete military 201 file, record of deployments, service longevity, citations, and promotion history.",
                    new Color(0.35f, 0.55f, 0.70f), () =>
                    {
                        var dossier = CareerDossierModal.Instance ?? Object.FindAnyObjectByType<CareerDossierModal>(FindObjectsInactive.Include);
                        if (dossier != null)
                        {
                            CloseModal();
                            dossier.OpenDossier(s);
                        }
                        else
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("DOSSIER",
                                $"<color=#00D4FF>[PERSONNEL DOSSIER]</color> Service Record: <b>{s.characterName}</b> | Rank: <b>{s.GetRankDisplayName()}</b> | Service: <b>{(s.totalSemestersLived * 0.5f):F1} years</b>.");
                            RefreshUI();
                        }
                    });
            }
        }

        private void BuildTrainingAndDrillsActivities(SoldierProfile s)
        {
            if (s.militaryBranch == MilitaryBranch.AirForce && !s.isRetired)
            {
                // 1. UPT Enrolment & Graduation Pipeline
                if (s.flightSchoolStatus == FlightSchoolStatus.None || s.flightSchoolStatus == FlightSchoolStatus.WashedOut)
                {
                    CreateActivityCard("Apply for Flight School: Undergraduate Pilot Training (UPT)",
                        "Apply to attend the Imperial Air Force Undergraduate Pilot Training. Requires physical fitness and mental agility. Graduates earn Wings of Gold and +25% Flight Duty Pay.",
                        new Color(0.20f, 0.55f, 0.85f), () =>
                        {
                            AirForceFlightSchoolEngine.ApplyForFlightSchool(s, GameManager.Instance);
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                            RefreshUI();
                        });
                }
                else if (s.flightSchoolStatus == FlightSchoolStatus.EnrolledUPT)
                {
                    if (s.flightSchoolSemestersEnrolled < 2)
                    {
                        CreateActivityCard($"UPT Flight Training (Term {s.flightSchoolSemestersEnrolled + 1}/2)",
                            "Conduct high-G centrifuge spin training, flight simulators, and solo cross-country navigation. (+40 Flight Hours, +Fitness, +Intellect, consumes 1 AP).",
                            new Color(0.25f, 0.60f, 0.90f), () =>
                            {
                                if (s.hasTrainedFlightThisSemester)
                                {
                                    GameEvents.OnFeedEventLogged?.Invoke("FLIGHT SCHOOL",
                                        "<color=#FFAA00>[SEMESTER FLIGHT CAP]</color> You already logged flight curriculum hours for this semester. Advance the tour to continue to next term's flight syllabus!");
                                    return;
                                }
                                if (!s.TryConsumeActionPoint("Flight Training"))
                                {
                                    GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                                        "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester.");
                                    return;
                                }
                                s.hasTrainedFlightThisSemester = true;
                                AirForceFlightSchoolEngine.ProcessUPTSemester(s, GameManager.Instance);
                                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                                RefreshUI();
                            });
                    }
                    else
                    {
                        CreateActivityCard("Graduate UPT: Commission as Fighter Pilot",
                            "Earn your Gold Wings and select Fighter Pilot stream: Air Superiority, high-G dogfights, and AIM interceptors.",
                            new Color(0.95f, 0.75f, 0.20f), () =>
                            {
                                AirForceFlightSchoolEngine.GraduateFlightSchool(s, PilotSpecialization.FighterPilot, GameManager.Instance);
                                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                                RefreshUI();
                            });

                        CreateActivityCard("Graduate UPT: Commission as CAS Strike Pilot",
                            "Earn your Gold Wings and select CAS Strike stream: A-10 Warthog, 30mm GAU-8 Gatling cannon, and anti-armor tank-busting.",
                            new Color(0.85f, 0.45f, 0.15f), () =>
                            {
                                AirForceFlightSchoolEngine.GraduateFlightSchool(s, PilotSpecialization.CasStrikePilot, GameManager.Instance);
                                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                                RefreshUI();
                            });

                        CreateActivityCard("Graduate UPT: Commission as Heavy Transport Pilot",
                            "Earn your Gold Wings and select Heavy Transport stream: C-130 / C-17 tactical airdrops and rough dirt strip landings.",
                            new Color(0.30f, 0.65f, 0.40f), () =>
                            {
                                AirForceFlightSchoolEngine.GraduateFlightSchool(s, PilotSpecialization.HeavyTransportPilot, GameManager.Instance);
                                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                                RefreshUI();
                            });
                    }
                }

                // 2. Active Pilot Sorties & Weapons School
                if (s.isFlightDuty || s.flightSchoolStatus == FlightSchoolStatus.Graduated)
                {
                    string specName = s.pilotSpecialization switch
                    {
                        PilotSpecialization.FighterPilot => "Fighter Pilot (Air Superiority)",
                        PilotSpecialization.CasStrikePilot => "CAS Strike Pilot (Tank Buster)",
                        PilotSpecialization.HeavyTransportPilot => "Heavy Transport Pilot (Airlift)",
                        _ => "Combat Aviator"
                    };

                    CreateActivityCard($"Fly Operational Combat Sortie [{specName}]",
                        $"Scramble on frontline aviation sortie. Encounter dynamic aerial dilemmas: Dogfights, SAM launches, flameouts, and bailout SERE survival. Flight Hours: <b>{s.flightHoursLogged} hrs</b>.",
                        new Color(0.15f, 0.50f, 0.85f), () =>
                        {
                            AirForceFlightSchoolEngine.ExecuteAviationSortie(s, null, 0, GameManager.Instance);
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                            RefreshUI();
                        });

                    if (AirForceFlightSchoolEngine.CheckTopGunEligibility(s))
                    {
                        CreateActivityCard("⭐ Apply for Top Gun Fighter Weapons School",
                            "Your flight hours and aerial prowess qualify you for the prestigious Top Gun Weapons School! Become an elite combat flight instructor (+Respect, +Merit, Top Gun honor).",
                            new Color(1f, 0.84f, 0f), () =>
                            {
                                AirForceFlightSchoolEngine.EnrollInTopGunSchool(s, GameManager.Instance);
                                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                                RefreshUI();
                            });
                    }
                }
                else if (s.flightSchoolStatus != FlightSchoolStatus.EnrolledUPT)
                {
                    CreateActivityCard("Turbine & Radar Avionics Calibration",
                        "Conduct pre-flight diagnostic overhauls on fighter jet turbines and tactical targeting radar arrays (+5 Discipline, +4 Board Points).",
                        new Color(0.35f, 0.50f, 0.65f), () =>
                        {
                            s.discipline = Mathf.Min(100f, s.discipline + 5f);
                            s.promotionPoints += 4;
                            GameEvents.OnFeedEventLogged?.Invoke("AVIONICS",
                                "<color=#00FFAA>[FLIGHT LINE MAINTENANCE]</color> You inspected and prepped an entire squadron of fighter jets for combat takeoff (+5 Discipline, +4 Merit Points).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                            RefreshUI();
                        });
                }
                return;
            }

            // Standard Army Training
            CreateActivityCard("PT & Tactical Obstacle Course (1 AP)",
                "Complete the grueling military physical fitness test and confidence course.",
                new Color(0.18f, 0.65f, 0.35f), () =>
                {
                    if (!s.TryConsumeActionPoint("PT Obstacle Course"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester. Advance the semester to recover your physical stamina!");
                        return;
                    }
                    s.fitness = Mathf.Min(100f, s.fitness + 4f);
                    s.promotionPoints += 5;
                    s.health = Mathf.Max(20f, s.health - 3f);
                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                        $"<color=#00FFAA>[FITNESS DRILL]</color> You aced the physical fitness course. (+4 Fitness, +5 Board Points, {s.actionPointsRemaining} AP remaining).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Live-Fire Firing Range (1 AP)",
                "Qualify on military service rifles, light machine guns, and CQB marksmanship.",
                new Color(0.20f, 0.50f, 0.80f), () =>
                {
                    if (!s.TryConsumeActionPoint("Live-Fire Range"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester. Advance the semester to recover your physical stamina!");
                        return;
                    }
                    s.combatSkill = Mathf.Min(100f, s.combatSkill + 3.5f);
                    s.promotionPoints += 4;
                    GameEvents.OnFeedEventLogged?.Invoke("MARKSMANSHIP",
                        $"<color=#00D4FF>[EXPERT QUALIFICATION]</color> You earned Expert Marksman badge points! (+3.5 Combat Skill, +4 Board Points, {s.actionPointsRemaining} AP remaining).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Combat Readiness & Qualification Drills (1 AP)",
                "Participate in advanced tactical field maneuvers, squad coordination, and obstacle course qualifications.",
                new Color(0.55f, 0.25f, 0.75f), () =>
                {
                    if (!s.TryConsumeActionPoint("Combat Drills"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester. Advance the semester to recover your physical stamina!");
                        return;
                    }
                    MilitaryPhysicalConditioningEngine.ConductFitnessQualificationTest(s);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("🎯 Tactical Firing Range (Interactive Target Minigame, 1 AP)",
                "Step onto the live-fire lanes! Test your trigger discipline, windage, and target accuracy to earn expert marksmanship scores.",
                new Color(0.20f, 0.65f, 0.90f), () =>
                {
                    if (!s.TryConsumeActionPoint("Firing Range Minigame"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester. Advance the semester to recover your physical stamina!");
                        return;
                    }
                    var mg = TacticalMinigamesModal.Instance ?? Object.FindAnyObjectByType<TacticalMinigamesModal>(FindObjectsInactive.Include);
                    if (mg != null)
                    {
                        CloseModal();
                        mg.OpenFiringRange();
                    }
                    else
                    {
                        s.combatSkill = Mathf.Min(100f, s.combatSkill + 4f);
                        GameEvents.OnFeedEventLogged?.Invoke("MARKSMANSHIP", "<color=#00D4FF>[FIRING RANGE]</color> Expert marksmanship qualification completed (+4 Combat Skill).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    }
                });

            CreateActivityCard("🛑 Forward Border Checkpoint (Papers, Please Minigame, 1 AP)",
                "Take command of the forward security checkpoint. Inspect civilian passports, transit visas, forged documents, and confiscate contraband!",
                new Color(0.85f, 0.55f, 0.15f), () =>
                {
                    if (!s.TryConsumeActionPoint("Checkpoint Duty"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester. Advance the semester to recover your physical stamina!");
                        return;
                    }
                    var mg = TacticalMinigamesModal.Instance ?? Object.FindAnyObjectByType<TacticalMinigamesModal>(FindObjectsInactive.Include);
                    if (mg != null)
                    {
                        CloseModal();
                        mg.OpenCheckpoint();
                    }
                    else
                    {
                        s.meritRating = Mathf.Min(100f, s.meritRating + 5f);
                        GameEvents.OnFeedEventLogged?.Invoke("SECURITY", "<color=#FFD700>[CHECKPOINT DUTY]</color> Thorough security inspection maintained (+5 Merit).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    }
                });
        }

        private void BuildQuartermasterActivities(SoldierProfile s)
        {
            long helmetPrice = WartimeMarketEngine.GetAdjustedPrice(250L, GearCategory.Armor);
            long vestPrice = WartimeMarketEngine.GetAdjustedPrice(500L, GearCategory.Armor);
            long sidearmPrice = WartimeMarketEngine.GetAdjustedPrice(400L, GearCategory.Weapon);
            long watchPrice = WartimeMarketEngine.GetAdjustedPrice(350L, GearCategory.Morale);
            long medkitPrice = WartimeMarketEngine.GetAdjustedPrice(150L, GearCategory.Medical);
            long cigPrice = WartimeMarketEngine.GetAdjustedPrice(50L, GearCategory.Morale);

            string shockBadge = "";
            if (WartimeMarketEngine.CurrentState.activeShock != MacroEconomicShock.NormalStagnant)
            {
                shockBadge = $" <color=#FF8844>[{WartimeMarketEngine.CurrentState.headlineText}]</color>";
            }

            if (!s.hasSteelHelmet)
            {
                CreateActivityCard($"PX Store: Buy M1 Reinforced Steel Helmet (${helmetPrice:N0}){shockBadge}",
                    $"Deflects mortar shrapnel and sniper ricochets. Lowers frontline fatality risk by 5%. (Base $250 -> Current ${helmetPrice:N0})",
                    new Color(0.20f, 0.50f, 0.35f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_helmet", "M1 Reinforced Steel Helmet", "Military ballistic helmet.", GearCategory.Armor, helmetPrice, 0.05f));
                        }
                        RefreshUI();
                    });
            }

            if (!s.hasBallisticVest)
            {
                CreateActivityCard($"PX Store: Buy Ballistic Flak Vest (${vestPrice:N0}){shockBadge}",
                    $"Heavy-duty protective body armor. Lowers frontline wound and fatality risk by 8%. (Base $500 -> Current ${vestPrice:N0})",
                    new Color(0.20f, 0.50f, 0.35f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_vest", "Ballistic Flak Vest", "Protective torso armor.", GearCategory.Armor, vestPrice, 0.08f));
                        }
                        RefreshUI();
                    });
            }

            if (!s.hasServiceSidearm)
            {
                CreateActivityCard($"PX Store: Buy Trench Service Sidearm (${sidearmPrice:N0}){shockBadge}",
                    $"Reliable close-quarters pistol. Adds +10 CQB combat skill during ambushes and room clearing. (Base $400 -> Current ${sidearmPrice:N0})",
                    new Color(0.55f, 0.35f, 0.20f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_sidearm", "Service Sidearm", "Close-quarters combat pistol.", GearCategory.Weapon, sidearmPrice, 0f, 10f));
                        }
                        RefreshUI();
                    });
            }

            if (!s.hasLuckyWatch)
            {
                CreateActivityCard($"PX Store: Buy Engraved Pocket Watch (${watchPrice:N0}){shockBadge}",
                    $"A keepsake lucky charm. Provides a permanent passive +4% boost to frontline survival rolls. (Base $350 -> Current ${watchPrice:N0})",
                    new Color(0.65f, 0.50f, 0.15f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_watch", "Engraved Pocket Watch", "Keepsake lucky charm.", GearCategory.Morale, watchPrice, 0.04f));
                        }
                        RefreshUI();
                    });
            }

            CreateActivityCard($"PX Store: Buy Field Trauma Kit (${medkitPrice:N0}) [Have: {s.fieldMedkitsCount}]{shockBadge}",
                $"Emergency military trauma syrette. Consumed to resuscitate yourself if you suffer a fatal wound! (Base $150 -> Current ${medkitPrice:N0})",
                new Color(0.18f, 0.55f, 0.50f), () =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_medkit", "Emergency Field Trauma Kit", "Life-saving syrette.", GearCategory.Medical, medkitPrice, consumable: true, count: 1));
                    }
                    RefreshUI();
                });

            CreateActivityCard($"PX Store: Buy Cigarette Pack (${cigPrice:N0}) [Have: {s.cigarettesCount}]{shockBadge}",
                $"Wartime currency and camaraderie builder. Share with squadmates to boost loyalty and fireteam cohesion. (Base $50 -> Current ${cigPrice:N0})",
                new Color(0.45f, 0.40f, 0.30f), () =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_cig", "PX Cigarette Pack", "Camaraderie builder.", GearCategory.Morale, cigPrice, consumable: true, count: 1));
                    }
                    RefreshUI();
                });
        }

        private void BuildBarracksAndSquadActivities(SoldierProfile s)
        {
            if (s.militaryBranch == MilitaryBranch.AirForce)
            {
                CreateActivityCard("Air Base Officers & Aces Club",
                    "Unwind with squadron wingmen at the Air Base Mess, drinking top-shelf spirits and sharing aerial tactics (+15 Happiness).",
                    new Color(0.25f, 0.60f, 0.50f), () =>
                    {
                        s.happiness = Mathf.Min(100f, s.happiness + 15f);
                        GameEvents.OnFeedEventLogged?.Invoke("AIR BASE",
                            "<color=#00FFAA>[OFFICERS CLUB]</color> You enjoyed an evening celebrating aerial victories with your squadron wingmen (+15 Happiness).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
            else
            {
                CreateActivityCard("Enlisted Mess Hall & Recreation",
                    "Blow off steam with squadmates, enjoy hot chow, and raise morale.",
                    new Color(0.30f, 0.55f, 0.45f), () =>
                    {
                        s.happiness = Mathf.Min(100f, s.happiness + 15f);
                        GameEvents.OnFeedEventLogged?.Invoke("SOCIAL",
                            "<color=#00FFAA>[RECREATION]</color> You enjoyed a warm feast and played cards in the barracks mess hall (+15% Happiness).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }

            if (!s.isCivilian && !s.isRetired)
            {
                CreateActivityCard("PX Mess: Buy Squad Drinks & Feast ($150)",
                    "Buy a lavish round of whiskey and steaks at the Enlisted/Officers Club for your entire squad (+20 Squad Respect, +15 Comradeship with all squadmates).",
                    new Color(0.35f, 0.45f, 0.65f), () =>
                    {
                        EconomyEngine.BuySquadRounds(s);
                        RefreshUI();
                    });

                CreateActivityCard("🎲 Barracks Dice & Craps (Minigame)",
                    "Roll dice with your squadmates on a footlocker bunk. Place cash bets, win coin, and raise barracks morale (+Squad Comradeship).",
                    new Color(0.75f, 0.35f, 0.25f), () =>
                    {
                        var mg = TacticalMinigamesModal.Instance ?? Object.FindAnyObjectByType<TacticalMinigamesModal>(FindObjectsInactive.Include);
                        if (mg != null)
                        {
                            CloseModal();
                            mg.OpenBarracksDice();
                        }
                        else
                        {
                            s.happiness = Mathf.Min(100f, s.happiness + 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("BARRACKS", "<color=#00FFAA>[BARRACKS CRAPS]</color> You enjoyed a lively dice game with your squad (+10 Morale).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                            RefreshUI();
                        }
                    });

                CreateActivityCard("📬 Mail Call: Letters & Care Packages",
                    "Visit the regimental mail clerk to receive family correspondence, letters from sweethearts, and homemade care packages.",
                    new Color(0.30f, 0.55f, 0.70f), () =>
                    {
                        CloseModal();
                        MailCallEngine.RollMailCallEvent(s);
                    });
            }

            s.InitializeSquadMatesIfEmpty();
            foreach (var mate in s.squadMates)
            {
                if (mate.isDead)
                {
                    CreateActivityCard($"Brothers in Arms: {mate.GetDisplayName()} (KIA)",
                        $"<color=#FF4444>Fallen Hero ({mate.roleTitle}). Died: {mate.deathReason}. Battles Survived: {mate.battlesSurvived}.</color>\nPay tribute at the regimental memorial wall.",
                        new Color(0.40f, 0.15f, 0.15f), () =>
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("MEMORIAL",
                                $"<color=#FF8866>[FALLEN COMRADE MEMORIAL]</color> You visited the battlefield grave of {mate.GetDisplayName()}. You swore never to forget your squad brother.");
                            RefreshUI();
                        });

                    CreateActivityCard($"Requisition Replacement for {mate.GetDisplayName()} (KIA)",
                        $"Requisition a fresh recruit from the depot to restore full fireteam strength for the {mate.roleTitle} role.",
                        new Color(0.20f, 0.45f, 0.65f), () =>
                        {
                            SquadmateCommandEngine.RecruitReplacement(s, mate.role);
                            RefreshUI();
                        });
                }
                else
                {
                    string mateDesc = $"{mate.roleTitle} | Hometown: <b>{mate.hometown}</b> | Rel: <b>{mate.relationship:F0}%</b> | Morale: <b>{mate.morale:F0}%</b>\n<i>\"{mate.backstoryAndQuirk}\"</i>";
                    
                    CreateActivityCard($"Heart-to-Heart with {mate.GetDisplayName()}",
                        $"{mateDesc}\nSit down and speak of home, easing combat fatigue and building brotherhood (+12 Rel, +10 Morale).",
                        new Color(0.20f, 0.50f, 0.55f), () =>
                        {
                            SquadmateCommandEngine.TalkWithSquadmate(s, mate);
                            RefreshUI();
                        });

                    CreateActivityCard($"Fireteam: {mate.GetDisplayName()} (Share Smoke & Bond)",
                        $"Spend 1 Cigarette pack to share smokes beneath the revetment and strengthen fireteam loyalty (+15 Rel, +10 Happiness).",
                        new Color(0.20f, 0.40f, 0.60f), () =>
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.InteractWithSquadMate(mate, "ShareCigarettes");
                            }
                            RefreshUI();
                        });

                    CreateActivityCard($"Fireteam: {mate.GetDisplayName()} (Barracks Cards & Banter)",
                        $"Play cards in the barracks mess hall to blow off steam (+8 Rel, +8 Happiness).",
                        new Color(0.25f, 0.35f, 0.50f), () =>
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.InteractWithSquadMate(mate, "BunkBanter");
                            }
                            RefreshUI();
                        });
                }
            }

            if (SquadmateCommandEngine.CanExecuteTacticalCommand(s))
            {
                CreateActivityCard("⚔️ NCO/Officer Tactical Command: Issue Squad Maneuver",
                    "Exercise tactical authority as a Sergeant (E-5+) or Officer (O-1+)! Order your fireteam to execute a tactical flanking sweep or heavy suppression pin.",
                    new Color(0.70f, 0.40f, 0.10f), () =>
                    {
                        var order = UnityEngine.Random.value < 0.5f 
                            ? TacticalCommandOrder.ScoutFlankManeuver 
                            : TacticalCommandOrder.HeavySuppressionPin;
                        SquadmateCommandEngine.ExecuteTacticalCommand(s, order, null);
                        RefreshUI();
                    });
            }
        }

        private void BuildMedicalActivities(SoldierProfile s)
        {
            if (s.isCivilian)
            {
                CreateActivityCard("Civilian Medical Clinic Checkup ($80)",
                    "Receive routine medical care and restorative checkup ($80) to repair strain and fatigue.",
                    new Color(0.85f, 0.25f, 0.25f), () =>
                    {
                        if (s.cashBalance < 80)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("CLINIC", "<color=#FF4444>[INSUFFICIENT CASH]</color> Clinic visit costs $80.");
                            return;
                        }
                        s.cashBalance -= 80;
                        s.health = Mathf.Min(100f, s.health + 25f);
                        s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 15f);
                        GameEvents.OnFeedEventLogged?.Invoke("CLINIC",
                            "<color=#00FFAA>[HEALTH RESTORED]</color> The municipal doctors treated your ailments (+25 Health, +15 Readiness).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
            else
            {
                CreateActivityCard("Base Medical Bay & Recovery ($150)",
                    "Receive intensive medical therapy and physical rehabilitation ($150) to treat combat wounds.",
                    new Color(0.85f, 0.25f, 0.25f), () =>
                    {
                        if (s.cashBalance < 150)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Medical bay therapy costs $150.");
                            return;
                        }
                        s.cashBalance -= 150;
                        s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 25f);
                        s.health = Mathf.Min(100f, s.health + 30f);
                        GameEvents.OnFeedEventLogged?.Invoke("MEDICAL",
                            "<color=#00FFAA>[MEDICAL TREATMENT]</color> Military trauma surgeons treated your combat wounds (+25 Readiness, +30 Health).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }

            CreateActivityCard("Military Specialist Hospital Surgery ($500)",
                "Undergo specialized surgical care and trauma rehabilitation. Restores +50 Health and rehabilitates severe combat shrapnel and nerve trauma.",
                new Color(0.80f, 0.25f, 0.25f), () =>
                {
                    EconomyEngine.TreatAtImperialHospital(s);
                    RefreshUI();
                });

            if (!s.hasBionicProsthetics)
            {
                CreateActivityCard("Fit Advanced Carbon-Titanium Prosthetics ($2,500)",
                    "Fitted with lightweight carbon-composite running blades and orthopedic prosthetics, followed by intensive military rehab. Permanently grants <b>+12 Combat Skill</b>, <b>+8 Fitness</b>, and full health restoration!",
                    new Color(0.55f, 0.25f, 0.75f), () =>
                    {
                        if (s.cashBalance < 2500)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Advanced military prosthetics cost $2,500.");
                            return;
                        }
                        s.cashBalance -= 2500;
                        s.hasBionicProsthetics = true;
                        s.combatSkill = Mathf.Min(100f, s.combatSkill + 12f);
                        s.fitness = Mathf.Min(100f, s.fitness + 8f);
                        s.health = 100f;
                        s.AddTrait("Rehabilitated Veteran");
                        GameEvents.OnFeedEventLogged?.Invoke("MEDICAL",
                            "<color=#FFD700><b>[ADVANCED PROSTHETICS FITTED]</b></color> Military orthopedic surgeons fitted high-performance carbon-titanium prosthetics and completed physical rehab! (+12 Combat Skill, +8 Fitness, Healed to 100% Health).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }

            CreateActivityCard("Family Emergency Medical Care ($1,200)",
                "Fund emergency hospital surgeries and top apothecary elixir treatments for your parents and family (+30 Parent Relationship, +15 Clan Favor).",
                new Color(0.75f, 0.35f, 0.30f), () =>
                {
                    EconomyEngine.TreatFamilyMedicalEmergency(s);
                    RefreshUI();
                });

            // Convalescence Surgery if admitted
            if (s.inMedicalConvalescence)
            {
                CreateActivityCard("Specialist Trauma Surgery & Rehab ($400)",
                    "Undergo intensive orthopedic surgery and nerve grafting in the forward field hospital ($400) to maximize your chances of clearing the Medical Board.",
                    new Color(0.85f, 0.20f, 0.20f), () =>
                    {
                        if (s.cashBalance < FieldHospitalEngine.SPECIALIST_SURGERY_COST)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("SURGERY", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Specialist surgery costs $400.");
                            return;
                        }
                        FieldHospitalEngine.ProcessConvalescenceTurn(s, GameManager.Instance, optedForSpecialistSurgery: true);
                        RefreshUI();
                    });
            }

            // Veteran / Military Mental Health & Trauma Healing
            CreateActivityCard("Visit National War Memorial",
                "Pay quiet tribute before the black granite wall etched with the names of fallen brothers. Eases survivor's guilt (-25 Combat Stress, +15 Morale).",
                new Color(0.20f, 0.60f, 0.55f), () =>
                {
                    if (!s.TryConsumeActionPoint("War Memorial Vigil"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester.");
                        return;
                    }
                    CombatTraumaEngine.VisitWarMemorial(s);
                    RefreshUI();
                });

            CreateActivityCard("VA Peer Support & Trauma Therapy",
                "Attend group therapy with fellow combat veterans to process artillery nightmares and shell shock (-35 Combat Stress).",
                new Color(0.30f, 0.50f, 0.70f), () =>
                {
                    if (!s.TryConsumeActionPoint("VA Therapy"))
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                            "<color=#FFAA00>[EXHAUSTED FOR SEMESTER]</color> You have no Action Points remaining this semester.");
                        return;
                    }
                    CombatTraumaEngine.AttendVASupportGroup(s);
                    RefreshUI();
                });
        }

        private void BuildRealEstateAndCommerceActivities(SoldierProfile s)
        {
            BuildPersonalHousingActivities(s);
            BuildFamilyHousingActivities(s);
            BuildCommercialBusinessActivities(s);
        }

        private void BuildPersonalHousingActivities(SoldierProfile s)
        {
            if (s.housingTier == HousingTier.BarracksOrSlums)
            {
                CreateActivityCard("Move to Modest City Apartment ($1,500 Buy)",
                    "Move out of squalid tenements or barracks bunks into a private city apartment. Upkeep: $1,800/sem ($300/mo). Perks: +10% Morale, +5% Health recovery.",
                    new Color(0.30f, 0.60f, 0.40f), () =>
                    {
                        if (s.cashBalance < 1500)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("HOUSING", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Moving to an apartment requires $1,500 deposit and furnishing fees.");
                            return;
                        }
                        s.cashBalance -= 1500;
                        s.housingTier = HousingTier.ModestApartment;
                        s.happiness = Mathf.Min(100f, s.happiness + 15f);
                        GameEvents.OnFeedEventLogged?.Invoke("HOUSING",
                            "<color=#00FFAA><b>[APARTMENT LEASE SIGNED]</b></color> You moved into a private city apartment! (+15 Happiness, +10% Morale recovery).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
            else if (s.housingTier == HousingTier.ModestApartment)
            {
                CreateActivityCard("Upgrade to Comfortable Family Villa ($8,000 Buy)",
                    "Purchase a suburban family villa with a garden and nursery. Upkeep: $4,200/sem. Perks: +25% Morale, +15% Health, +5 Clan Favor.",
                    new Color(0.25f, 0.65f, 0.50f), () =>
                    {
                        if (s.cashBalance < 8000)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("HOUSING", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Upgrading to a family villa costs $8,000.");
                            return;
                        }
                        s.cashBalance -= 8000;
                        s.housingTier = HousingTier.ComfortableHome;
                        s.happiness = Mathf.Min(100f, s.happiness + 25f);
                        s.clanFavor = Mathf.Min(100f, s.clanFavor + 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("HOUSING",
                            "<color=#00FFAA><b>[FAMILY VILLA ACQUIRED]</b></color> You bought a beautiful family villa! (+25 Happiness, +10 Clan Favor).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
            else if (s.housingTier == HousingTier.ComfortableHome)
            {
                CreateActivityCard("Acquire Grand Noble Estate & Manor ($35,000 Buy)",
                    "Acquire a sprawling gated estate with private security staff and wine cellars. Upkeep: $5,000/sem. Perks: +50% Morale, +30% Health, +15 Clan Favor, +10 Prestige.",
                    new Color(0.75f, 0.55f, 0.15f), () =>
                    {
                        if (s.cashBalance < 35000)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("HOUSING", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Noble estate acquisition costs $35,000.");
                            return;
                        }
                        s.cashBalance -= 35000;
                        s.housingTier = HousingTier.NobleEstate;
                        s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 15f);
                        s.clanFavor = 100f;
                        GameEvents.OnFeedEventLogged?.Invoke("HOUSING",
                            "<color=#FFD700><b>[NOBLE ESTATE ACQUIRED]</b></color> You acquired a grand aristocratic manor! (+15 Political Influence, Maximum Clan Respect).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
            else if (s.housingTier == HousingTier.NobleEstate)
            {
                CreateActivityCard("Acquire Sovereign Imperial Palace Wing ($150,000 Buy)",
                    "Move into a grand imperial palace wing with private Praetorian guards and banqueting halls. Upkeep: $25,000/sem. Perks: Supreme Sovereign Prestige, +50 Political Influence.",
                    new Color(0.85f, 0.70f, 0.20f), () =>
                    {
                        if (s.cashBalance < 150000)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("HOUSING", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Imperial Palace Wing acquisition costs $150,000.");
                            return;
                        }
                        s.cashBalance -= 150000;
                        s.housingTier = HousingTier.ImperialPalaceWing;
                        s.politicalInfluence = 100f;
                        s.clanFavor = 100f;
                        GameEvents.OnFeedEventLogged?.Invoke("HOUSING",
                            "<color=#FFD700><b>[IMPERIAL PALACE WING ACQUIRED • SOVEREIGN PRESTIGE]</b></color> You moved into the Sovereign Palace Wing! (Maximum Political Sway & Royal Standing).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
        }

        private void BuildFamilyHousingActivities(SoldierProfile s)
        {
            if (s.familyHouseTier == FamilyHouseTier.None)
            {
                CreateActivityCard("Buy Family a Rural Homestead ($5,000)",
                    "Buy your parents their own farmland and rural cottage, lifting them out of wartime poverty! (+20 Parent Approval, +15 Clan Favor, +25 Living Standard).",
                    new Color(0.40f, 0.65f, 0.25f), () =>
                    {
                        EconomyEngine.BuyFamilyHouse(s, FamilyHouseTier.RuralHomestead);
                        RefreshUI();
                    });
            }
            else if (s.familyHouseTier == FamilyHouseTier.RuralHomestead)
            {
                CreateActivityCard("Buy Family a Capital City Townhouse ($25,000)",
                    "Move your family into an affluent neighborhood in Valeria Magna. (+35 Parent Approval, +30 Clan Favor, +10 Political Influence).",
                    new Color(0.20f, 0.60f, 0.65f), () =>
                    {
                        EconomyEngine.BuyFamilyHouse(s, FamilyHouseTier.CityTownhouse);
                        RefreshUI();
                    });
            }
            else if (s.familyHouseTier == FamilyHouseTier.CityTownhouse)
            {
                CreateActivityCard("Acquire Ancestral Dynastic Estate ($100,000)",
                    "Establish a permanent ancestral castle for your dynasty. (Max Living Standard 100, +40 Succession Score, +25 Political Influence).",
                    new Color(0.80f, 0.60f, 0.15f), () =>
                    {
                        EconomyEngine.BuyFamilyHouse(s, FamilyHouseTier.AncestralEstate);
                        RefreshUI();
                    });
            }
            else if (s.familyHouseTier == FamilyHouseTier.AncestralEstate)
            {
                CreateActivityCard("Erect High Ducal Palace Citadel ($500,000)",
                    "Build a towering clifftop ducal fortress commanding a private house guard! (+100 Succession Score, +50 Political Influence).",
                    new Color(0.85f, 0.65f, 0.15f), () =>
                    {
                        EconomyEngine.BuyFamilyHouse(s, FamilyHouseTier.DucalPalaceCitadel);
                        RefreshUI();
                    });
            }
            else if (s.familyHouseTier == FamilyHouseTier.DucalPalaceCitadel)
            {
                CreateActivityCard("Acquire Sovereign Royal Château ($2,500,000)",
                    "Purchase a legendary sovereign palace and rolling vineyards. (+250 Succession Score, Max 100 Political Influence, Sovereign Royalty).",
                    new Color(0.90f, 0.70f, 0.20f), () =>
                    {
                        EconomyEngine.BuyFamilyHouse(s, FamilyHouseTier.SovereignImperialChateau);
                        RefreshUI();
                    });
            }
            else if (s.familyHouseTier == FamilyHouseTier.SovereignImperialChateau)
            {
                CreateActivityCard("Found The Eternal Mountain Citadel ($10,000,000)",
                    "Carve an unassailable sovereign fortress into the imperial mountains! The ultimate pinnacle of multi-generational dynasty (+1000 Succession).",
                    new Color(1.0f, 0.85f, 0.20f), () =>
                    {
                        EconomyEngine.BuyFamilyHouse(s, FamilyHouseTier.ImperialDynasticSeat);
                        RefreshUI();
                    });
            }
        }

        private void BuildCommercialBusinessActivities(SoldierProfile s)
        {
            if (s.ownedBusiness == CommercialBusiness.None)
            {
                CreateBusinessCard(s, CommercialBusiness.Workshop, new Color(0.60f, 0.45f, 0.25f));
                CreateBusinessCard(s, CommercialBusiness.Tavern, new Color(0.70f, 0.40f, 0.20f));
                CreateBusinessCard(s, CommercialBusiness.MerchantCaravan, new Color(0.25f, 0.55f, 0.65f));
                CreateBusinessCard(s, CommercialBusiness.PrivateSecurityAgency, new Color(0.35f, 0.50f, 0.35f));
                CreateBusinessCard(s, CommercialBusiness.DefenseMunitionsConglomerate, new Color(0.75f, 0.25f, 0.20f));
                CreateBusinessCard(s, CommercialBusiness.ImperialShippingCartel, new Color(0.20f, 0.55f, 0.80f));
                CreateBusinessCard(s, CommercialBusiness.CentralBankingHouseAndMint, new Color(0.90f, 0.75f, 0.15f));
            }
            else
            {
                string curName = EconomyData.GetBusinessDisplayName(s.ownedBusiness);
                long curYield = EconomyData.GetBusinessBaseSemesterRevenue(s.ownedBusiness);
                CreateActivityCard($"Current Enterprise: {curName} (Yield: ~${curYield:N0}/sem)",
                    $"You already own {curName}. To acquire a higher-tier empire, you can divest or purchase a more prestigious conglomerate.",
                    new Color(0.40f, 0.55f, 0.40f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("COMMERCE",
                            $"<b>[ENTERPRISE STATUS]</b> You currently operate <b>{curName}</b> distributing ~${curYield:N0} dividends each semester.");
                    });

                if (s.ownedBusiness < CommercialBusiness.DefenseMunitionsConglomerate)
                {
                    CreateBusinessCard(s, CommercialBusiness.DefenseMunitionsConglomerate, new Color(0.75f, 0.25f, 0.20f));
                }
                if (s.ownedBusiness < CommercialBusiness.ImperialShippingCartel)
                {
                    CreateBusinessCard(s, CommercialBusiness.ImperialShippingCartel, new Color(0.20f, 0.55f, 0.80f));
                }
                if (s.ownedBusiness < CommercialBusiness.CentralBankingHouseAndMint)
                {
                    CreateBusinessCard(s, CommercialBusiness.CentralBankingHouseAndMint, new Color(0.90f, 0.75f, 0.15f));
                }
            }
        }

        private void CreateBusinessCard(SoldierProfile s, CommercialBusiness b, Color cardColor)
        {
            long cost = EconomyData.GetBusinessPurchaseCost(b);
            string name = EconomyData.GetBusinessDisplayName(b);
            string desc = EconomyData.GetBusinessDescription(b);

            CreateActivityCard($"Acquire {name} (${cost:N0})",
                desc,
                cardColor, () =>
                {
                    EconomyEngine.PurchaseBusiness(s, b);
                    RefreshUI();
                });
        }

        private void CycleMOSQualification(SoldierProfile s)
        {
            if (s.militaryBranch == MilitaryBranch.AirForce)
            {
                s.militaryMOS = (s.militaryMOS == MilitaryMOS.FighterPilotAce) 
                    ? MilitaryMOS.PararescuePJ 
                    : MilitaryMOS.FighterPilotAce;
            }
            else
            {
                switch (s.militaryMOS)
                {
                    case MilitaryMOS.InfantryRifleman11B:
                        s.militaryMOS = MilitaryMOS.CombatMedic68W;
                        break;
                    case MilitaryMOS.CombatMedic68W:
                        s.militaryMOS = MilitaryMOS.ScoutSniper19D;
                        break;
                    case MilitaryMOS.ScoutSniper19D:
                        s.militaryMOS = MilitaryMOS.CombatEngineer12B;
                        break;
                    case MilitaryMOS.CombatEngineer12B:
                    default:
                        s.militaryMOS = MilitaryMOS.InfantryRifleman11B;
                        break;
                }
            }

            GameEvents.OnFeedEventLogged?.Invoke("DUTY",
                $"<color=#00FFAA><b>[MOS QUALIFICATION CERTIFIED]</b></color> You completed cross-training and certified as <b>{s.militaryMOS}</b>!");
            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
        }

        private void BuildDynastyAndLegacyActivities(SoldierProfile s)
        {
            // ==================== ROMANCE & COURTSHIP ====================
            if (!s.isMarried)
            {
                if (string.IsNullOrEmpty(s.romanticPartnerName))
                {
                    CreateActivityCard("Find a Romantic Partner (Blind Date / PX Gala)",
                        "Seek out companionship at the regimental canteen or through family introductions. Find someone to share peacetime letters with!",
                        new Color(0.85f, 0.40f, 0.65f), () =>
                        {
                            var partner = RomanceAndCourtshipEngine.GenerateProspectivePartner(s);
                            if (partner != null)
                            {
                                s.romanticPartnerName = partner.name;
                                s.courtshipStage = CourtshipStage.CasualSweetheart;
                                GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                                    $"<color=#FF88FF><b>[MET SOMEONE SPECIAL]</b></color> You met <b>{partner.name}</b> ({partner.occupation})! " +
                                    $"They are <i>{partner.personalityTrait}</i> and smiled warmly when talking with you.");
                                RefreshUI();
                            }
                        });
                }
                else
                {
                    string partnerName = s.romanticPartnerName;
                    CreateActivityCard($"Take {partnerName} on a Date (Diner $30)",
                        $"Enjoy warm pie and milkshakes at the town diner with {partnerName} (+14% Affection, +10 Morale).",
                        new Color(0.75f, 0.35f, 0.60f), () =>
                        {
                            var tempPartner = new RomanticPartner(partnerName, "Sweetheart", Mathf.Max(18, (int)s.age), 60f, "Devoted");
                            RomanceAndCourtshipEngine.TakeOnDate(s, tempPartner, "Diner");
                            RefreshUI();
                        });

                    CreateActivityCard($"Escort {partnerName} to Regimental Officers' Gala ($200)",
                        $"Dress up in your military finest and dance with {partnerName} at the annual military ball (+32% Affection, +22 Morale).",
                        new Color(0.85f, 0.45f, 0.70f), () =>
                        {
                            var tempPartner = new RomanticPartner(partnerName, "Sweetheart", Mathf.Max(18, (int)s.age), 60f, "Devoted");
                            RomanceAndCourtshipEngine.TakeOnDate(s, tempPartner, "MilitaryBall");
                            RefreshUI();
                        });

                    if (s.courtshipStage != CourtshipStage.Engaged)
                    {
                        CreateActivityCard($"Pop the Question: Propose to {partnerName} (Diamond Ring $2,000)",
                            $"Purchase an engagement ring and ask {partnerName} to be your lifetime military spouse!",
                            new Color(0.90f, 0.75f, 0.20f), () =>
                            {
                                var tempPartner = new RomanticPartner(partnerName, "Sweetheart", Mathf.Max(18, (int)s.age), 75f, "Devoted");
                                if (RomanceAndCourtshipEngine.ProposeMarriage(s, tempPartner, "Imperial Diamond Solitaire", 2000L))
                                {
                                    s.courtshipStage = CourtshipStage.Engaged;
                                    s.engagementRing = "Imperial Diamond Solitaire";
                                }
                                RefreshUI();
                            });

                        CreateActivityCard($"Pop the Question: Propose to {partnerName} (Silver Band $300)",
                            $"Propose with a simple, honest sterling silver band.",
                            new Color(0.70f, 0.65f, 0.50f), () =>
                            {
                                var tempPartner = new RomanticPartner(partnerName, "Sweetheart", Mathf.Max(18, (int)s.age), 60f, "Devoted");
                                if (RomanceAndCourtshipEngine.ProposeMarriage(s, tempPartner, "Engraved Silver Band", 300L))
                                {
                                    s.courtshipStage = CourtshipStage.Engaged;
                                    s.engagementRing = "Engraved Silver Band";
                                }
                                RefreshUI();
                            });
                    }
                    else
                    {
                        CreateActivityCard($"Arch-of-Sabers Military Cathedral Wedding ($1,000)",
                            $"Hold a glorious formal wedding! Comrades raise crossed ceremonial sabers overhead as you marry {partnerName}!",
                            new Color(0.20f, 0.75f, 0.50f), () =>
                            {
                                var tempPartner = new RomanticPartner(partnerName, "Sweetheart", Mathf.Max(18, (int)s.age), 100f, "Devoted");
                                RomanceAndCourtshipEngine.HoldWeddingCeremony(s, tempPartner, "CathedralMilitary");
                                RefreshUI();
                            });

                        CreateActivityCard($"Courthouse Registry Marriage ($150)",
                            $"A quiet, intimate ceremony at the municipal registry office.",
                            new Color(0.40f, 0.60f, 0.50f), () =>
                            {
                                var tempPartner = new RomanticPartner(partnerName, "Sweetheart", Mathf.Max(18, (int)s.age), 100f, "Devoted");
                                RomanceAndCourtshipEngine.HoldWeddingCeremony(s, tempPartner, "Courthouse");
                                RefreshUI();
                            });
                    }
                }
            }
            else
            {
                // Married Life
                CreateActivityCard($"Spouse Date Night: An Evening with {s.spouseName} ($100)",
                    $"Celebrate your loving marriage away from military duties (+15 Morale, -10 Stress).",
                    new Color(0.85f, 0.40f, 0.65f), () =>
                    {
                        if (s.cashBalance >= 100)
                        {
                            s.cashBalance -= 100;
                            s.happiness = Mathf.Min(100f, s.happiness + 15f);
                            s.combatStress = Mathf.Max(0f, s.combatStress - 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                                $"<color=#FF88FF>[MARRIED BLISS]</color> You and {s.spouseName} enjoyed a quiet, romantic dinner at home (+15 Morale, -10 Stress).");
                        }
                        RefreshUI();
                    });

                if (s.childHeir == null)
                {
                    CreateActivityCard("Family Planning: Try for a Baby",
                        $"Start your family and welcome a child heir into the world to carry on your legacy and medals!",
                        new Color(0.30f, 0.70f, 0.60f), () =>
                        {
                            RomanceAndCourtshipEngine.TryForBaby(s);
                            RefreshUI();
                        });
                }
            }

            // ==================== CHILDREN & DYNASTY ====================
            if (s.childHeir != null)
            {
                string childDesc = $"Child: <b>{s.childHeir.childName} {s.lastName}</b> ({s.childHeir.gender}, Age {s.childHeir.age:F1}). " +
                                   $"Status: {(s.childHeir.isEligibleForEnlistment ? "<color=#00FFAA>Eligible for Enlistment (Age 18+)</color>" : "<color=#FF8800>Growing Up</color>")}.";

                if (s.childHeir.isEligibleForEnlistment)
                {
                    CreateActivityCard($"Pass the Torch: Enlist as {s.childHeir.childName} (Gen {s.generationNumber + 1})",
                        $"{childDesc}\n<b>Continue your dynasty!</b> Enlist as your child on Day 1 with the <b>Military Legacy</b> origin, inheriting 50% family savings and your war medals!",
                        new Color(0.85f, 0.65f, 0.15f), () =>
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.ContinueAsChildHeir();
                            }
                            RefreshUI();
                        });
                }
                else
                {
                    CreateActivityCard($"Family: Raising {s.childHeir.childName} (Age {s.childHeir.age:F1})",
                        childDesc,
                        new Color(0.55f, 0.35f, 0.50f), () =>
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#00FFAA>[FAMILY TIME]</color> You spent leave teaching {s.childHeir.childName} discipline and sharing stories from the frontlines.");
                            RefreshUI();
                        });

                    string tutorStatus = s.isHeirTutoringEnrolled 
                        ? "<color=#00FFAA>[ACTIVE: +15 Intellect, Top Exam Preparation]</color>" 
                        : "<color=#FF8866>[NOT ENROLLED]</color>";

                    CreateActivityCard("Child Heir: Private Academy Tutors ($1,000/sem)",
                        $"Hire elite private proctors and martial tutors for {s.childHeir.childName}. Status: {tutorStatus}. Guarantees high scores on their Age 18 National Military Exam!",
                        new Color(0.20f, 0.55f, 0.70f), () =>
                        {
                            EconomyEngine.SetHeirPrivateTutor(s, !s.isHeirTutoringEnrolled);
                            RefreshUI();
                        });
                }
            }

            // ==================== VETERAN MEMOIRS & PUBLIC OFFICE ====================
            if (s.isRetired || s.totalSemestersLived >= 10)
            {
                if (s.publishedMemoirsCount == 0)
                {
                    CreateActivityCard("Write Wartime Memoir: 'Blood in the Arden' ($200)",
                        "Publish your frontline combat experiences in a military autobiography. Earn semester book royalties based on your medals and tours!",
                        new Color(0.85f, 0.60f, 0.20f), () =>
                        {
                            if (s.cashBalance >= 200)
                            {
                                s.cashBalance -= 200;
                                s.publishedMemoirsCount = 1;
                                s.publishedMemoirTitle = "Blood in the Arden: A Soldier's Truth";
                                s.warMemoirRoyaltiesPerSemester = 800L + (s.GetWoundStarsCount() * 300L) + (s.totalSemestersLived * 40L);
                                s.AddTrait("Published War Author");
                                GameEvents.OnFeedEventLogged?.Invoke("MEMOIR",
                                    $"<color=#FFD700><b>[MEMOIR PUBLISHED • BESTSELLER]</b></color> Your book, <b>'{s.publishedMemoirTitle}'</b>, hit the military bestseller lists! " +
                                    $"Awarded <b>${s.warMemoirRoyaltiesPerSemester:N0}/semester</b> in lifetime publication royalties!");
                                RefreshUI();
                            }
                        });
                }
                else
                {
                    CreateActivityCard($"Published Memoir: '{s.publishedMemoirTitle}'",
                        $"Your autobiography continues to sell nationwide. Generates <b>${s.warMemoirRoyaltiesPerSemester:N0}/semester</b> in royalties.",
                        new Color(0.60f, 0.50f, 0.25f), () =>
                        {
                            RefreshUI();
                        });
                }

                CreateActivityCard("Campaign for Civic Office: Run for City Council",
                    "Leverage your military honors, medals, and veteran respect to campaign for municipal government office ($300 campaign fee).",
                    new Color(0.25f, 0.55f, 0.75f), () =>
                    {
                        if (s.cashBalance >= 300)
                        {
                            s.cashBalance -= 300;
                            s.politicalInfluencePoints += 20;
                            s.civicOfficeHeld = "City Council Member";
                            s.AddTrait("Elected Official");
                            GameEvents.OnFeedEventLogged?.Invoke("POLITICS",
                                $"<color=#00FFAA><b>[ELECTION VICTORY]</b></color> Voters overwhelmingly elected you to the <b>City Council</b> on a platform of veteran honor and civic duty (+20 Influence)!");
                            RefreshUI();
                        }
                    });
            }
        }

        private void BuildCrimeAndUnderworldActivities(SoldierProfile s)
        {
            if (s.isImprisoned || s.militaryBrigSemestersRemaining > 0)
            {
                CreateActivityCard("Serving Prison / Stockade Sentence",
                    $"You are locked in the stockade/penitentiary! <b>{Mathf.Max(s.prisonSemestersRemaining, s.militaryBrigSemestersRemaining)} semester(s) remaining</b>. Reflect on your crimes or endure hard labor.",
                    new Color(0.50f, 0.15f, 0.15f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("PRISON", "You marked off another month on the cold concrete prison wall.");
                        RefreshUI();
                    });
                return;
            }

            // Heat status banner
            string heatBar = CrimeEngine.GetHeatBarString(s.criminalHeat);
            CreateActivityCard($"Underworld Status: Heat {s.criminalHeat:F0}%",
                $"Current Criminal Heat: <b>{heatBar}</b>. Crimes Committed: <b>{s.totalCrimesCommitted}</b>. Heat over 50% triggers dawn MP / Police SWAT raids!",
                new Color(0.65f, 0.10f, 0.10f), () =>
                {
                    RefreshUI();
                });

            // Military Crimes
            if (!s.isCivilian && !s.isRetired)
            {
                CreateActivityCard("Illicit Hustle: Siphon Fuel & Field Rations",
                    "Siphon military depot diesel into civilian black-market fuel drums. <b>Payout: +$800 Cash</b> | Risk: Low (Heat +10%).",
                    new Color(0.70f, 0.20f, 0.15f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.SiphonFuelAndRations);
                        RefreshUI();
                    });

                CreateActivityCard("Illicit Hustle: Armory Weapons & Optics Theft",
                    "Smuggle night-vision scopes and service carbines out of the regimental armory. <b>Payout: +$3,200 Cash</b> | Risk: High (Heat +25%).",
                    new Color(0.80f, 0.20f, 0.10f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.ArmoryTheft);
                        RefreshUI();
                    });

                CreateActivityCard("Illicit Hustle: Barracks Craps & Dice Ring",
                    "Run an underground high-stakes dice game in the barracks basement. <b>Payout: +$1,500 Cash</b> | Risk: Medium (Heat +15%).",
                    new Color(0.75f, 0.30f, 0.15f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.BarracksGamblingRing);
                        RefreshUI();
                    });

                if (s.isInCombatDeployment)
                {
                    CreateActivityCard("Illicit Hustle: Frontline War Plunder & Looting",
                        "Loot gold watches and contraband from fallen enemy trenches against military orders. <b>Payout: +$5,000 Cash</b> | Risk: High (Heat +30%).",
                        new Color(0.85f, 0.15f, 0.15f), () =>
                        {
                            CrimeEngine.CommitCrime(s, CrimeType.FrontlineWarPlunder);
                            RefreshUI();
                        });
                }

                if (s.isOfficer)
                {
                    CreateActivityCard("Illicit Hustle: Defense Procurement Bribery",
                        "Accept secret kickbacks from corporate defense contractors to award artillery contracts. <b>Payout: +$35,000 Cash</b> | Risk: Extreme (Heat +45%).",
                        new Color(0.90f, 0.10f, 0.10f), () =>
                        {
                            CrimeEngine.CommitCrime(s, CrimeType.DefenseProcurementBribery);
                            RefreshUI();
                        });
                }
            }
            // Civilian Crimes
            else if (s.isCivilian)
            {
                CreateActivityCard("Illicit Hustle: Underground Pit Fight Club",
                    "Brawl in illegal warehouse bare-knuckle pits for dirty purse money. <b>Payout: +$1,200 Cash</b> | Cost: -20 Health, Low Heat (+15%).",
                    new Color(0.70f, 0.20f, 0.15f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.UndergroundFightClub);
                        RefreshUI();
                    });

                CreateActivityCard("Illicit Hustle: Contraband Courier",
                    "Smuggle black-market stimulants and stolen crystals through municipal police checkpoints. <b>Payout: +$3,500 Cash</b> | Risk: Medium (Heat +20%).",
                    new Color(0.75f, 0.25f, 0.15f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.ContrabandCourier);
                        RefreshUI();
                    });

                CreateActivityCard("Illicit Hustle: Syndicate Debt Enforcer",
                    "Collect overdue street loans for the local crime syndicate with intimidation and muscle. <b>Payout: +$2,000 Cash</b> | Risk: Medium (Heat +18%).",
                    new Color(0.80f, 0.25f, 0.15f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.SyndicateDebtEnforcer);
                        RefreshUI();
                    });

                CreateActivityCard("Illicit Hustle: Customs Warehouse Heist",
                    "Execute an armed nighttime heist on the imperial commercial customs warehouse. <b>Payout: +$30,000 Cash</b> | Risk: Extreme (Heat +40%).",
                    new Color(0.90f, 0.15f, 0.15f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.WarehouseHeist);
                        RefreshUI();
                    });

                if (s.careerTrack == CareerTrack.CivilianProfessional)
                {
                    CreateActivityCard("Illicit Hustle: Corporate Embezzlement",
                        "Siphon company funds through dummy corporate shell accounts. <b>Payout: +$18,000 Cash</b> | Risk: High (Heat +35%).",
                        new Color(0.85f, 0.20f, 0.10f), () =>
                        {
                            CrimeEngine.CommitCrime(s, CrimeType.CorporateEmbezzlement);
                            RefreshUI();
                        });
                }
            }
            // Veteran Crimes
            else if (s.isRetired)
            {
                CreateActivityCard("Illicit Hustle: Military Surplus Gunrunning",
                    "Broker black-market export of decommissioned armored vehicles and assault rifles. <b>Payout: +$75,000 Cash</b> | Risk: Extreme (Heat +45%).",
                    new Color(0.85f, 0.15f, 0.10f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.SurplusGunrunning);
                        RefreshUI();
                    });

                CreateActivityCard("Illicit Hustle: Shadow PMC Wetwork Contract",
                    "Execute an off-the-books assassination hit against a rival oligarch's security retinue. <b>Payout: +$35,000 Cash</b> | Risk: High (Heat +40%).",
                    new Color(0.80f, 0.20f, 0.15f), () =>
                    {
                        CrimeEngine.CommitCrime(s, CrimeType.ShadowPMCWetwork);
                        RefreshUI();
                    });

                if (s.veteranCareer == VeteranCareer.CityMayor || s.veteranCareer == VeteranCareer.RegionalGovernor || s.veteranCareer == VeteranCareer.MinisterOfDefense || s.politicalInfluencePoints >= 20)
                {
                    CreateActivityCard("Illicit Hustle: City Hall Municipal Graft & Bribes",
                        "Siphon public infrastructure bond funds into private family trusts via political power. <b>Payout: +$120,000 Cash</b> | Risk: Extreme (Heat +50%).",
                        new Color(0.95f, 0.10f, 0.10f), () =>
                        {
                            CrimeEngine.CommitCrime(s, CrimeType.CityHallGraft);
                            RefreshUI();
                        });
                }
            }

            // Bribe / Lay Low Option
            if (s.criminalHeat >= 15f)
            {
                CreateActivityCard("Lay Low & Bribe Dock Inspectors ($500)",
                    "Slip $500 to port authorities and provost investigators to burn records and clear your name (-30% Criminal Heat).",
                    new Color(0.35f, 0.55f, 0.35f), () =>
                    {
                        if (s.cashBalance < 500)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Bribing inspectors costs $500.");
                            return;
                        }
                        s.cashBalance -= 500;
                        s.criminalHeat = Mathf.Max(0f, s.criminalHeat - 30f);
                        GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD",
                            $"<color=#00FFAA>[HEAT CLEARED]</color> You slipped bribes to internal affairs officers. Criminal Heat reduced to <b>{s.criminalHeat:F0}%</b>.");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
        }

        // ==================== UI CARD & TILE CREATION HELPERS ====================

        private void CreateCategoryTile(string icon, string title, string description, int actionCount, UnityEngine.Events.UnityAction onClick)
        {
            var tileObj = new GameObject($"Category_{title}");
            tileObj.transform.SetParent(activitiesContainer, false);

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);

            var img = tileObj.AddComponent<Image>();
            img.color = p.subCardBackground;

            var btn = tileObj.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.highlightedColor = Color.Lerp(p.subCardBackground, Color.white, 0.15f);
            cb.pressedColor = Color.Lerp(p.subCardBackground, Color.black, 0.20f);
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            var vLayout = tileObj.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(18, 18, 14, 14);
            vLayout.spacing = 6;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = tileObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var le = tileObj.AddComponent<LayoutElement>();
            le.minHeight = 84;

            // Content Text: Header row with title + action count badge, followed by description
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(tileObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.fontSize = 24;
            tmp.color = p.textPrimary;
            string descHex = ColorUtility.ToHtmlStringRGB(p.textSecondary);
            string goldHex = ColorUtility.ToHtmlStringRGB(p.accentGold);
            string actionBadge = $"<color=#{goldHex}><b>[ {actionCount} OPTIONS › ]</b></color>";

            tmp.text = $"<size=26>{icon}</size>  <b>{title}</b>   {actionBadge}\n<size=19><color=#{descHex}>{description}</color></size>";
        }

        private void CreateBackToDirectoryButton()
        {
            var backBtnObj = new GameObject("Btn_BackToDirectory");
            backBtnObj.transform.SetParent(activitiesContainer, false);

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);

            var img = backBtnObj.AddComponent<Image>();
            img.color = p.btnNormalBg;

            var btn = backBtnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.highlightedColor = Color.Lerp(p.btnNormalBg, Color.white, 0.15f);
            cb.pressedColor = Color.Lerp(p.btnNormalBg, Color.black, 0.20f);
            btn.colors = cb;
            btn.onClick.AddListener(() => SetCategory(ActivityCategory.Directory));

            var le = backBtnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 52;
            le.minHeight = 48;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(backBtnObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = "<b>⬅  BACK TO ALL DEPARTMENTS</b>";
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = p.accentGold;

            var rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void CreateActivityCard(string name, string desc, Color btnColor, UnityEngine.Events.UnityAction onClick)
        {
            var cardObj = new GameObject($"Activity_{name}");
            cardObj.transform.SetParent(activitiesContainer, false);

            var img = cardObj.AddComponent<Image>();
            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            img.color = p.subCardBackground;

            var vLayout = cardObj.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(18, 18, 14, 14);
            vLayout.spacing = 8;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = cardObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title & Description
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(cardObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 24;
            tmp.color = p.textPrimary;
            string descHex = ColorUtility.ToHtmlStringRGB(p.textSecondary);
            tmp.text = $"<b>{name}</b>\n<size=19><color=#{descHex}>{desc}</color></size>";

            // Action Button
            var btnObj = new GameObject("Button");
            btnObj.transform.SetParent(cardObj.transform, false);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = btnColor;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 52;
            le.minHeight = 48;

            var btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTmp.raycastTarget = false;
            btnTmp.text = "EXECUTE ACTION";
            btnTmp.fontSize = 22;
            btnTmp.fontStyle = FontStyles.Bold;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;

            var btnRt = btnTextObj.GetComponent<RectTransform>();
            btnRt.anchorMin = Vector2.zero;
            btnRt.anchorMax = Vector2.one;
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;
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
            if (subtitleText != null) subtitleText.color = p.textSecondary;

            if (closeButton != null)
            {
                var img = closeButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }
        }
    }
}