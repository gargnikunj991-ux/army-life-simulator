using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Represents a high-stakes financial, black-market, or military graft dilemma scenario.
    /// </summary>
    public class EconomicDilemma
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string ChoiceALabel { get; set; }
        public string ChoiceBLabel { get; set; }
        public string ChoiceCLabel { get; set; }

        public Func<SoldierProfile, bool> EligibilityPredicate { get; set; }

        public Action<SoldierProfile, Action> ActionA { get; set; }
        public Action<SoldierProfile, Action> ActionB { get; set; }
        public Action<SoldierProfile, Action> ActionC { get; set; }

        public bool HasChoiceC => !string.IsNullOrEmpty(ChoiceCLabel);

        public bool IsEligible(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead || soldier.isYouthPhase) return false;
            if (EligibilityPredicate != null)
            {
                return EligibilityPredicate(soldier);
            }
            return true;
        }
    }

    /// <summary>
    /// High-Stakes Financial & Black-Market Dilemmas Deck for Army Life Simulator.
    /// Features 15 gripping, branching dilemmas covering military contraband, barracks vice,
    /// predatory syndicates, supply graft, insider trading, and wartime plundering.
    /// </summary>
    public static class EconomicDilemmaDeck
    {
        /// <summary>
        /// Event fired whenever an economic dilemma is triggered.
        /// Mirrors GameEvents.OnChildhoodDilemmaTriggered for decoupled listeners.
        /// </summary>
        public static Action<string, string, string, string, string, string, Action, Action, Action> OnDilemmaTriggered;

        /// <summary>
        /// Optional hook notified when any dilemma choice is resolved.
        /// </summary>
        public static Action<EconomicDilemma, int, SoldierProfile> OnDilemmaResolved;

        /// <summary>
        /// When true, TryTriggerEconomicDilemma will automatically resolve choice 0 in headless/batchmode environments
        /// if no UI modal or external listener is subscribed.
        /// </summary>
        public static bool AutoResolveInHeadlessMode = false;

        private static readonly List<EconomicDilemma> dilemmas = new List<EconomicDilemma>();

        public static IReadOnlyList<EconomicDilemma> AllDilemmas => dilemmas;

        static EconomicDilemmaDeck()
        {
            InitializeDeck();
        }

        // =========================================================================
        // DECK INITIALIZATION (15 HIGH-STAKES SCENARIOS)
        // =========================================================================

        private static void InitializeDeck()
        {
            dilemmas.Clear();

            // 1. Black-Market Syrettes Offer
            dilemmas.Add(new EconomicDilemma
            {
                Id = "black_market_syrettes",
                Category = "BLACK MARKET",
                Title = "Black-Market Syrettes Offer",
                Description = "A rogue combat medic slips into your barracks bunk after lights out. He opens a stained canvas field kit packed with stolen military morphine syrettes and concentrated neural stimulants, offering you a distributor's cut to flip them to underworld street peddlers.",
                ChoiceALabel = "Buy and flip the syrettes through street fences (+$3,500 Cash, +20 Heat, -15 Discipline)",
                ChoiceBLabel = "Report the medic to the Provost Marshal (+15 CO Favor, +12 Merit, -15 Squad Respect)",
                ChoiceCLabel = "Confiscate a trauma syrette for squad emergency use (+1 Medkit, +5 Morale, -5 Discipline)",
                EligibilityPredicate = s => !s.isImprisoned && s.militaryBrigSemestersRemaining == 0,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 3500L;
                    s.totalCrimeEarnings += 3500L;
                    s.crimesCommittedCount++;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 20f);
                    s.discipline = Mathf.Max(0f, s.discipline - 15f);
                    s.AddTrait("Black Market Fence");
                    GameEvents.OnFeedEventLogged?.Invoke("BLACK MARKET",
                        "<color=#00FFAA><b>[ILLICIT NARCOTICS SOLD]</b></color> You brokered the stolen morphine syrettes through dockside syndicates! " +
                        "Pocketed <color=#FFD700><b>+$3,500 Dirty Cash</b></color>, but Military Police heat climbed (+20% Heat, -15 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.coFavor = Mathf.Min(100f, s.coFavor + 15f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 12f);
                    s.discipline = Mathf.Min(100f, s.discipline + 10f);
                    s.squadRespect = Mathf.Max(5f, s.squadRespect - 15f);
                    GameEvents.OnFeedEventLogged?.Invoke("MILITARY JUSTICE",
                        "<color=#00D4FF><b>[PROVOST INFORMANT]</b></color> You turned over the rogue medic to the battalion stockade. " +
                        "Command commended your vigilance (+15 CO Favor, +12 Merit), but the enlisted squad brands you an untrustworthy snitch (-15 Squad Respect).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.fieldMedkitsCount++;
                    s.happiness = Mathf.Min(100f, s.happiness + 5f);
                    s.discipline = Mathf.Max(0f, s.discipline - 5f);
                    s.coFavor = Mathf.Max(0f, s.coFavor - 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("STASHED GEAR",
                        "<color=#00FFAA><b>[EMERGENCY TRAUMA SYRETTE SECURED]</b></color> You drove the dealer away but stashed a military morphine syrette in your medical pouch (+1 Field Medkit, +5 Morale).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 2. Barracks High-Stakes Poker
            dilemmas.Add(new EconomicDilemma
            {
                Id = "barracks_high_stakes_poker",
                Category = "BARRACKS VICE",
                Title = "Barracks High-Stakes Poker",
                Description = "In the damp shadows beneath the supply depot, veteran sergeants and ordnance corporals have overturned an artillery crate for a forbidden midnight poker match. Entire paychecks are stacked upon the felt.",
                ChoiceALabel = "Buy in with $500 and play aggressive bluffs (Intellect/Grit gamble)",
                ChoiceBLabel = "Bankroll house security and skim a 15% rake (+$950 Cash, +12 Heat, -10 Discipline)",
                ChoiceCLabel = "Raid the game as duty sergeant and order lights out (+15 Discipline, -25 Respect, -10 Morale)",
                EligibilityPredicate = s => !s.isCivilian && !s.isInCombatDeployment && s.cashBalance >= 500L,
                ActionA = (s, onComplete) =>
                {
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 10f);
                    bool win = (s.intellect >= 55f) || (UnityEngine.Random.value < 0.65f);
                    if (win)
                    {
                        s.cashBalance += 2200L;
                        s.happiness = Mathf.Min(100f, s.happiness + 15f);
                        s.squadRespect = Mathf.Min(100f, s.squadRespect + 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("POKER VICTORY",
                            "<color=#00FFAA><b>[ROYAL BLUFF CLEAN-OUT]</b></color> You stared down Master Sergeant Kowalski with three deuces and swept the entire pot! " +
                            "Won <color=#FFD700><b>+$2,200 Cash</b></color> (+15 Morale, +10 Squad Respect).");
                    }
                    else
                    {
                        s.cashBalance = Math.Max(0L, s.cashBalance - 500L);
                        s.happiness = Mathf.Max(10f, s.happiness - 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("POKER LOSS",
                            "<color=#FF6600>[BAD BEAT]</color> Kowalski revealed a full house on the river. You surrendered your buy-in (-$500 Cash, -10 Morale).");
                    }
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.cashBalance += 950L;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 12f);
                    s.discipline = Mathf.Max(0f, s.discipline - 10f);
                    s.squadRespect = Mathf.Min(100f, s.squadRespect + 10f);
                    s.totalCrimeEarnings += 950L;
                    s.crimesCommittedCount++;
                    GameEvents.OnFeedEventLogged?.Invoke("HOUSE RAKE",
                        "<color=#00FFAA><b>[GAMBLING RING OPERATOR]</b></color> You posted lookouts at the depot door and collected an orderly 15% table tax (+<b>$950 Cash</b>, +12 Heat).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.discipline = Mathf.Min(100f, s.discipline + 15f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 10f);
                    s.squadRespect = Mathf.Max(5f, s.squadRespect - 25f);
                    s.happiness = Mathf.Max(10f, s.happiness - 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("CRACKDOWN",
                        "<color=#AA4444>[ENFORCED LIGHTS OUT]</color> You kicked over the card table, confiscated the deck, and threatened everyone with court-martial. " +
                        "Officers approve (+15 Discipline), but the squad treats you with icy silence (-25 Squad Respect, -10 Morale).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 3. Corrupt Medical Board Bribery
            dilemmas.Add(new EconomicDilemma
            {
                Id = "corrupt_medical_board",
                Category = "MILITARY GRAFT",
                Title = "Corrupt Medical Board Bribery",
                Description = "During your annual physical assessment, the Chief Medical Examiner closes the examination room door. He drops subtle hints: an under-the-table 'expedited processing donation' of $2,000 will grant you permanent combat disability status with lifetime monthly pension benefits.",
                ChoiceALabel = "Pay $2,000 bribe for permanent VA disability stipend (+$300/mo, +25 Heat, -20 Discipline)",
                ChoiceBLabel = "Refuse indignantly and insist on honest medical records (+15 Discipline, +10 Merit, +10 Morale)",
                ChoiceCLabel = "Threaten to expose the bribery scheme to the Inspector General (Risk vs Whistleblower Bounty)",
                EligibilityPredicate = s => s.cashBalance >= 2000L,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance = Math.Max(0L, s.cashBalance - 2000L);
                    s.vaDisabilityAllowancePerMonth += 300L;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 25f);
                    s.discipline = Mathf.Max(0f, s.discipline - 20f);
                    s.AddTrait("Corrupt Disability Rating");
                    GameEvents.OnFeedEventLogged?.Invoke("PENSION FRAUD",
                        "<color=#00FFAA><b>[DISABILITY PENSION SECURED]</b></color> You handed an envelope stuffed with $2,000 cash to the medical examiner. " +
                        "The board classified you with permanent service trauma: <color=#00FFAA><b>+$300/Month Lifetime VA Disability Allowance</b></color> granted! (+25 Heat, -20 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.discipline = Mathf.Min(100f, s.discipline + 15f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 10f);
                    s.happiness = Mathf.Min(100f, s.happiness + 10f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("INTEGRITY",
                        "<color=#00D4FF><b>[HONORABLE CONDUCT]</b></color> You rebuked the examiner's extortion and completed the grueling medical battery honestly. " +
                        "Your record remains untainted (+15 Discipline, +10 Merit, +10 Morale).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    if (s.intellect >= 55f || s.politicalInfluence >= 20f)
                    {
                        s.cashBalance += 1500L;
                        s.meritRating = Mathf.Min(100f, s.meritRating + 20f);
                        s.coFavor = Mathf.Min(100f, s.coFavor + 15f);
                        s.AddTrait("Whistleblower Commendation");
                        GameEvents.OnFeedEventLogged?.Invoke("WHISTLEBLOWER",
                            "<color=#FFD700><b>[FRAUD RING EXPOSED]</b></color> Your covert audio recording nailed the corrupt medical board cold! " +
                            "Awarded official whistleblower bounty of <color=#00FFAA><b>+$1,500 Cash</b></color> and highest state commendations (+20 Merit, +15 CO Favor).");
                    }
                    else
                    {
                        s.meritRating = Mathf.Max(0f, s.meritRating - 20f);
                        s.coFavor = Mathf.Max(0f, s.coFavor - 15f);
                        s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 15f);
                        GameEvents.OnFeedEventLogged?.Invoke("RETALIATION",
                            "<color=#FF4444>[MEDICAL BOARD RETALIATION]</color> The entrenched medical brass counter-attacked, labeling you a defamatory malingerer (-20 Merit, -15 CO Favor).");
                    }
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 4. Looting Enemy Officer Vault
            dilemmas.Add(new EconomicDilemma
            {
                Id = "looting_enemy_vault",
                Category = "FRONTLINE PLUNDER",
                Title = "Looting Enemy Officer Vault",
                Description = "Breaching the shattered steel bunker of an enemy general staff headquarters, you discover a concealed fireproof safe containing minted gold sovereign coins, enemy campaign insignia, and bearer bonds.",
                ChoiceALabel = "Pocket the minted gold sovereigns secretly (+$6,500 Dirty Cash, +30 Heat, -20 Discipline)",
                ChoiceBLabel = "Split the bullion evenly among your assault squad (+$2,500 Cash, +35 Squad Respect, +15 Heat)",
                ChoiceCLabel = "Surrender the safe intact to Division Intelligence (+25 Merit, +20 CO Favor, +300 CMP)",
                EligibilityPredicate = s => !s.isCivilian && (s.isInCombatDeployment || s.combatSkill >= 30f),
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 6500L;
                    s.totalCrimeEarnings += 6500L;
                    s.crimesCommittedCount++;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 30f);
                    s.discipline = Mathf.Max(0f, s.discipline - 20f);
                    s.AddTrait("War Plunderer");
                    GameEvents.OnFeedEventLogged?.Invoke("WAR PLUNDER",
                        "<color=#FFD700><b>[CONCEALED GOLD SOVEREIGNS]</b></color> You stuffed your field webbing with glittering enemy bullion! " +
                        "Pocketed <color=#00FFAA><b>+$6,500 Dirty Cash</b></color>. High risk of Provost Marshal inspection (+30% Heat, -20 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.cashBalance += 2500L;
                    s.squadRespect = Mathf.Min(100f, s.squadRespect + 35f);
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 15f);
                    s.discipline = Mathf.Max(0f, s.discipline - 10f);
                    s.totalCrimeEarnings += 2500L;
                    s.crimesCommittedCount++;
                    GameEvents.OnFeedEventLogged?.Invoke("SQUAD PLUNDER",
                        "<color=#00D4FF><b>[SQUAD SHARES THE SPOILS]</b></color> You passed out heavy gold coins to every soldier in your fireteam. " +
                        "Your squad would charge through artillery barrage for you (+35 Squad Respect, +<b>$2,500 Cash</b>).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.meritRating = Mathf.Min(100f, s.meritRating + 25f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 20f);
                    s.combatMeritPoints += 300L;
                    s.AddMedal("Valerian Merit Ribbon");
                    GameEvents.OnFeedEventLogged?.Invoke("WAR TROPHY",
                        "<color=#FFD700><b>[CODEBOOKS & CIPHERS SURRENDERED]</b></color> Regimental Intelligence decoded vital battle orders from the surrendered safe. " +
                        "Awarded <b>Valerian Merit Ribbon</b> (+25 Merit, +20 CO Favor, +300 Combat Merit Points).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 5. Predatory Loan Shark Collection
            dilemmas.Add(new EconomicDilemma
            {
                Id = "loan_shark_collection",
                Category = "UNDERWORLD FINANCE",
                Title = "Predatory Loan Shark Collection",
                Description = "Two scarred underworld enforcers with lead pipes trap you in an alleyway outside the perimeter. They present an old gambling marker with astronomical compound interest, demanding an immediate settlement of $1,500.",
                ChoiceALabel = "Pay the $1,500 extortion debt in full (-$1,500 Cash, -15 Morale, -10 Heat)",
                ChoiceBLabel = "Fight the enforcers bare-handed in the alley (Combat/Fitness check vs Severe Injury)",
                ChoiceCLabel = "Offer to work off the debt enforcing syndicate contracts (+$1,000 Cash, +30 Heat, -25 Discipline)",
                EligibilityPredicate = s => !s.isImprisoned,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance = Math.Max(0L, s.cashBalance - 1500L);
                    s.happiness = Mathf.Max(10f, s.happiness - 15f);
                    s.criminalHeat = Mathf.Max(0f, s.criminalHeat - 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("DEBT SETTLED",
                        "<color=#FF8844>[LOAN SHARK EXTORTION]</color> You handed over your savings to settle the marker. " +
                        "The syndicates back off, but your wallet and pride are severely bruised (-$1,500 Cash, -15 Morale).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    if (s.fitness >= 55f || s.combatSkill >= 50f)
                    {
                        s.combatSkill = Mathf.Min(100f, s.combatSkill + 6f);
                        s.happiness = Mathf.Min(100f, s.happiness + 10f);
                        s.squadRespect = Mathf.Min(100f, s.squadRespect + 15f);
                        s.AddTrait("Alley Brawler");
                        GameEvents.OnFeedEventLogged?.Invoke("BRAWL VICTORY",
                            "<color=#00FFAA><b>[ENFORCERS OBLITERATED]</b></color> You sidestepped a lead pipe, broke the head enforcer's jaw, and sent them running bleeding through the alleys! " +
                            "Saved your money (+6 Combat Skill, +15 Squad Respect, +10 Morale).");
                    }
                    else
                    {
                        s.health = Mathf.Max(15f, s.health - 30f);
                        s.cashBalance = Math.Max(0L, s.cashBalance - 800L);
                        s.happiness = Mathf.Max(10f, s.happiness - 20f);
                        s.AddMedal("Imperial Wound Star");
                        GameEvents.OnFeedEventLogged?.Invoke("BEATEN IN ALLEY",
                            "<color=#FF4444>[SAVAGELY BEATEN]</color> The knuckle-dusters cracked your ribs and emptied your pockets! " +
                            "Hospitalized with severe contusions (-30 Health, -$800 Cash, -20 Morale, awarded Imperial Wound Star).");
                    }
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.cashBalance += 1000L;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 30f);
                    s.discipline = Mathf.Max(0f, s.discipline - 25f);
                    s.totalCrimeEarnings += 1000L;
                    s.crimesCommittedCount++;
                    s.AddTrait("Syndicate Enforcer");
                    GameEvents.OnFeedEventLogged?.Invoke("SYNDICATE PLEDGE",
                        "<color=#AA4444><b>[SYNDICATE DEBT ENFORCER]</b></color> You agreed to shake down civilian debtors on behalf of the loan syndicate. " +
                        "Debt cancelled with an advance payment (+<b>$1,000 Cash</b>, +30 Heat, -25 Discipline).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 6. Selling Military Fuel Drums
            dilemmas.Add(new EconomicDilemma
            {
                Id = "selling_fuel_drums",
                Category = "SUPPLY GRAFT",
                Title = "Selling Military Fuel Drums",
                Description = "A civilian transport syndicate offers to pay top coin for five barrels of high-octane military diesel rolled off the motor pool depot dock during late night watch rotation.",
                ChoiceALabel = "Roll the diesel drums into their truck and pocket cash (+$3,800 Dirty Cash, +22 Heat, -15 Discipline)",
                ChoiceBLabel = "Rig an ambush with base sentries to capture the cartel (+18 Merit, +15 CO Favor, +$800 Reward)",
                ChoiceCLabel = "Siphon two drums and water down the depot reserves (Intellect risk check)",
                EligibilityPredicate = s => !s.isCivilian,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 3800L;
                    s.totalCrimeEarnings += 3800L;
                    s.crimesCommittedCount++;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 22f);
                    s.discipline = Mathf.Max(0f, s.discipline - 15f);
                    s.AddTrait("Fuel Racketeer");
                    GameEvents.OnFeedEventLogged?.Invoke("FUEL GRAFT",
                        "<color=#00FFAA><b>[MILITARY DIESEL SIPHONED]</b></color> Five heavy fuel drums rolled into the cartel truck under cover of darkness. " +
                        "Earned <color=#FFD700><b>+$3,800 Dirty Cash</b></color> (+22% Heat, -15 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.meritRating = Mathf.Min(100f, s.meritRating + 18f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 15f);
                    s.discipline = Mathf.Min(100f, s.discipline + 10f);
                    s.cashBalance += 800L;
                    GameEvents.OnFeedEventLogged?.Invoke("AMBUSH VICTORY",
                        "<color=#00D4FF><b>[FUEL SMUGGLERS NABBED]</b></color> Sentries surrounded the smugglers at gunpoint! " +
                        "Base commander issued an official commendation with cash bonus (+18 Merit, +15 CO Favor, +$800 Legal Bounty).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    if (s.intellect >= 55f)
                    {
                        s.cashBalance += 1800L;
                        s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 10f);
                        s.totalCrimeEarnings += 1800L;
                        s.crimesCommittedCount++;
                        GameEvents.OnFeedEventLogged?.Invoke("DILUTED RESERVES",
                            "<color=#00FFAA><b>[CLEVER RATIO CHEM-BLEND]</b></color> You calculated the specific gravity dilution flawlessly. " +
                            "Inspectors noticed nothing (+<b>$1,800 Cash</b>, +10 Heat).");
                    }
                    else
                    {
                        s.discipline = Mathf.Max(0f, s.discipline - 20f);
                        s.meritRating = Mathf.Max(0f, s.meritRating - 20f);
                        s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 25f);
                        GameEvents.OnFeedEventLogged?.Invoke("ENGINE FAILURE",
                            "<color=#FF4444>[SPOILED FUEL DETECTED]</color> Diluted fuel clogged the armored convoy engines during morning drills! " +
                            "Military police launched an immediate depot investigation (-20 Merit, -20 Discipline, +25 Heat).");
                    }
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 7. Leaked Defense Munitions Stock
            dilemmas.Add(new EconomicDilemma
            {
                Id = "leaked_defense_stock",
                Category = "INSIDER TRADING",
                Title = "Leaked Defense Munitions Stock",
                Description = "An imperial general staff aide lets slip that the Empire will commission a massive artillery shell procurement contract next month with the Crown Armaments Guild, guaranteeing massive stock dividends.",
                ChoiceALabel = "Invest $2,000 in Crown Armaments insider shares (+$3,000 Net Profit, +2 Guild Shares, +20 Heat)",
                ChoiceBLabel = "Tip off your family clan elders to capitalize on contracts (+25 Clan Favor, +15 Influence, +$1,500 Cash)",
                ChoiceCLabel = "Burn the confidential memorandum and maintain secrecy (+15 Discipline, +12 Merit, +10 Favor)",
                EligibilityPredicate = s => s.cashBalance >= 2000L || s.isOfficer || s.isRetired,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 3000L; // Net profit ($2k invested -> $5k return)
                    s.tradeGuildShares += 2;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 20f);
                    s.discipline = Mathf.Max(0f, s.discipline - 10f);
                    s.AddTrait("Insider Trader");
                    GameEvents.OnFeedEventLogged?.Invoke("INSIDER PROFIT",
                        "<color=#00FFAA><b>[ARMAMENTS STOCK RALLIED 300%]</b></color> When the procurement order was declared, stock prices exploded! " +
                        "Secured <color=#FFD700><b>+$3,000 Net Cash Profit</b></color> and +2 Trade Guild Shares (+20 Heat, -10 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.clanFavor = Mathf.Min(100f, s.clanFavor + 25f);
                    s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 15f);
                    s.cashBalance += 1500L;
                    GameEvents.OnFeedEventLogged?.Invoke("CLAN DIVIDEND",
                        "<color=#00D4FF><b>[CLAN FORTUNE EXPANDED]</b></color> Your clan house purchased the armaments shares before the public market. " +
                        "Elders rewarded your loyalty with a lavish family dividend (+25 Clan Favor, +15 Influence, +$1,500 Cash).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.discipline = Mathf.Min(100f, s.discipline + 15f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 12f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("DUTY BOUND",
                        "<color=#00FFAA><b>[CLASSIFIED SECRECY MAINTAINED]</b></color> You preserved the sacred trust of military command intelligence (+15 Discipline, +12 Merit, +10 CO Favor).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 8. Forged Leave Transit Pass
            dilemmas.Add(new EconomicDilemma
            {
                Id = "forged_transit_pass",
                Category = "BLACK MARKET",
                Title = "Forged Leave Transit Pass",
                Description = "A crooked clerk in the battalion adjutant office has duplicated the General Staff brass authorization seal. For $400, he offers you a forged emergency furlough transit pass with official imperial stamps.",
                ChoiceALabel = "Buy forged pass and take luxury leave into the capital (-$400 Cash, +25 Morale, +18 Heat, -12 Discipline)",
                ChoiceBLabel = "Turn in the forgery ring to Regimental Counterintelligence (+15 Merit, +15 Favor, -15 Squad Respect)",
                ChoiceCLabel = "Blackmail the clerk to operate a joint transit pass racket (+$2,800 Dirty Cash, +35 Heat, -20 Discipline)",
                EligibilityPredicate = s => !s.isCivilian && !s.isInCombatDeployment && s.cashBalance >= 400L,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance = Math.Max(0L, s.cashBalance - 400L);
                    s.happiness = Mathf.Min(100f, s.happiness + 25f);
                    s.health = Mathf.Min(100f, s.health + 10f);
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 18f);
                    s.discipline = Mathf.Max(0f, s.discipline - 12f);
                    GameEvents.OnFeedEventLogged?.Invoke("FORGED FURLOUGH",
                        "<color=#00FFAA><b>[LUXURY CAPITAL WEEKEND]</b></color> You flashed the forged transit pass past railway sentries and enjoyed lavish tavern feasts! " +
                        "Morale soared (+25 Morale, +10 Health, -$400 Cash, +18 Heat).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.meritRating = Mathf.Min(100f, s.meritRating + 15f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 15f);
                    s.squadRespect = Mathf.Max(5f, s.squadRespect - 15f);
                    GameEvents.OnFeedEventLogged?.Invoke("FORGER BUSTED",
                        "<color=#00D4FF><b>[COUNTERFEIT SEALS SEIZED]</b></color> Military Intelligence raided the adjutant desk and shut down the forgery ring (+15 Merit, +15 CO Favor, -15 Squad Respect).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.cashBalance += 2800L;
                    s.totalCrimeEarnings += 2800L;
                    s.crimesCommittedCount++;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 35f);
                    s.discipline = Mathf.Max(0f, s.discipline - 20f);
                    s.AddTrait("Transit Pass Racketeer");
                    GameEvents.OnFeedEventLogged?.Invoke("FORGERY RACKET",
                        "<color=#FF8844><b>[PASS DISTRIBUTION RACKET]</b></color> You pressured the clerk to print passes for twenty AWOL soldiers, skimming massive payoffs! " +
                        "Looted <color=#FFD700><b>+$2,800 Dirty Cash</b></color> (+35 Heat, -20 Discipline).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 9. Extortion by Military Provost
            dilemmas.Add(new EconomicDilemma
            {
                Id = "provost_extortion",
                Category = "MILITARY CORRUPTION",
                Title = "Extortion by Military Provost",
                Description = "A crooked Military Police Lieutenant stops you at an inland checkpoint. He taps his holster and claims your travel documents contain 'grave irregularities,' demanding an immediate shakedown bribe of $800 to avoid 6 months in the stockade brig.",
                ChoiceALabel = "Pay the $800 extortion bribe quietly (-$800 Cash, -15 Morale, avoids brig)",
                ChoiceBLabel = "Stand at attention and demand an immediate JAG Court-Martial (Discipline/Merit check)",
                ChoiceCLabel = "Rally squadmates to stare down the crooked MP patrol (+30 Squad Respect, -15 Favor, +10 Heat)",
                EligibilityPredicate = s => !s.isCivilian && s.cashBalance >= 800L,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance = Math.Max(0L, s.cashBalance - 800L);
                    s.happiness = Mathf.Max(10f, s.happiness - 15f);
                    s.criminalHeat = Mathf.Max(0f, s.criminalHeat - 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("EXTORTION PAID",
                        "<color=#FF8844>[SHAKEDOWN BRIBE PAID]</color> You handed over your wallet to the corrupt MP lieutenant. " +
                        "Your record remains clear, but bitterness eats at your morale (-$800 Cash, -15 Morale).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    if (s.discipline >= 55f || s.meritRating >= 50f)
                    {
                        s.discipline = Mathf.Min(100f, s.discipline + 15f);
                        s.squadRespect = Mathf.Min(100f, s.squadRespect + 10f);
                        s.AddTrait("Unyielding Soldier");
                        GameEvents.OnFeedEventLogged?.Invoke("PROVOST BLUFF BROKEN",
                            "<color=#00FFAA><b>[JAG BLUFF CALLED]</b></color> You quoted Imperial Military Code chapters word-for-word! " +
                            "The lieutenant turned pale and waved you through immediately (+15 Discipline, +10 Squad Respect).");
                    }
                    else
                    {
                        s.militaryBrigSemestersRemaining = 1;
                        s.meritRating = Mathf.Max(0f, s.meritRating - 20f);
                        s.happiness = Mathf.Max(10f, s.happiness - 20f);
                        GameEvents.OnFeedEventLogged?.Invoke("STOCKADE SENTENCE",
                            "<color=#FF4444>[CONFINED TO BRIG]</color> The lieutenant framed you for insubordination! " +
                            "Sentenced to <b>6 Months in the Battalion Stockade Brig</b> on half-rations (-20 Merit, -20 Morale).");
                    }
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.squadRespect = Mathf.Min(100f, s.squadRespect + 30f);
                    s.coFavor = Mathf.Max(0f, s.coFavor - 15f);
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("SQUAD STANDOFF",
                        "<color=#00D4FF><b>[SQUAD STANDS UNITED]</b></color> Your fireteam rack their carbine bolts and stepped up beside you. " +
                        "The outnumbered MP patrol retreated back to their jeep (+30 Squad Respect, -15 CO Favor, +10 Heat).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 10. Plundered Golden Pocket Watch
            dilemmas.Add(new EconomicDilemma
            {
                Id = "plundered_pocket_watch",
                Category = "FRONTLINE PLUNDER",
                Title = "Plundered Golden Pocket Watch",
                Description = "Sifting through the rubble of an enemy division command carriage, your fingers clasp an exquisite gold pocket watch set with small rubies and embossed with royal heraldry, ticking steadily despite the cratered hell around you.",
                ChoiceALabel = "Keep the watch as a lucky talisman (+Lucky Watch Gear, +20 Morale, +Survival Bonus)",
                ChoiceBLabel = "Pawn the watch to a black-market fence (+$2,400 Dirty Cash, +10 Heat, -8 Discipline)",
                ChoiceCLabel = "Melt it down and send proceeds home to parents (+20 Living Condition, +25 Parent Rel, +15 Clan)",
                EligibilityPredicate = s => !s.isYouthPhase,
                ActionA = (s, onComplete) =>
                {
                    s.hasLuckyWatch = true;
                    s.happiness = Mathf.Min(100f, s.happiness + 20f);
                    s.AddTrait("Lucky Golden Chronometer");
                    GameEvents.OnFeedEventLogged?.Invoke("LUCKY HEIRLOOM",
                        "<color=#00FFAA><b>[LUCKY CHRONOMETER POCKETED]</b></color> You slid the heavy gold watch into your tunic breast pocket. " +
                        "Its rhythmic ticking grants an uncanny calm under fire (+Lucky Watch item, +20 Morale, +10% survival bonus).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.cashBalance += 2400L;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 10f);
                    s.discipline = Mathf.Max(0f, s.discipline - 8f);
                    s.totalCrimeEarnings += 2400L;
                    s.crimesCommittedCount++;
                    GameEvents.OnFeedEventLogged?.Invoke("WATCH FENCED",
                        "<color=#FFD700><b>[GOLD WATCH PAWNED]</b></color> An underworld jeweler paid top dollar for the royal gemstones (+<b>$2,400 Cash</b>, +10 Heat, -8 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.familyLivingCondition = Mathf.Min(100f, s.familyLivingCondition + 20f);
                    s.parentRelationship = Mathf.Min(100f, s.parentRelationship + 25f);
                    s.clanFavor = Mathf.Min(100f, s.clanFavor + 15f);
                    s.totalSentToFamily += 1500L;
                    GameEvents.OnFeedEventLogged?.Invoke("REMITTANCE",
                        "<color=#00FFAA><b>[FAMILY FORTUNE FORTIFIED]</b></color> You converted the bullion into a draft and mailed it to your family estate (+20 Family Living Condition, +25 Parent Relationship, +15 Clan Favor).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 11. Distressed Squadmate Emergency Loan
            dilemmas.Add(new EconomicDilemma
            {
                Id = "distressed_squadmate_loan",
                Category = "BROTHERHOOD & DEBT",
                Title = "Distressed Squadmate Emergency Loan",
                Description = "Your closest squadmate collapses on his cot in tears. His family's provincial tenant farm faces foreclosure, and ruthless loan sharks are threatening his mother. He desperately begs you for an emergency loan of $1,000.",
                ChoiceALabel = "Lend him $1,000 unconditionally as a brother in arms (-$1,000 Cash, +35 Squad Respect, +20 Morale)",
                ChoiceBLabel = "Lend $1,000 with a strict 40% repayment contract (-$1,000 Cash, +$1,400 Future, -25 Respect, -10 Morale)",
                ChoiceCLabel = "Refuse apologetically to protect your own savings (-15 Squad Respect, -10 Morale)",
                EligibilityPredicate = s => s.cashBalance >= 1000L,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance = Math.Max(0L, s.cashBalance - 1000L);
                    s.squadRespect = Mathf.Min(100f, s.squadRespect + 35f);
                    s.happiness = Mathf.Min(100f, s.happiness + 20f);
                    s.AddTrait("Loyal Benefactor");
                    GameEvents.OnFeedEventLogged?.Invoke("BROTHERHOOD LOAN",
                        "<color=#00FFAA><b>[A BROTHER SAVED]</b></color> You placed $1,000 into your comrade's hands with no conditions. " +
                        "The squad witnessed your selfless generosity (+35 Squad Respect, +20 Morale, trait: [Loyal Benefactor]).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.cashBalance = Math.Max(0L, s.cashBalance - 1000L);
                    s.cashBalance += 1400L; // Immediate net +$400 for sim resolution
                    s.squadRespect = Mathf.Max(5f, s.squadRespect - 25f);
                    s.happiness = Mathf.Max(10f, s.happiness - 10f);
                    s.AddTrait("Barracks Usurer");
                    GameEvents.OnFeedEventLogged?.Invoke("USURY CONTRACT",
                        "<color=#FF8844>[BARRACKS USURY]</color> You forced him to sign a 40% interest blood-voucher. " +
                        "You collected your principal plus $400 vigorish, but the squad now looks at you with disgust (-25 Squad Respect, -10 Morale).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.squadRespect = Mathf.Max(5f, s.squadRespect - 15f);
                    s.happiness = Mathf.Max(10f, s.happiness - 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("COLD REFUSAL",
                        "<color=#888888>[LOAN DENIED]</color> You shook your head and fastened your footlocker lock. " +
                        "Your squadmate walked away silently in despair (-15 Squad Respect, -10 Morale).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 12. Counterfeit War Bonds Racket
            dilemmas.Add(new EconomicDilemma
            {
                Id = "counterfeit_war_bonds",
                Category = "ORGANIZED CRIME",
                Title = "Counterfeit War Bonds Racket",
                Description = "A master engraver who escaped the state mint approaches you with stacks of indistinguishable counterfeit Imperial War Victory Bonds. He needs a decorated soldier with valid service credentials to cash them at the central treasury.",
                ChoiceALabel = "Redeem the forged bonds at the provincial treasury (+$10,000 Dirty Cash, +40 Heat, -25 Discipline)",
                ChoiceBLabel = "Arrest the engraver and turn plates over to Crown Police (+25 Merit, +20 Favor, +$2,500 Bounty, -20 Heat)",
                ChoiceCLabel = "Blackmail the engraver for a monthly silence tribute (+$3,000 Cash, +15 Heat, -15 Discipline)",
                EligibilityPredicate = s => !s.isImprisoned && (s.criminalHeat >= 15f || s.medalsEarnedCount >= 1 || s.isRetired || s.isCivilian),
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 10000L;
                    s.totalCrimeEarnings += 10000L;
                    s.crimesCommittedCount++;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 40f);
                    s.discipline = Mathf.Max(0f, s.discipline - 25f);
                    s.AddTrait("Counterfeit Bond Courier");
                    GameEvents.OnFeedEventLogged?.Invoke("COUNTERFEIT CASHOUT",
                        "<color=#FFD700><b>[TREASURY BONDS CASHED]</b></color> Flashing your military insignia, the treasury teller stamped the bonds without suspicion! " +
                        "Looted a monstrous <color=#00FFAA><b>+$10,000 Dirty Cash</b></color>! Crown investigators are frantic (+40% Heat, -25 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.meritRating = Mathf.Min(100f, s.meritRating + 25f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 20f);
                    s.cashBalance += 2500L;
                    s.criminalHeat = Mathf.Max(0f, s.criminalHeat - 20f);
                    GameEvents.OnFeedEventLogged?.Invoke("COUNTERFEIT CRACKDOWN",
                        "<color=#00FFAA><b>[CURRENCY FORGER ARRESTED]</b></color> You led Crown Detectives to the engraver's underground cellar! " +
                        "Awarded Imperial Treasury Bounty of <color=#FFD700><b>+$2,500 Cash</b></color> and full pardon on existing suspicions (+25 Merit, +20 CO Favor, -20% Heat).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.cashBalance += 3000L;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 15f);
                    s.discipline = Mathf.Max(0f, s.discipline - 15f);
                    s.totalCrimeEarnings += 3000L;
                    s.crimesCommittedCount++;
                    GameEvents.OnFeedEventLogged?.Invoke("HUSH TRIBUTE",
                        "<color=#FF8844><b>[EXTORTION TRIBUTE COLLECTED]</b></color> You allowed the engraver to slip out of the province in exchange for cold hard cash (+<b>$3,000 Cash</b>, +15 Heat).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 13. Decommissioned Heavy Weapons Brokerage
            dilemmas.Add(new EconomicDilemma
            {
                Id = "decommissioned_weapons_brokerage",
                Category = "VETERAN ARMS TRAFFICKING",
                Title = "Decommissioned Heavy Weapons Brokerage",
                Description = "An international mercenary consortium offers a staggering cash commission to divert a railway shipment of decommissioned 105mm howitzer barrels and artillery munitions bound for the scrap yard.",
                ChoiceALabel = "Broker the shadow arms export deal (+$24,000 Dirty Cash, +45 Heat, -25 Discipline)",
                ChoiceBLabel = "Tip off the Ministry of Defense to ambush the syndicate (+25 Influence, +25 Merit, +$6,000 Reward)",
                ChoiceCLabel = "Take the advance payment and sabotage the rail convoy (+$8,000 Cash, +15 Merit, +15 Heat)",
                EligibilityPredicate = s => s.isRetired || s.isOfficer || s.meritRating >= 45f,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 24000L;
                    s.totalCrimeEarnings += 24000L;
                    s.crimesCommittedCount++;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 45f);
                    s.discipline = Mathf.Max(0f, s.discipline - 25f);
                    s.AddTrait("Shadow Arms Trafficker");
                    GameEvents.OnFeedEventLogged?.Invoke("ARMS TRAFFICKING",
                        "<color=#FFD700><b>[HEAVY ARTILLERY TRAFFICKED]</b></color> Railcars were quietly diverted through port warehouses onto international freighters! " +
                        "Pocketed an immense king's ransom of <color=#00FFAA><b>+$24,000 Dirty Cash</b></color>! Imperial Security has opened a high-priority dossier (+45% Heat, -25 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 25f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 25f);
                    s.cashBalance += 6000L;
                    s.AddMedal("Distinguished Service Cross");
                    GameEvents.OnFeedEventLogged?.Invoke("NATIONAL DEFENSE",
                        "<color=#00FFAA><b>[ARMS SMUGGLERS INTERCEPTED]</b></color> Ministry strike teams intercepted the foreign cartel at the docks! " +
                        "Decorated with <b>Distinguished Service Cross</b> and <color=#FFD700><b>+$6,000 State Bounty</b></color> (+25 Political Influence, +25 Merit).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.cashBalance += 8000L;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 15f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 15f);
                    GameEvents.OnFeedEventLogged?.Invoke("RAILWAY SABOTAGE",
                        "<color=#00FFAA><b>[ADVANCE CASH TAKEN & RAILS BLOWN]</b></color> You pocketed their advance retainer, then signaled track engineers to derail the cargo before it reached the cartel (+<b>$8,000 Cash</b>, +15 Merit).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 14. Imperial Grain Hoarding Syndicate
            dilemmas.Add(new EconomicDilemma
            {
                Id = "imperial_grain_hoarding",
                Category = "CIVILIAN SPECULATION",
                Title = "Imperial Grain Hoarding Syndicate",
                Description = "A terrible provincial drought causes food scarcity. A cartel of merchant oligarchs invites you to contribute $3,000 into a grain hoarding syndicate to corner granaries and resell bread at triple prices.",
                ChoiceALabel = "Invest $3,000 into the predatory grain monopoly (+$6,000 Net Profit, +28 Heat, -20 Morale, -15 Discipline)",
                ChoiceBLabel = "Refuse and donate $1,000 to municipal soup kitchens (-$1,000 Cash, +30 Morale, +20 Clan Favor)",
                ChoiceCLabel = "Expose the cartel to the Crown Price Commissioner (+20 Merit, +15 Influence, +$2,000 Cash)",
                EligibilityPredicate = s => s.cashBalance >= 3000L || s.isCivilian,
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 6000L; // Net $6,000 gain
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 28f);
                    s.happiness = Mathf.Max(10f, s.happiness - 20f);
                    s.discipline = Mathf.Max(0f, s.discipline - 15f);
                    s.totalCrimeEarnings += 6000L;
                    s.crimesCommittedCount++;
                    s.AddTrait("War Profiteer");
                    GameEvents.OnFeedEventLogged?.Invoke("GRAIN SPECULATION",
                        "<color=#FF8844><b>[PREDATORY SPECULATION PROFIT]</b></color> Bread prices skyrocketed across the province. " +
                        "Your share netted <color=#FFD700><b>+$6,000 Net Profit</b></color>, but the cries of starving families weigh on your conscience (+28 Heat, -20 Morale, -15 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.cashBalance = Math.Max(0L, s.cashBalance - 1000L);
                    s.happiness = Mathf.Min(100f, s.happiness + 30f);
                    s.clanFavor = Mathf.Min(100f, s.clanFavor + 20f);
                    s.squadRespect = Mathf.Min(100f, s.squadRespect + 15f);
                    s.AddTrait("Patron of the Poor");
                    GameEvents.OnFeedEventLogged?.Invoke("SOUP KITCHEN PATRON",
                        "<color=#00FFAA><b>[RELIEF BREAD DISTRIBUTED]</b></color> You funded community bakeries delivering free loaves to thousands. " +
                        "The common folk revere your name (+30 Morale, +20 Clan Favor, trait: [Patron of the Poor]).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.meritRating = Mathf.Min(100f, s.meritRating + 20f);
                    s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 15f);
                    s.cashBalance += 2000L;
                    GameEvents.OnFeedEventLogged?.Invoke("PRICE CARTEL BUST",
                        "<color=#00FFAA><b>[SPECULATORS BUSTED]</b></color> Imperial magistrates raided the locked granaries and released food supplies (+20 Merit, +15 Influence, +$2,000 Cash Reward).");
                    CommitStateUpdate(s, onComplete);
                }
            });

            // 15. Ghost Payroll Embezzlement
            dilemmas.Add(new EconomicDilemma
            {
                Id = "ghost_payroll_embezzlement",
                Category = "OFFICER CORRUPTION",
                Title = "Ghost Payroll Embezzlement",
                Description = "While auditing battalion division rolls, you discover ten deceased or missing soldiers whose names were never removed from the active muster list, their monthly imperial paychecks quietly accumulating in an unmonitored escrow account.",
                ChoiceALabel = "Divert the ghost paychecks into private offshore accounts (+$7,500 Dirty Cash, +30 Heat, -20 Discipline)",
                ChoiceBLabel = "Disburse the unclaimed money to the fallen soldiers' destitute widows (+35 Squad Respect, +25 Morale)",
                ChoiceCLabel = "Report the bookkeeping defect to the Division Comptroller (+18 Discipline, +18 Merit, +15 CO Favor)",
                EligibilityPredicate = s => s.isOfficer || s.intellect >= 55f || (s.militaryBranch == MilitaryBranch.Army && s.meritRating >= 50f),
                ActionA = (s, onComplete) =>
                {
                    s.cashBalance += 7500L;
                    s.totalCrimeEarnings += 7500L;
                    s.crimesCommittedCount++;
                    s.criminalHeat = Mathf.Min(100f, s.criminalHeat + 30f);
                    s.discipline = Mathf.Max(0f, s.discipline - 20f);
                    s.AddTrait("Ghost Payroll Embezzler");
                    GameEvents.OnFeedEventLogged?.Invoke("EMBEZZLEMENT",
                        "<color=#FFD700><b>[GHOST PAYROLL EMBEZZLED]</b></color> With a few strokes of the pen, you routed dead men's salaries into your private fund! " +
                        "Pocketed <color=#00FFAA><b>+$7,500 Dirty Cash</b></color> (+30% Heat, -20 Discipline).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionB = (s, onComplete) =>
                {
                    s.squadRespect = Mathf.Min(100f, s.squadRespect + 35f);
                    s.happiness = Mathf.Min(100f, s.happiness + 25f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 15f);
                    s.AddTrait("Guardian of the Fallen");
                    GameEvents.OnFeedEventLogged?.Invoke("WIDOWS AIDED",
                        "<color=#00FFAA><b>[WAR WIDOW RELIEF]</b></color> You personally hand-delivered the accrued pensions to the weeping families of the fallen. " +
                        "Veterans throughout the empire pledge their undying honor to you (+35 Squad Respect, +25 Morale, +15 Merit).");
                    CommitStateUpdate(s, onComplete);
                },
                ActionC = (s, onComplete) =>
                {
                    s.discipline = Mathf.Min(100f, s.discipline + 18f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 18f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 15f);
                    GameEvents.OnFeedEventLogged?.Invoke("LEDGER AUDITED",
                        "<color=#00D4FF><b>[BATTERY ROSTER CLEANSED]</b></color> Division auditors commended your scrupulous fiscal honesty (+18 Discipline, +18 Merit, +15 CO Favor).");
                    CommitStateUpdate(s, onComplete);
                }
            });
        }

        // =========================================================================
        // PUBLIC API & TRIGGER LOGIC
        // =========================================================================

        /// <summary>
        /// Retrieves all scenarios currently eligible for the given soldier.
        /// </summary>
        public static List<EconomicDilemma> GetEligibleDilemmas(SoldierProfile soldier)
        {
            var eligible = new List<EconomicDilemma>();
            if (soldier == null) return eligible;

            foreach (var d in dilemmas)
            {
                if (d.IsEligible(soldier))
                {
                    eligible.Add(d);
                }
            }
            return eligible;
        }

        /// <summary>
        /// Looks up a dilemma by Id (or title if Id not matched).
        /// </summary>
        public static EconomicDilemma GetDilemmaById(string idOrTitle)
        {
            if (string.IsNullOrEmpty(idOrTitle)) return null;
            foreach (var d in dilemmas)
            {
                if (string.Equals(d.Id, idOrTitle, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.Title, idOrTitle, StringComparison.OrdinalIgnoreCase))
                {
                    return d;
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to select and trigger an eligible high-stakes economic / black-market dilemma.
        /// Invokes OnDilemmaTriggered and GameEvents.OnChildhoodDilemmaTriggered so UI popups handle player input.
        /// If running headless and AutoResolveInHeadlessMode is true, auto-resolves Choice A.
        /// </summary>
        public static bool TryTriggerEconomicDilemma(SoldierProfile soldier, Action onComplete = null)
        {
            if (soldier == null || soldier.isDead || soldier.isYouthPhase) return false;

            List<EconomicDilemma> eligible = GetEligibleDilemmas(soldier);
            if (eligible.Count == 0)
            {
                // Fallback to all non-null dilemmas if filtering was overly restrictive
                eligible = new List<EconomicDilemma>(dilemmas);
            }

            if (eligible.Count == 0) return false;

            int index = UnityEngine.Random.Range(0, eligible.Count);
            EconomicDilemma chosen = eligible[index];

            // Build individual choice callbacks
            Action actionA = () =>
            {
                ExecuteChoice(chosen, 0, soldier, onComplete);
            };

            Action actionB = () =>
            {
                ExecuteChoice(chosen, 1, soldier, onComplete);
            };

            Action actionC = chosen.HasChoiceC ? () =>
            {
                ExecuteChoice(chosen, 2, soldier, onComplete);
            } : (Action)null;

            // Broadcast to custom event and global decoupled game event bus
            OnDilemmaTriggered?.Invoke(
                chosen.Category,
                chosen.Title,
                chosen.Description,
                chosen.ChoiceALabel,
                chosen.ChoiceBLabel,
                chosen.ChoiceCLabel,
                actionA,
                actionB,
                actionC
            );

            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                chosen.Category,
                chosen.Title,
                chosen.Description,
                chosen.ChoiceALabel,
                chosen.ChoiceBLabel,
                chosen.ChoiceCLabel,
                actionA,
                actionB,
                actionC
            );

            // Headless fallback resolution if requested and no UI is subscribed
            if (AutoResolveInHeadlessMode && OnDilemmaTriggered == null && GameEvents.OnChildhoodDilemmaTriggered == null)
            {
                ExecuteChoice(chosen, 0, soldier, onComplete);
            }

            return true;
        }

        /// <summary>
        /// Direct / headless resolver for Unity batchmode testing and headless simulations.
        /// Executes the chosen option (0 for Choice A, 1 for Choice B, 2 for Choice C) immediately.
        /// </summary>
        public static bool ResolveDilemma(string dilemmaIdOrTitle, int choiceIndex, SoldierProfile soldier, Action onComplete = null)
        {
            EconomicDilemma dilemma = GetDilemmaById(dilemmaIdOrTitle);
            if (dilemma == null)
            {
                Debug.LogWarning($"[EconomicDilemmaDeck] Dilemma '{dilemmaIdOrTitle}' not found in deck!");
                return false;
            }
            return ResolveDilemma(dilemma, choiceIndex, soldier, onComplete);
        }

        /// <summary>
        /// Direct / headless resolver for a specific EconomicDilemma instance.
        /// </summary>
        public static bool ResolveDilemma(EconomicDilemma dilemma, int choiceIndex, SoldierProfile soldier, Action onComplete = null)
        {
            if (dilemma == null || soldier == null) return false;
            ExecuteChoice(dilemma, choiceIndex, soldier, onComplete);
            return true;
        }

        /// <summary>
        /// Resolves a dilemma by deck index.
        /// </summary>
        public static bool ResolveDilemmaByIndex(int dilemmaIndex, int choiceIndex, SoldierProfile soldier, Action onComplete = null)
        {
            if (dilemmaIndex < 0 || dilemmaIndex >= dilemmas.Count) return false;
            return ResolveDilemma(dilemmas[dilemmaIndex], choiceIndex, soldier, onComplete);
        }

        // =========================================================================
        // EXECUTION ENGINE
        // =========================================================================

        private static void ExecuteChoice(EconomicDilemma dilemma, int choiceIndex, SoldierProfile soldier, Action onComplete)
        {
            if (dilemma == null || soldier == null) return;

            switch (choiceIndex)
            {
                case 0:
                    dilemma.ActionA?.Invoke(soldier, onComplete);
                    break;
                case 1:
                    dilemma.ActionB?.Invoke(soldier, onComplete);
                    break;
                case 2:
                    if (dilemma.HasChoiceC && dilemma.ActionC != null)
                    {
                        dilemma.ActionC.Invoke(soldier, onComplete);
                    }
                    else
                    {
                        dilemma.ActionA?.Invoke(soldier, onComplete);
                    }
                    break;
                default:
                    dilemma.ActionA?.Invoke(soldier, onComplete);
                    break;
            }

            OnDilemmaResolved?.Invoke(dilemma, choiceIndex, soldier);
        }

        private static void CommitStateUpdate(SoldierProfile s, Action onComplete)
        {
            if (s != null)
            {
                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
            }
            onComplete?.Invoke();
        }
    }
}
