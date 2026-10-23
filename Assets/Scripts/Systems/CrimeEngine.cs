using System;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Underworld Crime Engine driving illicit black-market operations, bribes, heists, and law enforcement heat
    /// across Military, Civilian, and Veteran life tracks.
    /// </summary>
    public static class CrimeEngine
    {
        public static CrimeInfo GetCrimeInfo(CrimeType crime)
        {
            switch (crime)
            {
                // Military Crimes
                case CrimeType.SiphonFuelAndRations:
                    return new CrimeInfo("Siphon Fuel & Rations", 800L, 10f, 75f, "Military", "Siphon depot diesel into fuel drums and sell military rations on the local civilian black market.");
                case CrimeType.ArmoryTheft:
                    return new CrimeInfo("Armory Weapons Theft", 3200L, 25f, 60f, "Military", "Smuggle surplus service carbines, optical scopes, and ammunition crates out of the regimental armory.");
                case CrimeType.BarracksGamblingRing:
                    return new CrimeInfo("Barracks Gambling Ring", 1500L, 15f, 70f, "Military", "Run an illicit high-stakes dice and card game behind closed barracks footlockers taking house rake.");
                case CrimeType.FrontlineWarPlunder:
                    return new CrimeInfo("Frontline War Plunder", 5000L, 30f, 65f, "Military", "Loot gold rings, watches, and contraband from captured enemy trenches and bunkers against military directives.");
                case CrimeType.DefenseProcurementBribery:
                    return new CrimeInfo("Defense Procurement Bribery", 35000L, 45f, 50f, "Military", "Accept substantial covert kickbacks from defense contractors to award imperial artillery supply contracts.");

                // Civilian Crimes
                case CrimeType.UndergroundFightClub:
                    return new CrimeInfo("Underground Fight Club", 1200L, 15f, 70f, "Civilian", "Brawl in illegal no-rules warehouse pits for spectator purses and underworld wagers.");
                case CrimeType.ContrabandCourier:
                    return new CrimeInfo("Contraband Courier", 3500L, 20f, 65f, "Civilian", "Run unlicensed alchemical stimulants and black-market crystals through imperial municipal checkpoints.");
                case CrimeType.SyndicateDebtEnforcer:
                    return new CrimeInfo("Syndicate Debt Enforcer", 2000L, 18f, 68f, "Civilian", "Collect overdue loans and enforce street cartel contracts with heavy fists and intimidation.");
                case CrimeType.WarehouseHeist:
                    return new CrimeInfo("Customs Warehouse Heist", 30000L, 40f, 48f, "Civilian", "Organize a team of masked thieves to crack the safe of an imperial commercial customs warehouse.");
                case CrimeType.CorporateEmbezzlement:
                    return new CrimeInfo("Corporate Embezzlement", 18000L, 35f, 55f, "Civilian", "Divert company accounts through dummy ledger accounts and shell corporations.");

                // Veteran Crimes
                case CrimeType.SurplusGunrunning:
                    return new CrimeInfo("Surplus Gunrunning", 75000L, 45f, 45f, "Veteran", "Broker the export of decommissioned military armored vehicles and assault rifles to overseas rebellions.");
                case CrimeType.ShadowPMCWetwork:
                    return new CrimeInfo("Shadow PMC Wetwork", 35000L, 40f, 52f, "Veteran", "Execute an off-the-books wetwork hit contract against a rival oligarch's security retinue.");
                case CrimeType.CityHallGraft:
                    return new CrimeInfo("City Hall Treasury Graft", 120000L, 50f, 42f, "Veteran", "Siphon public infrastructure bond funds into private family trusts via municipal political leverage.");

                default:
                    return new CrimeInfo("Petty Theft", 500L, 10f, 75f, "Civilian", "Street hustling.");
            }
        }

        /// <summary>
        /// Executes an underworld crime attempt.
        /// Evaluates success, grants illicit cash, increases heat, or executes arrests/court-martials on failure.
        /// </summary>
        public static bool CommitCrime(SoldierProfile soldier, CrimeType crime)
        {
            if (soldier == null || soldier.isDead) return false;

            if (soldier.isImprisoned || soldier.militaryBrigSemestersRemaining > 0)
            {
                GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD",
                    "<color=#FF4444>[INCARCERATED]</color> You cannot commit organized underworld crimes while locked behind bars!");
                return false;
            }

            CrimeInfo info = GetCrimeInfo(crime);

            // Eligibility verification
            if (info.category == "Military")
            {
                if (soldier.isCivilian || soldier.isRetired)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD",
                        "<color=#FF8844>[INELIGIBLE]</color> This illicit operation requires active military clearance and garrison access.");
                    return false;
                }
                if (crime == CrimeType.DefenseProcurementBribery && !soldier.isOfficer)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD",
                        "<color=#FF8844>[INSUFFICIENT RANK]</color> Only Commissioned Officers (O-1+) hold procurement contract authority to solicit bribes.");
                    return false;
                }
                if (crime == CrimeType.FrontlineWarPlunder && !soldier.isInCombatDeployment)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD",
                        "<color=#FF8844>[NOT DEPLOYED]</color> War plunder is only available when actively deployed to a frontline combat theater.");
                    return false;
                }
            }
            else if (info.category == "Veteran")
            {
                if (!soldier.isRetired)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD",
                        "<color=#FF8844>[INELIGIBLE]</color> High-level veteran syndicate operations require veteran status and post-service contractor networks.");
                    return false;
                }
                if (crime == CrimeType.CityHallGraft && soldier.veteranCareer != VeteranCareer.CityMayor && soldier.veteranCareer != VeteranCareer.RegionalGovernor && soldier.veteranCareer != VeteranCareer.MinisterOfDefense && soldier.politicalInfluencePoints < 20)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("UNDERWORLD",
                        "<color=#FF8844>[INSUFFICIENT INFLUENCE]</color> Municipal graft requires elected office (Mayor/Governor) or at least 20 Political Influence points.");
                    return false;
                }
            }

            // Calculate Success Chance based on soldier attributes
            float statBonus = 0f;
            switch (crime)
            {
                case CrimeType.SiphonFuelAndRations:
                case CrimeType.BarracksGamblingRing:
                    statBonus = (soldier.intellect - 50f) * 0.35f + (soldier.coFavor - 50f) * 0.15f;
                    break;
                case CrimeType.ArmoryTheft:
                case CrimeType.FrontlineWarPlunder:
                case CrimeType.SurplusGunrunning:
                    statBonus = (soldier.combatSkill - 50f) * 0.30f + (soldier.intellect - 50f) * 0.20f;
                    break;
                case CrimeType.DefenseProcurementBribery:
                case CrimeType.CorporateEmbezzlement:
                case CrimeType.CityHallGraft:
                    statBonus = (soldier.intellect - 50f) * 0.45f + (soldier.politicalInfluence - 50f) * 0.20f;
                    break;
                case CrimeType.UndergroundFightClub:
                case CrimeType.SyndicateDebtEnforcer:
                    statBonus = (soldier.combatSkill - 50f) * 0.40f + (soldier.fitness - 50f) * 0.20f;
                    break;
                case CrimeType.ContrabandCourier:
                case CrimeType.WarehouseHeist:
                    statBonus = (soldier.fitness - 50f) * 0.25f + (soldier.intellect - 50f) * 0.35f;
                    break;
            }

            // Heat penalty: higher existing heat makes authorities more vigilant
            float heatPenalty = soldier.criminalHeat * 0.25f;
            float finalSuccessChance = Mathf.Clamp(info.baseSuccessRate + statBonus - heatPenalty, 15f, 95f);

            soldier.committedCrimeThisSemester = true;
            float roll = UnityEngine.Random.Range(0f, 100f);

            if (roll <= finalSuccessChance)
            {
                // SUCCESS!
                soldier.cashBalance += info.payout;
                soldier.totalCrimeEarnings += info.payout;
                soldier.crimesCommittedCount++;
                soldier.criminalHeat = Mathf.Min(100f, soldier.criminalHeat + info.heatGain);

                GameEvents.OnFeedEventLogged?.Invoke("CRIME SUCCESS",
                    $"<color=#00FFAA><b>[CRIME SUCCESSFUL • {info.crimeName.ToUpper()}]</b></color> " +
                    $"You pulled off the job without leaving evidence! " +
                    $"Loot: <color=#FFD700><b>+${info.payout:N0} Dirty Cash</b></color>. " +
                    $"Criminal Heat increased to <b>{soldier.criminalHeat:F0}%</b> (+{info.heatGain:F0}%).");

                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return true;
            }
            else
            {
                // FAILURE! Handle bust / court-martial / arrest
                soldier.criminalHeat = Mathf.Min(100f, soldier.criminalHeat + (info.heatGain * 1.5f));
                HandleCrimeFailure(soldier, info);
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                return false;
            }
        }

        private static void HandleCrimeFailure(SoldierProfile soldier, CrimeInfo info)
        {
            if (!soldier.isCivilian && !soldier.isRetired)
            {
                // MILITARY JUSTICE: Military Police (MP) & Court-Martial
                soldier.meritRating = Mathf.Max(0f, soldier.meritRating - 35f);
                soldier.coFavor = Mathf.Max(0f, soldier.coFavor - 40f);

                if (soldier.crimesCommittedCount >= 2 || soldier.criminalHeat >= 60f)
                {
                    // Full Court-Martial: Stockade Brig & Demotion
                    soldier.militaryBrigSemestersRemaining = 2; // 1 year in military brig
                    soldier.AddTrait("Dishonorable Conduct");

                    GameEvents.OnFeedEventLogged?.Invoke("COURT-MARTIAL",
                        $"<color=#FF0000><b>[MILITARY POLICE BUST • GENERAL COURT-MARTIAL]</b></color> " +
                        $"The MP Investigation Corps caught you red-handed during {info.crimeName}! " +
                        "The Judge Advocate General court-martialed you: Sentenced to <b>1 Year in the Military Stockade Brig</b>! " +
                        "Merit shattered (-35 Merit, -40 CO Favor). Payroll suspended.");
                }
                else
                {
                    // Minor MP Citation & Short Brig Time
                    soldier.militaryBrigSemestersRemaining = 1; // 6 months in brig
                    GameEvents.OnFeedEventLogged?.Invoke("MILITARY POLICE",
                        $"<color=#FF4444><b>[ARRESTED BY MILITARY POLICE]</b></color> " +
                        $"Base sentries caught you in the act of {info.crimeName}! " +
                        "Sentenced to <b>6 Months in the Battalion Stockade Brig</b> on bread and water rations.");
                }
            }
            else
            {
                // CIVILIAN & VETERAN JUSTICE: Municipal Police & Criminal Record
                soldier.hasCriminalRecord = true;
                soldier.isImprisoned = true;
                soldier.prisonSemestersRemaining = 4; // 2 years in prison

                if (soldier.isCivilian)
                {
                    soldier.civilianJobTitle = "Unemployed Felon";
                    soldier.civilianSalaryPerMonth = 0L;
                }

                GameEvents.OnFeedEventLogged?.Invoke("ARRESTED",
                    $"<color=#FF0000><b>[POLICE ARREST & CONVICTION]</b></color> " +
                    $"Imperial Detectives raided your operation during {info.crimeName}! " +
                    "The Crown Magistrate convicted you of grand felonies: <b>Sentenced to 2 Years (4 Semesters) in Imperial Penitentiary</b>! " +
                    "Permanent criminal record branded.");
            }
        }

        /// <summary>
        /// Called each semester by GameManager to process criminal heat decay and high-heat raids.
        /// </summary>
        public static void ProcessSemesterHeatAndRaids(SoldierProfile soldier)
        {
            if (soldier == null || soldier.isDead) return;

            // 1. Advance Prison Incarceration
            if (soldier.isImprisoned)
            {
                soldier.prisonSemestersRemaining--;
                soldier.criminalHeat = Mathf.Max(0f, soldier.criminalHeat - 25f);

                if (soldier.prisonSemestersRemaining <= 0)
                {
                    soldier.isImprisoned = false;
                    GameEvents.OnFeedEventLogged?.Invoke("RELEASED",
                        "<color=#00FFAA><b>[PAROLE & PRISON RELEASE]</b></color> You completed your sentence and were released from the Imperial Penitentiary! " +
                        "You step out into civilian society with a criminal record. Time to rebuild your life or dive back into the shadows.");
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("PRISON",
                        $"<color=#FF8866>[IMPERIAL PENITENTIARY]</color> Serving time behind iron bars (Semesters Remaining: {soldier.prisonSemestersRemaining}). " +
                        "Free prison gruel provided. Heat cooled by -25%.");
                }
                soldier.committedCrimeThisSemester = false;
                return;
            }

            // 2. Advance Military Brig Incarceration
            if (soldier.militaryBrigSemestersRemaining > 0)
            {
                soldier.militaryBrigSemestersRemaining--;
                soldier.criminalHeat = Mathf.Max(0f, soldier.criminalHeat - 25f);

                if (soldier.militaryBrigSemestersRemaining <= 0)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("BRIG RELEASE",
                        "<color=#00FFAA><b>[RELEASED FROM MILITARY STOCKADE]</b></color> You served your brig detention. " +
                        "Returned to regular duty under probation. Keep your nose clean!");
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("BRIG",
                        $"<color=#FF8866>[BATTALION BRIG]</color> Confined to the concrete stockade cells (Semesters Remaining: {soldier.militaryBrigSemestersRemaining}). " +
                        "No payroll or rank progression granted during confinement.");
                }
                soldier.committedCrimeThisSemester = false;
                return;
            }

            // 3. Natural Heat Decay if laying low (-15% per clean semester)
            if (!soldier.committedCrimeThisSemester && soldier.criminalHeat > 0f)
            {
                soldier.criminalHeat = Mathf.Max(0f, soldier.criminalHeat - 15f);
                if (soldier.criminalHeat == 0f)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("HEAT",
                        "<color=#00FFAA>[HEAT CLEARED]</color> Law enforcement dossiers on you have grown cold. Criminal Heat: <b>0%</b>.");
                }
            }

            // 4. Elevated Criminal Heat Raids (> 50% Heat)
            if (soldier.criminalHeat >= 50f)
            {
                // High-heat raid probability scales from 25% at 50% heat to 70% at 100% heat
                float raidChance = (soldier.criminalHeat - 50f) * 0.90f + 25f;
                float raidRoll = UnityEngine.Random.Range(0f, 100f);

                if (raidRoll <= raidChance)
                {
                    // RAID INITIATED!
                    if (!soldier.isCivilian && !soldier.isRetired)
                    {
                        soldier.militaryBrigSemestersRemaining = 2;
                        soldier.criminalHeat = 20f;
                        soldier.meritRating = Mathf.Max(0f, soldier.meritRating - 25f);

                        // Seize illicit cash
                        long seized = Math.Min(soldier.cashBalance, 10000L);
                        soldier.cashBalance -= seized;

                        GameEvents.OnFeedEventLogged?.Invoke("MP RAID",
                            $"<color=#FF0000><b>[MILITARY POLICE DAWN RAID • HIGH HEAT {soldier.criminalHeat:F0}%]</b></color> " +
                            $"MP investigators kicked open your barracks locker! Seized ${seized:N0} in contraband cash. " +
                            "Sentenced to <b>1 Year in the Stockade Brig</b>.");
                    }
                    else
                    {
                        soldier.isImprisoned = true;
                        soldier.prisonSemestersRemaining = 4;
                        soldier.hasCriminalRecord = true;
                        soldier.criminalHeat = 20f;

                        long seized = Math.Min(soldier.cashBalance, 25000L);
                        soldier.cashBalance -= seized;

                        GameEvents.OnFeedEventLogged?.Invoke("SWAT RAID",
                            $"<color=#FF0000><b>[POLICE TACTICAL RAID • HIGH HEAT {soldier.criminalHeat:F0}%]</b></color> " +
                            $"Armed SWAT detectives stormed your residence! Evidence seized with ${seized:N0} cash. " +
                            "Sentenced to <b>2 Years in Imperial Penitentiary</b>.");
                    }
                }
            }

            soldier.committedCrimeThisSemester = false;
        }

        public static string GetHeatBarString(float heatFloat)
        {
            int heat = Mathf.Clamp(Mathf.RoundToInt(heatFloat), 0, 100);
            int filled = heat / 10;
            int empty = 10 - filled;
            string bar = new string('█', filled) + new string('░', empty);
            string status;
            if (heat >= 75) status = "<color=#FF2222>WANTED FELON / MP RAID IMMINENT</color>";
            else if (heat >= 45) status = "<color=#FFAA00>PROVOST MARSHAL WATCHLIST</color>";
            else if (heat >= 20) status = "<color=#FFD700>SUSPICIOUS</color>";
            else status = "<color=#00FFAA>CLEAN</color>";

            return $"[{bar}] {heat}% ({status})";
        }
    }

    public struct CrimeInfo
    {
        public string crimeName;
        public long payout;
        public float heatGain;
        public float baseSuccessRate;
        public string category;
        public string description;

        public CrimeInfo(string name, long payout, float heat, float success, string category, string desc)
        {
            this.crimeName = name;
            this.payout = payout;
            this.heatGain = heat;
            this.baseSuccessRate = success;
            this.category = category;
            this.description = desc;
        }
    }
}
