using System;
using UnityEngine;

namespace ArmyLife.Data
{
    /// <summary>
    /// Specialized pilot combat streams and operational tracks within the Imperial Air Force.
    /// </summary>
    public enum PilotSpecialization
    {
        None = 0,
        FighterPilot = 1,       // Air Superiority (e.g. F-15/F-22): High G-force, dogfights, AIM missile interception
        CasStrikePilot = 2,     // Close Air Support (e.g. A-10 Warthog): 30mm GAU-8 Gatling cannon, anti-armor tank-busting, low-altitude AAA flak hazards
        HeavyTransportPilot = 3 // Heavy Transport (e.g. C-130 / C-17): Combat airdrops, rough dirt strip tactical landings, aeromedical evacuations
    }

    /// <summary>
    /// Student and qualification standing within Undergraduate Pilot Training (UPT).
    /// </summary>
    public enum FlightSchoolStatus
    {
        None = 0,
        EnrolledUPT = 1,        // Currently in Undergraduate Pilot Training
        Graduated = 2,          // Qualified combat aviator; earned Imperial Flight Wings
        WashedOut = 3           // Failed centrifuge G-tolerance, flight checkride, or academic boards
    }

    /// <summary>
    /// High-stakes aerial combat and emergency flight dilemmas during sorties.
    /// </summary>
    public enum AviationDilemmaType
    {
        HighGDogfight = 0,      // High-G Dogfight engagement (guns vs missile evasion)
        SamLaunchEvasion = 1,   // Surface-to-Air Missile (SAM) launch evasion with chaff/flares
        DeadstickLanding = 2,   // Engine flameout / hydraulic failure deadstick emergency landing
        BailoutBehindLines = 3  // Ejection seat bailout behind enemy lines with SERE evasion survival
    }

    /// <summary>
    /// Comprehensive outcome of an aviation mission or training sortie.
    /// </summary>
    [Serializable]
    public class AviationSortieOutcome
    {
        public bool isSuccess;
        public bool airToAirVictory;
        public bool antiArmorVictory;
        public bool combatAirdropCompleted;
        public bool planeDamaged;
        public bool pilotEjected;
        public bool survivedSere;
        public int flightHoursEarned;
        public int meritEarned;
        public int cmpEarned;
        public float moraleChange;
        public float healthDamage;
        public string headline;
        public string summaryNarrative;
        public string awardedMedal;

        public AviationSortieOutcome()
        {
            isSuccess = true;
            headline = "Sortie Complete";
            summaryNarrative = "Flight mission conducted without major incident.";
        }
    }
}
