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

            Debug.Log($"[Noble Candidate] Age 17.5 Mock Score: {nobleMockScore}/100 (Projected: {nobleProjectedAcademy})");

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

            Debug.Log($"[Noble Candidate] Final Exam Score (Age 18.0): {nobleViscount.examScore}/100 (Improvement: +{nobleViscount.examScore - nobleMockScore})");
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

            Debug.Log($"[Commoner Grit] Age 17.5 Mock Score: {gritMockScore}/100 (Projected: {gritProjectedAcademy})");

            // Commoner chooses Solo Midnight Cramming
            commonerGrit.examPreparationBonus += 10;
            commonerGrit.intellect += 10f;
            commonerGrit.fitness += 8f;
            commonerGrit.health -= 10f;

            // Advance to 18.0
            commonerGrit.age = 18.0f;
            ChildhoodLifeEngine.ConductNationalMilitaryExam(commonerGrit, null);

            Debug.Log($"[Commoner Grit] Final Exam Score (Age 18.0): {commonerGrit.examScore}/100 (Improvement: +{commonerGrit.examScore - gritMockScore})");
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

            Debug.Log($"[Commoner Father Help] Final Exam Score (Age 18.0): {commonerFather.examScore}/100 (Improvement: +{commonerFather.examScore - commonerMock}, Father Rel: {commonerFather.parentRelationship}, Cash: ${commonerFather.cashBalance})");
            Debug.Assert(commonerFather.examScore > commonerMock, "Commoner father help score should improve over mock exam!");
            Debug.Assert(commonerFather.cashBalance == 80, "Commoner father should have given $50 to child!");

            Debug.Log("<color=#00FFAA>========== ALL MOCK EXAM & FATHER HELP TESTS PASSED PERFECTLY! ==========</color>");
        }
    }
}
#endif
