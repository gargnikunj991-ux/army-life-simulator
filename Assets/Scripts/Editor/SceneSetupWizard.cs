#if UNITY_EDITOR
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
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
        public static void BuildFullProjectScene()
        {
            SetupCompletePlayableScene();
        }

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
            cam.backgroundColor = new Color(0.08f, 0.10f, 0.12f); // Tactical Dark Slate
            camObj.transform.position = new Vector3(0, 0, -10);
            camObj.AddComponent<AudioListener>(); // Required for Unity audio output to speakers/headphones

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
            var themeManager = coreObj.AddComponent<ThemeManager>();
            var audioManager = coreObj.AddComponent<AudioManager>();

            // Auto-populate Game Data into GameManager
            gm.militaryRankDatabase = LoadAllAssets<MilitaryRankSO>("Assets/GameData/Ranks");
            gm.familyLineageDatabase = LoadAllAssets<FamilyLineageSO>("Assets/GameData/Lineages");

            // 4. Canvas
            // 4. Canvas (16:9 PC Widescreen Tactical Command Layout)
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // 16:9 PC Native Widescreen
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Controller for Opening Scene & Menus
            var openingCtrl = canvasObj.AddComponent<OpeningSceneController>();

            // Root Canvas Panel
            var rootPanel = CreateUIElement("RootPanel", canvasObj.transform);
            SetFullScreen(rootPanel.GetComponent<RectTransform>());

            // =========================================================================
            // 5. MAIN HUD PANEL (16:9 PC TACTICAL WAR ROOM - 3-COLUMN COMMAND DESK)
            // =========================================================================
            var mainHUDPanel = CreateUIElement("MainHUDPanel", rootPanel.transform);
            SetFullScreen(mainHUDPanel.GetComponent<RectTransform>());
            var hudView = mainHUDPanel.AddComponent<MainHUDView>();
            openingCtrl.mainHUDPanel = mainHUDPanel;

            // --- TOP COMMAND BAR (Full Width: y: 0.93 to 1.0) ---
            var topBar = CreateUIElement("TopCommandBar", mainHUDPanel.transform);
            SetRect(topBar.GetComponent<RectTransform>(), 0.01f, 0.93f, 0.99f, 0.995f);
            var topBg = topBar.AddComponent<Image>();
            topBg.color = new Color(0.12f, 0.15f, 0.19f);
            hudView.headerPanelImage = topBg;

            var titleHeader = CreateText("HQHeaderLabel", topBar.transform, "ARMY LIFE SIMULATOR | TACTICAL COMMAND WAR ROOM", 20, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.95f, 0.85f, 0.35f));
            SetRect(titleHeader.GetComponent<RectTransform>(), 0.02f, 0.15f, 0.40f, 0.85f);

            var ageDateText = CreateText("AgeDateLabel", topBar.transform, "Age 18.0 | Year 2026 S1 [GARRISON BASE]", 20, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.85f, 0.90f, 0.95f));
            SetRect(ageDateText.GetComponent<RectTransform>(), 0.40f, 0.15f, 0.72f, 0.85f);
            hudView.ageAndCalendarLabel = ageDateText;

            var settingsBtnObj = CreateButton("SettingsButton", topBar.transform, "SETTINGS", new Color(0.20f, 0.28f, 0.38f));
            SetRect(settingsBtnObj.GetComponent<RectTransform>(), 0.75f, 0.15f, 0.86f, 0.85f);
            hudView.settingsButton = settingsBtnObj.GetComponent<Button>();

            var menuBtnObj = CreateButton("MenuButton", topBar.transform, "MAIN MENU", new Color(0.28f, 0.36f, 0.46f));
            SetRect(menuBtnObj.GetComponent<RectTransform>(), 0.875f, 0.15f, 0.985f, 0.85f);
            hudView.returnToMenuButton = menuBtnObj.GetComponent<Button>();
            openingCtrl.returnToMenuButton = menuBtnObj.GetComponent<Button>();

            // =========================================================================
            // COLUMN 1: LEFT PANE — SOLDIER DOSSIER & ATTRIBUTES (x: 0.01 to 0.27)
            // =========================================================================
            var leftCol = CreateUIElement("LeftDossierColumn", mainHUDPanel.transform);
            SetRect(leftCol.GetComponent<RectTransform>(), 0.01f, 0.015f, 0.27f, 0.92f);
            var leftBg = leftCol.AddComponent<Image>();
            leftBg.color = new Color(0.10f, 0.12f, 0.16f);
            hudView.subHeaderPanelImage = leftBg;

            // Insignia / Photo card
            var photoBox = CreateUIElement("InsigniaBox", leftCol.transform);
            SetRect(photoBox.GetComponent<RectTransform>(), 0.04f, 0.84f, 0.96f, 0.98f);
            var photoImg = photoBox.AddComponent<Image>();
            photoImg.color = new Color(0.14f, 0.17f, 0.22f);

            var insigniaImgObj = CreateUIElement("RankInsignia", photoBox.transform);
            SetRect(insigniaImgObj.GetComponent<RectTransform>(), 0.03f, 0.10f, 0.24f, 0.90f);
            var rankImg = insigniaImgObj.AddComponent<Image>();
            rankImg.preserveAspect = true;
            hudView.rankInsigniaImage = rankImg;

            var crestImgObj = CreateUIElement("BranchCrest", photoBox.transform);
            SetRect(crestImgObj.GetComponent<RectTransform>(), 0.25f, 0.10f, 0.46f, 0.90f);
            var crestImg = crestImgObj.AddComponent<Image>();
            crestImg.preserveAspect = true;
            hudView.branchCrestImage = crestImg;

            var nameText = CreateText("NameLabel", photoBox.transform, "Ethan Vance", 22, TextAlignmentOptions.Left, FontStyles.Bold, Color.white);
            SetRect(nameText.GetComponent<RectTransform>(), 0.48f, 0.50f, 0.98f, 0.95f);
            hudView.nameLabel = nameText;

            var rankText = CreateText("RankLabel", photoBox.transform, "Recruit (E-1)", 17, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.40f, 0.80f, 1f));
            SetRect(rankText.GetComponent<RectTransform>(), 0.48f, 0.05f, 0.98f, 0.50f);
            hudView.rankLabel = rankText;

            // Origin / Trait label
            var nobleText = CreateText("NobleTitleLabel", leftCol.transform, "Origin: Farmhand (Country Grit)", 15, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.85f, 0.75f, 0.40f));
            SetRect(nobleText.GetComponent<RectTransform>(), 0.04f, 0.775f, 0.96f, 0.825f);
            hudView.nobleTitleLabel = nobleText;

            // 4 Core Military Gauges
            var healthBar = CreateStatBar("HealthBar", leftCol.transform, "Health: 100%", new Color(0.85f, 0.25f, 0.30f), out var healthText);
            SetRect(healthBar.GetComponent<RectTransform>(), 0.04f, 0.69f, 0.96f, 0.76f);
            hudView.healthSlider = healthBar;
            hudView.healthBarText = healthText;

            var fitnessBar = CreateStatBar("FitnessBar", leftCol.transform, "Morale: 85%", new Color(0.18f, 0.70f, 0.35f), out var fitnessText);
            SetRect(fitnessBar.GetComponent<RectTransform>(), 0.04f, 0.605f, 0.96f, 0.675f);
            hudView.fitnessSlider = fitnessBar;
            hudView.fitnessBarText = fitnessText;

            var combatBar = CreateStatBar("CombatBar", leftCol.transform, "Combat: 50%", new Color(0.90f, 0.55f, 0.15f), out var combatText);
            SetRect(combatBar.GetComponent<RectTransform>(), 0.04f, 0.52f, 0.96f, 0.59f);
            hudView.combatSkillSlider = combatBar;
            hudView.combatBarText = combatText;

            var intellectBar = CreateStatBar("IntellectBar", leftCol.transform, "Discipline: 75%", new Color(0.20f, 0.60f, 0.95f), out var intellectText);
            SetRect(intellectBar.GetComponent<RectTransform>(), 0.04f, 0.435f, 0.96f, 0.505f);
            hudView.intellectSlider = intellectBar;
            hudView.intellectBarText = intellectText;

            // Compensation & Finance Summary Card
            var finCard = CreateUIElement("FinancesCard", leftCol.transform);
            SetRect(finCard.GetComponent<RectTransform>(), 0.04f, 0.22f, 0.96f, 0.42f);
            var finBg = finCard.AddComponent<Image>();
            finBg.color = new Color(0.13f, 0.16f, 0.22f);

            var finTitle = CreateText("FinTitle", finCard.transform, "SERVICE COMPENSATION & ASSETS", 14, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.70f, 0.80f, 0.90f));
            SetRect(finTitle.GetComponent<RectTransform>(), 0.05f, 0.76f, 0.95f, 0.96f);

            var cashLabel = CreateText("CashLabel", finCard.transform, "Bank Balance: $1,450", 17, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.40f, 1f, 0.60f));
            SetRect(cashLabel.GetComponent<RectTransform>(), 0.08f, 0.50f, 0.95f, 0.74f);
            hudView.cashBalanceLabel = cashLabel;

            var payLabel = CreateText("PayLabel", finCard.transform, "Net Pay: +$900 / mo", 15, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetRect(payLabel.GetComponent<RectTransform>(), 0.08f, 0.26f, 0.95f, 0.48f);
            hudView.monthlyPayLabel = payLabel;

            var cmpLabel = CreateText("CMPLabel", finCard.transform, "Combat Merit: 0 CMP", 15, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.60f, 1f));
            SetRect(cmpLabel.GetComponent<RectTransform>(), 0.08f, 0.04f, 0.95f, 0.24f);
            hudView.cmpBalanceLabel = cmpLabel;

            // Left Quick Action Drawers: Dossier & Finance
            var lifeBtnObj = CreateButton("LifeButton", leftCol.transform, "[DOSSIER & SERVICE RECORD]", new Color(0.20f, 0.28f, 0.38f));
            SetRect(lifeBtnObj.GetComponent<RectTransform>(), 0.04f, 0.115f, 0.96f, 0.195f);
            hudView.lifeButton = lifeBtnObj.GetComponent<Button>();

            var finBtnObj = CreateButton("FinanceButton", leftCol.transform, "[FINANCES & ASSETS]", new Color(0.18f, 0.25f, 0.34f));
            SetRect(finBtnObj.GetComponent<RectTransform>(), 0.04f, 0.02f, 0.96f, 0.10f);
            hudView.financeButton = finBtnObj.GetComponent<Button>();

            // =========================================================================
            // COLUMN 2: CENTER PANE — FIELD TELETYPE & ACTION CONSOLE (x: 0.28 to 0.72)
            // =========================================================================
            var centerCol = CreateUIElement("CenterFeedColumn", mainHUDPanel.transform);
            SetRect(centerCol.GetComponent<RectTransform>(), 0.28f, 0.015f, 0.72f, 0.92f);
            var centerBg = centerCol.AddComponent<Image>();
            centerBg.color = new Color(0.08f, 0.10f, 0.13f);
            hudView.feedContainerImage = centerBg;

            var feedHeader = CreateText("FeedHeader", centerCol.transform, "OFFICIAL SERVICE RECORD CHRONICLE", 16, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.75f, 0.85f, 0.95f));
            SetRect(feedHeader.GetComponent<RectTransform>(), 0.02f, 0.955f, 0.98f, 0.99f);

            var feedScrollObj = CreateUIElement("FeedScrollArea", centerCol.transform);
            SetRect(feedScrollObj.GetComponent<RectTransform>(), 0.02f, 0.135f, 0.98f, 0.95f);
            var feedScroll = feedScrollObj.AddComponent<ScrollRect>();
            feedScroll.horizontal = false;
            feedScroll.vertical = true;
            feedScroll.movementType = ScrollRect.MovementType.Clamped;
            feedScroll.scrollSensitivity = 45f;

            var feedViewport = CreateUIElement("Viewport", feedScrollObj.transform);
            SetFullScreen(feedViewport.GetComponent<RectTransform>());
            var vpImg = feedViewport.AddComponent<Image>();
            vpImg.color = new Color(0.06f, 0.08f, 0.10f);
            feedViewport.AddComponent<Mask>().showMaskGraphic = true;
            feedScroll.viewport = feedViewport.GetComponent<RectTransform>();

            var feedContent = CreateUIElement("Content", feedViewport.transform);
            var feedContentRt = feedContent.GetComponent<RectTransform>();
            feedContentRt.anchorMin = new Vector2(0f, 1f);
            feedContentRt.anchorMax = new Vector2(1f, 1f);
            feedContentRt.pivot = new Vector2(0.5f, 1f);
            feedContentRt.sizeDelta = new Vector2(0f, 1200f);
            feedScroll.content = feedContentRt;

            var vlg = feedContent.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(15, 15, 15, 15);

            var csf = feedContent.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var feedTextObj = CreateText("FeedContentText", feedContent.transform, "Welcome to the Armed Forces.\nEnlistment Day initialized.\n", 18, TextAlignmentOptions.TopLeft, FontStyles.Normal, Color.white);
            feedTextObj.textWrappingMode = TextWrappingModes.Normal;
            var feedLe = feedTextObj.gameObject.AddComponent<LayoutElement>();
            feedLe.flexibleWidth = 1f;

            var feedView = feedScrollObj.AddComponent<BitLifeFeedView>();
            feedView.feedContentText = feedTextObj;
            feedView.feedScrollRect = feedScroll;

            // Massive Central Action Button: [ ADVANCE 6-MONTH TOUR ]
            var advanceBtnObj = CreateButton("AdvanceSemesterButton", centerCol.transform, "[ ADVANCE 6-MONTH TOUR OF DUTY ]", new Color(0.12f, 0.65f, 0.35f));
            SetRect(advanceBtnObj.GetComponent<RectTransform>(), 0.02f, 0.015f, 0.98f, 0.115f);
            var advanceBtnText = advanceBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (advanceBtnText != null)
            {
                advanceBtnText.fontSize = 22;
                advanceBtnText.fontStyle = FontStyles.Bold;
            }
            hudView.advanceSemesterButton = advanceBtnObj.GetComponent<Button>();

            // =========================================================================
            // COLUMN 3: RIGHT PANE — TACTICAL FRONT & SQUAD ROSTER (x: 0.73 to 0.99)
            // =========================================================================
            var rightCol = CreateUIElement("RightTacticalColumn", mainHUDPanel.transform);
            SetRect(rightCol.GetComponent<RectTransform>(), 0.73f, 0.015f, 0.99f, 0.92f);
            var rightBg = rightCol.AddComponent<Image>();
            rightBg.color = new Color(0.10f, 0.12f, 0.16f);
            hudView.statsPanelImage = rightBg;

            // Section 1: Active Front / Theater Briefing Card
            var frontCard = CreateUIElement("FrontBriefingCard", rightCol.transform);
            SetRect(frontCard.GetComponent<RectTransform>(), 0.04f, 0.72f, 0.96f, 0.98f);
            var frontCardBg = frontCard.AddComponent<Image>();
            frontCardBg.color = new Color(0.14f, 0.17f, 0.22f);

            var frontTitle = CreateText("FrontTitle", frontCard.transform, "ACTIVE DEPLOYMENT THEATER", 14, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.85f, 0.35f));
            SetRect(frontTitle.GetComponent<RectTransform>(), 0.05f, 0.78f, 0.95f, 0.96f);

            var frontSitrep = CreateText("FrontSitrep", frontCard.transform, "<b>GARRISON BASE</b>\nWeapons Qualification & Field Drills\nThreat Level: Low (Peacetime Rotation)", 15, TextAlignmentOptions.Center, FontStyles.Normal, Color.white);
            SetRect(frontSitrep.GetComponent<RectTransform>(), 0.05f, 0.08f, 0.95f, 0.75f);

            // Section 2: Active Fireteam Roster Card
            var squadCard = CreateUIElement("SquadRosterCard", rightCol.transform);
            SetRect(squadCard.GetComponent<RectTransform>(), 0.04f, 0.37f, 0.96f, 0.70f);
            var squadCardBg = squadCard.AddComponent<Image>();
            squadCardBg.color = new Color(0.12f, 0.15f, 0.20f);

            var squadTitle = CreateText("SquadTitle", squadCard.transform, "FIRETEAM SQUADMATES", 14, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.40f, 0.80f, 1f));
            SetRect(squadTitle.GetComponent<RectTransform>(), 0.05f, 0.84f, 0.95f, 0.97f);

            var squadListText = CreateText("SquadList", squadCard.transform, 
                "• Scout: <b>Pvt. Miller</b> (Loyalty: 85%)\n" +
                "• Gunner: <b>Cpl. Kowalski</b> (Loyalty: 90%)\n" +
                "• Medic: <b>Pvt. Chen</b> (Trauma Ready)", 
                15, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.90f, 0.92f, 0.95f));
            SetRect(squadListText.GetComponent<RectTransform>(), 0.08f, 0.08f, 0.92f, 0.82f);

            // Section 3: Operational Action Drawers
            var opsTitle = CreateText("OpsTitle", rightCol.transform, "FIELD & BASE OPERATIONS", 14, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.70f, 0.80f, 0.90f));
            SetRect(opsTitle.GetComponent<RectTransform>(), 0.05f, 0.295f, 0.95f, 0.355f);

            var squadBtnObj = CreateButton("RelationshipsButton", rightCol.transform, "[SQUAD & BARRACKS]", new Color(0.20f, 0.28f, 0.38f));
            SetRect(squadBtnObj.GetComponent<RectTransform>(), 0.04f, 0.20f, 0.96f, 0.285f);
            hudView.relationshipsButton = squadBtnObj.GetComponent<Button>();

            var actBtnObj = CreateButton("ActivitiesButton", rightCol.transform, "[BASE OPERATIONS & PX]", new Color(0.22f, 0.30f, 0.40f));
            SetRect(actBtnObj.GetComponent<RectTransform>(), 0.04f, 0.105f, 0.96f, 0.19f);
            hudView.activitiesButton = actBtnObj.GetComponent<Button>();

            var medalsBtnObj = CreateButton("MedalsButton", rightCol.transform, "[SERVICE UNIFORM & MEDALS]", new Color(0.35f, 0.30f, 0.15f));
            SetRect(medalsBtnObj.GetComponent<RectTransform>(), 0.04f, 0.015f, 0.96f, 0.095f);
            var medalsBtn = medalsBtnObj.GetComponent<Button>();
            medalsBtn.onClick.AddListener(() =>
            {
                if (hudView.medalRibbonRackModal != null && GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
                {
                    hudView.medalRibbonRackModal.OpenModal(GameManager.Instance.currentSoldier);
                }
            });

            // ==========================================
            // 6. OPENING SCREEN PANEL (Title / Main Menu)
            // ==========================================
            var openingPanel = CreateUIElement("OpeningScreenPanel", rootPanel.transform);
            SetFullScreen(openingPanel.GetComponent<RectTransform>());
            var openingBg = openingPanel.AddComponent<Image>();
            openingBg.color = new Color(0.07f, 0.09f, 0.12f);
            openingCtrl.openingScreenPanel = openingPanel;

            // Title Banner
            var titleText = CreateText("GameTitle", openingPanel.transform, "ARMY LIFE SIMULATOR", 64, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.95f, 0.80f, 0.30f));
            SetRect(titleText.GetComponent<RectTransform>(), 0.10f, 0.72f, 0.90f, 0.85f);

            var subtitleText = CreateText("Subtitle", openingPanel.transform, "A Gritty Military Life Simulation RPG\n<color=#00D4FF>From Drafted Recruit to 4-Star General</color>", 26, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            SetRect(subtitleText.GetComponent<RectTransform>(), 0.10f, 0.60f, 0.90f, 0.71f);

            var recordsLabel = CreateText("RecordsLabel", openingPanel.transform, "Active Military Service Records: 0", 22, TextAlignmentOptions.Center, FontStyles.Italic, new Color(0.6f, 0.7f, 0.8f));
            SetRect(recordsLabel.GetComponent<RectTransform>(), 0.10f, 0.53f, 0.90f, 0.58f);
            openingCtrl.totalLivesCountLabel = recordsLabel;

            // Option 1: Create New Life Button
            var createBtnObj = CreateButton("CreateNewLifeButton", openingPanel.transform, "ENLIST NEW RECRUIT (START CAREER)", new Color(0.18f, 0.65f, 0.35f));
            SetRect(createBtnObj.GetComponent<RectTransform>(), 0.30f, 0.40f, 0.70f, 0.49f);
            openingCtrl.createNewLifeButton = createBtnObj.GetComponent<Button>();

            // Option 2: Continue or Manage Lives Button (Dynamically switched in code!)
            var contBtnObj = CreateButton("ContinueOrManageButton", openingPanel.transform, "CONTINUE CAREER", new Color(0.20f, 0.45f, 0.75f));
            SetRect(contBtnObj.GetComponent<RectTransform>(), 0.30f, 0.29f, 0.70f, 0.38f);
            openingCtrl.continueOrManageButton = contBtnObj.GetComponent<Button>();
            openingCtrl.continueOrManageButtonText = contBtnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Option 3: Settings Button
            var openingSettingsBtnObj = CreateButton("OpeningSettingsButton", openingPanel.transform, "SETTINGS", new Color(0.24f, 0.32f, 0.42f));
            SetRect(openingSettingsBtnObj.GetComponent<RectTransform>(), 0.30f, 0.18f, 0.70f, 0.27f);
            openingCtrl.settingsButton = openingSettingsBtnObj.GetComponent<Button>();

            // Footer Lore Hint
            var footerText = CreateText("FooterHint", openingPanel.transform, "Survive Harrowing Frontline Tours • Forge Unbreakable Squad Bonds • Rise Through the Ranks", 20, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.55f, 0.60f, 0.68f));
            SetRect(footerText.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.95f, 0.14f);

            // ==========================================
            // 7. BITLIFE-STYLE CHARACTER CREATION MODAL
            // ==========================================
            var creationModal = CreateUIElement("CharacterCreationModal", rootPanel.transform);
            SetFullScreen(creationModal.GetComponent<RectTransform>());
            var modalDimBg = creationModal.AddComponent<Image>();
            modalDimBg.color = new Color(0.07f, 0.09f, 0.13f, 1.0f); // 100% Solid Opaque to prevent any title bleed!
            openingCtrl.characterCreationModal = creationModal;

            // TOP HEADER BAR (Fixed outside scroll area)
            var creationTopBar = CreateUIElement("TopHeaderBar", creationModal.transform);
            var topBarRect = creationTopBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 1f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.sizeDelta = new Vector2(0f, 110f);
            topBarRect.anchoredPosition = Vector2.zero;
            var topBarBg = creationTopBar.AddComponent<Image>();
            topBarBg.color = new Color(0.11f, 0.14f, 0.20f, 1.0f);

            var cancelBtnObj = CreateButton("CancelCreationBtn", creationTopBar.transform, "CANCEL", new Color(0.40f, 0.18f, 0.22f));
            var cancelBtnRect = cancelBtnObj.GetComponent<RectTransform>();
            cancelBtnRect.anchorMin = new Vector2(0f, 0.5f);
            cancelBtnRect.anchorMax = new Vector2(0f, 0.5f);
            cancelBtnRect.pivot = new Vector2(0f, 0.5f);
            cancelBtnRect.anchoredPosition = new Vector2(30f, 0f);
            cancelBtnRect.sizeDelta = new Vector2(160f, 64f);
            openingCtrl.cancelCreationButton = cancelBtnObj.GetComponent<Button>();

            var createTitle = CreateText("ModalTitle", creationTopBar.transform, "CUSTOM LIFE", 38, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.95f, 0.82f, 0.35f));
            SetFullScreen(createTitle.GetComponent<RectTransform>());
            createTitle.raycastTarget = false;

            // BOTTOM ACTION BAR (Fixed outside scroll area)
            var creationBottomBar = CreateUIElement("BottomActionBar", creationModal.transform);
            var bottomBarRect = creationBottomBar.GetComponent<RectTransform>();
            bottomBarRect.anchorMin = new Vector2(0f, 0f);
            bottomBarRect.anchorMax = new Vector2(1f, 0f);
            bottomBarRect.pivot = new Vector2(0.5f, 0f);
            bottomBarRect.sizeDelta = new Vector2(0f, 120f);
            bottomBarRect.anchoredPosition = Vector2.zero;
            var bottomBarBg = creationBottomBar.AddComponent<Image>();
            bottomBarBg.color = new Color(0.10f, 0.13f, 0.18f, 1.0f);

            var confirmBtnObj = CreateButton("ConfirmStartBtn", creationBottomBar.transform, "ENLIST IN ARMED FORCES (START CAREER)", new Color(0.15f, 0.68f, 0.35f));
            var confirmBtnRect = confirmBtnObj.GetComponent<RectTransform>();
            confirmBtnRect.anchorMin = Vector2.zero;
            confirmBtnRect.anchorMax = Vector2.one;
            confirmBtnRect.offsetMin = new Vector2(480f, 18f);
            confirmBtnRect.offsetMax = new Vector2(-480f, -18f);
            openingCtrl.confirmStartLifeButton = confirmBtnObj.GetComponent<Button>();

            // SCROLL CONTAINER (Bounded between Top Bar and Bottom Bar)
            var scrollObj = CreateUIElement("CreationScroll", creationModal.transform);
            var scrollRectRt = scrollObj.GetComponent<RectTransform>();
            scrollRectRt.anchorMin = Vector2.zero;
            scrollRectRt.anchorMax = Vector2.one;
            scrollRectRt.offsetMin = new Vector2(0f, 120f);
            scrollRectRt.offsetMax = new Vector2(0f, -110f);

            var creationScroll = scrollObj.AddComponent<ScrollRect>();
            creationScroll.horizontal = false;
            creationScroll.vertical = true;
            creationScroll.scrollSensitivity = 35f;

            var viewport = CreateUIElement("Viewport", scrollObj.transform);
            SetFullScreen(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            creationScroll.viewport = viewport.GetComponent<RectTransform>();

            var formContent = CreateUIElement("Content", viewport.transform);
            var formRect = formContent.GetComponent<RectTransform>();
            formRect.anchorMin = new Vector2(0f, 1f);
            formRect.anchorMax = new Vector2(1f, 1f);
            formRect.pivot = new Vector2(0.5f, 1f);
            formRect.offsetMin = Vector2.zero;
            formRect.offsetMax = Vector2.zero;
            creationScroll.content = formRect;

            var formLayout = formContent.AddComponent<VerticalLayoutGroup>();
            formLayout.padding = new RectOffset(420, 420, 20, 40);
            formLayout.spacing = 16f;
            formLayout.childControlHeight = true;
            formLayout.childControlWidth = true;
            formLayout.childForceExpandHeight = false;
            formLayout.childForceExpandWidth = true;

            var formFitter = formContent.AddComponent<ContentSizeFitter>();
            formFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // CARD 1: FIRST NAME ROW
            var firstNameCard = CreateUIElement("FirstNameCard", formContent.transform);
            var fnCardBg = firstNameCard.AddComponent<Image>();
            fnCardBg.color = new Color(0.12f, 0.16f, 0.22f);
            SetLayoutHeight(firstNameCard, 74);
            var fnLayout = firstNameCard.AddComponent<HorizontalLayoutGroup>();
            fnLayout.padding = new RectOffset(20, 20, 10, 10);
            fnLayout.spacing = 14;
            fnLayout.childControlWidth = true;
            fnLayout.childControlHeight = true;
            fnLayout.childForceExpandWidth = false;
            fnLayout.childForceExpandHeight = true;

            var fnLabel = CreateText("Label", firstNameCard.transform, "First Name", 24, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Color(0.40f, 0.85f, 1f));
            SetLayoutWidth(fnLabel.gameObject, 170);
            fnLabel.raycastTarget = false;

            var firstInput = CreateInputField("FirstNameInput", firstNameCard.transform, "First Name");
            var firstLayout = firstInput.gameObject.AddComponent<LayoutElement>();
            firstLayout.flexibleWidth = 1f;
            firstLayout.minWidth = 180;
            openingCtrl.characterFirstNameInput = firstInput;

            var randNameBtn = CreateButton("RandNameBtn", firstNameCard.transform, "RAND", new Color(0.24f, 0.35f, 0.48f));
            SetLayoutWidth(randNameBtn, 70);
            openingCtrl.randomizeNameButton = randNameBtn.GetComponent<Button>();

            // CARD 2: LAST NAME ROW
            var lastNameCard = CreateUIElement("LastNameCard", formContent.transform);
            var lnCardBg = lastNameCard.AddComponent<Image>();
            lnCardBg.color = new Color(0.12f, 0.16f, 0.22f);
            SetLayoutHeight(lastNameCard, 74);
            var lnLayout = lastNameCard.AddComponent<HorizontalLayoutGroup>();
            lnLayout.padding = new RectOffset(20, 20, 10, 10);
            lnLayout.spacing = 14;
            lnLayout.childControlWidth = true;
            lnLayout.childControlHeight = true;
            lnLayout.childForceExpandWidth = false;
            lnLayout.childForceExpandHeight = true;

            var lnLabel = CreateText("Label", lastNameCard.transform, "Last Name", 24, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Color(0.40f, 0.85f, 1f));
            SetLayoutWidth(lnLabel.gameObject, 170);
            lnLabel.raycastTarget = false;

            var lastInput = CreateInputField("LastNameInput", lastNameCard.transform, "Last Name");
            var lastLayout = lastInput.gameObject.AddComponent<LayoutElement>();
            lastLayout.flexibleWidth = 1f;
            lastLayout.minWidth = 180;
            openingCtrl.characterLastNameInput = lastInput;

            // CARD 3: GENDER ROW
            var genderCard = CreateUIElement("GenderCard", formContent.transform);
            var genCardBg = genderCard.AddComponent<Image>();
            genCardBg.color = new Color(0.12f, 0.16f, 0.22f);
            SetLayoutHeight(genderCard, 74);
            var genLayout = genderCard.AddComponent<HorizontalLayoutGroup>();
            genLayout.padding = new RectOffset(20, 20, 10, 10);
            genLayout.spacing = 14;
            genLayout.childControlWidth = true;
            genLayout.childControlHeight = true;
            genLayout.childForceExpandWidth = false;
            genLayout.childForceExpandHeight = true;

            var genLabel = CreateText("Label", genderCard.transform, "Gender", 24, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Color(0.40f, 0.85f, 1f));
            SetLayoutWidth(genLabel.gameObject, 170);
            genLabel.raycastTarget = false;

            var maleBtn = CreateButton("MaleBtn", genderCard.transform, "MALE", new Color(0.20f, 0.50f, 0.85f));
            var maleLayout = maleBtn.AddComponent<LayoutElement>();
            maleLayout.flexibleWidth = 1f;
            maleLayout.minWidth = 180;
            openingCtrl.maleGenderButton = maleBtn.GetComponent<Button>();
            openingCtrl.maleGenderText = maleBtn.GetComponentInChildren<TextMeshProUGUI>();

            var femaleBtn = CreateButton("FemaleBtn", genderCard.transform, "FEMALE", new Color(0.18f, 0.22f, 0.28f));
            var femaleLayout = femaleBtn.AddComponent<LayoutElement>();
            femaleLayout.flexibleWidth = 1f;
            femaleLayout.minWidth = 180;
            openingCtrl.femaleGenderButton = femaleBtn.GetComponent<Button>();
            openingCtrl.femaleGenderText = femaleBtn.GetComponentInChildren<TextMeshProUGUI>();

            // CARD 4: BIRTH COUNTRY CARD
            var countryCard = CreateUIElement("CountryCard", formContent.transform);
            var cCardBg = countryCard.AddComponent<Image>();
            cCardBg.color = new Color(0.12f, 0.16f, 0.22f);
            SetLayoutHeight(countryCard, 155);
            var cLayout = countryCard.AddComponent<VerticalLayoutGroup>();
            cLayout.padding = new RectOffset(20, 20, 12, 12);
            cLayout.spacing = 10;
            cLayout.childControlWidth = true;
            cLayout.childControlHeight = true;
            cLayout.childForceExpandWidth = true;
            cLayout.childForceExpandHeight = false;

            var cRowObj = CreateUIElement("SelectorRow", countryCard.transform);
            SetLayoutHeight(cRowObj, 52);
            var cRowLayout = cRowObj.AddComponent<HorizontalLayoutGroup>();
            cRowLayout.spacing = 12;
            cRowLayout.childControlWidth = true;
            cRowLayout.childControlHeight = true;
            cRowLayout.childForceExpandWidth = false;
            cRowLayout.childForceExpandHeight = true;

            var cLabel = CreateText("Label", cRowObj.transform, "Country", 24, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Color(0.40f, 0.85f, 1f));
            SetLayoutWidth(cLabel.gameObject, 170);
            cLabel.raycastTarget = false;

            var prevCObj = CreateButton("PrevBtn", cRowObj.transform, "◄", new Color(0.20f, 0.25f, 0.32f));
            SetLayoutWidth(prevCObj, 60);
            openingCtrl.prevCountryButton = prevCObj.GetComponent<Button>();

            var cValBox = CreateUIElement("ValueBox", cRowObj.transform);
            var cValBoxImg = cValBox.AddComponent<Image>();
            cValBoxImg.color = new Color(0.08f, 0.11f, 0.16f);
            var cValLayout = cValBox.AddComponent<LayoutElement>();
            cValLayout.flexibleWidth = 1f;
            cValLayout.minWidth = 180;

            var cValObj = CreateText("ValueText", cValBox.transform, "The Valerian Empire", 24, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.25f, 0.95f, 0.70f));
            SetFullScreen(cValObj.GetComponent<RectTransform>());
            cValObj.raycastTarget = false;
            openingCtrl.countryTitleText = cValObj;

            var nextCObj = CreateButton("NextBtn", cRowObj.transform, "►", new Color(0.20f, 0.25f, 0.32f));
            SetLayoutWidth(nextCObj, 60);
            openingCtrl.nextCountryButton = nextCObj.GetComponent<Button>();

            var countryDescBox = CreateUIElement("CountryDescBox", countryCard.transform);
            var countryDescBoxBg = countryDescBox.AddComponent<Image>();
            countryDescBoxBg.color = new Color(0.08f, 0.11f, 0.16f);
            SetLayoutHeight(countryDescBox, 65);

            var countryDesc = CreateText("CountryDesc", countryDescBox.transform, "Country details...", 20, TextAlignmentOptions.MidlineLeft, FontStyles.Italic, new Color(0.75f, 0.85f, 0.95f));
            var cdRect = countryDesc.GetComponent<RectTransform>();
            cdRect.anchorMin = Vector2.zero;
            cdRect.anchorMax = Vector2.one;
            cdRect.offsetMin = new Vector2(16, 6);
            cdRect.offsetMax = new Vector2(-16, -6);
            openingCtrl.countryDescText = countryDesc;

            // CARD 5-8: APPEARANCE CARDS (Hairstyle, Hair Color, Eye Color, Skin Tone)
            CreateCycleSelector("HairstyleSelector", formContent.transform, "Hairstyle", out var prevHairStyle, out var nextHairStyle, out var hairStyleTmp);
            openingCtrl.prevHairStyleButton = prevHairStyle;
            openingCtrl.nextHairStyleButton = nextHairStyle;
            openingCtrl.hairStyleText = hairStyleTmp;

            CreateCycleSelector("HairColorSelector", formContent.transform, "Hair Color", out var prevHairColor, out var nextHairColor, out var hairColorTmp);
            openingCtrl.prevHairColorButton = prevHairColor;
            openingCtrl.nextHairColorButton = nextHairColor;
            openingCtrl.hairColorText = hairColorTmp;

            CreateCycleSelector("EyeColorSelector", formContent.transform, "Eye Color", out var prevEyeColor, out var nextEyeColor, out var eyeColorTmp);
            openingCtrl.prevEyeColorButton = prevEyeColor;
            openingCtrl.nextEyeColorButton = nextEyeColor;
            openingCtrl.eyeColorText = eyeColorTmp;

            CreateCycleSelector("SkinToneSelector", formContent.transform, "Skin Tone", out var prevSkinTone, out var nextSkinTone, out var skinToneTmp);
            openingCtrl.prevSkinToneButton = prevSkinTone;
            openingCtrl.nextSkinToneButton = nextSkinTone;
            openingCtrl.skinToneText = skinToneTmp;

            // CARD 9: RANDOMIZE APPEARANCE BUTTON
            var randLookBtn = CreateButton("RandLookBtn", formContent.transform, "RANDOMIZE APPEARANCE", new Color(0.22f, 0.32f, 0.44f));
            SetLayoutHeight(randLookBtn, 65);
            openingCtrl.randomizeAppearanceButton = randLookBtn.GetComponent<Button>();

            // CARD 10: MILITARY RECRUIT HERITAGE
            var linCard = CreateUIElement("FateOfBirthBox", formContent.transform);
            var linCardBg = linCard.AddComponent<Image>();
            linCardBg.color = new Color(0.10f, 0.13f, 0.18f);
            SetLayoutHeight(linCard, 120);

            var linCardText = CreateText("FateDesc", linCard.transform, 
                "<b>Recruit Background & Heritage:</b>\n" +
                "• <b>Farmhand:</b> High endurance & physical grit (+15% Health, +15% Fitness)\n" +
                "• <b>Street Tough:</b> Urban brawler instincts (+15% Combat Skill)\n" +
                "• <b>Military Legacy:</b> Respected soldier heritage (+15% Unit Respect & Standing)", 
                20, TextAlignmentOptions.MidlineLeft, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            var lcRect = linCardText.GetComponent<RectTransform>();
            lcRect.anchorMin = Vector2.zero;
            lcRect.anchorMax = Vector2.one;
            lcRect.offsetMin = new Vector2(18, 10);
            lcRect.offsetMax = new Vector2(-18, -10);

            // CARD 11: LIVE IDENTITY DOSSIER
            var previewBox = CreateUIElement("PreviewBox", formContent.transform);
            var prevBoxBg = previewBox.AddComponent<Image>();
            prevBoxBg.color = new Color(0.08f, 0.11f, 0.15f);
            SetLayoutHeight(previewBox, 130);

            var previewTmp = CreateText("PreviewBadge", previewBox.transform, "Identity Dossier...", 22, TextAlignmentOptions.MidlineLeft, FontStyles.Normal, Color.white);
            var pbRect = previewTmp.GetComponent<RectTransform>();
            pbRect.anchorMin = Vector2.zero;
            pbRect.anchorMax = Vector2.one;
            pbRect.offsetMin = new Vector2(18, 10);
            pbRect.offsetMax = new Vector2(-18, -10);
            openingCtrl.characterPreviewBadgeText = previewTmp;

            creationModal.SetActive(false);

            // ==========================================
            // 8. MANAGE LIVES MODAL
            // ==========================================
            var manageModal = CreateUIElement("ManageLivesModal", rootPanel.transform);
            SetFullScreen(manageModal.GetComponent<RectTransform>());
            var manageDimBg = manageModal.AddComponent<Image>();
            manageDimBg.color = new Color(0f, 0f, 0f, 0.88f);
            openingCtrl.manageLivesModal = manageModal;

            var manageBox = CreateUIElement("ManageBox", manageModal.transform);
            SetRect(manageBox.GetComponent<RectTransform>(), 0.05f, 0.08f, 0.95f, 0.92f);
            var manageBoxBg = manageBox.AddComponent<Image>();
            manageBoxBg.color = new Color(0.11f, 0.13f, 0.17f);

            var manageTitle = CreateText("ManageTitle", manageBox.transform, "MILITARY SERVICE ARCHIVES", 44, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.40f, 0.80f, 1f));
            SetRect(manageTitle.GetComponent<RectTransform>(), 0.05f, 0.92f, 0.95f, 0.98f);

            var listScrollObj = CreateUIElement("ListScrollView", manageBox.transform);
            SetRect(listScrollObj.GetComponent<RectTransform>(), 0.04f, 0.12f, 0.96f, 0.90f);
            var scroll = listScrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 30f;

            var listViewport = CreateUIElement("Viewport", listScrollObj.transform);
            SetFullScreen(listViewport.GetComponent<RectTransform>());
            listViewport.AddComponent<RectMask2D>();
            scroll.viewport = listViewport.GetComponent<RectTransform>();

            var contentObj = CreateUIElement("Content", listViewport.transform);
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var vertLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            vertLayout.spacing = 15;
            vertLayout.childControlHeight = true;
            vertLayout.childControlWidth = true;
            vertLayout.childForceExpandHeight = false;
            vertLayout.childForceExpandWidth = true;
            vertLayout.padding = new RectOffset(10, 10, 10, 10);

            var fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
            openingCtrl.livesListContent = contentObj.transform;

            var closeManageBtnObj = CreateButton("CloseManageBtn", manageBox.transform, "BACK TO MAIN MENU", new Color(0.35f, 0.40f, 0.48f));
            SetRect(closeManageBtnObj.GetComponent<RectTransform>(), 0.20f, 0.02f, 0.80f, 0.09f);
            openingCtrl.closeManageLivesButton = closeManageBtnObj.GetComponent<Button>();

            manageModal.SetActive(false);

            // ==========================================
            // 9. INTERACTIVE DILEMMA POPUP MODAL
            // ==========================================
            var dilemmaModalObj = CreateUIElement("DilemmaModal", rootPanel.transform);
            SetFullScreen(dilemmaModalObj.GetComponent<RectTransform>());
            var dilemmaCtrl = dilemmaModalObj.AddComponent<DilemmaModal>();
            hudView.dilemmaModal = dilemmaCtrl;

            var dilemmaOverlay = CreateUIElement("DilemmaOverlay", dilemmaModalObj.transform);
            SetFullScreen(dilemmaOverlay.GetComponent<RectTransform>());
            var dilemmaDimBg = dilemmaOverlay.AddComponent<Image>();
            dilemmaDimBg.color = new Color(0f, 0f, 0f, 0.88f);
            dilemmaCtrl.modalRoot = dilemmaOverlay;

            var dilemmaBox = CreateUIElement("DilemmaBox", dilemmaOverlay.transform);
            SetRect(dilemmaBox.GetComponent<RectTransform>(), 0.08f, 0.20f, 0.92f, 0.80f);
            var dilemmaBoxBg = dilemmaBox.AddComponent<Image>();
            dilemmaBoxBg.color = new Color(0.12f, 0.15f, 0.19f);

            var catBadge = CreateText("CategoryBadge", dilemmaBox.transform, "[MILESTONE EVENT]", 26, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetRect(catBadge.GetComponent<RectTransform>(), 0.05f, 0.88f, 0.95f, 0.96f);
            dilemmaCtrl.categoryBadgeText = catBadge;

            var dTitle = CreateText("DilemmaTitle", dilemmaBox.transform, "Event Title", 36, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.85f, 0.35f));
            SetRect(dTitle.GetComponent<RectTransform>(), 0.05f, 0.76f, 0.95f, 0.87f);
            dilemmaCtrl.titleText = dTitle;

            var dDesc = CreateText("DilemmaDesc", dilemmaBox.transform, "Scenario narrative...", 28, TextAlignmentOptions.TopLeft, FontStyles.Normal, Color.white);
            SetRect(dDesc.GetComponent<RectTransform>(), 0.06f, 0.44f, 0.94f, 0.74f);
            dilemmaCtrl.descriptionText = dDesc;

            var btnA = CreateButton("ChoiceABtn", dilemmaBox.transform, "Choice A", new Color(0.18f, 0.50f, 0.75f));
            SetRect(btnA.GetComponent<RectTransform>(), 0.06f, 0.29f, 0.94f, 0.41f);
            dilemmaCtrl.choiceAButton = btnA.GetComponent<Button>();
            dilemmaCtrl.choiceAText = btnA.GetComponentInChildren<TextMeshProUGUI>();

            var btnB = CreateButton("ChoiceBBtn", dilemmaBox.transform, "Choice B", new Color(0.20f, 0.55f, 0.40f));
            SetRect(btnB.GetComponent<RectTransform>(), 0.06f, 0.16f, 0.94f, 0.27f);
            dilemmaCtrl.choiceBButton = btnB.GetComponent<Button>();
            dilemmaCtrl.choiceBText = btnB.GetComponentInChildren<TextMeshProUGUI>();

            var btnC = CreateButton("ChoiceCBtn", dilemmaBox.transform, "Choice C", new Color(0.50f, 0.35f, 0.65f));
            SetRect(btnC.GetComponent<RectTransform>(), 0.06f, 0.03f, 0.94f, 0.14f);
            dilemmaCtrl.choiceCButton = btnC.GetComponent<Button>();
            dilemmaCtrl.choiceCText = btnC.GetComponentInChildren<TextMeshProUGUI>();

            dilemmaOverlay.SetActive(false);
            dilemmaModalObj.SetActive(true);







            // ==========================================
            // 11. NEW HUD MODALS (5-BUTTON ECOSYSTEM)
            // ==========================================
            CreateSettingsModal(rootPanel.transform, hudView);
            CreateLifeModal(rootPanel.transform, hudView);
            CreateFinanceModal(rootPanel.transform, hudView);
            CreateRelationshipsModal(rootPanel.transform, hudView);
            CreateActivitiesModal(rootPanel.transform, hudView);
            CreateCareerDossierModal(rootPanel.transform, hudView);
            CreateMailCallModal(rootPanel.transform, hudView);
            CreateMedalRibbonRackModal(rootPanel.transform, hudView);

            // Initial view states: Opening screen active, HUD hidden
            openingPanel.SetActive(true);
            mainHUDPanel.SetActive(false);

            // Save Scene
            string scenePath = "Assets/Scenes/MainGame.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Army Life Simulator", 
                    "Playable Scene updated successfully with full BitLife-style Character Customization!\n\nJust click PLAY (▶) in Unity to test!", 
                    "Let's Play!");
            }
            else
            {
                Debug.Log("<color=#00FFAA>[SceneSetupWizard] Playable scene generated and saved successfully!</color>");
            }
        }

        private static void CreateCycleSelector(string name, Transform parent, string labelText, out Button prevBtn, out Button nextBtn, out TextMeshProUGUI valueTmp)
        {
            var rowObj = CreateUIElement(name, parent);
            var rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.12f, 0.16f, 0.22f);
            SetLayoutHeight(rowObj, 74);

            var layout = rowObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.spacing = 12;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;

            var label = CreateText("Label", rowObj.transform, labelText, 24, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Color(0.40f, 0.85f, 1f));
            SetLayoutWidth(label.gameObject, 170);
            label.raycastTarget = false;

            var prevObj = CreateButton("PrevBtn", rowObj.transform, "◄", new Color(0.20f, 0.25f, 0.32f));
            SetLayoutWidth(prevObj, 60);
            prevBtn = prevObj.GetComponent<Button>();

            var valBox = CreateUIElement("ValueBox", rowObj.transform);
            var valBoxImg = valBox.AddComponent<Image>();
            valBoxImg.color = new Color(0.08f, 0.11f, 0.16f);
            var valLayout = valBox.AddComponent<LayoutElement>();
            valLayout.flexibleWidth = 1f;
            valLayout.minWidth = 180;

            var valObj = CreateText("ValueText", valBox.transform, "Option", 24, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            SetFullScreen(valObj.GetComponent<RectTransform>());
            valObj.raycastTarget = false;
            valueTmp = valObj;

            var nextObj = CreateButton("NextBtn", rowObj.transform, "►", new Color(0.20f, 0.25f, 0.32f));
            SetLayoutWidth(nextObj, 60);
            nextBtn = nextObj.GetComponent<Button>();
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
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Color color)
        {
            var btnObj = CreateUIElement(name, parent);
            var img = btnObj.AddComponent<Image>();
            img.color = color;
            var btn = btnObj.AddComponent<Button>();

            var colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.35f);
            colors.selectedColor = Color.Lerp(color, Color.white, 0.15f);
            btn.colors = colors;

            var text = CreateText("Label", btnObj.transform, label, 24, TextAlignmentOptions.Center, FontStyles.Bold);
            SetFullScreen(text.GetComponent<RectTransform>());
            text.raycastTarget = false;

            return btnObj;
        }

        private static TMP_InputField CreateInputField(string name, Transform parent, string placeholderText)
        {
            var obj = CreateUIElement(name, parent);
            var img = obj.AddComponent<Image>();
            img.color = new Color(0.13f, 0.16f, 0.21f);

            var textArea = CreateUIElement("Text Area", obj.transform);
            var areaRect = textArea.GetComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(15, 5);
            areaRect.offsetMax = new Vector2(-15, -5);
            textArea.AddComponent<RectMask2D>();

            var placeholderObj = CreateUIElement("Placeholder", textArea.transform);
            SetFullScreen(placeholderObj.GetComponent<RectTransform>());
            var placeholderTmp = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = placeholderText;
            placeholderTmp.fontSize = 24;
            placeholderTmp.fontStyle = FontStyles.Italic;
            placeholderTmp.color = new Color(0.5f, 0.58f, 0.68f, 0.7f);
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;
            placeholderTmp.raycastTarget = false;

            var textObj = CreateUIElement("Text", textArea.transform);
            SetFullScreen(textObj.GetComponent<RectTransform>());
            var textTmp = textObj.AddComponent<TextMeshProUGUI>();
            textTmp.fontSize = 24;
            textTmp.fontStyle = FontStyles.Bold;
            textTmp.color = new Color(0.95f, 0.98f, 1f);
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;
            textTmp.raycastTarget = false;

            var inputField = obj.AddComponent<TMP_InputField>();
            inputField.textViewport = areaRect;
            inputField.textComponent = textTmp;
            inputField.placeholder = placeholderTmp;
            inputField.selectionColor = new Color(0.2f, 0.5f, 0.85f, 0.5f);
            inputField.caretColor = Color.white;
            inputField.customCaretColor = true;

            return inputField;
        }

        private static Slider CreateStatBar(string name, Transform parent, string labelText, Color fillColor, out TextMeshProUGUI valText)
        {
            var barObj = CreateUIElement(name, parent);
            var bgImg = barObj.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.15f, 0.20f);

            var fillArea = CreateUIElement("Fill Area", barObj.transform);
            var faRect = fillArea.GetComponent<RectTransform>();
            faRect.anchorMin = Vector2.zero;
            faRect.anchorMax = Vector2.one;
            faRect.offsetMin = Vector2.zero;
            faRect.offsetMax = Vector2.zero;

            var fill = CreateUIElement("Fill", fillArea.transform);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;

            var slider = barObj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;
            slider.interactable = false;

            var text = CreateText("Label", barObj.transform, labelText, 20, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            SetFullScreen(text.GetComponent<RectTransform>());
            text.raycastTarget = false;
            valText = text;

            return slider;
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

        private static void SetLayoutHeight(GameObject obj, float height)
        {
            var le = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            var rt = obj.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        private static void SetLayoutWidth(GameObject obj, float width)
        {
            var le = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            le.minWidth = width;
            le.preferredWidth = width;
            var rt = obj.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
        }

        private static void CreateSettingsModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("SettingsModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<SettingsModal>();
            hudView.settingsModal = ctrl;

            var overlay = CreateUIElement("SettingsOverlay", modalObj.transform);
            SetFullScreen(overlay.GetComponent<RectTransform>());
            var dimBg = overlay.AddComponent<Image>();
            dimBg.color = new Color(0f, 0f, 0f, 0.92f);
            ctrl.modalRoot = overlay;
            ctrl.modalBackdropImage = dimBg;

            var box = CreateUIElement("SettingsBox", overlay.transform);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.06f, 0.08f);
            boxRect.anchorMax = new Vector2(0.94f, 0.92f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            var boxBg = box.AddComponent<Image>();
            boxBg.color = new Color(0.10f, 0.13f, 0.18f);
            ctrl.dialogPanelImage = boxBg;

            var title = CreateText("Title", box.transform, "GAME SETTINGS", 36, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetRect(title.GetComponent<RectTransform>(), 0.05f, 0.92f, 0.95f, 0.98f);
            ctrl.titleText = title;

            // Scroll container for settings items
            var scrollObj = CreateUIElement("SettingsScroll", box.transform);
            SetRect(scrollObj.GetComponent<RectTransform>(), 0.04f, 0.09f, 0.96f, 0.91f);
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 35f;

            var viewport = CreateUIElement("Viewport", scrollObj.transform);
            SetFullScreen(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var contentObj = CreateUIElement("Content", viewport.transform);
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            scroll.content = contentRect;

            var vLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 10f;
            vLayout.padding = new RectOffset(14, 14, 10, 10);
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 1. Theme Selection Row
            var themeLabel = CreateText("ThemeLabel", contentObj.transform, "<b>DISPLAY THEME MODE:</b>", 22, TextAlignmentOptions.Left, FontStyles.Bold, Color.white);
            SetLayoutHeight(themeLabel.gameObject, 28);

            var themeRow = CreateUIElement("ThemeRow", contentObj.transform);
            SetLayoutHeight(themeRow, 54);
            var thLayout = themeRow.AddComponent<HorizontalLayoutGroup>();
            thLayout.spacing = 12f;
            thLayout.childControlHeight = true;
            thLayout.childControlWidth = true;
            thLayout.childForceExpandHeight = false;
            thLayout.childForceExpandWidth = true;

            var darkBtn = CreateButton("DarkThemeBtn", themeRow.transform, "TACTICAL DRAB", new Color(0.14f, 0.19f, 0.15f));
            ctrl.darkThemeButton = darkBtn.GetComponent<Button>();
            ctrl.darkThemeButtonText = darkBtn.GetComponentInChildren<TextMeshProUGUI>();

            var lightBtn = CreateButton("LightThemeBtn", themeRow.transform, "CLASSIFIED DOSSIER", new Color(0.78f, 0.75f, 0.69f));
            ctrl.lightThemeButton = lightBtn.GetComponent<Button>();
            ctrl.lightThemeButtonText = lightBtn.GetComponentInChildren<TextMeshProUGUI>();
            var lightTxt = ctrl.lightThemeButtonText;
            if (lightTxt != null) lightTxt.color = new Color(0.1f, 0.12f, 0.1f);

            var desertBtn = CreateButton("DesertThemeBtn", themeRow.transform, "DESERT COYOTE", new Color(0.22f, 0.18f, 0.15f));
            ctrl.desertThemeButton = desertBtn.GetComponent<Button>();
            ctrl.desertThemeButtonText = desertBtn.GetComponentInChildren<TextMeshProUGUI>();

            var nightOpsBtn = CreateButton("NightOpsThemeBtn", themeRow.transform, "NIGHT OPS HUD", new Color(0.08f, 0.14f, 0.09f));
            ctrl.nightOpsThemeButton = nightOpsBtn.GetComponent<Button>();
            ctrl.nightOpsThemeButtonText = nightOpsBtn.GetComponentInChildren<TextMeshProUGUI>();

            // 2. Difficulty Row
            var diffLabel = CreateText("DiffLabel", contentObj.transform, "<b>GAMEPLAY DIFFICULTY:</b>", 22, TextAlignmentOptions.Left, FontStyles.Bold, Color.white);
            SetLayoutHeight(diffLabel.gameObject, 28);

            var diffRow = CreateUIElement("DiffRow", contentObj.transform);
            SetLayoutHeight(diffRow, 48);
            var dfLayout = diffRow.AddComponent<HorizontalLayoutGroup>();
            dfLayout.spacing = 10f;
            dfLayout.childControlHeight = true;
            dfLayout.childControlWidth = true;
            dfLayout.childForceExpandHeight = false;
            dfLayout.childForceExpandWidth = true;

            var storyBtn = CreateButton("StoryBtn", diffRow.transform, "STORY", new Color(0.18f, 0.50f, 0.35f));
            ctrl.difficultyStoryButton = storyBtn.GetComponent<Button>();

            var realBtn = CreateButton("RealisticBtn", diffRow.transform, "REALISTIC", new Color(0.25f, 0.40f, 0.65f));
            ctrl.difficultyRealisticButton = realBtn.GetComponent<Button>();

            var ironBtn = CreateButton("IroncladBtn", diffRow.transform, "IRONCLAD", new Color(0.65f, 0.20f, 0.20f));
            ctrl.difficultyIroncladButton = ironBtn.GetComponent<Button>();

            var diffDesc = CreateText("DiffDesc", contentObj.transform, "<b>Realistic Mode:</b> Authentic military simulation, 40% war tax, combat tours, competitive promotion boards.", 19, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.7f, 0.75f, 0.8f));
            SetLayoutHeight(diffDesc.gameObject, 42);
            ctrl.difficultyDescriptionText = diffDesc;

            // 3. Pacing Row
            var paceLabel = CreateText("PaceLabel", contentObj.transform, "<b>SIMULATION PACING:</b>", 22, TextAlignmentOptions.Left, FontStyles.Bold, Color.white);
            SetLayoutHeight(paceLabel.gameObject, 28);

            var paceRow = CreateUIElement("PaceRow", contentObj.transform);
            SetLayoutHeight(paceRow, 46);
            var pcLayout = paceRow.AddComponent<HorizontalLayoutGroup>();
            pcLayout.spacing = 10f;
            pcLayout.childControlHeight = true;
            pcLayout.childControlWidth = true;
            pcLayout.childForceExpandHeight = false;
            pcLayout.childForceExpandWidth = true;

            var stdBtn = CreateButton("StdPaceBtn", paceRow.transform, "STANDARD (DETAILED)", new Color(0.20f, 0.35f, 0.50f));
            ctrl.paceStandardButton = stdBtn.GetComponent<Button>();

            var fastBtn = CreateButton("FastPaceBtn", paceRow.transform, "FAST-FORWARD", new Color(0.45f, 0.30f, 0.55f));
            ctrl.paceFastButton = fastBtn.GetComponent<Button>();

            var paceDesc = CreateText("PaceDesc", contentObj.transform, "Standard 6-month simulation turn cadence.", 19, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.7f, 0.75f, 0.8f));
            SetLayoutHeight(paceDesc.gameObject, 26);
            ctrl.pacingDescriptionText = paceDesc;

            // 4. Audio Sliders
            var audioLabel = CreateText("AudioLabel", contentObj.transform, "<b>AUDIO & ATMOSPHERE:</b>", 22, TextAlignmentOptions.Left, FontStyles.Bold, Color.white);
            SetLayoutHeight(audioLabel.gameObject, 28);

            var mVolText = CreateText("MVolText", contentObj.transform, "Master Volume: 80%", 20, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetLayoutHeight(mVolText.gameObject, 24);
            ctrl.masterVolumeLabel = mVolText;

            var mSlider = CreateSlider("MasterSlider", contentObj.transform, 0f, 1f, 0.8f, new Color(0.2f, 0.65f, 0.9f));
            SetLayoutHeight(mSlider.gameObject, 28);
            ctrl.masterVolumeSlider = mSlider;

            var sVolText = CreateText("SVolText", contentObj.transform, "Sound FX: 90%", 20, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetLayoutHeight(sVolText.gameObject, 24);
            ctrl.sfxVolumeLabel = sVolText;

            var sSlider = CreateSlider("SfxSlider", contentObj.transform, 0f, 1f, 0.9f, new Color(0.2f, 0.85f, 0.4f));
            SetLayoutHeight(sSlider.gameObject, 28);
            ctrl.sfxVolumeSlider = sSlider;

            // 5. Service Record Management Buttons
            var svcRow = CreateUIElement("SvcRow", contentObj.transform);
            SetLayoutHeight(svcRow, 50);
            var svLayout = svcRow.AddComponent<HorizontalLayoutGroup>();
            svLayout.spacing = 10f;
            svLayout.childControlHeight = true;
            svLayout.childControlWidth = true;
            svLayout.childForceExpandHeight = false;
            svLayout.childForceExpandWidth = true;

            var titleBtn = CreateButton("TitleMenuBtn", svcRow.transform, "MAIN MENU", new Color(0.20f, 0.40f, 0.60f));
            ctrl.returnToTitleButton = titleBtn.GetComponent<Button>();

            var resignBtn = CreateButton("ResignBtn", svcRow.transform, "RESIGN COMMISSION", new Color(0.60f, 0.25f, 0.25f));
            ctrl.resignCommissionButton = resignBtn.GetComponent<Button>();

            // Close Button
            var closeBtn = CreateButton("CloseBtn", box.transform, "CLOSE SETTINGS", new Color(0.35f, 0.40f, 0.45f));
            SetRect(closeBtn.GetComponent<RectTransform>(), 0.25f, 0.02f, 0.75f, 0.075f);
            ctrl.closeButton = closeBtn.GetComponent<Button>();

            overlay.SetActive(false);
            modalObj.SetActive(true);
        }

        private static void CreateLifeModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("LifeModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<LifeModal>();
            hudView.lifeModal = ctrl;

            var overlay = CreateUIElement("LifeOverlay", modalObj.transform);
            SetFullScreen(overlay.GetComponent<RectTransform>());
            var dimBg = overlay.AddComponent<Image>();
            dimBg.color = new Color(0f, 0f, 0f, 0.92f);
            ctrl.modalRoot = overlay;
            ctrl.modalBackdropImage = dimBg;

            var box = CreateUIElement("LifeBox", overlay.transform);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.05f, 0.06f);
            boxRect.anchorMax = new Vector2(0.95f, 0.94f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            var boxBg = box.AddComponent<Image>();
            boxBg.color = new Color(0.10f, 0.13f, 0.18f);
            ctrl.dialogPanelImage = boxBg;

            var title = CreateText("Title", box.transform, "SERVICE RECORD & PROFILE", 36, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetRect(title.GetComponent<RectTransform>(), 0.05f, 0.93f, 0.95f, 0.99f);
            ctrl.titleText = title;

            var scrollObj = CreateUIElement("LifeScroll", box.transform);
            SetRect(scrollObj.GetComponent<RectTransform>(), 0.04f, 0.09f, 0.96f, 0.92f);
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 35f;

            var viewport = CreateUIElement("Viewport", scrollObj.transform);
            SetFullScreen(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateUIElement("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            scroll.content = contentRect;

            var vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(12, 12, 12, 12);
            vLayout.spacing = 10f;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Bio Text
            var bio = CreateText("BioText", content.transform, "Personal Bio...", 22, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetLayoutHeight(bio.gameObject, 80);
            ctrl.characterBioText = bio;

            // Physical Status Text
            var phys = CreateText("PhysText", content.transform, "Physical & Meridian Status...", 21, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.9f, 0.95f));
            SetLayoutHeight(phys.gameObject, 60);
            ctrl.physicalStatusText = phys;

            // Health & Fitness Bars
            ctrl.healthSlider = CreateStatBar("HealthBar", content.transform, "Health: 100%", new Color(0.85f, 0.25f, 0.30f), out ctrl.healthText);
            SetLayoutHeight(ctrl.healthSlider.gameObject, 36);

            ctrl.fitnessSlider = CreateStatBar("FitnessBar", content.transform, "Fitness: 50%", new Color(0.18f, 0.70f, 0.35f), out ctrl.fitnessText);
            SetLayoutHeight(ctrl.fitnessSlider.gameObject, 36);

            // Military Rank & Department
            var pRealm = CreateText("MilitaryRankStandingText", content.transform, "Military Rank & Department...", 22, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.95f, 0.85f, 0.35f));
            SetLayoutHeight(pRealm.gameObject, 55);
            ctrl.powerRealmText = pRealm;

            ctrl.qiProgressSlider = CreateStatBar("CombatReadinessBar", content.transform, "Combat Readiness Index: 100%", new Color(0.25f, 0.65f, 0.40f), out ctrl.qiProgressText);
            SetLayoutHeight(ctrl.qiProgressSlider.gameObject, 36);

            var talent = CreateText("TacticalAptitudeText", content.transform, "Technical Aptitude (AFQT): 85/100", 21, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            SetLayoutHeight(talent.gameObject, 30);
            ctrl.talentScoreText = talent;

            // Military Record
            var mil = CreateText("MilText", content.transform, "Military Service Record...", 22, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetLayoutHeight(mil.gameObject, 75);
            ctrl.militaryRecordText = mil;

            ctrl.combatSkillSlider = CreateStatBar("CombatBar", content.transform, "Combat: 30%", new Color(0.90f, 0.55f, 0.15f), out ctrl.combatSkillText);
            SetLayoutHeight(ctrl.combatSkillSlider.gameObject, 36);

            ctrl.intellectSlider = CreateStatBar("IntellectBar", content.transform, "Intellect: 50%", new Color(0.20f, 0.60f, 0.95f), out ctrl.intellectText);
            SetLayoutHeight(ctrl.intellectSlider.gameObject, 36);

            // Dynasty
            var dyn = CreateText("DynText", content.transform, "Dynasty...", 21, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.8f, 0.95f));
            SetLayoutHeight(dyn.gameObject, 60);
            ctrl.dynasticStatusText = dyn;

            // Close button
            var closeBtn = CreateButton("CloseBtn", box.transform, "CLOSE PROFILE", new Color(0.35f, 0.40f, 0.45f));
            SetRect(closeBtn.GetComponent<RectTransform>(), 0.30f, 0.02f, 0.70f, 0.075f);
            ctrl.closeButton = closeBtn.GetComponent<Button>();

            overlay.SetActive(false);
            modalObj.SetActive(true);
        }

        private static void CreateFinanceModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("FinanceModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<FinanceModal>();
            hudView.financeModal = ctrl;

            var overlay = CreateUIElement("FinanceOverlay", modalObj.transform);
            SetFullScreen(overlay.GetComponent<RectTransform>());
            var dimBg = overlay.AddComponent<Image>();
            dimBg.color = new Color(0f, 0f, 0f, 0.92f);
            ctrl.modalRoot = overlay;
            ctrl.modalBackdropImage = dimBg;

            var box = CreateUIElement("FinanceBox", overlay.transform);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.05f, 0.06f);
            boxRect.anchorMax = new Vector2(0.95f, 0.94f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            var boxBg = box.AddComponent<Image>();
            boxBg.color = new Color(0.10f, 0.13f, 0.18f);
            ctrl.dialogPanelImage = boxBg;

            var title = CreateText("Title", box.transform, "FINANCES & TREASURY", 36, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.2f, 1f, 0.6f));
            SetRect(title.GetComponent<RectTransform>(), 0.05f, 0.93f, 0.95f, 0.99f);
            ctrl.titleText = title;

            var scrollObj = CreateUIElement("FinanceScroll", box.transform);
            SetRect(scrollObj.GetComponent<RectTransform>(), 0.04f, 0.09f, 0.96f, 0.92f);
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 35f;

            var viewport = CreateUIElement("Viewport", scrollObj.transform);
            SetFullScreen(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateUIElement("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            scroll.content = contentRect;

            var vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(12, 12, 12, 12);
            vLayout.spacing = 10f;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Liquid balances
            var cashLabel = CreateText("CashLabel", content.transform, "Liquid Cash: $2,000", 28, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.2f, 1f, 0.6f));
            SetLayoutHeight(cashLabel.gameObject, 36);
            ctrl.cashBalanceText = cashLabel;

            var cmpLabel = CreateText("CMPLabel", content.transform, "Combat Merit Points: 0 CMP", 24, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.9f, 0.5f, 1f));
            SetLayoutHeight(cmpLabel.gameObject, 32);
            ctrl.cmpBalanceText = cmpLabel;

            // Monthly Breakdown
            var mBreak = CreateText("BreakdownText", content.transform, "Monthly Breakdown...", 22, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetLayoutHeight(mBreak.gameObject, 130);
            ctrl.monthlyBreakdownText = mBreak;

            // Family Status
            var fStat = CreateText("FamilyStatusText", content.transform, "Family Wealth Status...", 22, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            SetLayoutHeight(fStat.gameObject, 90);
            ctrl.familyWealthStatusText = fStat;

            // Remittance buttons
            var rLabel = CreateText("RemittanceHeader", content.transform, "<b>FAMILY REMITTANCES:</b>", 22, TextAlignmentOptions.Left, FontStyles.Bold, new Color(1f, 0.85f, 0.3f));
            SetLayoutHeight(rLabel.gameObject, 28);

            var send200 = CreateButton("Send200Btn", content.transform, "SEND $200 (EMERGENCY FOOD CARE)", new Color(0.18f, 0.55f, 0.35f));
            SetLayoutHeight(send200, 52);
            ctrl.send200Button = send200.GetComponent<Button>();

            var send500 = CreateButton("Send500Btn", content.transform, "SEND $500 (SUBSTANTIAL FAMILY SUPPORT)", new Color(0.20f, 0.60f, 0.40f));
            SetLayoutHeight(send500, 52);
            ctrl.send500Button = send500.GetComponent<Button>();

            var send1000 = CreateButton("Send1000Btn", content.transform, "SEND $1,000 (CLAN WAR CHEST DONATION)", new Color(0.70f, 0.50f, 0.15f));
            SetLayoutHeight(send1000, 52);
            ctrl.send1000Button = send1000.GetComponent<Button>();

            // Imperial Logistics & Bonds
            var wbHeader = CreateText("WBHeader", content.transform, "<b>IMPERIAL MILITARY INVESTMENTS:</b>", 22, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.4f, 0.8f, 1f));
            SetLayoutHeight(wbHeader.gameObject, 28);

            var buyWB = CreateButton("BuyWBBtn", content.transform, "BUY $500 IMPERIAL WAR BOND (+Merit & Influence)", new Color(0.25f, 0.45f, 0.75f));
            SetLayoutHeight(buyWB, 52);
            ctrl.buyWarBondButton = buyWB.GetComponent<Button>();

            var advBtn = CreateButton("AdvanceBtn", content.transform, "REQUEST $300 EMERGENCY PAY ADVANCE (-Merit)", new Color(0.60f, 0.35f, 0.20f));
            SetLayoutHeight(advBtn, 52);
            ctrl.emergencyAdvanceButton = advBtn.GetComponent<Button>();

            var wbDesc = CreateText("WBDesc", content.transform, "War bonds support the Imperial war effort.", 19, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.7f, 0.75f, 0.8f));
            SetLayoutHeight(wbDesc.gameObject, 30);
            ctrl.warBondStatusText = wbDesc;

            // Close button
            var closeBtn = CreateButton("CloseBtn", box.transform, "CLOSE TREASURY", new Color(0.35f, 0.40f, 0.45f));
            SetRect(closeBtn.GetComponent<RectTransform>(), 0.30f, 0.02f, 0.70f, 0.075f);
            ctrl.closeButton = closeBtn.GetComponent<Button>();

            overlay.SetActive(false);
            modalObj.SetActive(true);
        }

        private static void CreateRelationshipsModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("RelationshipsModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<RelationshipsModal>();
            hudView.relationshipsModal = ctrl;

            var overlay = CreateUIElement("RelationshipsOverlay", modalObj.transform);
            SetFullScreen(overlay.GetComponent<RectTransform>());
            var dimBg = overlay.AddComponent<Image>();
            dimBg.color = new Color(0f, 0f, 0f, 0.92f);
            ctrl.modalRoot = overlay;
            ctrl.modalBackdropImage = dimBg;

            var box = CreateUIElement("RelationshipsBox", overlay.transform);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.05f, 0.06f);
            boxRect.anchorMax = new Vector2(0.95f, 0.94f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            var boxBg = box.AddComponent<Image>();
            boxBg.color = new Color(0.10f, 0.13f, 0.18f);
            ctrl.dialogPanelImage = boxBg;

            var title = CreateText("Title", box.transform, "HOUSE & COMRADE RELATIONS", 36, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.82f, 0.25f));
            SetRect(title.GetComponent<RectTransform>(), 0.05f, 0.93f, 0.95f, 0.99f);
            ctrl.titleText = title;

            // Dual Switcher Row (Friend & Clan)
            var tabRow = CreateUIElement("TabRow", box.transform);
            SetRect(tabRow.GetComponent<RectTransform>(), 0.05f, 0.86f, 0.95f, 0.925f);
            var tLayout = tabRow.AddComponent<HorizontalLayoutGroup>();
            tLayout.spacing = 12f;
            tLayout.childControlHeight = true;
            tLayout.childControlWidth = true;
            tLayout.childForceExpandHeight = false;
            tLayout.childForceExpandWidth = true;

            var clanTabBtn = CreateButton("ClanTabBtn", tabRow.transform, "CLAN & FAMILY", new Color(0.70f, 0.50f, 0.15f));
            ctrl.clanTabButton = clanTabBtn.GetComponent<Button>();
            ctrl.clanTabButtonText = clanTabBtn.GetComponentInChildren<TextMeshProUGUI>();

            var friendsTabBtn = CreateButton("FriendsTabBtn", tabRow.transform, "FRIENDS & COMRADES", new Color(0.20f, 0.45f, 0.75f));
            ctrl.friendsTabButton = friendsTabBtn.GetComponent<Button>();
            ctrl.friendsTabButtonText = friendsTabBtn.GetComponentInChildren<TextMeshProUGUI>();

            // --- 1. CLAN PANEL ---
            var clanPanel = CreateUIElement("ClanPanel", box.transform);
            SetRect(clanPanel.GetComponent<RectTransform>(), 0.04f, 0.09f, 0.96f, 0.85f);
            ctrl.clanPanel = clanPanel;

            var clanSummary = CreateText("ClanSummary", clanPanel.transform, "Clan Overview...", 21, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetRect(clanSummary.GetComponent<RectTransform>(), 0.02f, 0.88f, 0.98f, 0.99f);
            ctrl.clanOverviewText = clanSummary;

            var clanScrollObj = CreateUIElement("ClanScroll", clanPanel.transform);
            SetRect(clanScrollObj.GetComponent<RectTransform>(), 0.02f, 0.10f, 0.98f, 0.87f);
            var cScroll = clanScrollObj.AddComponent<ScrollRect>();
            cScroll.horizontal = false;
            cScroll.vertical = true;
            cScroll.scrollSensitivity = 35f;

            var cViewport = CreateUIElement("Viewport", clanScrollObj.transform);
            SetFullScreen(cViewport.GetComponent<RectTransform>());
            cViewport.AddComponent<RectMask2D>();
            cScroll.viewport = cViewport.GetComponent<RectTransform>();

            var cContent = CreateUIElement("Content", cViewport.transform);
            var cContentRect = cContent.GetComponent<RectTransform>();
            cContentRect.anchorMin = new Vector2(0f, 1f);
            cContentRect.anchorMax = new Vector2(1f, 1f);
            cContentRect.pivot = new Vector2(0.5f, 1f);
            cContentRect.offsetMin = Vector2.zero;
            cContentRect.offsetMax = Vector2.zero;
            cScroll.content = cContentRect;

            var cvLayout = cContent.AddComponent<VerticalLayoutGroup>();
            cvLayout.padding = new RectOffset(8, 8, 8, 8);
            cvLayout.spacing = 8f;
            cvLayout.childControlHeight = true;
            cvLayout.childControlWidth = true;
            cvLayout.childForceExpandHeight = false;
            cvLayout.childForceExpandWidth = true;

            var cFitter = cContent.AddComponent<ContentSizeFitter>();
            cFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ctrl.clanMembersContainer = cContent.transform;

            var clanHallBtn = CreateButton("FullClanHallBtn", clanPanel.transform, "OPEN ANCESTRAL CLAN HALL", new Color(0.65f, 0.45f, 0.15f));
            SetRect(clanHallBtn.GetComponent<RectTransform>(), 0.15f, 0.01f, 0.85f, 0.085f);
            ctrl.openFullClanHallButton = clanHallBtn.GetComponent<Button>();

            // --- 2. FRIENDS PANEL ---
            var friendsPanel = CreateUIElement("FriendsPanel", box.transform);
            SetRect(friendsPanel.GetComponent<RectTransform>(), 0.04f, 0.09f, 0.96f, 0.85f);
            ctrl.friendsPanel = friendsPanel;

            var friendsSummary = CreateText("FriendsSummary", friendsPanel.transform, "Comrades & Peers...", 21, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetRect(friendsSummary.GetComponent<RectTransform>(), 0.02f, 0.90f, 0.98f, 0.99f);
            ctrl.friendsOverviewText = friendsSummary;

            var fScrollObj = CreateUIElement("FriendsScroll", friendsPanel.transform);
            SetRect(fScrollObj.GetComponent<RectTransform>(), 0.02f, 0.02f, 0.98f, 0.89f);
            var fScroll = fScrollObj.AddComponent<ScrollRect>();
            fScroll.horizontal = false;
            fScroll.vertical = true;
            fScroll.scrollSensitivity = 35f;

            var fViewport = CreateUIElement("Viewport", fScrollObj.transform);
            SetFullScreen(fViewport.GetComponent<RectTransform>());
            fViewport.AddComponent<RectMask2D>();
            fScroll.viewport = fViewport.GetComponent<RectTransform>();

            var fContent = CreateUIElement("Content", fViewport.transform);
            var fContentRect = fContent.GetComponent<RectTransform>();
            fContentRect.anchorMin = new Vector2(0f, 1f);
            fContentRect.anchorMax = new Vector2(1f, 1f);
            fContentRect.pivot = new Vector2(0.5f, 1f);
            fContentRect.offsetMin = Vector2.zero;
            fContentRect.offsetMax = Vector2.zero;
            fScroll.content = fContentRect;

            var fvLayout = fContent.AddComponent<VerticalLayoutGroup>();
            fvLayout.padding = new RectOffset(8, 8, 8, 8);
            fvLayout.spacing = 8f;
            fvLayout.childControlHeight = true;
            fvLayout.childControlWidth = true;
            fvLayout.childForceExpandHeight = false;
            fvLayout.childForceExpandWidth = true;

            var fFitter = fContent.AddComponent<ContentSizeFitter>();
            fFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ctrl.friendsContainer = fContent.transform;

            // Close button
            var closeBtn = CreateButton("CloseBtn", box.transform, "CLOSE RELATIONS", new Color(0.35f, 0.40f, 0.45f));
            SetRect(closeBtn.GetComponent<RectTransform>(), 0.30f, 0.02f, 0.70f, 0.075f);
            ctrl.closeButton = closeBtn.GetComponent<Button>();

            overlay.SetActive(false);
            modalObj.SetActive(true);
        }

        private static void CreateActivitiesModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("ActivitiesModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<ActivitiesModal>();
            hudView.activitiesModal = ctrl;

            var overlay = CreateUIElement("ActivitiesOverlay", modalObj.transform);
            SetFullScreen(overlay.GetComponent<RectTransform>());
            var dimBg = overlay.AddComponent<Image>();
            dimBg.color = new Color(0f, 0f, 0f, 0.92f);
            ctrl.modalRoot = overlay;
            ctrl.modalBackdropImage = dimBg;

            var box = CreateUIElement("ActivitiesBox", overlay.transform);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.04f, 0.05f);
            boxRect.anchorMax = new Vector2(0.96f, 0.95f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            var boxBg = box.AddComponent<Image>();
            boxBg.color = new Color(0.10f, 0.13f, 0.18f);
            ctrl.dialogPanelImage = boxBg;

            var title = CreateText("Title", box.transform, "ACTIVITIES & OPERATIONS", 36, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetRect(title.GetComponent<RectTransform>(), 0.05f, 0.93f, 0.95f, 0.99f);
            ctrl.titleText = title;

            var subtitle = CreateText("Subtitle", box.transform, "Activity Status...", 21, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            SetRect(subtitle.GetComponent<RectTransform>(), 0.05f, 0.85f, 0.95f, 0.92f);
            ctrl.subtitleText = subtitle;

            var scrollObj = CreateUIElement("ActivitiesScroll", box.transform);
            SetRect(scrollObj.GetComponent<RectTransform>(), 0.04f, 0.09f, 0.96f, 0.84f);
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 35f;

            var viewport = CreateUIElement("Viewport", scrollObj.transform);
            SetFullScreen(viewport.GetComponent<RectTransform>());
            var vpImg = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateUIElement("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            scroll.content = contentRect;

            var vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(10, 10, 10, 10);
            vLayout.spacing = 10f;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ctrl.activitiesScrollRect = scroll;
            ctrl.activitiesContainer = content.transform;

            var closeBtn = CreateButton("CloseBtn", box.transform, "CLOSE ACTIVITIES", new Color(0.35f, 0.40f, 0.45f));
            SetRect(closeBtn.GetComponent<RectTransform>(), 0.30f, 0.02f, 0.70f, 0.075f);
            ctrl.closeButton = closeBtn.GetComponent<Button>();

            overlay.SetActive(false);
            modalObj.SetActive(true);
        }

        private static void CreateCareerDossierModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("CareerDossierModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<CareerDossierModal>();
            hudView.careerDossierModal = ctrl;

            var overlay = CreateUIElement("DossierOverlay", modalObj.transform);
            SetFullScreen(overlay.GetComponent<RectTransform>());
            var dimBg = overlay.AddComponent<Image>();
            dimBg.color = new Color(0.04f, 0.06f, 0.08f, 0.95f);
            ctrl.modalRoot = overlay;
            ctrl.modalBackdrop = dimBg;

            var box = CreateUIElement("DossierBox", overlay.transform);
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.05f, 0.05f);
            boxRect.anchorMax = new Vector2(0.95f, 0.95f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            var boxBg = box.AddComponent<Image>();
            boxBg.color = new Color(0.09f, 0.12f, 0.16f);

            var title = CreateText("Title", box.transform, "OFFICIAL MILITARY SERVICE RECORD", 34, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.85f, 0.3f));
            SetRect(title.GetComponent<RectTransform>(), 0.05f, 0.92f, 0.95f, 0.98f);
            ctrl.headerTitleText = title;

            var scrollObj = CreateUIElement("DossierScroll", box.transform);
            SetRect(scrollObj.GetComponent<RectTransform>(), 0.04f, 0.10f, 0.96f, 0.91f);
            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 35f;

            var viewport = CreateUIElement("Viewport", scrollObj.transform);
            SetFullScreen(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateUIElement("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            scroll.content = contentRect;

            var vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(16, 16, 16, 16);
            vLayout.spacing = 14f;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Soldier Name & Rank
            var nameRank = CreateText("NameRankText", content.transform, "PRIVATE RECRUIT ETHAN VANCE (E-1)", 28, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0f, 1f, 0.7f));
            SetLayoutHeight(nameRank.gameObject, 45);
            ctrl.soldierNameRankText = nameRank;

            // Unit & Origin
            var unit = CreateText("UnitText", content.transform, "Unit: 1st Infantry Division | Origin: Farmhand", 22, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetLayoutHeight(unit.gameObject, 38);
            ctrl.unitAndOriginText = unit;

            // Status Text
            var status = CreateText("StatusText", content.transform, "[STATUS: ACTIVE SERVICE RECORD]", 24, TextAlignmentOptions.Left, FontStyles.Bold, new Color(1f, 0.4f, 0.4f));
            SetLayoutHeight(status.gameObject, 55);
            ctrl.serviceStatusText = status;

            // Service Years & Finance
            var stats = CreateText("ServiceStatsText", content.transform, "Years Served: 0.0 Years | Final Balance: $0", 22, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            SetLayoutHeight(stats.gameObject, 50);
            ctrl.serviceYearsText = stats;

            // Combat & Promotions
            var combatStats = CreateText("CombatStatsText", content.transform, "Promotions: 0 | Combat: 35%", 22, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            SetLayoutHeight(combatStats.gameObject, 50);
            ctrl.combatToursText = combatStats;

            // Medals Section Title
            var medalsTitle = CreateText("MedalsTitle", content.transform, "SERVICE DECORATIONS & HONORS:", 24, TextAlignmentOptions.Left, FontStyles.Bold, new Color(1f, 0.85f, 0.35f));
            SetLayoutHeight(medalsTitle.gameObject, 36);

            // Medals List
            var medalsList = CreateText("MedalsListText", content.transform, "No decorations recorded.", 21, TextAlignmentOptions.Left, FontStyles.Normal, Color.white);
            SetLayoutHeight(medalsList.gameObject, 110);
            ctrl.medalsRackText = medalsList;

            // Final Legacy Score
            var scoreText = CreateText("ScoreText", content.transform, "CAREER LEGACY SCORE: 0 PTS", 26, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.85f, 0.25f));
            SetLayoutHeight(scoreText.gameObject, 45);
            ctrl.finalScoreText = scoreText;

            // Buttons
            var btnRow = CreateUIElement("BtnRow", box.transform);
            SetRect(btnRow.GetComponent<RectTransform>(), 0.05f, 0.02f, 0.95f, 0.085f);
            var hLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20f;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;
            hLayout.childForceExpandHeight = true;
            hLayout.childForceExpandWidth = true;

            var enlistBtn = CreateButton("EnlistBtn", btnRow.transform, "ENLIST NEW RECRUIT", new Color(0.18f, 0.65f, 0.35f));
            ctrl.enlistNewRecruitButton = enlistBtn.GetComponent<Button>();

            var closeBtn = CreateButton("CloseBtn", btnRow.transform, "CLOSE RECORD", new Color(0.35f, 0.40f, 0.45f));
            ctrl.closeDossierButton = closeBtn.GetComponent<Button>();

            overlay.SetActive(false);
            modalObj.SetActive(true);
        }

        private static void CreateMailCallModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("MailCallModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<MailCallModal>();
            hudView.mailCallModal = ctrl;
            ctrl.EnsureUIHierarchy();
            if (ctrl.modalRoot != null) ctrl.modalRoot.SetActive(false);
            modalObj.SetActive(true);
        }

        private static void CreateMedalRibbonRackModal(Transform parent, MainHUDView hudView)
        {
            var modalObj = CreateUIElement("MedalRibbonRackModal", parent);
            SetFullScreen(modalObj.GetComponent<RectTransform>());
            var ctrl = modalObj.AddComponent<MedalRibbonRackModal>();
            hudView.medalRibbonRackModal = ctrl;
            ctrl.EnsureUIStructure();
            if (ctrl.modalRoot != null) ctrl.modalRoot.SetActive(false);
            modalObj.SetActive(true);
        }


        private static Slider CreateSlider(string name, Transform parent, float minVal, float maxVal, float initialVal, Color fillColor)
        {
            var sliderObj = CreateUIElement(name, parent);
            var bgImg = sliderObj.AddComponent<Image>();
            bgImg.color = new Color(0.14f, 0.18f, 0.24f);

            var fillArea = CreateUIElement("Fill Area", sliderObj.transform);
            var faRect = fillArea.GetComponent<RectTransform>();
            faRect.anchorMin = Vector2.zero;
            faRect.anchorMax = Vector2.one;
            faRect.offsetMin = Vector2.zero;
            faRect.offsetMax = Vector2.zero;

            var fill = CreateUIElement("Fill", fillArea.transform);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;

            var slider = sliderObj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = minVal;
            slider.maxValue = maxVal;
            slider.value = initialVal;
            slider.interactable = true;

            return slider;
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
