using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Milestone 1: Squadmate Comrades & Tactical Command Engine (The "Band of Brothers" Pillar).
    /// Handles squad dynamics, camaraderie, battlefield first aid, casualty checks, replacements,
    /// and tactical command execution for NCOs (E-5+) and Officers (O-1+).
    /// </summary>
    public static class SquadmateCommandEngine
    {
        #region Rank & Command Authority

        /// <summary>
        /// Checks whether the soldier has reached the rank of Sergeant (E-5+) or Officer (O-1+).
        /// </summary>
        public static bool IsNCOOrOfficer(SoldierProfile soldier)
        {
            if (soldier == null) return false;
            if (soldier.isOfficer) return true;

            if (soldier.currentRank != null)
            {
                if (soldier.currentRank.type == PaygradeType.Officer) return true;
                if (soldier.currentRank.type == PaygradeType.Enlisted && soldier.currentRank.rankOrder >= 5) return true;

                if (!string.IsNullOrEmpty(soldier.currentRank.paygrade))
                {
                    string pg = soldier.currentRank.paygrade.Trim().ToUpper();
                    if (pg.StartsWith("O")) return true;
                    if (pg.StartsWith("E") && int.TryParse(pg.Substring(1), out int payNum) && payNum >= 5) return true;
                }

                if (!string.IsNullOrEmpty(soldier.currentRank.rankName))
                {
                    string r = soldier.currentRank.rankName.ToLower();
                    if (r.Contains("sergeant") || r.Contains("lieutenant") || r.Contains("captain") ||
                        r.Contains("major") || r.Contains("colonel") || r.Contains("general"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the soldier has command authority to issue tactical platoon/squad orders.
        /// </summary>
        public static bool CanExecuteTacticalCommand(SoldierProfile soldier)
        {
            if (soldier == null) return false;
            // If currentRank is set, strictly enforce E-5+ Sergeant or O-1+ Officer
            if (soldier.currentRank != null)
            {
                return IsNCOOrOfficer(soldier);
            }
            // If rank is not explicitly set in test scenarios, allow if marked as officer
            return soldier.isOfficer;
        }

        #endregion

        #region Squad Dynamics & Camaraderie

        /// <summary>
        /// Uses 1 cigarette pack, raises relationship (+15f), boosts target and player morale.
        /// </summary>
        public static void ShareCigarettesWithSquad(SoldierProfile soldier, SquadMate targetMate)
        {
            if (soldier == null || targetMate == null) return;

            if (targetMate.isDead)
            {
                GameEvents.OnFeedEventLogged?.Invoke("BARRACKS",
                    $"<color=#FF8800>[IN MEMORIAM]</color> You placed an unlit cigarette on the bunk memorial of {targetMate.GetDisplayName()}.");
                return;
            }

            if (soldier.cigarettesCount <= 0)
            {
                GameEvents.OnFeedEventLogged?.Invoke("BARRACKS",
                    "<color=#FF8800>[OUT OF CIGARETTES]</color> You don't have any cigarette packs left. Visit the PX canteen to purchase more!");
                return;
            }

            soldier.cigarettesCount--;
            targetMate.relationship = Mathf.Min(100f, targetMate.relationship + 15f);
            soldier.happiness = Mathf.Min(100f, soldier.happiness + 10f); // Boosts player morale
            soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 5f);

            GameEvents.OnFeedEventLogged?.Invoke("CAMARADERIE",
                $"<color=#00FFAA><b>[SMOKE BREAK • BROTHERS IN ARMS]</b></color> You shared a pack of PX cigarettes with {targetMate.GetDisplayName()} ({targetMate.roleTitle}). " +
                $"Huddled beneath the trench revetment, you spoke of home and forged an unbreakable bond. (+15 Relationship, +10 Morale).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        /// <summary>
        /// Uses 1 field medkit, saves wounded comrade, grants major loyalty boost (+30f) and adds "Life Saver" trait or medal consideration.
        /// </summary>
        public static void BandageSquadmate(SoldierProfile soldier, SquadMate woundedMate)
        {
            if (soldier == null || woundedMate == null) return;

            if (soldier.fieldMedkitsCount <= 0)
            {
                GameEvents.OnFeedEventLogged?.Invoke("FIELD TRAUMA",
                    "<color=#FF8800>[OUT OF MEDKITS]</color> You reach for your combat webbing, but your emergency field medkits are exhausted!");
                return;
            }

            soldier.fieldMedkitsCount--;
            bool wasDeadOrDying = woundedMate.isDead;
            woundedMate.isDead = false;
            woundedMate.deathReason = null;
            woundedMate.relationship = Mathf.Min(100f, woundedMate.relationship + 30f);
            soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 10f);

            // Add Life Saver trait & medal consideration
            soldier.AddTrait("Life Saver");
            if (!soldier.HasMedal("Imperial Life Saving Cross"))
            {
                soldier.AddMedal("Imperial Life Saving Cross");
            }

            string actionText = wasDeadOrDying
                ? $"You dragged {woundedMate.GetDisplayName()} ({woundedMate.roleTitle}) from rubble, halted arterial hemorrhage, and brought him back from the dead!"
                : $"You applied emergency trauma bandages and combat morphine to {woundedMate.GetDisplayName()} ({woundedMate.roleTitle}) under direct enemy fire!";

            GameEvents.OnFeedEventLogged?.Invoke("HEROIC RESCUE",
                $"<color=#00FFAA><b>[HEROIC FIRST AID • LIFE SAVER]</b></color> {actionText} " +
                $"His life was saved! (+30 Loyalty, 'Life Saver' trait awarded, -1 Field Medkit).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        /// <summary>
        /// Evaluates if a comrade takes casualties during intense frontline fighting,
        /// checking if another squadmate can assist, or logging tragic loss with feed obituary, decreasing player morale.
        /// </summary>
        public static void CheckSquadmateCombatLoss(
            SoldierProfile soldier, 
            WarTheater theater, 
            bool forceLoss = false, 
            SquadMate forcedMate = null, 
            bool allowAssistance = true)
        {
            if (soldier == null) return;
            soldier.InitializeSquadMatesIfEmpty();

            var aliveMates = soldier.squadMates.FindAll(m => !m.isDead);
            if (aliveMates.Count == 0) return;

            SquadMate target = (forcedMate != null && aliveMates.Contains(forcedMate))
                ? forcedMate
                : aliveMates[UnityEngine.Random.Range(0, aliveMates.Count)];

            float hazard = theater != null ? theater.hazardRating : 1.0f;
            string theaterName = theater != null ? theater.theaterName : "The Frontline";

            if (!forceLoss)
            {
                // Frontline casualty check based on theater hazard
                float incidentThreshold = Mathf.Clamp(0.35f * hazard, 0.15f, 0.65f);
                if (UnityEngine.Random.value > incidentThreshold) return;
            }

            // Check if another squadmate can intervene and save the comrade
            if (allowAssistance)
            {
                // 1. Doc Harris (Combat Medic)
                var medic = soldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead && m != target);
                if (medic != null && medic.relationship >= 40f)
                {
                    medic.battlesSurvived++;
                    medic.relationship = Mathf.Min(100f, medic.relationship + 8f);
                    target.relationship = Mathf.Min(100f, target.relationship + 10f);
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("MEDIC RESCUE",
                        $"<color=#00FFAA><b>[MEDIC RESCUE • BROTHERS IN ARMS]</b></color> Shrapnel struck {target.GetDisplayName()}! " +
                        $"{medic.GetDisplayName()} sprinted through a mortar blast, sealed the sucking chest wound, and saved his life!");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    return;
                }

                // 2. Anvil Kowalski (Heavy Gunner)
                var gunner = soldier.squadMates.Find(m => m.role == SquadRole.HeavyGunner && !m.isDead && m != target);
                if (gunner != null && gunner.relationship >= 40f)
                {
                    gunner.battlesSurvived++;
                    gunner.relationship = Mathf.Min(100f, gunner.relationship + 8f);
                    target.relationship = Mathf.Min(100f, target.relationship + 10f);
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("COVERING FIRE",
                        $"<color=#00FFAA><b>[GUNNER SUPPRESSION • SQUAD RESCUE]</b></color> Enemy stormtroopers converged on {target.GetDisplayName()}! " +
                        $"{gunner.GetDisplayName()} laid down a roaring stream of cyclic suppression, driving back the hostile assault and saving his comrade!");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    return;
                }

                // 3. Deadeye Vance (Lead Scout)
                var scout = soldier.squadMates.Find(m => m.role == SquadRole.LeadScout && !m.isDead && m != target);
                if (scout != null && scout.relationship > 50f)
                {
                    scout.battlesSurvived++;
                    scout.relationship = Mathf.Min(100f, scout.relationship + 8f);
                    target.relationship = Mathf.Min(100f, target.relationship + 10f);
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("SCOUT WARNING",
                        $"<color=#00FFAA><b>[SCOUT VIGILANCE • EARLY WARNING]</b></color> {scout.GetDisplayName()} detected a concealed enemy sniper glare, " +
                        $"tackling {target.GetDisplayName()} into the trench mud an instant before the bullet struck!");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    return;
                }
            }

            // Tragic Loss: Squadmate Killed in Action (KIA)
            target.isDead = true;
            target.deathReason = $"Killed in Action during heavy combat in {theaterName}";
            soldier.squadMatesKIA++;
            soldier.happiness = Mathf.Max(0f, soldier.happiness - 15f); // Player morale decreased
            soldier.squadRespect = Mathf.Max(0f, soldier.squadRespect - 5f);

            GameEvents.OnFeedEventLogged?.Invoke("FALLEN COMRADE",
                $"<color=#FF3333><b>[OBITUARY • BROTHER IN ARMS FALLEN]</b></color> <b>{target.GetDisplayName()}</b> ({target.roleTitle}) " +
                $"was Killed in Action in {theaterName}. You held your dying brother in the crater until his breath ceased. (-15 Morale).");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
        }

        public static void CheckSquadmateCombatLoss(SoldierProfile soldier, WarTheaterInfo theaterInfo)
        {
            CheckSquadmateCombatLoss(soldier, (WarTheater)theaterInfo);
        }

        public static void CheckSquadmateCombatLoss(SoldierProfile soldier, WarTheaterType theaterType)
        {
            CheckSquadmateCombatLoss(soldier, (WarTheater)theaterType);
        }

        /// <summary>
        /// Adds a replacement recruit to the squad to fill a KIA vacancy.
        /// </summary>
        public static void RecruitReplacement(SoldierProfile soldier, SquadRole role)
        {
            AddReplacement(soldier, role);
        }

        /// <summary>
        /// Adds a replacement recruit to the squad and returns the created SquadMate.
        /// </summary>
        public static SquadMate AddReplacement(SoldierProfile soldier, SquadRole role)
        {
            if (soldier == null) return null;
            if (soldier.squadMates == null) soldier.squadMates = new List<SquadMate>();

            // Remove dead comrade occupying this role slot if present
            var deadMate = soldier.squadMates.Find(m => m.role == role && m.isDead);
            if (deadMate != null)
            {
                soldier.squadMates.Remove(deadMate);
            }

            SquadMate newMate;
            switch (role)
            {
                case SquadRole.CombatMedic:
                    newMate = new SquadMate("Peter Lang", "Doc", SquadRole.CombatMedic, "Squad Field Medic", "Attentive Field Corps Medic", 50f);
                    break;
                case SquadRole.LeadScout:
                    newMate = new SquadMate("Cole Evans", "Deadeye", SquadRole.LeadScout, "Point Scout", "Sharp-Eyed Marksman", 50f);
                    break;
                case SquadRole.HeavyGunner:
                    newMate = new SquadMate("Roland Pike", "Anvil", SquadRole.HeavyGunner, "Machine Gunner", "Immovable Heavy Weapons Operator", 50f);
                    break;
                case SquadRole.RiflemanRookie:
                default:
                    newMate = new SquadMate("Lucas Reed", "Rookie", SquadRole.RiflemanRookie, "Junior Rifleman", "Eager Replacement Recruit", 50f);
                    break;
            }

            soldier.squadMates.Add(newMate);

            GameEvents.OnFeedEventLogged?.Invoke("REINFORCEMENTS",
                $"<color=#00D4FF><b>[REPLACEMENT RECRUIT ARRIVES]</b></color> Private {newMate.GetDisplayName()} ({newMate.roleTitle}) " +
                $"reported from the replacement depot to fill the fireteam vacancy.");

            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
            return newMate;
        }

        #endregion

        #region NCO & Officer Tactical Command Mode

        /// <summary>
        /// Executes tactical battlefield commands when the soldier is an NCO (Sergeant E-5+) or Officer (O-1+).
        /// Leverages squad roles, computes outcomes, subordinate casualties, and merit rewards.
        /// </summary>
        public static CommandOutcome ExecuteTacticalCommand(
            SoldierProfile leader, 
            TacticalCommandOrder order, 
            WarTheater theater, 
            bool? forceSuccess = null, 
            int? overrideSubordinateCasualties = null)
        {
            if (leader == null)
            {
                return new CommandOutcome(false, 0, 0f, 0f, "No Commander Assigned", "Command failed: No leader assigned.");
            }

            leader.InitializeSquadMatesIfEmpty();

            // Command Authority Check: Must be Sergeant (E-5+) or Officer (O-1+)
            if (leader.currentRank != null && !IsNCOOrOfficer(leader))
            {
                var deniedOutcome = new CommandOutcome(
                    false,
                    0,
                    0f,
                    0f,
                    "Command Authority Denied",
                    $"<color=#FF8800>[AUTHORITY DENIED]</color> {leader.characterName} ({leader.currentRank.paygrade}) lacks tactical command authority. " +
                    "Ordering platoon and squad combat maneuvers requires Sergeant (E-5+) or Commissioned Officer (O-1+) status."
                );
                GameEvents.OnFeedEventLogged?.Invoke("TACTICAL COMMAND", deniedOutcome.feedMessage);
                return deniedOutcome;
            }

            // Identify active squadmates
            var medic = leader.squadMates?.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);
            var scout = leader.squadMates?.Find(m => m.role == SquadRole.LeadScout && !m.isDead);
            var gunner = leader.squadMates?.Find(m => m.role == SquadRole.HeavyGunner && !m.isDead);

            bool scoutBonus = (scout != null && scout.relationship > 50f);
            bool gunnerAlive = (gunner != null);
            bool medicAlive = (medic != null);

            // Base success chance calculation per order
            float baseChance = 50f;
            switch (order)
            {
                case TacticalCommandOrder.FrontalAssault:
                    baseChance = 45f;
                    break;
                case TacticalCommandOrder.ScoutFlankManeuver:
                    baseChance = 50f;
                    break;
                case TacticalCommandOrder.HeavySuppressionPin:
                    baseChance = 60f;
                    break;
                case TacticalCommandOrder.DangerCloseArtilleryCall:
                    baseChance = 40f;
                    break;
                case TacticalCommandOrder.OrderlyFightingRetreat:
                    baseChance = 65f;
                    break;
                case TacticalCommandOrder.HoldDefenseToTheLast:
                    baseChance = 45f;
                    break;
            }

            // Leader leadership, intellect, and combat skill bonuses
            float leaderBonus = (leader.combatSkill * 0.20f) + (leader.intellect * 0.15f);

            // Theater hazard penalty
            float hazard = theater != null ? theater.hazardRating : 1.0f;
            float hazardPenalty = (hazard - 1.0f) * 15f;

            float successChance = baseChance + leaderBonus - hazardPenalty;

            // Squad Role Modifiers:
            // 1. If LeadScout (Deadeye) has high relationship (>50), ScoutFlankManeuver gets +25% success bonus.
            if (order == TacticalCommandOrder.ScoutFlankManeuver && scoutBonus)
            {
                successChance += 25f;
            }

            // 2. Heavy Gunner bonus for suppression & defense
            if (gunnerAlive)
            {
                if (order == TacticalCommandOrder.HeavySuppressionPin) successChance += 15f;
                else if (order == TacticalCommandOrder.HoldDefenseToTheLast) successChance += 10f;
            }

            successChance = Mathf.Clamp(successChance, 10f, 95f);

            bool isSuccess = forceSuccess.HasValue ? forceSuccess.Value : (UnityEngine.Random.value * 100f < successChance);

            // Subordinate casualty calculation
            int rawCasualties = 0;
            if (overrideSubordinateCasualties.HasValue)
            {
                rawCasualties = overrideSubordinateCasualties.Value;
            }
            else
            {
                switch (order)
                {
                    case TacticalCommandOrder.FrontalAssault:
                        rawCasualties = isSuccess ? UnityEngine.Random.Range(2, 5) : UnityEngine.Random.Range(5, 10);
                        break;
                    case TacticalCommandOrder.ScoutFlankManeuver:
                        rawCasualties = isSuccess ? UnityEngine.Random.Range(0, 3) : UnityEngine.Random.Range(3, 7);
                        break;
                    case TacticalCommandOrder.HeavySuppressionPin:
                        rawCasualties = isSuccess ? UnityEngine.Random.Range(3, 6) : UnityEngine.Random.Range(5, 8);
                        break;
                    case TacticalCommandOrder.DangerCloseArtilleryCall:
                        rawCasualties = isSuccess ? UnityEngine.Random.Range(1, 4) : UnityEngine.Random.Range(4, 9);
                        break;
                    case TacticalCommandOrder.OrderlyFightingRetreat:
                        rawCasualties = isSuccess ? UnityEngine.Random.Range(0, 2) : UnityEngine.Random.Range(2, 5);
                        break;
                    case TacticalCommandOrder.HoldDefenseToTheLast:
                        rawCasualties = isSuccess ? UnityEngine.Random.Range(3, 7) : UnityEngine.Random.Range(6, 11);
                        break;
                }
            }

            // Role Mitigation 1: Heavy Gunner (Anvil)
            // If HeavyGunner is alive, HeavySuppressionPin significantly reduces subordinate casualties.
            if (order == TacticalCommandOrder.HeavySuppressionPin && gunnerAlive)
            {
                rawCasualties = Mathf.Max(0, Mathf.FloorToInt(rawCasualties * 0.25f)); // Reduced by 75%
            }
            else if (gunnerAlive && !overrideSubordinateCasualties.HasValue)
            {
                rawCasualties = Mathf.Max(0, rawCasualties - 1);
            }

            // Role Mitigation 2: Combat Medic (Doc)
            // If CombatMedic is alive, wound mortality among troops is halved.
            if (medicAlive)
            {
                rawCasualties = Mathf.FloorToInt(rawCasualties * 0.5f);
            }

            int finalCasualties = Mathf.Max(0, rawCasualties);

            // Compute merit and combat skill changes
            float combatSkillDelta;
            float meritDelta;

            if (isSuccess)
            {
                switch (order)
                {
                    case TacticalCommandOrder.FrontalAssault:
                        combatSkillDelta = 4.0f;
                        meritDelta = 25f;
                        break;
                    case TacticalCommandOrder.ScoutFlankManeuver:
                        combatSkillDelta = 3.5f;
                        meritDelta = 20f;
                        break;
                    case TacticalCommandOrder.HeavySuppressionPin:
                        combatSkillDelta = 2.5f;
                        meritDelta = 18f;
                        break;
                    case TacticalCommandOrder.DangerCloseArtilleryCall:
                        combatSkillDelta = 3.0f;
                        meritDelta = 28f;
                        break;
                    case TacticalCommandOrder.OrderlyFightingRetreat:
                        combatSkillDelta = 2.0f;
                        meritDelta = 12f;
                        break;
                    case TacticalCommandOrder.HoldDefenseToTheLast:
                    default:
                        combatSkillDelta = 5.0f;
                        meritDelta = 35f;
                        break;
                }
            }
            else
            {
                combatSkillDelta = 1.0f; // Tactical experience under harsh fire
                switch (order)
                {
                    case TacticalCommandOrder.FrontalAssault:
                        meritDelta = -8f;
                        break;
                    case TacticalCommandOrder.ScoutFlankManeuver:
                        meritDelta = -5f;
                        break;
                    case TacticalCommandOrder.HeavySuppressionPin:
                        meritDelta = -4f;
                        break;
                    case TacticalCommandOrder.DangerCloseArtilleryCall:
                        meritDelta = -12f;
                        break;
                    case TacticalCommandOrder.OrderlyFightingRetreat:
                        meritDelta = -3f;
                        break;
                    case TacticalCommandOrder.HoldDefenseToTheLast:
                    default:
                        meritDelta = 5f; // Courageous defense recognized despite setback
                        break;
                }
            }

            // Apply updates to leader profile
            leader.combatSkill = Mathf.Min(100f, leader.combatSkill + combatSkillDelta);
            leader.meritRating = Mathf.Clamp(leader.meritRating + meritDelta, 0f, 100f);
            leader.accumulatedPromotionPoints += Mathf.Max(0, (int)meritDelta);

            if (isSuccess)
            {
                leader.squadRespect = Mathf.Min(100f, leader.squadRespect + 5f);
                leader.coFavor = Mathf.Min(100f, leader.coFavor + 4f);

                // Squadmates gain battle experience and morale
                if (leader.squadMates != null)
                {
                    foreach (var mate in leader.squadMates)
                    {
                        if (!mate.isDead)
                        {
                            mate.battlesSurvived++;
                            mate.relationship = Mathf.Min(100f, mate.relationship + 2f);
                        }
                    }
                }
            }
            else
            {
                leader.squadRespect = Mathf.Max(0f, leader.squadRespect - (finalCasualties > 3 ? 5f : 2f));
            }

            // Generate Headline & Feed Narrative
            string headline;
            string theaterName = theater != null ? theater.theaterName : "The Frontline";

            switch (order)
            {
                case TacticalCommandOrder.FrontalAssault:
                    headline = isSuccess ? "Frontal Assault Smashes Enemy Trench" : "Frontal Assault Repelled with Losses";
                    break;
                case TacticalCommandOrder.ScoutFlankManeuver:
                    headline = isSuccess ? "Scout Flank Infiltration Envelops Strongpoint" : "Flanking Force Pinned Down in Crossfire";
                    break;
                case TacticalCommandOrder.HeavySuppressionPin:
                    headline = isSuccess ? "Heavy Suppression Pins Down Hostile Redoubts" : "Suppression Overwhelmed by Enemy Counter-Fire";
                    break;
                case TacticalCommandOrder.DangerCloseArtilleryCall:
                    headline = isSuccess ? "Danger-Close Artillery Annihilates Enemy Pillboxes" : "Artillery Barrage Scatters with Friendly Concussion";
                    break;
                case TacticalCommandOrder.OrderlyFightingRetreat:
                    headline = isSuccess ? "Orderly Fighting Retreat Preserves Unit Cohesion" : "Fighting Retreat Disrupts Defensive Cohesion";
                    break;
                case TacticalCommandOrder.HoldDefenseToTheLast:
                default:
                    headline = isSuccess ? "Heroic Last Stand Repulses Enemy Assault Waves" : "Defensive Redoubt Overrun After Desperate Fight";
                    break;
            }

            // Construct descriptive feed message with role highlights
            string colorTag = isSuccess ? "#00FFAA" : "#FF5555";
            string roleHighlight = "";

            if (order == TacticalCommandOrder.ScoutFlankManeuver && scoutBonus)
            {
                roleHighlight += $" [Lead Scout {scout.nickname} provided route recon: +25% Success Bonus!]";
            }
            if (order == TacticalCommandOrder.HeavySuppressionPin && gunnerAlive)
            {
                roleHighlight += $" [Heavy Gunner {gunner.nickname} laid continuous cyclic suppression, drastically reducing subordinate casualties!]";
            }
            if (medicAlive)
            {
                roleHighlight += $" [Doc {medic.nickname} triaged wounded under fire, halving combat mortality!]";
            }

            string feedMessage = $"<color={colorTag}><b>[TACTICAL COMMAND • {order.ToString().ToUpper()}]</b></color> " +
                $"{headline} in {theaterName}!{roleHighlight} " +
                $"Casualties: {finalCasualties} subordinates. Combat Skill +{combatSkillDelta:F1}, Merit {(meritDelta >= 0 ? "+" : "")}{meritDelta:F0}.";

            var outcome = new CommandOutcome(
                isSuccess,
                finalCasualties,
                combatSkillDelta,
                meritDelta,
                headline,
                feedMessage
            );

            GameEvents.OnFeedEventLogged?.Invoke("TACTICAL COMMAND", outcome.feedMessage);
            GameEvents.OnSoldierStatsUpdated?.Invoke(leader);

            return outcome;
        }

        public static CommandOutcome ExecuteTacticalCommand(SoldierProfile leader, TacticalCommandOrder order, WarTheaterInfo theaterInfo)
        {
            return ExecuteTacticalCommand(leader, order, (WarTheater)theaterInfo);
        }

        public static CommandOutcome ExecuteTacticalCommand(SoldierProfile leader, TacticalCommandOrder order, WarTheaterType theaterType)
        {
            return ExecuteTacticalCommand(leader, order, (WarTheater)theaterType);
        }

        #endregion
    }
}
