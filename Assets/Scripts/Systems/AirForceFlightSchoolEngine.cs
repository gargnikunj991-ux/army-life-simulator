using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Comprehensive simulation engine for Air Force Undergraduate Pilot Training (UPT),
    /// specialized aircraft streams (Fighter, CAS Strike, Heavy Transport),
    /// combat sortie dilemmas (High-G dogfights, SAM evasion, deadstick landing, ejection SERE),
    /// and elite Top Gun Weapons School qualification.
    /// </summary>
    public static class AirForceFlightSchoolEngine
    {
        public const int UPT_REQUIRED_SEMESTERS = 2; // 1-year training
        public const int TOP_GUN_HOURS_THRESHOLD = 150;

        /// <summary>
        /// Applies for admission to the Air Force Undergraduate Pilot Training (UPT).
        /// Cadets, recruits, and airmen with sufficient physical and mental fitness are admitted.
        /// </summary>
        public static bool ApplyForFlightSchool(SoldierProfile soldier, GameManager gm = null)
        {
            if (soldier == null || soldier.isDead || soldier.isRetired) return false;

            // Ensure soldier is in Air Force (or transferring into Air Force)
            if (soldier.militaryBranch != MilitaryBranch.AirForce)
            {
                soldier.militaryBranch = MilitaryBranch.AirForce;
            }

            // Minimum entrance standards for military flight training
            if (soldier.fitness < 40f || soldier.intellect < 45f || soldier.health < 50f)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FLIGHT SCHOOL", 
                    "<color=#FF6666>[UPT FLIGHT SURGEON DISQUALIFIED]</color> You failed the aerospace medical check: insufficient cardiovascular fitness or visual acuity.");
                return false;
            }

            soldier.flightSchoolStatus = FlightSchoolStatus.EnrolledUPT;
            soldier.flightSchoolSemestersEnrolled = 0;
            soldier.assignedDepartment = "Flight Training Wing";

            GameEvents.OnFeedEventLogged?.Invoke("FLIGHT SCHOOL", 
                "<color=#00D4FF>[UNDERGRADUATE PILOT TRAINING]</color> Admitted to UPT Flight School! Beginning primary flight simulators, aerobatics, and centrifuge high-G spin training.");
            return true;
        }

        /// <summary>
        /// Advances a semester of Undergraduate Pilot Training.
        /// </summary>
        public static bool ProcessUPTSemester(SoldierProfile soldier, GameManager gm = null)
        {
            if (soldier == null || soldier.flightSchoolStatus != FlightSchoolStatus.EnrolledUPT) return false;

            soldier.flightSchoolSemestersEnrolled++;
            soldier.flightHoursLogged += 40; // 40 primary training hours
            soldier.fitness += 4f;
            soldier.intellect += 3f;
            soldier.discipline += 5f;

            // Attrition check: Physical exhaustion or flight disorientation
            if (soldier.fitness < 35f || soldier.health < 35f)
            {
                soldier.flightSchoolStatus = FlightSchoolStatus.WashedOut;
                soldier.airForceRole = AirForceRole.GroundDutyStaff;
                GameEvents.OnFeedEventLogged?.Invoke("FLIGHT SCHOOL",
                    "<color=#FF4444>[FLIGHT SCHOOL WASHOUT]</color> Physical collapse during high-G centrifuge centrifuge testing. Washed out of UPT to ground crew.");
                return false;
            }

            GameEvents.OnFeedEventLogged?.Invoke("FLIGHT SCHOOL",
                $"<color=#00FFAA>[UPT PROGRESSION]</color> Completed Term {soldier.flightSchoolSemestersEnrolled}/{UPT_REQUIRED_SEMESTERS} of Flight School. Solo flight and instrument checkrides passed! (+40 Flight Hours)");
            return true;
        }

        /// <summary>
        /// Successfully graduates the student from UPT into their chosen aircraft stream.
        /// Conveys Imperial Flight Wings of Gold and assigns Flight Duty role (+25% base pay).
        /// </summary>
        public static void GraduateFlightSchool(SoldierProfile soldier, PilotSpecialization specialization, GameManager gm = null)
        {
            if (soldier == null) return;

            soldier.flightSchoolStatus = FlightSchoolStatus.Graduated;
            soldier.airForceRole = AirForceRole.FlightDuty;
            soldier.pilotSpecialization = specialization;
            soldier.assignedDepartment = "Frontline Aviation Squadron";
            soldier.flightHoursLogged += 80;

            soldier.squadRespect += 15f;
            soldier.meritRating += 20f;
            soldier.combatSkill += 12f;

            string trackName = "";
            switch (specialization)
            {
                case PilotSpecialization.FighterPilot:
                    trackName = "Fighter Pilot (Air Superiority)";
                    soldier.AddTrait("Fighter Pilot");
                    break;
                case PilotSpecialization.CasStrikePilot:
                    trackName = "CAS Strike Pilot (Tank Buster)";
                    soldier.AddTrait("CAS Strike Pilot");
                    break;
                case PilotSpecialization.HeavyTransportPilot:
                    trackName = "Heavy Transport Pilot (Tactical Airlift)";
                    soldier.AddTrait("Heavy Transport Pilot");
                    break;
                default:
                    trackName = "Combat Aviator";
                    break;
            }

            soldier.AddTrait("Imperial Flight Wings");

            GameEvents.OnFeedEventLogged?.Invoke("FLIGHT SCHOOL GRADUATION",
                $"<color=#FFD700><b>[WINGS OF GOLD PINNED]</b></color> Graduated Undergraduate Pilot Training with supreme honors! Commissioned as a <b>{trackName}</b>. Flight Duty Pay bonus (+25%) activated!");
        }

        /// <summary>
        /// Executes a realistic combat flight sortie, triggering specialized aerial combat and emergency dilemmas.
        /// </summary>
        public static AviationSortieOutcome ExecuteAviationSortie(
            SoldierProfile soldier, 
            AviationDilemmaType? forcedDilemma = null, 
            int tacticalChoice = 0, 
            GameManager gm = null)
        {
            var outcome = new AviationSortieOutcome();
            if (soldier == null) return outcome;

            // Ensure flight role
            soldier.airForceRole = AirForceRole.FlightDuty;
            soldier.flightSchoolStatus = FlightSchoolStatus.Graduated;

            // Determine dilemma
            AviationDilemmaType dilemma = forcedDilemma ?? (AviationDilemmaType)UnityEngine.Random.Range(0, 4);

            outcome.flightHoursEarned = UnityEngine.Random.Range(25, 45);
            soldier.flightHoursLogged += outcome.flightHoursEarned;

            switch (dilemma)
            {
                // DILEMMA 1: HIGH-G DOGFIGHT (Air Superiority)
                case AviationDilemmaType.HighGDogfight:
                    outcome.headline = "High-G Aerial Dogfight Engagement";
                    if (tacticalChoice == 0)
                    {
                        // Lead-turn AIM missile shot
                        outcome.isSuccess = true;
                        outcome.airToAirVictory = true;
                        soldier.airToAirKills++;
                        outcome.meritEarned = 25;
                        outcome.cmpEarned = 35;
                        outcome.moraleChange = 15f;
                        outcome.summaryNarrative = "Executed an 8.5-G lead turn across the sun, locked onto the hostile MiG interceptor, and launched an AIM missile splash!";
                    }
                    else
                    {
                        // High-G barrel roll guns defense
                        outcome.isSuccess = true;
                        outcome.airToAirVictory = true;
                        soldier.airToAirKills++;
                        outcome.meritEarned = 20;
                        outcome.cmpEarned = 30;
                        outcome.moraleChange = 12f;
                        outcome.summaryNarrative = "Pulled into a high-G defensive barrel roll, overshot the enemy bandit, and shredded their wing with 20mm rotary cannon fire!";
                    }
                    soldier.combatMeritPoints += outcome.cmpEarned;
                    soldier.meritRating += outcome.meritEarned;
                    break;

                // DILEMMA 2: SURFACE-TO-AIR MISSILE (SAM) EVASION
                case AviationDilemmaType.SamLaunchEvasion:
                    outcome.headline = "Surface-to-Air Missile (SAM) Threat Evasion";
                    if (tacticalChoice == 0)
                    {
                        // Chaff, flares & beam notch
                        outcome.isSuccess = true;
                        outcome.meritEarned = 18;
                        outcome.cmpEarned = 20;
                        outcome.moraleChange = 10f;
                        outcome.summaryNarrative = "Radar warning receiver shrieked incoming missile! Dispensed chaff and IR flares into the supersonic plume, beaming into the Doppler notch to break lock!";
                    }
                    else
                    {
                        // Hard nap-of-the-earth canyon run
                        outcome.isSuccess = true;
                        outcome.antiArmorVictory = (soldier.pilotSpecialization == PilotSpecialization.CasStrikePilot);
                        if (outcome.antiArmorVictory) soldier.antiArmorKills += 2;
                        outcome.meritEarned = 22;
                        outcome.cmpEarned = 25;
                        outcome.summaryNarrative = "Dove into the jagged river canyon at 450 knots, masking behind granite ridges as the missile detonated harmlessly against the canyon wall!";
                    }
                    soldier.combatMeritPoints += outcome.cmpEarned;
                    soldier.meritRating += outcome.meritEarned;
                    break;

                // DILEMMA 3: ENGINE FLAMEOUT / DEADSTICK EMERGENCY LANDING
                case AviationDilemmaType.DeadstickLanding:
                    outcome.headline = "Dual Engine Flameout & Deadstick Recovery";
                    if (tacticalChoice == 0)
                    {
                        // Deadstick highway landing
                        outcome.isSuccess = true;
                        outcome.meritEarned = 30;
                        outcome.cmpEarned = 40;
                        outcome.summaryNarrative = "Total turbine flameout! Established best glide airspeed, deadsticked the heavy bird onto a rural highway strip with zero hydraulic steering, preserving the multi-million dollar airframe!";
                    }
                    else
                    {
                        // In-flight APU relight
                        outcome.isSuccess = true;
                        outcome.meritEarned = 25;
                        outcome.cmpEarned = 30;
                        outcome.summaryNarrative = "Spun the auxiliary power unit, traded altitude for airspeed, and executed an airstart windmill relight at 4,000 feet, safely returning to base!";
                    }
                    soldier.combatMeritPoints += outcome.cmpEarned;
                    soldier.meritRating += outcome.meritEarned;
                    break;

                // DILEMMA 4: BAILOUT & SERE SURVIVAL BEHIND ENEMY LINES
                case AviationDilemmaType.BailoutBehindLines:
                    outcome.headline = "Combat Ejection & SERE Evasion";
                    outcome.pilotEjected = true;
                    soldier.emergencyEjectionsSurvived++;
                    outcome.planeDamaged = true;

                    if (tacticalChoice == 0)
                    {
                        // Parachute into pine forest & SERE evasion
                        outcome.isSuccess = true;
                        outcome.survivedSere = true;
                        soldier.hasSurvivedSereBehindLines = true;
                        soldier.AddTrait("SERE Survival Veteran");
                        outcome.meritEarned = 35;
                        outcome.cmpEarned = 50;
                        outcome.healthDamage = 15f;
                        outcome.moraleChange = 15f;
                        outcome.summaryNarrative = "Canopy jettisoned! Ejected at Mach 0.9, parachuted into dense hostile taiga, evaded patrol hounds using alchemical scent-masking, and signaled Combat Search & Rescue (CSAR) Jolly Green extraction!";
                    }
                    else
                    {
                        // Survival carbine defense
                        outcome.isSuccess = true;
                        outcome.survivedSere = true;
                        soldier.hasSurvivedSereBehindLines = true;
                        soldier.AddTrait("SERE Survival Veteran");
                        outcome.meritEarned = 40;
                        outcome.cmpEarned = 60;
                        outcome.healthDamage = 20f;
                        outcome.summaryNarrative = "Ejected behind enemy lines, held off hostile search squads with your survival carbine from a rocky crevice, and made rendezvous with imperial extraction helicopters!";
                    }
                    soldier.health = Mathf.Max(10f, soldier.health - outcome.healthDamage);
                    soldier.combatMeritPoints += outcome.cmpEarned;
                    soldier.meritRating += outcome.meritEarned;
                    break;
            }

            // Award Distinguished Flying Cross if exceptional bravery demonstrated
            if (soldier.airToAirKills >= 3 || soldier.hasSurvivedSereBehindLines || soldier.flightHoursLogged >= 250)
            {
                if (!soldier.HasMedal("Distinguished Flying Cross"))
                {
                    soldier.AddMedal("Distinguished Flying Cross");
                    outcome.awardedMedal = "Distinguished Flying Cross";
                }
            }

            GameEvents.OnFeedEventLogged?.Invoke("AIR COMBAT", 
                $"<color=#00D4FF><b>[{outcome.headline.ToUpper()}]</b></color> {outcome.summaryNarrative} (+{outcome.flightHoursEarned} Flight Hours, +{outcome.meritEarned} Merit)");

            return outcome;
        }

        /// <summary>
        /// Checks whether the aviator qualifies for the elite Top Gun Fighter Weapons School.
        /// </summary>
        public static bool CheckTopGunEligibility(SoldierProfile soldier)
        {
            if (soldier == null) return false;
            if (soldier.flightSchoolStatus != FlightSchoolStatus.Graduated) return false;
            if (soldier.isTopGunGraduate) return false;

            return (soldier.flightHoursLogged >= TOP_GUN_HOURS_THRESHOLD || 
                    soldier.airToAirKills >= 2 || 
                    soldier.meritRating >= 70f);
        }

        /// <summary>
        /// Enrolls and completes the Top Gun / Fighter Weapons School elite program.
        /// </summary>
        public static bool EnrollInTopGunSchool(SoldierProfile soldier, GameManager gm = null)
        {
            if (soldier == null) return false;

            soldier.isTopGunGraduate = true;
            soldier.AddTrait("Top Gun Graduate");
            soldier.flightHoursLogged += 50;
            soldier.squadRespect += 25f;
            soldier.meritRating += 30f;
            soldier.combatSkill += 15f;

            GameEvents.OnFeedEventLogged?.Invoke("TOP GUN",
                "<color=#FFD700><b>[TOP GUN WEAPONS SCHOOL GRADUATE]</b></color> Completed the apex Fighter Weapons School! Bestowed elite Top Gun Instructor status, dominating supersonic combat maneuvering.");
            return true;
        }
    }
}
