using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Comprehensive Uniform Code of Military Justice (UCMJ) engine simulating
    /// infraction tracking, Non-Judicial Punishment (Article 15 / Captain's Mast),
    /// formal Court-Martial Legal Tribunals with JAG defense counsel, penal detention
    /// at Fort Leavenworth Military Disciplinary Barracks, and official discharge characterizations.
    /// </summary>
    public static class MilitaryJusticeEngine
    {
        public const int ARTICLE_15_RESTRICTION_DAYS = 45;
        public const int LEAVENWORTH_DEFAULT_SENTENCE = 2; // 2 semesters (1 year)

        /// <summary>
        /// Records an infraction against a soldier and files formal military charges.
        /// </summary>
        public static MilitaryChargeRecord RecordInfraction(
            SoldierProfile soldier, 
            MilitaryInfractionType infraction, 
            string customDetails = "")
        {
            if (soldier == null) return null;

            InfractionSeverity severity = GetInfractionSeverity(infraction);
            string title = GetInfractionTitle(infraction);
            string details = string.IsNullOrEmpty(customDetails) ? GetDefaultSpecification(infraction) : customDetails;

            var record = new MilitaryChargeRecord(infraction, severity, title, details, soldier.totalSemestersLived);
            if (soldier.disciplinaryRecords == null) soldier.disciplinaryRecords = new List<MilitaryChargeRecord>();
            soldier.disciplinaryRecords.Add(record);

            // Increment heat and impact military discipline
            switch (severity)
            {
                case InfractionSeverity.Minor:
                    soldier.criminalHeat += 10f;
                    soldier.discipline = Mathf.Max(0f, soldier.discipline - 5f);
                    soldier.coFavor = Mathf.Max(0f, soldier.coFavor - 8f);
                    break;
                case InfractionSeverity.Moderate:
                    soldier.criminalHeat += 25f;
                    soldier.discipline = Mathf.Max(0f, soldier.discipline - 15f);
                    soldier.coFavor = Mathf.Max(0f, soldier.coFavor - 20f);
                    soldier.meritRating = Mathf.Max(0f, soldier.meritRating - 12f);
                    break;
                case InfractionSeverity.Capital:
                    soldier.criminalHeat += 50f;
                    soldier.discipline = Mathf.Max(0f, soldier.discipline - 30f);
                    soldier.coFavor = Mathf.Max(0f, soldier.coFavor - 40f);
                    soldier.meritRating = Mathf.Max(0f, soldier.meritRating - 35f);
                    break;
            }

            GameEvents.OnFeedEventLogged?.Invoke("UCMJ INFRACTION",
                $"<color=#FF8866><b>[UCMJ DISCIPLINARY CHARGE]</b></color> Charged with <b>{title}</b> ({severity} Offense): {details}");

            return record;
        }

        /// <summary>
        /// Imposes Non-Judicial Punishment (Article 15 / Captain's Mast) by the Commanding Officer:
        /// 1. Demotes rank by 1 paygrade.
        /// 2. Imposes 45-day barracks restriction.
        /// 3. Forfeiture of 1/2 pay for the current semester.
        /// </summary>
        public static bool ImposeArticle15(SoldierProfile soldier, MilitaryInfractionType infraction, GameManager gm = null)
        {
            if (soldier == null) return false;

            soldier.article15PunishmentsCount++;

            // 1. Demote rank by 1 paygrade
            DemoteOnePaygrade(soldier, gm);

            // 2. 45-day barracks restriction
            soldier.isBarracksRestricted = true;
            soldier.barracksRestrictionDaysRemaining = ARTICLE_15_RESTRICTION_DAYS;

            // 3. Forfeiture of 1/2 pay
            soldier.hasForfeitedPayThisSemester = true;

            soldier.morale = Mathf.Max(10f, soldier.morale - 25f);
            soldier.coFavor = Mathf.Max(0f, soldier.coFavor - 20f);

            string rankStr = soldier.currentRank != null ? soldier.currentRank.rankName : "Private";

            GameEvents.OnFeedEventLogged?.Invoke("ARTICLE 15",
                $"<color=#FF4444><b>[ARTICLE 15 / CAPTAIN'S MAST]</b></color> Non-Judicial Punishment imposed by CO for {GetInfractionTitle(infraction)}: " +
                $"<b>Demoted 1 Paygrade to {rankStr}</b>, <b>{ARTICLE_15_RESTRICTION_DAYS}-Day Barracks Restriction</b>, and <b>Forfeiture of 1/2 Pay</b>!");

            return true;
        }

        /// <summary>
        /// Conducts a formal Court-Martial Legal Tribunal with Judge Advocate General (JAG) defense counsel,
        /// military prosecutor, evidence score evaluation, and panel deliberation.
        /// </summary>
        public static CourtMartialTrialResult ConductCourtMartial(
            SoldierProfile soldier, 
            MilitaryInfractionType charge, 
            bool hireEliteJagCounsel = false, 
            GameManager gm = null)
        {
            var result = new CourtMartialTrialResult();
            if (soldier == null) return result;

            InfractionSeverity severity = GetInfractionSeverity(charge);

            // 1. Defense Evidence Score
            float defenseScore = (soldier.intellect * 0.35f) + 
                                 (soldier.meritRating * 0.30f) + 
                                 (soldier.squadRespect * 0.25f);

            // Feudal nobility standing influence
            defenseScore += ((int)soldier.nobleTitle) * 6f;

            // Retained private military counsel bonus
            if (hireEliteJagCounsel)
            {
                defenseScore += 35f;
            }

            // 2. Prosecution Evidence Score
            float prosecutionScore = severity switch
            {
                InfractionSeverity.Minor => 25f,
                InfractionSeverity.Moderate => 50f,
                InfractionSeverity.Capital => 75f,
                _ => 40f
            };

            prosecutionScore += (soldier.criminalHeat * 0.35f);
            prosecutionScore += (soldier.disciplinaryRecords.Count * 6f);
            prosecutionScore += (soldier.article15PunishmentsCount * 10f);

            result.defenseEvidenceScore = Mathf.RoundToInt(defenseScore);
            result.prosecutionEvidenceScore = Mathf.RoundToInt(prosecutionScore);

            // 3. Panel Deliberation
            if (defenseScore >= prosecutionScore)
            {
                // ACQUITTED
                result.isAcquitted = true;
                result.panelFinding = "NOT GUILTY on all specifications";
                result.sentenceDescription = "All court-martial charges dismissed with prejudice. Restored to full active service standing.";
                result.officialVerdictAnnouncement = $"The Court-Martial Tribunal finds the accused NOT GUILTY of {GetInfractionTitle(charge)}!";
                soldier.criminalHeat = Mathf.Max(0f, soldier.criminalHeat - 30f);
            }
            else
            {
                // CONVICTED
                result.isAcquitted = false;
                soldier.courtMartialConvictionsCount++;

                if (severity == InfractionSeverity.Capital)
                {
                    // Severe/Capital: Fort Leavenworth Confinement + Dishonorable Discharge
                    result.panelFinding = "GUILTY of Capital Military Offense";
                    result.leavenworthSemestersImposed = LEAVENWORTH_DEFAULT_SENTENCE;
                    result.dischargeImposed = DischargeCharacterization.DishonorableDischarge;
                    result.sentenceDescription = $"Sentenced to {result.leavenworthSemestersImposed} Semesters penal confinement at Fort Leavenworth Disciplinary Barracks and a DISHONORABLE DISCHARGE.";
                    result.officialVerdictAnnouncement = $"The Court-Martial Panel sentences the accused to Fort Leavenworth Confinement and DISHONORABLE DISCHARGE.";

                    // Apply penal confinement
                    soldier.isFortLeavenworthInmate = true;
                    soldier.leavenworthSemestersRemaining = result.leavenworthSemestersImposed;
                    soldier.isImprisoned = true;

                    ExecuteDischarge(soldier, DischargeCharacterization.DishonorableDischarge, $"Court-Martial conviction for {GetInfractionTitle(charge)}", gm);
                }
                else if (severity == InfractionSeverity.Moderate)
                {
                    // Moderate: Bad Conduct Discharge + 1 semester confinement
                    result.panelFinding = "GUILTY of General Court-Martial Offense";
                    result.leavenworthSemestersImposed = 1;
                    result.dischargeImposed = DischargeCharacterization.BadConductDischarge;
                    result.sentenceDescription = "Sentenced to 1 Semester penal confinement at Fort Leavenworth and a BAD CONDUCT DISCHARGE (BCD).";
                    result.officialVerdictAnnouncement = $"The Court-Martial Panel sentences the accused to BAD CONDUCT DISCHARGE (forfeiting all VA benefits).";

                    soldier.isFortLeavenworthInmate = true;
                    soldier.leavenworthSemestersRemaining = 1;
                    soldier.isImprisoned = true;

                    ExecuteDischarge(soldier, DischargeCharacterization.BadConductDischarge, $"Court-Martial conviction for {GetInfractionTitle(charge)}", gm);
                }
                else
                {
                    // Minor: Article 15 reduction + reprimand
                    result.panelFinding = "GUILTY of Summary Offense";
                    result.article15DemotionImposed = true;
                    result.sentenceDescription = "Sentenced to reduction to lowest enlisted grade and 45-day confinement.";
                    result.officialVerdictAnnouncement = "The Court-Martial Tribunal imposes rank demotion and disciplinary restriction.";

                    ImposeArticle15(soldier, charge, gm);
                }
            }

            GameEvents.OnFeedEventLogged?.Invoke("COURT-MARTIAL",
                $"<color=#FF0044><b>[LEGAL TRIBUNAL VERDICT]</b></color> {result.officialVerdictAnnouncement} (Defense: {result.defenseEvidenceScore} pts vs Prosecution: {result.prosecutionEvidenceScore} pts)");

            return result;
        }

        /// <summary>
        /// Formally separates the soldier from the armed forces under the specified characterization.
        /// </summary>
        public static void ExecuteDischarge(
            SoldierProfile soldier, 
            DischargeCharacterization characterization, 
            string reason, 
            GameManager gm = null)
        {
            if (soldier == null) return;

            soldier.dischargeCharacterization = characterization;
            soldier.isRetired = true;

            switch (characterization)
            {
                case DischargeCharacterization.Honorable:
                    soldier.AddTrait("Honorable Military Veteran");
                    GameEvents.OnFeedEventLogged?.Invoke("DISCHARGE",
                        "<color=#00FFAA><b>[HONORABLE DISCHARGE]</b></color> Separated with supreme military honors. Lifetime veteran pension and VA benefits secured.");
                    break;

                case DischargeCharacterization.GeneralUnderHonorableConditions:
                    soldier.AddTrait("General Discharge Veteran");
                    GameEvents.OnFeedEventLogged?.Invoke("DISCHARGE",
                        "<color=#00D4FF><b>[GENERAL DISCHARGE]</b></color> Separated under honorable conditions. Standard pension benefits conferred.");
                    break;

                case DischargeCharacterization.OtherThanHonorable:
                    soldier.AddTrait("Other Than Honorable Discharge");
                    GameEvents.OnFeedEventLogged?.Invoke("DISCHARGE",
                        "<color=#FFAA00><b>[OTHER THAN HONORABLE DISCHARGE]</b></color> Administrative separation for misconduct.");
                    break;

                case DischargeCharacterization.BadConductDischarge:
                    soldier.AddTrait("Bad Conduct Discharge");
                    // Forfeits all VA benefits and military pension
                    soldier.calculatedPensionPerMonth = 0L;
                    soldier.veteranPensionPerMonth = 0L;
                    soldier.vaDisabilityAllowancePerMonth = 0L;
                    GameEvents.OnFeedEventLogged?.Invoke("DISCHARGE",
                        "<color=#FF4444><b>[BAD CONDUCT DISCHARGE (BCD)]</b></color> Punitive separation by court-martial! All VA healthcare, disability allowances, and military pensions are FORFEITED.");
                    break;

                case DischargeCharacterization.DishonorableDischarge:
                    soldier.hasFelonyDischargeRecord = true;
                    soldier.AddTrait("Dishonorable Discharge (Felony Record)");

                    // Strip rank to lowest paygrade E-1
                    StripToLowestEnlistedRank(soldier, gm);

                    // Complete forfeiture of pension
                    soldier.calculatedPensionPerMonth = 0L;
                    soldier.veteranPensionPerMonth = 0L;
                    soldier.vaDisabilityAllowancePerMonth = 0L;

                    // Complete forfeiture of all medals
                    if (soldier.earnedMedals != null)
                    {
                        soldier.earnedMedals.Clear();
                    }
                    soldier.medalsEarnedCount = 0;

                    GameEvents.OnFeedEventLogged?.Invoke("DISCHARGE",
                        "<color=#FF0000><b>[DISHONORABLE DISCHARGE]</b></color> Stripped of all rank, all military decorations and medals REVOKED, military pension REVOKED, permanent federal felony record registered!");
                    break;
            }
        }

        /// <summary>
        /// Advances a semester of penal confinement at Fort Leavenworth Military Disciplinary Barracks.
        /// </summary>
        public static void ProcessLeavenworthPenalSemester(SoldierProfile soldier, GameManager gm = null)
        {
            if (soldier == null || !soldier.isFortLeavenworthInmate) return;

            soldier.leavenworthSemestersRemaining--;
            soldier.morale = Mathf.Max(5f, soldier.morale - 15f);

            if (soldier.leavenworthSemestersRemaining <= 0)
            {
                soldier.isFortLeavenworthInmate = false;
                soldier.isImprisoned = false;
                GameEvents.OnFeedEventLogged?.Invoke("PENAL FACILITY",
                    "<color=#00FFAA><b>[FORT LEAVENWORTH RELEASE]</b></color> Completed punitive confinement sentence at Fort Leavenworth Disciplinary Barracks. Released to civilian society.");
            }
            else
            {
                GameEvents.OnFeedEventLogged?.Invoke("PENAL FACILITY",
                    $"<color=#888888>[FORT LEAVENWORTH CONFINEMENT]</color> Serving hard labor sentence at Fort Leavenworth. ({soldier.leavenworthSemestersRemaining} semesters remaining)");
            }
        }

        #region Helper Demotion & Rank Methods
        public static void DemoteOnePaygrade(SoldierProfile soldier, GameManager gm = null)
        {
            if (soldier == null || soldier.currentRank == null) return;

            int targetOrder = soldier.currentRank.rankOrder - 1;
            if (targetOrder < 1) targetOrder = 1;

            MilitaryRankSO demotedRank = null;

            // Search database
            var db = (gm != null && gm.militaryRankDatabase != null) 
                ? gm.militaryRankDatabase 
                : (GameManager.Instance != null ? GameManager.Instance.militaryRankDatabase : null);

            if (db != null)
            {
                foreach (var r in db)
                {
                    if (r != null && r.rankOrder == targetOrder)
                    {
                        demotedRank = r;
                        break;
                    }
                }
            }

            if (demotedRank != null)
            {
                soldier.currentRank = demotedRank;
            }
            else
            {
                // In isolated unit tests: adjust existing instance or create ScriptableObject
                var newRank = ScriptableObject.CreateInstance<MilitaryRankSO>();
                newRank.rankOrder = targetOrder;
                newRank.paygrade = $"E-{targetOrder}";
                newRank.rankName = targetOrder switch
                {
                    1 => "Private Recruit",
                    2 => "Private",
                    3 => "Private First Class",
                    4 => "Corporal",
                    5 => "Sergeant",
                    6 => "Staff Sergeant",
                    7 => "Sergeant First Class",
                    8 => "Master Sergeant",
                    9 => "Command Sgt Major",
                    _ => $"Grade {targetOrder}"
                };
                newRank.type = PaygradeType.Enlisted;
                newRank.baseMonthlySalary = 1800 + (targetOrder * 250);
                soldier.currentRank = newRank;
            }
        }

        private static void StripToLowestEnlistedRank(SoldierProfile soldier, GameManager gm = null)
        {
            if (soldier == null) return;

            var db = (gm != null && gm.militaryRankDatabase != null) 
                ? gm.militaryRankDatabase 
                : (GameManager.Instance != null ? GameManager.Instance.militaryRankDatabase : null);

            if (db != null && db.Length > 0)
            {
                soldier.currentRank = db[0];
            }
            else
            {
                var lowest = ScriptableObject.CreateInstance<MilitaryRankSO>();
                lowest.rankName = "Private Recruit";
                lowest.paygrade = "E-1";
                lowest.rankOrder = 1;
                lowest.type = PaygradeType.Enlisted;
                lowest.baseMonthlySalary = 1800;
                soldier.currentRank = lowest;
            }
        }

        public static InfractionSeverity GetInfractionSeverity(MilitaryInfractionType type)
        {
            return type switch
            {
                MilitaryInfractionType.CurfewViolation => InfractionSeverity.Minor,
                MilitaryInfractionType.BarracksInsubordination => InfractionSeverity.Minor,
                MilitaryInfractionType.UniformNegligence => InfractionSeverity.Minor,
                MilitaryInfractionType.BarracksGambling => InfractionSeverity.Minor,

                MilitaryInfractionType.AbsentWithoutLeave => InfractionSeverity.Moderate,
                MilitaryInfractionType.BlackMarketGraft => InfractionSeverity.Moderate,
                MilitaryInfractionType.DerelictionOfWatch => InfractionSeverity.Moderate,

                MilitaryInfractionType.CombatInsubordination => InfractionSeverity.Capital,
                MilitaryInfractionType.UnauthorizedWeaponDischarge => InfractionSeverity.Capital,
                MilitaryInfractionType.FriendlyFireNegligence => InfractionSeverity.Capital,
                MilitaryInfractionType.WarCrimesOrTreason => InfractionSeverity.Capital,

                _ => InfractionSeverity.Minor
            };
        }

        public static string GetInfractionTitle(MilitaryInfractionType type)
        {
            return type switch
            {
                MilitaryInfractionType.CurfewViolation => "Curfew Violation",
                MilitaryInfractionType.BarracksInsubordination => "Barracks Insubordination",
                MilitaryInfractionType.UniformNegligence => "Uniform & Gear Negligence",
                MilitaryInfractionType.BarracksGambling => "Illegal Barracks Gambling",

                MilitaryInfractionType.AbsentWithoutLeave => "Absent Without Leave (AWOL)",
                MilitaryInfractionType.BlackMarketGraft => "Black Market PX Graft",
                MilitaryInfractionType.DerelictionOfWatch => "Dereliction of Watch Duty",

                MilitaryInfractionType.CombatInsubordination => "Insubordination Under Fire",
                MilitaryInfractionType.UnauthorizedWeaponDischarge => "Unauthorized Weapon Discharge",
                MilitaryInfractionType.FriendlyFireNegligence => "Friendly Fire Negligence",
                MilitaryInfractionType.WarCrimesOrTreason => "War Crimes & High Treason",

                _ => type.ToString()
            };
        }

        public static string GetDefaultSpecification(MilitaryInfractionType type)
        {
            return type switch
            {
                MilitaryInfractionType.CurfewViolation => "Failed to report to barracks prior to 2200 taps inspection.",
                MilitaryInfractionType.BarracksInsubordination => "Disrespected senior non-commissioned officer during morning muster.",
                MilitaryInfractionType.UniformNegligence => "Unpolished boots, soiled tunic, and dirty service rifle at company parade.",
                MilitaryInfractionType.BarracksGambling => "Conducted high-stakes dice ring in quarters during lights-out.",

                MilitaryInfractionType.AbsentWithoutLeave => "Departed military installation without authorized leave pass for 72 hours.",
                MilitaryInfractionType.BlackMarketGraft => "Diverted military rations and fuel canisters to civilian trade syndicates.",
                MilitaryInfractionType.DerelictionOfWatch => "Asleep on perimeter guard watchtower during hostile alert condition.",

                MilitaryInfractionType.CombatInsubordination => "Refused direct command to assault hostile trench pillbox during battle.",
                MilitaryInfractionType.UnauthorizedWeaponDischarge => "Fired live rifle rounds in populated mess hall area causing panic.",
                MilitaryInfractionType.FriendlyFireNegligence => "Careless artillery coordinate callout endangering adjacent platoon.",
                MilitaryInfractionType.WarCrimesOrTreason => "Leaked imperial cipher keys and executed unlawful pillage against non-combatants.",

                _ => "Violation of standard military code of conduct."
            };
        }
        #endregion
    }
}
