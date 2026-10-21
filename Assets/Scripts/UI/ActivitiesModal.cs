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

            // 2. Body Conditioning
            string stageName = PowerTierKnowledgeSystem.GetPreTierStageName(s.preTierStage);
            CreateActivityCard($"Sub-Dermal Conditioning ({stageName})",
                "Endure high-intensity physical conditioning to prepare bone density and neural pathways for super-soldier augmentations.",
                new Color(0.70f, 0.35f, 0.15f), () =>
                {
                    s.foundationalTempering = Mathf.Min(100f, s.foundationalTempering + 6f);
                    s.fitness = Mathf.Min(100f, s.fitness + 3f);
                    s.health = Mathf.Max(20f, s.health - 4f);

                    if (s.foundationalTempering >= 25f && s.preTierStage < 2) s.preTierStage = 2;
                    if (s.foundationalTempering >= 50f && s.preTierStage < 3) s.preTierStage = 3;
                    if (s.foundationalTempering >= 80f && s.preTierStage < 4) s.preTierStage = 4;

                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                        $"<color=#00FFAA>[SUB-DERMAL CONDITIONING]</color> You completed grueling muscular and biomechanical drills! (+6 Framework, +3 Fitness).");
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

            // 4. Classified Archives
            CreateActivityCard("Classified Military Archives",
                "Peruse classified doctrine, mechanized warfare manuals, and super-soldier gene blueprints.",
                new Color(0.50f, 0.25f, 0.70f), () =>
                {
                    s.intellect = Mathf.Min(100f, s.intellect + 3f);
                    s.knowsAboutCultivation = true;
                    GameEvents.OnFeedEventLogged?.Invoke("STUDY",
                        "<color=#FFD700>[CLASSIFIED ARCHIVES]</color> You unlocked classified Imperial files on Super-Soldier Augmentations and Neural Systems! (+3 Intellect).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            // 5. Bio-Regen Stimulants
            CreateActivityCard("Military Bio-Regen Dispensary",
                "Purchase cellular cleansing stims ($30) to purge neural strain and restore biological vitality.",
                new Color(0.15f, 0.55f, 0.35f), () =>
                {
                    if (s.cashBalance < 30)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "<color=#FF4444>[POVERTY]</color> You lack $30 for cleansing stims.");
                        return;
                    }
                    s.cashBalance -= 30;
                    s.health = Mathf.Min(100f, s.health + 15f);
                    s.meridianDamage = Mathf.Max(0f, s.meridianDamage - 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL",
                        "<color=#00FFAA>[CELLULAR CLEANSING]</color> The neural serum cleansed your implant receptors (-10% Neural Strain, +15 Health).");
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

            // 8. Bio-Rhythm Neural Synchronization
            CreateActivityCard("Bio-Rhythm Neural Sync",
                "Sit in a sensory recovery booth to harmonize neural bio-rhythms and reduce implant strain.",
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

            // 3. Super-Soldier Augmentation Pod
            CreateActivityCard("Augmentation Calibration Pod",
                "Enter the pressurized bio-chamber to infuse next-tier military serums and calibrate cyber-frames.",
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

            // 6. Extended Frontline Battlefield Deployment (> 6 Months)
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

            // 7. Officer Academy Admission (2-Year Program, $50,000 Tuition / 9% Loan)
            if (!s.isOfficer && s.careerTrack != CareerTrack.MilitaryWarAcademy)
            {
                string academyReqs = $"Requires <b>55% Discipline</b> (Current: {s.intellect:F0}%) and <b>55% Physical Readiness</b> (Current: {s.fitness:F0}%). Limited seats quota. Graduates commissioned directly as <b>2nd Lieutenant (O-1)</b>. Year 2 Top 10% cadets qualify for Special Forces selection!";

                if (s.cashBalance >= SoldierProfile.ACADEMY_TUITION_COST)
                {
                    CreateActivityCard("Apply for War Academy ($50,000 Cash Upfront)",
                        $"Pay $50,000 tuition upfront in cash. {academyReqs}",
                        new Color(0.20f, 0.60f, 0.40f), () =>
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.ApplyForOfficerAcademy(takeLoan: false);
                            }
                            RefreshUI();
                        });
                }

                CreateActivityCard($"Apply for War Academy (9% Loan, {s.academyLoanDeductionPercent * 100f:F0}% Pay Deduction)",
                    $"Finance the $50,000 tuition with a 9% annual interest Military Education Loan (monthly deduction from salary, minimum 10%). {academyReqs}",
                    new Color(0.20f, 0.50f, 0.85f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.ApplyForOfficerAcademy(takeLoan: true, s.academyLoanDeductionPercent);
                        }
                        RefreshUI();
                    });
            }

            // 8. Academy Loan Repayment Rate Adjustment
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

            // 9. Special Forces Qualification Pathways
            if (!s.isSpecialForces && !s.isSpecialForcesCandidate)
            {
                // Route A: Academy Cadet in Year 2 with Top 10% standing
                if (s.careerTrack == CareerTrack.MilitaryWarAcademy && s.academySemestersCompleted >= 2)
                {
                    if (s.isAcademyTop10Percent)
                    {
                        CreateActivityCard("Volunteer for Special Forces Qualification (Top 10% Cadet)",
                            $"Your stellar academic score of <b>{s.academyPerformanceScore:F1}%</b> qualifies you for the 1-year Special Forces Qualification Course! Earn <b>+$800/mo Elite Stipend</b> and high tactical experience, but face increased combat fatality risk.",
                            new Color(0.75f, 0.20f, 0.25f), () =>
                            {
                                if (GameManager.Instance != null)
                                {
                                    GameManager.Instance.JoinSpecialForcesFromAcademy();
                                }
                                RefreshUI();
                            });
                    }
                    else
                    {
                        CreateActivityCard("Special Forces Selection (Top 10% Required)",
                            $"Requires Year 2 Academy standing >= 90% (Top 10%). Current Standing: <b>{s.academyPerformanceScore:F1}%</b>. Train physical readiness and tactics to reach the top decile!",
                            new Color(0.50f, 0.50f, 0.50f), () =>
                            {
                                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                                    $"<color=#FF8800>[TOP 10% REQUIRED]</color> Special Forces recruitment is restricted to the top 10% of Year 2 cadets (Score >= 90%). Current score: <b>{s.academyPerformanceScore:F1}%</b>.");
                                RefreshUI();
                            });
                    }
                }

                // Route B: Enlisted Soldier Petitioning CO Recommendation
                if (!s.isOfficer && s.careerTrack == CareerTrack.MilitaryEnlisted)
                {
                    string sfCODesc = $"Petition your Commanding Officer for direct recommendation to the 1-Year Special Forces Qualification Course. " +
                                      $"Requires <b>CO Favor >= 70%</b> (Current: {s.coFavor:F0}%) and <b>Merit >= 60%</b> (Current: {s.meritRating:F0}%). " +
                                      $"Operators draw <b>+$800/mo Elite Stipend</b>, but face high-lethality black ops!";

                    CreateActivityCard("Petition CO for Special Forces Recommendation",
                        sfCODesc,
                        new Color(0.70f, 0.25f, 0.35f), () =>
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
                CreateActivityCard("Special Forces Candidate Training Status",
                    $"Currently undergoing the brutal 1-year Special Forces Qualification Course (Term <b>{s.specialForcesTrainingSemesters + 1} / 2</b>). Concentrating on elite mountain warfare, covert infiltration, and extreme survival.",
                    new Color(0.80f, 0.40f, 0.15f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                            $"<color=#FFD700>[SF CANDIDATE]</color> Qualification Term {s.specialForcesTrainingSemesters + 1}/2 in progress. Advance semester to complete training.");
                        RefreshUI();
                    });
            }
            else if (s.isSpecialForces)
            {
                CreateActivityCard("Special Forces Operator Service Record",
                    $"Active Elite Operator. Covert Operations Survived: <b>{s.specialForcesMissionsSurvived}</b>. Drawing <b>+$800/mo Special Operations Hazard Pay</b>. Rapid experience scaling active across all military ranks.",
                    new Color(0.15f, 0.55f, 0.35f), () =>
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("SPECIAL FORCES",
                            $"<color=#00FFAA>[SPECIAL FORCES OPERATOR]</color> Service File: {s.specialForcesMissionsSurvived} covert operations survived. Armed with Special Forces Elite Beret.");
                        RefreshUI();
                    });
            }

            // 10. Barracks Mess Hall
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

            // 11. PX Quartermaster & Field Gear Depot
            string helmetStatus = s.hasSteelHelmet ? " <color=#00FFAA>[EQUIPPED]</color>" : " ($250)";
            string vestStatus = s.hasBallisticVest ? " <color=#00FFAA>[EQUIPPED]</color>" : " ($500)";
            string sidearmStatus = s.hasServiceSidearm ? " <color=#00FFAA>[EQUIPPED]</color>" : " ($400)";
            string watchStatus = s.hasLuckyWatch ? " <color=#00FFAA>[EQUIPPED]</color>" : " ($350)";

            string pxSummary = $"Inventory: <b>{s.fieldMedkitsCount} Medkits</b>, <b>{s.cigarettesCount} Cigarette Packs</b>. " +
                               $"Protective Gear: Helmet{helmetStatus}, Flak Vest{vestStatus}, Sidearm{sidearmStatus}, Lucky Watch{watchStatus}.";

            if (!s.hasSteelHelmet)
            {
                CreateActivityCard("PX Store: Buy M1 Reinforced Steel Helmet ($250)",
                    "Deflects mortar shrapnel and sniper ricochets. Lowers frontline fatality risk by 5%.",
                    new Color(0.20f, 0.50f, 0.35f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_helmet", "M1 Reinforced Steel Helmet", "Military ballistic helmet.", GearCategory.Armor, 250L, 0.05f));
                        }
                        RefreshUI();
                    });
            }

            if (!s.hasBallisticVest)
            {
                CreateActivityCard("PX Store: Buy Ballistic Flak Vest ($500)",
                    "Heavy-duty protective body armor. Lowers frontline wound and fatality risk by 8%.",
                    new Color(0.20f, 0.50f, 0.35f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_vest", "Ballistic Flak Vest", "Protective torso armor.", GearCategory.Armor, 500L, 0.08f));
                        }
                        RefreshUI();
                    });
            }

            if (!s.hasServiceSidearm)
            {
                CreateActivityCard("PX Store: Buy Trench Service Sidearm ($400)",
                    "Reliable close-quarters pistol. Adds +10 CQB combat skill during ambushes and room clearing.",
                    new Color(0.55f, 0.35f, 0.20f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_sidearm", "Service Sidearm", "Close-quarters combat pistol.", GearCategory.Weapon, 400L, 0f, 10f));
                        }
                        RefreshUI();
                    });
            }

            if (!s.hasLuckyWatch)
            {
                CreateActivityCard("PX Store: Buy Engraved Pocket Watch ($350)",
                    "A keepsake lucky charm. Provides a permanent passive +4% boost to frontline survival rolls.",
                    new Color(0.65f, 0.50f, 0.15f), () =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_watch", "Engraved Pocket Watch", "Keepsake lucky charm.", GearCategory.Morale, 350L, 0.04f));
                        }
                        RefreshUI();
                    });
            }

            CreateActivityCard($"PX Store: Buy Field Trauma Kit ($150) [Have: {s.fieldMedkitsCount}]",
                "Emergency military trauma syrette. Automatically consumed to resuscitate yourself if you suffer a fatal wound!",
                new Color(0.18f, 0.55f, 0.50f), () =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_medkit", "Emergency Field Trauma Kit", "Life-saving syrette.", GearCategory.Medical, 150L, consumable: true, count: 1));
                    }
                    RefreshUI();
                });

            CreateActivityCard($"PX Store: Buy Cigarette Pack ($50) [Have: {s.cigarettesCount}]",
                "Wartime currency and camaraderie builder. Share with squadmates to boost loyalty and fireteam cohesion.",
                new Color(0.45f, 0.40f, 0.30f), () =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.BuyGearItem(new MilitaryGearItem("gear_cig", "PX Cigarette Pack", "Camaraderie builder.", GearCategory.Morale, 50L, consumable: true, count: 1));
                    }
                    RefreshUI();
                });

            // 12. Brothers in Arms (Persistent Fireteam Squadmates)
            s.InitializeSquadMatesIfEmpty();
            foreach (var mate in s.squadMates)
            {
                if (mate.isDead)
                {
                    CreateActivityCard($"Brothers in Arms: {mate.GetDisplayName()} (KIA)",
                        $"<color=#FF4444>Fallen Hero ({mate.roleTitle}). Died: {mate.deathReason}. Battles Survived: {mate.battlesSurvived}.</color>",
                        new Color(0.40f, 0.15f, 0.15f), () =>
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("MEMORIAL",
                                $"<color=#FF8866>[FALLEN COMRADE MEMORIAL]</color> You visited the battlefield grave of {mate.GetDisplayName()}. You swore never to forget your squad brother.");
                            RefreshUI();
                        });
                }
                else
                {
                    string mateDesc = $"{mate.roleTitle} | Relationship: <b>{mate.relationship:F0}%</b> | Battles Survived: <b>{mate.battlesSurvived}</b> | Personality: <i>{mate.personalityTrait}</i>.";
                    CreateActivityCard($"Fireteam: {mate.GetDisplayName()} (Share Smoke & Bond)",
                        $"{mateDesc}\nSpend 1 Cigarette pack to share stories and raise fireteam loyalty (+15 Rel).",
                        new Color(0.20f, 0.40f, 0.60f), () =>
                        {
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.InteractWithSquadMate(mate, "ShareCigarettes");
                            }
                            RefreshUI();
                        });

                    CreateActivityCard($"Fireteam: {mate.GetDisplayName()} (Barracks Cards & Banter)",
                        $"{mateDesc}\nPlay cards in the barracks mess hall to blow off steam (+8 Rel, +8 Happiness).",
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

            // 13. Theaters of War (Frontline Reassignment)
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

                // Transfer options
                if (s.currentTheater != WarTheaterType.WesternMudTrenches)
                {
                    CreateActivityCard("Request Transfer: Western Mud Trenches",
                        "Request deployment to Sector 7. Brutal artillery and mortar barrages with stagnant trench attrition.",
                        new Color(0.45f, 0.35f, 0.25f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.WesternMudTrenches);
                            RefreshUI();
                        });
                }

                if (s.currentTheater != WarTheaterType.EasternFrozenSteppe)
                {
                    CreateActivityCard("Request Transfer: Eastern Frozen Steppe",
                        "Request deployment to the Karkov Steppe. Sub-zero blizzards, frostbite hazards, and armored panzer counter-offensives.",
                        new Color(0.25f, 0.45f, 0.65f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.EasternFrozenSteppe);
                            RefreshUI();
                        });
                }

                if (s.currentTheater != WarTheaterType.PacificJungleIslands)
                {
                    CreateActivityCard("Request Transfer: Pacific Jungle Atolls",
                        "Request deployment to the Peleliu Jungle Islands. Mangrove swamps, spider-hole ambushes, and bunker flamethrowers.",
                        new Color(0.25f, 0.55f, 0.30f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.PacificJungleIslands);
                            RefreshUI();
                        });
                }

                if (s.currentTheater != WarTheaterType.UrbanCitadel)
                {
                    CreateActivityCard("Request Transfer: The Citadel Siege (City Ruins)",
                        "Request deployment to the shattered urban citadel. Sniper alleys, room clearing, and close-quarters rubble skirmishes.",
                        new Color(0.50f, 0.25f, 0.45f), () =>
                        {
                            if (GameManager.Instance != null) GameManager.Instance.TransferWarTheater(WarTheaterType.UrbanCitadel);
                            RefreshUI();
                        });
                }
            }

            // 14. Post-Service Veteran Careers & Civic Office (If Retired)
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

            // 15. Generational Military Legacy ("The Soldier's Child")
            if (s.childHeir == null)
            {
                CreateActivityCard("Family Legacy: Start a Family & Raise a Child",
                    "Take leave to marry your sweetheart and raise a child. You can bequeath your war medals and savings to them!",
                    new Color(0.75f, 0.40f, 0.60f), () =>
                    {
                        s.HaveChild();
                        RefreshUI();
                    });
            }
            else
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
                }
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
