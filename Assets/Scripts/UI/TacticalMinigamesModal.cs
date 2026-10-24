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
                string sealColor = p.officialSealValid ? "#00FF66" : "#FF4444";
                string expiryColor = p.isExpired ? "#FF4444" : "#FFFFFF";

                passportDetailsText.text =
                    $"<b>--- TRANSIT PASSPORT ---</b>\n" +
                    $"<b>Name:</b> {p.civilianName}\n" +
                    $"<b>ID:</b> {p.photoId}\n" +
                    $"<b>Issued:</b> {p.issuedDistrict}\n" +
                    $"<b>Validity:</b> <color={expiryColor}>{p.expirationDate}</color>\n" +
                    $"<b>Imperial Seal:</b> <color={sealColor}>{(p.officialSealValid ? "AUTHENTIC IMPERIAL EAGLE" : "SUSPICIOUS / FORGED SMUDGE")}</color>\n" +
                    $"<b>Purpose:</b> {p.travelPurpose}";
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

        private void EnsureUIBindings()
        {
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

        private void BindEventListeners() { }
        private void UnbindEventListeners() { }
    }
}
