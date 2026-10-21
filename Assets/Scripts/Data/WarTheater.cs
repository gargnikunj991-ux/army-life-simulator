using System;

namespace ArmyLife.Data
{
    public enum WarTheaterType
    {
        WesternMudTrenches,     // Sector 7: Poison gas, artillery barrages, static trench attrition
        EasternFrozenSteppe,    // The Winter Front: Frostbite, snowstorm armor clashes, starvation
        PacificJungleIslands,   // Southern Atolls: Ambush raids, booby traps, bunker flamethrowers
        UrbanCitadel            // The Citadel Siege: CQB room-clearing, snipers, civilian dilemmas
    }

    [Serializable]
    public class WarTheaterInfo
    {
        public WarTheaterType theaterType;
        public string theaterName;
        public string description;
        public string weatherAndEnvironment;
        public float baseHazardModifier; // 1.0f default, up to 1.35f for brutal fronts

        public WarTheaterInfo(WarTheaterType type, string name, string desc, string weather, float hazard)
        {
            this.theaterType = type;
            this.theaterName = name;
            this.description = desc;
            this.weatherAndEnvironment = weather;
            this.baseHazardModifier = hazard;
        }

        public static WarTheaterInfo GetInfo(WarTheaterType type)
        {
            switch (type)
            {
                case WarTheaterType.WesternMudTrenches:
                    return new WarTheaterInfo(type, "The Western Front (Sector 7 Mud Trenches)", 
                        "Miles of barbed wire, stagnant shell craters, and constant heavy artillery bombardments.", 
                        "Rain & Mud", 1.0f);
                case WarTheaterType.EasternFrozenSteppe:
                    return new WarTheaterInfo(type, "The Eastern Front (Karkov Frozen Steppe)", 
                        "Sub-zero blizzards, frozen bolt mechanisms, and ferocious mechanized armor counter-offensives.", 
                        "Blizzard & Frostbite", 1.25f);
                case WarTheaterType.PacificJungleIslands:
                    return new WarTheaterInfo(type, "The Southern Atolls (Peleliu Jungle Islands)", 
                        "Dense mangrove swamps, concealed spider-hole ambushes, and suffocating tropical humidity.", 
                        "Sweltering Heat & Malaria", 1.20f);
                case WarTheaterType.UrbanCitadel:
                    return new WarTheaterInfo(type, "The Citadel Siege (Metropolis in Ruins)", 
                        "Shattered brick canyons, concealed enemy snipers, booby-trapped basements, and room-by-room CQB.", 
                        "Dust & Falling Rubble", 1.30f);
                default:
                    return new WarTheaterInfo(type, "Garrison Front", "Standard military theater.", "Temperate", 1.0f);
            }
        }
    }
}
