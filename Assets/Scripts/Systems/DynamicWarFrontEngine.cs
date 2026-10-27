using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Dynamic Living Campaign War Map Engine:
    /// Tracks macro-campaign frontline territory progress across all 6 authentic war theaters (-100 to +100).
    /// Battles shift the front: Decisive victories drive towards enemy capitulation and armistice celebrations;
    /// catastrophic ambushes trigger desperate encirclements and Dunkirk-style evacuations.
    /// </summary>
    public static class DynamicWarFrontEngine
    {
        private static readonly Dictionary<WarTheaterType, int> frontlineProgress = new Dictionary<WarTheaterType, int>();
        private static readonly HashSet<WarTheaterType> liberatedTheaters = new HashSet<WarTheaterType>();

        public const int CAPITULATION_THRESHOLD = 80;
        public const int ENCIRCLEMENT_THRESHOLD = -80;

        public static int GetFrontlineProgress(WarTheaterType theater)
        {
            if (!frontlineProgress.ContainsKey(theater))
            {
                frontlineProgress[theater] = 0; // Neutral baseline
            }
            return frontlineProgress[theater];
        }

        public static void SetFrontlineProgress(WarTheaterType theater, int progress)
        {
            frontlineProgress[theater] = Mathf.Clamp(progress, -100, 100);
        }

        public static bool IsTheaterLiberated(WarTheaterType theater)
        {
            return liberatedTheaters.Contains(theater);
        }

        public static void ResetFrontlines()
        {
            frontlineProgress.Clear();
            liberatedTheaters.Clear();
        }

        /// <summary>
        /// Updates the territorial frontline based on tactical combat mission outcomes.
        /// </summary>
        public static int RecordBattleOutcome(WarTheaterType theater, TacticalOutcomeType outcome, SoldierProfile soldier)
        {
            int delta = 0;
            switch (outcome)
            {
                case TacticalOutcomeType.DecisiveVictory:
                    delta = 20;
                    break;
                case TacticalOutcomeType.TacticalSuccess:
                    delta = 10;
                    break;
                case TacticalOutcomeType.PyrrhicVictory:
                    delta = 5;
                    break;
                case TacticalOutcomeType.TacticalWithdrawal:
                    delta = -5;
                    break;
                case TacticalOutcomeType.CatastrophicAmbush:
                    delta = -20;
                    break;
            }

            int current = GetFrontlineProgress(theater);
            int newProgress = Mathf.Clamp(current + delta, -100, 100);
            frontlineProgress[theater] = newProgress;

            // Check for Armistice / Theater Liberation
            if (newProgress >= CAPITULATION_THRESHOLD && !liberatedTheaters.Contains(theater))
            {
                liberatedTheaters.Add(theater);
                AudioManager.PlayBugleFanfare();

                if (soldier != null)
                {
                    string medalName = $"Campaign Victory Cross ({theater})";
                    if (!soldier.HasMedal(medalName))
                    {
                        soldier.AddMedal(medalName);
                    }
                    soldier.cashBalance += 1500L;
                    soldier.accumulatedPromotionPoints += 25;
                    soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 15f);
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 20f);
                    soldier.AddTrait($"Liberator of {theater}");

                    GameEvents.OnFeedEventLogged?.Invoke("ARMISTICE",
                        $"<color=#FFD700><b>[THEATER VICTORY & ARMISTICE DAY]</b></color> " +
                        $"The hostile 4th Army has formally surrendered in <b>{theater}</b>! Church bells ring across the liberated capital! " +
                        $"Awarded <b>{medalName}</b>, +$1,500 Victory Remittance, and +25 Promotion Points!");
                }
            }
            // Check for Frontline Encirclement Crisis
            else if (newProgress <= ENCIRCLEMENT_THRESHOLD)
            {
                AudioManager.PlayAlertPing();
                GameEvents.OnFeedEventLogged?.Invoke("ENCIRCLEMENT",
                    $"<color=#FF3333><b>[CRITICAL ENCIRCLEMENT ALERT: {theater}]</b></color> " +
                    $"Hostile armored spearheads have shattered friendly flanks! Regimental supply lines cut. " +
                    $"Defensive perimeter established for emergency extraction!");
            }

            return newProgress;
        }

        /// <summary>
        /// Calculates overall macro war dominance across all 5 authentic combat theaters (0% to 100%).
        /// </summary>
        public static float GetMacroWarDominanceScore()
        {
            var theaters = WarTheaterInfo.GetAllTheaters();
            if (theaters == null || theaters.Count == 0) return 50f;

            float totalScore = 0f;
            foreach (var th in theaters)
            {
                int p = GetFrontlineProgress(th.theaterType);
                // Map -100..+100 to 0..100
                totalScore += (p + 100f) / 2f;
            }
            return totalScore / theaters.Count;
        }

        /// <summary>
        /// Formats an authentic military status assessment based on territory progress.
        /// </summary>
        public static string GetTheaterStatusText(WarTheaterType theater)
        {
            if (IsTheaterLiberated(theater))
            {
                return "<color=#00FFAA><b>[LIBERATED] Hostile Army Capitulated • Armistice Enforced</b></color>";
            }

            int p = GetFrontlineProgress(theater);
            if (p >= 50)
            {
                return $"<color=#00D4FF><b>[ALLIED OFFENSIVE • +{p}%]</b> Piercing Hostile Redoubts</color>";
            }
            else if (p > 10)
            {
                return $"<color=#38BDF8><b>[ALLIED ADVANCE • +{p}%]</b> Securing Outer Defensive Perimeter</color>";
            }
            else if (p >= -10)
            {
                return $"<color=#FBBF24><b>[CONTESTED FRONT • {p}%]</b> Fierce Artillery & Trench Stalemate</color>";
            }
            else if (p >= -50)
            {
                return $"<color=#FB923C><b>[ENEMY PRESSURE • {p}%]</b> Friendly Outposts Under Heavy Fire</color>";
            }
            else
            {
                return $"<color=#EF4444><b>[CRITICAL SIEGE • {p}%]</b> Encirclement Perimeter • Urgent Relief Required</color>";
            }
        }

        /// <summary>
        /// Delivers an emergency airdrop supply crate of field medkits, ammo, and rations to turn the tide.
        /// Costs $200 cash, shifts frontline +10%, and grants +12 CO favor.
        /// </summary>
        public static bool AirdropSupplyCrate(WarTheaterType theater, SoldierProfile soldier)
        {
            if (soldier == null || soldier.cashBalance < 200L) return false;

            soldier.cashBalance -= 200L;
            int current = GetFrontlineProgress(theater);
            SetFrontlineProgress(theater, current + 12);

            soldier.coFavor = Mathf.Min(100f, soldier.coFavor + 12f);
            soldier.meritRating = Mathf.Min(100f, soldier.meritRating + 8f);

            AudioManager.PlayRadioSquelch();
            GameEvents.OnFeedEventLogged?.Invoke("AIRDROP",
                $"<color=#00FFAA><b>[EMERGENCY AIRDROP CONFIRMED]</b></color> You arranged an emergency tactical supply drop to <b>{theater}</b> (-$200). " +
                $"Medical supplies and ammunition bolstered the line (+12% Frontline Momentum, +12 CO Standing)!");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }

        /// <summary>
        /// Requests official battalion transfer to a different active war theater.
        /// </summary>
        public static bool RequestTheaterTransfer(WarTheaterType targetTheater, SoldierProfile soldier)
        {
            if (soldier == null) return false;

            soldier.currentTheater = targetTheater;
            soldier.toursInCurrentTheater = 0;

            AudioManager.PlayBugleFanfare();
            var info = WarTheaterInfo.GetInfo(targetTheater);
            GameEvents.OnFeedEventLogged?.Invoke("TRANSFER",
                $"<color=#FFD700><b>[BATTALION TRANSFER APPROVED]</b></color> Your transfer petition was authorized by High Command. " +
                $"You are now assigned to <b>{info.theaterName}</b>! (Danger Level: {info.dangerLevel}/5 | Hazard Pay: ${info.hazardPayRate}/mo).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return true;
        }
    }
}
