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
                string tab1Title = isMilitary ? "FAMILY & HOMEFRONT" : "CLAN & SUCCESSION";
                clanTabButtonText.text = (currentTab == ActiveTab.Clan) 
                    ? $"<b>{tab1Title} [ACTIVE]</b>" 
                    : tab1Title;
            }

            if (friendsTabButtonText != null)
            {
                string tab2Title = isMilitary ? "BAND OF BROTHERS" : "CLASSMATES & FRIENDS";
                friendsTabButtonText.text = (currentTab == ActiveTab.Friends) 
                    ? $"<b>{tab2Title} [ACTIVE]</b>" 
                    : tab2Title;
            }
        }

        /// <summary>
        /// Tab 1: Always dedicated to Immediate Family & Clan Members (Parents, Siblings, Spouse, Children).
        /// </summary>
        private void RefreshClanView()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            bool isMilitary = !s.isCivilian;
            s.InitializeDefaultClanMembers();
            s.SyncImmediateFamilyMembers();

            if (titleText != null)
            {
                titleText.text = isMilitary 
                    ? $"FAMILY & HOMEFRONT • HOUSE {s.lastName.ToUpper()}" 
                    : $"HOUSE {s.lastName.ToUpper()} • CLAN DECISIONS";
            }

            if (clanOverviewText != null)
            {
                if (isMilitary)
                {
                    clanOverviewText.text = 
                        $"<b>Family on the Homefront:</b> Your loved ones back home awaiting your safe return from the warfront.\n" +
                        $"<i>Military Remittances Sent: <b>${s.totalRemittancesSent:N0}</b> | Family Standing: <b>{s.clanFavor:F0}%</b> | Available Cash: <b>${s.cashBalance:N0}</b></i>";
                }
                else
                {
                    string rankName = s.nobleTitle > NobleRank.Commoner ? $"{s.nobleTitle} House" : "Commoner Family";
                    clanOverviewText.text = 
                        $"<b>Noble Lineage:</b> {rankName}   |   <b>Clan Succession Favor:</b> +{s.clanSuccessionScore} Favor   |   <b>Clan Standing:</b> {s.clanFavor:F0}%\n" +
                        $"<i>Take crucial decisions regarding family succession, filial duty, and clan alliances.</i>";
                }
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

            if (!member.isAlive)
            {
                infoText.text = $"<b>{member.name}</b> ({member.roleTitle})\n" +
                                $"<size=20><color=#888888><i>Passed away peacefully. Fondly remembered in loving memory.</i></color></size>";
                CreateActionButton(btnRow.transform, "HONOR MEMORY", new Color(0.40f, 0.40f, 0.45f), () =>
                {
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 5f);
                    soldier.combatStress = Mathf.Max(0f, soldier.combatStress - 8f);
                    GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                        $"<color=#AAAAAA>[FAMILY TRIBUTE]</color> You lit a quiet candle and honored the memory of <b>{member.name}</b> (-8 Stress).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });
                return;
            }

            infoText.text = $"<b>{member.name}</b> ({member.roleTitle}) • Age {member.age}\n" +
                            $"<size=20>Relationship: <b>{member.relationship:F0}%</b> | Disposition: <i>{member.personalityTrait}</i></size>";

            bool isMilitary = !soldier.isCivilian;

            if (isMilitary)
            {
                // =========================================================================
                // MILITARY HOMEFRONT ACTIONS
                // =========================================================================

                // 1. Write Letter / Call Home (Free)
                CreateActionButton(btnRow.transform, "WRITE LETTER", new Color(0.20f, 0.55f, 0.35f), () =>
                {
                    member.relationship = Mathf.Min(100f, member.relationship + 10f);
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 8f);
                    soldier.combatStress = Mathf.Max(0f, soldier.combatStress - 6f);
                    GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                        $"<color=#00FFAA>[LETTER HOME]</color> You penned a heartfelt letter to <b>{member.name}</b>. " +
                        $"A warm reply arrived wishing you safety on the front lines (+10% Rel, +8% Morale, -6 Stress).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });

                // 2. Send Military Remittance ($200)
                CreateActionButton(btnRow.transform, "REMITTANCE ($200)", new Color(0.25f, 0.45f, 0.75f), () =>
                {
                    if (soldier.cashBalance >= 200)
                    {
                        soldier.cashBalance -= 200;
                        soldier.totalRemittancesSent += 200;
                        member.relationship = Mathf.Min(100f, member.relationship + 15f);
                        soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 6f);
                        soldier.clanSuccessionScore += 2;
                        GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                            $"<color=#FFD700>[MILITARY REMITTANCE]</color> You wired a <b>$200</b> remittance from your military pay home to <b>{member.name}</b>. " +
                            $"The household sends heartfelt blessings for your support (+15% Rel, +6% Clan Standing)!");
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                            $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You do not have $200 in cash to send a remittance.");
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });

                // 3. Member-specific military homefront actions
                if (member.IsParent)
                {
                    CreateActionButton(btnRow.transform, "PAY MEDICAL AID ($500)", new Color(0.65f, 0.25f, 0.25f), () =>
                    {
                        if (soldier.cashBalance >= 500)
                        {
                            soldier.cashBalance -= 500;
                            soldier.totalMedicalExpensesPaid += 500;
                            member.relationship = Mathf.Min(100f, member.relationship + 25f);
                            soldier.parentRelationship = Mathf.Min(100f, soldier.parentRelationship + 20f);
                            soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 12f);
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#00FFAA>[FAMILY HEALTH CARE]</color> You covered $500 of vital medical treatments for <b>{member.name}</b>. " +
                                $"Your parents praise your filial devotion and loyalty to the family (+25% Rel, +12% Clan Standing)!");
                        }
                        else
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You require $500 in cash to cover medical expenses.");
                        }
                        GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                        RefreshClanView();
                    });
                }
                else if (member.IsSibling)
                {
                    if (member.age < 5)
                    {
                        CreateActionButton(btnRow.transform, "PLAY / BABYSIT", new Color(0.85f, 0.45f, 0.65f), () =>
                        {
                            member.relationship = Mathf.Min(100f, member.relationship + 20f);
                            soldier.siblingRelationship = Mathf.Min(100f, soldier.siblingRelationship + 10f);
                            soldier.happiness = Mathf.Min(100f, soldier.happiness + 12f);
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#FF88FF>[SIBLING BONDING]</color> You played peek-a-boo and read picture books with baby <b>{member.name}</b>. " +
                                $"Giggles echoed throughout the home (+20% Rel, +12 Happiness)!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                            RefreshClanView();
                        });
                    }
                    else if (member.age < 18)
                    {
                        CreateActionButton(btnRow.transform, "TUITION AID ($150)", new Color(0.60f, 0.30f, 0.65f), () =>
                        {
                            if (soldier.cashBalance >= 150)
                            {
                                soldier.cashBalance -= 150;
                                member.relationship = Mathf.Min(100f, member.relationship + 25f);
                                soldier.siblingRelationship = Mathf.Min(100f, soldier.siblingRelationship + 15f);
                                soldier.happiness = Mathf.Min(100f, soldier.happiness + 10f);
                                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                    $"<color=#FF88FF>[SCHOOL AID]</color> You sponsored $150 for <b>{member.name}</b>'s school tuition and books. " +
                                    $"They sent a beaming letter thanking their big sibling (+25% Rel, +10% Happiness)!");
                            }
                            else
                            {
                                GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                    $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You lack $150 in cash for school aid.");
                            }
                            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                            RefreshClanView();
                        });
                    }
                    else
                    {
                        CreateActionButton(btnRow.transform, "TACTICAL ADVICE", new Color(0.70f, 0.40f, 0.15f), () =>
                        {
                            member.relationship = Mathf.Min(100f, member.relationship + 12f);
                            soldier.siblingRelationship = Mathf.Min(100f, soldier.siblingRelationship + 8f);
                            soldier.clanSuccessionScore += 2;
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#00D4FF>[TACTICAL BONDING]</color> You shared frontline combat doctrines with <b>{member.name}</b>. " +
                                $"Mutual martial respect deepened (+12% Rel, +2 Succession Favor)!");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                            RefreshClanView();
                        });
                    }
                }
                else if (member.IsSpouse)
                {
                    CreateActionButton(btnRow.transform, "ROMANTIC GIFT ($150)", new Color(0.75f, 0.25f, 0.50f), () =>
                    {
                        if (soldier.cashBalance >= 150)
                        {
                            soldier.cashBalance -= 150;
                            member.relationship = Mathf.Min(100f, member.relationship + 20f);
                            soldier.happiness = Mathf.Min(100f, soldier.happiness + 15f);
                            soldier.combatStress = Mathf.Max(0f, soldier.combatStress - 12f);
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#FF88FF>[ROMANCE FROM THE FRONT]</color> You sent home an engraved jewelry pendant and romantic letter to your spouse <b>{member.name}</b> (-$150). " +
                                $"Your marriage grew deeper and stronger (+20% Rel, +15 Happiness, -12 Stress)!");
                        }
                        else
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You lack $150 to send a romantic gift.");
                        }
                        GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                        RefreshClanView();
                    });
                }
                else if (member.IsChild)
                {
                    CreateActionButton(btnRow.transform, "TRUST FUND ($250)", new Color(0.20f, 0.60f, 0.55f), () =>
                    {
                        if (soldier.cashBalance >= 250)
                        {
                            soldier.cashBalance -= 250;
                            member.relationship = Mathf.Min(100f, member.relationship + 25f);
                            if (soldier.childHeir != null) soldier.childHeir.relationship = Mathf.Min(100f, soldier.childHeir.relationship + 25f);
                            soldier.happiness = Mathf.Min(100f, soldier.happiness + 12f);
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#00FFAA>[CHILD TRUST FUND]</color> You deposited $250 into <b>{member.name}</b>'s education trust fund. " +
                                $"Your child proudly shares your letters with their friends (+25% Rel, +12% Happiness)!");
                        }
                        else
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You lack $250 for the trust fund.");
                        }
                        GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                        RefreshClanView();
                    });
                }
            }
            else
            {
                // =========================================================================
                // CIVILIAN HOMEFRONT ACTIONS
                // =========================================================================

                // 1. Spend Time button
                CreateActionButton(btnRow.transform, "SPEND TIME", new Color(0.20f, 0.55f, 0.35f), () =>
                {
                    member.relationship = Mathf.Min(100f, member.relationship + 12f);
                    soldier.happiness = Mathf.Min(100f, soldier.happiness + 8f);
                    GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                        $"<color=#00FFAA>[FAMILY BONDING]</color> You spent intimate quality time with <b>{member.name}</b>. " +
                        $"Your bond grew stronger (+12% Relationship, +8% Happiness).");
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshClanView();
                });

                // 2. Specific relation action
                if (member.IsSibling)
                {
                    if (member.age < 5)
                    {
                        CreateActionButton(btnRow.transform, "PLAY TOGETHER", new Color(0.85f, 0.45f, 0.65f), () =>
                        {
                            member.relationship = Mathf.Min(100f, member.relationship + 15f);
                            soldier.happiness = Mathf.Min(100f, soldier.happiness + 10f);
                            GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                                $"<color=#FF88FF>[PLAYTIME]</color> You spent the afternoon playing toys with little <b>{member.name}</b> (+15% Rel, +10% Happiness).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                            RefreshClanView();
                        });
                    }
                    else if (member.age < 18)
                    {
                        CreateActionButton(btnRow.transform, "HELP WITH STUDIES", new Color(0.40f, 0.60f, 0.85f), () =>
                        {
                            member.relationship = Mathf.Min(100f, member.relationship + 15f);
                            soldier.intellect = Mathf.Min(100f, soldier.intellect + 1f);
                            GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                                $"<color=#00D4FF>[STUDY SESSION]</color> You tutored <b>{member.name}</b> in mathematics and tactics (+15% Rel, +1 Intellect).");
                            GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                            RefreshClanView();
                        });
                    }
                    else
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
                                    $"Family peers noted your martial prowess (+4 Succession Favor, +3 Combat Skill).");
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
                }
                else if (member.IsParent)
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
                else if (member.IsSpouse)
                {
                    CreateActionButton(btnRow.transform, "ROMANTIC DATE ($100)", new Color(0.75f, 0.25f, 0.50f), () =>
                    {
                        if (soldier.cashBalance >= 100)
                        {
                            soldier.cashBalance -= 100;
                            member.relationship = Mathf.Min(100f, member.relationship + 20f);
                            soldier.happiness = Mathf.Min(100f, soldier.happiness + 15f);
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#FF88FF>[ROMANTIC DINNER]</color> You took <b>{member.name}</b> out for a romantic evening (-$100, +20% Rel, +15 Happiness).");
                        }
                        else
                        {
                            GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                                $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You need $100 for a date.");
                        }
                        GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                        RefreshClanView();
                    });
                }
                else if (member.IsChild)
                {
                    CreateActionButton(btnRow.transform, "TEACH & PLAY", new Color(0.20f, 0.60f, 0.55f), () =>
                    {
                        member.relationship = Mathf.Min(100f, member.relationship + 20f);
                        soldier.happiness = Mathf.Min(100f, soldier.happiness + 10f);
                        GameEvents.OnFeedEventLogged?.Invoke("FAMILY",
                            $"<color=#00FFAA>[QUALITY PARENTING]</color> You taught <b>{member.name}</b> life lessons and played games (+20% Rel, +10 Happiness).");
                        GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                        RefreshClanView();
                    });
                }
            }
        }

        /// <summary>
        /// Tab 2: Dedicated to Fireteam Squadmates (Band of Brothers) in Military, or School Classmates in Civilian.
        /// </summary>
        private void RefreshFriendsView()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            bool isMilitary = !s.isCivilian;

            if (friendsContainer == null) return;

            for (int i = friendsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(friendsContainer.GetChild(i).gameObject);
            }

            if (isMilitary)
            {
                s.InitializeSquadMatesIfEmpty();

                if (titleText != null)
                {
                    titleText.text = "BAND OF BROTHERS • 1ST PLATOON FIRETEAM";
                }

                if (friendsOverviewText != null)
                {
                    friendsOverviewText.text = 
                        $"<b>Fireteam Squad:</b> Your sworn brothers-in-arms. Maintain high cohesion and readiness; their tactical specialties protect your life under enemy fire!\n" +
                        $"<i>Cigarettes: <b>{s.cigarettesCount} packs</b> | Field Medkits: <b>{s.fieldMedkitsCount} kits</b> | Squad Morale: <b>{s.happiness:F0}%</b></i>";
                }

                foreach (var mate in s.squadMates)
                {
                    CreateSquadmateRow(mate, s);
                }
            }
            else
            {
                if (titleText != null)
                {
                    titleText.text = s.isYouthPhase ? "ACADEMY CLASSMATES & PEERS" : "CIVILIAN FRIENDS & ASSOCIATES";
                }

                if (friendsOverviewText != null)
                {
                    friendsOverviewText.text = s.isYouthPhase
                        ? "<b>Academy Classmates:</b> Classmates and training partners who will become your future officers and battlefield peers."
                        : "<b>Civic Acquaintances:</b> Civilian companions and associates from your town.";
                }

                s.InitializePeers();

                if (s.schoolPeers != null)
                {
                    foreach (var peer in s.schoolPeers)
                    {
                        CreateFriendRow(peer, s);
                    }
                }
            }
        }

        private void CreateSquadmateRow(SquadMate mate, SoldierProfile soldier)
        {
            var rowObj = new GameObject($"Squadmate_{mate.nickname}");
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
                    RefreshFriendsView();
                });

                CreateActionButton(btnRow.transform, "REQUISITION RECRUIT", new Color(0.20f, 0.45f, 0.65f), () =>
                {
                    SquadmateCommandEngine.RecruitReplacement(soldier, mate.role);
                    RefreshFriendsView();
                });
            }
            else
            {
                // 1. Share Smoke
                CreateActionButton(btnRow.transform, "SHARE SMOKE (1 CIG)", new Color(0.20f, 0.50f, 0.40f), () =>
                {
                    SquadmateCommandEngine.ShareCigarettesWithSquad(soldier, mate);
                    RefreshFriendsView();
                });

                // 2. Bandage / First Aid
                CreateActionButton(btnRow.transform, "TREAT TRAUMA (1 MEDKIT)", new Color(0.70f, 0.25f, 0.25f), () =>
                {
                    SquadmateCommandEngine.BandageSquadmate(soldier, mate);
                    RefreshFriendsView();
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
                    RefreshFriendsView();
                });

                // 4. Buy Round at PX ($50)
                CreateActionButton(btnRow.transform, "BUY PX DRINK ($50)", new Color(0.75f, 0.50f, 0.15f), () =>
                {
                    if (soldier.cashBalance >= 50)
                    {
                        soldier.cashBalance -= 50;
                        mate.relationship = Mathf.Min(100f, mate.relationship + 12f);
                        mate.morale = Mathf.Min(100f, mate.morale + 15f);
                        soldier.happiness = Mathf.Min(100f, soldier.happiness + 8f);
                        GameEvents.OnFeedEventLogged?.Invoke("PX",
                            $"<color=#FFD700>[PX ROUND]</color> You bought a cold round of drinks at the PX for <b>{mate.GetDisplayName()}</b>. " +
                            $"Morale and comradeship soared (+12 Comradeship, +15 Morale)!");
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("PX",
                            $"<color=#FF6666>[INSUFFICIENT FUNDS]</color> You lack $50 in cash to buy a round at the PX.");
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshFriendsView();
                });
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
                    $"<color=#00FFAA>[COMRADE BONDING]</color> You grabbed lunch and swapped stories with <b>{peer.name}</b>. " +
                    $"Relationship improved to {peer.relationship:F0}%!");
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                RefreshFriendsView();
            });

            // 2. Spar / Train Together
            CreateActionButton(btnRow.transform, "TRAIN TOGETHER", new Color(0.70f, 0.40f, 0.15f), () =>
            {
                soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 2.5f);
                soldier.fitness = Mathf.Min(100f, soldier.fitness + 2f);
                peer.relationship = Mathf.Min(100f, peer.relationship + 6f);
                GameEvents.OnFeedEventLogged?.Invoke("TRAINING",
                    $"<color=#00D4FF>[COMRADE DRILL]</color> You ran physical obstacle courses with <b>{peer.name}</b> (+2.5 Combat, +2 Fitness)!");
                GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                RefreshFriendsView();
            });

            // 3. Romancing / Confess
            if (peer.isCrush || (!peer.isFriend && peer.relationship >= 65f))
            {
                CreateActionButton(btnRow.transform, peer.isCrush ? "DATE SWEETHEART" : "CONFESS FEELINGS", new Color(0.75f, 0.25f, 0.50f), () =>
                {
                    if (!peer.isCrush)
                    {
                        peer.isCrush = true;
                        peer.relationship = Mathf.Min(100f, peer.relationship + 15f);
                        soldier.happiness = Mathf.Min(100f, soldier.happiness + 20f);
                        GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                            $"<color=#FF88FF>[CONFESSION ACCEPTED!]</color> You confessed your heartfelt feelings to <b>{peer.name}</b>. " +
                            $"They blushed and agreed to become your sweetheart!");
                    }
                    else
                    {
                        peer.relationship = Mathf.Min(100f, peer.relationship + 10f);
                        soldier.happiness = Mathf.Min(100f, soldier.happiness + 12f);
                        GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                            $"<color=#FF88FF>[SWEETHEART DATE]</color> You shared a romantic evening stroll with <b>{peer.name}</b> (+10% Relationship, +12% Happiness)!");
                    }
                    GameEvents.OnSoldierStatsUpdated?.Invoke(soldier);
                    RefreshFriendsView();
                });
            }
        }

        private void CreateActionButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btnObj = new GameObject($"Btn_{label}");
            btnObj.transform.SetParent(parent, false);

            var img = btnObj.AddComponent<Image>();
            img.color = color;

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
