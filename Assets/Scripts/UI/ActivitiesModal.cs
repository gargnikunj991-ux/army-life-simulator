using System;
using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public enum ActivityCategory
    {
        Directory,
        TrainingAndDrills,
        BarracksAndSquad,
        Quartermaster,
        Medical,
        CareerAndDuty
    }

    /// <summary>
    /// Streamlined, high-polish Military Duty & Base Operations Modal.
    /// Pure military operations: Drills, Squad Bonding, Armory Gear, Medical Care, and Career Advancement.
    /// Zero fluff, zero fantasy bloat.
    /// </summary>
    public class ActivitiesModal : MonoBehaviour
    {
        public static ActivitiesModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;
        public Button closeButton;

        [Header("Scroll Hierarchy")]
        public ScrollRect activitiesScrollRect;
        public Transform activitiesContainer;

        private ActivityCategory currentCategory = ActivityCategory.Directory;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            EnsureUIHierarchy();

            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            EnsureUIHierarchy();
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void EnsureUIHierarchy()
        {
            if (activitiesScrollRect == null)
            {
                activitiesScrollRect = GetComponentInChildren<ScrollRect>(true);
            }

            if (activitiesScrollRect != null)
            {
                activitiesScrollRect.horizontal = false;
                activitiesScrollRect.vertical = true;
                activitiesScrollRect.scrollSensitivity = 45f;
                activitiesScrollRect.movementType = ScrollRect.MovementType.Clamped;

                if (activitiesContainer == null && activitiesScrollRect.content != null)
                {
                    activitiesContainer = activitiesScrollRect.content;
                }
            }

            if (modalRoot == null)
            {
                var overlay = transform.Find("ActivitiesOverlay");
                modalRoot = (overlay != null) ? overlay.gameObject : gameObject;
            }
        }

        public void OpenModal()
        {
            EnsureUIHierarchy();
            currentCategory = ActivityCategory.Directory;

            if (modalRoot != null) modalRoot.SetActive(true);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            RefreshUI();

            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            ApplyThemePalette(p);
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);

            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                GameEvents.OnSoldierStatsUpdated?.Invoke(GameManager.Instance.currentSoldier);
            }
        }

        public void SetCategory(ActivityCategory category)
        {
            currentCategory = category;
            RefreshUI();

            if (activitiesScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                activitiesScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void RefreshUI()
        {
            if (activitiesContainer == null) return;

            // Clear old items
            for (int i = activitiesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(activitiesContainer.GetChild(i).gameObject);
            }

            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            UpdateHeader(s);

            if (currentCategory == ActivityCategory.Directory)
            {
                BuildDirectory(s);
            }
            else
            {
                CreateBackToDirectoryButton();
                switch (currentCategory)
                {
                    case ActivityCategory.TrainingAndDrills:
                        BuildTrainingDrills(s);
                        break;
                    case ActivityCategory.BarracksAndSquad:
                        BuildBarracksSquad(s);
                        break;
                    case ActivityCategory.Quartermaster:
                        BuildQuartermaster(s);
                        break;
                    case ActivityCategory.Medical:
                        BuildMedical(s);
                        break;
                    case ActivityCategory.CareerAndDuty:
                        BuildCareerAdvancement(s);
                        break;
                }
            }
        }

        private void UpdateHeader(SoldierProfile s)
        {
            if (titleText != null)
            {
                titleText.text = currentCategory == ActivityCategory.Directory 
                    ? "BASE DUTIES & MILITARY OPERATIONS" 
                    : $"BASE OPERATIONS — {currentCategory.ToString().ToUpper()}";
            }

            if (subtitleText != null)
            {
                int ap = s.actionPointsRemaining;
                int maxAp = s.isInCombatDeployment ? 4 : 3;
                subtitleText.text = $"Soldier: <b>{s.characterName}</b>  |  Rank: <b>{s.GetRankDisplayName()}</b>  |  Stamina: <b>{ap}/{maxAp} Action Points</b>  |  Funds: <b>${s.cashBalance:N0}</b>";
            }
        }

        private void BuildDirectory(SoldierProfile s)
        {
            CreateCategoryTile("FIELD DRILLS & COMBAT TRAINING",
                "Live-fire weapons qualification, marksmanship, tactical breaching, and physical PT.",
                4, () => SetCategory(ActivityCategory.TrainingAndDrills));

            CreateCategoryTile("BARRACKS & FIRETEAM LIFE",
                "Chow hall mess with squadmates, barracks recreation, unit inspection, and letters home.",
                4, () => SetCategory(ActivityCategory.BarracksAndSquad));

            CreateCategoryTile("ARMORY & POST EXCHANGE (PX)",
                "Standard issue ballistic body armor, combat optics, tactical trauma kits, and supplies.",
                5, () => SetCategory(ActivityCategory.Quartermaster));

            CreateCategoryTile("FIELD HOSPITAL & MEDICAL CARE",
                "Combat wound surgery, physical rehabilitation, trauma recovery, and psychological decompression.",
                4, () => SetCategory(ActivityCategory.Medical));

            CreateCategoryTile("CAREER, PROMOTIONS & SPECIAL FORCES",
                "Promotion board review, Special Forces Q-Course, Officer Candidate School, and tour extensions.",
                4, () => SetCategory(ActivityCategory.CareerAndDuty));
        }

        #region Category 1: Field Drills & Combat Training
        private void BuildTrainingDrills(SoldierProfile s)
        {
            CreateActivityCard("Live-Fire Marksmanship Qualification (1 AP)",
                "Report to the automated combat firing range. Practice trigger control, target acquisition, and rapid reload drills (+Combat Skill, +Merit).",
                new Color(0.24f, 0.45f, 0.65f), () =>
                {
                    if (!s.TryConsumeActionPoint("Firing Range"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    float gain = UnityEngine.Random.Range(3.5f, 6.0f) * s.StudySkillGainMultiplier;
                    s.combatSkill = Mathf.Min(100f, s.combatSkill + gain);
                    s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 3f);
                    s.combatMeritPoints += 25;
                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING", $"[MARKSMANSHIP] Scored sharpshooter qualification on the live-fire range (+{gain:F1}% Combat Skill, +25 CMP).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Physical Training (PT) & Ruck March (1 AP)",
                "Undertake a rigorous 12-mile weighted ruck march with full combat kit and obstacle course (+Fitness, +Health, -Stress).",
                new Color(0.25f, 0.55f, 0.35f), () =>
                {
                    if (!s.TryConsumeActionPoint("PT Drill"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    float fitGain = UnityEngine.Random.Range(3.0f, 5.5f);
                    s.fitness = Mathf.Min(100f, s.fitness + fitGain);
                    s.health = Mathf.Min(100f, s.health + 2f);
                    s.combatStress = Mathf.Max(0f, s.combatStress - 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING", $"[PT DRILL] Completed grueling 12-mile road march in standard time (+{fitGain:F1}% Fitness, +2% Health).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Tactical Room Clearing & Breaching Drills (1 AP)",
                "Conduct live-action kill-house urban clearing with your fireteam. Focus on cornering, suppressive crossfire, and flashbang entry (+Combat Skill, +Discipline).",
                new Color(0.60f, 0.40f, 0.20f), () =>
                {
                    if (!s.TryConsumeActionPoint("CQB Drill"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    s.combatSkill = Mathf.Min(100f, s.combatSkill + 4f);
                    s.discipline = Mathf.Min(100f, s.discipline + 3f);
                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING", "[CQB TACTICS] Executed flawless urban breaching exercise with fireteam (+4% Combat Skill, +3% Discipline).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Military Doctrine & Field Manual Studies (1 AP)",
                "Study battlefield tactical doctrine, call-for-fire coordinates, radio communications, and staff logistics in the base library (+Intellect, +Promotion Points).",
                new Color(0.35f, 0.35f, 0.55f), () =>
                {
                    if (!s.TryConsumeActionPoint("Study Doctrine"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    s.intellect = Mathf.Min(100f, s.intellect + 4.5f);
                    s.meritRating = Mathf.Min(100f, s.meritRating + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("TRAINING", "[DOCTRINE] Mastered field operational manual & artillery coordinate protocols (+4.5% Intellect, +5 Merit).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }
        #endregion

        #region Category 2: Barracks & Fireteam Life
        private void BuildBarracksSquad(SoldierProfile s)
        {
            CreateActivityCard("Chow Hall Feast with Squad ($25, 1 AP)",
                "Buy hot steaks and share hearty rations at the base mess hall with your fireteam (+Morale, +Squad Loyalty).",
                new Color(0.25f, 0.55f, 0.35f), () =>
                {
                    if (s.cashBalance < 25)
                    {
                        ShowFundsNotice(25);
                        return;
                    }
                    if (!s.TryConsumeActionPoint("Chow Hall"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    s.cashBalance -= 25;
                    s.happiness = Mathf.Min(100f, s.happiness + 12f);
                    s.squadRespect = Mathf.Min(100f, s.squadRespect + 8f);
                    GameEvents.OnFeedEventLogged?.Invoke("BARRACKS", "[SQUAD BONDING] Shared a hearty dinner and banter with your squad in the dining facility (+12% Morale, +8 Squad Loyalty).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Barracks Cards & Downtime (1 AP)",
                "Kick back on a bunk with a card game, radio broadcast, and letters from home (+Morale, -10 Combat Stress).",
                new Color(0.35f, 0.45f, 0.60f), () =>
                {
                    if (!s.TryConsumeActionPoint("Barracks Rest"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    s.happiness = Mathf.Min(100f, s.happiness + 8f);
                    s.combatStress = Mathf.Max(0f, s.combatStress - 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("BARRACKS", "[REST & RECREATION] Enjoyed quiet barracks downtime with the unit. Restored mental resilience (-10 Combat Stress).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Unit Stand-To & Locker Inspection (1 AP)",
                "Polish combat boots, press uniform, and ensure all field gear is razor-sharp for command inspection (+Discipline, +CO Favor).",
                new Color(0.50f, 0.45f, 0.25f), () =>
                {
                    if (!s.TryConsumeActionPoint("Inspection"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    s.discipline = Mathf.Min(100f, s.discipline + 6f);
                    s.coFavor = Mathf.Min(100f, s.coFavor + 5f);
                    GameEvents.OnFeedEventLogged?.Invoke("BARRACKS", "[INSPECTION] First Sergeant commended your locker order and weapon maintenance (+6% Discipline, +5 CO Favor).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Write Dispatch Letter Home to Family",
                "Write a heartfelt letter to your parents and family recounting garrison service and sending warm regards (+15 Family Relationship).",
                new Color(0.40f, 0.35f, 0.50f), () =>
                {
                    s.parentRelationship = Mathf.Min(100f, s.parentRelationship + 15f);
                    s.siblingRelationship = Mathf.Min(100f, s.siblingRelationship + 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("FAMILY", "[MAIL CALL] Sent a detailed dispatch letter home to your family. They sent warm prayers for your safe return (+15 Family Bond).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }
        #endregion

        #region Category 3: Armory & Post Exchange (PX)
        private void BuildQuartermaster(SoldierProfile s)
        {
            CreateGearBuyCard(s, "Standard Issue Ballistic Vest ($450)", 450,
                "Ceramic-reinforced body armor plate. Drastically reduces the severity of combat wounds and shrapnel.",
                "Ballistic Armor Vest", () => s.hasBallisticVest);

            CreateGearBuyCard(s, "Kevlar Combat Helmet ($350)", 350,
                "Standard issue tactical ballistic helmet with NVG shroud (-15% casualty risk).",
                "Kevlar Helmet", () => s.hasSteelHelmet);

            CreateGearBuyCard(s, "Tactical IFAK Trauma Medical Kit ($200)", 200,
                "Individual first aid kit with tourniquets, hemostatic gauze, and chest seals (+Field Survivability).",
                "Tactical Trauma Kit", () => s.fieldMedkitsCount > 0);

            CreateGearBuyCard(s, "Service Combat Sidearm ($280)", 280,
                "Sidearm backup pistol for close-quarters emergency self-defense.",
                "Service Sidearm", () => s.hasServiceSidearm);

            CreateActivityCard("PX Morale Care Package ($80)",
                "Purchase quality snacks, coffee, grooming essentials, and letters from the PX canteen (+15% Morale).",
                new Color(0.35f, 0.55f, 0.40f), () =>
                {
                    if (s.cashBalance < 80)
                    {
                        ShowFundsNotice(80);
                        return;
                    }
                    s.cashBalance -= 80;
                    s.happiness = Mathf.Min(100f, s.happiness + 15f);
                    GameEvents.OnFeedEventLogged?.Invoke("PX", "[CARE PACKAGE] Purchased fresh roasted coffee, premium rations, and essentials at the PX (+15% Morale).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }

        private void CreateGearBuyCard(SoldierProfile s, string title, long cost, string desc, string itemName, Func<bool> alreadyOwned)
        {
            bool owned = alreadyOwned();
            string status = owned ? " <color=#38A65E>[EQUIPPED]</color>" : "";
            Color btnCol = owned ? new Color(0.20f, 0.25f, 0.30f) : new Color(0.20f, 0.55f, 0.35f);

            CreateActivityCard(title + status, desc, btnCol, () =>
            {
                if (owned)
                {
                    GameEvents.OnFeedEventLogged?.Invoke("ARMORY", $"You already have {itemName} equipped in your standard combat kit.");
                    return;
                }
                if (s.cashBalance < cost)
                {
                    ShowFundsNotice(cost);
                    return;
                }
                s.cashBalance -= cost;
                if (itemName == "Ballistic Armor Vest") s.hasBallisticVest = true;
                else if (itemName == "Kevlar Helmet") s.hasSteelHelmet = true;
                else if (itemName == "Tactical Trauma Kit") s.fieldMedkitsCount++;
                else if (itemName == "Service Sidearm") s.hasServiceSidearm = true;

                s.combatReadiness = Mathf.Min(100f, s.combatReadiness + 10f);
                GameEvents.OnFeedEventLogged?.Invoke("ARMORY", $"[PX ARMORY] Purchased and equipped <b>{itemName}</b>! Combat readiness improved (+10%).");
                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                RefreshUI();
            });
        }
        #endregion

        #region Category 4: Field Hospital & Medical Care
        private void BuildMedical(SoldierProfile s)
        {
            bool hasWounds = s.inMedicalConvalescence || s.health < 80f;
            CreateActivityCard("Medical Ward Consultation & Health Check",
                $"Report to the regimental aid station. Current Health: <b>{s.health:F0}%</b> | Wounds: {(hasWounds ? "<color=#FF4444>Active Battlefield Wounds</color>" : "<color=#38A65E>Fit for Duty</color>")}.",
                new Color(0.25f, 0.45f, 0.60f), () =>
                {
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", $"[AID STATION] Battalion surgeon completed full physical examination. Vitals stable at {s.health:F0}% Health.");
                    RefreshUI();
                });

            CreateActivityCard("Emergency Surgical Treatment & Shrapnel Extraction ($250)",
                "Full surgical intervention to remove battlefield shrapnel, suture deep lacerations, and sterilize wounds (+30% Health).",
                new Color(0.65f, 0.25f, 0.25f), () =>
                {
                    if (s.cashBalance < 250)
                    {
                        ShowFundsNotice(250);
                        return;
                    }
                    s.cashBalance -= 250;
                    s.health = Mathf.Min(100f, s.health + 30f);
                    s.inMedicalConvalescence = false;
                    s.convalescenceSemestersRemaining = 0;
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "[SURGERY COMPLETED] Military surgeon extracted shrapnel and treated wounds (+30% Health, wounds cleaned).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Physical Therapy & Convalescence Rehab (1 AP)",
                "Engage in medically supervised physical therapy and limb rehabilitation to restore full combat mobility (+10% Health, +10% Fitness).",
                new Color(0.25f, 0.55f, 0.40f), () =>
                {
                    if (!s.TryConsumeActionPoint("Physical Therapy"))
                    {
                        ShowExhaustedNotice();
                        return;
                    }
                    s.health = Mathf.Min(100f, s.health + 10f);
                    s.fitness = Mathf.Min(100f, s.fitness + 10f);
                    if (s.convalescenceSemestersRemaining > 0) s.convalescenceSemestersRemaining--;
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "[REHABILITATION] Completed intensive therapy drills (+10% Health, +10% Fitness).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Combat Stress Decompression & Chaplain Counseling",
                "Meet with the regimental chaplain or mental health counselor for psychological decompression and confidential debrief (-15 Combat Stress, +10 Morale).",
                new Color(0.40f, 0.45f, 0.55f), () =>
                {
                    s.combatStress = Mathf.Max(0f, s.combatStress - 15f);
                    s.happiness = Mathf.Min(100f, s.happiness + 10f);
                    GameEvents.OnFeedEventLogged?.Invoke("MEDICAL", "[DECOMPRESSION] Attended confidential stress counseling. Psychological resilience restored (-15 Combat Stress).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });
        }
        #endregion

        #region Category 5: Career, Promotions & Special Forces
        private void BuildCareerAdvancement(SoldierProfile s)
        {
            CreateActivityCard("Promotion Board Eligibility Review",
                $"Review qualifications for next rank. Current Merit: <b>{s.combatMeritPoints} CMP</b> | Fitness: <b>{s.fitness:F0}%</b> | Discipline: <b>{s.discipline:F0}%</b>.",
                new Color(0.55f, 0.45f, 0.15f), () =>
                {
                    if (GameManager.Instance != null && GameManager.Instance.militaryRankDatabase != null)
                    {
                        PromotionBoardEngine.EvaluatePromotionBoard(s, GameManager.Instance.militaryRankDatabase);
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER", $"[BOARD REVIEW] Service record logged. Maintain high fitness and merit to qualify for the next promotion board.");
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Apply for Special Forces Assessment & Selection (SFAS)",
                "Volunteer for 1st Special Operations Group. Requires exceptional physical fitness (75%+) and proven combat skill (55%+).",
                new Color(0.18f, 0.55f, 0.35f), () =>
                {
                    if (s.isSpecialForces)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER", "You are already a fully qualified Special Forces Operator wearing the elite Green Beret!");
                        return;
                    }
                    if (s.isSpecialForcesCandidate)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER", $"You are currently enrolled in the Special Forces Qualification Course (Training Semester {s.specialForcesTrainingSemesters + 1}/2).");
                        return;
                    }
                    if (s.fitness < 75f || s.combatSkill < 55f)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER", $"[SFAS DENIED] Application rejected by SOCOM. Requires minimum 75% Fitness (Current: {s.fitness:F0}%) and 55% Combat Skill (Current: {s.combatSkill:F0}%).");
                        return;
                    }
                    s.isSpecialForcesCandidate = true;
                    s.specialForcesTrainingSemesters = 0;
                    GameEvents.OnFeedEventLogged?.Invoke("CAREER", "<color=#D4AF37>[SFAS CANDIDATE ACCEPTED]</color> Your selection packet was approved by Special Operations Command! You will report to the Q-Course on the next rotation.");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Volunteer for Frontline Deployment / Tour Extension",
                "Request immediate assignment to an active combat theater (+Combat Pay, hazardous duty stipend, and valor medal eligibility).",
                new Color(0.65f, 0.22f, 0.22f), () =>
                {
                    if (s.isInCombatDeployment)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER", "You are already stationed on the active frontlines in a hazardous combat zone.");
                        return;
                    }
                    s.isInCombatDeployment = true;
                    s.isVoluntaryExtendedTour = true;
                    GameEvents.OnFeedEventLogged?.Invoke("CAREER", "<color=#C7392E>[FRONTLINE VOLUNTEER]</color> Orders cut: Transferred to active combat theater with +10% Combat Hazardous Pay!");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    RefreshUI();
                });

            CreateActivityCard("Retire Honorably from Military Service",
                "Submit official retirement papers after 20+ years of honorable service to receive your full military pension and service dossier.",
                new Color(0.40f, 0.35f, 0.30f), () =>
                {
                    if (s.age < 38.0f && s.totalSemestersLived < 40)
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("CAREER", "[RETIREMENT INELIGIBLE] Military retirement requires 20+ years of active duty service (40 semesters). You may request an early discharge if medical disability permits.");
                        return;
                    }
                    s.isRetired = true;
                    long pension = SalaryCalculator.CalculateNetMonthlyPay(s);
                    GameEvents.OnFeedEventLogged?.Invoke("CAREER", $"<color=#D4AF37>[HONORABLY RETIRED]</color> Retired after a distinguished career! Awarded full monthly pension of ${pension:N0}/mo.");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                    CloseModal();
                });
        }
        #endregion

        #region Helper UI Builders
        private void CreateCategoryTile(string title, string description, int actionCount, UnityEngine.Events.UnityAction onClick)
        {
            var tileObj = new GameObject($"Category_{title}");
            tileObj.transform.SetParent(activitiesContainer, false);

            ThemePalette p = ThemeManager.Instance != null ? ThemeManager.Instance.GetCurrentPalette() : ThemeManager.GetPalette(ThemeMode.Dark);

            var img = tileObj.AddComponent<Image>();
            img.color = p.subCardBackground;

            var btn = tileObj.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.highlightedColor = Color.Lerp(p.subCardBackground, Color.white, 0.12f);
            cb.pressedColor = Color.Lerp(p.subCardBackground, Color.black, 0.18f);
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            var vLayout = tileObj.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(20, 20, 16, 16);
            vLayout.spacing = 6;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = tileObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var le = tileObj.AddComponent<LayoutElement>();
            le.minHeight = 84;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(tileObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.fontSize = 22;
            tmp.color = p.textPrimary;
            string descHex = ColorUtility.ToHtmlStringRGB(p.textSecondary);
            string goldHex = ColorUtility.ToHtmlStringRGB(p.accentGold);
            string actionBadge = $"<color=#{goldHex}><b>[ {actionCount} OPTIONS ]</b></color>";

            tmp.text = $"<b>{title}</b>   {actionBadge}\n<size=17><color=#{descHex}>{description}</color></size>";
        }

        private void CreateBackToDirectoryButton()
        {
            var backBtnObj = new GameObject("Btn_BackToDirectory");
            backBtnObj.transform.SetParent(activitiesContainer, false);

            ThemePalette p = ThemeManager.Instance != null ? ThemeManager.Instance.GetCurrentPalette() : ThemeManager.GetPalette(ThemeMode.Dark);

            var img = backBtnObj.AddComponent<Image>();
            img.color = p.btnNormalBg;

            var btn = backBtnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.highlightedColor = Color.Lerp(p.btnNormalBg, Color.white, 0.15f);
            cb.pressedColor = Color.Lerp(p.btnNormalBg, Color.black, 0.20f);
            btn.colors = cb;
            btn.onClick.AddListener(() => SetCategory(ActivityCategory.Directory));

            var le = backBtnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 48;
            le.minHeight = 44;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(backBtnObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = "<b>< ALL BASE DEPARTMENTS</b>";
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = p.accentGold;

            var rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void CreateActivityCard(string name, string desc, Color btnColor, UnityEngine.Events.UnityAction onClick)
        {
            var cardObj = new GameObject($"Activity_{name}");
            cardObj.transform.SetParent(activitiesContainer, false);

            var img = cardObj.AddComponent<Image>();
            ThemePalette p = ThemeManager.Instance != null ? ThemeManager.Instance.GetCurrentPalette() : ThemeManager.GetPalette(ThemeMode.Dark);
            img.color = p.subCardBackground;

            var vLayout = cardObj.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(18, 18, 14, 14);
            vLayout.spacing = 8;
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;

            var fitter = cardObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(cardObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 22;
            tmp.color = p.textPrimary;
            string descHex = ColorUtility.ToHtmlStringRGB(p.textSecondary);
            tmp.text = $"<b>{name}</b>\n<size=18><color=#{descHex}>{desc}</color></size>";

            var btnObj = new GameObject("Button");
            btnObj.transform.SetParent(cardObj.transform, false);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = btnColor;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 48;
            le.minHeight = 44;

            var btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTmp.raycastTarget = false;
            btnTmp.text = "EXECUTE ORDER";
            btnTmp.fontSize = 20;
            btnTmp.fontStyle = FontStyles.Bold;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;

            var btnRt = btnTextObj.GetComponent<RectTransform>();
            btnRt.anchorMin = Vector2.zero;
            btnRt.anchorMax = Vector2.one;
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;
        }

        private void ShowExhaustedNotice()
        {
            GameEvents.OnFeedEventLogged?.Invoke("FATIGUE",
                "<color=#D4AF37>[PHYSICAL FATIGUE]</color> You have expended all Action Points for this 6-month tour. Advance the tour to rest and restore stamina!");
        }

        private void ShowFundsNotice(long needed)
        {
            GameEvents.OnFeedEventLogged?.Invoke("FINANCES",
                $"<color=#C7392E>[INSUFFICIENT FUNDS]</color> Requires ${needed:N0} military cash. Check your savings or wait for payday.");
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (modalBackdropImage != null)
                modalBackdropImage.color = new Color(0f, 0f, 0f, 0.88f);
            if (dialogPanelImage != null)
                dialogPanelImage.color = p.cardBackground;
            if (titleText != null)
                titleText.color = p.accentGold;
            if (subtitleText != null)
                subtitleText.color = p.textSecondary;
        }
        #endregion
    }
}
