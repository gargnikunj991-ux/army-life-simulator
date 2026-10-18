using ArmyLife.Data;
using ArmyLife.Systems;
using UnityEngine;

namespace ArmyLife.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Active Character State")]
        public SoldierProfile currentSoldier;

        [Header("Data Registries")]
        public PowerTierSO[] powerTierDatabase;
        public MilitaryRankSO[] militaryRankDatabase;
        public FamilyLineageSO[] familyLineageDatabase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Do not force-spawn Ethan Vance if no soldier is loaded.
            // OpeningSceneController will handle showing the menu or character creation.
        }

        public void ResumeLife(string lifeId)
        {
            SoldierProfile loaded = LifePersistenceManager.LoadSoldierLife(lifeId, this);
            if (loaded != null)
            {
                currentSoldier = loaded;
                GameEvents.OnFeedEventLogged?.Invoke("RESUME",
                    $"<color=#00D4FF><b>[SERVICE RECORD RESUMED]</b></color> Welcome back to active duty, <b>{currentSoldier.characterName}</b>! " +
                    $"Rank: <b>{(currentSoldier.currentRank != null ? currentSoldier.currentRank.rankName : "Private")} ({(currentSoldier.currentRank != null ? currentSoldier.currentRank.paygrade : "E-1")})</b> | " +
                    $"Age: <b>{currentSoldier.age:F1}</b> | Cultivation: <b>Tier {currentSoldier.powerTier}/12</b>.");

                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
            }
        }

        public FamilyLineageSO RollBirthLineage()
        {
            if (familyLineageDatabase == null || familyLineageDatabase.Length == 0) return null;

            int roll = Random.Range(1, 101); // 1 to 100

            string targetArchetype;
            if (roll <= 80) // 80% Commoner (8 out of 10)
            {
                targetArchetype = "Civilian Clean Slate";
            }
            else if (roll <= 94) // 14% Military Baron House
            {
                targetArchetype = "Command Sgt Major Backbone";
            }
            else if (roll <= 99) // 5% Special Forces Viscount House
            {
                targetArchetype = "Special Forces Legend";
            }
            else // 1% Grand Duke Dynasty (Ultra-Rare!)
            {
                targetArchetype = "4-Star General Dynasty";
            }

            foreach (var lin in familyLineageDatabase)
            {
                if (lin != null && lin.archetypeName == targetArchetype)
                {
                    return lin;
                }
            }

            return familyLineageDatabase[0];
        }

        public void StartNewLife(
            string soldierName, 
            FamilyLineageSO lineage = null, 
            string gender = "Male", 
            string birthCountry = "The Valerian Empire", 
            string hairStyle = "Military Buzzcut",
            string hairColor = "Jet Black",
            string eyeColor = "Steel Grey",
            string skinTone = "Tan",
            float looks = 80f,
            int? explicitTalent = null)
        {
            if (lineage == null)
            {
                lineage = RollBirthLineage();
            }

            int talent = explicitTalent ?? Random.Range(40, 185);

            currentSoldier = new SoldierProfile(soldierName, lineage, talent)
            {
                gender = gender,
                birthCountry = birthCountry,
                hairStyle = hairStyle,
                hairColor = hairColor,
                eyeColor = eyeColor,
                skinTone = skinTone,
                looks = looks
            };

            // Split first and last name
            string[] parts = soldierName.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0) currentSoldier.firstName = parts[0];
            if (parts.Length > 1) currentSoldier.lastName = parts[parts.Length - 1];
            
            // Clock starts at Year 2008 for newborn baby (turns 18 in 2026!)
            if (TimeCycleManager.Instance != null)
            {
                TimeCycleManager.Instance.ResetClockForNewborn(2008);
            }

            // Newborn starts with no military rank (enlists at age 18)
            currentSoldier.currentRank = null;

            // Save new life into persistent storage
            LifePersistenceManager.SaveSoldierLife(currentSoldier);

            GameEvents.OnFeedEventLogged?.Invoke("BIRTH",
                $"<color=#00FFAA><b>[A NEW LIFE BEGINS]</b></color> You were born in <b>{birthCountry}</b> as <b>{soldierName}</b> ({gender}). " +
                $"Appearance: <i>{hairStyle}, {hairColor} hair, {eyeColor} eyes, {skinTone} complexion ({looks:F0}% Looks)</i>. " +
                $"Lineage: <b>{(lineage != null ? lineage.archetypeName : "Civilian Grunt")}</b>. " +
                $"Hereditary Noble Rank: <b>{currentSoldier.nobleTitle}</b>.");

            if (currentSoldier.isPersonalTalentRevealed)
            {
                GameEvents.OnFeedEventLogged?.Invoke("LINEAGE",
                    $"<color=#FFD700>[NOBLE PRIVILEGE]</color> Your noble family tested your talent at birth with their ancestral stone: <b>{currentSoldier.talentScore} / 200</b>!");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        /// <summary>
        /// Advances the game by 6 months (1 turn) with realistic mortality, retirement, and breakthrough risks.
        /// </summary>
        public void AdvanceTurn()
        {
            if (currentSoldier == null) return;

            // 0. If soldier is dead, trigger reincarnation option
            if (currentSoldier.isDead)
            {
                GameEvents.OnFeedEventLogged?.Invoke("REINCARNATION",
                    "<color=#FFD700><b>[DYNASTIC SUCCESSION]</b></color> Your previous life has concluded. Reincarnating as the youngest descendant of the noble house...");
                
                StartNewLife($"{currentSoldier.characterName} II", currentSoldier.lineage);
                return;
            }

            // 1. Advance Calendar Clock
            if (TimeCycleManager.Instance != null)
            {
                TimeCycleManager.Instance.AdvanceSemester(currentSoldier);
            }
            else
            {
                currentSoldier.age += 0.5f;
                currentSoldier.totalSemestersLived++;
            }

            // 2. Natural Lifespan Check based on Power Tier
            PowerTierSO currentTierSO = GetPowerTierSO(currentSoldier.powerTier);
            int maxLifespan = (currentTierSO != null) ? currentTierSO.maxLifespanYears : 80;

            if (currentSoldier.age >= maxLifespan)
            {
                currentSoldier.isDead = true;
                currentSoldier.deathReason = $"Peaceful passing of old age after living a full lifespan of {currentSoldier.age:F0} years (Tier {currentSoldier.powerTier} limit)";

                GameEvents.OnFeedEventLogged?.Invoke("DEATH",
                    $"<color=#FFD700><b>[NATURAL PASSING OF OLD AGE]</b></color> Having reached the biological ceiling of Tier {currentSoldier.powerTier} ({maxLifespan} Years), " +
                    $"you passed away peacefully surrounded by children and grandchildren. Your name will be remembered in the clan ancestral hall!");

                GameEvents.OnImperialExecutionTriggered?.Invoke(currentSoldier.deathReason);
                GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
                return;
            }

            // 3. Phase Splitting: Youth Childhood (Ages 0 to 18) vs Active Duty Military
            if (currentSoldier.isYouthPhase)
            {
                ChildhoodLifeEngine.ProcessYouthSemester(currentSoldier, this);
            }
            else
            {
                // Turning Point: Age 18.0 Enlistment & National Military Ranking Exam!
                if (currentSoldier.currentRank == null)
                {
                    ChildhoodLifeEngine.ConductNationalMilitaryExam(currentSoldier, this);
                }

                // Mandatory Military Retirement Evaluation ("Up or Out" policy)
                MilitaryCareerEngine.EvaluateCareerRetirement(currentSoldier);

                // Frontline Combat Tour Processing (Real mortality & casualty risk!)
                if (!currentSoldier.isRetired && currentSoldier.isInCombatDeployment)
                {
                    MilitaryCareerEngine.ProcessCombatDeploymentTour(currentSoldier);

                    // If killed in action, stop further processing this turn
                    if (currentSoldier.isDead) return;

                    // Check Secret Ancient Sect Cave anomaly (1 in 100 chance for Tier 10+)
                    PowerCultivationEngine.CheckSecretCaveAnomaly(currentSoldier);
                }

                // Payday: 6 months of compensation with 50% Imperial War Tax
                if (currentSoldier.isRetired)
                {
                    long pension = 1500L * 6;
                    currentSoldier.cashBalance += pension;
                    GameEvents.OnFeedEventLogged?.Invoke("PENSION",
                        $"[VETERAN PENSION] Received 6 months of military retirement pension: <b>+${pension:N0}</b> ($1,500/mo).");
                }
                else
                {
                    long gross = SalaryCalculator.CalculateGrossMonthlyPay(currentSoldier);
                    long tax = SalaryCalculator.CalculateMonthlyTax(currentSoldier);
                    long net = SalaryCalculator.CalculateNetMonthlyPay(currentSoldier);
                    long semesterNet = net * 6;
                    long semesterTax = tax * 6;

                    currentSoldier.cashBalance += semesterNet;

                    GameEvents.OnFeedEventLogged?.Invoke("PAYDAY",
                        $"<b>[MILITARY PAY & 50% WAR TAX]</b> Gross: ${gross:N0}/mo | " +
                        $"<color=#FF4444>50% Imperial War Tax: -${tax:N0}/mo (-${semesterTax:N0})</color> | " +
                        $"<b>Net Take-Home: +${semesterNet:N0}</b> (+${net:N0}/mo). Bank: <b>${currentSoldier.cashBalance:N0}</b>.");

                    // Basic living expenses & low-level military resources (rations, uniform maintenance, bone balms)
                    int basicExpenses = 600; // $100/mo for basic supplies
                    currentSoldier.cashBalance = System.Math.Max(0, currentSoldier.cashBalance - basicExpenses);
                    GameEvents.OnFeedEventLogged?.Invoke("EXPENSES",
                        $"[LIVING EXPENSES] Deducted <b>-${basicExpenses}</b> for 6 months of mess hall rations, uniform upkeep, and basic bone-tempering ointments.");

                    // Cultivation Energy Burn (Only if soldier is noble or has learned about cultivation)
                    if (currentSoldier.knowsAboutCultivation || currentSoldier.nobleTitle > NobleRank.Commoner)
                    {
                        PowerCultivationEngine.ProcessSemesterCultivation(currentSoldier, currentTierSO);
                    }

                    // Military Promotion Board Evaluation (Climbs through PT tests, merit, and service!)
                    PromotionBoardEngine.EvaluatePromotionBoard(currentSoldier, militaryRankDatabase);
                }
            }

            // Auto-save soldier profile to persistent storage
            LifePersistenceManager.SaveSoldierLife(currentSoldier);

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        /// <summary>
        /// Sends money home to improve the family's living conditions during the wartime economy.
        /// </summary>
        public void SendMoneyToFamily(long amount)
        {
            if (currentSoldier == null || currentSoldier.cashBalance < amount || amount <= 0)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                    "<color=#FF8800>[INSUFFICIENT FUNDS]</color> You do not have enough cash to send this remittance home.");
                return;
            }

            currentSoldier.cashBalance -= amount;
            currentSoldier.totalSentToFamily += amount;

            // Improve family living condition (0-100)
            float improvement = (amount / 300f);
            currentSoldier.familyLivingCondition = Mathf.Min(100f, currentSoldier.familyLivingCondition + improvement);
            currentSoldier.clanFavor = Mathf.Min(100f, currentSoldier.clanFavor + 6f);

            string tierStr;
            if (currentSoldier.familyLivingCondition >= 80f) tierStr = "<color=#FFD700>PROMINENT LOCAL NOBILITY</color>";
            else if (currentSoldier.familyLivingCondition >= 55f) tierStr = "<color=#00FFAA>AFFLUENT & COMFORTABLE</color>";
            else if (currentSoldier.familyLivingCondition >= 35f) tierStr = "<color=#00AAFF>STABLE WORKING CLASS</color>";
            else tierStr = "<color=#FF8844>IMPOVERISHED WARTIME HOUSEHOLD</color>";

            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                $"<color=#00FFAA><b>[FAMILY REMITTANCE SENT]</b></color> You sent <b>${amount:N0}</b> home to your family! " +
                $"Your parents bought proper medicine and nutrition for your younger siblings. " +
                $"Family Living Condition: <b>{tierStr} ({currentSoldier.familyLivingCondition:F0}/100)</b>.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public void PromoteSoldier(MilitaryRankSO nextRank)
        {
            if (currentSoldier == null || nextRank == null || currentSoldier.isRetired || currentSoldier.isDead) return;

            currentSoldier.currentRank = nextRank;
            currentSoldier.totalPromotionsEarned++;

            GameEvents.OnFeedEventLogged?.Invoke("PROMOTION",
                $"<color=#00FFAA><b>[PROMOTION APPROVED!]</b></color> You have been promoted to <b>{nextRank.rankName} ({nextRank.paygrade})</b>! " +
                $"New Base Salary: ${nextRank.baseMonthlySalary:N0}/mo.");

            if (nextRank.unlocksSelfTalentTesting && !currentSoldier.isPersonalTalentRevealed)
            {
                currentSoldier.knowsAboutCultivation = true;
                GameEvents.OnFeedEventLogged?.Invoke("CLEARANCE",
                    "<color=#00AAFF>[SECURITY CLEARANCE GRANTED]</color> Reaching rank E-4 grants clearance to requisition the Army Resonance Chamber to officially test your personal talent!");
            }

            if (currentSoldier.totalPromotionsEarned == 3 && !currentSoldier.isMarried)
            {
                GameEvents.OnThreePromotionsReached?.Invoke();
                GameEvents.OnFeedEventLogged?.Invoke("MARRIAGE",
                    "<color=#FF88FF><b>[FAMILY ARRANGED MARRIAGE]</b></color> Having proven your military capability with 3 promotions, your parents have formally arranged a marriage alliance for you!");
            }

            GameEvents.OnSoldierStatsUpdated?.Invoke(currentSoldier);
        }

        public PowerTierSO GetPowerTierSO(int tierLevel)
        {
            if (powerTierDatabase == null) return null;
            foreach (var tier in powerTierDatabase)
            {
                if (tier != null && tier.tierLevel == tierLevel) return tier;
            }
            return null;
        }

        public FamilyLineageSO GetDefaultLineage()
        {
            if (familyLineageDatabase != null && familyLineageDatabase.Length > 0)
            {
                return familyLineageDatabase[0];
            }
            return null;
        }
    }
}
