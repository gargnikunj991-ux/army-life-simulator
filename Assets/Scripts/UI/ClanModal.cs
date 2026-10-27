using System;
using ArmyLife.Core;
using ArmyLife.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class ClanModal : MonoBehaviour
    {
        public static ClanModal Instance { get; private set; }

        [Header("UI Root")]
        public GameObject modalRoot;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI clanSummaryText;
        public Transform membersContainer;
        public Button closeButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            RefreshClanDisplay();
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void RefreshClanDisplay()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            s.InitializeDefaultClanMembers();

            if (titleText != null)
            {
                string famName = !string.IsNullOrEmpty(s.lastName) ? s.lastName.ToUpper() : "VALERIAN";
                titleText.text = $"<color=#FFD700>HOUSE {famName} • ANCESTRAL CLAN</color>";
            }

            if (clanSummaryText != null)
            {
                string rankName = s.nobleTitle > NobleRank.Commoner ? $"Hereditary {s.nobleTitle} House" : "Commoner Lineage";
                clanSummaryText.text = 
                    $"<b>Lineage:</b> {rankName}   |   <b>Clan Standing:</b> +{s.clanSuccessionScore} Favor   |   <b>Clan Favor:</b> {s.clanFavor:F0}%\n" +
                    $"<i>Feudal loyalty is cutthroat. Not all clan members share good will towards your succession rights.</i>";
            }

            if (membersContainer == null) return;

            // Clear old member rows
            for (int i = membersContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(membersContainer.GetChild(i).gameObject);
            }

            // Build card for each clan member
            foreach (var member in s.clanMembers)
            {
                CreateClanMemberCard(member, s);
            }
        }

        private void CreateClanMemberCard(ClanMember member, SoldierProfile soldier)
        {
            var cardObj = new GameObject($"Member_{member.name}");
            cardObj.transform.SetParent(membersContainer, false);

            var rect = cardObj.AddComponent<RectTransform>();
            var layout = cardObj.AddComponent<LayoutElement>();
            layout.minHeight = 84;
            layout.preferredHeight = 90;
            layout.flexibleWidth = 1f;

            var bg = cardObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.16f, 0.22f);

            var hLayout = cardObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.padding = new RectOffset(16, 16, 10, 10);
            hLayout.spacing = 14;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;

            // Info column
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(cardObj.transform, false);
            var infoLayout = infoObj.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1f;

            var infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 20;

            string relColor = (member.relationship >= 70f) ? "#00FFAA" : (member.relationship >= 40f ? "#FFD700" : "#FF4444");
            string traitColor = (member.relationship < 40f) ? "#FF6666" : "#00D4FF";

            infoText.text = 
                $"<b>{member.name}</b> <size=16>({member.roleTitle})</size>\n" +
                $"<size=17><color={traitColor}>[{member.personalityTrait}]</color>  •  Relationship: <color={relColor}><b>{member.relationship:F0}%</b></color> (Age {member.age})</size>";

            // Action Button
            var actionBtnObj = new GameObject("InteractBtn");
            actionBtnObj.transform.SetParent(cardObj.transform, false);
            var btnLayout = actionBtnObj.AddComponent<LayoutElement>();
            btnLayout.minWidth = 120;
            btnLayout.preferredWidth = 130;

            var btnImg = actionBtnObj.AddComponent<Image>();
            btnImg.color = (member.relationship < 40f) ? new Color(0.65f, 0.25f, 0.25f) : new Color(0.20f, 0.45f, 0.70f);

            var btn = actionBtnObj.AddComponent<Button>();
            var btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(actionBtnObj.transform, false);
            var bText = btnTextObj.AddComponent<TextMeshProUGUI>();
            bText.text = "INTERACT";
            bText.fontSize = 18;
            bText.fontStyle = FontStyles.Bold;
            bText.alignment = TextAlignmentOptions.Center;
            bText.color = Color.white;
            var bRect = btnTextObj.GetComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;

            btn.onClick.AddListener(() => OpenMemberInteractions(member, soldier));
        }

        private void OpenMemberInteractions(ClanMember member, SoldierProfile soldier)
        {
            if (member == null || soldier == null) return;

            string title = $"Clan Encounter: {member.name}";
            string relDesc = (member.relationship >= 70f) 
                ? $"{member.name} looks at you with fond family pride."
                : (member.relationship >= 40f 
                    ? $"{member.name} nods politely, observing your conduct with measured feudal courtesy."
                    : $"{member.name} regards you with cold, competitive detachment, harboring subtle jealousy.");

            string desc = $"<b>{member.roleTitle}</b> (Age {member.age})\n" +
                          $"Relationship: <b>{member.relationship:F0}%</b> | Trait: <i>{member.personalityTrait}</i>\n\n" +
                          $"{relDesc}\nHow will you interact with them?";

            string choiceA = "Spend Time & Converse (+Relationship)";
            string choiceB = "Ask for Allowance / Clan Funds";
            string choiceC = (soldier.age >= 4.0f) ? "Courtyard Sparring Match" : "Compliment & Honor";

            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "CLAN INTERACTION",
                title,
                desc,
                choiceA,
                choiceB,
                choiceC,
                // Action A: Converse
                () =>
                {
                    member.relationship = Mathf.Min(100f, member.relationship + UnityEngine.Random.Range(5f, 9f));
                    soldier.intellect = Mathf.Min(100f, soldier.intellect + 2f);
                    GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                        $"<b>[CONVERSED WITH {member.name.ToUpper()}]</b> You sat together discussing clan heritage and military drills. " +
                        $"Relationship with {member.name}: <b>{member.relationship:F0}%</b> (+Intellect).");
                    RefreshClanDisplay();
                },
                // Action B: Ask Money
                () =>
                {
                    if (member.relationship >= 50f)
                    {
                        int gift = Mathf.RoundToInt((member.generosity * 1.2f) + UnityEngine.Random.Range(20, 50));
                        soldier.cashBalance += gift;
                        member.relationship = Mathf.Min(100f, member.relationship + 2f);
                        GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                            $"<color=#FFD700><b>[CLAN GIFT]</b></color> {member.name} smiles warmly and slips you <b>+${gift}</b> pocket money! " +
                            $"<i>'Invest this in your martial preparation, child.'</i> (Bank: ${soldier.cashBalance:N0}).");
                    }
                    else
                    {
                        member.relationship = Mathf.Max(0f, member.relationship - 5f);
                        GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                            $"<color=#FF6600><b>[REQUEST REBUFFED]</b></color> {member.name} sneers coldly: " +
                            $"<i>'Why should clan coin be wasted on you? Earn your own glory.'</i> Relationship dropped to {member.relationship:F0}%.");
                    }
                    RefreshClanDisplay();
                },
                // Action C: Spar or Compliment
                () =>
                {
                    if (soldier.age >= 4.0f)
                    {
                        // Spar
                        soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 4f);
                        if (member.relationType == ClanRelationType.OlderBrother || member.relationType == ClanRelationType.Brother)
                        {
                            soldier.clanSuccessionScore += 8;
                            member.relationship = Mathf.Max(0f, member.relationship - 4f);
                            GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                                $"<b>[SPARRED WITH {member.name.ToUpper()}]</b> You crossed practice wooden blades in the courtyard! Your decisive thrust pushed them back! " +
                                $"Family Standing: <b>+{soldier.clanSuccessionScore}</b> (+4 Combat Skill).");
                        }
                        else
                        {
                            member.relationship = Mathf.Min(100f, member.relationship + 4f);
                            GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                                $"<b>[MARTIAL SPARRING]</b> You and {member.name} drilled basic combat stances in the courtyard (+4 Combat Skill, +4 Relationship).");
                        }
                    }
                    else
                    {
                        // Compliment
                        member.relationship = Mathf.Min(100f, member.relationship + 8f);
                        soldier.clanFavor = Mathf.Min(100f, soldier.clanFavor + 3f);
                        GameEvents.OnFeedEventLogged?.Invoke("CLAN",
                            $"<b>[COMPLIMENT]</b> You bowed respectfully and praised {member.name}'s martial dignity (+8 Relationship, +3 Clan Favor).");
                    }
                    RefreshClanDisplay();
                }
            );
        }
    }
}
