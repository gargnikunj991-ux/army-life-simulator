using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Comprehensive Field Hospital & Medical Convalescence Engine:
    /// 1. Triage admission when soldier health falls below critical threshold (<35 HP) or catastrophic trauma.
    /// 2. Severe combat wounds, IED detonations, limb amputations, and prosthetic fittings.
    /// 3. Convalescence recovery cycles with surgical procedures and physical therapy.
    /// 4. Armed Forces Medical Evaluation Board (MEB) enforcing mandatory Medical Retirement (Disability Pension)
    ///    when wounds are permanent or recovery fails.
    /// </summary>
    public static class FieldHospitalEngine
    {
        public const float CRITICAL_CASUALTY_THRESHOLD = 35f;
        public const long SPECIALIST_SURGERY_COST = 400L;

        /// <summary>
        /// Admits a wounded soldier to the forward field hospital for emergency surgical stabilization.
        /// </summary>
        public static void AdmitToHospital(SoldierProfile soldier, string woundReason, bool isIEDAmputation = false)
        {
            if (soldier == null || soldier.isDead) return;

            soldier.inMedicalConvalescence = true;
            soldier.convalescenceSemestersRemaining = 1;
            soldier.isInCombatDeployment = false;

            if (isIEDAmputation)
            {
                soldier.hasLostLimb = true;
                soldier.amputatedLimbDescription = "Left Leg Severed in IED Pressure-Plate Detonation";
                soldier.woundStarsCount++;
                soldier.AddTrait("Combat Amputee (Lost Leg in IED Blast)");
                soldier.AddMedal("Imperial Purple Heart with Valor");
                soldier.AddMedal("Imperial Wound Star");
                soldier.combatStress = Mathf.Min(100f, soldier.combatStress + 35f);
                AudioManager.PlayHeartbeat();
            }

            string details = isIEDAmputation
                ? "<color=#FF2222><b>[CATASTROPHIC IED TRAUMA]</b></color> Stretcher-bearers rushed you into trauma surgery under tourniquet compression! Left leg severed below knee."
                : $"<color=#FF5555><b>[FORWARD FIELD HOSPITAL ADMISSION]</b></color> Critical casualty triage: Admitted for intensive wound care ({woundReason}).";

            GameEvents.OnFeedEventLogged?.Invoke("HOSPITAL",
                $"{details} Medical staff initiate blood transfusions and wound irrigation.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        /// <summary>
        /// Simulates a catastrophic IED detonation under patrol or combat lines.
        /// Destroys a limb, drops health to critical, and evacuates soldier to field hospital.
        /// </summary>
        public static void TriggerIEDDetonation(SoldierProfile soldier, GameManager gm)
        {
            if (soldier == null || soldier.isDead) return;

            AudioManager.PlayArtilleryExplosion();
            AudioManager.PlayHeartbeat();

            soldier.health = Mathf.Min(12f, soldier.health);
            soldier.combatReadiness = Mathf.Max(10f, soldier.combatReadiness - 40f);

            GameEvents.OnFeedEventLogged?.Invoke("EXPLOSION",
                "<color=#FF0000><b>[EARTH-SHATTERING IED DETONATION!]</b></color> A buried 155mm artillery shell pressure-plate IED detonated beneath your patrol line! " +
                "The shockwave tossed you into the air through black smoke and jagged shrapnel. Your left leg was blown off below the knee! " +
                "Squadmates applied an emergency combat tourniquet under incoming rifle fire.");

            AdmitToHospital(soldier, "IED Pressure-Plate Blast & Traumatic Amputation", isIEDAmputation: true);
        }

        /// <summary>
        /// Processes a 6-month convalescence cycle in the military hospital ward.
        /// Evaluates whether the soldier recovers or is forced into medical retirement by the MEB.
        /// </summary>
        public static void ProcessConvalescenceTurn(SoldierProfile soldier, GameManager gm, bool optedForSpecialistSurgery = false)
        {
            if (soldier == null || soldier.isDead || !soldier.inMedicalConvalescence) return;

            if (optedForSpecialistSurgery && soldier.cashBalance >= SPECIALIST_SURGERY_COST)
            {
                soldier.cashBalance -= SPECIALIST_SURGERY_COST;
                soldier.totalMedicalExpensesPaid += SPECIALIST_SURGERY_COST;
            }

            // Recovery calculation: Base 45% + fitness bonus + specialist surgery bonus
            float recoveryScore = UnityEngine.Random.Range(20f, 65f) + (soldier.physicalFitness * 0.35f);
            if (optedForSpecialistSurgery) recoveryScore += 25f;

            // Combat medic presence boost
            if (soldier.squadMates != null)
            {
                var doc = soldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);
                if (doc != null && doc.relationship >= 50f)
                {
                    recoveryScore += 12f;
                }
            }

            // Evaluation threshold
            if (recoveryScore >= 55f)
            {
                // Successful recovery!
                soldier.health = Mathf.Clamp(soldier.health + 45f, 60f, 85f);
                soldier.inMedicalConvalescence = false;
                soldier.convalescenceSemestersRemaining = 0;

                if (soldier.hasLostLimb)
                {
                    soldier.hasCombatProsthetic = true;
                    soldier.AddTrait("Prosthetic Mobility: Titanium Combat Leg");
                    soldier.physicalFitness = Mathf.Max(35f, soldier.physicalFitness - 10f);

                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL RECOVERY",
                        "<color=#00FFAA><b>[SURGICAL GRAFT & PROSTHETIC SUCCESS]</b></color> Orthopedic trauma surgeons successfully stabilized your leg stump and fitted a combat-grade titanium prosthetic! " +
                        "Through grueling rehab and stubborn willpower, you learned to run and march again. The Medical Board has cleared you for active duty!");
                }
                else
                {
                    soldier.AddTrait("Scarred Combat Veteran");
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL RECOVERY",
                        "<color=#00FFAA><b>[DISCHARGED FROM FIELD HOSPITAL]</b></color> Shrapnel excised, deep tissue stitched, and fever broken! " +
                        "Physical therapy completed (+45 Health). Returned to active duty readiness!");
                }
            }
            else
            {
                // Recovery failed: Chronic complications, nerve damage, or body shattered
                soldier.health = Mathf.Min(30f, soldier.health);
                ForceMedicalRetirement(soldier, gm, soldier.hasLostLimb
                    ? "Severe Traumatic Amputation & Incurable Chronic Phantom Nerve Pain"
                    : "Irreversible Blast Trauma, Shrapnel Osteomyelitis & Compromised Vital Capacity");
            }

            LifePersistenceManager.SaveSoldierLife(soldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        /// <summary>
        /// Armed Forces Medical Evaluation Board convenes and forces an honorable medical discharge with lifetime disability pension.
        /// </summary>
        public static void ForceMedicalRetirement(SoldierProfile soldier, GameManager gm, string medicalReason)
        {
            if (soldier == null || soldier.isDead) return;

            soldier.isRetired = true;
            soldier.isMedicallyRetired = true;
            soldier.inMedicalConvalescence = false;
            soldier.isInCombatDeployment = false;

            // 100% Medical Disability Pension: Guaranteed $2,100+/mo base + wound stars
            long rankBase = soldier.currentRank != null ? soldier.currentRank.baseMonthlySalary : 600L;
            soldier.medicalDisabilityPension = EconomyData.MINIMUM_VETERAN_PENSION + 600L + (soldier.GetWoundStarsCount() * EconomyData.VA_DISABILITY_PER_WOUND_STAR);
            soldier.calculatedPensionPerMonth = soldier.medicalDisabilityPension;

            soldier.AddTrait("Medically Retired Veteran (Combat Disabled)");
            if (!soldier.HasMedal("Order of the Broken Blade"))
            {
                soldier.AddMedal("Order of the Broken Blade");
            }
            if (!soldier.HasMedal("Imperial Wound Star"))
            {
                soldier.AddMedal("Imperial Wound Star");
            }

            GameEvents.OnFeedEventLogged?.Invoke("FORCED RETIREMENT",
                $"<color=#FF8800><b>[MEDICAL EVALUATION BOARD: MANDATORY DISCHARGE]</b></color> " +
                $"The Armed Forces Medical Review Board has ruled your combat wounds ({medicalReason}) permanently incompatible with military service. " +
                $"You have been honorably retired on 100% Combat Disability with full military honors. " +
                $"Awarded guaranteed lifetime disability pension of <b>${soldier.medicalDisabilityPension:N0}/month</b> and the Order of the Broken Blade.");

            LifePersistenceManager.SaveSoldierLife(soldier);
            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }
    }
}
