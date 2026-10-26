#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using ArmyLife.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArmyLife.Editor
{
    public static class MasterSystemAuditRunner
    {
        [MenuItem("Army Life/Run Full Master System Audit", false, 99)]
        public static void RunMasterAudit()
        {
            Debug.Log("<color=#00D4FF>========================================================================</color>");
            Debug.Log("<color=#00D4FF>                 MASTER ARMY LIFE SYSTEM & FEATURE AUDIT                </color>");
            Debug.Log("<color=#00D4FF>========================================================================</color>");

            int passed = 0;
            int failed = 0;
            List<string> issues = new List<string>();

            // 1. DATA CONSTANTS & ACADEMY TUITIONS AUDIT
            try
            {
                Debug.Log("\n[AUDIT 1] Verifying Data Constants & Academy Tuitions...");
                if (EconomyData.AIRFORCE_ACADEMY_TUITION_COST != 200000L)
                {
                    issues.Add($"EconomyData.AIRFORCE_ACADEMY_TUITION_COST expected 200,000 but got {EconomyData.AIRFORCE_ACADEMY_TUITION_COST}");
                    failed++;
                }
                else if (EconomyData.ARMY_ACADEMY_TUITION_COST != 100000L)
                {
                    issues.Add($"EconomyData.ARMY_ACADEMY_TUITION_COST expected 100,000 but got {EconomyData.ARMY_ACADEMY_TUITION_COST}");
                    failed++;
                }
                else
                {
                    Debug.Log("  -> Air Force Academy tuition: $200,000 [VERIFIED]");
                    Debug.Log("  -> Army War Academy tuition: $100,000 [VERIFIED]");
                    passed++;
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 1 Exception] {ex.Message}");
                failed++;
            }

            // 2. RUN SQUADMATES & COMMAND TEST
            try
            {
                Debug.Log("\n[AUDIT 2] Running SquadmateCommandTestRunner...");
                SquadmateCommandTestRunner.RunSquadmatesAndCommandTest();
                passed++;
                Debug.Log("  -> Squadmates & Command Engine [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 2 Exception] {ex.Message}");
                failed++;
            }

            // 3. RUN COMBAT OPERATIONS TEST
            try
            {
                Debug.Log("\n[AUDIT 3] Running CombatOperationsTestRunner...");
                CombatOperationsTestRunner.RunCombatOperationsTests();
                passed++;
                Debug.Log("  -> Combat Operations Engine [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 3 Exception] {ex.Message}");
                failed++;
            }

            // 4. RUN TACTICAL MINIGAMES TEST
            try
            {
                Debug.Log("\n[AUDIT 4] Running MinigamesTestRunner...");
                MinigamesTestRunner.RunAllMinigamesTests();
                passed++;
                Debug.Log("  -> Tactical Minigames (Firing Range, Checkpoint, Dice) [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 4 Exception] {ex.Message}");
                failed++;
            }

            // 5. RUN MAIL CALL TEST
            try
            {
                Debug.Log("\n[AUDIT 5] Running MailCallTestRunner...");
                MailCallTestRunner.RunAllMailCallTests();
                passed++;
                Debug.Log("  -> Mail Call & Homefront Engine [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 5 Exception] {ex.Message}");
                failed++;
            }

            // 6. RUN MEDAL RACK TEST
            try
            {
                Debug.Log("\n[AUDIT 6] Running MedalRackTestRunner...");
                MedalRackTestRunner.RunAllTests();
                passed++;
                Debug.Log("  -> Medal Rack & Ribbon Engine [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 6 Exception] {ex.Message}");
                failed++;
            }

            // 7. RUN ECONOMIC DILEMMAS TEST
            try
            {
                Debug.Log("\n[AUDIT 7] Running EconomicDilemmaTestRunner...");
                EconomicDilemmaTestRunner.RunEconomicDilemmasTest();
                passed++;
                Debug.Log("  -> Economic Dilemmas Deck [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 7 Exception] {ex.Message}");
                failed++;
            }

            // 8. RUN WARTIME MARKET TEST
            try
            {
                Debug.Log("\n[AUDIT 8] Running WartimeMarketTestRunner...");
                WartimeMarketTestRunner.RunWartimeMarketTest();
                passed++;
                Debug.Log("  -> Wartime Market & Macro Shocks [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 8 Exception] {ex.Message}");
                failed++;
            }

            // 9. RUN CIVIL & MILITARY CAREER SYSTEM TEST
            try
            {
                Debug.Log("\n[AUDIT 9] Running MockExamTestRunner.RunCivilAndMilitaryCareerSystemTest...");
                MockExamTestRunner.RunCivilAndMilitaryCareerSystemTest();
                passed++;
                Debug.Log("  -> Civil & Military Career Tracks [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 9 Exception] {ex.Message}");
                failed++;
            }

            // 10. RUN SPECIAL FORCES SELECTION ATTRITION & PRESTIGE TEST
            try
            {
                Debug.Log("\n[AUDIT 10] Running MockExamTestRunner.RunSpecialForcesSelectionTrialAndPrestigeTest...");
                MockExamTestRunner.RunSpecialForcesSelectionTrialAndPrestigeTest();
                passed++;
                Debug.Log("  -> Special Forces Selection Attrition & Prestige [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 10 Exception] {ex.Message}");
                failed++;
            }

            // 11. RUN AUDIO ENGINE & SOUNDSCAPE TEST
            try
            {
                Debug.Log("\n[AUDIT 11] Running MockExamTestRunner.RunAudioEngineAndSoundscapeTest...");
                MockExamTestRunner.RunAudioEngineAndSoundscapeTest();
                passed++;
                Debug.Log("  -> Audio Engine & Procedural Soundscape [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 11 Exception] {ex.Message}");
                failed++;
            }

            // 12. SCENE HIERARCHY & INTEGRATION AUDIT (Check MainGame.unity)
            try
            {
                Debug.Log("\n[AUDIT 12] Checking MainGame.unity Scene Hierarchy Integration...");
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
                var minigamesModal = UnityEngine.Object.FindAnyObjectByType<TacticalMinigamesModal>(FindObjectsInactive.Include);
                var mailModal = UnityEngine.Object.FindAnyObjectByType<MailCallModal>(FindObjectsInactive.Include);
                var medalModal = UnityEngine.Object.FindAnyObjectByType<MedalRibbonRackModal>(FindObjectsInactive.Include);
                var audioMgr = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
                var hud = UnityEngine.Object.FindAnyObjectByType<MainHUDView>(FindObjectsInactive.Include);

                if (minigamesModal == null)
                {
                    issues.Add("[SCENE DEFECT] TacticalMinigamesModal is MISSING from MainGame.unity scene hierarchy!");
                }
                else
                {
                    Debug.Log("  -> TacticalMinigamesModal found in scene [OK]");
                }

                if (mailModal == null)
                {
                    issues.Add("[SCENE DEFECT] MailCallModal is MISSING from MainGame.unity scene hierarchy!");
                }
                else
                {
                    Debug.Log("  -> MailCallModal found in scene [OK]");
                }

                if (medalModal == null)
                {
                    issues.Add("[SCENE DEFECT] MedalRibbonRackModal is MISSING from MainGame.unity scene hierarchy!");
                }
                else
                {
                    Debug.Log("  -> MedalRibbonRackModal found in scene [OK]");
                }

                if (audioMgr != null)
                {
                    Debug.Log("  -> AudioManager found in scene [OK]");
                }

                var canvas = UnityEngine.Object.FindAnyObjectByType<UnityEngine.UI.CanvasScaler>(FindObjectsInactive.Include);
                if (canvas != null)
                {
                    Debug.Log($"  -> CanvasScaler reference resolution: {canvas.referenceResolution.x} x {canvas.referenceResolution.y} (Vertical Mobile/BitLife Layout [OK])");
                }

                // Check Activities connection to Minigames
                var activities = UnityEngine.Object.FindAnyObjectByType<ActivitiesModal>(FindObjectsInactive.Include);
                if (activities != null)
                {
                    Debug.Log("  -> ActivitiesModal found in scene [OK]");
                }

                passed++;
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 12 Exception] {ex.Message}");
                failed++;
            }

            // SUMMARY REPORT
            Debug.Log("\n<color=#00D4FF>========================================================================</color>");
            Debug.Log($"<color=#00D4FF>AUDIT COMPLETE: Passed Suites: {passed} | Failed/Issues Found: {issues.Count}</color>");
            if (issues.Count > 0)
            {
                Debug.Log("<color=#FF4444>FOUND DEFECTS / INCONSISTENCIES:</color>");
                for (int i = 0; i < issues.Count; i++)
                {
                    Debug.Log($"  {i + 1}. <color=#FF8866>{issues[i]}</color>");
                }
            }
            else
            {
                Debug.Log("<color=#00FFAA>ALL SYSTEMS CLEAN WITH ZERO DEFECTS!</color>");
            }
            Debug.Log("<color=#00D4FF>========================================================================</color>");
        }
    }
}
#endif
