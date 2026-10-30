using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Official military decoration category classifications.
    /// </summary>
    public enum MedalCategory
    {
        ImperialValor,          // Highest national gallantry (Medal of Honor / Imperial Grand Star, DSC, Silver Cross)
        CombatValorAndMerit,    // Heroism and meritorious service under fire (Bronze Star, Valor Citation)
        WoundsAndCasualties,    // Purple Heart / Imperial Wound Star
        CampaignAndTheaters,    // Campaign service medals for major theaters of operation
        SpecialOperations,      // Elite commandos, airborne, reconnaissance
        AviationGallantry,      // Aerial combat, dogfights, flight decorations
        ServiceAndConduct,      // Good conduct, longevity service hashes, leadership merit
        CombatQualification     // Combat Infantry Badge, Marksman, Life Saver
    }

    /// <summary>
    /// Complete definition of an authentic military medal ribbon bar and decoration.
    /// </summary>
    [Serializable]
    public class MedalRibbonDefinition
    {
        public string medalId;
        public string displayName;
        public int precedenceOrder;                         // Lower number = higher military precedence (1 is highest)
        public string[] ribbonColors;                       // Hex color codes representing vertical color stripes
        public string criteriaDescription;
        public string historicalCitationTemplate;
        public MedalCategory category;
        public string deviceIcon = "";                      // Appurtenance symbol: "★", "V", "⚔", "🛡", "✚", etc.
        public string appurtenanceName = "None";            // Name of device: e.g. "Bronze Star Device", "Valor 'V' Device"
        public long imperialBounty = 0L;                    // Imperial Treasury cash honorarium

        public MedalRibbonDefinition() { }

        public MedalRibbonDefinition(
            string medalId,
            string displayName,
            int precedenceOrder,
            string[] ribbonColors,
            string criteriaDescription,
            string citationTemplate,
            MedalCategory category,
            string deviceIcon = "",
            string appurtenanceName = "None",
            long bounty = 0L)
        {
            this.medalId = medalId;
            this.displayName = displayName;
            this.precedenceOrder = precedenceOrder;
            this.ribbonColors = ribbonColors ?? new string[] { "#555555" };
            this.criteriaDescription = criteriaDescription;
            this.historicalCitationTemplate = citationTemplate;
            this.category = category;
            this.deviceIcon = deviceIcon;
            this.appurtenanceName = appurtenanceName;
            this.imperialBounty = bounty;
        }

        /// <summary>
        /// Converts the hex ribbon stripes into Unity Color structures.
        /// </summary>
        public Color[] GetColors()
        {
            if (ribbonColors == null || ribbonColors.Length == 0)
                return new Color[] { Color.gray };

            Color[] colors = new Color[ribbonColors.Length];
            for (int i = 0; i < ribbonColors.Length; i++)
            {
                if (ColorUtility.TryParseHtmlString(ribbonColors[i], out Color col))
                {
                    colors[i] = col;
                }
                else
                {
                    colors[i] = Color.gray;
                }
            }
            return colors;
        }

        /// <summary>
        /// Retrieves the hex code for a specific stripe index.
        /// </summary>
        public string GetStripeColor(int index)
        {
            if (ribbonColors == null || ribbonColors.Length == 0) return "#555555";
            int clamped = Mathf.Clamp(index, 0, ribbonColors.Length - 1);
            return ribbonColors[clamped];
        }
    }

    /// <summary>
    /// Persistent memorial tribute entry for a fireteam comrade killed in action.
    /// Inscribed on the Regimental Memorial Wall of Honor.
    /// </summary>
    [Serializable]
    public class FallenHeroMemorial
    {
        public string mateId;
        public string soldierName;
        public string nickname;
        public SquadRole role;
        public string roleTitle;
        public string rank;
        public string campaignFallen;
        public string causeOfDeath;
        public string finalWords;
        public string posthumousMedal;
        public int semesterFallen;
        public float ageFallen;
        public bool wreathLaid;
        public int tributesPaidCount;
        public string lastTributeDate;

        public FallenHeroMemorial() { }

        public FallenHeroMemorial(
            string mateId,
            string soldierName,
            string nickname,
            SquadRole role,
            string roleTitle,
            string rank,
            string campaignFallen,
            string causeOfDeath,
            string finalWords,
            string posthumousMedal,
            int semesterFallen,
            float ageFallen)
        {
            this.mateId = mateId;
            this.soldierName = soldierName;
            this.nickname = nickname;
            this.role = role;
            this.roleTitle = roleTitle;
            this.rank = string.IsNullOrEmpty(rank) ? "Private" : rank;
            this.campaignFallen = campaignFallen;
            this.causeOfDeath = causeOfDeath;
            this.finalWords = finalWords;
            this.posthumousMedal = posthumousMedal;
            this.semesterFallen = semesterFallen;
            this.ageFallen = ageFallen;
            this.wreathLaid = false;
            this.tributesPaidCount = 0;
            this.lastTributeDate = string.Empty;
        }

        /// <summary>
        /// Formatted military tribute moniker, e.g. "[Cpl] Raymond 'Doc' Miller".
        /// </summary>
        public string GetFullDisplayName()
        {
            string rankPrefix = string.IsNullOrEmpty(rank) ? "Pvt" : rank;
            return string.IsNullOrEmpty(nickname) 
                ? $"[{rankPrefix}] {soldierName}" 
                : $"[{rankPrefix}] {soldierName} \"{nickname}\"";
        }
    }

    /// <summary>
    /// Formatted official parchment citation accompanying an awarded decoration.
    /// </summary>
    [Serializable]
    public class AwardedMedalCitation
    {
        public string medalId;
        public string medalName;
        public string recipientName;
        public string recipientRank;
        public string theaterOfAction;
        public string dateAwarded;
        public string commandingOfficerName;
        public string commandingOfficerTitle;
        public string citationText;
        public string imperialSealCode;
        public int precedence;

        public AwardedMedalCitation() { }

        public AwardedMedalCitation(
            string medalId,
            string medalName,
            string recipientName,
            string recipientRank,
            string theaterOfAction,
            string dateAwarded,
            string commandingOfficerName,
            string commandingOfficerTitle,
            string citationText,
            string imperialSealCode,
            int precedence)
        {
            this.medalId = medalId;
            this.medalName = medalName;
            this.recipientName = recipientName;
            this.recipientRank = recipientRank;
            this.theaterOfAction = theaterOfAction;
            this.dateAwarded = dateAwarded;
            this.commandingOfficerName = commandingOfficerName;
            this.commandingOfficerTitle = commandingOfficerTitle;
            this.citationText = citationText;
            this.imperialSealCode = imperialSealCode;
            this.precedence = precedence;
        }
    }

    /// <summary>
    /// Centralized registry of all official military decorations, order of precedence,
    /// authentic ribbon patterns, criteria descriptions, and historical citation templates.
    /// </summary>
    public static class MedalRegistry
    {
        private static readonly Dictionary<string, MedalRibbonDefinition> _definitions = 
            new Dictionary<string, MedalRibbonDefinition>(StringComparer.OrdinalIgnoreCase);

        static MedalRegistry()
        {
            InitializeRegistry();
        }

        private static void InitializeRegistry()
        {
            _definitions.Clear();

            // =========================================================================================
            // 1. HIGHEST MILITARY VALOR (Precedence 1 - 10)
            // =========================================================================================

            Register(new MedalRibbonDefinition(
                "MEDAL_OF_HONOR",
                "Grand Imperial Star",
                1,
                new string[] { "#0B2F64", "#D4AF37", "#0B2F64", "#D4AF37", "#0B2F64" },
                "The highest military decoration awarded for conspicuous gallantry and intrepidity at the risk of life above and beyond the call of duty in action against an armed enemy of the Empire.",
                "For conspicuous gallantry, dauntless valor, and intrepidity at the risk of his life above and beyond the call of duty. In the fierce crucible of combat at {Theater}, {Rank} {Name} stood solitary against overwhelming hostile forces, single-handedly rallying shattered frontline defensive lines and securing critical strategic victory at the imminent hazard of mortal demise. His magnificent courage and steadfast devotion reflect supreme glory upon himself and the Imperial Armed Forces.",
                MedalCategory.ImperialValor,
                "★",
                "Imperial Constellation Star",
                75000L
            ));

            Register(new MedalRibbonDefinition(
                "DISTINGUISHED_SERVICE_CROSS",
                "Distinguished Service Cross",
                2,
                new string[] { "#0F3E76", "#FFFFFF", "#C41E3A", "#FFFFFF", "#0F3E76" },
                "Awarded for extraordinary heroism in connection with military operations against an armed enemy of the Empire, involving actions of such caliber that clearly set the recipient apart from his comrades.",
                "For extraordinary heroism in frontline combat operations. During severe hostile counter-offensive assaults at {Theater}, {Rank} {Name} demonstrated unparalleled martial tenacity, spearheading assault elements through entrenched enemy bunkers and preserving the tactical integrity of the regiment under lethal artillery bombardment.",
                MedalCategory.ImperialValor,
                "⚔",
                "Imperial Oak Leaves",
                30000L
            ));

            Register(new MedalRibbonDefinition(
                "SILVER_CROSS",
                "Silver Cross",
                3,
                new string[] { "#FFFFFF", "#8C9BAE", "#0A1C3E", "#8C9BAE", "#FFFFFF" },
                "Awarded for gallantry in action against hostile armed forces during frontline combat operations.",
                "For gallantry in action during intense combat operations at {Theater}. When his fireteam was pinned down by overlapping machine-gun crossfire, {Rank} {Name} boldly exposed himself to hostile fire, neutralizing fortified pillboxes with high-explosive ordnance and enabling the tactical breakout of friendly units.",
                MedalCategory.ImperialValor,
                "★",
                "Silver Star Device",
                15000L
            ));

            Register(new MedalRibbonDefinition(
                "BRONZE_STAR",
                "Bronze Star",
                4,
                new string[] { "#8B0000", "#FFFFFF", "#0F3E76", "#FFFFFF", "#8B0000" },
                "Awarded for heroic or meritorious achievement and service in frontline combat operations with the armed forces.",
                "For heroic achievement in combat operations at {Theater}. {Rank} {Name} exhibited exceptional tactical initiative and courage under fire, coordinating concentrated squad fire and successfully defending vital defensive sectors against repeated enemy assaults.",
                MedalCategory.CombatValorAndMerit,
                "★",
                "Bronze Star Device",
                6000L
            ));

            Register(new MedalRibbonDefinition(
                "IMPERIAL_WOUND_STAR",
                "Imperial Wound Star",
                5,
                new string[] { "#FFFFFF", "#4A0E4E", "#4A0E4E", "#4A0E4E", "#FFFFFF" },
                "Awarded to members of the Armed Forces who have been wounded or killed in combat against an enemy of the Empire. (Purple Heart equivalent)",
                "For military merit and combat wounds suffered in the line of duty against hostile enemy forces during offensive operations in {Theater}. Despite severe trauma and physical injury, {Rank} {Name} refused evacuation until his fireteam was safely withdrawn from the lethal kill-zone.",
                MedalCategory.WoundsAndCasualties,
                "★",
                "Imperial Wound Star",
                3000L
            ));

            Register(new MedalRibbonDefinition(
                "VALOR_CITATION",
                "Valor Citation",
                6,
                new string[] { "#C41E3A", "#D4AF37", "#C41E3A", "#D4AF37", "#C41E3A" },
                "Conferred for distinguished gallantry and tactical initiative under hostile fire during frontline combat engagements.",
                "For gallantry under fire during combat operations at {Theater}. {Rank} {Name} provided decisive leadership and aggressive suppression fire, repelling hostile flanking assaults and safeguarding his platoon.",
                MedalCategory.CombatValorAndMerit,
                "V",
                "Combat Valor 'V' Device",
                3500L
            ));

            Register(new MedalRibbonDefinition(
                "SPECIAL_OPS_CROSS",
                "Special Operations Valor Cross",
                7,
                new string[] { "#1A1A1A", "#4A4A4A", "#8B0000", "#4A4A4A", "#1A1A1A" },
                "Conferred upon elite commandos and special forces operators for clandestine combat valor behind enemy lines.",
                "For extraordinary gallantry in high-risk special reconnaissance and direct-action combat deep behind enemy lines. {Rank} {Name} executed mission-critical sabotage and precision hostage rescue under extreme peril, upholding the highest traditions of the Imperial Special Forces.",
                MedalCategory.SpecialOperations,
                "⚔",
                "Commando Dagger",
                20000L
            ));

            Register(new MedalRibbonDefinition(
                "DISTINGUISHED_FLYING_CROSS",
                "Distinguished Flying Cross",
                8,
                new string[] { "#2B5B84", "#FFFFFF", "#D4AF37", "#FFFFFF", "#2B5B84" },
                "Awarded for heroism and extraordinary achievement while participating in aerial dogfights and frontline combat flight missions.",
                "For heroism and exceptional airmanship while participating in frontline aerial combat missions. {Rank} {Name} engaged hostile interceptor formations with supreme tactical acumen, destroying enemy aircraft and ensuring total airspace superiority.",
                MedalCategory.AviationGallantry,
                "★",
                "Valkyrie Wings",
                15000L
            ));

            Register(new MedalRibbonDefinition(
                "IRON_VANGUARD_CROSS",
                "Iron Vanguard Cross",
                9,
                new string[] { "#333333", "#8B0000", "#D4AF37", "#8B0000", "#333333" },
                "Awarded for unyielding grit and staunch trench defense during relentless enemy counter-assaults and artillery sieges.",
                "For unyielding tenacity and resolute defiance in the defense of frontline positions at {Theater}. Subjected to uninterrupted heavy artillery fire, {Rank} {Name} maintained defensive weapon stations and halted the enemy's spearhead.",
                MedalCategory.CombatValorAndMerit,
                "🛡",
                "Iron Shield Device",
                10000L
            ));

            Register(new MedalRibbonDefinition(
                "LIFE_SAVING_CROSS",
                "Imperial Life Saving Cross",
                10,
                new string[] { "#1B4332", "#FFFFFF", "#C41E3A", "#FFFFFF", "#1B4332" },
                "Conferred upon personnel who perform heroic emergency battlefield trauma triage and rescue, saving comrades under lethal enemy fire.",
                "For heroism in saving life under fire. During heavy bombardment at {Theater}, {Rank} {Name} crossed open exposed ground to administer emergency trauma bandages and tourniquets to critically wounded squadmates, carrying them to safety.",
                MedalCategory.CombatQualification,
                "✚",
                "Red Cross Device",
                10000L
            ));

            // =========================================================================================
            // 2. THEATERS OF WAR CAMPAIGN MEDALS (Precedence 20 - 29)
            // =========================================================================================

            Register(new MedalRibbonDefinition(
                "ARDEN_RIDGE_CAMPAIGN",
                "Arden Ridge Campaign",
                20,
                new string[] { "#4A3B32", "#556B2F", "#8B0000", "#556B2F", "#4A3B32" },
                "Awarded for active frontline service in the brutal artillery barrages, trench networks, and razor wire of the Arden Ridge Trench Lines.",
                "For honorable frontline combat service in the Arden Ridge Campaign. {Rank} {Name} endured merciless trench warfare, freezing mud, and sustained creeping artillery barrages in defense of the Imperial border.",
                MedalCategory.CampaignAndTheaters,
                "★",
                "Campaign Battle Star",
                2500L
            ));

            Register(new MedalRibbonDefinition(
                "PORT_IRON_BEACHHEAD",
                "Port Iron Beachhead",
                21,
                new string[] { "#2F4F4F", "#1E3F66", "#E6C229", "#1E3F66", "#2F4F4F" },
                "Awarded for participating in the perilous amphibious assault and beachhead fortifications against the reinforced concrete sea-walls of Port Iron.",
                "For distinguished service during the amphibious invasion of Port Iron. {Rank} {Name} breached coastal fortifications and secured tactical landing zones against murderous crossfire.",
                MedalCategory.CampaignAndTheaters,
                "★",
                "Campaign Battle Star",
                3000L
            ));

            Register(new MedalRibbonDefinition(
                "KAELEN_URBAN_PACIFICATION",
                "Kaelen Urban Pacification",
                22,
                new string[] { "#3D3D3D", "#B22222", "#C0C0C0", "#B22222", "#3D3D3D" },
                "Awarded for close-quarters room-clearing, barricade combat, and counter-insurgency throughout the ruined boulevards of Kaelen Valley.",
                "For combat operations in the Kaelen Valley Urban Zone. {Rank} {Name} cleared sniper nests, neutralized fortified city blocks, and restored military order under perilous urban warfare conditions.",
                MedalCategory.CampaignAndTheaters,
                "★",
                "Campaign Battle Star",
                2500L
            ));

            Register(new MedalRibbonDefinition(
                "BADLANDS_STEPPE_PATROL",
                "Badlands Steppe Patrol",
                23,
                new string[] { "#C2B280", "#8B4513", "#FFD700", "#8B4513", "#C2B280" },
                "Awarded for extended motorized reconnaissance, convoy escorts, and skirmishes across the arid dust flats of the Badlands Steppe.",
                "For service in long-range desert patrol and reconnaissance operations across the Badlands Steppe. {Rank} {Name} survived extreme heat, ambushes, and hostile armor engagements.",
                MedalCategory.CampaignAndTheaters,
                "★",
                "Campaign Battle Star",
                2500L
            ));

            Register(new MedalRibbonDefinition(
                "NORTHERN_TAIGA_DEFENSE",
                "Northern Taiga Winter Defense",
                24,
                new string[] { "#FFFFFF", "#1C3B2B", "#4682B4", "#1C3B2B", "#FFFFFF" },
                "Awarded for holding fortified sub-zero outposts, enduring blizzards, and combating elite winter raiders in the Northern Taiga.",
                "For resolute endurance and valor during the sub-zero winter defense of Northern Taiga outposts. {Rank} {Name} held fortified watchposts against hostile winter storm attacks.",
                MedalCategory.CampaignAndTheaters,
                "★",
                "Campaign Battle Star",
                2500L
            ));

            // =========================================================================================
            // 3. SERVICE, CONDUCT & BADGES (Precedence 30 - 40)
            // =========================================================================================

            Register(new MedalRibbonDefinition(
                "GOOD_CONDUCT_MEDAL",
                "Valerian Merit Ribbon",
                30,
                new string[] { "#0A1C3E", "#D4AF37", "#0A1C3E" },
                "Conferred for exemplary fidelity, superior military discipline, and honorable active duty service in the Imperial Armed Forces.",
                "For exemplary fidelity and military conduct throughout consecutive combat deployments. {Rank} {Name} has served with spotless honor, inspiring comrades through unwavering dedication.",
                MedalCategory.ServiceAndConduct,
                "",
                "None",
                2000L
            ));

            Register(new MedalRibbonDefinition(
                "LONGEVITY_SERVICE_HASH",
                "Longevity Service Hash",
                31,
                new string[] { "#0D1B2A", "#D4AF37", "#0D1B2A", "#D4AF37", "#0D1B2A" },
                "Signifies three full years (6 semesters) of continuous honorable active military service in defense of the Empire.",
                "In recognition of sustained, honorable, and devoted service in the Imperial Armed Forces for three continuous years. Conferred upon {Rank} {Name} with the deepest gratitude of the Emperor.",
                MedalCategory.ServiceAndConduct,
                "═",
                "Gold Service Hash",
                2000L
            ));

            Register(new MedalRibbonDefinition(
                "COMBAT_INFANTRY_BADGE",
                "Combat Infantry Badge",
                32,
                new string[] { "#4682B4", "#FFFFFF", "#4682B4" },
                "Awarded to infantry riflemen who have personally engaged the enemy in active ground combat under hostile fire.",
                "For personal participation in close-quarters ground combat against hostile enemy infantry forces. Conferred upon {Rank} {Name} for direct battlefield engagement.",
                MedalCategory.CombatQualification,
                "⚔",
                "Combat Musket Insignia",
                2500L
            ));

            Register(new MedalRibbonDefinition(
                "ACADEMY_HONORS_SABER",
                "Academy Honors Saber",
                33,
                new string[] { "#7A1C1C", "#FFD700", "#0A1C3E", "#FFD700", "#7A1C1C" },
                "Conferred upon commissioned officers graduating in the top 10 percentile of the Imperial War Academy.",
                "For academic supremacy, tactical excellence, and superior leadership demonstrated at the Imperial War Academy. Conferred upon {Rank} {Name} upon receiving his officer commission.",
                MedalCategory.ServiceAndConduct,
                "⚔",
                "Golden Saber Device",
                5000L
            ));

            Register(new MedalRibbonDefinition(
                "MARKSMAN_COMMENDATION",
                "Marksman Commendation",
                34,
                new string[] { "#2D5A27", "#FFFFFF", "#2D5A27" },
                "Awarded for exceptional battlefield marksmanship and precision rifle engagement.",
                "For superior marksmanship skills and battlefield precision under hostile fire at {Theater}. {Rank} {Name} eliminated high-value enemy targets and suppressed advancing hostile infantry.",
                MedalCategory.CombatQualification,
                "🎯",
                "Bullseye Device",
                2000L
            ));
        }

        private static void Register(MedalRibbonDefinition def)
        {
            if (def != null && !string.IsNullOrEmpty(def.medalId))
            {
                _definitions[def.medalId] = def;
                _definitions[def.displayName] = def;
            }
        }

        /// <summary>
        /// Retrieves the registered ribbon definition with flexible fuzzy name and alias matching.
        /// Guaranteed to never return null (returns a handsome military fallback if unrecognized).
        /// </summary>
        public static MedalRibbonDefinition GetDefinition(string rawMedalName)
        {
            if (string.IsNullOrEmpty(rawMedalName))
            {
                return GetDefaultFallback("Military Commendation");
            }

            string clean = rawMedalName.Trim();

            // Direct key match
            if (_definitions.TryGetValue(clean, out var exact))
            {
                return exact;
            }

            // Normalization of aliases across the codebase
            if (clean.IndexOf("Medal of Honor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Grand Imperial Star", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["MEDAL_OF_HONOR"];

            if (clean.IndexOf("Distinguished Service Cross", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["DISTINGUISHED_SERVICE_CROSS"];

            if (clean.IndexOf("Silver Vanguard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Silver Star", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Silver Cross", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["SILVER_CROSS"];

            if (clean.IndexOf("Bronze Star", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["BRONZE_STAR"];

            if (clean.IndexOf("Wound Star", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Purple Heart", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["IMPERIAL_WOUND_STAR"];

            if (clean.IndexOf("Valor Citation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Imperial Star of Valor", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["VALOR_CITATION"];

            if (clean.IndexOf("Special Operations", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Commando Ribbon", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["SPECIAL_OPS_CROSS"];

            if (clean.IndexOf("Flying Cross", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Valkyrie Wings", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["DISTINGUISHED_FLYING_CROSS"];

            if (clean.IndexOf("Iron Vanguard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Hero of the Trenches", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Falcon", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["IRON_VANGUARD_CROSS"];

            if (clean.IndexOf("Life Saving", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["LIFE_SAVING_CROSS"];

            if (clean.IndexOf("Combat Infantry", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["COMBAT_INFANTRY_BADGE"];

            if (clean.IndexOf("Academy Honors", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["ACADEMY_HONORS_SABER"];

            if (clean.IndexOf("Marksman", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["MARKSMAN_COMMENDATION"];

            // Campaign aliases
            if (clean.IndexOf("Arden", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["ARDEN_RIDGE_CAMPAIGN"];

            if (clean.IndexOf("Port Iron", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["PORT_IRON_BEACHHEAD"];

            if (clean.IndexOf("Kaelen", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["KAELEN_URBAN_PACIFICATION"];

            if (clean.IndexOf("Badlands", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["BADLANDS_STEPPE_PATROL"];

            if (clean.IndexOf("Taiga", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["NORTHERN_TAIGA_DEFENSE"];

            if (clean.IndexOf("Longevity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Service Hash", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["LONGEVITY_SERVICE_HASH"];

            if (clean.IndexOf("Good Conduct", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Valerian Merit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clean.IndexOf("Merit Ribbon", StringComparison.OrdinalIgnoreCase) >= 0)
                return _definitions["GOOD_CONDUCT_MEDAL"];

            return GetDefaultFallback(clean);
        }

        private static MedalRibbonDefinition GetDefaultFallback(string medalName)
        {
            return new MedalRibbonDefinition(
                "CUSTOM_" + medalName.Replace(" ", "_").ToUpper(),
                medalName,
                50,
                new string[] { "#1E3F66", "#D4AF37", "#1E3F66" },
                $"Official military commendation: {medalName}.",
                $"For distinguished service and dedicated devotion to duty. Conferred upon {{Rank}} {{Name}} for meritorious actions in the defense of the Valerian Empire.",
                MedalCategory.CombatValorAndMerit,
                "★",
                "Commendation Star",
                2000L
            );
        }

        /// <summary>
        /// Returns all official military decorations in the registry, sorted by precedence.
        /// </summary>
        public static List<MedalRibbonDefinition> GetAllOfficialMedals()
        {
            var list = new List<MedalRibbonDefinition>();
            var seen = new HashSet<string>();

            foreach (var kvp in _definitions)
            {
                if (!seen.Contains(kvp.Value.medalId))
                {
                    seen.Add(kvp.Value.medalId);
                    list.Add(kvp.Value);
                }
            }

            list.Sort((a, b) => a.precedenceOrder.CompareTo(b.precedenceOrder));
            return list;
        }

        /// <summary>
        /// Compatibility alias for GetAllOfficialMedals().
        /// </summary>
        public static List<MedalRibbonDefinition> GetAllMedals() => GetAllOfficialMedals();
    }
}
