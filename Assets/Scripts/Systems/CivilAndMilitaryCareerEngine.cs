using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class CivilAndMilitaryCareerEngine
    {
        public const long COLLEGE_SEMESTER_TUITION = 12500L; // $12,500 / sem = $100,000 for 4 years (8 semesters)
        public const long LOWEST_CIVIL_PAY = 150L;           // Lowest paying job: $150/mo
        public const long MAX_NON_COLLEGE_PAY = 300L;        // Max job without college: $300/mo
        public const long COLLEGE_GRAD_BASE_PAY = 500L;      // College graduate starting pay: $500/mo

        /// <summary>
        /// Evaluates age 18 National Exam score and presents branching options.
        /// If score between 30% and 50%: Player chooses between Civil and Military life!
        /// </summary>
        public static void EvaluateAge18CareerChoice(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null || soldier.age < 18.0f) return;

            float scorePercent = (soldier.examScore / 750f) * 100f;

            if (scorePercent >= 30f && scorePercent <= 50f)
            {
                // Score 30% - 50%: Player choice between Civil Life, Army, and Air Force!
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "CAREER DESTINY",
                    "Age 18 Milestone: Civil vs Military Destiny",
                    $"National Exam Result: {soldier.examScore}/750 ({scorePercent:F1}%)\n" +
                    "Your score places you in the middle tier. Imperial regulations grant you freedom of service: Enter Civilian Employment, Enlist in the Imperial Army (6 Mo Boot Camp -> Frontline Bounties), or Enlist in the Imperial Air Force (1 Yr Boot Camp -> Superior Quarters & Flight Pay).",
                    "ENTER CIVILIAN LIFE ($150-$300/mo Jobs & Trade)",
                    "ENLIST IN IMPERIAL ARMY (6 Mo Boot Camp -> Frontline Bounties)",
                    "ENLIST IN AIR FORCE (1 Yr Boot Camp -> +15% Flight Pay)",
                    () => ChooseCivilianLife(soldier),
                    () => EnlistAsRecruit(soldier, gm, MilitaryBranch.Army),
                    () => EnlistAsRecruit(soldier, gm, MilitaryBranch.AirForce, AirForceRole.FlightDuty)
                );
            }
            else if (scorePercent < 30f)
            {
                // Below 30%: Grassroots civilian work, Army recruit, or Air Force ground staff
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "EXAM FAILURE",
                    "National Exam Concluded (< 30%)",
                    $"National Exam Result: {soldier.examScore}/750 ({scorePercent:F1}%)\n" +
                    "You failed to achieve the academic threshold for officer commissioning. You must work grassroots labor or enlist as a recruit.",
                    "ACCEPT WORKSHOP LABOR ($150/mo Entry Job)",
                    "VOLUNTEER: IMPERIAL ARMY (6 Mo Boot Camp -> E-1 Private)",
                    "VOLUNTEER: AIR FORCE (1 Yr Boot Camp -> Ground Support Staff)",
                    () => StartDirectCivilianJob(soldier, "Apprentice Workshop Laborer", LOWEST_CIVIL_PAY),
                    () => EnlistAsRecruit(soldier, gm, MilitaryBranch.Army),
                    () => EnlistAsRecruit(soldier, gm, MilitaryBranch.AirForce, AirForceRole.GroundDutyStaff)
                );
            }
            else
            {
                // Above 50%: Elite academies unlocked! Army War Academy vs Air Force Flight Academy vs University
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "ACADEMY COMMISSIONS",
                    "National Exam Honors (> 50%)",
                    $"National Exam Result: {soldier.examScore}/750 ({scorePercent:F1}%)\n" +
                    "High honors! You qualify for Imperial Military Academies or Prestigious University Degrees.",
                    "ARMY WAR ACADEMY (2 Yrs, $50k Tuition -> The Iron Vanguard)",
                    "AIR FORCE FLIGHT ACADEMY (3 Yrs, $200k Tuition -> Celestial Valkyries)",
                    "IMPERIAL UNIVERSITY DEGREE ($100,000 Tuition -> High Civil Office)",
                    () => EnrollInWarAcademy(soldier, takeLoan: soldier.cashBalance < EconomyData.ARMY_ACADEMY_TUITION_COST, deductionPercent: 0.15f, branch: MilitaryBranch.Army),
                    () => EnrollInWarAcademy(soldier, takeLoan: soldier.cashBalance < EconomyData.AIRFORCE_ACADEMY_TUITION_COST, deductionPercent: 0.15f, branch: MilitaryBranch.AirForce),
                    () => EnrollInCollege(soldier, "Aetheric Energy & Cyber Engineering")
                );
            }
        }

        // ==================== CAREER CHOICES ====================

        public static void ChooseCivilianLife(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.careerTrack = CareerTrack.CivilianWork;
            soldier.isInCombatDeployment = false;
            soldier.civilianJobTitle = "Junior Municipal Clerk";
            soldier.civilianSalaryPerMonth = 220L;

            GameEvents.OnFeedEventLogged?.Invoke("CIVILIAN LIFE",
                $"<color=#00FFAA><b>[CIVILIAN LIFE CHOSEN]</b></color> You declined military conscription and took up civilian employment as a <b>{soldier.civilianJobTitle}</b>! " +
                $"Starting Pay: <b>${soldier.civilianSalaryPerMonth}/mo</b>. You can work toward higher civil positions ($150 - $300/mo) or save for Imperial College ($100k).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void StartDirectCivilianJob(SoldierProfile soldier, string title, long pay)
        {
            if (soldier == null) return;
            soldier.careerTrack = CareerTrack.CivilianWork;
            soldier.civilianJobTitle = title;
            soldier.civilianSalaryPerMonth = System.Math.Clamp(pay, LOWEST_CIVIL_PAY, MAX_NON_COLLEGE_PAY);
            soldier.isInCombatDeployment = false;

            GameEvents.OnFeedEventLogged?.Invoke("JOB",
                $"<color=#00D4FF>[CIVILIAN EMPLOYMENT]</color> You began work as <b>{title}</b> earning <b>${soldier.civilianSalaryPerMonth}/mo</b>.");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void EnrollInCollege(SoldierProfile soldier, string degreeName = "Imperial Administration")
        {
            if (soldier == null) return;
            soldier.careerTrack = CareerTrack.CivilianCollege;
            soldier.collegeDegreeName = degreeName;
            soldier.collegeSemestersCompleted = 0;
            soldier.isInCombatDeployment = false;

            // Deduct or loan 1st semester tuition ($12,500)
            if (soldier.cashBalance >= COLLEGE_SEMESTER_TUITION)
            {
                soldier.cashBalance -= COLLEGE_SEMESTER_TUITION;
                GameEvents.OnFeedEventLogged?.Invoke("COLLEGE",
                    $"<color=#00FFAA><b>[IMPERIAL COLLEGE ENROLLMENT]</b></color> You enrolled in the 4-year degree for <b>{degreeName}</b>! " +
                    $"Paid $12,500 semester tuition (Total 4-Year Tuition: $100,000). Upon graduation, monthly salaries exceed $500+/mo!");
            }
            else
            {
                long loan = COLLEGE_SEMESTER_TUITION - soldier.cashBalance;
                soldier.collegeTuitionDebt += loan;
                soldier.cashBalance = 0;
                GameEvents.OnFeedEventLogged?.Invoke("COLLEGE",
                    $"<color=#FFD700><b>[COLLEGE STUDENT LOAN]</b></color> You enrolled in <b>{degreeName}</b> using an Imperial Student Debt Loan (+${loan:N0} Debt). " +
                    $"Total 4-Year Tuition: $100,000. 8 semesters of study commence.");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void EnlistAsRecruit(SoldierProfile soldier, GameManager gm, MilitaryBranch branch = MilitaryBranch.Army, AirForceRole role = AirForceRole.FlightDuty)
        {
            if (soldier == null) return;
            soldier.militaryBranch = branch;
            soldier.airForceRole = (branch == MilitaryBranch.AirForce) ? role : AirForceRole.None;
            soldier.careerTrack = CareerTrack.MilitaryRecruitBasic;
            soldier.militarySchoolSemestersCompleted = 0;
            soldier.currentRank = null; // Unranked recruit
            soldier.isInCombatDeployment = false; // In training camp

            if (branch == MilitaryBranch.AirForce)
            {
                soldier.assignedDepartment = "Imperial Air Force Recruit Depot";
                GameEvents.OnFeedEventLogged?.Invoke("ENLISTMENT",
                    "<color=#00D4FF><b>[ENLISTED AS AIR FORCE RECRUIT • 1-YEAR BASIC TRAINING]</b></color> You entered the rigorous 1-year (2-semester) Air Force Recruit Depot. " +
                    "<b>Pay Policy: $0 / month during basic training</b> (flight dorms & rations provided). " +
                    "Endure 1 year of aviation drills and endurance conditioning to commission as Airman Basic (E-1) with superior quarters (+15% Morale) and flight opportunities!");
            }
            else
            {
                soldier.assignedDepartment = "Infantry Recruit Depot";
                GameEvents.OnFeedEventLogged?.Invoke("ENLISTMENT",
                    "<color=#FF8844><b>[ENLISTED AS RECRUIT • BASIC TRAINING]</b></color> You entered the 6-month Basic Combat Training Depot. " +
                    "<b>Pay Policy: $0 / month during basic training</b> (free mess hall rations only). " +
                    "Endure physical drills for 6 months to earn your Private (E-1) stripes and begin receiving military pay!");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void EnrollInWarAcademy(SoldierProfile soldier, bool takeLoan = false, float deductionPercent = 0.15f, MilitaryBranch branch = MilitaryBranch.Army)
        {
            if (soldier == null) return;
            soldier.militaryBranch = branch;
            soldier.careerTrack = CareerTrack.MilitaryWarAcademy;
            soldier.militarySchoolSemestersCompleted = 0;
            soldier.academySemestersCompleted = 0;
            soldier.isInCombatDeployment = false;

            long tuition = (branch == MilitaryBranch.AirForce)
                ? EconomyData.AIRFORCE_ACADEMY_TUITION_COST
                : EconomyData.ARMY_ACADEMY_TUITION_COST;
            int totalYears = (branch == MilitaryBranch.AirForce)
                ? (EconomyData.AIRFORCE_ACADEMY_SEMESTERS / 2)
                : (EconomyData.ARMY_WAR_ACADEMY_SEMESTERS / 2);
            string academyName = (branch == MilitaryBranch.AirForce)
                ? "The Imperial Flight Academy"
                : "The Imperial War Academy";
            string sfName = soldier.specialForcesBranchName;

            soldier.assignedDepartment = academyName;

            bool mustLoan = takeLoan || soldier.cashBalance < tuition;
            if (mustLoan)
            {
                long downPayment = takeLoan ? 0L : soldier.cashBalance;
                long loanAmount = tuition - downPayment;
                soldier.cashBalance -= downPayment;
                soldier.academyTuitionDebt = loanAmount;
                soldier.academyLoanDeductionPercent = Mathf.Clamp(deductionPercent, 0.10f, 0.80f);

                string downPaymentMsg = downPayment > 0 ? $" (Paid ${downPayment:N0} down payment)" : "";
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#FFD700><b>[{academyName.ToUpper()} • {totalYears} YEARS]</b></color> You secured a limited seat in the Officer Cadet class! " +
                    $"<b>Tuition: ${loanAmount:N0} Education Loan approved</b>{downPaymentMsg} at 9% annual interest. " +
                    $"Monthly deduction set at <b>{soldier.academyLoanDeductionPercent * 100f:F0}%</b> of salary. Top 10% qualify for {sfName}!");
            }
            else
            {
                soldier.cashBalance -= tuition;
                soldier.academyTuitionDebt = 0;
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#FFD700><b>[{academyName.ToUpper()} • {totalYears} YEARS]</b></color> You paid the ${tuition:N0} tuition upfront in cash! " +
                    $"{totalYears} years of intensive tactical command and physical preparation begin. Top 10% qualify for {sfName}!");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void EnrollInTechOrMedical(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null) return;
            bool chooseMedical = soldier.intellect >= soldier.combatSkill;

            if (chooseMedical)
            {
                soldier.careerTrack = CareerTrack.MilitaryMedicalSchool;
                soldier.militarySchoolSemestersCompleted = 0;
                soldier.assignedDepartment = "Imperial Military Medical College";
                soldier.isInCombatDeployment = false;

                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    "<color=#00D4FF><b>[MILITARY MEDICAL SCHOOL • 3 YEARS]</b></color> Admitted to the 3-year medical officer track! " +
                    "<b>Pay Policy: Year 1 unpaid ($0); Years 2 and 3 paid $250 / month</b>. " +
                    "Passes out as a Combat Medic Officer.");
            }
            else
            {
                soldier.careerTrack = CareerTrack.MilitaryTechSchool;
                soldier.militarySchoolSemestersCompleted = 0;
                soldier.assignedDepartment = "Imperial Technical & Cyber Academy";
                soldier.isInCombatDeployment = false;

                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    "<color=#00FFAA><b>[MILITARY TECHNICAL SCHOOL • 2 YEARS]</b></color> Admitted to the 2-year technical academy! " +
                    "<b>Pay Policy: Year 1 unpaid ($0); Year 2 paid $200 / month</b>. " +
                    "Passes out as Specialist (E-3) with permanent Tech ($450) and Cyber ($350) stipends.");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        // ==================== SEMESTER PROGRESSION ENGINE ====================

        /// <summary>
        /// Called each semester by GameManager to advance education, training, and civilian careers.
        /// </summary>
        public static void AdvanceCareerTurn(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired) return;

            switch (soldier.careerTrack)
            {
                // 1. Basic Training (Army: 6 months = 1 sem; Air Force: 1 year = 2 sem)
                case CareerTrack.MilitaryRecruitBasic:
                    soldier.militarySchoolSemestersCompleted++;
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 8f);
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 6f);
                    if (soldier.militaryBranch == MilitaryBranch.AirForce)
                    {
                        soldier.intellect = Mathf.Min(100f, soldier.intellect + 4f);
                    }

                    int reqBasicSemesters = (soldier.militaryBranch == MilitaryBranch.AirForce)
                        ? EconomyData.AIRFORCE_BASIC_TRAINING_SEMESTERS // 2 semesters = 1 year
                        : EconomyData.ARMY_BASIC_TRAINING_SEMESTERS;     // 1 semester = 6 months

                    if (soldier.militarySchoolSemestersCompleted >= reqBasicSemesters)
                    {
                        // Passed Basic Training! Commission as E-1
                        soldier.careerTrack = CareerTrack.MilitaryEnlisted;
                        var e1Rank = FindRank(gm, "E-1");
                        soldier.currentRank = e1Rank;
                        soldier.timeInCurrentRankSemesters = 0;
                        soldier.accumulatedPromotionPoints = 0;

                        if (soldier.militaryBranch == MilitaryBranch.AirForce)
                        {
                            soldier.assignedDepartment = (soldier.airForceRole == AirForceRole.FlightDuty)
                                ? "1st Tactical Aviation Wing (Flight Duty)"
                                : "Air Base Ground Logistics & Avionics";

                            GameEvents.OnFeedEventLogged?.Invoke("GRADUATION",
                                "<color=#00D4FF><b>[AIR FORCE BASIC TRAINING CONCLUDED • AIRMAN E-1]</b></color> You completed 1 year of aviation recruit conditioning! " +
                                $"Commissioned as <b>Airman (E-1)</b> in {soldier.assignedDepartment}. Monthly military pay begins and superior air base quarters activated!");
                        }
                        else
                        {
                            soldier.assignedDepartment = "Line Infantry Regiment";
                            GameEvents.OnFeedEventLogged?.Invoke("GRADUATION",
                                "<color=#00FFAA><b>[BASIC TRAINING CONCLUDED • PRIVATE E-1]</b></color> You completed the grueling 6-month recruit drills! " +
                                "You earned your private stripes (E-1). Your monthly military pay and frontline combat bounty eligibility begin.");
                        }
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                            $"<color=#00D4FF>[AIR FORCE BASIC RECRUIT DEPOT • TERM {soldier.militarySchoolSemestersCompleted}/{reqBasicSemesters}]</color> Completed basic aerodynamics and endurance conditioning. 6 months remain before graduation.");
                    }
                    break;

                // 2. Technical School (2 years = 4 semesters)
                case CareerTrack.MilitaryTechSchool:
                    soldier.militarySchoolSemestersCompleted++;
                    soldier.intellect = Mathf.Min(100f, soldier.intellect + 4f);

                    if (soldier.militarySchoolSemestersCompleted == 2)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                            "<color=#00FFAA>[TECH SCHOOL YEAR 2]</color> Year 1 complete! Technical cadet stipend of <b>$200 / mo</b> activated for Year 2.");
                    }
                    else if (soldier.militarySchoolSemestersCompleted >= 4)
                    {
                        soldier.graduatedTechSchool = true;
                        soldier.isTechnicalSchoolGraduate = true;
                        soldier.careerTrack = CareerTrack.MilitaryEnlisted;
                        soldier.currentRank = FindRank(gm, "E-3");
                        soldier.timeInCurrentRankSemesters = 0;
                        soldier.accumulatedPromotionPoints = 0;

                        GameEvents.OnFeedEventLogged?.Invoke("GRADUATION",
                            "<color=#00FFAA><b>[TECH SCHOOL GRADUATION • SPECIALIST E-3]</b></color> You graduated the 2-year technical program with honors! " +
                            "Commissioned directly as <b>Specialist (E-3)</b> with Tech ($450) and Cyber ($350) stipends.");
                    }
                    break;

                // 3. Medical School (3 years = 6 semesters)
                case CareerTrack.MilitaryMedicalSchool:
                    soldier.militarySchoolSemestersCompleted++;
                    soldier.intellect = Mathf.Min(100f, soldier.intellect + 5f);

                    if (soldier.militarySchoolSemestersCompleted == 2)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                            "<color=#00FFAA>[MEDICAL SCHOOL YEAR 2]</color> Year 1 complete! Medical cadet stipend of <b>$250 / mo</b> activated for Years 2 and 3.");
                    }
                    else if (soldier.militarySchoolSemestersCompleted >= 6)
                    {
                        soldier.graduatedMedicalSchool = true;
                        soldier.careerTrack = CareerTrack.MilitaryOfficer;
                        soldier.currentRank = FindRank(gm, "O-1") ?? FindRank(gm, "E-4");
                        soldier.timeInCurrentRankSemesters = 0;
                        soldier.accumulatedPromotionPoints = 0;

                        GameEvents.OnFeedEventLogged?.Invoke("GRADUATION",
                            "<color=#00FFAA><b>[MEDICAL ACADEMY GRADUATION • MEDICAL OFFICER]</b></color> You graduated the 3-year military medical program! " +
                            "Commissioned as a Combat Medic Officer.");
                    }
                    break;

                // 4. War Academy / Flight Academy (Army: 2 years = 4 sem; Air Force: 3 years = 6 sem)
                case CareerTrack.MilitaryWarAcademy:
                    soldier.militarySchoolSemestersCompleted++;
                    soldier.academySemestersCompleted++;
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 5f);
                    soldier.intellect = Mathf.Min(100f, soldier.intellect + 6f);
                    soldier.leadership = Mathf.Min(100f, soldier.leadership + 6f);
                    soldier.discipline = Mathf.Min(100f, soldier.discipline + 5f);

                    if (soldier.militaryBranch == MilitaryBranch.AirForce)
                    {
                        soldier.flightHoursLogged += 40; // 40 flight hours per semester in supersonic trainer jets
                    }

                    // Academic and tactical performance evaluation (0 - 100%)
                    float currentSemesterGrade = (soldier.intellect * 0.35f) + (soldier.fitness * 0.25f) + (soldier.discipline * 0.20f) + (soldier.combatSkill * 0.20f);
                    soldier.academyPerformanceScore = Mathf.Clamp(currentSemesterGrade, 0f, 100f);

                    int maxAcademySemesters = (soldier.militaryBranch == MilitaryBranch.AirForce)
                        ? EconomyData.AIRFORCE_ACADEMY_SEMESTERS // 6 semesters = 3 years
                        : EconomyData.ARMY_WAR_ACADEMY_SEMESTERS; // 4 semesters = 2 years

                    int sfTriggerSemester = (soldier.militaryBranch == MilitaryBranch.AirForce)
                        ? 5 // Year 3, Semester 1 (Semester 5)
                        : 3; // Year 2, Semester 1 (Semester 3)

                    string branchAcademyName = (soldier.militaryBranch == MilitaryBranch.AirForce)
                        ? "Imperial Flight Academy"
                        : "Imperial War Academy";

                    string branchSfName = soldier.specialForcesBranchName;

                    // Progress notifications for intermediate completed years
                    if (soldier.academySemestersCompleted % 2 == 0 && soldier.academySemestersCompleted < maxAcademySemesters)
                    {
                        int yearCompleted = soldier.academySemestersCompleted / 2;
                        GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                            $"<color=#00FFAA><b>[{branchAcademyName.ToUpper()} • YEAR {yearCompleted} COMPLETED]</b></color> You completed Year {yearCompleted} of officer command training! Class Standing: <b>{soldier.academyPerformanceScore:F1}%</b>.");
                    }

                    // Special Forces Selection Qualification Trigger
                    if (soldier.academySemestersCompleted == sfTriggerSemester)
                    {
                        if (soldier.isAcademyTop10Percent && !soldier.isSpecialForces && !soldier.isSpecialForcesCandidate)
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                                $"<color=#FFD700><b>[{branchSfName.ToUpper()} SELECTION QUALIFIED • TOP 10%]</b></color> Your top 10% class standing (<b>{soldier.academyPerformanceScore:F1}%</b>) has officially qualified you to volunteer for elite <b>{branchSfName} Special Forces</b>!");
                        }
                    }

                    // Graduation and Direct O-1 Commission
                    if (soldier.academySemestersCompleted >= maxAcademySemesters)
                    {
                        soldier.graduatedWarAcademy = true;
                        soldier.careerTrack = CareerTrack.MilitaryOfficer;
                        soldier.currentRank = FindRank(gm, "O-1");
                        soldier.timeInCurrentRankSemesters = 0;
                        soldier.accumulatedPromotionPoints = 0;
                        soldier.AddTrait(soldier.militaryBranch == MilitaryBranch.AirForce ? "Flight Academy Graduate" : "War Academy Graduate");
                        soldier.AddTrait("Commissioned Officer");

                        if (soldier.militaryBranch == MilitaryBranch.AirForce)
                        {
                            soldier.airForceRole = AirForceRole.FlightDuty;
                            soldier.assignedDepartment = "1st Imperial Fighter Squadron";
                        }
                        else
                        {
                            soldier.assignedDepartment = "Field Tactical Command";
                        }

                        string honorsText = "";
                        if (soldier.isAcademyTop10Percent)
                        {
                            soldier.AddMedal("Academy Honors Saber");
                            honorsText = " Graduated in the <b>Top 10% with High Honors</b> and awarded the <i>Imperial Honors Saber</i>!";
                        }

                        string officerTitle = (soldier.militaryBranch == MilitaryBranch.AirForce) ? "Flight Lieutenant (O-1)" : "2nd Lieutenant (O-1)";
                        GameEvents.OnFeedEventLogged?.Invoke("GRADUATION",
                            $"<color=#FFD700><b>[{branchAcademyName.ToUpper()} GRADUATION • COMMISSIONED {officerTitle.ToUpper()}]</b></color> " +
                            $"You conquered the {maxAcademySemesters / 2}-year {branchAcademyName}!{honorsText} " +
                            $"Directly commissioned as an Imperial Officer <b>({soldier.currentRank?.rankName ?? officerTitle})</b> with full command authority and ${(soldier.currentRank?.baseMonthlySalary ?? 3800):N0}/mo base salary!");
                    }
                    break;

                // 5. College Track (4 years = 8 semesters, $100k total tuition)
                case CareerTrack.CivilianCollege:
                    soldier.collegeSemestersCompleted++;
                    soldier.intellect = Mathf.Min(100f, soldier.intellect + 5f);

                    // Tuition check
                    if (soldier.cashBalance >= COLLEGE_SEMESTER_TUITION)
                    {
                        soldier.cashBalance -= COLLEGE_SEMESTER_TUITION;
                    }
                    else
                    {
                        long shortfall = COLLEGE_SEMESTER_TUITION - soldier.cashBalance;
                        soldier.collegeTuitionDebt += shortfall;
                        soldier.cashBalance = 0;
                    }

                    if (soldier.collegeSemestersCompleted >= 8)
                    {
                        // Graduation! Degree unlocked!
                        soldier.graduatedCollege = true;
                        soldier.careerTrack = CareerTrack.CivilianProfessional;
                        soldier.civilianJobTitle = GetGraduateJobTitle(soldier.collegeDegreeName);
                        soldier.civilianSalaryPerMonth = COLLEGE_GRAD_BASE_PAY;

                        GameEvents.OnFeedEventLogged?.Invoke("GRADUATION",
                            $"<color=#00FFAA><b>[COLLEGE GRADUATION • DEGREE CONFERRED]</b></color> You graduated after 4 years of rigorous study with a degree in <b>{soldier.collegeDegreeName}</b>! " +
                            $"You secured a professional position as <b>{soldier.civilianJobTitle}</b> starting at <b>${soldier.civilianSalaryPerMonth:N0} / month</b>!");
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("COLLEGE",
                            $"[COLLEGE TERM {soldier.collegeSemestersCompleted}/8] Completed academic term in {soldier.collegeDegreeName}. Intellect: {soldier.intellect:F0}/100.");
                    }
                    break;

                // 6. Civilian Work (Direct entry)
                case CareerTrack.CivilianWork:
                    // Experience can slowly raise pay up to $300 max
                    if (soldier.civilianSalaryPerMonth < MAX_NON_COLLEGE_PAY && Random.Range(0f, 1f) < 0.25f)
                    {
                        soldier.civilianSalaryPerMonth = System.Math.Min(MAX_NON_COLLEGE_PAY, soldier.civilianSalaryPerMonth + 25L);
                        GameEvents.OnFeedEventLogged?.Invoke("RAISE",
                            $"<color=#00FFAA>[CIVILIAN PAY RAISE]</color> Your employer recognized your diligence. Monthly wage raised to <b>${soldier.civilianSalaryPerMonth}/mo</b>.");
                    }
                    break;

                // 7. Civilian Professional (College graduates)
                case CareerTrack.CivilianProfessional:
                    // Can earn promotions to $750, $1,200, $2,500
                    if (Random.Range(0f, 1f) < 0.20f)
                    {
                        soldier.civilianSalaryPerMonth += 150L;
                        GameEvents.OnFeedEventLogged?.Invoke("PROMOTION",
                            $"<color=#FFD700>[CIVIL SERVICE ADVANCEMENT]</color> You earned a managerial civil promotion! Monthly Salary: <b>${soldier.civilianSalaryPerMonth:N0}/mo</b>.");
                    }
                    break;
            }

            // Time in military rank counter
            if (soldier.currentRank != null && !soldier.isCivilian)
            {
                soldier.timeInCurrentRankSemesters++;
            }
        }

        private static string GetGraduateJobTitle(string degree)
        {
            if (string.IsNullOrEmpty(degree)) return "Junior Magistrate";
            if (degree.Contains("Energy") || degree.Contains("Cyber")) return "Aetheric Systems Engineer";
            if (degree.Contains("Commerce") || degree.Contains("Merchant")) return "Guild Commercial Officer";
            if (degree.Contains("Alchemy") || degree.Contains("Medical")) return "Certified Imperial Alchemist";
            return "Junior Imperial Magistrate";
        }

        private static MilitaryRankSO FindRank(GameManager gm, string paygrade)
        {
            if (gm == null || gm.militaryRankDatabase == null) return null;
            foreach (var rank in gm.militaryRankDatabase)
            {
                if (rank != null && rank.paygrade == paygrade) return rank;
            }
            return null;
        }
    }
}
