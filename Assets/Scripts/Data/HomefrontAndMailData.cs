using System;
using System.Collections.Generic;
using ArmyLife.Core;
using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Relationship relation of the person sending mail to the soldier.
    /// </summary>
    public enum MailSenderRelation
    {
        Sweetheart = 0,
        Mother = 1,
        Father = 2,
        Brother = 3,
        Sister = 4,
        Creditor = 5,
        HighCommandOrOfficer = 6,
        CivilianFriend = 7,
        HometownOfficial = 8
    }

    /// <summary>
    /// Narrative archetype of the letter received at mail call.
    /// </summary>
    public enum LetterCategory
    {
        Sweetheart = 0,
        FamilyHometown = 1,
        OfficerCommandInvitation = 2,
        CreditorDebt = 3,
        DearJohn = 4,
        GratitudeRemittance = 5,
        CivilianOpportunity = 6
    }

    /// <summary>
    /// Dynastic prosperity and living tier of the soldier's civilian family back home.
    /// </summary>
    public enum HomefrontProsperityTier
    {
        Destitute = 0,           // 0-19 pts: Dilapidated hovel, food rationing, high risk of foreclosure
        Struggling = 1,          // 20-39 pts: Modest tenant farm, vulnerable to poor harvests
        Modest = 2,              // 40-59 pts: Stable family homestead, warm hearth, basic comforts
        Comfortable = 3,         // 60-79 pts: Prosperous family land, livestock herds, respected in town
        Prosperous = 4,          // 80-94 pts: Fine provincial estate, merchant holdings, local prominence
        DynasticAristocrat = 5   // 95-100 pts: Aristocratic manor, dynastic clout, generational wealth
    }

    /// <summary>
    /// Romantic courtship progression with civilian sweetheart.
    /// </summary>
    public enum CourtshipStage
    {
        Single = 0,
        CasualSweetheart = 1,
        SeriousCourtship = 2,
        Engaged = 3,
        Married = 4,
        Heartbroken = 5
    }

    /// <summary>
    /// Player decision paths when confronting the dreaded "Dear John" breakup letter.
    /// </summary>
    public enum DearJohnChoice
    {
        FocusOnMission = 0,      // Discipline +10, Morale -15
        WriteDesperatePlea = 1,  // Morale -25
        TearItUpMoveForward = 2  // Morale +5, acquire "Iron Heart" trait
    }

    /// <summary>
    /// Rest & Recuperation (R&R) activities during capital leave.
    /// </summary>
    public enum FurloughActivityType
    {
        CanteenVisit = 0,        // Relax in capital canteen with comrades & music ($100)
        CourtSweetheart = 1,     // High-class dinner, gifts, and romantic promenade ($250)
        GetMarried = 2,          // Sacred chapel wedding feast ($1,000)
        InvestCivilianShops = 3, // Buy shares in capital trade shops/bakery ($1,500)
        InvestCivilianLand = 4,  // Purchase fertile rural acreage for family farm ($2,000)
        FamilyFarmVisit = 5      // Travel back to rural homestead to help parents ($150)
    }

    /// <summary>
    /// Interactive response choice presented on a mail letter.
    /// </summary>
    [Serializable]
    public class MailResponseOption
    {
        public string optionId;
        public string buttonLabel;
        public string description;
        public long remittanceCost;
        public float moraleModifier;
        public float disciplineModifier;
        public float familyProsperityModifier;
        public float courtshipAffectionModifier;
        public float squadRespectModifier;
        public float coFavorModifier;
        public string acquiredTrait;
        public string responseFeedMessage;

        public MailResponseOption() { }

        public MailResponseOption(
            string optionId,
            string buttonLabel,
            string description,
            long remittanceCost = 0,
            float moraleModifier = 0f,
            float disciplineModifier = 0f,
            float familyProsperityModifier = 0f,
            float courtshipAffectionModifier = 0f,
            float squadRespectModifier = 0f,
            string acquiredTrait = null,
            string responseFeedMessage = null)
        {
            this.optionId = optionId;
            this.buttonLabel = buttonLabel;
            this.description = description;
            this.remittanceCost = remittanceCost;
            this.moraleModifier = moraleModifier;
            this.disciplineModifier = disciplineModifier;
            this.familyProsperityModifier = familyProsperityModifier;
            this.courtshipAffectionModifier = courtshipAffectionModifier;
            this.squadRespectModifier = squadRespectModifier;
            this.acquiredTrait = acquiredTrait;
            this.responseFeedMessage = responseFeedMessage;
        }
    }

    /// <summary>
    /// Represents a letter received during mail call.
    /// </summary>
    [Serializable]
    public class MailLetter
    {
        public string letterId;
        public string senderName;
        public MailSenderRelation relation;
        public LetterCategory category;
        public string subject;
        [TextArea(4, 10)]
        public string letterBodyText;
        public int timestampSemester;
        public int timestampYear;
        public List<MailResponseOption> responseOptions = new List<MailResponseOption>();
        public bool requiresRemittance;
        public long remittanceAmountRequested;
        public float moraleImpactIfIgnored = 15f;
        public string censorStampText = "PASSED MILITARY CENSOR #42";
        public bool isDearJohn;
        public bool isRead;

        public MailLetter()
        {
            responseOptions = new List<MailResponseOption>();
        }

        public MailLetter(
            string letterId,
            string senderName,
            MailSenderRelation relation,
            LetterCategory category,
            string subject,
            string letterBodyText,
            int timestampYear = 2008,
            int timestampSemester = 1,
            bool requiresRemittance = false,
            long remittanceAmountRequested = 0,
            float moraleImpactIfIgnored = 15f,
            string censorStampText = "PASSED MILITARY CENSOR #42")
        {
            this.letterId = letterId;
            this.senderName = senderName;
            this.relation = relation;
            this.category = category;
            this.subject = subject;
            this.letterBodyText = letterBodyText;
            this.timestampYear = timestampYear;
            this.timestampSemester = timestampSemester;
            this.requiresRemittance = requiresRemittance;
            this.remittanceAmountRequested = remittanceAmountRequested;
            this.moraleImpactIfIgnored = moraleImpactIfIgnored;
            this.censorStampText = censorStampText;
            this.isDearJohn = (category == LetterCategory.DearJohn);
            this.responseOptions = new List<MailResponseOption>();
        }
    }

    /// <summary>
    /// Persistent homefront dynasty and personal relationship state.
    /// Tracks courtship, marriage, family prosperity tier, and remittances sent.
    /// </summary>
    [Serializable]
    public class HomefrontDynastyState
    {
        [Header("Sweetheart & Marriage")]
        public CourtshipStage courtshipStage = CourtshipStage.CasualSweetheart;
        public string sweetheartName = "Evelyn";
        [Range(0f, 100f)] public float courtshipAffection = 60f;
        public bool isMarried;
        public string spouseName;

        [Header("Homefront Prosperity & Remittances")]
        public HomefrontProsperityTier prosperityTier = HomefrontProsperityTier.Struggling;
        [Range(0f, 100f)] public float familyProsperityPoints = 30f;
        public long totalCivilianRemittancesSent;
        public int remittancesSentCount;
        public int lettersReceivedCount;
        public int lettersIgnoredCount;
        public int furloughsTakenCount;

        [Header("Civilian Capital Assets")]
        public long civilianShopInvestments;
        public int civilianLandParcels;

        [Header("Mail Call State")]
        public bool hasReceivedDearJohn;
        public bool pendingGratitudeLetterNextTurn;
        public string pendingGratitudeSender = "Mother";
        public long lastRemittanceAmount;

        public HomefrontDynastyState()
        {
            UpdateProsperityTier();
        }

        /// <summary>
        /// Synchronizes properties bidirectionally with SoldierProfile.
        /// </summary>
        public void SyncFromSoldier(SoldierProfile soldier)
        {
            if (soldier == null) return;

            // Sync romance & marriage
            if (soldier.isMarried)
            {
                isMarried = true;
                courtshipStage = CourtshipStage.Married;
                if (!string.IsNullOrEmpty(soldier.spouseName))
                {
                    spouseName = soldier.spouseName;
                    sweetheartName = soldier.spouseName;
                }
            }
            else if (!string.IsNullOrEmpty(soldier.childhoodRomanceInterest))
            {
                sweetheartName = soldier.childhoodRomanceInterest;
            }

            // Sync remittances & family well-being
            if (soldier.totalSentToFamily > totalCivilianRemittancesSent)
            {
                totalCivilianRemittancesSent = soldier.totalSentToFamily;
            }

            if (soldier.familyLivingCondition > 0f)
            {
                familyProsperityPoints = Mathf.Clamp(soldier.familyLivingCondition, 0f, 100f);
            }

            UpdateProsperityTier();
        }

        /// <summary>
        /// Writes homefront state back to SoldierProfile fields.
        /// </summary>
        public void SyncToSoldier(SoldierProfile soldier)
        {
            if (soldier == null) return;

            soldier.familyLivingCondition = familyProsperityPoints;
            soldier.totalSentToFamily = totalCivilianRemittancesSent;

            if (isMarried)
            {
                soldier.isMarried = true;
                if (!string.IsNullOrEmpty(spouseName)) soldier.spouseName = spouseName;
            }
        }

        public void AddProsperity(float amount)
        {
            familyProsperityPoints = Mathf.Clamp(familyProsperityPoints + amount, 0f, 100f);
            UpdateProsperityTier();
        }

        public void DeductProsperity(float amount)
        {
            familyProsperityPoints = Mathf.Clamp(familyProsperityPoints - amount, 0f, 100f);
            UpdateProsperityTier();
        }

        public void RecordRemittance(long amount, string senderBeneficiary = "Family")
        {
            totalCivilianRemittancesSent += amount;
            remittancesSentCount++;
            lastRemittanceAmount = amount;
            pendingGratitudeLetterNextTurn = true;
            pendingGratitudeSender = senderBeneficiary;

            // Remittance points: +1 pt per $100 up to $2,000 (+20 max per remittance)
            float pts = Mathf.Clamp(amount / 100f, 5f, 25f);
            AddProsperity(pts);
        }

        public void UpdateProsperityTier()
        {
            if (familyProsperityPoints < 20f)
                prosperityTier = HomefrontProsperityTier.Destitute;
            else if (familyProsperityPoints < 40f)
                prosperityTier = HomefrontProsperityTier.Struggling;
            else if (familyProsperityPoints < 60f)
                prosperityTier = HomefrontProsperityTier.Modest;
            else if (familyProsperityPoints < 80f)
                prosperityTier = HomefrontProsperityTier.Comfortable;
            else if (familyProsperityPoints < 95f)
                prosperityTier = HomefrontProsperityTier.Prosperous;
            else
                prosperityTier = HomefrontProsperityTier.DynasticAristocrat;
        }

        public string GetProsperityDescription()
        {
            switch (prosperityTier)
            {
                case HomefrontProsperityTier.Destitute:
                    return "Destitute (Dilapidated tenement, food rationing, facing eviction)";
                case HomefrontProsperityTier.Struggling:
                    return "Struggling (Modest tenant farm, vulnerable to harsh winter harvests)";
                case HomefrontProsperityTier.Modest:
                    return "Modest (Secure family homestead, warm hearth, stable pantry)";
                case HomefrontProsperityTier.Comfortable:
                    return "Comfortable (Prosperous acreage, cattle herds, respected in township)";
                case HomefrontProsperityTier.Prosperous:
                    return "Prosperous (Fine provincial estate, commercial investments, local prominence)";
                case HomefrontProsperityTier.DynasticAristocrat:
                    return "Dynastic Aristocrat (Ancestral palace citadel, vast patronage, political clout)";
                default:
                    return "Modest";
            }
        }
    }

    /// <summary>
    /// Outcome result for letter decision resolution.
    /// </summary>
    public class MailResolutionResult
    {
        public bool success = true;
        public string message;
        public long cashSpent;
        public float moraleChange;
        public float disciplineChange;
        public float prosperityChange;
        public string acquiredTrait;
        public bool wasDearJohn;
        public bool wasRemittanceSent;
        public bool wasIgnored;
    }

    /// <summary>
    /// Outcome result for furlough R&R activities.
    /// </summary>
    public class FurloughResult
    {
        public bool success = true;
        public string title;
        public string message;
        public long cashSpent;
        public float moraleChange;
        public string acquiredTrait;
        public FurloughActivityType activityType;
    }

    /// <summary>
    /// Catalog and generator of gritty, authentic wartime letter decks.
    /// </summary>
    public static class MailDeckCatalog
    {
        // =========================================================================
        // 1. SWEETHEART LETTERS DECK
        // =========================================================================

        public static MailLetter CreateSweetheartEncouragement(string sweetheartName, int year, int semester)
        {
            var letter = new MailLetter(
                "sweetheart_encouragement",
                sweetheartName,
                MailSenderRelation.Sweetheart,
                LetterCategory.Sweetheart,
                "Pressed Wildflowers & Missing You",
                $"My Dearest,\n\n" +
                $"I picked these purple mountain asters by the mill stream where we used to walk in spring. " +
                $"The autumn winds are cold here in the valley, but my heart stays warm thinking of you. " +
                $"Everyone in town talks about the brave boys at the front, but I only look for the mailman's bicycle every morning.\n\n" +
                $"Please keep your head low in the trenches. I pray by candle every night for the war to end and bring you home to me.\n\n" +
                $"With all my love forever,\n{sweetheartName}",
                year,
                semester,
                requiresRemittance: false,
                remittanceAmountRequested: 0,
                moraleImpactIfIgnored: 12f,
                censorStampText: "PASSED MILITARY CENSOR #18 - VALERIA POST"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "sweetheart_reply_tender",
                "Write Tender Romantic Reply",
                "Spend the evening drafting a poetic, heartfelt letter professing your love and enclosing dried trench heather.",
                remittanceCost: 0,
                moraleModifier: 15f,
                courtshipAffectionModifier: 20f,
                acquiredTrait: null,
                responseFeedMessage: $"You spent the quiet night by kerosene lantern writing a tender letter to {sweetheartName} (+15 Morale, +20 Sweetheart Affection)."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "sweetheart_send_keepsake",
                "Send PX Silver Keepsake ($150)",
                "Purchase an engraved sterling silver locket from the field PX exchange and mail it to her with your photograph.",
                remittanceCost: 150,
                moraleModifier: 20f,
                courtshipAffectionModifier: 30f,
                acquiredTrait: "Hopeless Romantic",
                responseFeedMessage: $"You mailed an engraved silver locket to {sweetheartName}. Your devotion is legendary (+20 Morale, +30 Affection, Trait: 'Hopeless Romantic')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "sweetheart_ignore",
                "Stash Letter Unanswered (Duty First)",
                "You have night watch and mortar drills. You shove the letter into your rucksack without replying.",
                remittanceCost: 0,
                moraleModifier: -8f,
                courtshipAffectionModifier: -10f,
                acquiredTrait: null,
                responseFeedMessage: $"You shoved {sweetheartName}'s letter into your kit without answering. The guilt gnaws at you during guard duty (-8 Morale)."
            ));

            return letter;
        }

        public static MailLetter CreateSweetheartMarriageHints(string sweetheartName, int year, int semester)
        {
            var letter = new MailLetter(
                "sweetheart_marriage_hints",
                sweetheartName,
                MailSenderRelation.Sweetheart,
                LetterCategory.Sweetheart,
                "Our Dreams for After the War",
                $"My Darling,\n\n" +
                $"Reverend Thomas asked after you yesterday after church service. Old Mrs. Finch said the old miller's stone cottage by the brook is coming up for lease in the spring. " +
                $"I found myself daydreaming about white lace curtains and a small vegetable garden.\n\n" +
                $"Do you ever think about our future when your tour is over? What kind of home will we build together once the guns fall silent?\n\n" +
                $"Faithfully yours,\n{sweetheartName}",
                year,
                semester,
                requiresRemittance: false,
                remittanceAmountRequested: 0,
                moraleImpactIfIgnored: 15f,
                censorStampText: "PASSED MILITARY CENSOR #42 - EXPEDITIONARY MAIL"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "marriage_promise_reply",
                "Promise Your Hand & Vow Marriage",
                "Write an impassioned pledge swearing that as soon as peace arrives or on your next capital leave, you will make her your bride.",
                remittanceCost: 0,
                moraleModifier: 20f,
                courtshipAffectionModifier: 25f,
                acquiredTrait: "Sworn Betrothed",
                responseFeedMessage: $"You pledged your sacred vow of marriage to {sweetheartName}! Your squadmates cheer your impending betrothal (+20 Morale, Trait: 'Sworn Betrothed')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "marriage_hesitant_reply",
                "Urge Caution & Wartime Patience",
                "Explain the brutal uncertainty of the frontline and gently ask to wait until the war concludes before making promises.",
                remittanceCost: 0,
                moraleModifier: 0f,
                courtshipAffectionModifier: 5f,
                acquiredTrait: null,
                responseFeedMessage: $"You urged pragmatic patience until your combat contract finishes. {sweetheartName} respects your stoic honesty."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "marriage_hints_ignore",
                "Discard Letter Without Comment",
                "You cannot bear to think of civilian domestic life while comrades are dying around you. You discard the letter.",
                remittanceCost: 0,
                moraleModifier: -12f,
                courtshipAffectionModifier: -20f,
                acquiredTrait: "Hardened Stoic",
                responseFeedMessage: $"You burned the letter over an entrenching candle. Emotional detachment hardens your heart (-12 Morale, Trait: 'Hardened Stoic')."
            ));

            return letter;
        }

        public static MailLetter CreateSweetheartAnxiety(string sweetheartName, int year, int semester)
        {
            var letter = new MailLetter(
                "sweetheart_wartime_anxiety",
                sweetheartName,
                MailSenderRelation.Sweetheart,
                LetterCategory.Sweetheart,
                "Blackouts, Air Sirens & Constant Fear",
                $"My Love,\n\n" +
                $"The war is coming closer to the homefront. Three nights ago the air sirens wailed and we hid in the church cellar while enemy reconnaissance bombers droned overhead. " +
                $"Young corporal Higgins from down the road was brought back missing both legs. His mother’s wailing kept the entire street awake.\n\n" +
                $"I am so terrified, my love. Every time someone knocks on the front door, my heart stops thinking it is a telegram from the Ministry of War.\n\n" +
                $"Please tell me you are safe.\n{sweetheartName}",
                year,
                semester,
                requiresRemittance: false,
                remittanceAmountRequested: 0,
                moraleImpactIfIgnored: 15f,
                censorStampText: "PASSED MILITARY CENSOR #09 - HOME DEFENSE"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "anxiety_comfort_reply",
                "Write Reassuring Letter of Strength",
                "Send a calm, confident letter assuring her of your unit's entrenched defenses and iron discipline.",
                remittanceCost: 0,
                moraleModifier: 12f,
                courtshipAffectionModifier: 15f,
                acquiredTrait: null,
                responseFeedMessage: $"Your reassuring words brought peace to {sweetheartName}'s sleepless nights (+12 Morale, +15 Affection)."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "anxiety_remittance_aid",
                "Send Comfort Remittance ($250)",
                "Send funds so she can purchase canned provisions, candles, and fortified shelter timber.",
                remittanceCost: 250,
                moraleModifier: 18f,
                courtshipAffectionModifier: 25f,
                familyProsperityModifier: 5f,
                acquiredTrait: "Homefront Guardian",
                responseFeedMessage: $"You sent $250 for civilian air raid provisions. {sweetheartName} wept with gratitude (+18 Morale, Trait: 'Homefront Guardian')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "anxiety_ignore",
                "Burn Letter (War Has No Room for Fear)",
                "You cannot afford to let civilian terror infect your frontline combat focus. You burn the letter.",
                remittanceCost: 0,
                moraleModifier: -10f,
                courtshipAffectionModifier: -15f,
                acquiredTrait: "Hardened Stoic",
                responseFeedMessage: $"You watched the letter turn to ash in the trench brazier (-10 Morale, Trait: 'Hardened Stoic')."
            ));

            return letter;
        }

        public static MailLetter CreateDearJohnLetter(string sweetheartName, int year, int semester)
        {
            var letter = new MailLetter(
                "dear_john_letter",
                sweetheartName,
                MailSenderRelation.Sweetheart,
                LetterCategory.DearJohn,
                "Forgive Me... (Goodbye)",
                $"Dear Soldier,\n\n" +
                $"I have rewritten this letter a dozen times, and my hands are trembling as I hold the pen. " +
                $"The three years of separation and the terrifying silence between campaigns have been too heavy for me to bear alone. " +
                $"I feel like I am in love with a ghost in uniform.\n\n" +
                $"I have met someone here in the province—a foreman at the railway transport depot. He asked for my hand, and yesterday I accepted his ring. " +
                $"I know you will despise me, and perhaps I deserve it. Please forgive me, and please survive this cruel war.\n\n" +
                $"Farewell,\n{sweetheartName}",
                year,
                semester,
                requiresRemittance: false,
                remittanceAmountRequested: 0,
                moraleImpactIfIgnored: 20f,
                censorStampText: "PASSED MILITARY CENSOR #66 - REDACTED PERSONAL"
            );

            letter.isDearJohn = true;

            letter.responseOptions.Add(new MailResponseOption(
                "dear_john_mission",
                "Focus on the Mission",
                "Choke down the grief, lock your heart in armor, and volunteer for front-line assault duty. (Discipline +10, Morale -15)",
                remittanceCost: 0,
                moraleModifier: -15f,
                disciplineModifier: 10f,
                acquiredTrait: "Cold Professional",
                responseFeedMessage: "You folded the breakup letter into your breast pocket and cleaned your rifle. Love is dead; only duty remains (Discipline +10, Morale -15, Trait: 'Cold Professional')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "dear_john_plea",
                "Write Desperate Plea",
                "Pour your broken soul onto the paper, begging her to reconsider and wait for your return. (Morale -25)",
                remittanceCost: 0,
                moraleModifier: -25f,
                disciplineModifier: -5f,
                acquiredTrait: "Heartbroken",
                responseFeedMessage: "You spent three nights in drunken despair writing pages of desperate pleas. The letter went unanswered (Morale -25, Trait: 'Heartbroken')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "dear_john_tear_up",
                "Tear It Up & Move Forward",
                "Tear the letter into confetti, toss it into the mud, and refuse to let betrayal break your spirit. (Morale +5, Trait: 'Iron Heart')",
                remittanceCost: 0,
                moraleModifier: 5f,
                disciplineModifier: 5f,
                acquiredTrait: "Iron Heart",
                responseFeedMessage: "You tore the Dear John letter into scraps and cast them into the mud. You emerge unshakable (Morale +5, Discipline +5, Trait: 'Iron Heart')."
            ));

            return letter;
        }

        // =========================================================================
        // 2. FAMILY FARM / HOMETOWN LETTERS DECK
        // =========================================================================

        public static MailLetter CreateHarvestStrugglesLetter(int year, int semester)
        {
            var letter = new MailLetter(
                "family_harvest_struggles",
                "Father (Julian)",
                MailSenderRelation.Father,
                LetterCategory.FamilyHometown,
                "Early Frost & Ruined Grain Crops",
                $"My Son,\n\n" +
                $"A brutal frost swept across the lowlands three weeks before harvest, freezing the ears of barley in the field. " +
                $"We lost nearly seventy percent of the winter crop. The millers are demanding full price for livestock feed, and our draft horses will not survive the winter without oats.\n\n" +
                $"If you can spare four hundred dollars from your military pay stipend, it will allow us to buy feed and spring seed grain. " +
                $"Your mother’s arthritis worsens with the damp cold, but she sends her blessing.\n\n" +
                $"Your Father",
                year,
                semester,
                requiresRemittance: true,
                remittanceAmountRequested: 400,
                moraleImpactIfIgnored: 18f,
                censorStampText: "PASSED MILITARY CENSOR #33 - RURAL DISTRICT"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "harvest_send_full",
                "Send $400 Harvest Remittance",
                "Wire $400 from your pay savings. Saves the family livestock and buys certified spring seed grain.",
                remittanceCost: 400,
                moraleModifier: 15f,
                familyProsperityModifier: 15f,
                acquiredTrait: "Devoted Son",
                responseFeedMessage: "You wired $400 back to the homestead. Your father saved the livestock and praised your filial devotion (+15 Morale, +15 Family Prosperity, Trait: 'Devoted Son')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "harvest_send_partial",
                "Send Partial $200 Remittance",
                "Send $200 with an encouraging letter asking father to sell off surplus tools to bridge the gap.",
                remittanceCost: 200,
                moraleModifier: 8f,
                familyProsperityModifier: 8f,
                acquiredTrait: null,
                responseFeedMessage: "You wired $200 home. The farm scraped through the winter on lean rations (+8 Morale, +8 Prosperity)."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "harvest_ignore",
                "Ignore Request (Save Pay for Yourself)",
                "You cannot afford to deplete your combat savings. You throw the letter aside.",
                remittanceCost: 0,
                moraleModifier: -18f,
                familyProsperityModifier: -12f,
                acquiredTrait: "Hardened Stoic",
                responseFeedMessage: "You ignored your father's plea. The family was forced to slaughter two workhorses to survive the winter (-18 Morale, -12 Prosperity)."
            ));

            return letter;
        }

        public static MailLetter CreateMortgageForeclosureLetter(int year, int semester)
        {
            var letter = new MailLetter(
                "family_mortgage_foreclosure",
                "Imperial Provincial Bailiff",
                MailSenderRelation.Creditor,
                LetterCategory.CreditorDebt,
                "FINAL NOTICE: Homestead Foreclosure Action",
                $"ATTENTION: Soldier of the Imperial Army,\n\n" +
                $"Take official notice that the ancestral farm mortgage of Deed #4092-B (held by the Vance Household) is in arrears by $1,000. " +
                $"The Provincial Agricultural Credit Bank has scheduled public auction of the property at next month's magistrate court unless the delinquent balance is satisfied in full.\n\n" +
                $"Military pay garnishment or immediate wire transfer is required to halt foreclosure proceedings and prevent immediate eviction of your parents.\n\n" +
                $"Bailiff M. Corvus\nProvincial Revenue Office",
                year,
                semester,
                requiresRemittance: true,
                remittanceAmountRequested: 1000,
                moraleImpactIfIgnored: 25f,
                censorStampText: "PASSED MILITARY CENSOR #02 - LEGAL OFFICIAL"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "foreclosure_pay_full",
                "Wire Full $1,000 Mortgage Payoff",
                "Clear the entire delinquent debt from your combat treasury. Permanently preserves ancestral land and relieves parents.",
                remittanceCost: 1000,
                moraleModifier: 25f,
                familyProsperityModifier: 25f,
                acquiredTrait: "Savior of the Homestead",
                responseFeedMessage: "You wired $1,000 to the Provincial Bank! The foreclosure was canceled and your parents wept with joy (+25 Morale, +25 Prosperity, Trait: 'Savior of the Homestead')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "foreclosure_pay_extension",
                "Pay $400 Stay of Eviction",
                "Pay an interim legal stay fee of $400 to forestall foreclosure for another year while parents negotiate.",
                remittanceCost: 400,
                moraleModifier: 5f,
                familyProsperityModifier: 10f,
                acquiredTrait: null,
                responseFeedMessage: "You paid a $400 legal extension fee. The foreclosure was stayed for twelve months (+5 Morale, +10 Prosperity)."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "foreclosure_default",
                "Refuse / Default on Debt",
                "You refuse to bail out the family debt. Let the bailiffs seize the acreage.",
                remittanceCost: 0,
                moraleModifier: -25f,
                familyProsperityModifier: -30f,
                acquiredTrait: "Hardened Stoic",
                responseFeedMessage: "You let the foreclosure proceed. Your parents were evicted to a provincial tenement (-25 Morale, -30 Prosperity, Trait: 'Hardened Stoic')."
            ));

            return letter;
        }

        public static MailLetter CreateSiblingDraftLetter(int year, int semester)
        {
            var letter = new MailLetter(
                "family_sibling_draft",
                "Mother (Eleanor)",
                MailSenderRelation.Mother,
                LetterCategory.FamilyHometown,
                "Peter Got His Conscription Notice!",
                $"My Dearest Boy,\n\n" +
                $"I can hardly see the paper for my tears. Yesterday the Imperial Gendarmerie delivered conscription papers for your younger brother Peter. " +
                $"He is only seventeen, Ethan! They are assigning him directly to the 14th Infantry Shock Division as a line recruit.\n\n" +
                $"Peter is terrified, though he tries to puff out his chest like a brave soldier. " +
                $"He needs $300 for proper combat boots, insulated thermal undergarments, and field rations. " +
                $"More than money, Ethan, please write to him! Tell him how to survive the trenches!\n\n" +
                $"Your heartbroken Mother",
                year,
                semester,
                requiresRemittance: true,
                remittanceAmountRequested: 300,
                moraleImpactIfIgnored: 15f,
                censorStampText: "PASSED MILITARY CENSOR #77 - FAMILY MAIL"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "sibling_send_gear_funds",
                "Send $300 & Combat Survival Guide",
                "Wire $300 for premium infantry boots and write a 6-page manual detailing how to survive artillery barrages and sniper fire.",
                remittanceCost: 300,
                moraleModifier: 18f,
                disciplineModifier: 5f,
                familyProsperityModifier: 10f,
                acquiredTrait: "Protective Big Brother",
                responseFeedMessage: "You equipped little Peter with $300 of gear and veteran survival advice. Mother's prayers bless you (+18 Morale, Trait: 'Protective Big Brother')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "sibling_veteran_advice_only",
                "Write Inspiring Combat Letter (Free)",
                "Write Peter an inspiring letter emphasizing weapon maintenance, trench hygiene, and staying low under fire.",
                remittanceCost: 0,
                moraleModifier: 10f,
                disciplineModifier: 5f,
                familyProsperityModifier: 0f,
                acquiredTrait: null,
                responseFeedMessage: "You wrote an earnest letter to recruit Peter. Your frontline wisdom will keep him alive in basic training (+10 Morale)."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "sibling_ignore",
                "Ignore Letter (Everyone Must Fight)",
                "The war spares no one. Peter must learn to be a man the hard way, just like you did.",
                remittanceCost: 0,
                moraleModifier: -15f,
                familyProsperityModifier: -5f,
                acquiredTrait: "Hardened Stoic",
                responseFeedMessage: "You ignored your mother's tears. Peter deployed to the trenches unequipped and fearful (-15 Morale)."
            ));

            return letter;
        }

        public static MailLetter CreateFamilySicknessLetter(int year, int semester)
        {
            var letter = new MailLetter(
                "family_sickness_epidemic",
                "Sister (Seraphina)",
                MailSenderRelation.Sister,
                LetterCategory.FamilyHometown,
                "Mother Struck with Winter Lung Fever",
                $"Dearest Brother,\n\n" +
                $"Mother collapsed in the kitchen four days ago with rattling coughing and burning fever. " +
                $"The village apothecary says her lungs are full of winter fluid, and ordinary herbal teas will not save her. " +
                $"A physician from the provincial capital can bring penicillin and respiratory tonics, but he demands $500 in upfront cash before boarding the carriage.\n\n" +
                $"Father is beside himself with worry and has not slept in three days. We need you, brother.\n\n" +
                $"Your loving Sister,\nSeraphina",
                year,
                semester,
                requiresRemittance: true,
                remittanceAmountRequested: 500,
                moraleImpactIfIgnored: 22f,
                censorStampText: "PASSED MILITARY CENSOR #15 - MEDICAL PRIORITY"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "sickness_hire_physician",
                "Wire $500 for Capital Physician",
                "Pay the upfront fee to dispatch the capital physician with penicillin and restorative medicines.",
                remittanceCost: 500,
                moraleModifier: 20f,
                familyProsperityModifier: 15f,
                acquiredTrait: "Dutiful Provider",
                responseFeedMessage: "You wired $500 immediately. The capital physician saved your mother's life! She made a full recovery (+20 Morale, +15 Prosperity, Trait: 'Dutiful Provider')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "sickness_local_herbs",
                "Send $150 for Local Remedies",
                "Wire $150 to buy local apothecary draughts and tinctures.",
                remittanceCost: 150,
                moraleModifier: 5f,
                familyProsperityModifier: 5f,
                acquiredTrait: null,
                responseFeedMessage: "You sent $150 for local herbal medicine. Mother slowly recovered after two months in bed (+5 Morale)."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "sickness_ignore",
                "Ignore Request",
                "You keep your wages in your pocket and ignore the telegram.",
                remittanceCost: 0,
                moraleModifier: -22f,
                familyProsperityModifier: -15f,
                acquiredTrait: "Hardened Stoic",
                responseFeedMessage: "You did not send aid. Mother lingered near death for weeks, permanently weakening her constitution (-22 Morale, -15 Prosperity)."
            ));

            return letter;
        }

        // =========================================================================
        // 3. OFFICER / HIGH COMMAND SOCIAL INVITATIONS DECK
        // =========================================================================

        public static MailLetter CreateCapitalGalaInvitation(int year, int semester)
        {
            var letter = new MailLetter(
                "officer_capital_gala",
                "General Staff Social Liaison",
                MailSenderRelation.HighCommandOrOfficer,
                LetterCategory.OfficerCommandInvitation,
                "Invitation: High Command Winter Officers' Ball",
                $"To the Honorable Soldier,\n\n" +
                $"By commendation of Imperial High Command, you are cordially invited to attend the sovereign Winter Officers' & Heroes' Gala " +
                $"at the Imperial Citadel Palace during upcoming seasonal furlough.\n\n" +
                $"The ballroom will host cabinet ministers, senior generals, and prominent civilian industrialists. " +
                $"Attire: Formal Dress Uniform with Medal Ribbons. Attendance grants high command favor and aristocratic social connections.\n\n" +
                $"Major General Von Halen\nImperial Citadel Liaison",
                year,
                semester,
                requiresRemittance: false,
                remittanceAmountRequested: 0,
                moraleImpactIfIgnored: 5f,
                censorStampText: "OFFICIAL COMMAND DISPATCH - GOLD EMBOSSED"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "gala_accept_splurge",
                "Attend in Tailored Dress Uniform ($350)",
                "Purchase a gold-trimmed tailored dress uniform and attend the gala in grand aristocratic style.",
                remittanceCost: 350,
                moraleModifier: 25f,
                disciplineModifier: 5f,
                courtshipAffectionModifier: 15f,
                acquiredTrait: "High Society Darling",
                responseFeedMessage: "You dazzled the high command gala! Generals toasted your frontline bravery and patrons offered lucrative civilian connections (+25 Morale, Trait: 'High Society Darling')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "gala_attend_standard",
                "Attend in Standard Service Dress (Free)",
                "Attend in standard pressed service khakis. Respectable, humble, and soldierly.",
                remittanceCost: 0,
                moraleModifier: 15f,
                disciplineModifier: 5f,
                acquiredTrait: null,
                responseFeedMessage: "You attended the gala in standard combat dress. Senior officers applauded your grounded humility (+15 Morale, +5 Discipline)."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "gala_decline",
                "Decline Invitation (Stay with the Squad)",
                "You have no taste for champagne and velvet while comrades patrol the cold wire. You politely decline.",
                remittanceCost: 0,
                moraleModifier: 5f,
                disciplineModifier: 10f,
                squadRespectModifier: 15f,
                acquiredTrait: "Grunt's Champion",
                responseFeedMessage: "You turned down high society to drink cheap beer with your squadmates in the barracks (+15 Squad Respect, Trait: 'Grunt's Champion')."
            ));

            return letter;
        }

        public static MailLetter CreateIndustrialistPatronageLetter(int year, int semester)
        {
            var letter = new MailLetter(
                "officer_wealthy_patronage",
                "Baron Eric Rothwell (Imperial Munitions)",
                MailSenderRelation.HighCommandOrOfficer,
                LetterCategory.OfficerCommandInvitation,
                "Confidential: Dynastic Patronage & Investment Opportunity",
                $"Esteemed Soldier,\n\n" +
                $"My directors and I have followed your decorated battlefield service with profound admiration. " +
                $"As Chairman of Rothwell Defense Foundry, I believe frontline combat veterans deserve a stake in the industrial engine that feeds the empire.\n\n" +
                $"I am offering you priority private placement shares in our naval artillery expansion syndicate ($800 buy-in), " +
                $"which promises guaranteed biannual dividends and establishes a lasting dynastic bond between our houses.\n\n" +
                $"With profound esteem,\nBaron Eric Rothwell",
                year,
                semester,
                requiresRemittance: true,
                remittanceAmountRequested: 800,
                moraleImpactIfIgnored: 0f,
                censorStampText: "PASSED MILITARY CENSOR #01 - EXECUTIVE DIPLOMATIC"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "patronage_accept_investment",
                "Accept Patronage & Invest $800",
                "Wire $800 for priority industrial syndicate shares. Builds immense dynastic wealth and industrial clout.",
                remittanceCost: 800,
                moraleModifier: 20f,
                familyProsperityModifier: 25f,
                acquiredTrait: "Aristocratic Protégé",
                responseFeedMessage: "You partnered with Baron Rothwell! Your family gained prestigious industrial dividends (+20 Morale, +25 Prosperity, Trait: 'Aristocratic Protégé')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "patronage_decline_cordial",
                "Send Cordial Decline Letter",
                "Write a polite letter expressing honor at the proposal but pleading current lack of investment liquidity.",
                remittanceCost: 0,
                moraleModifier: 5f,
                acquiredTrait: null,
                responseFeedMessage: "You cordially declined the baron's investment offer. He replied with words of continued respect."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "patronage_burn",
                "Discard as War Profiteering",
                "You scoff at defense industrialists getting rich while grunts bleed in trenches. You discard the letter.",
                remittanceCost: 0,
                moraleModifier: 0f,
                disciplineModifier: 5f,
                acquiredTrait: "Incorruptible Grunt",
                responseFeedMessage: "You tossed the industrialist's invitation in the trash (+5 Discipline, Trait: 'Incorruptible Grunt')."
            ));

            return letter;
        }

        // =========================================================================
        // 4. FAMILY REMITTANCE GRATITUDE LETTER
        // =========================================================================

        public static MailLetter CreateGratitudeLetter(string senderName, long amountSent, int year, int semester)
        {
            var letter = new MailLetter(
                "family_remittance_gratitude",
                senderName,
                MailSenderRelation.Mother,
                LetterCategory.GratitudeRemittance,
                "Tears of Gratitude & Our Prayers for You",
                $"My Beloved Ethan,\n\n" +
                $"The post rider delivered the official postal money order for ${amountSent:N0} this past Tuesday. " +
                $"I cannot begin to describe the relief that flooded our home. Your father sat at the wooden table and wept with gratitude, thanking Heaven for blessing us with such a devoted son.\n\n" +
                $"Because of your remittance, our debts are settled, the pantry is full of smoked ham and winter apples, and the village treats us with honor. " +
                $"Take care of yourself out there. Knowing you are looking after us gives us strength every single dawn.\n\n" +
                $"With all our endless love,\nYour Family ({senderName})",
                year,
                semester,
                requiresRemittance: false,
                remittanceAmountRequested: 0,
                moraleImpactIfIgnored: 5f,
                censorStampText: "PASSED MILITARY CENSOR #12 - FAMILY GRATITUDE"
            );

            letter.responseOptions.Add(new MailResponseOption(
                "gratitude_warm_reply",
                "Reply: 'Everything I do is for family'",
                "Write a loving postcard reassuring your family that serving them is your highest pride.",
                remittanceCost: 0,
                moraleModifier: 15f,
                familyProsperityModifier: 5f,
                acquiredTrait: "Dynastic Pillar",
                responseFeedMessage: "You sent a proud postcard home. Knowing your family thrives gives you unbreakable fighting spirit (+15 Morale, Trait: 'Dynastic Pillar')."
            ));

            letter.responseOptions.Add(new MailResponseOption(
                "gratitude_keep_silent",
                "Keep Letter in Chest Pocket",
                "Fold the letter neatly and keep it close to your heart beneath your combat tunic for good luck.",
                remittanceCost: 0,
                moraleModifier: 10f,
                acquiredTrait: null,
                responseFeedMessage: "You tucked your family's thank-you letter into your tunic pocket. It feels like a talisman of protection (+10 Morale)."
            ));

            return letter;
        }
    }
}
