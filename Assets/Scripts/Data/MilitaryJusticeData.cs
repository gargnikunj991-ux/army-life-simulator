using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Severity classification for Uniform Code of Military Justice (UCMJ) violations.
    /// </summary>
    public enum InfractionSeverity
    {
        Minor = 0,
        Moderate = 1,
        Capital = 2
    }

    /// <summary>
    /// Specific military infractions and offenses punishable under the UCMJ.
    /// </summary>
    public enum MilitaryInfractionType
    {
        // Minor Infractions
        CurfewViolation = 0,
        BarracksInsubordination = 1,
        UniformNegligence = 2,
        BarracksGambling = 3,

        // Moderate Infractions
        AbsentWithoutLeave = 10,        // AWOL
        BlackMarketGraft = 11,
        DerelictionOfWatch = 12,

        // Capital / Severe Infractions
        CombatInsubordination = 20,
        UnauthorizedWeaponDischarge = 21,
        FriendlyFireNegligence = 22,
        WarCrimesOrTreason = 23
    }

    /// <summary>
    /// Characterization of military discharge and separation from service.
    /// </summary>
    public enum DischargeCharacterization
    {
        None = 0,                               // Still serving in active military duty
        Honorable = 1,                          // Conferred with supreme honors, lifetime pension, VA medical benefits, medals preserved
        GeneralUnderHonorableConditions = 2,    // Standard separation with minor blemish; standard benefits retained
        OtherThanHonorable = 3,                 // OTH: Administrative discharge for misconduct; forfeits selective veteran perks
        BadConductDischarge = 4,                // BCD: Conferred by Special/General Court-Martial; completely forfeits VA and veteran benefits
        DishonorableDischarge = 5               // Dishonorable: Stripped to E-1, felony record, complete forfeiture of pension and all medals
    }

    /// <summary>
    /// Formal record of an infraction or UCMJ charge lodged against a service member.
    /// </summary>
    [Serializable]
    public class MilitaryChargeRecord
    {
        public MilitaryInfractionType infraction;
        public InfractionSeverity severity;
        public string chargeTitle;
        public string specificationDetails;
        public int semesterCommitted;
        public bool resolved;
        public string dispositionResult;

        public MilitaryChargeRecord() { }

        public MilitaryChargeRecord(MilitaryInfractionType infraction, InfractionSeverity severity, string title, string details, int semester)
        {
            this.infraction = infraction;
            this.severity = severity;
            this.chargeTitle = title;
            this.specificationDetails = details;
            this.semesterCommitted = semester;
            this.resolved = false;
            this.dispositionResult = "Pending Disciplinary Action";
        }
    }

    /// <summary>
    /// Outcome of a formal Court-Martial Legal Tribunal.
    /// </summary>
    [Serializable]
    public class CourtMartialTrialResult
    {
        public bool isAcquitted;
        public int defenseEvidenceScore;
        public int prosecutionEvidenceScore;
        public string panelFinding;
        public string sentenceDescription;
        public DischargeCharacterization dischargeImposed;
        public int leavenworthSemestersImposed;
        public bool article15DemotionImposed;
        public string officialVerdictAnnouncement;

        public CourtMartialTrialResult()
        {
            dischargeImposed = DischargeCharacterization.None;
        }
    }
}
