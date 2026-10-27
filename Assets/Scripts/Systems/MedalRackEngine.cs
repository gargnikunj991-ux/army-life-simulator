using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Represents a single horizontal row of mounted ribbons on the military uniform rack.
    /// Following military protocol, ribbons are mounted up to 3 per row.
    /// </summary>
    public class MountedRibbonRow
    {
        public int rowIndex;
        public List<MedalRibbonDefinition> ribbons = new List<MedalRibbonDefinition>();

        public MountedRibbonRow(int index)
        {
            this.rowIndex = index;
            this.ribbons = new List<MedalRibbonDefinition>();
        }
    }

    /// <summary>
    /// Core simulation engine responsible for:
    /// 1. Building ordered military ribbon racks based on official military order of precedence.
    /// 2. Formatting authentic historical parchment citations with commanding officer signatures and imperial seals.
    /// 3. Recording and maintaining the solemn Regimental Memorial Wall for squadmates killed in action.
    /// </summary>
    public static class MedalRackEngine
    {
        public const int MAX_RIBBONS_PER_ROW = 3;
        public const long WREATH_TRIBUTE_COST = 50L;

        // In-memory persistent cache for Memorial Wall records
        private static readonly Dictionary<string, List<FallenHeroMemorial>> _memorialRegistry = 
            new Dictionary<string, List<FallenHeroMemorial>>();

        // =========================================================================================
        // 1. RIBBON RACK ASSEMBLY & PRECEDENCE ORDERING
        // =========================================================================================

        /// <summary>
        /// Retrieves all ribbons earned by the soldier, sorted in strict order of military precedence
        /// (Precedence #1 on top-left, descending to lower precedence honors).
        /// </summary>
        public static List<MedalRibbonDefinition> GetSortedRibbons(SoldierProfile soldier)
        {
            if (soldier == null) return new List<MedalRibbonDefinition>();

            var collectedDefinitions = new Dictionary<string, MedalRibbonDefinition>(StringComparer.OrdinalIgnoreCase);

            // 1. Process explicit earned medals
            if (soldier.earnedMedals != null)
            {
                foreach (var medalStr in soldier.earnedMedals)
                {
                    if (string.IsNullOrEmpty(medalStr)) continue;
                    var def = MedalRegistry.GetDefinition(medalStr);
                    if (def != null && !collectedDefinitions.ContainsKey(def.medalId))
                    {
                        collectedDefinitions[def.medalId] = def;
                    }
                }
            }

            // 2. Process inherited medals from family legacy
            if (soldier.inheritedMedals != null)
            {
                foreach (var medalStr in soldier.inheritedMedals)
                {
                    if (string.IsNullOrEmpty(medalStr)) continue;
                    var def = MedalRegistry.GetDefinition(medalStr);
                    if (def != null && !collectedDefinitions.ContainsKey(def.medalId))
                    {
                        collectedDefinitions[def.medalId] = def;
                    }
                }
            }

            // 3. Ensure Imperial Wound Star is present if soldier has recorded combat wounds
            int woundCount = CalculateWoundStars(soldier);
            if (woundCount > 0 && !collectedDefinitions.ContainsKey("IMPERIAL_WOUND_STAR"))
            {
                var woundDef = MedalRegistry.GetDefinition("Imperial Wound Star");
                collectedDefinitions[woundDef.medalId] = woundDef;
            }

            // 4. Check for Campaign Medals based on tours survived
            if (soldier.toursInCurrentTheater > 0)
            {
                string campaignKey = GetCampaignMedalKeyForTheater(soldier.currentTheater);
                if (!string.IsNullOrEmpty(campaignKey) && !collectedDefinitions.ContainsKey(campaignKey))
                {
                    var campaignDef = MedalRegistry.GetDefinition(campaignKey);
                    collectedDefinitions[campaignDef.medalId] = campaignDef;
                }
            }

            // 5. Longevity Service Hashes (Every 6 semesters = 3 years honorable active duty)
            int longevityHashes = CalculateLongevityHashes(soldier);
            if (longevityHashes > 0 && !collectedDefinitions.ContainsKey("LONGEVITY_SERVICE_HASH"))
            {
                var hashDef = MedalRegistry.GetDefinition("Longevity Service Hash");
                collectedDefinitions[hashDef.medalId] = hashDef;
            }

            // Convert to list and sort strictly by precedence order (ascending: 1 = top)
            var sortedList = new List<MedalRibbonDefinition>(collectedDefinitions.Values);
            sortedList.Sort((a, b) => a.precedenceOrder.CompareTo(b.precedenceOrder));

            return sortedList;
        }

        /// <summary>
        /// Mounts ribbons into standardized military rows of up to 3 ribbons per row.
        /// When the total count is not divisible by 3, standard military mounting protocol places the
        /// remainder on the top row centered (e.g. 5 ribbons: top row has 2, bottom row has 3).
        /// </summary>
        public static List<MountedRibbonRow> BuildRibbonRack(SoldierProfile soldier)
        {
            var sortedRibbons = GetSortedRibbons(soldier);
            var rows = new List<MountedRibbonRow>();

            int total = sortedRibbons.Count;
            if (total == 0) return rows;

            int remainder = total % MAX_RIBBONS_PER_ROW;
            int topRowCount = remainder == 0 ? MAX_RIBBONS_PER_ROW : remainder;

            int ribbonIndex = 0;
            int rowIndex = 0;

            // Build Top Row (contains remainder or full 3)
            var topRow = new MountedRibbonRow(rowIndex++);
            for (int i = 0; i < topRowCount && ribbonIndex < total; i++)
            {
                topRow.ribbons.Add(sortedRibbons[ribbonIndex++]);
            }
            rows.Add(topRow);

            // Build subsequent standard 3-ribbon rows
            while (ribbonIndex < total)
            {
                var row = new MountedRibbonRow(rowIndex++);
                for (int i = 0; i < MAX_RIBBONS_PER_ROW && ribbonIndex < total; i++)
                {
                    row.ribbons.Add(sortedRibbons[ribbonIndex++]);
                }
                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// Calculates the number of 3-year service longevity stripes earned.
        /// Each 3 years of honorable service (6 semesters) grants 1 gold hash.
        /// </summary>
        public static int CalculateLongevityHashes(SoldierProfile soldier)
        {
            if (soldier == null) return 0;
            return Mathf.Max(0, soldier.totalSemestersLived / 6);
        }

        /// <summary>
        /// Calculates total effective combat wounds / wound stars.
        /// </summary>
        public static int CalculateWoundStars(SoldierProfile soldier)
        {
            if (soldier == null) return 0;
            return soldier.GetWoundStarsCount();
        }

        private static string GetCampaignMedalKeyForTheater(WarTheaterType theater)
        {
            switch (theater)
            {
                case WarTheaterType.ArdenRidgeTrenchLines:
                case WarTheaterType.WesternMudTrenches:
                    return "Arden Ridge Campaign";

                case WarTheaterType.PortIronAmphibiousFront:
                case WarTheaterType.PacificJungleIslands:
                    return "Port Iron Beachhead";

                case WarTheaterType.KaelenValleyUrbanZone:
                case WarTheaterType.UrbanCitadel:
                    return "Kaelen Urban Pacification";

                case WarTheaterType.BadlandsSteppeRecon:
                    return "Badlands Steppe Patrol";

                case WarTheaterType.NorthernTaigaOutpost:
                case WarTheaterType.EasternFrozenSteppe:
                    return "Northern Taiga Winter Defense";

                default:
                    return "Arden Ridge Campaign";
            }
        }

        // =========================================================================================
        // 2. HISTORICAL CITATIONS DOSSIER GENERATOR
        // =========================================================================================

        /// <summary>
        /// Generates a complete official parchment citation for a decorated medal.
        /// </summary>
        public static AwardedMedalCitation GenerateCitation(SoldierProfile soldier, string medalName, string theaterName = null)
        {
            var def = MedalRegistry.GetDefinition(medalName);
            string recipientName = soldier != null ? soldier.characterName : "Unknown Soldier";
            string recipientRank = soldier != null && soldier.currentRank != null 
                ? soldier.currentRank.rankName 
                : "Private First Class";

            string activeTheater = !string.IsNullOrEmpty(theaterName)
                ? theaterName
                : (soldier != null ? WarTheaterInfo.GetInfo(soldier.currentTheater).theaterName : "The Western Front");

            int year = 940 + (soldier != null ? soldier.totalSemestersLived / 2 : 2);
            int semester = (soldier != null ? (soldier.totalSemestersLived % 2) + 1 : 1);
            string dateAwarded = $"Semester {semester}, Imperial Year {year}";

            string coName;
            string coTitle;

            if (soldier != null && soldier.militaryBranch == MilitaryBranch.AirForce)
            {
                coName = "Air Marshal Catherine Sterling";
                coTitle = "High Command, Imperial Tactical Air Corps";
            }
            else
            {
                coName = "Major General Julian Vance";
                coTitle = "Commanding General, 7th Imperial Vanguard Division";
            }

            // Fill template variables
            string rawTemplate = !string.IsNullOrEmpty(def.historicalCitationTemplate)
                ? def.historicalCitationTemplate
                : "For distinguished valor and exemplary devotion to duty in action against hostile armed forces.";

            string formattedCitation = rawTemplate
                .Replace("{Rank}", recipientRank)
                .Replace("{Name}", recipientName)
                .Replace("{Theater}", activeTheater)
                .Replace("{Date}", dateAwarded)
                .Replace("{CO}", coName);

            int hash = Math.Abs((recipientName + def.medalId).GetHashCode());
            string sealCode = $"SEAL-ORD-{hash % 90000 + 10000}-VAL";

            return new AwardedMedalCitation(
                def.medalId,
                def.displayName,
                recipientName,
                recipientRank,
                activeTheater,
                dateAwarded,
                coName,
                coTitle,
                formattedCitation,
                sealCode,
                def.precedenceOrder
            );
        }

        /// <summary>
        /// Generates citations for all unique medals earned by the soldier, sorted by precedence.
        /// </summary>
        public static List<AwardedMedalCitation> GetAllCitations(SoldierProfile soldier)
        {
            var sortedRibbons = GetSortedRibbons(soldier);
            var citations = new List<AwardedMedalCitation>();

            foreach (var rib in sortedRibbons)
            {
                citations.Add(GenerateCitation(soldier, rib.displayName));
            }

            return citations;
        }

        // =========================================================================================
        // 3. REGIMENTAL MEMORIAL WALL MANAGEMENT
        // =========================================================================================

        /// <summary>
        /// Retrieves the Memorial Wall records for a soldier, automatically synchronizing with any
        /// fallen squadmates from soldier.squadMates.
        /// </summary>
        public static List<FallenHeroMemorial> GetMemorialWall(SoldierProfile soldier)
        {
            if (soldier == null) return new List<FallenHeroMemorial>();

            string profileKey = string.IsNullOrEmpty(soldier.lifeId) ? "CURRENT_LIFE" : soldier.lifeId;
            if (!_memorialRegistry.TryGetValue(profileKey, out var memorials))
            {
                memorials = new List<FallenHeroMemorial>();
                _memorialRegistry[profileKey] = memorials;
            }

            // Synchronize from soldier.squadMates for any fallen comrades
            if (soldier.squadMates != null)
            {
                foreach (var mate in soldier.squadMates)
                {
                    if (mate != null && mate.isDead)
                    {
                        if (!memorials.Exists(m => m.mateId == mate.id))
                        {
                            var entry = CreateMemorialEntryFromSquadmate(soldier, mate);
                            memorials.Add(entry);
                        }
                    }
                }
            }

            return memorials;
        }

        private static FallenHeroMemorial CreateMemorialEntryFromSquadmate(SoldierProfile soldier, SquadMate mate)
        {
            string rank;
            string cause;
            string finalWords;
            string medal = "Imperial Wound Star (Posthumous)";

            switch (mate.role)
            {
                case SquadRole.CombatMedic:
                    rank = "Corporal";
                    cause = !string.IsNullOrEmpty(mate.deathReason) 
                        ? mate.deathReason 
                        : "Mortar shrapnel while shielding a wounded recruit during a triage evacuation.";
                    finalWords = "\"Patch up the recruit first... don't worry about me, brother...\"";
                    medal = "Imperial Life Saving Cross (Posthumous)";
                    break;

                case SquadRole.LeadScout:
                    rank = "Sergeant";
                    cause = !string.IsNullOrEmpty(mate.deathReason) 
                        ? mate.deathReason 
                        : "Counter-sniper fire while covering the fireteam's withdrawal across No Man's Land.";
                    finalWords = "\"Hostile sniper in the clock tower... keep your heads down, boys...\"";
                    medal = "Silver Cross (Posthumous)";
                    break;

                case SquadRole.HeavyGunner:
                    rank = "Specialist";
                    cause = !string.IsNullOrEmpty(mate.deathReason) 
                        ? mate.deathReason 
                        : "Held the trench crest against three waves of enemy shock troops with an overheated machine gun.";
                    finalWords = "\"Keep firing! Don't let them breach the trench line!\"";
                    medal = "Distinguished Service Cross (Posthumous)";
                    break;

                case SquadRole.RiflemanRookie:
                default:
                    rank = "Private";
                    cause = !string.IsNullOrEmpty(mate.deathReason) 
                        ? mate.deathReason 
                        : "Fell to direct enemy fire during the frontal bayonet charge.";
                    finalWords = "\"Tell my family I stood tall and did my duty for the Empire...\"";
                    medal = "Imperial Wound Star (Posthumous)";
                    break;
            }

            string theaterName = WarTheaterInfo.GetInfo(soldier.currentTheater).theaterName;
            int semester = soldier.totalSemestersLived;
            float age = soldier.age;

            return new FallenHeroMemorial(
                mate.id,
                mate.name,
                mate.nickname,
                mate.role,
                mate.roleTitle,
                rank,
                theaterName,
                cause,
                finalWords,
                medal,
                semester,
                age
            );
        }

        /// <summary>
        /// Explicitly registers a squadmate casualty on the Memorial Wall and dispatches narrative events.
        /// </summary>
        public static void RecordSquadmateCasualty(
            SoldierProfile soldier, 
            SquadMate fallenMate, 
            string campaign = null, 
            string cause = null, 
            string finalWords = null)
        {
            if (soldier == null || fallenMate == null) return;

            fallenMate.isDead = true;
            if (!string.IsNullOrEmpty(cause)) fallenMate.deathReason = cause;

            soldier.squadMatesKIA++;

            string profileKey = string.IsNullOrEmpty(soldier.lifeId) ? "CURRENT_LIFE" : soldier.lifeId;
            if (!_memorialRegistry.TryGetValue(profileKey, out var memorials))
            {
                memorials = new List<FallenHeroMemorial>();
                _memorialRegistry[profileKey] = memorials;
            }

            if (!memorials.Exists(m => m.mateId == fallenMate.id))
            {
                var entry = CreateMemorialEntryFromSquadmate(soldier, fallenMate);
                if (!string.IsNullOrEmpty(campaign)) entry.campaignFallen = campaign;
                if (!string.IsNullOrEmpty(cause)) entry.causeOfDeath = cause;
                if (!string.IsNullOrEmpty(finalWords)) entry.finalWords = finalWords;

                memorials.Add(entry);
            }

            GameEvents.OnFeedEventLogged?.Invoke("CASUALTY",
                $"<color=#FF4444><b>[SQUAD CASUALTY • KIA]</b></color> Your comrade <b>{fallenMate.GetDisplayName()}</b> " +
                $"was killed in action during frontline operations. His name has been permanently inscribed upon the <b>Regimental Memorial Wall</b>.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        /// <summary>
        /// Pays solemn respects and lays a laurel memorial wreath at the Memorial Wall for a fallen brother.
        /// Boosts morale (+15), squad respect (+10), and records tribute history.
        /// </summary>
        public static bool LayWreathAtMemorial(SoldierProfile soldier, FallenHeroMemorial hero)
        {
            if (soldier == null || hero == null) return false;

            // Deduct wreath cost if funds are available
            if (soldier.cashBalance >= WREATH_TRIBUTE_COST)
            {
                soldier.cashBalance -= WREATH_TRIBUTE_COST;
            }

            hero.wreathLaid = true;
            hero.tributesPaidCount++;
            hero.lastTributeDate = $"Semester {soldier.totalSemestersLived}";

            soldier.happiness = Mathf.Min(100f, soldier.happiness + 15f);
            soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 10f);

            GameEvents.OnFeedEventLogged?.Invoke("MEMORIAL",
                $"<color=#FFD700><b>[REGIMENTAL MEMORIAL • WREATH LAID]</b></color> You visited the marble wall of honor and laid a wreath of white poppies " +
                $"for <b>{hero.GetFullDisplayName()}</b>. <i>\"They shall grow not old, as we that are left grow old: Age shall not weary them, nor the years condemn.\"</i> " +
                $"(<color=#00FF88>+15 Morale</color>, <color=#00D4FF>+10 Squad Respect</color>).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        /// <summary>
        /// Resets the in-memory memorial cache (useful for testing and new game cycles).
        /// </summary>
        public static void ClearMemorialRegistry()
        {
            _memorialRegistry.Clear();
        }

        // =========================================================================================
        // 4. MILITARY PYRAMID OF VALOR: STRICT MEDAL EVALUATION ENGINE
        // =========================================================================================

        /// <summary>
        /// Evaluates battlefield performance against the strict Military Pyramid of Valor hierarchy:
        /// Level 1 (Precedence #1): Medal of Honor / Grand Imperial Star - Supreme valor at risk of life
        /// Level 2 (Precedence #2): Distinguished Service Cross - Extraordinary heroism against hostile forces
        /// Level 3 (Precedence #3): Silver Star / Silver Cross - Conspicuous gallantry in action
        /// Level 4 (Precedence #4): Bronze Star - Meritorious combat achievement under fire
        /// Level 5 (Precedence #5): Purple Heart / Imperial Wound Star - STRICT REQUIREMENT: Recipient must be wounded in combat
        /// Level 6 (Precedence #6): Valor Citation - Tactical initiative and gallantry under fire
        /// Level 7 (Precedence #32): Combat Infantry Badge - First active ground combat engagement
        /// Level 8 (Precedence #20-24): Campaign Medals - Completing combat tours in active theater
        /// </summary>
        public static List<string> EvaluateBattlefieldMedalAwards(
            SoldierProfile soldier, 
            CombatOperationOutcome outcome, 
            TacticalMission mission, 
            WarTheaterInfo theater,
            bool hasRescuedComrade = false,
            bool hasSlayedNemesis = false)
        {
            var newHonors = new List<string>();
            if (soldier == null || outcome == null) return newHonors;

            // =========================================================================
            // LEVEL 5: PURPLE HEART / IMPERIAL WOUND STAR (STRICT CASUALTY REQUIREMENT)
            // Strict rule: Awarded ONLY if recipient sustained physical combat wounds!
            // =========================================================================
            if (outcome.playerDamageTaken >= 15f || outcome.playerWounded || soldier.hasLostLimb || soldier.inMedicalConvalescence)
            {
                if (!soldier.HasMedal("Imperial Wound Star"))
                {
                    soldier.AddMedal("Imperial Wound Star");
                    newHonors.Add("Imperial Wound Star");
                }
                else
                {
                    soldier.woundStarsCount++;
                }
            }

            // =========================================================================
            // LEVEL 1: MEDAL OF HONOR / GRAND IMPERIAL STAR (SUPREME VALOR)
            // Strict criteria: Extreme risk of life above and beyond the call of duty:
            // High danger theater (>=4), Decisive Victory, and either heroic rescue under critical health (<35 HP)
            // or high-tier Nemesis kill or single-handedly shattering overwhelming enemy forces (skill >= 75)
            // =========================================================================
            bool qualifiesForMOH = !soldier.HasMedal("Grand Imperial Star") &&
                theater != null && theater.dangerLevel >= 4 &&
                (outcome.outcomeType == TacticalOutcomeType.DecisiveVictory || outcome.outcomeType == TacticalOutcomeType.PyrrhicVictory) &&
                ((hasRescuedComrade && soldier.health < 35f) ||
                 (hasSlayedNemesis && theater.dangerLevel >= 5) ||
                 (soldier.combatSkill >= 75f && mission != null && mission.missionDifficulty >= 4 && outcome.playerDamageTaken >= 20f));

            if (qualifiesForMOH)
            {
                soldier.AddMedal("Grand Imperial Star");
                newHonors.Add("Grand Imperial Star");
                GameEvents.OnFeedEventLogged?.Invoke("SUPREME_VALOR",
                    $"<color=#FFD700><b>[SUPREME MILITARY HONOR • MEDAL OF HONOR]</b></color> For conspicuous gallantry and intrepidity at the risk of life above and beyond the call of duty in {theater.theaterName}, you have been awarded the <b>Grand Imperial Star (Medal of Honor)</b>!");
                return newHonors;
            }

            // =========================================================================
            // LEVEL 2: DISTINGUISHED SERVICE CROSS (EXTRAORDINARY HEROISM)
            // Strict criteria: Extraordinary heroism in operations against armed enemy:
            // Decisive Victory in theater danger >= 3, with combatSkill >= 60 and either Nemesis slayed
            // or flawless victory against fortified pillboxes with 0 squad casualties.
            // =========================================================================
            bool qualifiesForDSC = !soldier.HasMedal("Distinguished Service Cross") &&
                outcome.outcomeType == TacticalOutcomeType.DecisiveVictory &&
                theater != null && theater.dangerLevel >= 3 &&
                (hasSlayedNemesis || (soldier.combatSkill >= 60f && outcome.casualties == 0 && mission != null && mission.missionDifficulty >= 3));

            if (qualifiesForDSC)
            {
                soldier.AddMedal("Distinguished Service Cross");
                newHonors.Add("Distinguished Service Cross");
                GameEvents.OnFeedEventLogged?.Invoke("HIGH_VALOR",
                    $"<color=#FFD700><b>[EXTRAORDINARY HEROISM • DISTINGUISHED SERVICE CROSS]</b></color> In recognition of extraordinary heroism under intense hostile fire at {theater.theaterName}, you have been awarded the <b>Distinguished Service Cross</b>!");
                return newHonors;
            }

            // =========================================================================
            // LEVEL 3: SILVER STAR / SILVER CROSS (CONSPICUOUS GALLANTRY)
            // Strict criteria: Conspicuous gallantry under direct enemy fire.
            // Decisive Victory OR (Tactical Success in danger >= 3 with combatSkill >= 45).
            // =========================================================================
            if (outcome.outcomeType == TacticalOutcomeType.DecisiveVictory)
            {
                if (!soldier.HasMedal("Silver Cross"))
                {
                    soldier.AddMedal("Silver Cross");
                    newHonors.Add("Silver Cross");
                }
                else if (!soldier.HasMedal("Bronze Star"))
                {
                    soldier.AddMedal("Bronze Star");
                    newHonors.Add("Bronze Star");
                }
            }
            else if (outcome.outcomeType == TacticalOutcomeType.TacticalSuccess)
            {
                // =====================================================================
                // LEVEL 4: BRONZE STAR (MERITORIOUS COMBAT ACHIEVEMENT)
                // =====================================================================
                if (!soldier.HasMedal("Bronze Star"))
                {
                    soldier.AddMedal("Bronze Star");
                    newHonors.Add("Bronze Star");
                }
                else if (!soldier.HasMedal("Valor Citation"))
                {
                    soldier.AddMedal("Valor Citation");
                    newHonors.Add("Valor Citation");
                }
            }
            else if (outcome.outcomeType == TacticalOutcomeType.PyrrhicVictory)
            {
                // =====================================================================
                // LEVEL 6: VALOR CITATION (COMBAT TENACITY UNDER SEVERE HOSTILE FIRE)
                // =====================================================================
                if (!soldier.HasMedal("Valor Citation"))
                {
                    soldier.AddMedal("Valor Citation");
                    newHonors.Add("Valor Citation");
                }
            }

            // =========================================================================
            // LEVEL 7: COMBAT INFANTRY BADGE (GROUND COMBAT QUALIFICATION)
            // Awarded on active ground combat engagement under enemy fire
            // =========================================================================
            if (!soldier.HasMedal("Combat Infantry Badge"))
            {
                soldier.AddMedal("Combat Infantry Badge");
                newHonors.Add("Combat Infantry Badge");
            }

            // =========================================================================
            // LEVEL 8: CAMPAIGN THEATER BATTLE STAR
            // Awarded for completing frontline combat tours in theater
            // =========================================================================
            if (soldier.toursInCurrentTheater >= 1 && theater != null)
            {
                string campaignMedal = GetCampaignMedalKeyForTheater(theater.theaterType);
                var def = MedalRegistry.GetDefinition(campaignMedal);
                if (def != null && !soldier.HasMedal(def.displayName))
                {
                    soldier.AddMedal(def.displayName);
                    newHonors.Add(def.displayName);
                }
            }

            return newHonors;
        }
    }
}
