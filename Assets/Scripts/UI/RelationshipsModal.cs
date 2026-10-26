using System.Collections.Generic;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class RelationshipsModal : MonoBehaviour
    {
        public static RelationshipsModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header")]
        public TextMeshProUGUI titleText;
        public Button closeButton;

        [Header("Dual Mode Switcher Buttons (Friend & Clan)")]
        public Button clanTabButton;
        public Button friendsTabButton;
        public TextMeshProUGUI clanTabButtonText;
        public TextMeshProUGUI friendsTabButtonText;

        [Header("Panels Container")]
        public GameObject clanPanel;
        public GameObject friendsPanel;

        [Header("Clan View Elements")]
        public TextMeshProUGUI clanOverviewText;
        public Transform clanMembersContainer;
        public Button openFullClanHallButton;

        [Header("Friends View Elements")]
        public TextMeshProUGUI friendsOverviewText;
        public Transform friendsContainer;

        public enum ActiveTab { Clan, Friends }
        private ActiveTab currentTab = ActiveTab.Clan;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            if (clanTabButton != null) clanTabButton.onClick.AddListener(() => SwitchTab(ActiveTab.Clan));
            if (friendsTabButton != null) friendsTabButton.onClick.AddListener(() => SwitchTab(ActiveTab.Friends));
            if (openFullClanHallButton != null) openFullClanHallButton.onClick.AddListener(OnOpenFullClanHallClicked);

            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            if (clanTabButton != null) clanTabButton.onClick.RemoveAllListeners();
            if (friendsTabButton != null) friendsTabButton.onClick.RemoveAllListeners();
            if (openFullClanHallButton != null) openFullClanHallButton.onClick.RemoveListener(OnOpenFullClanHallClicked);

            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void OpenModal(bool showFriendsFirst = false)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            currentTab = showFriendsFirst ? ActiveTab.Friends : ActiveTab.Clan;
            SwitchTab(currentTab);
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void SwitchTab(ActiveTab tab)
        {
            currentTab = tab;

            if (clanPanel != null) clanPanel.SetActive(currentTab == ActiveTab.Clan);
            if (friendsPanel != null) friendsPanel.SetActive(currentTab == ActiveTab.Friends);

            UpdateTabButtonVisuals();

            if (currentTab == ActiveTab.Clan)
            {
                RefreshClanView();
            }
            else
            {
                RefreshFriendsView();
            }

            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private void UpdateTabButtonVisuals()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            bool isMilitary = s != null && !s.isCivilian;

            if (clanTabButtonText != null)
            {
                string tab1Title = isMilitary ? "FIRETEAM SQUAD" : "CLAN & FAMILY";
                clanTabButtonText.text = (currentTab == ActiveTab.Clan) 
                    ? $"<b>{tab1Title} [ACTIVE]</b>" 
                    : tab1Title;
            }

            if (friendsTabButtonText != null)
            {
                string tab2Title = isMilitary ? "HOMEFRONT & PEERS" : "FRIENDS & COMRADES";
                friendsTabButtonText.text = (currentTab == ActiveTab.Friends) 
                    ? $"<b>{tab2Title} [ACTIVE]</b>" 
                    : tab2Title;
            }
        }

        private void RefreshClanView()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            bool isMilitary = !s.isCivilian;

            if (isMilitary)
            {
                s.InitializeSquadMatesIfEmpty();

                if (titleText != null)
                {
                    titleText.text = "FIRETEAM SQUAD • 1ST PLATOON";
                }

                if (clanOverviewText != null)
                {
                    clanOverviewText.text = 
                        $"<b>Band of Brothers:</b> Your sworn fireteam comrades. Maintain high trust and readiness; their combat specialties directly protect your life under enemy fire!\n" +
                        $"<i>Cigarettes: <b>{s.cigarettesCount} packs</b> | Field Medkits: <b>{s.fieldMedkitsCount} kits</b> | Squad Morale: <b>{s.happiness:F0}%</b></i>";
                }

                if (clanMembersContainer == null) return;

                for (int i = clanMembersContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(clanMembersContainer.GetChild(i).gameObject);
                }

                foreach (var mate in s.squadMates)
                {
                    CreateSquadmateRow(mate, s);
                }
            }
            else
            {
                s.InitializeDefaultClanMembers();

                if (titleText != null)
                {
                    titleText.text = $"HOUSE {s.lastName.ToUpper()} - CLAN DECISIONS";
                }

                if (clanOverviewText != null)
                {
                    string rankName = s.nobleTitle > NobleRank.Commoner ? $"{s.nobleTitle} House" : "Commoner Family";
                    clanOverviewText.text = 
                        $"<b>Noble Lineage:</b> {rankName}   |   <b>Clan Succession Favor:</b> +{s.clanSuccessionScore} Favor   |   <b>Clan Standing:</b> {s.clanFavor:F0}%\n" +
                        $"<i>Take crucial decisions regarding family succession, filial duty, and clan alliances.</i>";
                }

                if (clanMembersContainer == null) return;

                for (int i = clanMembersContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(clanMembersContainer.GetChild(i).gameObject);
                }

                foreach (var m in s.clanMembers)
                {
                    CreateClanMemberRow(m, s);
                }
            }
        }

        private void CreateSquadmateRow(SquadMate mate, SoldierProfile soldier)
        {
            var rowObj = new GameObject($"Squadmate_{mate.nickname}");
            rowObj.transform.SetParent(clanMembersContainer, false);

            var rowImg = rowObj.AddComponent<Image>();
            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            rowImg.color = p.subCardBackground;

            var rowLayout = rowObj.AddComponent<VerticalLayoutGroup>();
            rowLayout.padding = new RectOffset(16, 16, 12, 12);
            rowLayout.spacing = 6;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = true;

            var fitter = rowObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Info text
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(rowObj.transform, false);
            var infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 24;
            infoText.color = p.textPrimary;

            if (mate.isDead)
            {
                infoText.text = $"<color=#FF4444><b>[KIA • FALLEN HERO]</b></color> <b>{mate.GetDisplayName()}</b> ({mate.roleTitle})\n" +
                                $"<size=20>Died: <i>{mate.deathReason}</i> | Battles Survived: <b>{mate.battlesSurvived}</b></size>";
            }
            else
            {
                string statusColor = mate.isInjured ? "#FF8800" : (mate.fatigue > 70f ? "#FFCC00" : "#00FFAA");
                string statusText = mate.isInjured ? $"Injured ({mate.injuryStatus})" : (mate.fatigue > 70f ? "Fatigued" : "Combat Ready");

                infoText.text = $"<b>{mate.GetDisplayName()}</b> ({mate.roleTitle}) - <i>{mate.personalityTrait}</i>\n" +
                                $"<size=20>Comradeship: <b>{mate.relationship:F0}%</b> | Morale: <b>{mate.morale:F0}%</b> | Fatigue: <b>{mate.fatigue:F0}%</b> | Status: <color={statusColor}><b>{statusText}</b></color> | Battles: <b>{mate.battlesSurvived}</b></size>";
            }

            // Action Buttons Row
            var btnRow = new GameObject("ButtonsRow");
            btnRow.transform.SetParent(rowObj.transform, false);
            var hLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 10;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;
            hLayout.childForceExpandHeight = false;
            hLayout.childForceExpandWidth = true;
            var hFitter = btnRow.AddComponent<ContentSizeFitter>();
            hFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (mate.isDead)
            {
                CreateActionButton(btnRow.transform, "VISIT MEMORIAL", new Color(0.40f, 0.20f, 0.20f), () =>
                {
                    GameEvents.OnFeedEventLogged?.Invoke("MEMORIAL",
                        $"<color=#FF8866>[BATTLEFIELD MEMORIAL]</color> You visited the grave marker of your fallen brother, <b>{mate.GetDisplayName()}</b> ({mate.roleTitle}). You left a spent shell casing in salute.");
                    RefreshClanView();
                });

                CreateActionButton(btnRow.transform, "REQUISITION RECRUIT", new Color(0.20f, 0.45f, 0.65f), () =>
                {
                    SquadmateCommandEngine.RecruitReplacement(soldier, mate.role);
                    RefreshClanView();
                });
            }
            else
            {
                // 1. Share Smoke
                CreateActionButton(btnRow.transform, "SHARE SMOKE (1 CIG)", new Color(0.20f, 0.50f, 0.40f), () =>
                {
                    SquadmateCommandEngine.ShareCigarettesWithSquad(soldier, mate);
                    RefreshClanView();
                });

                // 2. Bandage / First Aid
                CreateActionButton(btnRow.transform, "TREAT TRAUMA (1 MEDKIT)", new Color(0.70f, 0.25f, 0.25f), () =>
                {
                    SquadmateCommandEngine.BandageSquadmate(soldier, mate);
                    RefreshClanView();
                });

                // 3. Tactical Drill & Bond
                CreateActionButton(btnRow.transform, "TACTICAL DRILL", new Color(0.25f, 0.45f, 0.70f), () =>
                {
                    mate.relationship = Mathf.Min(100f, mate.relationship + 6f);
                    mate.fatigue = Mathf.Max(0f, mate.fatigue - 8f);
                    mate.morale = Mathf.Min(100f, mate.morale + 5f);
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 1.5f);
                    GameEvents.OnFeedEventLogged?.Invoke("DRILLS",
                        $"<color=#00D4FF>[FIRETEAM DRILL]</color> You ran live-fire breaching drills with <b>{mate.GetDisplayName()}</b>. Cohesion and morale improved (+6 Comradeship, -8 Fatigue, +1.5 Combat Skill)!");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });
            }
        }

        private void CreateClanMemberRow(ClanMember member, SoldierProfile soldier)
        {
            var rowObj = new GameObject($"ClanMember_{member.name}");
            rowObj.transform.SetParent(clanMembersContainer, false);

            var rowImg = rowObj.AddComponent<Image>();
            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            rowImg.color = p.subCardBackground;

            var rowLayout = rowObj.AddComponent<VerticalLayoutGroup>();
            rowLayout.padding = new RectOffset(16, 16, 12, 12);
            rowLayout.spacing = 6;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = true;

            var fitter = rowObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Info text
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(rowObj.transform, false);
            var infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 24;
            infoText.color = p.textPrimary;
            infoText.text = $"<b>{member.name}</b> ({member.roleTitle})\n" +
                            $"<size=20>Relationship: <b>{member.relationship:F0}%</b> | Trait: <i>{member.personalityTrait}</i></size>";

            // Action Buttons Row
            var btnRow = new GameObject("ButtonsRow");
            btnRow.transform.SetParent(rowObj.transform, false);
            var hLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 10;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;
            hLayout.childForceExpandHeight = false;
            hLayout.childForceExpandWidth = true;
            var hFitter = btnRow.AddComponent<ContentSizeFitter>();
            hFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 1. Spend Time button
            CreateActionButton(btnRow.transform, "SPEND TIME", new Color(0.20f, 0.55f, 0.35f), () =>
            {
                member.relationship = Mathf.Min(100f, member.relationship + 12f);
                soldier.happiness = Mathf.Min(100f, soldier.happiness + 8f);
                GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                    $"<color=#00FFAA>[CLAN BONDING]</color> You spent intimate quality time with <b>{member.name}</b>. " +
                    $"Your bond grew stronger (+12% Relationship, +8% Happiness).");
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                RefreshClanView();
            });

            // 2. Specific relation action
            if (member.relationType == ClanRelationType.OlderBrother || member.relationType == ClanRelationType.BranchCousin)
            {
                CreateActionButton(btnRow.transform, "SPAR DUEL", new Color(0.70f, 0.35f, 0.15f), () =>
                {
                    bool won = (soldier.combatSkill + soldier.fitness * 0.4f) >= (45f + member.age * 0.4f);
                    if (won)
                    {
                        soldier.clanSuccessionScore += 4;
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 3f);
                        member.relationship = Mathf.Max(0f, member.relationship - 5f);
                        GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                            $"<color=#FFD700>[SIBLING DUEL WON]</color> You bested <b>{member.name}</b> in a close-combat sparring bout! " +
                            $"The clan elders noted your superiority (+4 Succession Favor, +3 Combat Skill).");
                    }
                    else
                    {
                        soldier.health = Mathf.Max(20f, soldier.health - 8f);
                        GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                            $"<color=#FF6666>[SIBLING DUEL LOST]</color> <b>{member.name}</b> struck you down during sparring. " +
                            $"They mocked your technique (-8 Health).");
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });
            }
            else if (member.relationType == ClanRelationType.Father || member.relationType == ClanRelationType.Mother)
            {
                CreateActionButton(btnRow.transform, "ASK ALLOWANCE", new Color(0.25f, 0.45f, 0.75f), () =>
                {
                    if (member.relationship >= 45f)
                    {
                        int gift = (soldier.nobleTitle > NobleRank.Commoner) ? 150 : 40;
                        soldier.cashBalance += gift;
                        member.relationship = Mathf.Max(0f, member.relationship - 4f);
                        GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                            $"<color=#00D4FF>[PARENTAL SUPPORT]</color> <b>{member.name}</b> gave you a generous pocket allowance of <b>${gift}</b>!");
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                            $"<color=#888888>[ALLOWANCE REFUSED]</color> <b>{member.name}</b> scolded you: 'Work hard for your own coin!'");
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });
            }
            else if (member.relationType == ClanRelationType.ClanElder)
            {
                CreateActionButton(btnRow.transform, "PLEDGE LOYALTY", new Color(0.55f, 0.25f, 0.70f), () =>
                {
                    soldier.clanSuccessionScore += 3;
                    soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 5f);
                    member.relationship = Mathf.Min(100f, member.relationship + 8f);
                    GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                        $"<color=#FFD700>[PATRIARCH ENDORSEMENT]</color> You bowed before <b>{member.name}</b> and pledged to elevate the clan's martial banner. " +
                        $"The Elder approved (+3 Succession Favor, +5% Clan Favor).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });
            }
        }

        private void RefreshFriendsView()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            if (titleText != null)
            {
                titleText.text = s.isYouthPhase ? "ACADEMY COMRADES & FRIENDS" : "BARRACKS COMRADES & PLATOON";
            }

            if (friendsOverviewText != null)
            {
                friendsOverviewText.text = s.isYouthPhase
                    ? "<b>Academy Classmates:</b> Classmates and training partners who will become your future officers and battlefield peers."
                    : "<b>Barracks Squad:</b> Your sworn squadmates in the Imperial Vanguard. High comradeship boosts survival during combat deployments!";
            }

            if (friendsContainer == null) return;

            for (int i = friendsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(friendsContainer.GetChild(i).gameObject);
            }

            s.InitializePeers();

            // Populate friends
            if (s.schoolPeers != null)
            {
                foreach (var peer in s.schoolPeers)
                {
                    CreateFriendRow(peer, s);
                }
            }
        }

        private void CreateFriendRow(SchoolPeer peer, SoldierProfile soldier)
        {
            var rowObj = new GameObject($"Friend_{peer.name}");
            rowObj.transform.SetParent(friendsContainer, false);

            var rowImg = rowObj.AddComponent<Image>();
            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);
            rowImg.color = p.subCardBackground;

            var rowLayout = rowObj.AddComponent<VerticalLayoutGroup>();
            rowLayout.padding = new RectOffset(16, 16, 12, 12);
            rowLayout.spacing = 6;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = true;

            var fitter = rowObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Info text
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(rowObj.transform, false);
            var infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 24;
            infoText.color = p.textPrimary;
            
            string friendColorHex = ColorUtility.ToHtmlStringRGB(p.accentEmerald);
            string crushColorHex = ColorUtility.ToHtmlStringRGB(p.accentGold);
            string friendTag = peer.isCrush ? $" <color=#{crushColorHex}>[CRUSH]</color>" : (peer.isFriend ? $" <color=#{friendColorHex}>[FRIEND]</color>" : "");
            infoText.text = $"<b>{peer.name}</b> ({peer.role}){friendTag}\n" +
                            $"<size=20>Relationship: <b>{peer.relationship:F0}%</b> | Personality: <i>{peer.personalityTrait}</i></size>";

            // Action buttons row
            var btnRow = new GameObject("ButtonsRow");
            btnRow.transform.SetParent(rowObj.transform, false);
            var hLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 10;
            hLayout.childControlHeight = true;
            hLayout.childControlWidth = true;
            hLayout.childForceExpandHeight = false;
            hLayout.childForceExpandWidth = true;
            var hFitter = btnRow.AddComponent<ContentSizeFitter>();
            hFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 1. Hang Out
            CreateActionButton(btnRow.transform, "HANG OUT", new Color(0.20f, 0.55f, 0.35f), () =>
            {
                peer.relationship = Mathf.Min(100f, peer.relationship + 10f);
                if (peer.relationship >= 60f) peer.isFriend = true;
                soldier.happiness = Mathf.Min(100f, soldier.happiness + 6f);
                GameEvents.OnFeedEventLogged?.Invoke("SOCIAL",
                    $"<color=#00FFAA>[COMRADE BONDING]</color> You grabbed lunch and swapped combat stories with <b>{peer.name}</b>. " +
                    $"Relationship improved to {peer.relationship:F0}%!");
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                RefreshFriendsView();
            });

            // 2. Spar / Train Together
            CreateActionButton(btnRow.transform, "TRAIN TOGETHER", new Color(0.70f, 0.40f, 0.15f), () =>
            {
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2.5f);
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 2f);
                peer.relationship = Mathf.Min(100f, peer.relationship + 5f);
                GameEvents.OnFeedEventLogged?.Invoke("SOCIAL",
                    $"<color=#FFD700>[JOINT DRILLS]</color> You ran firing and CQB drills with <b>{peer.name}</b>. (+2.5 Combat, +2 Fitness).");
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                RefreshFriendsView();
            });

            // 3. Share Rations
            CreateActionButton(btnRow.transform, "SHARE RATIONS", new Color(0.25f, 0.45f, 0.75f), () =>
            {
                peer.relationship = Mathf.Min(100f, peer.relationship + 15f);
                peer.isFriend = true;
                soldier.happiness = Mathf.Min(100f, soldier.happiness + 4f);
                GameEvents.OnFeedEventLogged?.Invoke("SOCIAL",
                    $"<color=#00D4FF>[GENEROSITY]</color> You gifted an imperial combat ration pack to <b>{peer.name}</b>. (+15% Relationship!).");
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                RefreshFriendsView();
            });
        }

        private void CreateActionButton(Transform parent, string label, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);

            var img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 48;
            le.minHeight = 44;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;

            var rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void OnOpenFullClanHallClicked()
        {
            CloseModal();
            if (ClanModal.Instance != null)
            {
                ClanModal.Instance.OpenModal();
            }
            else
            {
                var found = FindAnyObjectByType<ClanModal>(FindObjectsInactive.Include);
                if (found != null) found.OpenModal();
            }
        }

        private void OnThemeChanged(ThemeMode mode, ThemePalette palette)
        {
            ApplyThemePalette(palette);
        }

        private void ApplyThemePalette(ThemePalette p)
        {
            if (modalBackdropImage != null)
            {
                modalBackdropImage.color = p.mode == ThemeMode.Light 
                    ? new Color(0.12f, 0.12f, 0.10f, 0.75f) 
                    : new Color(0f, 0f, 0f, 0.90f);
            }
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (titleText != null) titleText.color = p.accentGold;
            if (clanOverviewText != null) clanOverviewText.color = p.textSecondary;
            if (friendsOverviewText != null) friendsOverviewText.color = p.textSecondary;

            if (clanTabButton != null)
            {
                var img = clanTabButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                if (clanTabButtonText != null) clanTabButtonText.color = p.btnNormalText;
            }

            if (friendsTabButton != null)
            {
                var img = friendsTabButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                if (friendsTabButtonText != null) friendsTabButtonText.color = p.btnNormalText;
            }

            if (closeButton != null)
            {
                var img = closeButton.GetComponent<Image>();
                if (img != null) img.color = p.btnNormalBg;
                var txt = closeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = p.btnNormalText;
            }
        }
    }
}
