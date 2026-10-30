using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyLife.Data
{
    [Serializable]
    public class CityData
    {
        public string cityName;
        public bool isStateCapital;
        public bool isImperialCapital;

        public CityData(string name, bool isCapital = false, bool isImperial = false)
        {
            cityName = name;
            isStateCapital = isCapital;
            isImperialCapital = isImperial;
        }
    }

    [Serializable]
    public class StateData
    {
        public string stateName;
        public string capitalName;
        public bool isImperialCrownState;
        public List<CityData> cities = new List<CityData>();

        public StateData(string name, string capital, bool isImperialState = false)
        {
            stateName = name;
            capitalName = capital;
            isImperialCrownState = isImperialState;
            cities = new List<CityData>();
        }
    }

    [Serializable]
    public class EmpireGeography
    {
        public string empireName;
        public int standardStateCount;
        public StateData imperialState;
        public List<StateData> standardStates = new List<StateData>();

        public EmpireGeography(string name, int standardCount)
        {
            empireName = name;
            standardStateCount = standardCount;
            standardStates = new List<StateData>();
        }
    }

    public static class WorldGeographyDatabase
    {
        private static Dictionary<string, EmpireGeography> Empires;

        static WorldGeographyDatabase()
        {
            BuildWorldGeography();
        }

        public static EmpireGeography GetEmpire(string empireName)
        {
            if (Empires == null) BuildWorldGeography();
            if (Empires.TryGetValue(empireName, out var emp)) return emp;
            return Empires["The Valerian Empire"];
        }

        public static (string state, string city, bool isImperial) RollBirthLocation(string empireName, int familyInfluence = 0)
        {
            var empire = GetEmpire(empireName);

            // High Military Influence families (General / Officer dynasties) have high chance to be stationed in the Capital!
            float imperialChance = (familyInfluence >= 70) ? 0.80f : ((familyInfluence >= 30) ? 0.30f : 0.05f);

            if (UnityEngine.Random.value < imperialChance && empire.imperialState != null)
            {
                var city = empire.imperialState.cities[UnityEngine.Random.Range(0, empire.imperialState.cities.Count)];
                return (empire.imperialState.stateName, city.cityName, true);
            }

            // Standard Provincial State
            if (empire.standardStates.Count > 0)
            {
                var state = empire.standardStates[UnityEngine.Random.Range(0, empire.standardStates.Count)];
                var city = state.cities[UnityEngine.Random.Range(0, state.cities.Count)];
                return (state.stateName, city.cityName, false);
            }

            return ("Central Crown Province", "Valeria Magna", true);
        }

        private static void BuildWorldGeography()
        {
            Empires = new Dictionary<string, EmpireGeography>();

            // 1. THE VALERIAN EMPIRE: 12 Standard States (18-22 cities each) + 1 Imperial State (5 cities)
            var valerian = new EmpireGeography("The Valerian Empire", 12);
            
            // Imperial Crown State (5 Cities + Imperial Capital)
            var valerianImperial = new StateData("The Imperial Crownlands (Aethelgard)", "Valeria Magna", true);
            valerianImperial.cities.Add(new CityData("Valeria Magna (Imperial Throne City)", true, true));
            valerianImperial.cities.Add(new CityData("Aegis Fortress Citadel", false, false));
            valerianImperial.cities.Add(new CityData("The Emperor's Sanctum (Solaris)", false, false));
            valerianImperial.cities.Add(new CityData("Grand Imperial War Academy District", false, false));
            valerianImperial.cities.Add(new CityData("Aethelgard Celestial Port", false, false));
            valerian.imperialState = valerianImperial;

            string[] valerianStateNames = {
                "Ironwood Province", "Sunspire Marches", "Silverbrook Duchy", "Stormcrag Highlands", 
                "Verdant Basin", "Gilded Reach", "Frostpeak Barony", "Emberfall Valley", 
                "Whispering Vale", "Crimson Bluffs", "Oakenhelm Shire", "Thunder Ridge"
            };

            string[] valerianCapitals = {
                "Highspire", "Solarium", "Argentum", "Drakencrag", 
                "Verdantia", "Aurelia", "Glaciarum", "Pyregard", 
                "Mistveil", "Redoubt", "Oakhaven", "Stormbastion"
            };

            string[] valerianCityPrefixes = { "Fort", "Port", "Mount", "Val", "Saint", "Old", "New", "Castle", "Iron", "River", "Stone", "High", "Deep", "Falcon", "Crow", "Gold", "Silver", "Eagle", "Barrow", "Wolf", "Crown", "Shield" };
            string[] valerianCitySuffixes = { "ford", "stead", "keep", "burgh", "haven", "shire", "wick", "crest", "cross", "gate", "wall", "pass", "ridge", "well", "glen", "fall", "mill", "rock", "watch", "fortress" };

            for (int s = 0; s < 12; s++)
            {
                var state = new StateData(valerianStateNames[s], valerianCapitals[s], false);
                state.cities.Add(new CityData(valerianCapitals[s] + " (Provincial Capital)", true));

                int cityCount = UnityEngine.Random.Range(18, 23); // 18 to 22 cities
                for (int c = 1; c < cityCount; c++)
                {
                    string pre = valerianCityPrefixes[(s * 3 + c) % valerianCityPrefixes.Length];
                    string suf = valerianCitySuffixes[(s * 5 + c * 2) % valerianCitySuffixes.Length];
                    state.cities.Add(new CityData($"{pre} {suf}"));
                }
                valerian.standardStates.Add(state);
            }
            Empires["The Valerian Empire"] = valerian;

            // 2. THE IRON FEDERATION: 10 Standard Cyber Sectors (18-22 cities/nodes) + 1 Apex Core State (5 cities)
            var ironFed = new EmpireGeography("The Iron Federation", 10);
            
            var ironImperial = new StateData("The Apex Directorate Core", "Apex Prime", true);
            ironImperial.cities.Add(new CityData("Apex Prime (Megacity Directorate)", true, true));
            ironImperial.cities.Add(new CityData("Cyber-Nexus Citadel", false, false));
            ironImperial.cities.Add(new CityData("Orbital EMP Rail Complex", false, false));
            ironImperial.cities.Add(new CityData("Grand Foundry 01", false, false));
            ironImperial.cities.Add(new CityData("Quantum Processing Grid Central", false, false));
            ironFed.imperialState = ironImperial;

            string[] ironSectorNames = {
                "Titan Mech Foundry Sector", "Nanite Synth District", "Volt-Haven Complex", "Steel Matrix Marches",
                "Silicon Ridge", "Cobalt Basin", "Heavy Gear Enclave", "Laser Grid Outskirts",
                "Sub-Terrene Drill Zone", "Borealis Turbine Belt"
            };

            string[] ironCapitals = {
                "Titanforge", "Nanopolis", "Voltagrad", "Steelhaven", 
                "Silicon City", "Cobaltum", "Gearspire", "Luminox", 
                "Deepcore 07", "Turbinia"
            };

            for (int s = 0; s < 10; s++)
            {
                var state = new StateData(ironSectorNames[s], ironCapitals[s], false);
                state.cities.Add(new CityData(ironCapitals[s] + " (Sector Nexus)", true));

                int cityCount = UnityEngine.Random.Range(18, 23);
                for (int c = 1; c < cityCount; c++)
                {
                    state.cities.Add(new CityData($"Node-{s + 1:D2}-Sub{(c * 7) % 99:D2}"));
                }
                ironFed.standardStates.Add(state);
            }
            Empires["The Iron Federation"] = ironFed;

            // 3. THE OUROBOROS DOMINION: 14 Standard Beast Territories (18-22 cities/hives) + 1 Primal Sovereign State (5 cities)
            var ouroboros = new EmpireGeography("The Ouroboros Dominion", 14);

            var ouroImperial = new StateData("The Sovereign Primal Domain", "Drakonhold Spire", true);
            ouroImperial.cities.Add(new CityData("Drakonhold Spire (Throne of Chimera)", true, true));
            ouroImperial.cities.Add(new CityData("Ancient Wyrm Blood Well", false, false));
            ouroImperial.cities.Add(new CityData("Colosseum of Beast Warlords", false, false));
            ouroImperial.cities.Add(new CityData("Apex Chimera Breeding Spire", false, false));
            ouroImperial.cities.Add(new CityData("Primal Shaman Altar", false, false));
            ouroboros.imperialState = ouroImperial;

            string[] ouroTerritoryNames = {
                "Blood-Claw Steppes", "Chimera Fang Wastes", "Venom Marshlands", "Gargoyle Crags",
                "Hydra Deep-Fens", "Beast-Rider Badlands", "Serpentspine Mountains", "Mammoth Tundra",
                "Savage Maw Canopy", "Gore-Peak Cliffs", "Bramble-Spire Glade", "Roaring Caldera",
                "Shadow-Beast Hollows", "Raptor Ridge Territory"
            };

            string[] ouroCapitals = {
                "Bloodhaven", "Fangspire", "Venomhold", "Gargoyle Fort", 
                "Hydra Watch", "Beastrock", "Serpentia", "Mammoth Skull", 
                "Mawgate", "Gorecliff", "Brambleglen", "Calderon", 
                "Shadowlair", "Raptor Roost"
            };

            for (int s = 0; s < 14; s++)
            {
                var state = new StateData(ouroTerritoryNames[s], ouroCapitals[s], false);
                state.cities.Add(new CityData(ouroCapitals[s] + " (Chieftain Spire)", true));

                int cityCount = UnityEngine.Random.Range(18, 23);
                for (int c = 1; c < cityCount; c++)
                {
                    state.cities.Add(new CityData($"Clan-Den {(c * 4 + s):D2} Stronghold"));
                }
                ouroboros.standardStates.Add(state);
            }
            Empires["The Ouroboros Dominion"] = ouroboros;
        }
    }
}
