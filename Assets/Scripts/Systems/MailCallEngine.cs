using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Milestone: Personal Life, Mail Call & Homefront Dynasty Engine (The "BitLife & Band of Brothers" Pillar).
    /// Manages semiannual mail call delivery, family remittances, sweetheart courtship & heartbreak ("Dear John"),
    /// family farm economic survival, officer social patronage, and capital furlough leave.
    /// </summary>
    public static class MailCallEngine
    {
        // Active in-memory registry of homefront dynasty states by Soldier LifeId
        private static readonly Dictionary<string, HomefrontDynastyState> s_dynastyStates = new Dictionary<string, HomefrontDynastyState>();

        // Current pending unread mail queue for the active semester
        public static List<MailLetter> CurrentMailQueue { get; private set; } = new List<MailLetter>();

        // Global Event Hooks
        public static Action<List<MailLetter>> OnMailCallDispatched;
        public static Action<MailLetter, MailResponseOption, MailResolutionResult> OnLetterResolved;
        public static Action<FurloughResult> OnFurloughCompleted;

        /// <summary>
        /// Explicitly triggers a mail call inquiry from the soldier's activity menu.
        /// </summary>
        public static List<MailLetter> RollMailCallEvent(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.isYouthPhase) return new List<MailLetter>();

            int currentYear = TimeCycleManager.Instance != null ? TimeCycleManager.Instance.currentYear : 2026;
            var letters = DispatchSemesterMailCall(soldier, currentYear, 1, soldier.isInCombatDeployment);
            var modal = ArmyLife.UI.MailCallModal.Instance ?? UnityEngine.Object.FindAnyObjectByType<ArmyLife.UI.MailCallModal>(FindObjectsInactive.Include);

            if (letters != null && letters.Count > 0 && modal != null)
            {
                modal.OpenMailQueue(letters);
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("MAIL CALL",
                    $"<color=#00D4FF>[REGIMENTAL FIELD POST]</color> You visited the mail clerk desk. No letters arrived today from the homefront. You sent a short field postal card home.");
            }

            return letters;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSubscribeToEvents()
        {
            GameEvents.OnSemesterAdvanced -= HandleSemesterAdvanced;
            GameEvents.OnSemesterAdvanced += HandleSemesterAdvanced;
        }

        private static void HandleSemesterAdvanced(int year, int semester, bool isDeployment)
        {
            SoldierProfile soldier = (GameManager.Instance != null) ? GameManager.Instance.currentSoldier : null;
            if (soldier == null || soldier.isDead || soldier.isYouthPhase || soldier.isRetired) return;

            // Dispatch semiannual mail call
            var letters = DispatchSemesterMailCall(soldier, year, semester, isDeployment);

            // If UI modal is available, trigger first letter in queue
            if (letters != null && letters.Count > 0)
            {
                var modal = ArmyLife.UI.MailCallModal.Instance ?? UnityEngine.Object.FindAnyObjectByType<ArmyLife.UI.MailCallModal>(FindObjectsInactive.Include);
                if (modal != null)
                {
                    modal.OpenMailQueue(letters);
                }
            }
        }

        /// <summary>
        /// Clears transient state and queues for deterministic unit tests.
        /// </summary>
        public static void ResetEngine()
        {
            s_dynastyStates.Clear();
            CurrentMailQueue.Clear();
        }

        /// <summary>
        /// Retrieves or initializes the persistent HomefrontDynastyState for a soldier.
        /// </summary>
        public static HomefrontDynastyState GetHomefrontState(SoldierProfile soldier)
        {
            if (soldier == null) return new HomefrontDynastyState();

            string key = !string.IsNullOrEmpty(soldier.lifeId) ? soldier.lifeId : "active_soldier";
            if (!s_dynastyStates.TryGetValue(key, out HomefrontDynastyState state))
            {
                state = new HomefrontDynastyState();
                state.SyncFromSoldier(soldier);
                s_dynastyStates[key] = state;
            }
            else
            {
                state.SyncFromSoldier(soldier);
            }

            return state;
        }

        /// <summary>
        /// Stores or replaces the HomefrontDynastyState for a soldier.
        /// </summary>
        public static void SetHomefrontState(SoldierProfile soldier, HomefrontDynastyState state)
        {
            if (soldier == null || state == null) return;
            string key = !string.IsNullOrEmpty(soldier.lifeId) ? soldier.lifeId : "active_soldier";
            s_dynastyStates[key] = state;
            state.SyncToSoldier(soldier);
        }

        // =========================================================================
        // 1. SEMIANNUAL MAIL CALL DISPATCHER (1-2 LETTERS PER SEMESTER)
        // =========================================================================

        /// <summary>
        /// Dispatches 1 to 2 letters per semester during garrison or frontline downtime.
        /// Reflects family farm conditions, courtship status, officer standing, and prior remittances.
        /// </summary>
        public static List<MailLetter> DispatchSemesterMailCall(SoldierProfile soldier, int year, int semester, bool isInCombat)
        {
            if (soldier == null || soldier.isDead || soldier.isYouthPhase) return new List<MailLetter>();

            HomefrontDynastyState state = GetHomefrontState(soldier);
            var generatedLetters = new List<MailLetter>();

            // 1. Check for Pending Gratitude Letter from previous semester's remittance
            if (state.pendingGratitudeLetterNextTurn)
            {
                long amt = state.lastRemittanceAmount > 0 ? state.lastRemittanceAmount : 400L;
                string sender = !string.IsNullOrEmpty(state.pendingGratitudeSender) ? state.pendingGratitudeSender : "Mother";
                var gratitudeLetter = MailDeckCatalog.CreateGratitudeLetter(sender, amt, year, semester);
                generatedLetters.Add(gratitudeLetter);

                // Reset pending flag
                state.pendingGratitudeLetterNextTurn = false;
            }

            // 2. Sweetheart or Dear John Letter (If courtship is active and not already heartbroken)
            bool rollSweetheart = (state.courtshipStage != CourtshipStage.Single && state.courtshipStage != CourtshipStage.Heartbroken);
            if (rollSweetheart && generatedLetters.Count < 2)
            {
                // Dear John trigger condition:
                // If soldier has been deployed in combat for 2+ consecutive semesters, or low affection, or high-risk roll
                bool triggerDearJohn = !state.hasReceivedDearJohn && 
                                       ((soldier.consecutiveCombatSemesters >= 2 && state.courtshipAffection < 50f) || 
                                        (soldier.isInCombatDeployment && UnityEngine.Random.value < 0.15f));

                if (triggerDearJohn)
                {
                    var dearJohn = MailDeckCatalog.CreateDearJohnLetter(state.sweetheartName, year, semester);
                    generatedLetters.Add(dearJohn);
                    state.hasReceivedDearJohn = true;
                }
                else
                {
                    // Roll variety of sweetheart correspondence
                    int sweetRoll = UnityEngine.Random.Range(1, 4);
                    MailLetter sweetLetter;
                    switch (sweetRoll)
                    {
                        case 1:
                            sweetLetter = MailDeckCatalog.CreateSweetheartEncouragement(state.sweetheartName, year, semester);
                            break;
                        case 2:
                            sweetLetter = MailDeckCatalog.CreateSweetheartMarriageHints(state.sweetheartName, year, semester);
                            break;
                        default:
                            sweetLetter = MailDeckCatalog.CreateSweetheartAnxiety(state.sweetheartName, year, semester);
                            break;
                    }
                    generatedLetters.Add(sweetLetter);
                }
            }

            // 3. Family Farm / Hometown Crisis Letter
            if (generatedLetters.Count < 2)
            {
                // Choose family scenario based on prosperity and random roll
                int familyRoll = UnityEngine.Random.Range(1, 5);
                MailLetter familyLetter;

                switch (familyRoll)
                {
                    case 1:
                        familyLetter = MailDeckCatalog.CreateHarvestStrugglesLetter(year, semester);
                        break;
                    case 2:
                        // Foreclosure if struggling or destitute
                        if (state.prosperityTier <= HomefrontProsperityTier.Struggling || UnityEngine.Random.value < 0.4f)
                        {
                            familyLetter = MailDeckCatalog.CreateMortgageForeclosureLetter(year, semester);
                        }
                        else
                        {
                            familyLetter = MailDeckCatalog.CreateHarvestStrugglesLetter(year, semester);
                        }
                        break;
                    case 3:
                        familyLetter = MailDeckCatalog.CreateSiblingDraftLetter(year, semester);
                        break;
                    default:
                        familyLetter = MailDeckCatalog.CreateFamilySicknessLetter(year, semester);
                        break;
                }

                generatedLetters.Add(familyLetter);
            }

            // 4. Officer / High Command Social Invitations (For Officers O-1+ or High Merit soldiers)
            if (generatedLetters.Count < 2 && (soldier.isOfficer || soldier.meritRating >= 70f))
            {
                if (UnityEngine.Random.value < 0.50f)
                {
                    var invite = (UnityEngine.Random.value > 0.5f) 
                        ? MailDeckCatalog.CreateCapitalGalaInvitation(year, semester) 
                        : MailDeckCatalog.CreateIndustrialistPatronageLetter(year, semester);
                    generatedLetters.Add(invite);
                }
            }

            // Track stats
            state.lettersReceivedCount += generatedLetters.Count;
            CurrentMailQueue = new List<MailLetter>(generatedLetters);

            // Feed Log announcement
            string location = isInCombat ? "Frontline Trenches" : "Base Garrison";
            GameEvents.OnFeedEventLogged?.Invoke("MAIL CALL",
                $"<color=#FFD700><b>[MAIL CALL • {location.ToUpper()}]</b></color> " +
                $"The field post courier delivered <b>{generatedLetters.Count}</b> letter{(generatedLetters.Count > 1 ? "s" : "")} from the homefront!");

            OnMailCallDispatched?.Invoke(generatedLetters);
            return generatedLetters;
        }

        // =========================================================================
        // 2. LETTER DECISION RESOLUTION
        // =========================================================================

        /// <summary>
        /// Resolves a player's selected response option on a received letter.
        /// </summary>
        public static MailResolutionResult ResolveLetterOption(SoldierProfile soldier, MailLetter letter, int optionIndex)
        {
            if (soldier == null || letter == null)
            {
                return new MailResolutionResult { success = false, message = "Invalid soldier or letter instance." };
            }

            if (letter.responseOptions == null || optionIndex < 0 || optionIndex >= letter.responseOptions.Count)
            {
                return new MailResolutionResult { success = false, message = "Invalid response option index selected." };
            }

            HomefrontDynastyState state = GetHomefrontState(soldier);
            MailResponseOption option = letter.responseOptions[optionIndex];
            var result = new MailResolutionResult();

            // Check if this is a "Dear John" resolution
            if (letter.isDearJohn || letter.category == LetterCategory.DearJohn)
            {
                DearJohnChoice djChoice = (DearJohnChoice)Mathf.Clamp(optionIndex, 0, 2);
                return ResolveDearJohn(soldier, letter, djChoice);
            }

            // Check if this option requires cash / remittance
            if (option.remittanceCost > 0)
            {
                if (soldier.cashBalance < option.remittanceCost)
                {
                    // Fallback to partial or insufficient funds
                    result.success = false;
                    result.message = $"Insufficient funds! You need ${option.remittanceCost:N0} but only possess ${soldier.cashBalance:N0}.";
                    return result;
                }

                soldier.cashBalance -= option.remittanceCost;
                result.cashSpent = option.remittanceCost;
                result.wasRemittanceSent = true;

                // Record remittance in homefront state and soldier profile
                state.RecordRemittance(option.remittanceCost, letter.senderName);
            }

            // Check if letter was ignored / discarded
            bool isIgnoreOption = option.optionId.EndsWith("_ignore") || 
                                 option.optionId.EndsWith("_burn") || 
                                 option.optionId.EndsWith("_default");
            if (isIgnoreOption)
            {
                state.lettersIgnoredCount++;
                result.wasIgnored = true;
            }

            // Apply Morale (happiness)
            float effectiveMoraleChange = option.moraleModifier;
            if (isIgnoreOption && effectiveMoraleChange >= 0f)
            {
                effectiveMoraleChange = -letter.moraleImpactIfIgnored;
            }
            soldier.happiness = Mathf.Clamp(soldier.happiness + effectiveMoraleChange, 0f, 100f);
            result.moraleChange = effectiveMoraleChange;

            // Apply Discipline
            if (option.disciplineModifier != 0f)
            {
                soldier.discipline = Mathf.Clamp(soldier.discipline + option.disciplineModifier, 0f, 100f);
                result.disciplineChange = option.disciplineModifier;
            }

            // Apply Family Prosperity
            if (option.familyProsperityModifier != 0f)
            {
                state.AddProsperity(option.familyProsperityModifier);
                result.prosperityChange = option.familyProsperityModifier;
            }

            // Apply Courtship Affection
            if (option.courtshipAffectionModifier != 0f)
            {
                state.courtshipAffection = Mathf.Clamp(state.courtshipAffection + option.courtshipAffectionModifier, 0f, 100f);
            }

            // Apply Squad Respect & CO Favor
            if (option.squadRespectModifier != 0f)
            {
                soldier.squadRespect = Mathf.Clamp(soldier.squadRespect + option.squadRespectModifier, 0f, 100f);
            }
            if (option.coFavorModifier != 0f)
            {
                soldier.coFavor = Mathf.Clamp(soldier.coFavor + option.coFavorModifier, 0f, 100f);
            }

            // Apply Acquired Trait
            if (!string.IsNullOrEmpty(option.acquiredTrait))
            {
                soldier.AddTrait(option.acquiredTrait);
                result.acquiredTrait = option.acquiredTrait;
            }

            // Sync state back to soldier
            state.SyncToSoldier(soldier);
            letter.isRead = true;

            // Remove letter from active queue
            if (CurrentMailQueue != null && CurrentMailQueue.Contains(letter))
            {
                CurrentMailQueue.Remove(letter);
            }

            // Format outcome narrative
            string feedMsg = !string.IsNullOrEmpty(option.responseFeedMessage) 
                ? option.responseFeedMessage 
                : $"You chose: '{option.buttonLabel}' in response to {letter.senderName}'s letter.";
            result.message = feedMsg;

            GameEvents.OnFeedEventLogged?.Invoke("MAIL RESOLUTION", feedMsg);
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            OnLetterResolved?.Invoke(letter, option, result);

            return result;
        }

        /// <summary>
        /// Explicit resolution handler for the high-impact "Dear John" breakup letter.
        /// Choices:
        /// 1. "Focus on the Mission" -> Discipline +10, Morale -15
        /// 2. "Write Desperate Plea" -> Morale -25
        /// 3. "Tear It Up & Move Forward" -> Morale +5, acquire "Iron Heart" trait
        /// </summary>
        public static MailResolutionResult ResolveDearJohn(SoldierProfile soldier, MailLetter letter, DearJohnChoice choice)
        {
            if (soldier == null)
            {
                return new MailResolutionResult { success = false, message = "Invalid soldier." };
            }

            HomefrontDynastyState state = GetHomefrontState(soldier);
            var result = new MailResolutionResult
            {
                wasDearJohn = true,
                success = true
            };

            switch (choice)
            {
                case DearJohnChoice.FocusOnMission:
                    // Discipline +10, Morale -15
                    soldier.discipline = Mathf.Clamp(soldier.discipline + 10f, 0f, 100f);
                    soldier.happiness = Mathf.Clamp(soldier.happiness - 15f, 0f, 100f);
                    soldier.AddTrait("Cold Professional");
                    state.courtshipStage = CourtshipStage.Single;
                    state.courtshipAffection = 0f;

                    result.disciplineChange = 10f;
                    result.moraleChange = -15f;
                    result.acquiredTrait = "Cold Professional";
                    result.message = "You folded the breakup letter into your breast pocket and gripped your service weapon. Love is dead; only duty remains (+10 Discipline, -15 Morale, Trait: 'Cold Professional').";
                    break;

                case DearJohnChoice.WriteDesperatePlea:
                    // Morale -25
                    soldier.happiness = Mathf.Clamp(soldier.happiness - 25f, 0f, 100f);
                    soldier.AddTrait("Heartbroken");
                    state.courtshipStage = CourtshipStage.Heartbroken;
                    state.courtshipAffection = 10f;

                    result.moraleChange = -25f;
                    result.acquiredTrait = "Heartbroken";
                    result.message = "You spent nights in desperate despair drafting pleas in the dark. The silence of the postal bags shatters your spirit (-25 Morale, Trait: 'Heartbroken').";
                    break;

                case DearJohnChoice.TearItUpMoveForward:
                    // Morale +5, acquire "Iron Heart" trait
                    soldier.happiness = Mathf.Clamp(soldier.happiness + 5f, 0f, 100f);
                    soldier.discipline = Mathf.Clamp(soldier.discipline + 5f, 0f, 100f);
                    soldier.AddTrait("Iron Heart");
                    state.courtshipStage = CourtshipStage.Single;
                    state.courtshipAffection = 0f;

                    result.moraleChange = 5f;
                    result.disciplineChange = 5f;
                    result.acquiredTrait = "Iron Heart";
                    result.message = "You shredded the Dear John letter into confetti and watched the mud claim it. You refuse to let anyone break you (+5 Morale, +5 Discipline, Trait: 'Iron Heart').";
                    break;
            }

            state.hasReceivedDearJohn = true;
            state.SyncToSoldier(soldier);
            if (letter != null) letter.isRead = true;

            if (CurrentMailQueue != null && CurrentMailQueue.Contains(letter))
            {
                CurrentMailQueue.Remove(letter);
            }

            GameEvents.OnFeedEventLogged?.Invoke("HEARTBREAK",
                $"<color=#FF4444><b>[DEAR JOHN RESOLUTION]</b></color> {result.message}");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);

            return result;
        }

        // =========================================================================
        // 3. FURLOUGH R&R ENGINE (CAPITAL LEAVE ACTIVITIES)
        // =========================================================================

        /// <summary>
        /// Checks whether the soldier is eligible to take capital furlough leave.
        /// </summary>
        public static bool CanTakeFurlough(SoldierProfile soldier, out string reason)
        {
            if (soldier == null)
            {
                reason = "No active soldier record.";
                return false;
            }
            if (soldier.isDead)
            {
                reason = "Soldier is deceased.";
                return false;
            }
            if (soldier.isYouthPhase)
            {
                reason = "Youth cadets cannot take military capital leave.";
                return false;
            }
            if (soldier.isInCombatDeployment)
            {
                reason = "Cannot take capital leave while engaged in frontline combat deployment!";
                return false;
            }

            reason = "Eligible for capital furlough leave.";
            return true;
        }

        /// <summary>
        /// Executes a Rest & Recuperation (R&R) activity during capital furlough leave.
        /// Supports: Canteen visits, courting partner, getting married, investing in civilian land/shops.
        /// </summary>
        public static FurloughResult TakeFurlough(SoldierProfile soldier, FurloughActivityType activity)
        {
            if (!CanTakeFurlough(soldier, out string eligibilityReason))
            {
                return new FurloughResult { success = false, message = eligibilityReason, activityType = activity };
            }

            HomefrontDynastyState state = GetHomefrontState(soldier);
            var result = new FurloughResult { activityType = activity, success = true };

            switch (activity)
            {
                case FurloughActivityType.CanteenVisit:
                    // Costs $100. Relaxes in capital canteen. Morale +15, Squad Respect +5.
                    long canteenCost = 100L;
                    if (soldier.cashBalance < canteenCost)
                    {
                        return new FurloughResult { success = false, message = $"Insufficient funds for canteen leave! (Requires ${canteenCost})." };
                    }
                    soldier.cashBalance -= canteenCost;
                    soldier.happiness = Mathf.Clamp(soldier.happiness + 15f, 0f, 100f);
                    soldier.squadRespect = Mathf.Clamp(soldier.squadRespect + 5f, 0f, 100f);

                    result.cashSpent = canteenCost;
                    result.moraleChange = 15f;
                    result.title = "Capital Canteen & Officers' Club";
                    result.message = "You spent an evening enjoying live jazz, cold lager, and laughs with comrades in the capital (+15 Morale, +5 Squad Respect).";
                    break;

                case FurloughActivityType.CourtSweetheart:
                    // Costs $250. Romantic dinner & promenade. Affection +25. If affection >= 80 -> Engaged!
                    long courtCost = 250L;
                    if (soldier.cashBalance < courtCost)
                    {
                        return new FurloughResult { success = false, message = $"Insufficient funds to wine and dine your sweetheart! (Requires ${courtCost})." };
                    }
                    soldier.cashBalance -= courtCost;
                    state.courtshipAffection = Mathf.Clamp(state.courtshipAffection + 25f, 0f, 100f);
                    soldier.happiness = Mathf.Clamp(soldier.happiness + 20f, 0f, 100f);

                    result.cashSpent = courtCost;
                    result.moraleChange = 20f;
                    result.title = $"Courting {state.sweetheartName}";

                    if (state.courtshipAffection >= 80f && state.courtshipStage < CourtshipStage.Engaged)
                    {
                        state.courtshipStage = CourtshipStage.Engaged;
                        soldier.AddTrait("Engaged");
                        result.acquiredTrait = "Engaged";
                        result.message = $"You treated {state.sweetheartName} to a candlelight supper and presented a gold promise ring. She said YES! You are now officially ENGAGED (+20 Morale, Trait: 'Engaged')!";
                    }
                    else
                    {
                        result.message = $"You strolled hand-in-hand through the imperial botanical gardens with {state.sweetheartName} (+20 Morale, +25 Affection).";
                    }
                    break;

                case FurloughActivityType.GetMarried:
                    // Wedding Feast ($1,000). Requires courtship / engagement.
                    if (soldier.isMarried || state.isMarried)
                    {
                        return new FurloughResult { success = false, message = "You are already married!" };
                    }
                    if (state.courtshipAffection < 50f && state.courtshipStage != CourtshipStage.Engaged)
                    {
                        return new FurloughResult { success = false, message = $"Your sweetheart affection ({state.courtshipAffection:F0}%) is too low. Court her more before proposing!" };
                    }

                    long weddingCost = 1000L;
                    if (soldier.cashBalance < weddingCost)
                    {
                        return new FurloughResult { success = false, message = $"Insufficient funds for a proper wedding banquet! (Requires ${weddingCost})." };
                    }

                    soldier.cashBalance -= weddingCost;
                    soldier.isMarried = true;
                    soldier.spouseName = state.sweetheartName;
                    state.isMarried = true;
                    state.spouseName = state.sweetheartName;
                    state.courtshipStage = CourtshipStage.Married;
                    state.courtshipAffection = 100f;

                    soldier.happiness = Mathf.Clamp(soldier.happiness + 30f, 0f, 100f);
                    soldier.AddTrait("Devoted Husband");

                    result.cashSpent = weddingCost;
                    result.moraleChange = 30f;
                    result.acquiredTrait = "Devoted Husband";
                    result.title = $"Wedding to {state.sweetheartName}";
                    result.message = $"The cathedral bells rang as you married {state.sweetheartName} under an arch of crossed military sabers! (+30 Morale, Trait: 'Devoted Husband').";
                    break;

                case FurloughActivityType.InvestCivilianShops:
                    // Costs $1,500. Buys shares in trade shops / bakery. Generates family prosperity +20.
                    long shopCost = 1500L;
                    if (soldier.cashBalance < shopCost)
                    {
                        return new FurloughResult { success = false, message = $"Insufficient capital to invest in civilian shops! (Requires ${shopCost})." };
                    }
                    soldier.cashBalance -= shopCost;
                    state.civilianShopInvestments += shopCost;
                    state.AddProsperity(20f);
                    soldier.happiness = Mathf.Clamp(soldier.happiness + 15f, 0f, 100f);
                    soldier.AddTrait("Civilian Investor");

                    result.cashSpent = shopCost;
                    result.moraleChange = 15f;
                    result.acquiredTrait = "Civilian Investor";
                    result.title = "Civilian Commercial Investment";
                    result.message = $"You acquired commercial shares in a thriving capital mercantile shop. Generates dynastic security (+15 Morale, +20 Family Prosperity, Trait: 'Civilian Investor').";
                    break;

                case FurloughActivityType.InvestCivilianLand:
                    // Costs $2,000. Purchases 10 fertile acres for family farm. Prosperity +25.
                    long landCost = 2000L;
                    if (soldier.cashBalance < landCost)
                    {
                        return new FurloughResult { success = false, message = $"Insufficient capital to buy civilian farmland! (Requires ${landCost})." };
                    }
                    soldier.cashBalance -= landCost;
                    state.civilianLandParcels += 1;
                    state.AddProsperity(25f);
                    soldier.happiness = Mathf.Clamp(soldier.happiness + 20f, 0f, 100f);
                    soldier.AddTrait("Landowner");

                    result.cashSpent = landCost;
                    result.moraleChange = 20f;
                    result.acquiredTrait = "Landowner";
                    result.title = "Fertile Land Acquisition";
                    result.message = $"You bought 10 fertile agricultural acres for your ancestral homestead. Protects your family from winter crop failures (+20 Morale, +25 Family Prosperity, Trait: 'Landowner').";
                    break;

                case FurloughActivityType.FamilyFarmVisit:
                    // Costs $150 travel. Visits parents at rural homestead. Morale +15, Prosperity +5.
                    long travelCost = 150L;
                    if (soldier.cashBalance < travelCost)
                    {
                        return new FurloughResult { success = false, message = $"Insufficient funds for stagecoach ticket home! (Requires ${travelCost})." };
                    }
                    soldier.cashBalance -= travelCost;
                    soldier.happiness = Mathf.Clamp(soldier.happiness + 15f, 0f, 100f);
                    state.AddProsperity(5f);

                    result.cashSpent = travelCost;
                    result.moraleChange = 15f;
                    result.title = "Homestead Family Reunion";
                    result.message = "You spent your leave helping father chop firewood and sharing warm home-cooked stew by mother's hearth (+15 Morale, +5 Prosperity).";
                    break;
            }

            state.furloughsTakenCount++;
            state.SyncToSoldier(soldier);

            GameEvents.OnFeedEventLogged?.Invoke("FURLOUGH",
                $"<color=#00FFAA><b>[FURLOUGH LEAVE • {result.title.ToUpper()}]</b></color> {result.message}");
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            OnFurloughCompleted?.Invoke(result);

            return result;
        }
    }
}
