using System;
using System.Collections.Generic;

namespace ArmyLife.Data
{
    public enum WarTheaterType
    {
        // 5 Primary Authentic Combat Theaters
        ArdenRidgeTrenchLines = 0,
        PortIronAmphibiousFront = 1,
        KaelenValleyUrbanZone = 2,
        BadlandsSteppeRecon = 3,
        NorthernTaigaOutpost = 4,

        // Backward-Compatibility Aliases
        WesternMudTrenches = 5,
        EasternFrozenSteppe = 6,
        PacificJungleIslands = 7,
        UrbanCitadel = 8
    }

    /// <summary>
    /// Scale and strategic escalation level of military conflicts.
    /// Determines senior officer (O-6+ Colonel, O-7/O-8 General, O-10 4-Star General) command hierarchy.
    /// </summary>
    public enum WarScale
    {
        SmallScale = 0,     // Low-intensity border skirmish, counter-insurgency (Colonel O-6 oversees deployment)
        MidScale = 1,       // Regional brigade combat (Brigade force arrives, General O-7/O-8 commands soldiers)
        LargeScale = 2      // Continental / Theater War (Supreme O-10 4-Star General commands, receives directives from civilian Government)
    }

    [Serializable]
    public class WarTheaterInfo
    {
        public WarTheaterType theaterType;
        public string theaterName;
        public string description;
        public int dangerLevel;                       // 1 (Low) to 5 (Lethal)
        public float casualtyRateModifier;            // e.g. 1.10f to 1.60f
        public long hazardPayRate;                    // Monthly/Tour hazard bonus pay (e.g. $450 - $750)
        public string specializedTerrainTrait;        // Authentic terrain traits and hazards
        public List<string> operationalDoctrinesRecommended = new List<string>();
        public string enemyDoctrine;                  // Hostile tactical doctrine
        public string weatherAndEnvironment;          // Atmospheric weather conditions
        public float baseHazardModifier;              // Backward compatibility with legacy hazard modifier
        public WarScale warScale = WarScale.MidScale; // Conflict escalation scale

        public WarTheaterInfo()
        {
            this.operationalDoctrinesRecommended = new List<string>();
        }

        public WarTheaterInfo(
            WarTheaterType type,
            string name,
            string desc,
            int danger,
            float casualtyModifier,
            long hazardPay,
            string terrainTrait,
            string enemyDoc,
            string weather,
            List<string> recommendedDoctrines)
        {
            this.theaterType = type;
            this.theaterName = name;
            this.description = desc;
            this.dangerLevel = danger;
            this.casualtyRateModifier = casualtyModifier;
            this.hazardPayRate = hazardPay;
            this.specializedTerrainTrait = terrainTrait;
            this.enemyDoctrine = enemyDoc;
            this.weatherAndEnvironment = weather;
            this.baseHazardModifier = casualtyModifier;
            this.operationalDoctrinesRecommended = recommendedDoctrines ?? new List<string>();
        }

        // Backward-compatible constructor for existing scripts and tests
        public WarTheaterInfo(WarTheaterType type, string name, string desc, string weather, float hazard)
            : this(type, name, desc, 3, hazard, 500L, "Frontline Combat Zone", "Standard Hostile Infantry", weather, new List<string>())
        {
        }

        public bool IsRecommendedDoctrine(string doctrine)
        {
            if (operationalDoctrinesRecommended == null || string.IsNullOrEmpty(doctrine)) return false;
            return operationalDoctrinesRecommended.Exists(d => d.IndexOf(doctrine, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public static WarTheaterInfo GetInfo(WarTheaterType type)
        {
            switch (type)
            {
                case WarTheaterType.ArdenRidgeTrenchLines:
                case WarTheaterType.WesternMudTrenches:
                    return new WarTheaterInfo(
                        WarTheaterType.ArdenRidgeTrenchLines,
                        "Arden Ridge Trench Lines",
                        "Brutal artillery barrages, trench warfare, muddy trench foot, static defense. Miles of fortified ditch networks, shell craters, and razor wire under endless cold drizzle.",
                        4,
                        1.35f,
                        500L,
                        "Muddy Trench Foot & Artillery Barrages",
                        "Static Trench Defense & Rolling Creeping Barrages",
                        "Bleak Drizzle & Sucking Mud",
                        new List<string> { "Static Trench Defense", "Heavy Gunner Suppression", "Trench Breach Assault", "Artillery Counter-Battery" }
                    ) { warScale = WarScale.MidScale };

                case WarTheaterType.PortIronAmphibiousFront:
                case WarTheaterType.PacificJungleIslands:
                    return new WarTheaterInfo(
                        WarTheaterType.PortIronAmphibiousFront,
                        "Port Iron Amphibious Front",
                        "Coastal assault, bunker clearing, naval gunfire support, heavy casualties. Fortified concrete seawalls, surf obstacles, and interlocking machine gun pillboxes under shore battery fire.",
                        5,
                        1.55f,
                        750L,
                        "Beachhead Shelling & Fortified Sea-Walls",
                        "Interlocking Bunker Crossfire & Heavy Coastal Batteries",
                        "Salt Spray & Churning Surf",
                        new List<string> { "Amphibious Breach Assault", "Naval Gunfire Support", "Under-Fire CASEVAC", "Bunker Clearing" }
                    ) { warScale = WarScale.LargeScale };

                case WarTheaterType.KaelenValleyUrbanZone:
                case WarTheaterType.UrbanCitadel:
                    return new WarTheaterInfo(
                        WarTheaterType.KaelenValleyUrbanZone,
                        "Kaelen Valley Urban Zone",
                        "Dense urban combat, sniper threats, IED ambushes, room-clearing CQB. Collapsed skyscrapers, rubble-strewn avenues, and subterranean sewer conduits occupied by guerrilla cells.",
                        4,
                        1.40f,
                        600L,
                        "CQB Rubble & Concealed Sniper Alleys",
                        "Urban Guerrilla Ambushes, Snipers & Booby-Trapped Basements",
                        "Smothering Concrete Dust & Ash",
                        new List<string> { "Room-Clearing CQB", "Lead Scout Overwatch", "Night Sabotage Raid", "IED Countermeasures" }
                    ) { warScale = WarScale.MidScale };

                case WarTheaterType.BadlandsSteppeRecon:
                    return new WarTheaterInfo(
                        WarTheaterType.BadlandsSteppeRecon,
                        "Badlands Steppe Recon",
                        "Wide desert expanse, mechanized vehicle patrols, heat exhaustion, long-range skirmishes. Expansive salt basins and jagged sandstone canyons with roving motorized raiders.",
                        3,
                        1.20f,
                        450L,
                        "Heat Exhaustion & Open Desert Vistas",
                        "Mechanized Hit-and-Run & Long-Range Skirmishing",
                        "Scorching Sun & Blinding Dust Storms",
                        new List<string> { "High-Value Recon Patrol", "Convoy Overwatch & Ambush Defense", "Long-Range Marksman Flank", "Mechanized Armor Patrol" }
                    ) { warScale = WarScale.SmallScale };

                case WarTheaterType.NorthernTaigaOutpost:
                case WarTheaterType.EasternFrozenSteppe:
                    return new WarTheaterInfo(
                        WarTheaterType.NorthernTaigaOutpost,
                        "Northern Taiga Outpost",
                        "Arctic warfare, sub-zero hypothermia, deep snow, guerrilla mountain counter-insurgency. Snowbound pine forests, frozen bolt mechanisms, and enemy alpine infiltrators.",
                        3,
                        1.25f,
                        550L,
                        "Sub-Zero Hypothermia & Dense Pine Snowdrifts",
                        "Mountain Guerrilla Raids & Deep Winter Infiltration",
                        "Freezing Blizzards & Deep Snowdrifts",
                        new List<string> { "Forward Artillery Observation", "Cold Weather Guerrilla Warfare", "Snow Scout Infiltration", "Under-Fire CASEVAC" }
                    ) { warScale = WarScale.SmallScale };

                default:
                    return new WarTheaterInfo(
                        type,
                        "Garrison Sector",
                        "Standard military staging area.",
                        1,
                        1.0f,
                        300L,
                        "Temperate Plains",
                        "Standard Patrols",
                        "Temperate",
                        new List<string> { "Standard Patrol" }
                    );
            }
        }

        public static List<WarTheaterInfo> GetAllTheaters()
        {
            return new List<WarTheaterInfo>
            {
                GetInfo(WarTheaterType.ArdenRidgeTrenchLines),
                GetInfo(WarTheaterType.PortIronAmphibiousFront),
                GetInfo(WarTheaterType.KaelenValleyUrbanZone),
                GetInfo(WarTheaterType.BadlandsSteppeRecon),
                GetInfo(WarTheaterType.NorthernTaigaOutpost)
            };
        }
    }
}
