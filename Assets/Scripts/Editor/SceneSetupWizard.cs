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
            cam.backgroundColor = new Color(0.08f, 0.10f, 0.12f); // Tactical Dark Slate
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

            // Controller for Opening Scene & Menus
            var openingCtrl = canvasObj.AddComponent<OpeningSceneController>();

            // Root Canvas Panel
            var rootPanel = CreateUIElement("RootPanel", canvasObj.transform);
            SetFullScreen(rootPanel.GetComponent<RectTransform>());

            // ==========================================
            // 5. MAIN HUD PANEL (Active Gameplay View)
            // ==========================================
            var mainHUDPanel = CreateUIElement("MainHUDPanel", rootPanel.transform);
            SetFullScreen(mainHUDPanel.GetComponent<RectTransform>());
            var hudView = mainHUDPanel.AddComponent<MainHUDView>();
            openingCtrl.mainHUDPanel = mainHUDPanel;

            // --- HEADER PANEL ---
            var headerPanel = CreateUIElement("HeaderPanel", mainHUDPanel.transform);
            var headerRect = headerPanel.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.78f);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.offsetMin = new Vector2(40, 20);
            headerRect.offsetMax = new Vector2(-40, -20);
            var headerBg = headerPanel.AddComponent<Image>();
            headerBg.color = new Color(0.15f, 0.18f, 0.22f);

            var nameText = CreateText("NameLabel", headerPanel.transform, "Ethan Vance", 48, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            SetRect(nameText.GetComponent<RectTransform>(), 0.05f, 0.65f, 0.60f, 0.95f);
            hudView.nameLabel = nameText;

            // Top Menu Button
            var menuBtnObj = CreateButton("MenuButton", headerPanel.transform, "MENU", new Color(0.25f, 0.35f, 0.45f));
            var menuBtnRect = menuBtnObj.GetComponent<RectTransform>();
            SetRect(menuBtnRect, 0.80f, 0.65f, 0.96f, 0.92f);
            hudView.returnToMenuButton = menuBtnObj.GetComponent<Button>();
            openingCtrl.returnToMenuButton = menuBtnObj.GetComponent<Button>();

            var nobleText = CreateText("NobleTitleLabel", headerPanel.transform, "Commoner", 34, TextAlignmentOptions.TopRight, FontStyles.Bold, new Color(1f, 0.85f, 0.3f));
            SetRect(nobleText.GetComponent<RectTransform>(), 0.50f, 0.65f, 0.78f, 0.95f);
            hudView.nobleTitleLabel = nobleText;

            var ageDateText = CreateText("AgeDateLabel", headerPanel.transform, "Age 0.0 | Year 2008 S1 [YOUTH PHASE]", 32, TextAlignmentOptions.Left, FontStyles.Normal, new Color(0.8f, 0.85f, 0.9f));
            SetRect(ageDateText.GetComponent<RectTransform>(), 0.05f, 0.35f, 0.55f, 0.65f);
            hudView.ageAndCalendarLabel = ageDateText;

            var rankText = CreateText("RankLabel", headerPanel.transform, "Infant", 34, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.4f, 0.8f, 1f));
            SetRect(rankText.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.55f, 0.35f);
            hudView.rankLabel = rankText;

            var payText = CreateText("PayLabel", headerPanel.transform, "Family Dependent ($0/mo)", 32, TextAlignmentOptions.Right, FontStyles.Bold, new Color(0.6f, 0.65f, 0.7f));
            SetRect(payText.GetComponent<RectTransform>(), 0.55f, 0.05f, 0.95f, 0.35f);
            hudView.monthlyPayLabel = payText;

            // --- FINANCES & CULTIVATION BAR ---
            var subHeader = CreateUIElement("SubHeader", mainHUDPanel.transform);
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
            var feedContainer = CreateUIElement("FeedContainer", mainHUDPanel.transform);
            var feedRect = feedContainer.GetComponent<RectTransform>();
            feedRect.anchorMin = new Vector2(0, 0.23f);
            feedRect.anchorMax = new Vector2(1, 0.67f);
            feedRect.offsetMin = new Vector2(40, 8);
            feedRect.offsetMax = new Vector2(-40, -8);
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

            // --- BITLIFE 4-GAUGE STATS BAR PANEL ---
            var statsPanel = CreateUIElement("StatsBarPanel", mainHUDPanel.transform);
            var statsPanelRect = statsPanel.GetComponent<RectTransform>();
            statsPanelRect.anchorMin = new Vector2(0, 0.115f);
            statsPanelRect.anchorMax = new Vector2(1, 0.22f);
            statsPanelRect.offsetMin = new Vector2(40, 4);
            statsPanelRect.offsetMax = new Vector2(-40, -4);
            var statsBg = statsPanel.AddComponent<Image>();
            statsBg.color = new Color(0.10f, 0.13f, 0.17f);

            // 1. Health Bar (Top Left)
            var healthBar = CreateStatBar("HealthBar", statsPanel.transform, "❤️ Health: 100%", new Color(0.85f, 0.25f, 0.30f), out var healthText);
            SetRect(healthBar.GetComponent<RectTransform>(), 0.02f, 0.54f, 0.48f, 0.94f);
            hudView.healthSlider = healthBar;
            hudView.healthBarText = healthText;

            // 2. Fitness Bar (Top Right)
            var fitnessBar = CreateStatBar("FitnessBar", statsPanel.transform, "🏃 Fitness: 50%", new Color(0.18f, 0.70f, 0.35f), out var fitnessText);
            SetRect(fitnessBar.GetComponent<RectTransform>(), 0.52f, 0.54f, 0.98f, 0.94f);
            hudView.fitnessSlider = fitnessBar;
            hudView.fitnessBarText = fitnessText;

            // 3. Combat Skill Bar (Bottom Left)
            var combatBar = CreateStatBar("CombatBar", statsPanel.transform, "⚔️ Combat: 30%", new Color(0.90f, 0.55f, 0.15f), out var combatText);
            SetRect(combatBar.GetComponent<RectTransform>(), 0.02f, 0.06f, 0.48f, 0.46f);
            hudView.combatSkillSlider = combatBar;
            hudView.combatBarText = combatText;

            // 4. Intellect Bar (Bottom Right)
            var intellectBar = CreateStatBar("IntellectBar", statsPanel.transform, "🧠 Intellect: 50%", new Color(0.20f, 0.60f, 0.95f), out var intellectText);
            SetRect(intellectBar.GetComponent<RectTransform>(), 0.52f, 0.06f, 0.98f, 0.46f);
            hudView.intellectSlider = intellectBar;
            hudView.intellectBarText = intellectText;

            // --- BOTTOM ACTION BUTTONS ---
            var bottomBar = CreateUIElement("BottomBar", mainHUDPanel.transform);
            var bottomRect = bottomBar.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0.02f);
            bottomRect.anchorMax = new Vector2(1, 0.105f);
            bottomRect.offsetMin = new Vector2(40, 4);
            bottomRect.offsetMax = new Vector2(-40, -4);

            var advanceBtnObj = CreateButton("AdvanceSemesterButton", bottomBar.transform, "GROW UP (+)", new Color(0.18f, 0.65f, 0.35f));
            SetRect(advanceBtnObj.GetComponent<RectTransform>(), 0.02f, 0.10f, 0.48f, 0.90f);
            hudView.advanceSemesterButton = advanceBtnObj.GetComponent<Button>();

            var sendBtnObj = CreateButton("SendMoneyHomeButton", bottomBar.transform, "📋 ACTIVITIES", new Color(0.20f, 0.45f, 0.75f));
            SetRect(sendBtnObj.GetComponent<RectTransform>(), 0.52f, 0.10f, 0.98f, 0.90f);
            hudView.sendMoneyHomeButton = sendBtnObj.GetComponent<Button>();

            var testBtnObj = CreateButton("TestTalentButton", bottomBar.transform, "TEST (E-4)", new Color(0.25f, 0.50f, 0.85f));
            SetRect(testBtnObj.GetComponent<RectTransform>(), 0.75f, 0.10f, 0.98f, 0.90f);
            hudView.testTalentButton = testBtnObj.GetComponent<Button>();
            testBtnObj.SetActive(false); // Hidden during youth phase

            // ==========================================
            // 6. OPENING SCREEN PANEL (Title / Main Menu)
            // ==========================================
            var openingPanel = CreateUIElement("OpeningScreenPanel", rootPanel.transform);
            SetFullScreen(openingPanel.GetComponent<RectTransform>());
            var openingBg = openingPanel.AddComponent<Image>();
            openingBg.color = new Color(0.07f, 0.09f, 0.12f);
            openingCtrl.openingScreenPanel = openingPanel;

            // Title Banner
            var titleText = CreateText("GameTitle", openingPanel.transform, "VALERIAN VANGUARD", 68, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.95f, 0.80f, 0.30f));
            SetRect(titleText.GetComponent<RectTransform>(), 0.05f, 0.72f, 0.95f, 0.85f);

            var subtitleText = CreateText("Subtitle", openingPanel.transform, "A Multi-Generational Military Life Simulation RPG\n<color=#00D4FF>The Great Energy Revival</color>", 32, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.8f, 0.85f, 0.9f));
            SetRect(subtitleText.GetComponent<RectTransform>(), 0.05f, 0.62f, 0.95f, 0.71f);

            var recordsLabel = CreateText("RecordsLabel", openingPanel.transform, "Active Military Service Records: 0", 28, TextAlignmentOptions.Center, FontStyles.Italic, new Color(0.6f, 0.7f, 0.8f));
            SetRect(recordsLabel.GetComponent<RectTransform>(), 0.05f, 0.55f, 0.95f, 0.60f);
            openingCtrl.totalLivesCountLabel = recordsLabel;

            // Option 1: Create New Life Button
            var createBtnObj = CreateButton("CreateNewLifeButton", openingPanel.transform, "CREATE NEW LIFE", new Color(0.18f, 0.65f, 0.35f));
            SetRect(createBtnObj.GetComponent<RectTransform>(), 0.15f, 0.40f, 0.85f, 0.50f);
            openingCtrl.createNewLifeButton = createBtnObj.GetComponent<Button>();

            // Option 2: Continue or Manage Lives Button (Dynamically switched in code!)
            var contBtnObj = CreateButton("ContinueOrManageButton", openingPanel.transform, "CONTINUE", new Color(0.20f, 0.45f, 0.75f));
            SetRect(contBtnObj.GetComponent<RectTransform>(), 0.15f, 0.27f, 0.85f, 0.37f);
            openingCtrl.continueOrManageButton = contBtnObj.GetComponent<Button>();
            openingCtrl.continueOrManageButtonText = contBtnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Footer Lore Hint
            var footerText = CreateText("FooterHint", openingPanel.transform, "Valerian Empire vs Iron Federation & Ouroboros Dominion\n12 Cultivation Tiers • Nobility Inheritance • Sibling Wars", 24, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.45f, 0.50f, 0.58f));
            SetRect(footerText.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.95f, 0.15f);

            // ==========================================
            // 7. BITLIFE-STYLE CHARACTER CREATION MODAL
            // ==========================================
            var creationModal = CreateUIElement("CharacterCreationModal", rootPanel.transform);
            SetFullScreen(creationModal.GetComponent<RectTransform>());
            var modalDimBg = creationModal.AddComponent<Image>();
            modalDimBg.color = new Color(0.07f, 0.09f, 0.13f, 1.0f); // 100% Solid Opaque to prevent any title bleed!
            openingCtrl.characterCreationModal = creationModal;

            // TOP HEADER BAR (Fixed outside scroll area)
            var topBar = CreateUIElement("TopHeaderBar", creationModal.transform);
            var topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 1f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.sizeDelta = new Vector2(0f, 110f);
            topBarRect.anchoredPosition = Vector2.zero;
            var topBarBg = topBar.AddComponent<Image>();
            topBarBg.color = new Color(0.11f, 0.14f, 0.20f, 1.0f);

            var cancelBtnObj = CreateButton("CancelCreationBtn", topBar.transform, "◄ CANCEL", new Color(0.40f, 0.18f, 0.22f));
            var cancelBtnRect = cancelBtnObj.GetComponent<RectTransform>();
            cancelBtnRect.anchorMin = new Vector2(0f, 0.5f);
            cancelBtnRect.anchorMax = new Vector2(0f, 0.5f);
            cancelBtnRect.pivot = new Vector2(0f, 0.5f);
            cancelBtnRect.anchoredPosition = new Vector2(30f, 0f);
            cancelBtnRect.sizeDelta = new Vector2(160f, 64f);
            openingCtrl.cancelCreationButton = cancelBtnObj.GetComponent<Button>();

            var createTitle = CreateText("ModalTitle", topBar.transform, "CUSTOM LIFE", 38, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.95f, 0.82f, 0.35f));
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

            var confirmBtnObj = CreateButton("ConfirmStartBtn", creationBottomBar.transform, "START NEW LIFE", new Color(0.15f, 0.68f, 0.35f));
            var confirmBtnRect = confirmBtnObj.GetComponent<RectTransform>();
            confirmBtnRect.anchorMin = Vector2.zero;
            confirmBtnRect.anchorMax = Vector2.one;
            confirmBtnRect.offsetMin = new Vector2(30f, 18f);
            confirmBtnRect.offsetMax = new Vector2(-30f, -18f);
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
            formLayout.padding = new RectOffset(30, 30, 20, 40);
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

            var randNameBtn = CreateButton("RandNameBtn", firstNameCard.transform, "🎲", new Color(0.24f, 0.35f, 0.48f));
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

            var maleBtn = CreateButton("MaleBtn", genderCard.transform, "♂  MALE", new Color(0.20f, 0.50f, 0.85f));
            var maleLayout = maleBtn.AddComponent<LayoutElement>();
            maleLayout.flexibleWidth = 1f;
            maleLayout.minWidth = 180;
            openingCtrl.maleGenderButton = maleBtn.GetComponent<Button>();
            openingCtrl.maleGenderText = maleBtn.GetComponentInChildren<TextMeshProUGUI>();

            var femaleBtn = CreateButton("FemaleBtn", genderCard.transform, "♀  FEMALE", new Color(0.18f, 0.22f, 0.28f));
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
            var randLookBtn = CreateButton("RandLookBtn", formContent.transform, "🎲  RANDOMIZE APPEARANCE", new Color(0.22f, 0.32f, 0.44f));
            SetLayoutHeight(randLookBtn, 65);
            openingCtrl.randomizeAppearanceButton = randLookBtn.GetComponent<Button>();

            // CARD 10: SOCIAL LINEAGE (FATE OF BIRTH)
            var linCard = CreateUIElement("FateOfBirthBox", formContent.transform);
            var linCardBg = linCard.AddComponent<Image>();
            linCardBg.color = new Color(0.10f, 0.13f, 0.18f);
            SetLayoutHeight(linCard, 140);

            var linCardText = CreateText("FateDesc", linCard.transform, 
                "<b>Social Lineage (Determined Randomly at Birth):</b>\n" +
                "• <b>80%</b> Commoner Grunt (Self-made climber, hidden talent)\n" +
                "• <b>14%</b> Baron Lineage  |  • <b>5%</b> Viscount Legend\n" +
                "• <color=#FFD700><b>1% Grand Duke Dynasty</b> (Ancestral Testing Stone & Ducal Guard!)</color>", 
                21, TextAlignmentOptions.MidlineLeft, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
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
            // 10. YOUTH ACTIVITIES MODAL
            // ==========================================
            var youthModalObj = CreateUIElement("YouthActivitiesModal", rootPanel.transform);
            SetFullScreen(youthModalObj.GetComponent<RectTransform>());
            var youthCtrl = youthModalObj.AddComponent<YouthActivitiesModal>();
            hudView.youthActivitiesModal = youthCtrl;

            var youthOverlay = CreateUIElement("YouthOverlay", youthModalObj.transform);
            SetFullScreen(youthOverlay.GetComponent<RectTransform>());
            var youthDimBg = youthOverlay.AddComponent<Image>();
            youthDimBg.color = new Color(0f, 0f, 0f, 0.92f);
            youthCtrl.modalRoot = youthOverlay;

            var youthBox = CreateUIElement("YouthBox", youthOverlay.transform);
            var ybRect = youthBox.GetComponent<RectTransform>();
            ybRect.anchorMin = new Vector2(0.04f, 0.05f);
            ybRect.anchorMax = new Vector2(0.96f, 0.95f);
            ybRect.offsetMin = Vector2.zero;
            ybRect.offsetMax = Vector2.zero;
            var youthBoxBg = youthBox.AddComponent<Image>();
            youthBoxBg.color = new Color(0.10f, 0.13f, 0.18f);

            var yTitle = CreateText("YouthTitle", youthBox.transform, "YOUTH ACTIVITIES (AGES 0-18)", 36, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetRect(yTitle.GetComponent<RectTransform>(), 0.05f, 0.93f, 0.95f, 0.99f);
            youthCtrl.titleText = yTitle;

            var ySummary = CreateText("YouthSummary", youthBox.transform, "Stats...", 22, TextAlignmentOptions.Center, FontStyles.Normal, new Color(0.85f, 0.90f, 0.95f));
            SetRect(ySummary.GetComponent<RectTransform>(), 0.05f, 0.82f, 0.95f, 0.92f);
            youthCtrl.youthStatsSummaryText = ySummary;

            // Scroll container for Activities
            var yScrollObj = CreateUIElement("ActivitiesScroll", youthBox.transform);
            SetRect(yScrollObj.GetComponent<RectTransform>(), 0.04f, 0.09f, 0.96f, 0.81f);
            var yScroll = yScrollObj.AddComponent<ScrollRect>();
            yScroll.horizontal = false;
            yScroll.vertical = true;
            yScroll.scrollSensitivity = 35f;

            var yViewport = CreateUIElement("Viewport", yScrollObj.transform);
            SetFullScreen(yViewport.GetComponent<RectTransform>());
            yViewport.AddComponent<RectMask2D>();
            yScroll.viewport = yViewport.GetComponent<RectTransform>();

            var yContent = CreateUIElement("Content", yViewport.transform);
            var yContentRect = yContent.GetComponent<RectTransform>();
            yContentRect.anchorMin = new Vector2(0f, 1f);
            yContentRect.anchorMax = new Vector2(1f, 1f);
            yContentRect.pivot = new Vector2(0.5f, 1f);
            yContentRect.offsetMin = Vector2.zero;
            yContentRect.offsetMax = Vector2.zero;
            yScroll.content = yContentRect;

            var yLayout = yContent.AddComponent<VerticalLayoutGroup>();
            yLayout.padding = new RectOffset(10, 10, 10, 10);
            yLayout.spacing = 12f;
            yLayout.childControlHeight = true;
            yLayout.childControlWidth = true;
            yLayout.childForceExpandHeight = false;
            yLayout.childForceExpandWidth = true;

            var yFitter = yContent.AddComponent<ContentSizeFitter>();
            yFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Activity Buttons (11 Interactive Choices)
            var yBtn1 = CreateButton("TemperingBtn", yContent.transform, "🏃 BODY TEMPERING (+Fitness & Marrow)", new Color(0.18f, 0.60f, 0.35f));
            SetLayoutHeight(yBtn1, 62);
            youthCtrl.bodyTemperingButton = yBtn1.GetComponent<Button>();

            var yBtn2 = CreateButton("StudyBtn", yContent.transform, "📚 STUDY STRATEGY & TACTICS (+Intellect)", new Color(0.20f, 0.45f, 0.75f));
            SetLayoutHeight(yBtn2, 62);
            youthCtrl.studyTacticsButton = yBtn2.GetComponent<Button>();

            var yBtnRange = CreateButton("RangeBtn", yContent.transform, "🎯 CADET FIRING RANGE (+Combat Skill & Marksmanship)", new Color(0.85f, 0.35f, 0.15f));
            SetLayoutHeight(yBtnRange, 62);
            youthCtrl.cadetFiringRangeButton = yBtnRange.GetComponent<Button>();

            var yBtnLib = CreateButton("LibraryBtn", yContent.transform, "📜 CLAN MARTIAL ARCHIVE (+Tempering & Qi Insight)", new Color(0.15f, 0.65f, 0.75f));
            SetLayoutHeight(yBtnLib, 62);
            youthCtrl.clanLibraryButton = yBtnLib.GetComponent<Button>();

            var yBtnJobs = CreateButton("OddJobsBtn", yContent.transform, "💼 DISTRICT ODD JOBS (+Earn Cash & Fitness)", new Color(0.70f, 0.50f, 0.20f));
            SetLayoutHeight(yBtnJobs, 62);
            youthCtrl.oddJobsButton = yBtnJobs.GetComponent<Button>();

            var yBtnHerb = CreateButton("ApothecaryBtn", yContent.transform, "🏥 APOTHECARY BONE BROTH (-$30, +Health & Bones)", new Color(0.25f, 0.65f, 0.45f));
            SetLayoutHeight(yBtnHerb, 62);
            youthCtrl.apothecaryButton = yBtnHerb.GetComponent<Button>();

            var yBtn3 = CreateButton("FamilyBtn", yContent.transform, "🏡 SPEND TIME WITH PARENTS (+Clan Favor)", new Color(0.75f, 0.55f, 0.20f));
            SetLayoutHeight(yBtn3, 62);
            youthCtrl.familyTimeButton = yBtn3.GetComponent<Button>();

            var yBtn4 = CreateButton("SiblingBtn", yContent.transform, "⚔️ SPAR WITH SIBLING LUCIAN (+Combat Skill)", new Color(0.65f, 0.25f, 0.30f));
            SetLayoutHeight(yBtn4, 62);
            youthCtrl.siblingSparButton = yBtn4.GetComponent<Button>();

            var yBtn5 = CreateButton("AllowanceBtn", yContent.transform, "💰 ASK PARENTS FOR ALLOWANCE (+Pocket Money)", new Color(0.85f, 0.60f, 0.15f));
            SetLayoutHeight(yBtn5, 62);
            youthCtrl.askAllowanceButton = yBtn5.GetComponent<Button>();

            var yBtn6 = CreateButton("MockExamBtn", yContent.transform, "📝 TAKE MOCK MILITARY EXAM (Age 17.5 Check)", new Color(0.25f, 0.55f, 0.85f));
            SetLayoutHeight(yBtn6, 62);
            youthCtrl.takeMockExamButton = yBtn6.GetComponent<Button>();

            var yBtn7 = CreateButton("FatherHelpBtn", yContent.transform, "👑 ASK FATHER FOR EXAM TUTORS & CLAN INFLUENCE", new Color(0.55f, 0.30f, 0.65f));
            SetLayoutHeight(yBtn7, 62);
            youthCtrl.askFatherExamHelpButton = yBtn7.GetComponent<Button>();

            // Close Button
            var yCloseBtn = CreateButton("CloseBtn", youthBox.transform, "◄ CLOSE", new Color(0.35f, 0.40f, 0.45f));
            SetRect(yCloseBtn.GetComponent<RectTransform>(), 0.30f, 0.02f, 0.70f, 0.075f);
            youthCtrl.closeButton = yCloseBtn.GetComponent<Button>();

            youthOverlay.SetActive(false);
            youthModalObj.SetActive(true);

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
