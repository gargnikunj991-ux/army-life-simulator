using System;
using System.Collections;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public enum MinigameTab
    {
        Checkpoint,
        FiringRange,
        BarracksDice
    }

    /// <summary>
    /// Interactive Tactical Minigames Modal.
    /// Encapsulates:
    /// 1. Checkpoint Desk (Papers, Please inspired inspection)
    /// 2. Firing Range Lane (Skill & reaction marksmanship qualification)
    /// 3. Barracks Dice (Craps 2d6 gambling ring)
    /// </summary>
    public class TacticalMinigamesModal : MonoBehaviour
    {
        public static TacticalMinigamesModal Instance { get; private set; }

        [Header("Modal Framework")]
        public GameObject modalRoot;
        public Image modalBackdrop;
        public Button closeModalButton;

        [Header("Navigation Tabs")]
        public Button tabCheckpointButton;
        public Button tabFiringRangeButton;
        public Button tabBarracksDiceButton;

        [Header("View Containers")]
        public GameObject checkpointViewRoot;
        public GameObject firingRangeViewRoot;
        public GameObject barracksDiceViewRoot;

        // =====================================================================
        // VIEW A: CHECKPOINT DESK INSPECTION
        // =====================================================================
        [Header("View A: Checkpoint Desk")]
        public TextMeshProUGUI checkpointCaseIdText;
        public TextMeshProUGUI vehicleInfoText;
        public TextMeshProUGUI passportDetailsText;
        public TextMeshProUGUI manifestDetailsText;
        public TextMeshProUGUI intelBulletinText;
        public TextMeshProUGUI checkpointVerdictText;

        public Button approvePassButton;
        public Button searchCargoButton;
        public Button detainDriverButton;
        public Button nextVehicleButton;

        // =====================================================================
        // VIEW B: FIRING RANGE LANE
        // =====================================================================
        [Header("View B: Firing Range Lane")]
        public TextMeshProUGUI rangeScoreboardText;
        public TextMeshProUGUI targetInfoText;
        public TextMeshProUGUI timerCountdownText;
        public TextMeshProUGUI trialFeedbackText;
        public GameObject certificatePanel;
        public TextMeshProUGUI certificateSummaryText;

        public Button fireShotButton;
        public Button holdFireButton;
        public Button startRangeButton;
        public Button closeCertificateButton;

        // =====================================================================
        // VIEW C: BARRACKS CRAPS / DICE TABLE
        // =====================================================================
        [Header("View C: Barracks Craps")]
        public TextMeshProUGUI diceOpponentText;
        public TextMeshProUGUI currentWagerText;
        public TextMeshProUGUI playerDiceText;
        public TextMeshProUGUI opponentDiceText;
        public TextMeshProUGUI diceOutcomeText;

        public Button wager20Button;
        public Button wager50Button;
        public Button wager100Button;
        public Button wager250Button;
        public Button rollDiceButton;

        // Internal State
        private MinigameTab currentTab = MinigameTab.Checkpoint;
        private CheckpointVehicleCase currentCase;
        private bool checkpointResolved;
        private bool isBindingsInitialized = false;

        // Firing Range State
        private List<TargetPopup> activeTargets;
        private int currentTargetIndex = 0;
        private int currentRangeScore = 0;
        private int maxRangeScore = 0;
        private float currentTargetTimer = 0f;
        private bool isTargetActive = false;
        private List<FiringTrialResult> rangeTrials = new List<FiringTrialResult>();

        // Dice State
        private long selectedWager = 50;
        private string activeOpponentName = "Deadeye";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            EnsureUIHierarchy();
            EnsureUIBindings();

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            BindEventListeners();
        }

        private void OnDisable()
        {
            UnbindEventListeners();
        }

        private void Update()
        {
            // Update active target countdown on firing range
            if (currentTab == MinigameTab.FiringRange && isTargetActive)
            {
                currentTargetTimer -= Time.deltaTime;
                if (timerCountdownText != null)
                {
                    timerCountdownText.text = $"<b>Window:</b> <color={(currentTargetTimer < 0.6f ? "#FF4444" : "#00FF66")}>{Mathf.Max(0f, currentTargetTimer):F2}s</color>";
                }

                if (currentTargetTimer <= 0f)
                {
                    OnTargetTimeout();
                }
            }
        }

        // =====================================================================
        // PUBLIC MODAL ENTRYPOINTS
        // =====================================================================

        public void OpenModal(MinigameTab tab = MinigameTab.Checkpoint)
        {
            EnsureUIHierarchy();
            EnsureUIBindings();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (modalRoot != null) modalRoot.SetActive(true);

            SwitchTab(tab);
        }

        public void OpenCheckpoint() => OpenModal(MinigameTab.Checkpoint);
        public void OpenFiringRange() => OpenModal(MinigameTab.FiringRange);
        public void OpenBarracksDice() => OpenModal(MinigameTab.BarracksDice);

        public void CloseModal()
        {
            isTargetActive = false;
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void SwitchTab(MinigameTab tab)
        {
            currentTab = tab;

            if (checkpointViewRoot != null) checkpointViewRoot.SetActive(tab == MinigameTab.Checkpoint);
            if (firingRangeViewRoot != null) firingRangeViewRoot.SetActive(tab == MinigameTab.FiringRange);
            if (barracksDiceViewRoot != null) barracksDiceViewRoot.SetActive(tab == MinigameTab.BarracksDice);

            switch (tab)
            {
                case MinigameTab.Checkpoint:
                    if (currentCase == null) SetupNewCheckpointCase();
                    break;
                case MinigameTab.FiringRange:
                    SetupFiringRange();
                    break;
                case MinigameTab.BarracksDice:
                    SetupBarracksDice();
                    break;
            }
        }

        // =====================================================================
        // SECTION 1: CHECKPOINT DESK INTERACTION
        // =====================================================================

        public void SetupNewCheckpointCase()
        {
            currentCase = CheckpointInspectionEngine.GenerateInspectionCase();
            checkpointResolved = false;

            if (checkpointCaseIdText != null)
            {
                checkpointCaseIdText.text = $"<b>GATE POST CASE:</b> {currentCase.caseId}";
            }

            if (vehicleInfoText != null)
            {
                vehicleInfoText.text = $"<b>Vehicle:</b> {currentCase.vehicleType}\n" +
                                       $"<b>Plate:</b> {currentCase.licensePlate}\n" +
                                       $"<b>Notes:</b> {currentCase.vehicleVisualNotes}";
            }

            if (passportDetailsText != null)
            {
                var p = currentCase.transitPassport;
                int currentYear = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2026;
                string sealDescription = p.officialSealValid ? "Embossed Imperial Eagle (7 Stars, Crisp)" : "Wax Stamp (5 Stars, Smudged Rim)";

                passportDetailsText.text =
                    $"<b>--- TRANSIT PASSPORT ---</b>\n" +
                    $"<b>Name:</b> {p.civilianName}\n" +
                    $"<b>ID:</b> {p.photoId}\n" +
                    $"<b>Issued District:</b> {p.issuedDistrict}\n" +
                    $"<b>Valid Until:</b> {p.expirationDate}\n" +
                    $"<b>Seal Imprint:</b> {sealDescription}\n" +
                    $"<b>Purpose:</b> {p.travelPurpose}\n\n" +
                    $"<size=80%><i>[REGIMENTAL ORDER] Current Year: {currentYear}. Authentic visas require 7-Star Crown Eagle and valid calendar date. Compare against Wanted Bulletin.</i></size>";
            }

            if (manifestDetailsText != null)
            {
                var m = currentCase.cargoManifest;
                manifestDetailsText.text =
                    $"<b>--- CARGO MANIFEST ---</b>\n" +
                    $"<b>Declared:</b> {m.declaredCargo}\n" +
                    $"<b>Weight:</b> {m.manifestWeightKg:N0} kg\n" +
                    $"<b>Destination:</b> {m.destination}\n" +
                    $"<i>Visual Check: Tarpaulins sealed with municipal lead stamps.</i>";
            }

            if (intelBulletinText != null)
            {
                if (!string.IsNullOrEmpty(currentCase.wantedBulletinDescription))
                {
                    intelBulletinText.text = $"<color=#FF4444><b>[WATCH BULLETIN ACTIVE]</b> {currentCase.wantedBulletinDescription}</color>";
                }
                else
                {
                    intelBulletinText.text = "<color=#AAAAAA>[No active bulletins for this driver]</color>";
                }
            }

            if (checkpointVerdictText != null)
            {
                checkpointVerdictText.text = "<b>STATUS:</b> Awaiting Guard Decision (Inspect documents carefully before deciding).";
            }

            SetCheckpointButtonsInteractable(true);
            if (nextVehicleButton != null) nextVehicleButton.gameObject.SetActive(false);
        }

        private void OnApprovePassClicked() => HandleCheckpointDecision(CheckpointDecision.ApprovePass);
        private void OnSearchCargoClicked() => HandleCheckpointDecision(CheckpointDecision.SearchCargo);
        private void OnDetainDriverClicked() => HandleCheckpointDecision(CheckpointDecision.DetainDriver);

        private void HandleCheckpointDecision(CheckpointDecision decision)
        {
            if (checkpointResolved || currentCase == null) return;
            checkpointResolved = true;

            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            var result = CheckpointInspectionEngine.ProcessPlayerDecision(currentCase, decision, soldier);

            if (checkpointVerdictText != null)
            {
                string color = result.success ? "#00FF66" : "#FF4444";
                checkpointVerdictText.text = $"<color={color}><b>[{result.headline}]</b></color>\n{result.narrative}\n" +
                                             $"<b>Reward / Penalty:</b> Discipline {(result.disciplineDelta >= 0 ? "+" : "")}{result.disciplineDelta:F0} | " +
                                             $"Bounty: +${result.cashBountyDelta:N0} | Promotion: {(result.promotionPointsDelta >= 0 ? "+" : "")}{result.promotionPointsDelta} pts";
            }

            SetCheckpointButtonsInteractable(false);
            if (nextVehicleButton != null) nextVehicleButton.gameObject.SetActive(true);
        }

        private void SetCheckpointButtonsInteractable(bool state)
        {
            if (approvePassButton != null) approvePassButton.interactable = state;
            if (searchCargoButton != null) searchCargoButton.interactable = state;
            if (detainDriverButton != null) detainDriverButton.interactable = state;
        }

        // =====================================================================
        // SECTION 2: FIRING RANGE INTERACTION
        // =====================================================================

        public void SetupFiringRange()
        {
            activeTargets = FiringRangeEngine.GenerateTrialTargets(10);
            maxRangeScore = FiringRangeEngine.CalculateMaxScore(activeTargets);
            currentTargetIndex = 0;
            currentRangeScore = 0;
            rangeTrials.Clear();

            if (certificatePanel != null) certificatePanel.SetActive(false);
            if (trialFeedbackText != null) trialFeedbackText.text = "<b>LANE STATUS:</b> Ready. Click [BEGIN QUALIFICATION] to start timed popups.";

            UpdateRangeScoreboard();
            ShowNextTarget();
        }

        private void ShowNextTarget()
        {
            if (currentTargetIndex >= activeTargets.Count)
            {
                CompleteQualificationCourse();
                return;
            }

            var target = activeTargets[currentTargetIndex];
            currentTargetTimer = target.popupWindowSeconds;
            isTargetActive = true;

            if (targetInfoText != null)
            {
                string typeColor = target.IsFriendly ? "#00FFAA" : "#FF6666";
                targetInfoText.text = $"<b>TARGET {target.targetId} OF {activeTargets.Count}:</b> " +
                                      $"<color={typeColor}><b>[{target.targetType}]</b></color>\n" +
                                      $"<b>Distance:</b> {target.distanceMeters:F0}m | <b>Value:</b> {target.pointValue} pts\n" +
                                      $"<i>{target.description}</i>";
            }

            if (fireShotButton != null) fireShotButton.interactable = true;
            if (holdFireButton != null) holdFireButton.interactable = true;

            UpdateRangeScoreboard();
        }

        private void OnFireShotClicked()
        {
            if (!isTargetActive || currentTargetIndex >= activeTargets.Count) return;
            isTargetActive = false;

            AudioManager.PlayRifleShot();

            var target = activeTargets[currentTargetIndex];
            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;

            // Fast player reaction time from button press
            float reaction = target.popupWindowSeconds - currentTargetTimer;
            var trial = FiringRangeEngine.EvaluateShot(target, soldier, playerFired: true, customReactionTime: reaction);

            ProcessTrialResult(trial);
        }

        private void OnHoldFireClicked()
        {
            if (!isTargetActive || currentTargetIndex >= activeTargets.Count) return;
            isTargetActive = false;

            AudioManager.PlayUiClick();

            var target = activeTargets[currentTargetIndex];
            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;

            var trial = FiringRangeEngine.EvaluateShot(target, soldier, playerFired: false);
            ProcessTrialResult(trial);
        }

        private void OnTargetTimeout()
        {
            if (!isTargetActive) return;
            isTargetActive = false;

            var target = activeTargets[currentTargetIndex];
            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;

            // Target dropped back down
            var trial = FiringRangeEngine.EvaluateShot(target, soldier, playerFired: false);
            ProcessTrialResult(trial);
        }

        private void ProcessTrialResult(FiringTrialResult trial)
        {
            rangeTrials.Add(trial);
            currentRangeScore += trial.pointsAwarded;
            if (currentRangeScore < 0) currentRangeScore = 0;

            if (trialFeedbackText != null)
            {
                string col = trial.pointsAwarded > 0 ? "#00FF66" : (trial.pointsAwarded < 0 ? "#FF4444" : "#AAAAAA");
                trialFeedbackText.text = $"<color={col}><b>{trial.feedback}</b></color>";
            }

            currentTargetIndex++;
            UpdateRangeScoreboard();

            // Brief delay or advance
            if (currentTargetIndex < activeTargets.Count)
            {
                ShowNextTarget();
            }
            else
            {
                CompleteQualificationCourse();
            }
        }

        private void CompleteQualificationCourse()
        {
            isTargetActive = false;
            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            var finalResult = FiringRangeEngine.CalculateQualification(currentRangeScore, maxRangeScore, rangeTrials);

            // Apply awards to soldier
            if (soldier != null)
            {
                soldier.marksmanshipScore = Mathf.Clamp(soldier.marksmanshipScore + finalResult.marksmanshipScoreGained, 0f, 100f);
                soldier.accumulatedPromotionPoints += finalResult.promotionPointsAwarded;
                soldier.cashBalance += finalResult.cashPrizeBonus;
                if (finalResult.badgeAwarded != "None")
                {
                    soldier.AddMedal(finalResult.badgeAwarded);
                }
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            }

            if (finalResult.tier != QualificationTier.Unqualified)
            {
                AudioManager.PlayBugleFanfare();
            }
            else
            {
                AudioManager.PlayAlertPing();
            }

            if (certificatePanel != null) certificatePanel.SetActive(true);

            if (certificateSummaryText != null)
            {
                certificateSummaryText.text =
                    $"<b>==============================================</b>\n" +
                    $"<b>IMPERIAL RIFLE MARKSMANSHIP QUALIFICATION</b>\n" +
                    $"<b>==============================================</b>\n" +
                    $"<b>Total Score:</b> {finalResult.totalScore} / {finalResult.maxPossibleScore} ({finalResult.scorePercentage:F0}%)\n" +
                    $"<b>Qualification Tier:</b> <color=#00FFAA><b>{finalResult.tier.ToString().ToUpper()}</b></color>\n" +
                    $"<b>Badge Conferred:</b> {finalResult.badgeAwarded}\n" +
                    $"<b>Imperial Cash Prize:</b> +${finalResult.cashPrizeBonus:N0}\n" +
                    $"<b>Promotion Points:</b> +{finalResult.promotionPointsAwarded} pts\n\n" +
                    $"<i>{finalResult.certificateSummary}</i>";
            }
        }

        private void UpdateRangeScoreboard()
        {
            if (rangeScoreboardText != null)
            {
                rangeScoreboardText.text = $"<b>Lane Score:</b> {currentRangeScore} / {maxRangeScore} pts | " +
                                           $"<b>Trial:</b> {Mathf.Min(currentTargetIndex + 1, activeTargets?.Count ?? 10)} / {activeTargets?.Count ?? 10}";
            }
        }

        // =====================================================================
        // SECTION 3: BARRACKS CRAPS & DICE TABLE
        // =====================================================================

        public void SetupBarracksDice()
        {
            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;

            if (soldier != null && soldier.squadMates != null && soldier.squadMates.Count > 0)
            {
                var randomMate = soldier.squadMates[UnityEngine.Random.Range(0, soldier.squadMates.Count)];
                activeOpponentName = randomMate.nickname;
            }
            else
            {
                activeOpponentName = "Deadeye";
            }

            if (diceOpponentText != null)
            {
                diceOpponentText.text = $"<b>Opponent:</b> {activeOpponentName} (Squad Comrade)\n" +
                                        $"<i>Footlocker table cleared. Two ivory dice ready.</i>";
            }

            SelectWager(50);

            if (playerDiceText != null) playerDiceText.text = "[ - ] [ - ]";
            if (opponentDiceText != null) opponentDiceText.text = "[ - ] [ - ]";
            if (diceOutcomeText != null) diceOutcomeText.text = "Choose your wager and roll the dice against your squadmate.";
        }

        public void SelectWager(long amount)
        {
            selectedWager = amount;
            if (currentWagerText != null)
            {
                currentWagerText.text = $"<b>CURRENT WAGER:</b> <color=#FFD700>${selectedWager:N0}</color>";
            }
        }

        private void OnRollDiceClicked()
        {
            var soldier = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;

            if (soldier != null && soldier.cashBalance < selectedWager)
            {
                if (diceOutcomeText != null)
                {
                    diceOutcomeText.text = $"<color=#FF4444>You do not have enough cash (${selectedWager:N0}) to place this wager!</color>";
                }
                return;
            }

            var result = BarracksDiceEngine.PlayDiceRoll(soldier, selectedWager, activeOpponentName);

            if (playerDiceText != null)
            {
                playerDiceText.text = $"<b>YOUR ROLL:</b> [{result.playerDie1}] [{result.playerDie2}] = <color=#00FFAA><b>{result.playerSum}</b></color>";
            }

            if (opponentDiceText != null)
            {
                opponentDiceText.text = $"<b>{activeOpponentName.ToUpper()}'S ROLL:</b> [{result.opponentDie1}] [{result.opponentDie2}] = <color=#FF8800><b>{result.opponentSum}</b></color>";
            }

            if (diceOutcomeText != null)
            {
                string col = result.won ? "#00FF66" : (result.isTie ? "#FFD700" : "#FF4444");
                diceOutcomeText.text = $"<color={col}><b>[{result.headline}]</b></color>\n{result.narrative}\n" +
                                       $"<b>Net Profit / Loss:</b> {(result.netProfit >= 0 ? "+" : "")}${result.netProfit:N0}";
            }
        }

        // =====================================================================
        // EVENT BINDINGS & SAFETY
        // =====================================================================

        /// <summary>
        /// Robust programmatic UI builder that creates the full tactical minigames hierarchy
        /// (Checkpoint Desk, Firing Range, Barracks Dice) if not already wired in editor/scene.
        /// </summary>
        public void EnsureUIHierarchy()
        {
            if (modalRoot != null && checkpointViewRoot != null && firingRangeViewRoot != null && barracksDiceViewRoot != null)
                return;

            var rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
            SetFullScreen(rootRect);

            // 1. Overlay Backdrop
            if (modalRoot == null)
            {
                var overlayObj = CreateUIElement("TacticalModalOverlay", transform);
                SetFullScreen(overlayObj.GetComponent<RectTransform>());
                modalBackdrop = overlayObj.AddComponent<Image>();
                modalBackdrop.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);
                modalRoot = overlayObj;
            }

            // 2. Main Frame Card
            var cardObj = CreateUIElement("MainFrameCard", modalRoot.transform);
            var cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.03f, 0.02f);
            cardRect.anchorMax = new Vector2(0.97f, 0.98f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            var cardImg = cardObj.AddComponent<Image>();
            cardImg.color = new Color(0.10f, 0.12f, 0.16f, 1f);

            // 3. Top Header Bar
            var headerObj = CreateUIElement("TopHeaderBar", cardObj.transform);
            SetRect(headerObj.GetComponent<RectTransform>(), 0f, 0.93f, 1f, 1f);
            var headerBg = headerObj.AddComponent<Image>();
            headerBg.color = new Color(0.15f, 0.18f, 0.24f);

            var titleText = CreateText("HeaderTitle", headerObj.transform,
                "TACTICAL SIMULATIONS & DRILLS", 28, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0.95f, 0.82f, 0.35f));
            SetRect(titleText.GetComponent<RectTransform>(), 0.04f, 0f, 0.80f, 1f);

            closeModalButton = CreateButton("CloseBtn", headerObj.transform, out _, "X", new Color(0.65f, 0.20f, 0.20f));
            SetRect(closeModalButton.GetComponent<RectTransform>(), 0.85f, 0.12f, 0.97f, 0.88f);

            // 4. Tab Navigation Bar
            var tabRowObj = CreateUIElement("TabRow", cardObj.transform);
            SetRect(tabRowObj.GetComponent<RectTransform>(), 0.02f, 0.865f, 0.98f, 0.925f);
            var tabHLayout = tabRowObj.AddComponent<HorizontalLayoutGroup>();
            tabHLayout.spacing = 6f;
            tabHLayout.childForceExpandWidth = true;
            tabHLayout.childForceExpandHeight = true;

            tabCheckpointButton = CreateButton("TabCheckpoint", tabRowObj.transform, out _, "CHECKPOINT", new Color(0.20f, 0.25f, 0.35f));
            tabFiringRangeButton = CreateButton("TabFiringRange", tabRowObj.transform, out _, "FIRING RANGE", new Color(0.20f, 0.25f, 0.35f));
            tabBarracksDiceButton = CreateButton("TabBarracksDice", tabRowObj.transform, out _, "BARRACKS DICE", new Color(0.20f, 0.25f, 0.35f));

            // Content Area Container
            var contentArea = CreateUIElement("ContentArea", cardObj.transform);
            SetRect(contentArea.GetComponent<RectTransform>(), 0.02f, 0.02f, 0.98f, 0.855f);

            // ==========================================
            // VIEW A: CHECKPOINT DESK
            // ==========================================
            checkpointViewRoot = CreateUIElement("CheckpointView", contentArea.transform);
            SetFullScreen(checkpointViewRoot.GetComponent<RectTransform>());

            var cpCaseBox = CreateUIElement("CaseBox", checkpointViewRoot.transform);
            SetRect(cpCaseBox.GetComponent<RectTransform>(), 0f, 0.92f, 1f, 1f);
            checkpointCaseIdText = CreateText("CaseIdText", cpCaseBox.transform, "GATE POST CASE: CP-000", 22, TextAlignmentOptions.Left, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetFullScreen(checkpointCaseIdText.GetComponent<RectTransform>());

            var bulletinBox = CreateUIElement("BulletinBox", checkpointViewRoot.transform);
            SetRect(bulletinBox.GetComponent<RectTransform>(), 0f, 0.81f, 1f, 0.91f);
            var bulletinImg = bulletinBox.AddComponent<Image>();
            bulletinImg.color = new Color(0.20f, 0.16f, 0.06f, 0.9f);
            intelBulletinText = CreateText("BulletinText", bulletinBox.transform, "BOLO: Watch for smuggled contraband and expired transit permits.", 18, TextAlignmentOptions.Left, FontStyles.Italic, new Color(1f, 0.85f, 0.3f));
            SetRect(intelBulletinText.GetComponent<RectTransform>(), 0.02f, 0.05f, 0.98f, 0.95f);

            var passportBox = CreateUIElement("PassportBox", checkpointViewRoot.transform);
            SetRect(passportBox.GetComponent<RectTransform>(), 0f, 0.52f, 1f, 0.80f);
            var passImg = passportBox.AddComponent<Image>();
            passImg.color = new Color(0.12f, 0.14f, 0.18f);
            passportDetailsText = CreateText("PassportText", passportBox.transform, "TRANSIT PASSPORT DETAILS", 18, TextAlignmentOptions.TopLeft, FontStyles.Normal, Color.white);
            SetRect(passportDetailsText.GetComponent<RectTransform>(), 0.02f, 0.04f, 0.98f, 0.96f);

            var manifestBox = CreateUIElement("ManifestBox", checkpointViewRoot.transform);
            SetRect(manifestBox.GetComponent<RectTransform>(), 0f, 0.35f, 1f, 0.51f);
            var manImg = manifestBox.AddComponent<Image>();
            manImg.color = new Color(0.12f, 0.14f, 0.18f);
            manifestDetailsText = CreateText("ManifestText", manifestBox.transform, "CARGO MANIFEST DETAILS", 18, TextAlignmentOptions.TopLeft, FontStyles.Normal, Color.white);
            SetRect(manifestDetailsText.GetComponent<RectTransform>(), 0.02f, 0.04f, 0.98f, 0.96f);

            var vehicleBox = CreateUIElement("VehicleBox", checkpointViewRoot.transform);
            SetRect(vehicleBox.GetComponent<RectTransform>(), 0f, 0.22f, 1f, 0.34f);
            var vehImg = vehicleBox.AddComponent<Image>();
            vehImg.color = new Color(0.12f, 0.14f, 0.18f);
            vehicleInfoText = CreateText("VehicleText", vehicleBox.transform, "VEHICLE DETAILS", 18, TextAlignmentOptions.TopLeft, FontStyles.Normal, Color.white);
            SetRect(vehicleInfoText.GetComponent<RectTransform>(), 0.02f, 0.04f, 0.98f, 0.96f);

            checkpointVerdictText = CreateText("VerdictText", checkpointViewRoot.transform, "", 20, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 1f, 0.4f));
            SetRect(checkpointVerdictText.GetComponent<RectTransform>(), 0f, 0.14f, 1f, 0.21f);

            var cpButtonsBox = CreateUIElement("CheckpointButtonsRow", checkpointViewRoot.transform);
            SetRect(cpButtonsBox.GetComponent<RectTransform>(), 0f, 0.01f, 1f, 0.13f);
            var cpHLayout = cpButtonsBox.AddComponent<HorizontalLayoutGroup>();
            cpHLayout.spacing = 6f;
            cpHLayout.childForceExpandWidth = true;
            cpHLayout.childForceExpandHeight = true;

            approvePassButton = CreateButton("ApprovePassBtn", cpButtonsBox.transform, out _, "APPROVE PASS", new Color(0.18f, 0.55f, 0.28f));
            searchCargoButton = CreateButton("SearchCargoBtn", cpButtonsBox.transform, out _, "SEARCH CARGO", new Color(0.65f, 0.45f, 0.15f));
            detainDriverButton = CreateButton("DetainDriverBtn", cpButtonsBox.transform, out _, "DETAIN DRIVER", new Color(0.65f, 0.20f, 0.20f));
            nextVehicleButton = CreateButton("NextVehicleBtn", cpButtonsBox.transform, out _, "NEXT VEHICLE", new Color(0.25f, 0.40f, 0.65f));

            // ==========================================
            // VIEW B: FIRING RANGE
            // ==========================================
            firingRangeViewRoot = CreateUIElement("FiringRangeView", contentArea.transform);
            SetFullScreen(firingRangeViewRoot.GetComponent<RectTransform>());

            rangeScoreboardText = CreateText("ScoreboardText", firingRangeViewRoot.transform, "QUALIFICATION SCORE: 0 / 0", 24, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetRect(rangeScoreboardText.GetComponent<RectTransform>(), 0f, 0.90f, 1f, 0.98f);

            var targetDisplayBox = CreateUIElement("TargetDisplayBox", firingRangeViewRoot.transform);
            SetRect(targetDisplayBox.GetComponent<RectTransform>(), 0.05f, 0.46f, 0.95f, 0.88f);
            var tgtImg = targetDisplayBox.AddComponent<Image>();
            tgtImg.color = new Color(0.08f, 0.10f, 0.14f);

            targetInfoText = CreateText("TargetInfoText", targetDisplayBox.transform, "READY ON FIRING LINE\nPress Start to begin.", 26, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            SetRect(targetInfoText.GetComponent<RectTransform>(), 0.05f, 0.35f, 0.95f, 0.90f);

            timerCountdownText = CreateText("TimerCountdownText", targetDisplayBox.transform, "Window: --", 22, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.85f, 0.2f));
            SetRect(timerCountdownText.GetComponent<RectTransform>(), 0.05f, 0.08f, 0.95f, 0.30f);

            trialFeedbackText = CreateText("TrialFeedbackText", firingRangeViewRoot.transform, "", 22, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.3f, 1f, 0.5f));
            SetRect(trialFeedbackText.GetComponent<RectTransform>(), 0.05f, 0.33f, 0.95f, 0.44f);

            var fireButtonsBox = CreateUIElement("FireButtonsRow", firingRangeViewRoot.transform);
            SetRect(fireButtonsBox.GetComponent<RectTransform>(), 0.05f, 0.18f, 0.95f, 0.31f);
            var frHLayout = fireButtonsBox.AddComponent<HorizontalLayoutGroup>();
            frHLayout.spacing = 10f;
            frHLayout.childForceExpandWidth = true;
            frHLayout.childForceExpandHeight = true;

            fireShotButton = CreateButton("FireShotBtn", fireButtonsBox.transform, out _, "PULL TRIGGER (FIRE)", new Color(0.70f, 0.20f, 0.20f));
            holdFireButton = CreateButton("HoldFireBtn", fireButtonsBox.transform, out _, "HOLD FIRE", new Color(0.30f, 0.45f, 0.60f));

            startRangeButton = CreateButton("StartRangeBtn", firingRangeViewRoot.transform, out _, "COMMENCE FIRING QUALIFICATION", new Color(0.18f, 0.60f, 0.30f));
            SetRect(startRangeButton.GetComponent<RectTransform>(), 0.10f, 0.04f, 0.90f, 0.15f);

            // Certificate Panel Overlay
            certificatePanel = CreateUIElement("CertificatePanel", firingRangeViewRoot.transform);
            SetFullScreen(certificatePanel.GetComponent<RectTransform>());
            var certBg = certificatePanel.AddComponent<Image>();
            certBg.color = new Color(0.05f, 0.07f, 0.10f, 0.95f);

            certificateSummaryText = CreateText("CertSummaryText", certificatePanel.transform, "MARKSMANSHIP QUALIFICATION CERTIFICATE", 24, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0.95f, 0.85f, 0.35f));
            SetRect(certificateSummaryText.GetComponent<RectTransform>(), 0.08f, 0.35f, 0.92f, 0.80f);

            closeCertificateButton = CreateButton("CloseCertBtn", certificatePanel.transform, out _, "DISMISS CERTIFICATE", new Color(0.25f, 0.45f, 0.70f));
            SetRect(closeCertificateButton.GetComponent<RectTransform>(), 0.20f, 0.18f, 0.80f, 0.28f);
            certificatePanel.SetActive(false);

            // ==========================================
            // VIEW C: BARRACKS CRAPS
            // ==========================================
            barracksDiceViewRoot = CreateUIElement("BarracksDiceView", contentArea.transform);
            SetFullScreen(barracksDiceViewRoot.GetComponent<RectTransform>());

            diceOpponentText = CreateText("OpponentText", barracksDiceViewRoot.transform, "OPPONENT: Corporal Vance | BARRACKS CRAPS", 22, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            SetRect(diceOpponentText.GetComponent<RectTransform>(), 0f, 0.90f, 1f, 0.98f);

            currentWagerText = CreateText("CurrentWagerText", barracksDiceViewRoot.transform, "Current Bet: $50", 22, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 1f, 0.5f));
            SetRect(currentWagerText.GetComponent<RectTransform>(), 0f, 0.81f, 1f, 0.89f);

            var wagerBox = CreateUIElement("WagerButtonsRow", barracksDiceViewRoot.transform);
            SetRect(wagerBox.GetComponent<RectTransform>(), 0.05f, 0.70f, 0.95f, 0.79f);
            var wHLayout = wagerBox.AddComponent<HorizontalLayoutGroup>();
            wHLayout.spacing = 8f;
            wHLayout.childForceExpandWidth = true;
            wHLayout.childForceExpandHeight = true;

            wager20Button = CreateButton("Wager20Btn", wagerBox.transform, out _, "$20", new Color(0.20f, 0.25f, 0.35f));
            wager50Button = CreateButton("Wager50Btn", wagerBox.transform, out _, "$50", new Color(0.20f, 0.25f, 0.35f));
            wager100Button = CreateButton("Wager100Btn", wagerBox.transform, out _, "$100", new Color(0.20f, 0.25f, 0.35f));
            wager250Button = CreateButton("Wager250Btn", wagerBox.transform, out _, "$250", new Color(0.20f, 0.25f, 0.35f));

            var diceDisplayBox = CreateUIElement("DiceDisplayBox", barracksDiceViewRoot.transform);
            SetRect(diceDisplayBox.GetComponent<RectTransform>(), 0.05f, 0.34f, 0.95f, 0.67f);
            var diceBoxImg = diceDisplayBox.AddComponent<Image>();
            diceBoxImg.color = new Color(0.08f, 0.10f, 0.14f);

            playerDiceText = CreateText("PlayerDiceText", diceDisplayBox.transform, "YOUR ROLL: [ - ] [ - ]", 26, TextAlignmentOptions.Center, FontStyles.Bold, new Color(0f, 0.85f, 1f));
            SetRect(playerDiceText.GetComponent<RectTransform>(), 0.05f, 0.52f, 0.95f, 0.92f);

            opponentDiceText = CreateText("OpponentDiceText", diceDisplayBox.transform, "OPPONENT: [ - ] [ - ]", 26, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.65f, 0.35f));
            SetRect(opponentDiceText.GetComponent<RectTransform>(), 0.05f, 0.08f, 0.95f, 0.48f);

            diceOutcomeText = CreateText("DiceOutcomeText", barracksDiceViewRoot.transform, "Place your bet and roll the bones!", 22, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.85f, 0.3f));
            SetRect(diceOutcomeText.GetComponent<RectTransform>(), 0.05f, 0.20f, 0.95f, 0.32f);

            rollDiceButton = CreateButton("RollDiceBtn", barracksDiceViewRoot.transform, out _, "ROLL THE DICE (2d6)", new Color(0.18f, 0.65f, 0.35f));
            SetRect(rollDiceButton.GetComponent<RectTransform>(), 0.10f, 0.04f, 0.90f, 0.16f);

            checkpointViewRoot.SetActive(true);
            firingRangeViewRoot.SetActive(false);
            barracksDiceViewRoot.SetActive(false);
        }

        private void EnsureUIBindings()
        {
            if (isBindingsInitialized) return;
            isBindingsInitialized = true;

            if (closeModalButton != null) closeModalButton.onClick.AddListener(CloseModal);

            if (tabCheckpointButton != null) tabCheckpointButton.onClick.AddListener(() => SwitchTab(MinigameTab.Checkpoint));
            if (tabFiringRangeButton != null) tabFiringRangeButton.onClick.AddListener(() => SwitchTab(MinigameTab.FiringRange));
            if (tabBarracksDiceButton != null) tabBarracksDiceButton.onClick.AddListener(() => SwitchTab(MinigameTab.BarracksDice));

            // Checkpoint buttons
            if (approvePassButton != null) approvePassButton.onClick.AddListener(OnApprovePassClicked);
            if (searchCargoButton != null) searchCargoButton.onClick.AddListener(OnSearchCargoClicked);
            if (detainDriverButton != null) detainDriverButton.onClick.AddListener(OnDetainDriverClicked);
            if (nextVehicleButton != null) nextVehicleButton.onClick.AddListener(SetupNewCheckpointCase);

            // Firing range buttons
            if (fireShotButton != null) fireShotButton.onClick.AddListener(OnFireShotClicked);
            if (holdFireButton != null) holdFireButton.onClick.AddListener(OnHoldFireClicked);
            if (startRangeButton != null) startRangeButton.onClick.AddListener(SetupFiringRange);
            if (closeCertificateButton != null) closeCertificateButton.onClick.AddListener(() =>
            {
                if (certificatePanel != null) certificatePanel.SetActive(false);
            });

            // Dice buttons
            if (wager20Button != null) wager20Button.onClick.AddListener(() => SelectWager(20));
            if (wager50Button != null) wager50Button.onClick.AddListener(() => SelectWager(50));
            if (wager100Button != null) wager100Button.onClick.AddListener(() => SelectWager(100));
            if (wager250Button != null) wager250Button.onClick.AddListener(() => SelectWager(250));
            if (rollDiceButton != null) rollDiceButton.onClick.AddListener(OnRollDiceClicked);
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize,
            TextAlignmentOptions alignment, FontStyles fontStyle, Color color)
        {
            var go = CreateUIElement(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, out TextMeshProUGUI label, string text, Color bgColor)
        {
            var go = CreateUIElement(name, parent);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();

            label = CreateText("Label", go.transform, text, 18, TextAlignmentOptions.Center, FontStyles.Bold, Color.white);
            SetFullScreen(label.GetComponent<RectTransform>());

            return btn;
        }

        private void BindEventListeners()
        {
            ThemeManager.OnThemeChanged += OnThemeChanged;
            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            ApplyThemePalette(p);
        }

        private void UnbindEventListeners()
        {
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (modalBackdrop != null)
            {
                modalBackdrop.color = p.mode == ThemeMode.Light 
                    ? new Color(0.12f, 0.12f, 0.10f, 0.75f) 
                    : new Color(0f, 0f, 0f, 0.90f);
            }

            if (checkpointViewRoot != null)
            {
                var img = checkpointViewRoot.GetComponent<Image>();
                if (img != null) img.color = p.cardBackground;
            }
            if (firingRangeViewRoot != null)
            {
                var img = firingRangeViewRoot.GetComponent<Image>();
                if (img != null) img.color = p.cardBackground;
            }
            if (barracksDiceViewRoot != null)
            {
                var img = barracksDiceViewRoot.GetComponent<Image>();
                if (img != null) img.color = p.cardBackground;
            }

            StyleTabButton(tabCheckpointButton, currentTab == MinigameTab.Checkpoint, p);
            StyleTabButton(tabFiringRangeButton, currentTab == MinigameTab.FiringRange, p);
            StyleTabButton(tabBarracksDiceButton, currentTab == MinigameTab.BarracksDice, p);

            if (closeModalButton != null)
            {
                var img = closeModalButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = closeModalButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }
        }

        private void StyleTabButton(Button btn, bool isActive, ThemePalette p)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = isActive ? p.btnAgeGrowBg : p.btnNormalBg;
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.color = isActive ? p.btnAgeGrowText : p.btnNormalText;
        }
    }
}
