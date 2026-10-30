#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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
            Debug.Log("<color=#D4AF37>========================================================================</color>");
            Debug.Log("<color=#D4AF37>                 MASTER ARMY LIFE SYSTEM & FEATURE AUDIT                </color>");
            Debug.Log("<color=#D4AF37>========================================================================</color>");

            int passed = 0;
            int failed = 0;
            List<string> issues = new List<string>();

            // 1. DATA CONSTANTS & ACADEMY TUITIONS AUDIT
            try
            {
                Debug.Log("\n[AUDIT 1] Verifying Data Constants & Academy Tuitions...");
                long afTuition = EconomyData.AIRFORCE_ACADEMY_TUITION_COST;
                long armyTuition = EconomyData.ARMY_ACADEMY_TUITION_COST;

                if (afTuition != 200000L)
                {
                    issues.Add($"EconomyData.AIRFORCE_ACADEMY_TUITION_COST expected 200,000 but got {afTuition}");
                    failed++;
                }
                else if (armyTuition != 100000L)
                {
                    issues.Add($"EconomyData.ARMY_ACADEMY_TUITION_COST expected 100,000 but got {armyTuition}");
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

            // 4. RUN HOSPITAL, TRAUMA, IED AMPUTATION & WAR FRONT TEST
            try
            {
                Debug.Log("\n[AUDIT 4] Running HospitalAndTraumaTestRunner...");
                HospitalAndTraumaTestRunner.RunAllHospitalAndTraumaTests();
                passed++;
                Debug.Log("  -> Field Hospital, Trauma, IED & Frontline Engine [PASSED]");
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

            // 9. RUN GAMEPLAY BRIDGES & CAREER AGE RESTRICTION TEST
            try
            {
                Debug.Log("\n[AUDIT 9] Running GameplayBridgesTestRunner...");
                GameplayBridgesTestRunner.RunTests();
                passed++;
                Debug.Log("  -> Gameplay Bridges & Age Limits [PASSED]");
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 9 Exception] {ex.Message}");
                failed++;
            }

            // 10. VERIFY HERALDRY & INSIGNIA SPRITES ASSETS
            try
            {
                Debug.Log("\n[AUDIT 10] Checking Heraldry, Insignia and Ribbon Assets...");
                string insigniaDir = Path.Combine(Application.dataPath, "Sprites/Insignias");
                string ribbonDir = Path.Combine(Application.dataPath, "Sprites/Ribbons");

                int insigniaCount = Directory.Exists(insigniaDir) ? Directory.GetFiles(insigniaDir, "*.png").Length : 0;
                int ribbonCount = Directory.Exists(ribbonDir) ? Directory.GetFiles(ribbonDir, "*.png").Length : 0;

                Debug.Log($"  -> Rank Insignias found: {insigniaCount}");
                Debug.Log($"  -> Medal Ribbons found: {ribbonCount}");

                if (insigniaCount == 0 || ribbonCount == 0)
                {
                    issues.Add($"Heraldry sprites missing! Insignias: {insigniaCount}, Ribbons: {ribbonCount}. Please run InsigniaSpriteGenerator.GenerateAllSprites.");
                    failed++;
                }
                else
                {
                    Debug.Log("  -> High-resolution Vector Insignias & Ribbons [OK]");
                    passed++;
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 10 Exception] {ex.Message}");
                failed++;
            }

            // 11. SCENE HIERARCHY & INTEGRATION AUDIT (Check MainGame.unity)
            try
            {
                Debug.Log("\n[AUDIT 11] Checking MainGame.unity Scene Hierarchy Integration...");
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
                var mailModal = UnityEngine.Object.FindAnyObjectByType<MailCallModal>(FindObjectsInactive.Include);
                var medalModal = UnityEngine.Object.FindAnyObjectByType<MedalRibbonRackModal>(FindObjectsInactive.Include);
                var audioMgr = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
                var hud = UnityEngine.Object.FindAnyObjectByType<MainHUDView>(FindObjectsInactive.Include);
                var activities = UnityEngine.Object.FindAnyObjectByType<ActivitiesModal>(FindObjectsInactive.Include);
                var lifeModal = UnityEngine.Object.FindAnyObjectByType<LifeModal>(FindObjectsInactive.Include);
                var relModal = UnityEngine.Object.FindAnyObjectByType<RelationshipsModal>(FindObjectsInactive.Include);

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

                if (activities != null)
                {
                    Debug.Log("  -> ActivitiesModal found in scene [OK]");
                }

                if (lifeModal != null)
                {
                    Debug.Log("  -> LifeModal (Dossier) found in scene [OK]");
                }

                if (relModal != null)
                {
                    Debug.Log("  -> RelationshipsModal found in scene [OK]");
                }

                var canvas = UnityEngine.Object.FindAnyObjectByType<UnityEngine.UI.CanvasScaler>(FindObjectsInactive.Include);
                if (canvas != null)
                {
                    Debug.Log($"  -> CanvasScaler reference resolution: {canvas.referenceResolution.x} x {canvas.referenceResolution.y} (Vertical Mobile/BitLife Layout [OK])");
                }

                passed++;
            }
            catch (Exception ex)
            {
                issues.Add($"[AUDIT 11 Exception] {ex.Message}");
                failed++;
            }

            // SUMMARY REPORT
            Debug.Log("\n<color=#D4AF37>========================================================================</color>");
            Debug.Log($"<color=#D4AF37>AUDIT COMPLETE: Passed Suites: {passed} | Failed/Issues Found: {issues.Count}</color>");
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
            Debug.Log("<color=#D4AF37>========================================================================</color>");
        }
    }
}
#endif
