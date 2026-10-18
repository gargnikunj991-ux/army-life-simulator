using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    public static class ChildhoodLifeEngine
    {
        /// <summary>
        /// Processes each 6-month semester of youth from Age 0.0 to 18.0.
        /// Handles physical development, schooling, sibling rivalries, and milestone dilemmas.
        /// </summary>
        public static void ProcessYouthSemester(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null) return;

            float age = soldier.age;

            // 1. Natural Growth & Health Baseline
            soldier.health = Mathf.Min(100f, soldier.health + 2f);

            // 2. Family Financial Upkeep (Child is supported by parents)
            if (soldier.lineage != null)
            {
                long familySupport = (soldier.lineage.startingNobleRank > NobleRank.Commoner) ? 100 : 20;
                soldier.cashBalance += familySupport;
            }

            // 3. Age-Specific Milestones & Feed Logs
            LogAgeMilestone(soldier, age);

            // 4. Milestone Interactive Dilemmas (Ages 0.5, 3.0, 6.0, 9.0, 11.0, 13.0, 15.0, 17.0)
            TriggerYouthDilemmaIfEligible(soldier, age);

            // 5. Turning Point: Age 18.0 Graduation & National Military Ranking Exam!
            if (age >= 18.0f)
            {
                ConductNationalMilitaryExam(soldier, gm);
            }
        }

        private static void LogAgeMilestone(SoldierProfile soldier, float age)
        {
            int ageInt = Mathf.FloorToInt(age);
            bool isFirstHalf = (age - ageInt) < 0.25f;

            if (age <= 1.0f && isFirstHalf)
            {
                GameEvents.OnFeedEventLogged?.Invoke("INFANCY",
                    $"<color=#00FFAA><b>[INFANCY • AGE 0.5]</b></color> You spoke your first words: <i>\"Ma-ma\"</i> and <i>\"Sword\"</i>! Your parents embrace you with pride.");
            }
            else if (age <= 1.5f && !isFirstHalf)
            {
                GameEvents.OnFeedEventLogged?.Invoke("GROWTH",
                    $"<b>[TODDLER • AGE 1.0]</b> You took your first independent steps across the courtyard, balancing steadily on your own two feet.");
            }
            else if (Mathf.Approximately(age, 2.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("GROWTH",
                    $"<b>[CHILDHOOD • AGE 2.0]</b> You survived a seasonal fever with high vitality. Your body's natural constitution is growing resilient.");
            }
            else if (Mathf.Approximately(age, 4.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<b>[FAMILY • AGE 4.0]</b> Your sibling <b>{soldier.siblingRivalName}</b> challenged you over a wooden warship toy. Childhood sibling rivalry begins!");
            }
            else if (Mathf.Approximately(age, 6.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACADEMY",
                    $"<color=#00D4FF><b>[PRIMARY SCHOOL • AGE 6.0]</b></color> You enrolled in primary school. You begin daily literacy, mathematics, and basic physical conditioning.");
            }
            else if (Mathf.Approximately(age, 8.5f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("EDUCATION",
                    $"<b>[TACTICS • AGE 8.5]</b> You read about the Great Continental War against the Iron Federation and Ouroboros Dominion (+3 Intellect).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 3f);
            }
            else if (Mathf.Approximately(age, 10.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("TEMPERING",
                    $"<color=#FFD700><b>[BODY TEMPERING • AGE 10.0]</b></color> You began youth foundational body tempering: distance running, pushups, and meridian breathing (+4 Fitness).");
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 4f);
                soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 6f);
            }
            else if (Mathf.Approximately(age, 12.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("CADET",
                    $"<color=#00FFAA><b>[YOUTH MARTIAL ACADEMY • AGE 12.0]</b></color> You enrolled in the District Cadet Academy! Uniforms, morning parade drills, and wooden rifle marksmanship.");
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
            }
            else if (Mathf.Approximately(age, 14.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("SPARRING",
                    $"<b>[SPARRING • AGE 14.0]</b> You competed in the cadet intra-school sparring tournament, learning defensive stances and close-quarters hand strikes.");
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 4f);
            }
            else if (Mathf.Approximately(age, 16.0f))
            {
                GameEvents.OnFeedEventLogged?.Invoke("EXAM PREP",
                    $"<b>[MOCK EXAM • AGE 16.0]</b> You took the preliminary district mock exam. Teachers commend your tactical reasoning (+5 Intellect, +5 Fitness).");
                soldier.intellect = Mathf.Min(100f, soldier.intellect + 5f);
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 5f);
            }
            else if (Mathf.Approximately(age, 17.5f))
            {
                soldier.mockExamScore = CalculateProjectedExamScore(soldier, out string projectedAcademy);
                soldier.tookMockExam = true;
                soldier.mockExamProjectedAcademy = projectedAcademy;

                GameEvents.OnFeedEventLogged?.Invoke("MOCK EXAM",
                    $"<color=#FF8800>==================================================\n" +
                    $"<b>[THE NATIONAL MOCK EXAM • AGE 17.5]</b>\n" +
                    $"You sat for the official preliminary trial ranking exam alongside candidates nationwide!\n" +
                    $"Mock Exam Score: <b>{soldier.mockExamScore} / 100</b>\n" +
                    $"Projected Placement: <color=#00FFAA><b>{projectedAcademy}</b></color>\n" +
                    $"Only 6 months remain before the final National Military Ranking Exam at Age 18.0!\n" +
                    $"==================================================</color>");
            }
        }

        private static void TriggerYouthDilemmaIfEligible(SoldierProfile soldier, float age)
        {
            // Trigger interactive dilemma events at key developmental crossroads
            if (Mathf.Approximately(age, 0.5f))
            {
                // Age 0.5: The Grasping Ritual
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "INFANCY MILESTONE",
                    "The Grasping Ceremony (One-Year Milestone)",
                    "On your first birthday ritual, your family places traditional symbolic items before you. Which one do your tiny hands reach for?",
                    "A Wooden Toy Dagger (+8 Fitness, +5 Tempering)",
                    "An Ancient Military Scroll (+10 Intellect)",
                    "A Family Gold Sovereign (+Clan Favor, +$200 Vault)",
                    () => { soldier.fitness += 8f; soldier.foundationalTempering += 5f; },
                    () => { soldier.intellect += 10f; },
                    () => { soldier.clanFavor += 8f; soldier.cashBalance += 200; }
                );
            }
            else if (Mathf.Approximately(age, 6.0f))
            {
                // Age 6.0: The School Bully
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "PRIMARY SCHOOL",
                    "The Courtyard Bully",
                    "A larger boy in the academy courtyard pushes you and demands your afternoon meat ration. How do you respond?",
                    "Fight back fiercely with bare hands! (+Combat, +Fitness)",
                    "Use clever words and outmaneuver him (+Intellect)",
                    "Report him to the martial proctor (+Merit)",
                    () => { soldier.combatSkill += 5f; soldier.fitness += 4f; soldier.health -= 5f; },
                    () => { soldier.intellect += 6f; soldier.squadRespect += 5f; },
                    () => { soldier.meritRating += 8f; }
                );
            }
            else if (Mathf.Approximately(age, 11.0f))
            {
                // Age 11.0: Sibling Rivalry
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "CLAN RIVALRY",
                    $"Sibling Rivalry: {soldier.siblingRivalName}'s Challenge",
                    $"Your competitive sibling {soldier.siblingRivalName} boasts that Father favors them for the future clan succession and challenges you to a courtyard duel!",
                    "Accept the challenge and spar with full force! (+Combat, +Clan Score)",
                    "Propose a tactical strategy game instead (+Intellect, +Clan Favor)",
                    "Humbly offer to train together as comrades (+Sibling Bond)",
                    () => { soldier.combatSkill += 6f; soldier.clanSuccessionScore += 10; soldier.siblingRelationship -= 5f; },
                    () => { soldier.intellect += 7f; soldier.clanFavor += 6f; },
                    () => { soldier.siblingRelationship += 15f; soldier.clanFavor += 5f; }
                );
            }
            else if (Mathf.Approximately(age, 14.5f))
            {
                // Age 14.5: Secret Training Opportunity
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "MARTIAL TRAINING",
                    "The Wandering Veteran's Advice",
                    "An old, scarred imperial veteran passing through your district offers to teach you a secret military breathing form for a few coins.",
                    "Pay $100 and study the breathing technique diligently (+15 Tempering)",
                    "Decline and stick to official academy curriculum (+5 Merit)",
                    "Ask him war stories about the Iron Federation (+8 Intellect)",
                    () => { soldier.cashBalance = Math.Max(0, soldier.cashBalance - 100); soldier.foundationalTempering += 15f; soldier.knowsAboutCultivation = true; },
                    () => { soldier.meritRating += 5f; soldier.fitness += 4f; },
                    () => { soldier.intellect += 8f; }
                );
            }
            else if (Mathf.Approximately(age, 17.0f))
            {
                // Age 17.0: Final Exam Specialization
                GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                    "NATIONAL EXAM FOCUS",
                    "Final Preparation Focus",
                    "With the National Military Ranking Exam approaching, where will you dedicate your final semester of preparation?",
                    "Intensive High-Gravity Bootcamp (+12 Fitness, +8 Combat)",
                    "War Room Strategy & Cyber Logistics (+15 Intellect)",
                    "Meditation & Foundational Tempering (+15 Tempering, +5 All Stats)",
                    () => { soldier.fitness += 12f; soldier.combatSkill += 8f; },
                    () => { soldier.intellect += 15f; soldier.isTechnicalSchoolGraduate = true; },
                    () => { soldier.foundationalTempering += 15f; soldier.fitness += 5f; soldier.intellect += 5f; }
                );
            }
            else if (Mathf.Approximately(age, 17.5f))
            {
                // Age 17.5: The National Mock Exam & "Run to Father for Help"
                TriggerAge17Point5MockExamDilemma(soldier);
            }
        }

        private static void TriggerAge17Point5MockExamDilemma(SoldierProfile soldier)
        {
            if (soldier == null) return;

            int mockScore = soldier.mockExamScore;
            string projectedAcademy = soldier.mockExamProjectedAcademy;

            bool isNoble = soldier.nobleTitle > NobleRank.Commoner;
            bool isWealthyCommoner = !isNoble && soldier.cashBalance >= 300;

            string choiceALabel;
            if (isNoble)
            {
                choiceALabel = "Run to Father for Help (Mobilize Clan Tutors & Lineage Backing)";
            }
            else if (isWealthyCommoner)
            {
                choiceALabel = "Run to Father for Help (Invest $300 Family Savings in Elite Tutors)";
            }
            else
            {
                choiceALabel = "Run to Father for Help (Plead for Family Guidance & Blessing)";
            }

            string choiceBLabel = "Solo Midnight Oil: Library Cramming & Night Ruck Drills (+10 Prep, -10 HP)";
            string choiceCLabel = "Breathing Cultivation: Inner Serenity & Meridian Tempering (+6 Prep, +10 HP)";

            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "NATIONAL MOCK EXAM",
                $"Mock Exam: {mockScore}/100 - Final Countdown",
                $"You received your preliminary National Mock Exam results: <b>{mockScore}/100</b> ({projectedAcademy}).\n\n" +
                $"Wealthy and noble peers are running to their fathers to hire elite tutors and pull strings for the final exam. " +
                $"You have 6 months left before the final National Military Ranking Exam at Age 18.0. What will you do?",
                choiceALabel,
                choiceBLabel,
                choiceCLabel,
                // Action A: Run to Father
                () =>
                {
                    soldier.receivedFatherExamBoost = true;
                    if (isNoble)
                    {
                        int boost = (soldier.nobleTitle >= NobleRank.Viscount) ? 18 : 14;
                        soldier.examPreparationBonus += boost;
                        soldier.intellect = Mathf.Min(100f, soldier.intellect + 8f);
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 8f);
                        soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 10f);
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 15f);

                        GameEvents.OnFeedEventLogged?.Invoke("FATHER'S AID",
                            $"<color=#FFD700><b>[FATHER'S NOBLE INTERVENTION]</b></color> Seeing your mock score of {mockScore}/100, your noble father mobilized clan wealth and hired Imperial Academy tutors! " +
                            $"You mastered secret exam strategy scenarios. Final Exam Score Boost: <b>+{boost} points</b>! (Intellect +8, Combat +8, Father Relation +15).");
                    }
                    else if (isWealthyCommoner)
                    {
                        soldier.cashBalance -= 300;
                        int boost = 12;
                        soldier.examPreparationBonus += boost;
                        soldier.intellect = Mathf.Min(100f, soldier.intellect + 7f);
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 6f);
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 10f);

                        GameEvents.OnFeedEventLogged?.Invoke("FATHER'S AID",
                            $"<color=#FFD700><b>[FATHER'S INVESTMENT]</b></color> You showed Father your mock score ({mockScore}/100). Father withdrew $300 of family savings to hire a veteran imperial drill master! " +
                            $"Final Exam Score Boost: <b>+{boost} points</b>! (Intellect +7, Combat +6).");
                    }
                    else
                    {
                        int boost = 6;
                        soldier.examPreparationBonus += boost;
                        soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 20f);
                        soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 10f);
                        soldier.cashBalance += 50;

                        GameEvents.OnFeedEventLogged?.Invoke("FATHER'S AID",
                            $"<color=#00FFAA><b>[FATHER'S BLESSING & SACRIFICE]</b></color> You showed Father your mock score ({mockScore}/100). " +
                            $"Tears in his eyes, he embraced you: <i>'Son, we don't have noble gold for tutors, but our clan has grit! Make us proud.'</i> " +
                            $"He slipped you $50 of emergency savings. Final Exam Score Boost: <b>+{boost} points</b>! (+10 Merit, +$50, +20 Father Relation).");
                    }
                },
                // Action B: Solo Midnight Oil
                () =>
                {
                    soldier.examPreparationBonus += 10;
                    soldier.intellect = Mathf.Min(100f, soldier.intellect + 10f);
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 8f);
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
                    soldier.health = Mathf.Max(25f, soldier.health - 10f);

                    GameEvents.OnFeedEventLogged?.Invoke("SOLO GRIT",
                        $"<color=#00D4FF><b>[MIDNIGHT CRAMMING & NIGHT DRILLS]</b></color> You relied on pure personal determination, studying tactics by candlelight and doing midnight ruck marches. " +
                        $"Final Exam Score Boost: <b>+10 points</b>! (Intellect +10, Fitness +8, Combat +5, Health -10).");
                },
                // Action C: Breathing Cultivation & Mental Composure
                () =>
                {
                    soldier.examPreparationBonus += 6;
                    soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 15f);
                    soldier.health = Mathf.Min(100f, soldier.health + 10f);
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 4f);

                    GameEvents.OnFeedEventLogged?.Invoke("INNER SERENITY",
                        $"<color=#00FFAA><b>[COMPOSURE & MERIDIAN TEMPERING]</b></color> You focused on mental stillness and foundational breath regulation, entering exam season with unshakeable calm. " +
                        $"Final Exam Score Boost: <b>+6 points</b>! (Foundational Tempering +15, Health +10).");
                }
            );
        }

        public static int CalculateProjectedExamScore(SoldierProfile soldier, out string projectedAcademy)
        {
            if (soldier == null)
            {
                projectedAcademy = "Recruit Depot";
                return 30;
            }

            float physicalScore = (soldier.fitness * 0.35f) + (soldier.combatSkill * 0.35f);
            float academicScore = soldier.intellect * 0.30f;
            float nobleBonus = (soldier.nobleTitle > NobleRank.Commoner) ? ((int)soldier.nobleTitle * 4f) : 0f;
            float temperingBonus = soldier.foundationalTempering * 0.08f;
            float prepBonus = soldier.examPreparationBonus;

            int score = Mathf.Clamp(Mathf.RoundToInt(physicalScore + academicScore + nobleBonus + temperingBonus + prepBonus), 30, 100);

            if (score >= 88)
            {
                projectedAcademy = "Imperial War Academy (Officer O-1)";
            }
            else if (score >= 70)
            {
                projectedAcademy = "Vanguard Candidate Academy (E-4 Specialist)";
            }
            else if (score >= 50)
            {
                projectedAcademy = "Technical & Cyber Warfare School (E-3)";
            }
            else
            {
                projectedAcademy = "Frontline Recruit Depot (E-1 Grunt)";
            }

            return score;
        }

        /// <summary>
        /// Conducts the National Military Ranking Exam at Age 18.
        /// Evaluates physical fitness, combat aptitude, intellect, noble privileges, and prep bonus.
        /// Unlocks War Academy (Officer O-1), Technical School (E-3), or Grunt (E-1)!
        /// </summary>
        public static void ConductNationalMilitaryExam(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null || soldier.currentRank != null) return;

            // 1. Calculate Score incorporating physical, combat, intellect, noble bonus, tempering, and preparation boost
            float physicalScore = (soldier.fitness * 0.35f) + (soldier.combatSkill * 0.35f);
            float academicScore = soldier.intellect * 0.30f;
            float nobleBonus = (soldier.nobleTitle > NobleRank.Commoner) ? ((int)soldier.nobleTitle * 4f) : 0f;
            float temperingBonus = soldier.foundationalTempering * 0.08f;
            float prepBonus = soldier.examPreparationBonus;

            float rawTotal = physicalScore + academicScore + nobleBonus + temperingBonus + prepBonus;
            soldier.examScore = Mathf.Clamp(Mathf.RoundToInt(rawTotal), 30, 100);

            // 2. Determine Commission & Starting Rank
            MilitaryRankSO assignedRank = null;
            string academyName;
            string department;
            int startingBonusCash = 0;

            if (soldier.examScore >= 88)
            {
                // Tier-1: Imperial War Academy (Officer O-1 2nd Lieutenant)
                academyName = "<color=#FFD700><b>IMPERIAL WAR ACADEMY (TOP 1% ELITE)</b></color>";
                department = "Officer Corps & Strategic Command";
                assignedRank = FindRank(gm, "O-1") ?? FindRank(gm, "E-4");
                startingBonusCash = 3000;
            }
            else if (soldier.examScore >= 70)
            {
                // Tier-2: Vanguard Candidate Academy (E-4 Specialist/Corporal)
                academyName = "<color=#00FFAA><b>VANGUARD CANDIDATE ACADEMY</b></color>";
                department = "Specialist & Reconnaissance";
                assignedRank = FindRank(gm, "E-4") ?? FindRank(gm, "E-3");
                startingBonusCash = 1500;
            }
            else if (soldier.examScore >= 50)
            {
                // Tier-3: Technical & Cyber Warfare School (E-3 Private First Class)
                academyName = "<color=#00D4FF><b>TECHNICAL & CYBER WARFARE SCHOOL</b></color>";
                department = "Technical & Cyber Operations";
                soldier.isTechnicalSchoolGraduate = true;
                assignedRank = FindRank(gm, "E-3");
                startingBonusCash = 800;
            }
            else
            {
                // Tier-4: Standard Frontline Walk-In (E-1 Private Recruit)
                academyName = "<color=#AAAAAA><b>VALERIAN GROUND FORCES RECRUIT DEPOT</b></color>";
                department = "Frontline Infantry";
                assignedRank = FindRank(gm, "E-1");
                startingBonusCash = 300;
            }

            if (assignedRank != null)
            {
                soldier.currentRank = assignedRank;
            }
            soldier.assignedDepartment = department;
            soldier.cashBalance += startingBonusCash;

            // 3. Score progression and father's contribution breakdown
            string scoreProgression = soldier.tookMockExam
                ? $"\n• Mock Exam Score (Age 17.5): <b>{soldier.mockExamScore} / 100</b> ({soldier.mockExamProjectedAcademy})" +
                  $"\n• Final Exam Score (Age 18.0): <b>{soldier.examScore} / 100</b> (<color=#00FFAA>+{(soldier.examScore - soldier.mockExamScore):+0;-0;0} improvement!</color>)"
                : $"\n• Final Exam Score: <b>{soldier.examScore} / 100</b>";

            string boostBreakdown = "";
            if (soldier.receivedFatherExamBoost)
            {
                boostBreakdown = (soldier.nobleTitle > NobleRank.Commoner)
                    ? $"\n• <color=#FFD700>Father's imperial tutors and clan backing delivered a critical <b>+{soldier.examPreparationBonus} pt</b> boost!</color>"
                    : (soldier.cashBalance >= 0 && soldier.examPreparationBonus >= 12
                        ? $"\n• <color=#FFD700>Father's hired drill master and family investment delivered a <b>+{soldier.examPreparationBonus} pt</b> boost!</color>"
                        : $"\n• <color=#00FFAA>Father's heartfelt blessing and family encouragement delivered a <b>+{soldier.examPreparationBonus} pt</b> boost!</color>");
            }
            else if (soldier.examPreparationBonus > 0)
            {
                boostBreakdown = $"\n• <color=#00D4FF>Your relentless solo preparation delivered a <b>+{soldier.examPreparationBonus} pt</b> boost!</color>";
            }

            // 4. Log Historic Transition from Youth to Active Duty Soldier
            GameEvents.OnFeedEventLogged?.Invoke("EXAM RESULTS",
                $"<color=#FFD700>==================================================\n" +
                $"<b>[NATIONAL MILITARY RANKING EXAM RESULTS • AGE 18.0]</b>" +
                $"{scoreProgression}" +
                $"{boostBreakdown}\n" +
                $"Placement: {academyName}\n" +
                $"Starting Commission: <b>{(soldier.currentRank != null ? soldier.currentRank.rankName : "Soldier")} ({(soldier.currentRank != null ? soldier.currentRank.paygrade : "E-1")})</b>\n" +
                $"Assigned Department: <b>{department}</b> | Signing Bonus: <b>+${startingBonusCash:N0}</b>\n" +
                $"==================================================</color>");

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVE DUTY",
                $"<color=#00FFAA><b>[OFFICIAL ENLISTMENT COMPLETE]</b></color> At age 18.0, your childhood in the Valerian Empire has concluded. " +
                $"You put on the combat fatigues of the Valerian Vanguard. Your military career begins now!");
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

        // ==================== YOUTH MANUAL ACTIVITIES ====================

        public static void PerformBodyTempering(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.fitness = Mathf.Min(100f, soldier.fitness + 5f);
            soldier.foundationalTempering = Mathf.Min(100f, soldier.foundationalTempering + 7f);
            soldier.health = Mathf.Min(100f, soldier.health + 2f);

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#00FFAA>[BODY TEMPERING DRILL]</color> You completed an intense session of body conditioning and meridian breathing! " +
                $"Fitness: <b>{soldier.fitness:F0}/100</b> | Foundational Tempering: <b>{soldier.foundationalTempering:F0}/100</b>.");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformStudyTactics(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.intellect = Mathf.Min(100f, soldier.intellect + 6f);
            soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 3f);

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#00AAFF>[TACTICAL STUDY]</color> You read imperial war archives and analyzed enemy drone strategies! " +
                $"Intellect: <b>{soldier.intellect:F0}/100</b> (Boosts age-18 exam score!).");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformFamilyBonding(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 8f);
            soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 4f);
            soldier.cashBalance += 40;

            if (soldier.age >= 17.0f)
            {
                soldier.examPreparationBonus += 2;
                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#FFD700>[FAMILY TIME & EXAM COACHING]</color> You spent the afternoon with Father reviewing past military campaigns. " +
                    $"Father shared veteran exam tips (+2 Exam Prep Bonus)! Parent Relationship: <b>{soldier.parentRelationship:F0}/100</b>. Father gave you <b>+$40</b> pocket money!");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                    $"<color=#FFD700>[FAMILY TIME]</color> You spent the afternoon helping your parents around the household. " +
                    $"Parent Relationship: <b>{soldier.parentRelationship:F0}/100</b>. Father slipped you <b>+$40</b> allowance!");
            }
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void PerformSiblingSparring(SoldierProfile soldier)
        {
            if (soldier == null) return;
            soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 5f);
            soldier.clanSuccessionScore += 5;

            GameEvents.OnFeedEventLogged?.Invoke("ACTIVITY",
                $"<color=#FF8844>[SIBLING SPARRING]</color> You challenged your rival sibling <b>{soldier.siblingRivalName}</b> to a mock duel in the courtyard! " +
                $"Combat Skill: <b>{soldier.combatSkill:F0}/100</b> | Clan Succession Standing: <b>+{soldier.clanSuccessionScore}</b>.");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }
    }
}
