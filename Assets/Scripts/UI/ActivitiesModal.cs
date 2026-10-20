using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
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

        [Header("Content Container")]
        public Transform activitiesContainer;

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

            if (titleText != null)
            {
                if (s.isYouthPhase) titleText.text = "YOUTH ACTIVITIES & TRAINING";
                else if (s.isCivilian) titleText.text = "CIVILIAN LIFE & CAREER ACTIVITIES";
                else titleText.text = "MILITARY OPERATIONS & BASE ACTIVITIES";
            }

            if (subtitleText != null)
            {
                if (s.isYouthPhase)
                {
                    subtitleText.text = $"Foundational Tempering: <b>{s.foundationalTempering:F0}/100</b>   |   Intellect: <b>{s.intellect:F0}%</b>   |   Combat: <b>{s.combatSkill:F0}%</b>";
                }
                else if (s.isCivilian)
                {
                    string debtStr = s.collegeTuitionDebt > 0 ? $"   |   Debt: <color=#FF6666>${s.collegeTuitionDebt:N0}</color>" : "";
                    subtitleText.text = $"Civilian Wage: <b>${s.civilianSalaryPerMonth:N0}/mo</b>   |   Intellect: <b>{s.intellect:F0}%</b>   |   Fitness: <b>{s.fitness:F0}%</b>{debtStr}";
                }
                else
                {
                    subtitleText.text = $"Fitness: <b>{s.fitness:F0}%</b>   |   Combat: <b>{s.combatSkill:F0}%</b>   |   Meridians: <b>{(100f - s.meridianDamage):F0}% Intact</b>";
                }
            }

            if (activitiesContainer == null) return;

            // Clear old items
            for (int i = activitiesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(activitiesContainer.GetChild(i).gameObject);
            }

            if (s.isYouthPhase)
            {
                BuildYouthActivities(s);
            }
            else if (s.isCivilian)
            {
                BuildCivilianActivities(s);
            }
            else
            {
                BuildAdultActivities(s);
            }

            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private void BuildYouthActivities(SoldierProfile s)
        {
            // 1. School & Academic Studies
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

            // 2. Body Tempering
            string stageName = PowerTierKnowledgeSystem.GetPreTierStageName(s.preTierStage);
            CreateActivityCard($"Martial Body Tempering ({stageName})",
                "Endure harsh physical conditioning to purify bones, meridians, and marrow foundation.",
                new Color(0.70f, 0.35f, 0.15f), () =>
                {
                    s.foundationalTempering = Mathf.Min(100f, s.foundationalTempering + 6f);
                    s.fitness = Mathf.Min(100f, s.fitness + 3f);
                    s.health = Mathf.Max(20f, s.health - 4f);

                    if (s.foundationalTempering >= 25f && s.preTierStage < 2) s.preTierStage = 2;
                    if (s.foundationalTempering >= 50f && s.preTierStage < 3) s.preTierStage = 3;
                    if (s.foundationalTempering >= 80f && s.preTierStage < 4) s.preTierStage = 4;

                    GameEvents.OnFeedEventLogged?.Invoke("MARTIAL",
                        $"<color=#00FFAA>[BODY TEMPERING]</color> You completed grueling muscle and bone refining drills! (+6 Tempering, +3 Fitness).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 3. Firing Range
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

            // 4. Clan Martial Library
            CreateActivityCard("Clan Martial Archives",
                "Peruse ancient imperial strategy manuals and classified breathing scriptures.",
                new Color(0.50f, 0.25f, 0.70f), () =>
                {
                    s.intellect = Mathf.Min(100f, s.intellect + 3f);
                    s.knowsAboutCultivation = true;
                    GameEvents.OnFeedEventLogged?.Invoke("STUDY",
                        "<color=#FFD700>[SECRET ARCHIVES]</color> You unlocked ancient scrolls detailing Qi pathways and imperial cultivation secrets! (+3 Intellect).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 5. Apothecary
            CreateActivityCard("Imperial Herbal Apothecary",
                "Purchase cleansing herbal teas ($30) to soothe meridian strain and restore vitality.",
                new Color(0.15f, 0.55f, 0.35f), () =>
                {
                    if (s.cashBalance < 30)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("APOTHECARY", "<color=#FF4444>[POVERTY]</color> You lack $30 for herbal tea.");
                        return;
                    }
                    s.cashBalance -= 30;
                    s.health = Mathf.Min(100f, s.health + 15f);
                    s.meridianDamage = Mathf.Max(0f, s.meridianDamage - 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("APOTHECARY",
                        "<color=#00FFAA>[HERBAL CLEANSING]</color> The herbal elixir purified your meridians (-10% Meridian Damage, +15 Health).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 6. Odd Jobs
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

            // 7. Mock Exam (Ages 16+)
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

        private void BuildCivilianActivities(SoldierProfile s)
        {
            // 1. Work Shift / Overtime
            long shiftEarnings = (s.careerTrack == CareerTrack.CivilianProfessional) ? 120L : 60L;
            CreateActivityCard("Work Overtime Shift",
                $"Put in additional hours at your civilian workplace to earn immediate income (+${shiftEarnings}).",
                new Color(0.20f, 0.55f, 0.35f), () =>
                {
                    s.cashBalance += shiftEarnings;
                    s.health = Mathf.Max(15f, s.health - 4f);
                    s.happiness = Mathf.Max(10f, s.happiness - 3f);
                    GameEvents.OnFeedEventLogged?.Invoke("WORK",
                        $"<color=#00FFAA>[OVERTIME WORKED]</color> You completed an extra evening shift and received <b>+${shiftEarnings}</b>.");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 2. Career Advancement & Promotion Search
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

            // 3. Imperial College Studies (If attending college)
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

            // 4. Pay Student Loan Debt (If in debt)
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

            // 5. Enroll in Imperial College (If not yet in college and not graduated)
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

            // 6. Civilian Fitness & Wellness Gym
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

            // 7. City Hospital & Clinic
            CreateActivityCard("Civilian Medical Clinic",
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
                    s.meridianDamage = Mathf.Max(0f, s.meridianDamage - 15f);
                    GameEvents.OnFeedEventLogged?.Invoke("CLINIC",
                        "<color=#00FFAA>[HEALTH RESTORED]</color> The municipal doctors treated your ailments (+25 Health, -15% Strain).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 8. Public Park Qi Meditation
            CreateActivityCard("Public Park Qi Meditation",
                "Sit beneath ancient willows to absorb energy currents and temper inner meridians.",
                new Color(0.55f, 0.25f, 0.75f), () =>
                {
                    if (s.tierProgressPercent >= 95f)
                    {
                        PowerCultivationEngine.AttemptBreakthrough(s, null);
                    }
                    else
                    {
                        PowerCultivationEngine.ProcessCultivationTraining(s);
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 9. City Lounge & Tavern
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

            // 10. Military Conscription / Volunteer Enlistment
            CreateActivityCard("Enlist as Volunteer Military Recruit",
                "Leave civilian life behind and report to the 6-month recruit training depot (Unpaid Basic Training).",
                new Color(0.70f, 0.35f, 0.15f), () =>
                {
                    CivilAndMilitaryCareerEngine.EnlistAsRecruit(s, GameManager.Instance);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }

        private void BuildAdultActivities(SoldierProfile s)
        {
            // 1. PT & Obstacle Course
            CreateActivityCard("PT & Tactical Obstacle Course",
                "Complete the grueling military physical fitness test and confidence course.",
                new Color(0.18f, 0.65f, 0.35f), () =>
                {
                    s.fitness = Mathf.Min(100f, s.fitness + 4f);
                    s.promotionPoints += 5;
                    s.health = Mathf.Max(20f, s.health - 3f);
                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                        "<color=#00FFAA>[FITNESS DRILL]</color> You aced the physical fitness course. (+4 Fitness, +5 Promotion Points).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 2. Live-Fire Marksmanship
            CreateActivityCard("Live-Fire Firing Range",
                "Qualify on long-range heavy rifles, plasma suppressors, and CQB marksmanship.",
                new Color(0.20f, 0.50f, 0.80f), () =>
                {
                    s.combatSkill = Mathf.Min(100f, s.combatSkill + 3.5f);
                    s.promotionPoints += 4;
                    GameEvents.OnFeedEventLogged?.Invoke("MARKSMANSHIP",
                        "<color=#00D4FF>[EXPERT QUALIFICATION]</color> You earned Expert Marksman badge points! (+3.5 Combat Skill, +4 Board Points).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 3. Qi Cultivation Chamber
            CreateActivityCard("Qi Meditation Chamber",
                "Channel ambient atmospheric energy. Progress Qi and attempt realm breakthroughs.",
                new Color(0.55f, 0.25f, 0.75f), () =>
                {
                    if (s.tierProgressPercent >= 95f)
                    {
                        PowerCultivationEngine.AttemptBreakthrough(s, null);
                    }
                    else
                    {
                        PowerCultivationEngine.ProcessCultivationTraining(s);
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 4. Base Medical Hospital
            CreateActivityCard("Base Medical Bay & Recovery",
                "Receive intensive medical therapy ($150) to repair torn meridians and restore health.",
                new Color(0.85f, 0.25f, 0.25f), () =>
                {
                    if (s.cashBalance < 150)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "<color=#FF4444>[INSUFFICIENT FUNDS]</color> Medical bay therapy costs $150.");
                        return;
                    }
                    s.cashBalance -= 150;
                    s.meridianDamage = Mathf.Max(0f, s.meridianDamage - 25f);
                    s.health = Mathf.Min(100f, s.health + 30f);
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL",
                        "<color=#00FFAA>[MEDICAL TREATMENT]</color> Imperial military doctors restored your cellular meridians (-25% Meridian Damage, +30 Health).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 5. Promotion Board Review
            CreateActivityCard("Promotion Board Submission",
                "Review service jacket and submit official petition for next military paygrade promotion.",
                new Color(0.85f, 0.65f, 0.15f), () =>
                {
                    PromotionBoardEngine.EvaluateBoardPromotion(s, null);
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 6. Frontline Deployment / Patrol
            string tourLabel = s.isInCombatDeployment ? "Request Frontline Extraction (Rest)" : "Volunteer for Frontline Combat Tour";
            CreateActivityCard(tourLabel,
                s.isInCombatDeployment 
                    ? "Rotate back to garrison base for rest and refit. Ceases combat hazard pay."
                    : "Deploy to active combat sectors. High mortality risk, earns +$600 hazard pay and Combat Merit Points (CMP).",
                new Color(0.75f, 0.20f, 0.20f), () =>
                {
                    s.isInCombatDeployment = !s.isInCombatDeployment;
                    if (s.isInCombatDeployment)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("DEPLOYMENT",
                            "<color=#FF4444><b>[DEPLOYED TO FRONT SECTOR]</b></color> You volunteered for active combat deployment against the enemy coalition! Hazard pay activated.");
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("DEPLOYMENT",
                            "<color=#00FFAA><b>[GARRISON REASSIGNMENT]</b></color> You concluded your frontline combat tour and returned to garrison refit.");
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 7. Barracks Mess Hall
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

            // 8. Test Talent (Rank E-4+)
            if (!s.isPersonalTalentRevealed)
            {
                CreateActivityCard("Resonance Testing Chamber (Rank E-4)",
                    "Submit to the army resonance stone to unlock and reveal your innate Power Talent score.",
                    new Color(0.20f, 0.40f, 0.80f), () =>
                    {
                        TalentTestingApparatus.PerformSelfApparatusTest(s);
                        GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        RefreshUI();
                    });
            }
        }

        private void CreateActivityCard(string name, string desc, Color btnColor, UnityEngine.Events.UnityAction onClick)
        {
            var cardObj = new GameObject($"Activity_{name}");
            cardObj.transform.SetParent(activitiesContainer, false);

            var img = cardObj.AddComponent<Image>();
            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            img.color = (mode == ThemeMode.Dark) ? new Color(0.13f, 0.16f, 0.21f) : new Color(0.92f, 0.94f, 0.97f);

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
            tmp.color = (mode == ThemeMode.Dark) ? Color.white : new Color(0.08f, 0.12f, 0.18f);
            tmp.text = $"<b>{name}</b>\n<size=19><color={(mode == ThemeMode.Dark ? "#94A3B8" : "#475569")}>{desc}</color></size>";

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
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (titleText != null) titleText.color = p.textPrimary;
            if (subtitleText != null) subtitleText.color = p.textSecondary;
        }
    }
}
