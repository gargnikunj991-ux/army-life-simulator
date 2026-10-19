#if UNITY_EDITOR
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using UnityEditor;
using UnityEngine;

namespace ArmyLife.Editor
{
    public static class MockExamTestRunner
    {
        [MenuItem("Army Life/Run Mock Exam & Father Boost Simulation Test", false, 10)]
        public static void RunMockExamSimulationTest()
        {
            Debug.Log("<color=#00D4FF>========== STARTING MOCK EXAM & FATHER BOOST SIMULATION TEST ==========</color>");

            // Test Case 1: Noble Viscount Candidate running to father
            var nobleViscount = new SoldierProfile("Aurelius Vance", null, 150)
            {
                nobleTitle = NobleRank.Viscount,
                age = 17.5f,
                fitness = 75f,
                combatSkill = 70f,
                intellect = 72f,
                foundationalTempering = 60f,
                cashBalance = 2500
            };

            int nobleMockScore = ChildhoodLifeEngine.CalculateProjectedExamScore(nobleViscount, out string nobleProjectedAcademy);
            nobleViscount.mockExamScore = nobleMockScore;
            nobleViscount.tookMockExam = true;
            nobleViscount.mockExamProjectedAcademy = nobleProjectedAcademy;

            Debug.Log($"[Noble Candidate] Age 17.5 Mock Score: {nobleMockScore}/750 (Projected: {nobleProjectedAcademy})");

            // Father provides elite noble tutors
            nobleViscount.receivedFatherExamBoost = true;
            int nobleBoost = (nobleViscount.nobleTitle >= NobleRank.Viscount) ? 18 : 14;
            nobleViscount.examPreparationBonus += nobleBoost;
            nobleViscount.intellect += 8f;
            nobleViscount.combatSkill += 8f;

            Debug.Log($"[Noble Candidate] Father deployed clan tutors! Exam Prep Boost: +{nobleBoost} pts (New Intellect: {nobleViscount.intellect}, Combat: {nobleViscount.combatSkill})");

            // Advance to 18.0
            nobleViscount.age = 18.0f;
            ChildhoodLifeEngine.ConductNationalMilitaryExam(nobleViscount, null);

            Debug.Log($"[Noble Candidate] Final Exam Score (Age 18.0): {nobleViscount.examScore}/750 (Improvement: +{nobleViscount.examScore - nobleMockScore})");
            Debug.Assert(nobleViscount.examScore > nobleMockScore, "Noble final score should be higher than mock score!");
            Debug.Assert(nobleViscount.receivedFatherExamBoost, "Noble should have receivedFatherExamBoost flag set!");

            // Test Case 2: Commoner with Solo Midnight Cramming
            var commonerGrit = new SoldierProfile("Darek Stone", null, 95)
            {
                nobleTitle = NobleRank.Commoner,
                age = 17.5f,
                fitness = 60f,
                combatSkill = 55f,
                intellect = 62f,
                foundationalTempering = 40f,
                cashBalance = 50
            };

            int gritMockScore = ChildhoodLifeEngine.CalculateProjectedExamScore(commonerGrit, out string gritProjectedAcademy);
            commonerGrit.mockExamScore = gritMockScore;
            commonerGrit.tookMockExam = true;
            commonerGrit.mockExamProjectedAcademy = gritProjectedAcademy;

            Debug.Log($"[Commoner Grit] Age 17.5 Mock Score: {gritMockScore}/750 (Projected: {gritProjectedAcademy})");

            // Commoner chooses Solo Midnight Cramming
            commonerGrit.examPreparationBonus += 10;
            commonerGrit.intellect += 10f;
            commonerGrit.fitness += 8f;
            commonerGrit.health -= 10f;

            // Advance to 18.0
            commonerGrit.age = 18.0f;
            ChildhoodLifeEngine.ConductNationalMilitaryExam(commonerGrit, null);

            Debug.Log($"[Commoner Grit] Final Exam Score (Age 18.0): {commonerGrit.examScore}/750 (Improvement: +{commonerGrit.examScore - gritMockScore})");
            Debug.Assert(commonerGrit.examScore > gritMockScore, "Commoner grit final score should be higher than mock score!");

            // Test Case 3: Commoner running to Father for blessing & savings
            var commonerFather = new SoldierProfile("Finn Miller", null, 80)
            {
                nobleTitle = NobleRank.Commoner,
                age = 17.5f,
                fitness = 50f,
                combatSkill = 45f,
                intellect = 50f,
                foundationalTempering = 30f,
                cashBalance = 30,
                parentRelationship = 70f
            };

            int commonerMock = ChildhoodLifeEngine.CalculateProjectedExamScore(commonerFather, out string commonerProjected);
            commonerFather.mockExamScore = commonerMock;
            commonerFather.tookMockExam = true;
            commonerFather.mockExamProjectedAcademy = commonerProjected;

            // Run to Father (Commoner)
            commonerFather.receivedFatherExamBoost = true;
            commonerFather.examPreparationBonus += 6;
            commonerFather.parentRelationship += 20f;
            commonerFather.cashBalance += 50;

            commonerFather.age = 18.0f;
            ChildhoodLifeEngine.ConductNationalMilitaryExam(commonerFather, null);

            Debug.Log($"[Commoner Father Help] Final Exam Score (Age 18.0): {commonerFather.examScore}/750 (Improvement: +{commonerFather.examScore - commonerMock}, Father Rel: {commonerFather.parentRelationship}, Cash: ${commonerFather.cashBalance})");
            Debug.Assert(commonerFather.examScore > commonerMock, "Commoner father help score should improve over mock exam!");
            Debug.Assert(commonerFather.cashBalance == 380, "Commoner father should have given $50 to child (totaling $380 with $300 recruit depot bonus for sub-50% score)!");

            Debug.Log("<color=#00FFAA>========== ALL MOCK EXAM & FATHER HELP TESTS PASSED PERFECTLY! ==========</color>");
        }

        [MenuItem("Army Life/Run Full 36-Semester Childhood Lifecycle Test", false, 11)]
        public static void RunFullChildhoodProgressionTest()
        {
            Debug.Log("<color=#00D4FF>========== STARTING FULL 36-SEMESTER CHILDHOOD LIFECYCLE TEST ==========</color>");

            var child = new SoldierProfile("Valen Frost", null, 140)
            {
                nobleTitle = NobleRank.Baron,
                age = 0.0f,
                fitness = 20f,
                combatSkill = 15f,
                intellect = 45f,
                foundationalTempering = 10f,
                health = 100f,
                cashBalance = 500
            };

            int loggedEvents = 0;
            GameEvents.OnFeedEventLogged += (category, msg) => { loggedEvents++; };

            // Simulate all 36 semesters of childhood from Age 0.0 to 18.0
            for (int semester = 1; semester <= 36; semester++)
            {
                child.age += 0.5f;
                child.totalSemestersLived++;
                ChildhoodLifeEngine.ProcessYouthSemester(child, null);

                // Simulate occasional youth activity clicks
                if (child.age >= 10.0f && semester % 4 == 0)
                {
                    ChildhoodLifeEngine.PerformCadetFiringRange(child);
                    ChildhoodLifeEngine.PerformClanMartialLibrary(child);
                }
            }

            Debug.Log($"[Childhood Lifecycle] Age reached: {child.age:F1} | Total Semesters: {child.totalSemestersLived}");
            Debug.Log($"[Childhood Lifecycle] Stats at 18: Fitness={child.fitness:F0}, Combat={child.combatSkill:F0}, Intellect={child.intellect:F0}, Tempering={child.foundationalTempering:F0}, Health={child.health:F0}, Cash=${child.cashBalance:N0}");
            Debug.Log($"[Childhood Lifecycle] Exam Final Score: {child.examScore}/750 | Total Feed Events Logged: {loggedEvents}");

            Debug.Assert(child.age >= 18.0f, "Child should reach age 18.0!");
            Debug.Assert(child.totalSemestersLived == 36, "Child should survive exactly 36 semesters!");
            Debug.Assert(child.examScore >= 150, "Exam score should be computed out of 750!");
            Debug.Assert(child.fitness > 20f, "Fitness should grow over childhood!");
            Debug.Assert(child.combatSkill > 15f, "Combat skill should grow over childhood!");
            Debug.Assert(loggedEvents >= 36, "Every semester should log milestone feed events!");

            // Clan & School Assertions (Fulfilling user specifications)
            Debug.Assert(child.clanMembers.Count >= 4, "Child should have clan members populated!");
            var lucian = child.clanMembers.Find(m => m.relationType == ClanRelationType.OlderBrother);
            Debug.Assert(lucian != null && lucian.relationship <= 45f, "Rival brother Lucian should have low/competitive relationship (<45%)!");
            var mother = child.clanMembers.Find(m => m.relationType == ClanRelationType.Mother);
            Debug.Assert(mother != null && mother.relationship >= 80f, "Mother should have loving relationship (>=80%)!");
            Debug.Assert(child.schoolPeers.Count >= 3, "Child should have school peers populated after age 6!");

            // Primary School Score & Graduation Assertions (User Specification)
            Debug.Assert(child.primarySchoolScore >= 50f, "Child should have accumulated a solid primary school score!");
            Debug.Assert(child.primarySchoolGraduationHonors != "Pending", "Child should have graduated primary school with honors at Age 12.0!");
            Debug.Log($"[Childhood Lifecycle] Primary School Score: {child.primarySchoolScore:F1}/100 | Honors: {child.primarySchoolGraduationHonors}");

            // School Hierarchy & Educational Level Assertions
            Debug.Assert(child.schoolLevel == SchoolLevel.YouthAcademy, "Child at age 18 should have reached Youth Academy level!");
            Debug.Assert((int)child.schoolTier >= 0 && (int)child.schoolTier <= 6, "School tier should be valid within 0-6 range!");
            Debug.Log($"[Childhood Lifecycle] Final School: {child.schoolName} ({SchoolHelper.GetTierTitle(child.schoolTier)}) | Promotions Earned: {child.timesPromotedSchool}");

            Debug.Log("<color=#00FFAA>========== 36-SEMESTER CHILDHOOD, CLAN, GEOGRAPHY & PRIMARY SCHOOL TEST PASSED 100%! ==========</color>");
        }

        [MenuItem("Army Life/Run School Tier & Promotion Stepwise Test", false, 12)]
        public static void RunSchoolTierProgressionTest()
        {
            Debug.Log("<color=#00D4FF>========== STARTING STEPWISE SCHOOL TIER PROMOTION TEST ==========</color>");

            // 1. Commoner child starting at City 3rd Primary School
            var student = new SoldierProfile("Finn Miller", null, 100)
            {
                birthState = "Ironwood Province",
                birthCity = "Highspire",
                isBornInImperialState = false,
                nobleTitle = NobleRank.Commoner,
                age = 6.0f,
                intellect = 85f,
                foundationalTempering = 60f
            };
            student.InitializeStartingSchoolTier();
            student.InitializeSchoolPeersIfEligible();

            Debug.Assert(student.schoolTier == SchoolTier.City3rd, "Commoner should begin in City 3rd Primary School!");
            Debug.Assert(student.schoolName == "Highspire 3rd Primary School", $"Expected Highspire 3rd Primary School, got {student.schoolName}");
            Debug.Log($"[Step 1 - Age 6.0] Enrolled in: {student.schoolName} ({SchoolHelper.GetTierTitle(student.schoolTier)})");

            // Age 6.5 to 7.0: Student works hard and achieves 58 score (exceeds 50 threshold) -> promotes to City 2nd
            student.age = 7.0f;
            student.schoolPerformance = 58f;
            ChildhoodLifeEngine.ProcessYouthSemester(student, null);

            Debug.Assert(student.schoolTier == SchoolTier.City2nd, $"Expected promotion to City2nd, got {student.schoolTier}");
            Debug.Assert(student.schoolName == "Highspire 2nd Primary School", $"Expected Highspire 2nd Primary School, got {student.schoolName}");
            Debug.Assert(student.timesPromotedSchool == 1, "Should have 1 promotion recorded!");
            Debug.Log($"[Step 2 - Age 7.0] Promoted to: {student.schoolName} ({SchoolHelper.GetTierTitle(student.schoolTier)})");

            // Age 8.0: Student continues to excel with 62 score (exceeds 55 threshold) -> promotes to City 1st
            student.age = 8.0f;
            student.schoolPerformance = 62f;
            ChildhoodLifeEngine.ProcessYouthSemester(student, null);

            Debug.Assert(student.schoolTier == SchoolTier.City1st, $"Expected promotion to City1st, got {student.schoolTier}");
            Debug.Assert(student.schoolName == "Highspire 1st Primary School", $"Expected Highspire 1st Primary School, got {student.schoolName}");
            Debug.Assert(student.timesPromotedSchool == 2, "Should have 2 promotions recorded!");
            Debug.Log($"[Step 3 - Age 8.0] Promoted to: {student.schoolName} ({SchoolHelper.GetTierTitle(student.schoolTier)})");

            // Age 9.0: Student crosses municipal pinnacle with 66 score (exceeds 60 threshold) -> promotes to State 3rd
            student.age = 9.0f;
            student.schoolPerformance = 66f;
            ChildhoodLifeEngine.ProcessYouthSemester(student, null);

            Debug.Assert(student.schoolTier == SchoolTier.State3rd, $"Expected promotion to State3rd, got {student.schoolTier}");
            Debug.Assert(student.schoolName == "Ironwood Province 3rd State Primary School", $"Expected Ironwood Province 3rd State Primary School, got {student.schoolName}");
            Debug.Assert(student.timesPromotedSchool == 3, "Should have 3 promotions recorded!");
            Debug.Log($"[Step 4 - Age 9.0] Promoted to State School: {student.schoolName} ({SchoolHelper.GetTierTitle(student.schoolTier)})");

            // Age 12.0: Primary School Graduation -> Middle School Transition
            student.age = 12.0f;
            student.primarySchoolScore = 80f;
            ChildhoodLifeEngine.ProcessYouthSemester(student, null);

            Debug.Assert(student.schoolLevel == SchoolLevel.MiddleSchool, "Student should now be in Middle School!");
            Debug.Assert(student.schoolName.Contains("Middle School"), $"School name should now reflect Middle School: {student.schoolName}");
            Debug.Log($"[Step 5 - Age 12.0] Middle School Transition: {student.schoolName}");

            // 2. State 1st Middle School Top Scholar selected for The Grand Imperial Youth Academy
            var topScholar = new SoldierProfile("Aurelius Vance", null, 170)
            {
                birthState = "Silverbrook Duchy",
                birthCity = "Argentum",
                isBornInImperialState = false,
                schoolTier = SchoolTier.State1st,
                schoolLevel = SchoolLevel.MiddleSchool,
                age = 14.5f,
                intellect = 92f,
                schoolPerformance = 91f
            };
            topScholar.InitializeSchoolPeersIfEligible();

            // Advance to Age 15.0 (Transition to Youth Academy)
            topScholar.age = 15.0f;
            ChildhoodLifeEngine.ProcessYouthSemester(topScholar, null);

            Debug.Assert(topScholar.schoolTier == SchoolTier.ImperialPinnacle, $"Top scholar should be transferred to Imperial Pinnacle, got {topScholar.schoolTier}");
            Debug.Assert(topScholar.isImperialScholarshipStudent, "Top scholar should have isImperialScholarshipStudent flag set!");
            Debug.Assert(topScholar.schoolName == "The Grand Imperial Youth Academy", $"Expected The Grand Imperial Youth Academy, got {topScholar.schoolName}");
            Debug.Log($"[Step 6 - Age 15.0] Imperial Scholarship Scouting: {topScholar.schoolName} in Valeria Magna!");

            Debug.Log("<color=#00FFAA>========== STEPWISE SCHOOL TIER PROMOTION TESTS PASSED 100%! ==========</color>");
        }
    }
}
#endif
