using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    [Serializable]
    public class NemesisProfile
    {
        public string name;
        public string title;
        public WarTheaterType theater;
        public string backstory;
        public int lethality;            // 60 to 95
        public string signatureWeapon;
        public long bounty;
        public bool isDefeated;

        public NemesisProfile(string name, string title, WarTheaterType theater, string backstory, int lethality, string weapon, long bounty)
        {
            this.name = name;
            this.title = title;
            this.theater = theater;
            this.backstory = backstory;
            this.lethality = lethality;
            this.signatureWeapon = weapon;
            this.bounty = bounty;
            this.isDefeated = false;
        }
    }

    /// <summary>
    /// Procedural Battlefield Nemesis & Rival Combatants Engine:
    /// Generates infamous enemy commanders, counter-snipers, and aces who haunt specific theaters of war.
    /// Clashes test soldier marksmanship, squad coordination, and tactical ingenuity.
    /// Defeating a rival awards unique war trophy relics, citations, and bounties.
    /// </summary>
    public static class BattlefieldNemesisEngine
    {
        private static readonly Dictionary<WarTheaterType, NemesisProfile> theaterNemesisRegistry = new Dictionary<WarTheaterType, NemesisProfile>()
        {
            {
                WarTheaterType.ArdenRidgeTrenchLines,
                new NemesisProfile(
                    "Major Koenig",
                    "The Iron Marksman of Arden",
                    WarTheaterType.ArdenRidgeTrenchLines,
                    "An infamous marksman who has eliminated 42 allied officers across the muddy trench line.",
                    82,
                    "Custom Scoped Kar98k",
                    600L)
            },
            {
                WarTheaterType.KaelenValleyUrbanZone,
                new NemesisProfile(
                    "The Ghost of Kaelen",
                    "Ghost Sniper of the Rubble",
                    WarTheaterType.KaelenValleyUrbanZone,
                    "A phantom shooter concealed inside hollowed cathedral belltowers zeroed on allied advance corridors.",
                    85,
                    "Suppressed Heavy Marksman Rifle",
                    750L)
            },
            {
                WarTheaterType.PortIronAmphibiousFront,
                new NemesisProfile(
                    "Captain Vorona",
                    "Iron Redoubt Commander",
                    WarTheaterType.PortIronAmphibiousFront,
                    "A veteran bunker garrison commander who has repelled three amphibious landing waves with interlocking machine-gun crossfire.",
                    78,
                    "Masterwork Trench Trench Shotgun & Field Optics",
                    550L)
            },
            {
                WarTheaterType.BadlandsSteppeRecon,
                new NemesisProfile(
                    "The Desert Jackal",
                    "Mounted Raider Ace",
                    WarTheaterType.BadlandsSteppeRecon,
                    "Commander of fast-moving armored scout half-tracks that butcher allied logistics convoys in open dust plains.",
                    75,
                    "Gold-Inlaid Cavalry Carbine",
                    500L)
            },
            {
                WarTheaterType.NorthernTaigaOutpost,
                new NemesisProfile(
                    "Oberst Frost",
                    "White Death of the Taiga",
                    WarTheaterType.NorthernTaigaOutpost,
                    "A ski-infantry commander who ambushes outposts in sub-zero whiteout blizzards, leaving no survivors.",
                    80,
                    "Sub-Zero Precision Arctic Rifle",
                    650L)
            },
            {
                WarTheaterType.PacificJungleIslands,
                new NemesisProfile(
                    "Captain Kuroda",
                    "Jungle Bunker Sovereign",
                    WarTheaterType.PacificJungleIslands,
                    "An unyielding commander who has burrowed an impenetrable labyrinth of coral caves and spider-hole traps.",
                    84,
                    "Imperial Officer Samurai Katana & Pistol",
                    700L)
            }
        };

        public static NemesisProfile GetNemesis(WarTheaterType theater)
        {
            if (theaterNemesisRegistry.TryGetValue(theater, out var nem))
            {
                return nem;
            }
            return theaterNemesisRegistry[WarTheaterType.ArdenRidgeTrenchLines];
        }

        /// <summary>
        /// Rolls to check if the theater nemesis appears during a frontline engagement (30% base chance).
        /// </summary>
        public static bool RollNemesisAppearance(WarTheaterType theater, SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return false;

            var nem = GetNemesis(theater);
            if (nem == null || nem.isDefeated) return false;

            int roll = UnityEngine.Random.Range(1, 101);
            return roll <= 30;
        }

        /// <summary>
        /// Resolves a tactical confrontation against the theater nemesis.
        /// choice 0 = Precision Counter-Sniper Duel
        /// choice 1 = Coordinated Squad Fireteam Ambush
        /// choice 2 = Heavy Artillery Pre-Registered Strike
        /// </summary>
        public static bool ResolveNemesisClash(SoldierProfile soldier, NemesisProfile nemesis, int choice, out string combatLog)
        {
            if (soldier == null || nemesis == null)
            {
                combatLog = "Error resolving clash.";
                return false;
            }

            bool victory = false;

            if (choice == 0) // Counter-Sniper Duel
            {
                float playerPower = (soldier.marksmanshipScore * 0.6f) + (soldier.combatSkill * 0.4f) + UnityEngine.Random.Range(-10f, 15f);
                if (playerPower >= nemesis.lethality)
                {
                    victory = true;
                    combatLog = $"<color=#00FFAA><b>[RIVAL ELIMINATED: COUNTER-SNIPER DUEL]</b></color> You steadied your breath through the scope, calculated crosswind, and squeezed the trigger. " +
                                $"Your round struck <b>{nemesis.name}</b> through the observation slit! The legend of '{nemesis.title}' has ended by your hand!";
                }
                else
                {
                    soldier.health = Mathf.Max(15f, soldier.health - 35f);
                    soldier.AddMedal("Imperial Wound Star");
                    soldier.woundStarsCount++;
                    soldier.combatStress = Mathf.Min(100f, soldier.combatStress + 20f);
                    combatLog = $"<color=#FF5555><b>[RIVAL BULLET GRAZE]</b></color> <b>{nemesis.name}</b> fired first! The high-caliber round smashed through your cover, showering you in shrapnel (-35 Health, Wound Star awarded).";
                }
            }
            else if (choice == 1) // Squad Fireteam Coordination
            {
                var scout = soldier.squadMates?.Find(m => m.role == SquadRole.LeadScout && !m.isDead);
                var gunner = soldier.squadMates?.Find(m => m.role == SquadRole.HeavyGunner && !m.isDead);

                bool scoutSuccess = scout != null && scout.relationship >= 40f;
                bool gunnerSuccess = gunner != null && gunner.relationship >= 40f;

                if (scoutSuccess && gunnerSuccess)
                {
                    victory = true;
                    combatLog = $"<color=#00FFAA><b>[RIVAL SURROUNDED: SQUAD COORDINATION]</b></color> Lead Scout {scout.nickname} spotted <b>{nemesis.name}'s</b> muzzle flash, while Heavy Gunner {gunner.nickname} laid down blistering cyclic fire pinning him! " +
                                $"You breached the redoubt from the rear and neutralized the enemy commander!";
                }
                else
                {
                    victory = false;
                    combatLog = $"<color=#FFAA00>[TACTICAL WITHDRAWAL FROM RIVAL]</color> Lacking suppressive firepower, <b>{nemesis.name}</b> held the crossfire corridor. Squad was forced to smoke the defile and break contact.";
                }
            }
            else // Artillery Call
            {
                victory = true;
                AudioManager.PlayArtilleryExplosion();
                combatLog = $"<color=#FFD700><b>[BATTERY OBLITERATION]</b></color> You radioed forward battery coordinates. Six 155mm high-explosive shells cratered the hideout! " +
                            $"<b>{nemesis.name}'s</b> position was obliterated in geysers of rock and earth.";
            }

            if (victory)
            {
                nemesis.isDefeated = true;
                soldier.cashBalance += nemesis.bounty;
                soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 20f);
                soldier.accumulatedPromotionPoints += 20;
                soldier.AddTrait($"Slayer of {nemesis.name}");
                soldier.AddTrait($"Trophy: {nemesis.signatureWeapon}");
                if (!soldier.HasMedal("Cross of the Nemesis Hunter"))
                {
                    soldier.AddMedal("Cross of the Nemesis Hunter");
                }
                AudioManager.PlayBugleFanfare();
            }

            GameEvents.OnFeedEventLogged?.Invoke("RIVAL", combatLog);
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return victory;
        }
    }
}
