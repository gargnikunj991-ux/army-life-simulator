#if UNITY_EDITOR
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArmyLife.Editor
{
    public static class SceneSetupWizard
    {
        [MenuItem("Army Life/1-Click Setup Complete Playable Scene", false, 0)]
        public static void SetupCompletePlayableScene()
        {
            // First ensure game data is generated
            GameSetupWizard.GenerateAllDefaultData();

            // Create a brand new Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.16f); // Tactical Dark Slate
            camObj.transform.position = new Vector3(0, 0, -10);

            // 2. Event System (Required for UI Clicks)
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<EventSystem>();
            var newModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newModuleType != null)
            {
                eventObj.AddComponent(newModuleType);
            }
            else
            {
                eventObj.AddComponent<StandaloneInputModule>();
            }

            // 3. Core Game Controllers
            var coreObj = new GameObject("[Core Controllers]");
            var gm = coreObj.AddComponent<GameManager>();
            var timeManager = coreObj.AddComponent<TimeCycleManager>();

            // Auto-populate Game Data into GameManager
            gm.powerTierDatabase = LoadAllAssets<PowerTierSO>("Assets/GameData/PowerTiers");
            gm.militaryRankDatabase = LoadAllAssets<MilitaryRankSO>("Assets/GameData/Ranks");
            gm.familyLineageDatabase = LoadAllAssets<FamilyLineageSO>("Assets/GameData/Lineages");

            // 4. Canvas
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Mobile/Bitlife vertical layout
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 5. Root UI Panel
            var rootPanel = CreateUIElement("RootPanel", canvasObj.transform);
            SetFullScreen(rootPanel.GetComponent<RectTransform>());
            var rootBg = rootPanel.AddComponent<Image>();
            rootBg.color = new Color(0.08f, 0.10f, 0.12f); // Deep tactical background

            // Vertical Layout for UI
            var hudView = rootPanel.AddComponent<MainHUDView>();

            // --- HEADER PANEL ---
            var headerPanel = CreateUIElement("HeaderPanel", rootPanel.transform);
            var headerRect = headerPanel.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.78f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(40, 20);
            headerRect.offsetMax = new Vector2(-40, -20);
            var headerBg = headerPanel.AddComponent<Image>();
            headerBg.color = new Color(0.15f, 0.18f, 0.22f);

            var nameText = CreateText("NameLabel", headerPanel.transform, "Ethan Vance", 48, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            SetRect(nameText.GetComponent<RectTransform>(), 0.05f, 0.65f, 0.65f, 0.95f);
            hudView.nameLabel = nameText;

            var nobleText = CreateText("NobleTitleLabel", headerPanel.transform, "Commoner", 36, TextAlignmentOptions.TopRight, FontStyles.Bold, new Color(1f, 0.85f, 0.3f));
            SetRect(nobleText.GetComponent<RectTransform>(), 0.65f, 0.65f, 0.95f, 0.95f);
            hudView.nobleTitleLabel = nobleText;

            var ageDateText = CreateText("AgeDateLabel", headerPanel.transform, "Age 18.0 | Year 2026 S1", 32, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.8f, 0.85f, 0.9f));
            SetRect(ageDateText.GetComponent<RectTransform>(), 0.05f, 0.35f, 0.55f, 0.65f);
            hudView.ageAndCalendarLabel = ageDateText;

            var rankText = CreateText("RankLabel", headerPanel.transform, "Private Recruit (E-1)", 34, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.4f, 0.8f, 1f));
            SetRect(rankText.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.55f, 0.35f);
            hudView.rankLabel = rankText;

            var payText = CreateText("PayLabel", headerPanel.transform, "+$1,950 / mo", 32, TextAlignmentOptions.Right, FontStyles.Bold, new Color(0.3f, 1f, 0.5f));
            SetRect(payText.GetComponent<RectTransform>(), 0.55f, 0.05f, 0.95f, 0.35f);
            hudView.monthlyPayLabel = payText;

            // --- FINANCES & CULTIVATION BAR ---
            var subHeader = CreateUIElement("SubHeader", rootPanel.transform);
            var subRect = subHeader.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0, 0.68f);
            subRect.anchorMax = new Vector2(1, 0.77f);
            subRect.offsetMin = new Vector2(40, 10);
            subRect.offsetMax = new Vector2(-40, -10);
            var subBg = subHeader.AddComponent<Image>();
            subBg.color = new Color(0.13f, 0.15f, 0.18f);

            var cashText = CreateText("CashLabel", subHeader.transform, "$2,000", 36, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.4f, 1f, 0.6f));
            SetRect(cashText.GetComponent<RectTransform>(), 0.05f, 0.5f, 0.45f, 0.95f);
            hudView.cashBalanceLabel = cashText;

            var cmpText = CreateText("CMPLabel", subHeader.transform, "0 CMP", 34, TextAlignmentOptions.Right, FontStyles.Bold, new Color(0.9f, 0.5f, 1f));
            SetRect(cmpText.GetComponent<RectTransform>(), 0.55f, 0.5f, 0.95f, 0.95f);
            hudView.cmpBalanceLabel = cmpText;

            var tierText = CreateText("TierLabel", subHeader.transform, "Power Tier: 0 / 12", 30, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetRect(tierText.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.50f, 0.48f);
            hudView.powerTierLabel = tierText;

            var talentText = CreateText("TalentLabel", subHeader.transform, "Talent: [??? / 200] (Requires Rank E-4)", 28, TextAlignmentOptions.Right, FontStyles.Normal, new Color(1f, 0.8f, 0.4f));
            SetRect(talentText.GetComponent<RectTransform>(), 0.50f, 0.05f, 0.95f, 0.48f);
            hudView.talentLabel = talentText;

            // --- BITLIFE FEED CONTAINER ---
            var feedContainer = CreateUIElement("FeedContainer", rootPanel.transform);
            var feedRect = feedContainer.GetComponent<RectTransform>();
            feedRect.anchorMin = new Vector2(0, 0.18f);
            feedRect.anchorMax = new Vector2(1, 0.67f);
            feedRect.offsetMin = new Vector2(40, 10);
            feedRect.offsetMax = new Vector2(-40, -10);
            var feedBg = feedContainer.AddComponent<Image>();
            feedBg.color = new Color(0.10f, 0.12f, 0.14f);

            var scrollRect = feedContainer.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var feedTextObj = CreateText("FeedContent", feedContainer.transform, "Welcome to the Valerian Empire...\n", 32, TextAlignmentOptions.TopLeft, FontStyles.Normal, Color.white);
            var feedTextRect = feedTextObj.GetComponent<RectTransform>();
            SetFullScreen(feedTextRect);
            scrollRect.content = feedTextRect;

            var feedView = feedContainer.AddComponent<BitLifeFeedView>();
            feedView.feedContentText = feedTextObj;
            feedView.feedScrollRect = scrollRect;

            // --- BOTTOM ACTION BUTTONS ---
            var bottomBar = CreateUIElement("BottomBar", rootPanel.transform);
            var bottomRect = bottomBar.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0.02f);
            bottomRect.anchorMax = new Vector2(1, 0.16f);
            bottomRect.offsetMin = new Vector2(40, 10);
            bottomRect.offsetMax = new Vector2(-40, -10);

            // Advance Time Button (Big Green Bitlife '+' Button)
            var advanceBtnObj = CreateButton("AdvanceSemesterButton", bottomBar.transform, "ADVANCE (+)", new Color(0.18f, 0.65f, 0.35f));
            var advRect = advanceBtnObj.GetComponent<RectTransform>();
            SetRect(advRect, 0.04f, 0.15f, 0.42f, 0.85f);
            hudView.advanceSemesterButton = advanceBtnObj.GetComponent<Button>();

            // Send Money Home Button
            var sendBtnObj = CreateButton("SendMoneyHomeButton", bottomBar.transform, "SEND $400 HOME", new Color(0.85f, 0.55f, 0.20f));
            var sendRect = sendBtnObj.GetComponent<RectTransform>();
            SetRect(sendRect, 0.45f, 0.15f, 0.72f, 0.85f);
            hudView.sendMoneyHomeButton = sendBtnObj.GetComponent<Button>();

            // Test Talent Button (Appears at E-4)
            var testBtnObj = CreateButton("TestTalentButton", bottomBar.transform, "TEST (E-4)", new Color(0.25f, 0.50f, 0.85f));
            var testRect = testBtnObj.GetComponent<RectTransform>();
            SetRect(testRect, 0.75f, 0.15f, 0.96f, 0.85f);
            hudView.testTalentButton = testBtnObj.GetComponent<Button>();

            // Save Scene
            string scenePath = "Assets/Scenes/MainGame.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Army Life Simulator", 
                "Playable Scene created successfully at Assets/Scenes/MainGame.unity!\n\nJust click PLAY (▶) in Unity to test the game!", 
                "Let's Play!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, TextAlignmentOptions align, FontStyles style, Color? col = null)
        {
            var obj = CreateUIElement(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.fontStyle = style;
            tmp.color = col ?? Color.white;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Color color)
        {
            var btnObj = CreateUIElement(name, parent);
            var img = btnObj.AddComponent<Image>();
            img.color = color;
            var btn = btnObj.AddComponent<Button>();

            var text = CreateText("Label", btnObj.transform, label, 30, TextAlignmentOptions.Center, FontStyles.Bold);
            SetFullScreen(text.GetComponent<RectTransform>());

            return btnObj;
        }

        private static void SetFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static T[] LoadAllAssets<T>(string folder) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            var list = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                list[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return list;
        }
    }
}
#endif
