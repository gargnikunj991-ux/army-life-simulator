using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    [Serializable]
    public class RomanticPartner
    {
        public string name;
        public string occupation;
        public int age;
        public float affection; // 0 to 100
        public string personalityTrait;
        public CourtshipStage stage;
        public string engagementRingType;
        public int semestersDating;

        public RomanticPartner(string name, string occupation, int age, float affection, string personalityTrait)
        {
            this.name = name;
            this.occupation = occupation;
            this.age = age;
            this.affection = Mathf.Clamp(affection, 0f, 100f);
            this.personalityTrait = personalityTrait;
            this.stage = CourtshipStage.CasualSweetheart;
            this.engagementRingType = "";
            this.semestersDating = 0;
        }
    }

    /// <summary>
    /// BitLife-style interactive Romance & Courtship Engine:
    /// - Meeting romantic partners (sweethearts, military nurses, civilian companions)
    /// - Interactive dating (Dinners, Drive-in Movies, Base Galas, Starlit Walks)
    /// - Ring buying (Brass, Silver, Gold, Diamond Solitaire)
    /// - Proposing marriage & wedding ceremonies (Courthouse vs Arch-of-Sabers Military Cathedral)
    /// - Having children and raising a family
    /// - The dramatic "Dear John" letter during long, grueling frontline combat deployments
    /// </summary>
    public static class RomanceAndCourtshipEngine
    {
        public static readonly string[] FEMALE_PARTNER_NAMES = new string[]
        {
            "Evelyn Reed", "Clara Oswald", "Sarah Jenkins", "Elena Vance", "Victoria Sterling",
            "Beatrice Hayes", "Diana Miller", "Margaret Cole", "Rose Montgomery", "Hannah Wright"
        };

        public static readonly string[] MALE_PARTNER_NAMES = new string[]
        {
            "Darek Stone", "Marcus Reed", "Julian Vance", "David Evans", "Thomas Sterling",
            "Arthur Cole", "Robert Hayes", "James Miller", "William Jenkins", "Edward Wright"
        };

        public static readonly string[] PARTNER_OCCUPATIONS = new string[]
        {
            "Frontline Flight Nurse", "PX Canteen Dispatcher", "Hometown Schoolteacher",
            "Field Hospital Medic", "Signal Corps Telegraphist", "University Scholar", "Quartermaster Clerk"
        };

        public static readonly string[] PARTNER_TRAITS = new string[]
        {
            "Devoted & Caring", "Spirited & Playful", "Quiet & Intellectual",
            "Fierce & Loyal", "Warm & Compassionate", "Ambitious & Elegant"
        };

        /// <summary>
        /// Generates a randomized prospective romantic partner.
        /// </summary>
        public static RomanticPartner GenerateProspectivePartner(SoldierProfile soldier)
        {
            if (soldier == null) return null;

            bool isFemaleSoldier = soldier.gender == "Female";
            string[] namePool = isFemaleSoldier ? MALE_PARTNER_NAMES : FEMALE_PARTNER_NAMES;
            string partnerName = namePool[UnityEngine.Random.Range(0, namePool.Length)];

            // If soldier has childhood crush Evelyn, prioritize her if single
            if (!soldier.isMarried && !string.IsNullOrEmpty(soldier.childhoodRomanceInterest) && UnityEngine.Random.value < 0.40f)
            {
                partnerName = soldier.childhoodRomanceInterest;
            }

            string occ = PARTNER_OCCUPATIONS[UnityEngine.Random.Range(0, PARTNER_OCCUPATIONS.Length)];
            string trait = PARTNER_TRAITS[UnityEngine.Random.Range(0, PARTNER_TRAITS.Length)];
            int partnerAge = Mathf.Max(18, Mathf.RoundToInt(soldier.age) + UnityEngine.Random.Range(-2, 3));
            float initialAffection = UnityEngine.Random.Range(45f, 75f);

            return new RomanticPartner(partnerName, occ, partnerAge, initialAffection, trait);
        }

        /// <summary>
        /// Takes a partner on a date with varied costs, affection gains, and morale boosts.
        /// </summary>
        public static bool TakeOnDate(SoldierProfile soldier, RomanticPartner partner, string dateType)
        {
            if (soldier == null || partner == null) return false;

            long cost = 0L;
            float affectionGain = 10f;
            float moraleGain = 8f;
            string narrative = "";

            switch (dateType)
            {
                case "Diner":
                    cost = 30L;
                    affectionGain = 14f;
                    moraleGain = 10f;
                    narrative = $"You shared milkshakes and warm apple pie with <b>{partner.name}</b> at the local diner. Laughter and warm conversation flowed easily!";
                    break;

                case "Cinema":
                    cost = 75L;
                    affectionGain = 20f;
                    moraleGain = 14f;
                    narrative = $"You took <b>{partner.name}</b> to the open-air drive-in cinema under the stars. Holding hands in the cool night breeze brought you much closer!";
                    break;

                case "MilitaryBall":
                    cost = 200L;
                    affectionGain = 32f;
                    moraleGain = 22f;
                    narrative = $"You escorted <b>{partner.name}</b> to the Regimental Officers' Spring Gala in full dress uniform. Dancing across the ballroom floor made you the envy of the garrison!";
                    break;

                case "StarlitWalk":
                default:
                    cost = 0L;
                    affectionGain = 10f;
                    moraleGain = 6f;
                    narrative = $"You took a quiet, peaceful stroll along the river bluffs with <b>{partner.name}</b>, talking of peacetime dreams away from the guns.";
                    break;
            }

            if (soldier.cashBalance < cost)
            {
                GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                    $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You do not have <b>${cost:N0}</b> for this date.");
                return false;
            }

            soldier.cashBalance -= cost;
            partner.affection = Mathf.Min(100f, partner.affection + affectionGain);
            soldier.happiness = Mathf.Min(100f, soldier.happiness + moraleGain);
            soldier.combatStress = Mathf.Max(0f, soldier.combatStress - (moraleGain * 0.5f));

            if (partner.affection >= 75f && partner.stage == CourtshipStage.CasualSweetheart)
            {
                partner.stage = CourtshipStage.SeriousCourtship;
                narrative += " <b>Your romance has evolved into a serious committed courtship!</b>";
            }

            GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                $"<color=#FF88FF><b>[ROMANTIC DATE]</b></color> {narrative} (+{affectionGain:F0}% Affection, +{moraleGain:F0}% Morale).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        /// <summary>
        /// Proposes marriage to the partner using a chosen engagement ring.
        /// </summary>
        public static bool ProposeMarriage(SoldierProfile soldier, RomanticPartner partner, string ringType, long ringCost)
        {
            if (soldier == null || partner == null) return false;

            if (soldier.cashBalance < ringCost)
            {
                GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                    $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You need <b>${ringCost:N0}</b> to purchase the {ringType} engagement ring.");
                return false;
            }

            soldier.cashBalance -= ringCost;
            partner.engagementRingType = ringType;

            // Acceptance probability based on affection & ring quality
            float acceptanceThreshold = 55f;
            if (ringCost >= 2000L) acceptanceThreshold -= 15f; // Diamond ring bonus
            else if (ringCost >= 700L) acceptanceThreshold -= 8f;

            if (partner.affection >= acceptanceThreshold)
            {
                partner.stage = CourtshipStage.Engaged;
                partner.affection = 100f;
                soldier.happiness = Mathf.Min(100f, soldier.happiness + 30f);
                soldier.combatStress = Mathf.Max(0f, soldier.combatStress - 20f);

                GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                    $"<color=#00FFAA><b>[ENGAGEMENT ACCEPTED • SHE SAID YES!]</b></color> You went down on one knee and presented the <b>{ringType}</b> to <b>{partner.name}</b>. " +
                    $"Tears of joy filled their eyes as they threw their arms around you and said YES! You are now officially engaged to be married (+30 Morale, 100% Affection)!");
                return true;
            }
            else
            {
                partner.affection = Mathf.Max(15f, partner.affection - 15f);
                GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                    $"<color=#FF6600><b>[PROPOSAL HESITATION]</b></color> <b>{partner.name}</b> gently held your hand: " +
                    $"<i>'It's too soon with the war raging on. Let's spend more time together first.'</i> You remain dating, but wedding plans are paused (-15% Affection).");
                return false;
            }
        }

        /// <summary>
        /// Conducts the wedding ceremony, turning the partner into your official military spouse.
        /// </summary>
        public static bool HoldWeddingCeremony(SoldierProfile soldier, RomanticPartner partner, string ceremonyType)
        {
            if (soldier == null || partner == null) return false;

            long cost = 150L;
            string ceremonyTitle = "Courthouse Registry";
            string narrative = "";

            if (ceremonyType == "CathedralMilitary")
            {
                cost = 1000L;
                ceremonyTitle = "Arch-of-Sabers Military Cathedral Wedding";
                narrative = $"In full dress uniform, you walked down the aisle under the traditional arched sabers of your fireteam comrades! Organ bells rang across the garrison.";
            }
            else if (ceremonyType == "CountryGarden")
            {
                cost = 450L;
                ceremonyTitle = "Country Garden Spring Wedding";
                narrative = $"Surrounded by wildflowers and smiling family, you exchanged vows under an ivory arbor in peaceful countryside bliss.";
            }
            else
            {
                cost = 150L;
                ceremonyTitle = "Simple Courthouse Marriage";
                narrative = $"You and {partner.name} signed the official registry before a justice of the peace and toasted with cheap sparkling cider in joyful simplicity.";
            }

            if (soldier.cashBalance < cost)
            {
                GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                    $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You lack <b>${cost:N0}</b> for the {ceremonyTitle}.");
                return false;
            }

            soldier.cashBalance -= cost;
            partner.stage = CourtshipStage.Married;
            soldier.isMarried = true;
            soldier.spouseName = partner.name;

            // Add or sync spouse in clan members list
            if (soldier.clanMembers == null) soldier.clanMembers = new List<ClanMember>();
            var existingSpouse = soldier.clanMembers.Find(m => m.relationType == ClanRelationType.Spouse);
            if (existingSpouse == null)
            {
                soldier.clanMembers.Add(new ClanMember(partner.name, ClanRelationType.Spouse, "Beloved Military Spouse", partner.age, 95f, soldier.nobleTitle, 0, partner.personalityTrait, 75f));
            }
            else
            {
                existingSpouse.name = partner.name;
                existingSpouse.age = partner.age;
                existingSpouse.relationship = 95f;
                existingSpouse.isAlive = true;
            }

            soldier.happiness = 100f;
            soldier.combatStress = Mathf.Max(0f, soldier.combatStress - 25f);
            soldier.AddTrait("Faithfully Married");

            GameEvents.OnFeedEventLogged?.Invoke("WEDDING",
                $"<color=#00FFAA><b>[JUST MARRIED • {ceremonyTitle.ToUpper()}]</b></color> {narrative} " +
                $"You and <b>{partner.name}</b> are now officially husband and wife! (100% Morale, 'Faithfully Married' trait acquired).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        /// <summary>
        /// Attempts to conceive a child with your spouse.
        /// </summary>
        public static bool TryForBaby(SoldierProfile soldier)
        {
            if (soldier == null || !soldier.isMarried)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    "<color=#FF6666>[MARRIAGE REQUIRED]</color> You must be married to start a family.");
                return false;
            }

            if (soldier.childHeir != null && !string.IsNullOrEmpty(soldier.childHeir.childName))
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<color=#00D4FF>[FAMILY NOTICE]</color> You already have an eldest child and dynastic heir, <b>{soldier.childHeir.childName}</b> (Age {soldier.childHeir.age:F0}).");
                return false;
            }

            // 75% conception success
            if (UnityEngine.Random.value < 0.75f)
            {
                soldier.HaveChild();
                soldier.happiness = Mathf.Min(100f, soldier.happiness + 25f);

                string cName = soldier.childHeir != null ? soldier.childHeir.childName : "Little Heir";
                string gender = soldier.childHeir != null ? soldier.childHeir.gender : "Child";

                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<color=#FF88FF><b>[BABY BORN • DYNASTIC LEGACY]</b></color> Joyous tears fill your home! Your spouse <b>{soldier.spouseName}</b> gave birth to a healthy baby {gender}, <b>{cName}</b>! " +
                    $"You now have an heir to inherit your military savings and war medals (+25 Morale)!");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    $"<color=#AAAAAA>[FAMILY PLANS]</color> You and {soldier.spouseName} spent intimate leave together, but are not expecting a baby this semester. Keep trying!");
                return false;
            }
        }

        /// <summary>
        /// Checks for the dramatic "Dear John" breakup letter during arduous overseas combat deployments.
        /// </summary>
        public static bool CheckForDearJohnLetter(SoldierProfile soldier, WarTheaterInfo theater)
        {
            if (soldier == null || theater == null) return false;
            if (!soldier.isMarried && string.IsNullOrEmpty(soldier.childhoodRomanceInterest)) return false;

            // Only occurs if deployed in hazardous combat theaters (danger level >= 3)
            if (theater.dangerLevel < 3) return false;

            // Rare dramatic roll: ~3% chance per high-danger deployment semester
            if (UnityEngine.Random.value > 0.035f) return false;

            string partnerName = soldier.isMarried ? soldier.spouseName : soldier.childhoodRomanceInterest;

            GameEvents.OnFeedEventLogged?.Invoke("MAIL CALL",
                $"<color=#FF4444><b>[THE DEAR JOHN LETTER]</b></color> Mail call arrived at the forward fire trench. Among the grease-stained ration cartons was a cold, formal envelope from <b>{partnerName}</b>.\n" +
                $"<i>\"The years away and the endless dread of the telegram boy have broken me. I met someone here in town who doesn't live with a rifle in hand. Forgive me...\"</i>");

            soldier.combatStress = Mathf.Min(100f, soldier.combatStress + 35f);
            soldier.happiness = Mathf.Max(5f, soldier.happiness - 40f);

            return true;
        }

        /// <summary>
        /// Resolves player reaction to the "Dear John" letter.
        /// </summary>
        public static void ResolveDearJohnChoice(SoldierProfile soldier, DearJohnChoice choice)
        {
            if (soldier == null) return;

            switch (choice)
            {
                case DearJohnChoice.TearItUpMoveForward:
                    soldier.AddTrait("Iron Heart");
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 8f);
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 15f);
                    soldier.isMarried = false;
                    GameEvents.OnFeedEventLogged?.Invoke("HEARTBREAK",
                        "<color=#FFD700><b>[IRON HEART]</b></color> You struck a match, watched the letter turn to ash in the trench mud, and strapped your helmet tight. " +
                        "Heartbreak hardened into unbreakable military resolve (+8 Combat Skill, 'Iron Heart' trait acquired).");
                    break;

                case DearJohnChoice.FocusOnMission:
                    soldier.discipline = Mathf.Min(100f, soldier.discipline + 12f);
                    soldier.isMarried = false;
                    GameEvents.OnFeedEventLogged?.Invoke("HEARTBREAK",
                        "<color=#00D4FF><b>[MISSION FIRST]</b></color> You buried the grief deep inside and focused solely on keeping your squad alive. " +
                        "Pain channeled into textbook tactical discipline (+12 Discipline).");
                    break;

                case DearJohnChoice.WriteDesperatePlea:
                default:
                    soldier.combatStress = Mathf.Min(100f, soldier.combatStress + 20f);
                    GameEvents.OnFeedEventLogged?.Invoke("HEARTBREAK",
                        "<color=#888888><b>[DESPERATE LETTER]</b></color> You penned an emotional plea under candlelight, but received no reply. " +
                        "The distance remained an unbridgeable gulf (+20 Stress).");
                    break;
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }
    }
}
