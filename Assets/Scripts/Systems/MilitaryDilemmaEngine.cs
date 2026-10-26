using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Evaluates and triggers adult military, wartime crisis, and civilian life dilemmas.
    /// Provides 25+ high-stakes branching choices every semester to keep gameplay gripping,
    /// dynamic, and full of permanent story traits and military medals.
    /// </summary>
    public static class MilitaryDilemmaEngine
    {
        public static void EvaluateAdultSemesterDilemma(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired || soldier.isYouthPhase) return;

            // 1. If currently deployed in active combat, roll Frontline Crisis Dilemma
            if (soldier.isInCombatDeployment && !soldier.isCivilian)
            {
                // 70% chance of frontline crisis event during wartime deployment
                if (UnityEngine.Random.value < 0.70f)
                {
                    TriggerFrontlineWartimeDilemma(soldier, gm);
                    return;
                }
            }
            // 2. If stationed at garrison / military base
            else if (!soldier.isCivilian)
            {
                // 50% chance of garrison / base politics dilemma
                if (UnityEngine.Random.value < 0.50f)
                {
                    TriggerGarrisonPoliticsDilemma(soldier, gm);
                    return;
                }
            }
            // 3. If in civilian track
            else
            {
                // 40% chance of civilian career dilemma
                if (UnityEngine.Random.value < 0.40f)
                {
                    TriggerCivilianLifeDilemma(soldier, gm);
                    return;
                }
            }
        }

        // =========================================================================
        // FRONTLINE WARTIME CRISIS DILEMMAS (15 SCENARIOS)
        // =========================================================================

        private static void TriggerFrontlineWartimeDilemma(SoldierProfile s, GameManager gm)
        {
            int roll = UnityEngine.Random.Range(1, 16);

            switch (roll)
            {
                case 1:
                    // Trench Artillery Barrage & Forward Pillbox
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "FRONTLINE COMBAT",
                        "Sector 7 Mortar Barrage",
                        "Enemy heavy mortar shells crash into your trench line! An enemy reinforced pillbox is pinning down your entire squad with heavy plasma fire.",
                        "Charge the pillbox through crossfire! (Combat/Fitness check)",
                        "Drag wounded Sergeant Miller into a crater (+Comrade, +Respect)",
                        "Call danger-close friendly artillery (Intellect check)",
                        () =>
                        {
                            float check = (s.combatSkill * 0.6f) + (s.fitness * 0.4f);
                            if (check >= 45f)
                            {
                                s.combatSkill = Mathf.Min(100f, s.combatSkill + 6f);
                                s.combatMeritPoints += 450;
                                s.AddMedal("Iron Vanguard Cross");
                                s.AddTrait("Decorated Hero");
                                GameEvents.OnFeedEventLogged?.Invoke("HEROISM",
                                    "<color=#00FFAA><b>[PILLBOX BREACHED]</b></color> You sprinted through heavy plasma fire, threw two grenades into the firing slit, and silenced the enemy bunker single-handedly! Earned <b>Iron Vanguard Cross</b> (+450 CMP, +6 Combat Skill).");
                            }
                            else
                            {
                                s.health = Mathf.Max(20f, s.health - 35f);
                                s.AddMedal("Imperial Wound Star");
                                s.AddTrait("Scarred Veteran");
                                GameEvents.OnFeedEventLogged?.Invoke("CASUALTY",
                                    "<color=#FF4444>[SHRAPNEL CASUALTY]</color> Shrapnel caught you in the torso as you stormed the trench! Medics dragged you back. Awarded <b>Imperial Wound Star</b>.");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 25f);
                            s.meritRating = Mathf.Min(100f, s.meritRating + 15f);
                            s.AddTrait("Sworn Comrade: Miller");
                            GameEvents.OnFeedEventLogged?.Invoke("BROTHERHOOD",
                                "<color=#00D4FF><b>[COMRADE RESCUED]</b></color> Under deafening bombardments, you dragged Sergeant Miller to safety and bound his arterial bleeding. Miller sworn his life to you (+25 Squad Respect, +15 Merit).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            if (s.intellect >= 55f)
                            {
                                s.meritRating = Mathf.Min(100f, s.meritRating + 25f);
                                s.combatMeritPoints += 300;
                                GameEvents.OnFeedEventLogged?.Invoke("TACTICS",
                                    "<color=#FFD700><b>[PRECISION BOMBARDMENT]</b></color> You calculated the target azimuth perfectly over the radio. A direct 155mm howitzer strike obliterated the bunker! (+25 Merit, +300 CMP).");
                            }
                            else
                            {
                                s.health = Mathf.Max(30f, s.health - 25f);
                                s.squadRespect = Mathf.Max(10f, s.squadRespect - 15f);
                                GameEvents.OnFeedEventLogged?.Invoke("BLUNDER",
                                    "<color=#FF6600>[DANGER CLOSE CONCUSSION]</color> The coordinates were off by 40 meters. Shrapnel concussed your own trench (-25 Health, -15 Squad Respect).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 2:
                    // The Belltower Sniper
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "FRONTLINE CRISIS",
                        "The Clocktower Sniper",
                        "An enemy long-range kinetic sniper stationed in a ruined cathedral spire has already eliminated two platoon scouts. No one can move.",
                        "Flank solo through the minefield (Fitness check)",
                        "Deploy smoke screen sprint (Safe squad advance)",
                        "Order fresh recruits to draw sniper fire (Ruthless officer move)",
                        () =>
                        {
                            if (s.fitness >= 55f)
                            {
                                s.combatSkill = Mathf.Min(100f, s.combatSkill + 8f);
                                s.AddMedal("Marksman Commendation");
                                s.AddTrait("Cold-Blooded Marksman");
                                GameEvents.OnFeedEventLogged?.Invoke("SNIPER",
                                    "<color=#00FFAA><b>[SNIPER ELIMINATED]</b></color> You navigated the mine crater quietly, scaled the cathedral ladder, and took down the enemy marksman! (+8 Combat Skill, Marksman Commendation).");
                            }
                            else
                            {
                                s.health = Mathf.Max(25f, s.health - 30f);
                                s.AddTrait("One-Eyed Veteran");
                                GameEvents.OnFeedEventLogged?.Invoke("WOUNDED",
                                    "<color=#FF4444>[SNIPER GRAZE]</color> A high-velocity round grazed your cheek and cost you an eye, but you survived! Gained trait: <b>[One-Eyed Veteran]</b>.");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.meritRating = Mathf.Min(100f, s.meritRating + 10f);
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 8f);
                            GameEvents.OnFeedEventLogged?.Invoke("SMOKE",
                                "<color=#00D4FF>[SMOKE SCREEN ADVANCE]</color> You laid down three dense phosphorus smoke canisters, allowing your squad to advance into trenches without casualties (+10 Merit).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.squadRespect = Mathf.Max(5f, s.squadRespect - 35f);
                            s.coFavor = Mathf.Min(100f, s.coFavor + 10f);
                            s.AddTrait("Ruthless Commander");
                            GameEvents.OnFeedEventLogged?.Invoke("RUTHLESS",
                                "<color=#AA4444>[SACRIFICE RECRUITS]</color> You ordered fresh recruits into the clearing to bait the sniper while you slipped past. The squad now views you with cold terror (-35 Squad Respect, [Ruthless Commander]).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 3:
                    // Aether Gas Canister Strike
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "CHEMICAL WARFARE",
                        "Aether Gas Influx",
                        "Corrosive green Aether gas shells rupture across your trench! Your squadmate's gas mask filter cracked during the explosion.",
                        "Hand your own mask to the squadmate (Heroic sacrifice)",
                        "Seal yourself in the deep bunker airtight",
                        "Fashion emergency charcoal filter (Intellect check)",
                        () =>
                        {
                            s.health = Mathf.Max(20f, s.health - 25f);
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 35f);
                            s.AddTrait("Selfless Hero");
                            s.AddMedal("Valerian Merit Ribbon");
                            GameEvents.OnFeedEventLogged?.Invoke("HEROIC",
                                "<color=#00FFAA><b>[SELFLESS ACT]</b></color> You gave your mask to your comrade! You suffered corrosive smoke inhalation, but saved his life. (+35 Squad Respect, [Selfless Hero]).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.meritRating += 5f;
                            GameEvents.OnFeedEventLogged?.Invoke("BUNKER",
                                "You secured the iron bunker hatch. The squad survived the gas cloud safely.");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            if (s.intellect >= 50f)
                            {
                                s.intellect = Mathf.Min(100f, s.intellect + 5f);
                                s.AddTrait("Field Resourceful");
                                GameEvents.OnFeedEventLogged?.Invoke("FILTERS",
                                    "<color=#FFD700><b>[CHARCOAL FILTER IMPROVISED]</b></color> You quickly fashioned an emergency charcoal filter from ordnance packs, safely weathering the toxic vapor! (+5 Intellect, [Field Resourceful]).");
                            }
                            else
                            {
                                s.health = Mathf.Max(15f, s.health - 35f);
                                GameEvents.OnFeedEventLogged?.Invoke("FAILED",
                                    "<color=#FF4444>[FILTRATION FAILED]</color> The improvised filter leaked. Corrosive gas burned your lungs (-35 Health).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 4:
                    // Suicidal Assault Order
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "COMMAND TENSION",
                        "The Suicidal Bayonet Charge",
                        "An aristocratic Major with no combat experience orders your platoon to charge headlong into an entrenched machine gun nest over open mud.",
                        "Lead the bayonet charge valiantly (High glory vs extreme death risk)",
                        "Dispute the order with tactical logic (Intellect check)",
                        "Delay until smoke mortar cover arrives",
                        () =>
                        {
                            float rollCasualty = UnityEngine.Random.value;
                            float casualtyChance = 0.25f;
                            if (s.hasSteelHelmet) casualtyChance *= 0.85f;
                            if (s.hasBallisticVest) casualtyChance *= 0.80f;

                            if (rollCasualty < casualtyChance && s.combatSkill < 55f)
                            {
                                if (s.fieldMedkitsCount > 0)
                                {
                                    s.fieldMedkitsCount--;
                                    s.health = 20f;
                                    s.AddMedal("Imperial Wound Star");
                                    GameEvents.OnFeedEventLogged?.Invoke("GEAR SAVED LIFE",
                                        $"<color=#00FFAA><b>[EMERGENCY MEDKIT CONSUMED]</b></color> High-explosive shrapnel pierced your chest during the bayonet charge! You injected your PX emergency trauma syrette (-1 Medkit). Survived with 20% Health!");
                                }
                                else if (s.hasBallisticVest && UnityEngine.Random.value < 0.40f)
                                {
                                    s.health = 25f;
                                    s.AddMedal("Imperial Wound Star");
                                    GameEvents.OnFeedEventLogged?.Invoke("ARMOR DEFLECTION",
                                        "<color=#00FFAA><b>[BALLISTIC VEST DEFLECTION]</b></color> A machine gun burst struck your chest, but your <b>Ballistic Flak Vest</b> absorbed the impact! Knocked down with fractured ribs, but alive!");
                                }
                                else
                                {
                                    s.isDead = true;
                                    s.deathReason = "Killed leading a suicidal bayonet charge ordered by an aristocratic Major";
                                    GameEvents.OnImperialExecutionTriggered?.Invoke(s.deathReason);
                                }
                            }
                            else
                            {
                                s.combatSkill = Mathf.Min(100f, s.combatSkill + 10f);
                                s.AddMedal("Imperial Star of Valor");
                                s.AddTrait("Fearless Charger");
                                GameEvents.OnFeedEventLogged?.Invoke("GLORY",
                                    "<color=#00FFAA><b>[AGAINST ALL ODDS]</b></color> You leaped over the sandbags, screamed a battle cry, and overran the machine gun nest! Awarded the <b>Imperial Star of Valor</b> (+10 Combat Skill)!");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            if (s.intellect >= 60f)
                            {
                                s.meritRating = Mathf.Min(100f, s.meritRating + 25f);
                                s.squadRespect = Mathf.Min(100f, s.squadRespect + 20f);
                                GameEvents.OnFeedEventLogged?.Invoke("TACTICAL VINDICATION",
                                    "<color=#00FFAA><b>[TACTICAL REVISION]</b></color> You demonstrated the firing arc on the map. The Major reconsidered and authorized a flanking sapper trench, saving dozens of men! (+25 Merit).");
                            }
                            else
                            {
                                s.meritRating = Mathf.Max(10f, s.meritRating - 15f);
                                s.coFavor = Mathf.Max(10f, s.coFavor - 20f);
                                s.AddTrait("Insubordination Mark");
                                GameEvents.OnFeedEventLogged?.Invoke("REPRIMAND",
                                    "<color=#FF6600>[INSUBORDINATION DEMERIT]</color> The Major screamed at you for questioning authority and docked your service merit (-15 Merit, [Insubordination Mark]).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.coFavor = Mathf.Max(20f, s.coFavor - 8f);
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("PRUDENCE",
                                "You timed the advance to coincide with the battalion's smoke battery. The squad crossed safely (+10 Squad Respect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 5:
                    // Captured Enemy Cipher Officer
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "INTELLIGENCE PRIZE",
                        "Captured Enemy Cipher Courier",
                        "Your squad ambushed an enemy staff car and captured an Iron Federation cryptographer carrying sealed tactical cipher codebooks.",
                        "Interrogate and decrypt the cipher yourself (Intellect check)",
                        "Turn him over immediately to Imperial Military Intelligence (+Merit)",
                        "Take his $1,500 gold bribe to let him slip away in the fog",
                        () =>
                        {
                            if (s.intellect >= 65f)
                            {
                                s.promotionPoints += 60;
                                s.combatMeritPoints += 600;
                                s.AddMedal("Order of the Imperial Falcon");
                                GameEvents.OnFeedEventLogged?.Invoke("CIPHER",
                                    "<color=#FFD700><b>[CIPHER BROKEN]</b></color> You deciphered the enemy division's radio codes! High Command routed an entire enemy counter-attack. Awarded <b>Order of the Imperial Falcon</b> (+60 Promotion Points, +600 CMP)!");
                            }
                            else
                            {
                                s.meritRating += 10f;
                                GameEvents.OnFeedEventLogged?.Invoke("CIPHER", "You extracted partial frequencies and forwarded them to staff command (+10 Merit).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.meritRating = Mathf.Min(100f, s.meritRating + 25f);
                            s.coFavor = Mathf.Min(100f, s.coFavor + 20f);
                            s.AddTrait("Disciplined Soldier");
                            GameEvents.OnFeedEventLogged?.Invoke("INTELLIGENCE",
                                "<color=#00FFAA>[BY THE BOOK]</color> You escorted the prisoner to Intelligence headquarters. Staff officers commended your professional discipline (+25 Merit, +20 CO Favor).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.cashBalance += 1500;
                            s.AddTrait("Corrupt");
                            GameEvents.OnFeedEventLogged?.Invoke("BRIBE",
                                "<color=#FFD700><b>[BRIBED IN BLOOD]</b></color> You pocketed the courier's heavy pouch of gold sovereigns (<b>+$1,500 Cash</b>) and reported he vanished in the mist. Gained trait: <b>[Corrupt]</b>.");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 6:
                    // Downed Imperial Valkyrie Gunship
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "VALKYRIE DOWN",
                        "Crash in No-Man's-Land",
                        "A twin-rotor Imperial Valkyrie gunship was shot down 200 meters into no-man's-land! The pilots are trapped beneath wreckage while enemy scouts close in.",
                        "Volunteer a rescue team into the crossfire (Heroic rescue)",
                        "Lay down continuous heavy machine gun suppression from trench",
                        "Hold trench line; declare it too suicidal",
                        () =>
                        {
                            float check = (s.fitness * 0.5f) + (s.combatSkill * 0.5f);
                            if (check >= 50f)
                            {
                                s.combatSkill = Mathf.Min(100f, s.combatSkill + 5f);
                                s.combatMeritPoints += 500;
                                s.cashBalance += 800; // Noble pilot's family reward!
                                s.AddMedal("Hero of the Trenches Medal");
                                GameEvents.OnFeedEventLogged?.Invoke("RESCUE",
                                    "<color=#00FFAA><b>[PILOT SAVED]</b></color> You cut the pilots free and carried them back under intense fire! One pilot is a Viscount's heir; his family sent you an <b>$800 reward</b>! (+500 CMP, Hero of the Trenches Medal).");
                            }
                            else
                            {
                                s.health = Mathf.Max(25f, s.health - 30f);
                                s.AddMedal("Imperial Wound Star");
                                GameEvents.OnFeedEventLogged?.Invoke("RESCUE WOUND",
                                    "<color=#FF6600>[WOUNDED IN CROSSFIRE]</color> You pulled one pilot to safety, but took shrapnel to your legs (-30 Health, Wound Star).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.meritRating = Mathf.Min(100f, s.meritRating + 12f);
                            s.combatSkill = Mathf.Min(100f, s.combatSkill + 4f);
                            GameEvents.OnFeedEventLogged?.Invoke("SUPPRESSION",
                                "Your continuous machine-gun fire kept enemy scouts at bay until an armored crawler arrived (+12 Merit, +4 Combat).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.squadRespect = Mathf.Max(20f, s.squadRespect - 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("HELD",
                                "You held your defensive positions. The gunship cooked off in flames (-10 Squad Respect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 7:
                    // Classified Enemy Command Bunker
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "ENEMY COMMAND VAULT",
                        "The Breached Command Bunker",
                        "An artillery shell breached an uncharted enemy command bunker beneath the trench line. An intact cryptographic cipher machine and strategic map table lie inside.",
                        "Study the encryption codes & maps personally (+Readiness, +Intellect)",
                        "Secure and turn over the cipher machine to Military Intelligence (+Merit, +CMP)",
                        "Salvage precision components to black market contacts (+$2,000 Cash)",
                        () =>
                        {
                            s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 25f);
                            s.intellect = Mathf.Min(100f, s.intellect + 15f);
                            s.AddTrait("Field Intelligence Specialist");
                            GameEvents.OnFeedEventLogged?.Invoke("INTELLIGENCE",
                                "<color=#FFD700><b>[CIPHER MASTERED]</b></color> You decoded enemy frequency lists and mapped upcoming counter-offensive routes! (+25 Readiness, +15 Intellect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.combatMeritPoints += 750;
                            s.promotionPoints += 45;
                            s.coFavor = Mathf.Min(100f, s.coFavor + 20f);
                            s.AddTrait("Imperial Loyalist");
                            GameEvents.OnFeedEventLogged?.Invoke("SUBMITTED",
                                "<color=#00FFAA><b>[INTELLIGENCE COMMENDATION]</b></color> You delivered the intact cipher machine to Regimental Intelligence. Staff officers praised your loyalty (+750 CMP, +45 Promotion Points)!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.cashBalance += 2000;
                            s.AddTrait("Black Marketeer");
                            GameEvents.OnFeedEventLogged?.Invoke("SMUGGLED",
                                "<color=#FFD700><b>[BLACK MARKET GOLD]</b></color> You chipped off glowing crystal fragments and sold them through trench smugglers (<b>+$2,000 Cash</b>, [Black Marketeer])!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 8:
                    // Night Trench Infiltration Raid
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "SPECIAL OPERATION",
                        "Night Infiltration Sapper Raid",
                        "Battalion command seeks volunteers for a midnight trench-knife raid to silence an enemy heavy plasma howitzer.",
                        "Lead the infiltration strike team (High risk, massive glory)",
                        "Serve as the rearguard sniper covering the retreat",
                        "Decline hazardous duty and remain on standard guard",
                        () =>
                        {
                            float rollCasualty = UnityEngine.Random.value;
                            float casualtyChance = 0.20f;
                            if (s.hasSteelHelmet) casualtyChance *= 0.85f;
                            if (s.hasBallisticVest) casualtyChance *= 0.80f;

                            if (rollCasualty < casualtyChance && s.combatSkill < 45f)
                            {
                                if (s.fieldMedkitsCount > 0)
                                {
                                    s.fieldMedkitsCount--;
                                    s.health = 20f;
                                    s.AddMedal("Imperial Wound Star");
                                    GameEvents.OnFeedEventLogged?.Invoke("GEAR SAVED LIFE",
                                        $"<color=#00FFAA><b>[EMERGENCY MEDKIT CONSUMED]</b></color> You were shot during exfiltration! You quickly jammed your emergency PX trauma kit into the wound (-1 Medkit). Survived with 20% Health!");
                                }
                                else if (s.hasSteelHelmet && UnityEngine.Random.value < 0.35f)
                                {
                                    s.health = 22f;
                                    s.AddMedal("Imperial Wound Star");
                                    GameEvents.OnFeedEventLogged?.Invoke("ARMOR DEFLECTION",
                                        "<color=#00FFAA><b>[STEEL HELMET DEFLECTION]</b></color> A sniper round grazed your <b>M1 Steel Helmet</b>, ringing your skull and leaving a deep dent, but sparing your life!");
                                }
                                else
                                {
                                    s.isDead = true;
                                    s.deathReason = "Killed in Action during a nighttime trench infiltration raid";
                                    GameEvents.OnImperialExecutionTriggered?.Invoke(s.deathReason);
                                }
                            }
                            else
                            {
                                s.combatSkill = Mathf.Min(100f, s.combatSkill + 9f);
                                s.combatMeritPoints += 550;
                                s.AddMedal("Iron Vanguard Cross");
                                s.AddTrait("Silent Stalker");
                                GameEvents.OnFeedEventLogged?.Invoke("RAID SUCCESS",
                                    "<color=#00FFAA><b>[HOWITZER DESTROYED]</b></color> You slipped into the enemy trench, planted thermal demolition charges on the howitzer, and exfiltrated undetected! Awarded <b>Iron Vanguard Cross</b> (+9 Combat Skill, +550 CMP).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.combatSkill = Mathf.Min(100f, s.combatSkill + 4f);
                            s.meritRating = Mathf.Min(100f, s.meritRating + 12f);
                            GameEvents.OnFeedEventLogged?.Invoke("REARGUARD",
                                "From your elevated hide, you eliminated three enemy pursuers as the raid team withdrew safely (+12 Merit, +4 Combat).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.meritRating = Mathf.Max(10f, s.meritRating - 5f);
                            GameEvents.OnFeedEventLogged?.Invoke("HELD", "You remained on standard perimeter duty.");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 9:
                    // The Terrified Deserter
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "MILITARY JUSTICE",
                        "The Fleeing Recruit",
                        "During night sentry watch, you catch a shell-shocked 17-year-old recruit trying to cut the barbed wire to flee toward civilian territory.",
                        "Arrest and report him to Military Police for Court-Martial",
                        "Drag him back to trench and keep it secret (Brotherhood)",
                        "Look the other way and let him escape into the dark",
                        () =>
                        {
                            s.coFavor = Mathf.Min(100f, s.coFavor + 20f);
                            s.squadRespect = Mathf.Max(10f, s.squadRespect - 25f);
                            s.AddTrait("Cold Enforcer");
                            GameEvents.OnFeedEventLogged?.Invoke("COURT MARTIAL",
                                "<color=#AA4444>[MILITARY DISCIPLINE]</color> You handed the recruit to the Provost Marshal. He was executed at dawn. The officers commend your firmness, but the squad resents you (-25 Squad Respect, [Cold Enforcer]).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 30f);
                            s.AddTrait("Trench Brother");
                            GameEvents.OnFeedEventLogged?.Invoke("MERCY",
                                "<color=#00FFAA><b>[TRENCH MERCY]</b></color> You pulled the boy back, warmed him with hot ration tea, and told him to hold the line with you. He pledged eternal loyalty to you (+30 Squad Respect, [Trench Brother]).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.happiness = Mathf.Min(100f, s.happiness + 15f);
                            GameEvents.OnFeedEventLogged?.Invoke("DESERTED",
                                "You pretended your rifle jammed and let the boy disappear into the dark forest.");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 10:
                    // Tank Wave Breach
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "ARMORED TERROR",
                        "Enemy Armored Breakthrough",
                        "Two heavy enemy crawling tanks rupture through the concrete tank-traps, crushing bunkers and rolling toward your battalion command post!",
                        "Charge the lead tank with magnetic aether-mines (Valor)",
                        "Organize defensive retreat to fallback trenches",
                        "Man the stationary anti-tank cannon under direct fire",
                        () =>
                        {
                            float check = (s.combatSkill * 0.6f) + (s.fitness * 0.4f);
                            if (check >= 50f)
                            {
                                s.combatMeritPoints += 800;
                                s.combatSkill = Mathf.Min(100f, s.combatSkill + 10f);
                                s.AddMedal("Imperial Star of Valor");
                                s.AddTrait("Tank Hunter");
                                GameEvents.OnFeedEventLogged?.Invoke("TANK DESTROYED",
                                    "<color=#00FFAA><b>[TANK DESTROYED]</b></color> You sprinted into the tank's blind spot, placed the magnetic mine on its engine manifold, and blew the turret clean off! Awarded <b>Imperial Star of Valor</b> (+800 CMP, +10 Combat Skill)!");
                            }
                            else
                            {
                                s.health = Mathf.Max(20f, s.health - 40f);
                                s.AddMedal("Imperial Wound Star");
                                GameEvents.OnFeedEventLogged?.Invoke("BLAST",
                                    "<color=#FF4444>[CONCUSSION BLAST]</color> The tank was damaged, but its machine gun burst tore into your armor (-40 Health, Wound Star).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 15f);
                            s.meritRating = Mathf.Min(100f, s.meritRating + 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("RETREAT",
                                "You rallied the squad and led an orderly fallback to the secondary trench line, preserving combat strength (+15 Squad Respect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.combatSkill = Mathf.Min(100f, s.combatSkill + 6f);
                            s.combatMeritPoints += 400;
                            GameEvents.OnFeedEventLogged?.Invoke("CANNON",
                                "You loaded 75mm armor-piercing shells and disabled the lead tank's tracks (+400 CMP, +6 Combat).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 11:
                    // Frontline Rotation & Voluntary Tour Extension
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "TOUR ROTATION",
                        "Relief Order & Voluntary Tour Extension",
                        "Your battalion's scheduled 6-month frontline combat rotation has ended. Relief troops arrive at the trenches. Command asks if you want to rotate to base or volunteer to stay.",
                        "Volunteer for Extended Combat Tour (Stay on battlefield > 6 mo)",
                        "Accept rotation to Garrison Base (Rest & refit)",
                        "Apply for War Academy Officer Commission (Direct O-1)",
                        () =>
                        {
                            s.isVoluntaryExtendedTour = true;
                            s.isInCombatDeployment = true;
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 20f);
                            s.coFavor = Mathf.Min(100f, s.coFavor + 20f);
                            s.AddTrait("Frontline Stalwart");
                            GameEvents.OnFeedEventLogged?.Invoke("EXTENDED TOUR",
                                "<color=#FF4444><b>[VOLUNTEERED FOR EXTENDED TOUR]</b></color> You chose to stay on the battlefield! " +
                                "Your comrades cheered your resolve (+20 Respect, +20 CO Favor). If you remain deployed past 1 year, your total pay will increase by <b>+10%</b>!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.isVoluntaryExtendedTour = false;
                            s.health = Mathf.Min(100f, s.health + 15f);
                            GameEvents.OnFeedEventLogged?.Invoke("ROTATION",
                                "<color=#00FFAA><b>[GARRISON ROTATION ACCEPTED]</b></color> You packed your kit and rotated back to Fort Valerius for warm barracks and physical recovery (+15 Health).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            if (gm != null)
                            {
                                gm.ApplyForOfficerAcademy();
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                default:
                    // Standard Battlefield Trial
                    s.combatSkill = Mathf.Min(100f, s.combatSkill + 3f);
                    s.combatMeritPoints += 250;
                    GameEvents.OnFeedEventLogged?.Invoke("COMBAT",
                        "<color=#00FFAA>[TRENCH ENGAGEMENT]</color> You repelled an enemy probing assault with disciplined rifle fire (+3 Combat Skill, +250 CMP).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    break;
            }
        }

        // =========================================================================
        // GARRISON & BASE POLITICS DILEMMAS (10 SCENARIOS)
        // =========================================================================

        private static void TriggerGarrisonPoliticsDilemma(SoldierProfile s, GameManager gm)
        {
            // 50% chance to draw from the 15 high-stakes Economic & Military Graft Dilemmas
            if (UnityEngine.Random.value < 0.50f)
            {
                if (EconomicDilemmaDeck.TryTriggerEconomicDilemma(s, () => GameEvents.OnSoldierStatsUpdated?.Invoke(s)))
                {
                    return;
                }
            }

            int roll = UnityEngine.Random.Range(1, 11);

            switch (roll)
            {
                case 1:
                    // Barracks Contraband Ring
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "GARRISON DRAMA",
                        "The Black Market Supply Ring",
                        "You discover your squadmate has been diverting military-grade super-soldier nanite serums and selling them to civilians for massive profit.",
                        "Report the smuggling ring to the Provost Marshal (+CO Favor, Squad Snitch)",
                        "Buy black market super-soldier serums at half price ($150)",
                        "Demand a 25% cut of the profits to keep quiet (+$400 Cash)",
                        () =>
                        {
                            s.coFavor = Mathf.Min(100f, s.coFavor + 25f);
                            s.squadRespect = Mathf.Max(5f, s.squadRespect - 30f);
                            s.AddTrait("Military Police Informant");
                            GameEvents.OnFeedEventLogged?.Invoke("REPORTED",
                                "<color=#00D4FF>[INTEGRITY]</color> You turned over the contraband. Battalion officers praised your fidelity, but the squad brands you a snitch (+25 CO Favor, -30 Squad Respect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            if (s.cashBalance >= 150)
                            {
                                s.cashBalance -= 150;
                                s.tierProgressPercent = Mathf.Min(100f, s.tierProgressPercent + 20f);
                                s.fitness = Mathf.Min(100f, s.fitness + 5f);
                                GameEvents.OnFeedEventLogged?.Invoke("BOUGHT",
                                    "<color=#FFD700>[BLACK MARKET SERUM]</color> You injected the high-potency nanite serum (-$150). Your cyberware integration surged (+20% Saturation, +5 Fitness)!");
                            }
                            else
                            {
                                GameEvents.OnFeedEventLogged?.Invoke("POVERTY", "You lacked $150 to buy the serum.");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.cashBalance += 400;
                            s.AddTrait("Corrupt");
                            GameEvents.OnFeedEventLogged?.Invoke("EXTORTION",
                                "<color=#FFD700><b>[BLACK MARKET CUT]</b></color> You took your $400 cut to look the other way. Gained trait: <b>[Corrupt]</b>.");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 2:
                    // Aristocratic Officer's Insult
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "MILITARY PRIDE",
                        "The Arrogant Noble Lieutenant",
                        "In the garrison mess hall, an aristocratic Lieutenant spills wine on your uniform and mocks your humble commoner origins before other enlisted men.",
                        "Challenge him to a sanctioned military saber/boxing duel (Combat check)",
                        "Maintain rigid parade rest and report to the Inspector General (+Discipline)",
                        "Swallow the insult quietly and focus on promotion",
                        () =>
                        {
                            if (s.combatSkill >= 50f)
                            {
                                s.squadRespect = Mathf.Min(100f, s.squadRespect + 35f);
                                s.meritRating = Mathf.Min(100f, s.meritRating + 10f);
                                s.AddTrait("Enlisted Champion");
                                GameEvents.OnFeedEventLogged?.Invoke("DUEL VICTORY",
                                    "<color=#00FFAA><b>[DUEL WON]</b></color> In the gym ring, you knocked the arrogant Lieutenant flat on his back! The enlisted troops cheered deafeningly (+35 Squad Respect, [Enlisted Champion]).");
                            }
                            else
                            {
                                s.health = Mathf.Max(30f, s.health - 20f);
                                s.squadRespect = Mathf.Max(20f, s.squadRespect - 10f);
                                GameEvents.OnFeedEventLogged?.Invoke("DUEL LOSS",
                                    "The noble's fencing instructor proved too skilled; you took several painful bruises (-20 Health).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.meritRating = Mathf.Min(100f, s.meritRating + 15f);
                            s.AddTrait("Iron Discipline");
                            GameEvents.OnFeedEventLogged?.Invoke("DISCIPLINE",
                                "You stood at attention without flinching. The Inspector General reprimanded the Lieutenant for unbecoming conduct (+15 Merit, [Iron Discipline]).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.happiness = Mathf.Max(10f, s.happiness - 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("STOIC", "You held your tongue and cleaned your uniform.");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 3:
                    // Promotion Board Oral Exam
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "PROMOTION BOARD",
                        "The Board President's Oral Exam",
                        "You stand before the Battalion Promotion Board. The stern Colonel asks: 'Soldier, what is the core virtue of an Imperial warrior?'",
                        "'Absolute tactical mastery and cold efficiency!' (Tactical intellect)",
                        "'Unquestioning loyalty to the Emperor and the Flag!' (Loyalty)",
                        "'Brothers in the trench - never leave a man behind!' (Brotherhood)",
                        () =>
                        {
                            s.promotionPoints += 45;
                            s.intellect = Mathf.Min(100f, s.intellect + 4f);
                            GameEvents.OnFeedEventLogged?.Invoke("BOARD",
                                "<color=#00FFAA><b>[BOARD COMMENDATION]</b></color> The Colonel nodded approvingly at your strategic intellect (+45 Promotion Points, +4 Intellect)!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.promotionPoints += 40;
                            s.coFavor = Mathf.Min(100f, s.coFavor + 15f);
                            GameEvents.OnFeedEventLogged?.Invoke("BOARD",
                                "<color=#FFD700><b>[PATRIOTIC PRAISE]</b></color> Your stirring loyalty resonated with the Imperial Commission (+40 Promotion Points, +15 CO Favor)!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.promotionPoints += 35;
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 20f);
                            GameEvents.OnFeedEventLogged?.Invoke("BOARD",
                                "<color=#00D4FF><b>[WARRIOR ETHOS]</b></color> The Command Sergeant Major grinned at your frontline brotherhood (+35 Promotion Points, +20 Squad Respect)!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 4:
                    // High-Stakes Motor Pool Gambling
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "BASE PASTIME",
                        "Motor Pool Dice Game",
                        "Veterans are rolling bones around an ammo crate with hundreds of dollars on the line. The dice are passed to you.",
                        "Stake $100 on the high roll (Gamble)",
                        "Decline and send the money home to family instead",
                        "Act as the house bookmaker taking a 10% rake",
                        () =>
                        {
                            if (s.cashBalance >= 100)
                            {
                                s.cashBalance -= 100;
                                if (UnityEngine.Random.value < 0.50f)
                                {
                                    s.cashBalance += 350;
                                    s.happiness = Mathf.Min(100f, s.happiness + 15f);
                                    GameEvents.OnFeedEventLogged?.Invoke("DICE WIN",
                                        "<color=#FFD700><b>[LUCKY SEVEN!]</b></color> You rolled three matching sixes and swept the table! Won <b>+$350 Cash</b>!");
                                }
                                else
                                {
                                    s.happiness = Mathf.Max(10f, s.happiness - 10f);
                                    GameEvents.OnFeedEventLogged?.Invoke("DICE LOSS", "Snake eyes! You lost your $100 stake.");
                                }
                            }
                            else
                            {
                                GameEvents.OnFeedEventLogged?.Invoke("POVERTY", "You lack $100 to gamble.");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            gm.SendMoneyToFamily(100);
                        },
                        () =>
                        {
                            s.cashBalance += 90;
                            s.AddTrait("Shrewd Operator");
                            GameEvents.OnFeedEventLogged?.Invoke("BOOKIE",
                                "<color=#00FFAA>[BOOKMAKER RAKE]</color> You managed the bets without risking a dime and pocketed <b>+$90 Cash</b>!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 5:
                    // War Correspondent Interview
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "IMPERIAL MEDIA",
                        "The Gazette Interview",
                        "An Imperial War Correspondent with a camera crew wants to feature your frontline company on national broadsheets.",
                        "Deliver a soaring, patriotic speech for national morale",
                        "Speak honestly about mud, blood, and fallen friends",
                        "Decline the spotlight and return to weapon maintenance",
                        () =>
                        {
                            s.meritRating = Mathf.Min(100f, s.meritRating + 20f);
                            s.AddTrait("Patriotic Icon");
                            GameEvents.OnFeedEventLogged?.Invoke("NEWSPAPER",
                                "<color=#FFD700><b>[FRONT PAGE HERO]</b></color> Your photo and quote appeared on broadsheets across the capital! (+20 Merit, [Patriotic Icon]).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.squadRespect = Mathf.Min(100f, s.squadRespect + 30f);
                            s.coFavor = Mathf.Max(20f, s.coFavor - 10f);
                            s.AddTrait("Truth Teller");
                            GameEvents.OnFeedEventLogged?.Invoke("TRUTH",
                                "<color=#00D4FF>[VOICE OF THE GRUNTS]</color> Your raw honesty struck a chord with enlisted soldiers nationwide (+30 Squad Respect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.intellect = Mathf.Min(100f, s.intellect + 3f);
                            GameEvents.OnFeedEventLogged?.Invoke("HUMBLE", "You declined the vanity and oiled your rifle (+3 Intellect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                default:
                    // General Base Life event
                    s.fitness = Mathf.Min(100f, s.fitness + 3f);
                    s.promotionPoints += 15;
                    GameEvents.OnFeedEventLogged?.Invoke("GARRISON",
                        "<color=#00FFAA>[BASE DRILLS]</color> You completed seasonal battalion maneuvers and weapon inspections (+3 Fitness, +15 Promotion Points).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    break;
            }
        }

        // =========================================================================
        // CIVILIAN CAREER DILEMMAS (FOR CIVILIAN TRACK PLAYERS)
        // =========================================================================

        private static void TriggerCivilianLifeDilemma(SoldierProfile s, GameManager gm)
        {
            // 50% chance to draw from high-stakes Economic & Civilian Dilemmas
            if (UnityEngine.Random.value < 0.50f)
            {
                if (EconomicDilemmaDeck.TryTriggerEconomicDilemma(s, () => GameEvents.OnSoldierStatsUpdated?.Invoke(s)))
                {
                    return;
                }
            }

            int roll = UnityEngine.Random.Range(1, 4);

            switch (roll)
            {
                case 1:
                    // Municipal Guild Strike
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "CIVIL STRIFE",
                        "The Artisan Guild Strike",
                        "Municipal factory workers organize a strike against wartime wage caps. Guild delegates urge you to join the picket line.",
                        "Join the picket line for higher long-term pay (Risk arrest)",
                        "Cross the picket line and work overtime (+Double shift pay)",
                        "Mediate between guild stewards and the factory commissioner",
                        () =>
                        {
                            if (UnityEngine.Random.value < 0.70f)
                            {
                                s.civilianSalaryPerMonth = System.Math.Min(CivilAndMilitaryCareerEngine.MAX_NON_COLLEGE_PAY, s.civilianSalaryPerMonth + 40);
                                s.AddTrait("Union Hero");
                                GameEvents.OnFeedEventLogged?.Invoke("STRIKE WIN",
                                    "<color=#00FFAA><b>[WAGE INCREASE WON]</b></color> The guild won! Your monthly wage increased by <b>+$40/mo</b>!");
                            }
                            else
                            {
                                s.cashBalance = System.Math.Max(0, s.cashBalance - 150);
                                GameEvents.OnFeedEventLogged?.Invoke("ARREST",
                                    "<color=#FF4444>[FINES LEVIED]</color> Municipal constables broke the strike and fined you $150.");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.cashBalance += 200;
                            s.happiness = Mathf.Max(10f, s.happiness - 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("OVERTIME",
                                "<color=#FFD700>[OVERTIME CASH]</color> You worked 60 hours overtime while others struck (+$200 Cash).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            s.intellect = Mathf.Min(100f, s.intellect + 4f);
                            s.politicalInfluence = Mathf.Min(100f, s.politicalInfluence + 8f);
                            GameEvents.OnFeedEventLogged?.Invoke("MEDIATION",
                                "<color=#00FFAA>[CIVIL MEDIATOR]</color> Your diplomacy earned praise from city aldermen (+4 Intellect, +8 Influence).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                case 2:
                    // Imperial Municipal Tax Audit
                    GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                        "CIVIL BUREAUCRACY",
                        "The 50% Municipal Tax Audit",
                        "Imperial Tax Commissioners arrive to audit your books. You discover an accounting loophole that could shield $600 of income.",
                        "Declare all earnings truthfully and pay the full 50% tax",
                        "Exploit the tax loophole to pocket the extra $600 (Tax evasion risk)",
                        "Donate $200 to the Imperial War Bond Foundation for an exemption",
                        () =>
                        {
                            s.AddTrait("Lawful Citizen");
                            GameEvents.OnFeedEventLogged?.Invoke("TAX AUDIT",
                                "<color=#00FFAA>[TAX COMPLIANCE]</color> The tax commissioner stamped your ledger clean: <b>[HONORABLE TAXPAYER]</b>.");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            if (UnityEngine.Random.value < 0.70f)
                            {
                                s.cashBalance += 600;
                                s.AddTrait("Tax Dodger");
                                GameEvents.OnFeedEventLogged?.Invoke("TAX SAVINGS",
                                    "<color=#FFD700><b>[TAX LOOPHOLE EXPLOITED]</b></color> You shielded <b>+$600 Cash</b> from the municipal tax collectors!");
                            }
                            else
                            {
                                s.cashBalance = System.Math.Max(0, s.cashBalance - 800);
                                GameEvents.OnFeedEventLogged?.Invoke("TAX PENALTY",
                                    "<color=#FF4444>[AUDIT PENALTY]</color> The commissioner discovered the evasion and fined you $800.");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        },
                        () =>
                        {
                            if (s.cashBalance >= 200)
                            {
                                s.cashBalance -= 200;
                                s.politicalInfluence += 6f;
                                GameEvents.OnFeedEventLogged?.Invoke("WAR BOND",
                                    "<color=#00FFAA>[WAR BONDS PURCHASED]</color> Purchased $200 in War Bonds. Gained civil tax deduction (+6 Influence).");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                        }
                    );
                    break;

                default:
                    s.cashBalance += 80;
                    GameEvents.OnFeedEventLogged?.Invoke("CIVILIAN", "Completed seasonal commercial orders (+80 Cash).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    break;
            }
        }
    }
}
