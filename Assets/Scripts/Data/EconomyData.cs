using System;
using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Military branches available for enlistment and service.
    /// </summary>
    public enum MilitaryBranch
    {
        Army = 0,       // Frontline combat, mud trenches, infantry divisions
        AirForce = 1    // Aviation wings, aerial interception, technical ground support
    }

    /// <summary>
    /// Specialized roles within the Air Force branch.
    /// </summary>
    public enum AirForceRole
    {
        None = 0,               // Non-Air Force or unassigned
        FlightDuty = 1,         // Combat pilot, navigator, flight engineer (+15% Aviation Pay)
        GroundDutyStaff = 2     // Avionics maintenance, radar ops, airfield logistics, munitions
    }

    /// <summary>
    /// Residential housing & living quarters tiers for military personnel and veterans.
    /// Determines monthly rent and provides morale, rest, and lifestyle benefits.
    /// </summary>
    public enum HousingTier
    {
        BarracksOrSlums = 0,            // On-Base Enlisted Barracks ($200/mo = $1,200/sem) - Minimum
        ModestApartment = 1,            // NCO Private Quarters / Dorms ($600/mo = $3,600/sem) - E-4+
        ComfortableHome = 2,            // Off-Base Rental Townhouse ($1,400/mo = $8,400/sem) - E-6+ / O-1+
        NobleEstate = 3,                // Officer Family Single Quarters ($2,800/mo = $16,800/sem) - E-8+ / O-3+
        ImperialPalaceWing = 4          // Senior Command Estate / General's Manor ($5,000/mo = $30,000/sem) - O-6+
    }

    /// <summary>
    /// Garrison dining and nutritional meal plans for military personnel.
    /// Deducted during non-deployed garrison service.
    /// </summary>
    public enum DiningPlanTier
    {
        StandardMessHallDFAC = 0,       // Base Dining Facility Mess Hall ($100/mo = $600/sem) - Minimum
        CommissaryCooking = 1,          // Commissary Fresh Groceries & Meal Prep ($350/mo = $2,100/sem)
        HighProteinAthletic = 2,        // Athletic High-Protein Performance Diet ($750/mo = $4,500/sem)
        OfficersClubDining = 3,         // Officers' Club Fine Dining ($1,300/mo = $7,800/sem)
        PrivateChefExecutive = 4        // Private Chef & Gourmet Executive Cuisine ($2,000/mo = $12,000/sem) - Maximum
    }

    /// <summary>
    /// Commercial businesses available for private player ownership and semester dividend revenue.
    /// </summary>
    public enum CommercialBusiness
    {
        None = 0,
        Workshop = 1,                           // Artisan Blacksmith & Craft Workshop ($5,000 purchase)
        Tavern = 2,                             // Garrison Tavern & Travelers Inn ($12,000 purchase)
        MerchantCaravan = 3,                    // Inter-State Merchant Caravan Fleet ($25,000 purchase)
        PrivateSecurityAgency = 4,              // Elite Private Security Consulting Agency ($50,000 purchase)
        DefenseMunitionsConglomerate = 5,        // Industrial cannon & small-arms manufacturing conglomerate ($250,000 purchase)
        ImperialShippingCartel = 6,              // Transcontinental naval fleet & armored rail logistics cartel ($1,000,000 purchase)
        CentralBankingHouseAndMint = 7          // Imperial mint & sovereign bond banking house ($5,000,000 purchase)
    }

    /// <summary>
    /// Real estate properties purchased to permanently house parents and extended family lineage.
    /// </summary>
    public enum FamilyHouseTier
    {
        None = 0,
        RuralHomestead = 1,              // $5,000 purchase: Stable country farmland and stone cottage
        CityTownhouse = 2,               // $25,000 purchase: Multi-story townhouse in provincial capital
        AncestralEstate = 3,             // $100,000 purchase: Magnificent ancestral manor with sprawling grounds
        DucalPalaceCitadel = 4,          // $500,000 purchase: High-walled ducal citadel with private garrison
        SovereignImperialChateau = 5,    // $2,500,000 purchase: Sprawling royal chateau with private vineyards
        ImperialDynasticSeat = 6         // $10,000,000 purchase: The Eternal Mountain Citadel, pinnacle of dynastic dynasty
    }

    /// <summary>
    /// Underworld crimes and illicit activities across military, civilian, and veteran tracks.
    /// </summary>
    public enum CrimeType
    {
        // Military Crimes
        SiphonFuelAndRations = 0,       // Siphon military diesel & sell MREs (+$800, +10% heat)
        ArmoryTheft = 1,                // Smuggle military rifles & ordnance to black market (+$3,200, +25% heat)
        BarracksGamblingRing = 2,       // Run high-stakes card/dice racket in squad quarters (+$1,500, +15% heat)
        FrontlineWarPlunder = 3,        // Loot conquered vaults & enemy officer contraband (+$5,000, +30% heat)
        DefenseProcurementBribery = 4,  // Take kickbacks on munitions supply contracts (+$35,000, +45% heat, Officer only)

        // Civilian Crimes
        UndergroundFightClub = 10,      // Bare-knuckle back-alley prize brawls (+$1,200, +15% heat)
        ContrabandCourier = 11,         // Smuggle unmarked military morphine & combat stims past checkpoints (+$3,500, +20% heat)
        SyndicateDebtEnforcer = 12,     // Shake down delinquent gambling debtors (+$2,000, +18% heat)
        WarehouseHeist = 13,            // Infiltrate and clean out an imperial customs depot (+$30,000, +40% heat)
        CorporateEmbezzlement = 14,     // Skim funds through double-entry accounting fraud (+$18,000, +35% heat)

        // Veteran Crimes
        SurplusGunrunning = 20,         // Traffic surplus artillery & assault carbines abroad (+$75,000, +45% heat)
        ShadowPMCWetwork = 21,          // High-value black ops assassination contract (+$35,000, +40% heat)
        CityHallGraft = 22              // Divert municipal infrastructure bond treasury (+$120,000, +50% heat)
    }

    /// <summary>
    /// Financial and economic constants, pricing tables, dividend yields, and helper formulas.
    /// </summary>
    public static class EconomyData
    {
        // ==================== AVIATION & AIR FORCE CONSTANTS ====================
        public const float AVIATION_FLIGHT_PAY_BONUS = 0.25f; // +25% Aviation Flight Pay Bonus on base pay
        public const float AIR_FORCE_HOUSING_DISCOUNT = 0.20f; // 20% lower living expense penalties
        public const float AIR_FORCE_MORALE_BOOST = 15f;      // +15% Morale/Happiness
        public const float AIR_FORCE_HEALTH_BOOST = 10f;      // +10% Health recovery

        // ==================== INVESTMENT CONSTANTS ====================
        public const long WAR_BOND_PRICE = 500L;
        public const long WAR_BOND_COST = WAR_BOND_PRICE;
        public const long WAR_BOND_SEMESTER_DIVIDEND = 30L; // 6% semi-annual = 12% annual yield

        public const long TRADE_GUILD_SHARE_PRICE = 1000L;
        public const long TRADE_GUILD_SHARE_COST = TRADE_GUILD_SHARE_PRICE;
        public const long TRADE_GUILD_SHARE_SEMESTER_DIVIDEND = 60L; // 6% semi-annual = 12% annual yield

        // ==================== SPENDING & HEALTHCARE CONSTANTS ====================
        public const long PRIVATE_TUTOR_SEMESTER_COST = 1000L;
        public const long IMPERIAL_HOSPITAL_TREATMENT_COST = 500L;
        public const long SPECIALIST_HOSPITAL_CARE_COST = IMPERIAL_HOSPITAL_TREATMENT_COST;
        public const long FAMILY_MEDICAL_EMERGENCY_COST = 1200L;
        public const long SQUAD_ROUNDS_COST = 150L;
        public const long SQUAD_MESS_DRINKS_COST = SQUAD_ROUNDS_COST;

        // ==================== TAXATION & PENSION CONSTANTS ====================
        public const float IMPERIAL_WAR_TAX_RATE = 0.40f;       // 40% active military income tax
        public const float MUNICIPAL_CIVIL_TAX_RATE = 0.50f;     // 50% civilian municipal tax
        public const long MINIMUM_VETERAN_PENSION = 1500L;      // $1,500/month guaranteed pension floor
        public const float PENSION_RATE_PER_YEAR = 0.025f;      // 2.5% of base pay per year of service
        public const long VA_DISABILITY_PER_WOUND_STAR = 300L;  // $300/month per wound star/medal
        public const float ACADEMY_LOAN_SEMESTER_INTEREST = 0.045f; // 9% annual = 4.5% per semester
        public const float COLLEGE_LOAN_SEMESTER_INTEREST = 0.025f; // 5% annual = 2.5% per semester

        // ==================== BRANCH BALANCE & ACADEMY METRICS ====================
        // Army: Faster 6-month boot camp, 2-year Academy ($100k), Ground Bounties & Plunder
        public const int ARMY_BASIC_TRAINING_SEMESTERS = 1;         // 6 Months
        public const int ARMY_WAR_ACADEMY_SEMESTERS = 4;            // 2 Years (4 semesters)
        public const long ARMY_ACADEMY_TUITION_COST = 100000L;      // $100,000

        // Air Force: Longer 1-year training, 3-year Academy ($200k), Flight Pay +15%, Better Quarters
        public const int AIRFORCE_BASIC_TRAINING_SEMESTERS = 2;     // 1 Year (2 semesters)
        public const int AIRFORCE_ACADEMY_SEMESTERS = 6;            // 3 Years (6 semesters)
        public const long AIRFORCE_ACADEMY_TUITION_COST = 200000L;  // $200,000 (Double/Quadruple tuition)

        // Unique Special Forces Names by Branch
        public const string ARMY_SPECIAL_FORCES_NAME = "The Iron Vanguard";
        public const string ARMY_SPECIAL_FORCES_ROLE = "Iron Vanguard Commando";
        public const string AIRFORCE_SPECIAL_FORCES_NAME = "The Celestial Valkyries";
        public const string AIRFORCE_SPECIAL_FORCES_ROLE = "Celestial Valkyrie Ace";

        // ==================== MEDAL IMPERIAL CASH BOUNTIES ====================
        public static long GetMedalCashReward(string medalName)
        {
            if (string.IsNullOrEmpty(medalName)) return 0L;

            if (medalName.Contains("Grand Imperial Star") || medalName.Contains("Medal of Honor"))
                return 75000L; // Nation's highest decoration
            if (medalName.Contains("Distinguished Service Cross"))
                return 30000L;
            if (medalName.Contains("Silver Vanguard Cross") || medalName.Contains("Silver Star") || medalName.Contains("Silver Cross"))
                return 15000L;
            if (medalName.Contains("Distinguished Flying Cross"))
                return 15000L;
            if (medalName.Contains("Special Operations Valor Cross"))
                return 20000L;
            if (medalName.Contains("Iron Vanguard") || medalName.Contains("Celestial Valkyrie"))
                return 10000L;
            if (medalName.Contains("Bronze Star"))
                return 6000L;
            if (medalName.Contains("Valor Citation"))
                return 3500L;
            if (medalName.Contains("Academy Honors Saber"))
                return 5000L;
            if (medalName.Contains("Imperial Wound Star"))
                return 3000L; // Combat casualty stipend
            if (medalName.Contains("Combat Infantry Badge"))
                return 2500L;

            return 2000L; // Standard military citation award
        }

        // ==================== HOUSING HELPERS ====================
        public static long GetHousingSemesterCost(HousingTier tier, MilitaryBranch branch = MilitaryBranch.Army)
        {
            long cost;
            switch (tier)
            {
                case HousingTier.BarracksOrSlums:    cost = 250L; break;
                case HousingTier.ModestApartment:    cost = 600L; break;
                case HousingTier.ComfortableHome:    cost = 1800L; break;
                case HousingTier.NobleEstate:        cost = 5000L; break;
                case HousingTier.ImperialPalaceWing: cost = 15000L; break;
                default:                             cost = 250L; break;
            }

            // Air Force base housing provides 20% discount on quarters upkeep
            if (branch == MilitaryBranch.AirForce)
            {
                cost = (long)(cost * (1.0f - AIR_FORCE_HOUSING_DISCOUNT));
            }

            return cost;
        }

        public static long GetHousingMonthlyRent(HousingTier tier) => Math.Max(1L, GetHousingSemesterCost(tier) / 6L);

        public static int GetMinimumRankOrderForHousing(HousingTier tier)
        {
            switch (tier)
            {
                case HousingTier.ModestApartment:    return 4;   // E-4 Corporal+
                case HousingTier.ComfortableHome:    return 6;   // E-6 Staff Sgt+ or O-1+
                case HousingTier.NobleEstate:        return 12;  // E-8+ or O-3 Captain+
                case HousingTier.ImperialPalaceWing: return 15;  // O-6 Colonel+ to O-10 General
                default:                             return 1;   // E-1 Recruit+
            }
        }

        public static string GetHousingDisplayName(HousingTier tier, MilitaryBranch branch = MilitaryBranch.Army)
        {
            if (branch == MilitaryBranch.AirForce && tier == HousingTier.BarracksOrSlums)
            {
                return "Air Base Dormitory ($200/mo)";
            }

            switch (tier)
            {
                case HousingTier.BarracksOrSlums:    return "On-Base Enlisted Barracks ($200/mo)";
                case HousingTier.ModestApartment:    return "NCO Private Quarters ($600/mo)";
                case HousingTier.ComfortableHome:    return "Off-Base Rental Townhouse ($1,400/mo)";
                case HousingTier.NobleEstate:        return "Officer Single Family Quarters ($2,800/mo)";
                case HousingTier.ImperialPalaceWing: return "Senior Command Estate ($5,000/mo)";
                default:                             return "Base Barracks ($200/mo)";
            }
        }

        public static string GetHousingDescription(HousingTier tier, MilitaryBranch branch = MilitaryBranch.Army)
        {
            if (branch == MilitaryBranch.AirForce && tier == HousingTier.BarracksOrSlums)
            {
                return "Air-conditioned air base suite with private lockers, mess amenities, and flight simulator lounge ($200/mo). Superior comfort.";
            }

            switch (tier)
            {
                case HousingTier.BarracksOrSlums:
                    return "Shared open-bay or double bunkhouse on base ($200/mo). Room inspections, shared latrines, military order.";
                case HousingTier.ModestApartment:
                    return "Private room in NCO barracks or modest shared off-base flat ($600/mo). Quiet rest, personal study, +2 Happiness.";
                case HousingTier.ComfortableHome:
                    return "Spacious suburban rental home with yard and garage ($1,400/mo). Private lifestyle, +5 Happiness, +2 Fitness.";
                case HousingTier.NobleEstate:
                    return "Single-family officer row house in prestigious base quarters ($2,800/mo). Formal parlor, +8 Happiness, +4 Respect.";
                case HousingTier.ImperialPalaceWing:
                    return "Historic Commanding Officer's Manor / Executive Villa ($5,000/mo). Gated perimeter, formal reception lawns, +15 Happiness, +8 Command Presence.";
                default:
                    return "Standard quarters ($200/mo).";
            }
        }

        // ==================== DINING PLAN HELPERS ====================
        public static long GetDiningMonthlyCost(DiningPlanTier tier)
        {
            switch (tier)
            {
                case DiningPlanTier.StandardMessHallDFAC: return 100L;   // Minimum: $100/mo standard mess hall
                case DiningPlanTier.CommissaryCooking:    return 350L;   // $350/mo fresh groceries
                case DiningPlanTier.HighProteinAthletic:  return 750L;   // $750/mo sports nutrition
                case DiningPlanTier.OfficersClubDining:   return 1300L;  // $1,300/mo officer dining
                case DiningPlanTier.PrivateChefExecutive: return 2000L;  // $2,000/mo private chef (Maximum)
                default:                                  return 100L;
            }
        }

        public static long GetDiningSemesterCost(DiningPlanTier tier) => GetDiningMonthlyCost(tier) * 6L;

        public static string GetDiningDisplayName(DiningPlanTier tier)
        {
            switch (tier)
            {
                case DiningPlanTier.StandardMessHallDFAC: return "Base DFAC Mess Hall ($100/mo)";
                case DiningPlanTier.CommissaryCooking:    return "Commissary Fresh Groceries ($350/mo)";
                case DiningPlanTier.HighProteinAthletic:  return "Athletic Performance Nutrition ($750/mo)";
                case DiningPlanTier.OfficersClubDining:   return "Officers' Club Dinners ($1,300/mo)";
                case DiningPlanTier.PrivateChefExecutive: return "Private Chef Gourmet Dining ($2,000/mo)";
                default:                                  return "Standard Mess Hall ($100/mo)";
            }
        }

        public static string GetDiningDescription(DiningPlanTier tier)
        {
            switch (tier)
            {
                case DiningPlanTier.StandardMessHallDFAC:
                    return "Standard cafeteria tray rations at the dining facility ($100/mo). Caloric baseline, zero fluff.";
                case DiningPlanTier.CommissaryCooking:
                    return "Home-cooked meals with fresh base commissary meats, greens, and fruits ($350/mo). +3 Fitness, +2 Health.";
                case DiningPlanTier.HighProteinAthletic:
                    return "Structured high-protein athletic meal prep, clean whole foods, and sports supplements ($750/mo). +6 Fitness, +5 Health.";
                case DiningPlanTier.OfficersClubDining:
                    return "Regular ribeye dinners, club lounge catering, and fine wines at the Officers' Club ($1,300/mo). +8 Happiness, +3 Respect.";
                case DiningPlanTier.PrivateChefExecutive:
                    return "Dedicated personal culinary chef delivering bespoke organic nutrition and executive banquets ($2,000/mo). +12 Happiness, +8 Health.";
                default:
                    return "Standard mess hall rations ($100/mo).";
            }
        }

        public static long GetWarBondDividendPerSemester(int count) => count * WAR_BOND_SEMESTER_DIVIDEND;
        public static long GetTradeGuildDividendPerSemester(int count) => count * TRADE_GUILD_SHARE_SEMESTER_DIVIDEND;

        public static long GetHousingUpkeepPerSemester(HousingTier tier, MilitaryBranch branch = MilitaryBranch.Army) => GetHousingSemesterCost(tier, branch);
        public static long GetHousingUpkeepPerMonth(HousingTier tier, MilitaryBranch branch = MilitaryBranch.Army) => GetHousingSemesterCost(tier, branch) / 6L;
        public static string GetHousingName(HousingTier tier, MilitaryBranch branch = MilitaryBranch.Army) => GetHousingDisplayName(tier, branch);

        // ==================== BUSINESS HELPERS ====================
        public static long GetBusinessPurchaseCost(CommercialBusiness business)
        {
            switch (business)
            {
                case CommercialBusiness.Workshop:                     return 5000L;
                case CommercialBusiness.Tavern:                       return 12000L;
                case CommercialBusiness.MerchantCaravan:              return 25000L;
                case CommercialBusiness.PrivateSecurityAgency:        return 50000L;
                case CommercialBusiness.DefenseMunitionsConglomerate: return 250000L;
                case CommercialBusiness.ImperialShippingCartel:       return 1000000L;
                case CommercialBusiness.CentralBankingHouseAndMint:   return 5000000L;
                default:                                              return 0L;
            }
        }

        public static long GetBusinessBaseSemesterRevenue(CommercialBusiness business)
        {
            switch (business)
            {
                case CommercialBusiness.Workshop:                     return 800L;
                case CommercialBusiness.Tavern:                       return 2000L;
                case CommercialBusiness.MerchantCaravan:              return 4500L;
                case CommercialBusiness.PrivateSecurityAgency:        return 10000L;
                case CommercialBusiness.DefenseMunitionsConglomerate: return 45000L;
                case CommercialBusiness.ImperialShippingCartel:       return 160000L;
                case CommercialBusiness.CentralBankingHouseAndMint:   return 750000L;
                default:                                              return 0L;
            }
        }

        public static long GetBusinessBaseSemesterYield(CommercialBusiness business) => GetBusinessBaseSemesterRevenue(business);

        public static string GetBusinessDisplayName(CommercialBusiness business)
        {
            switch (business)
            {
                case CommercialBusiness.Workshop:                     return "Artisan Craft & Blacksmith Workshop";
                case CommercialBusiness.Tavern:                       return "Garrison Tavern & Travelers Inn";
                case CommercialBusiness.MerchantCaravan:              return "Inter-State Merchant Caravan Fleet";
                case CommercialBusiness.PrivateSecurityAgency:        return "Elite Private Security Consulting Agency";
                case CommercialBusiness.DefenseMunitionsConglomerate: return "Imperial Munitions & Cannon Foundry";
                case CommercialBusiness.ImperialShippingCartel:       return "Transcontinental Armored Logistics Cartel";
                case CommercialBusiness.CentralBankingHouseAndMint:   return "The Imperial Bank & Sovereign Mint";
                default:                                              return "None";
            }
        }

        public static string GetBusinessName(CommercialBusiness business) => GetBusinessDisplayName(business);

        public static string GetBusinessDescription(CommercialBusiness business)
        {
            switch (business)
            {
                case CommercialBusiness.Workshop:
                    return "Forges weapons, armor, and industrial tools. Stable dividends driven by military procurement ($5k cost, ~$800/sem).";
                case CommercialBusiness.Tavern:
                    return "Popular tavern near garrison gates. Heavy flow of off-duty soldiers and traveling merchants ($12k cost, ~$2,000/sem).";
                case CommercialBusiness.MerchantCaravan:
                    return "Caravan fleet hauling exotic spices, silks, and military metals between provincial hubs ($25k cost, ~$4,500/sem).";
                case CommercialBusiness.PrivateSecurityAgency:
                    return "High-end private tactical consulting firm safeguarding noble estates, banks, and merchant VIPs ($50k cost, ~$10,000/sem).";
                case CommercialBusiness.DefenseMunitionsConglomerate:
                    return "Sprawling industrial complex manufacturing heavy siege howitzers, armor plating, and munitions ($250k cost, ~$45,000/sem). Massive yields backed by war contracts.";
                case CommercialBusiness.ImperialShippingCartel:
                    return "Transcontinental syndicate of merchant rail fleets and armored naval convoys ($1,000,000 cost, ~$160,000/sem). Controls essential wartime trade arteries.";
                case CommercialBusiness.CentralBankingHouseAndMint:
                    return "The absolute summit of oligarchic supremacy ($5,000,000 cost, ~$750,000/sem). Mints imperial coin, finances national war bonds, and wields kingmaker authority.";
                default:
                    return "No commercial business owned.";
            }
        }

        // ==================== FAMILY REAL ESTATE HELPERS ====================
        public static long GetFamilyHouseCost(FamilyHouseTier tier)
        {
            switch (tier)
            {
                case FamilyHouseTier.RuralHomestead:           return 5000L;
                case FamilyHouseTier.CityTownhouse:            return 25000L;
                case FamilyHouseTier.AncestralEstate:          return 100000L;
                case FamilyHouseTier.DucalPalaceCitadel:       return 500000L;
                case FamilyHouseTier.SovereignImperialChateau: return 2500000L;
                case FamilyHouseTier.ImperialDynasticSeat:     return 10000000L;
                default:                                       return 0L;
            }
        }

        public static string GetFamilyHouseDisplayName(FamilyHouseTier tier)
        {
            switch (tier)
            {
                case FamilyHouseTier.RuralHomestead:           return "Rural Family Homestead";
                case FamilyHouseTier.CityTownhouse:            return "City Family Townhouse";
                case FamilyHouseTier.AncestralEstate:          return "Ancestral Aristocratic Manor";
                case FamilyHouseTier.DucalPalaceCitadel:       return "High Ducal Palace Citadel";
                case FamilyHouseTier.SovereignImperialChateau: return "Sovereign Royal Château & Vineyards";
                case FamilyHouseTier.ImperialDynasticSeat:     return "The Eternal Dynastic Mountain Citadel";
                default:                                       return "None (Rationed Tenement)";
            }
        }

        public static string GetFamilyHouseDescription(FamilyHouseTier tier)
        {
            switch (tier)
            {
                case FamilyHouseTier.RuralHomestead:
                    return "A sturdy stone cottage surrounded by fertile arable farmland ($5,000). Guarantees your parents and siblings permanent security from wartime food shortages.";
                case FamilyHouseTier.CityTownhouse:
                    return "A stylish brick townhouse in the provincial merchant district ($25,000). Gives your family high social standing and access to elite municipal academies.";
                case FamilyHouseTier.AncestralEstate:
                    return "A sprawling aristocratic manor with manicured gardens and ancestral gatehouse ($100,000). Solidifies your family lineage as permanent regional nobility.";
                case FamilyHouseTier.DucalPalaceCitadel:
                    return "A fortified private citadel perched on high cliffs overlooking the capital ($500,000). Commands a private house guard and elevates your family to sovereign ducal standing.";
                case FamilyHouseTier.SovereignImperialChateau:
                    return "A legendary royal chateau sprawling across rolling vineyards and private estates ($2,500,000). Unlocks sovereign court privileges and historic generational dominance.";
                case FamilyHouseTier.ImperialDynasticSeat:
                    return "The legendary mountain redoubt carved into sheer granite cliffs ($10,000,000). The nearly unachievable zenith of multi-generational power, cementing your bloodline as immortal rulers of the realm.";
                default:
                    return "Your family lives in precarious wartime civilian housing.";
            }
        }
    }
}
