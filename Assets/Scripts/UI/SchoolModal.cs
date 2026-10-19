using System;
using ArmyLife.Core;
using ArmyLife.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class SchoolModal : MonoBehaviour
    {
        public static SchoolModal Instance { get; private set; }

        [Header("UI Root")]
        public GameObject modalRoot;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI schoolSummaryText;
        public Button studyHarderButton;
        public Button joinClubButton;
        public Transform peersContainer;
        public Button closeButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null) modalRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            if (studyHarderButton != null) studyHarderButton.onClick.AddListener(OnStudyHarderClicked);
            if (joinClubButton != null) joinClubButton.onClick.AddListener(OnJoinClubClicked);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            if (studyHarderButton != null) studyHarderButton.onClick.RemoveListener(OnStudyHarderClicked);
            if (joinClubButton != null) joinClubButton.onClick.RemoveListener(OnJoinClubClicked);
        }

        public void OpenModal()
        {
            gameObject.SetActive(true);
            if (modalRoot != null) modalRoot.SetActive(true);

            RefreshSchoolDisplay();
        }

        public void CloseModal()
        {
            if (modalRoot != null && modalRoot != gameObject)
            {
                modalRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void RefreshSchoolDisplay()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            s.InitializeSchoolPeersIfEligible();

            // BitLife style under-age check: modal opens, but shows locked pre-school state with no interactions
            if (s.age < 6.0f)
            {
                if (titleText != null)
                {
                    titleText.text = "<color=#AAAAAA>NOT ENROLLED (PRE-SCHOOL / HOME CARE)</color>";
                }

                if (schoolSummaryText != null)
                {
                    schoolSummaryText.text = 
                        $"<b>Current Age:</b> {s.age:F1}   |   <b>Status:</b> Too young for primary school (Enrollment at Age 6.0)\n" +
                        $"<i>Like in BitLife, options are visible but locked until Age 6.0. You remain home under family care. Formal schooling and peer interactions unlock at Age 6.0!</i>";
                }

                if (studyHarderButton != null)
                {
                    studyHarderButton.interactable = false;
                    SetButtonLabel(studyHarderButton, "📚 Study Harder [Age 6.0]");
                }

                if (joinClubButton != null)
                {
                    joinClubButton.interactable = false;
                    SetButtonLabel(joinClubButton, "🛡️ Cadet Clubs [Age 6.0]");
                }

                if (peersContainer != null)
                {
                    for (int i = peersContainer.childCount - 1; i >= 0; i--)
                    {
                        Destroy(peersContainer.GetChild(i).gameObject);
                    }
                    CreateLockedPeersNotice();
                }
                return;
            }

            // Age 6.0+ Active Schooling
            if (studyHarderButton != null)
            {
                studyHarderButton.interactable = true;
                SetButtonLabel(studyHarderButton, "📚 Study Harder (+Score, +Intellect)");
            }

            if (joinClubButton != null)
            {
                joinClubButton.interactable = true;
                SetButtonLabel(joinClubButton, "🛡️ Join / Change Cadet Club");
            }

            if (titleText != null)
            {
                titleText.text = $"<color=#00D4FF>{s.schoolName.ToUpper()}</color>";
            }

            if (schoolSummaryText != null)
            {
                string rankTier = (s.schoolPerformance >= 85f) 
                    ? "<color=#00FFAA>Top 5% Honor Roll</color>" 
                    : (s.schoolPerformance >= 65f ? "Passing Standings" : "<color=#FF4444>Academic Probation</color>");

                float promoThreshold = SchoolHelper.GetPromotionThreshold(s.schoolTier);
                string promoStatus = (s.schoolPerformance >= promoThreshold) 
                    ? $"<color=#00FFAA>Eligible for Promotion (>={promoThreshold:F0}%)</color>"
                    : $"Needs >={promoThreshold:F0}% to Promote (Currently: {s.schoolPerformance:F0}%)";

                string tierBadge = $"<color=#FFD700>[{SchoolHelper.GetTierTitle(s.schoolTier)}]</color>";

                if (s.schoolLevel == SchoolLevel.PrimarySchool)
                {
                    string grade = (s.primarySchoolScore >= 90f) ? "A+" : (s.primarySchoolScore >= 80f ? "A" : (s.primarySchoolScore >= 70f ? "B" : (s.primarySchoolScore >= 60f ? "C" : "D")));
                    schoolSummaryText.text = 
                        $"<b>Institution:</b> {tierBadge}   |   <b>Cadet Club:</b> {s.schoolClub}   |   <b>Promotions:</b> +{s.timesPromotedSchool}\n" +
                        $"<b>Academic Performance:</b> {s.schoolPerformance:F0}% ({rankTier})   |   <b>Primary Cumulative Score:</b> <color=#00FFAA><b>{s.primarySchoolScore:F1}/100 ({grade})</b></color>\n" +
                        $"<b>Annual Status:</b> {promoStatus}\n" +
                        $"<i>{SchoolHelper.GetTierDescription(s.schoolTier)}</i>";
                }
                else if (s.schoolLevel == SchoolLevel.MiddleSchool)
                {
                    schoolSummaryText.text = 
                        $"<b>Institution:</b> {tierBadge}   |   <b>Cadet Club:</b> {s.schoolClub}   |   <b>Promotions:</b> +{s.timesPromotedSchool}\n" +
                        $"<b>Middle School Performance:</b> {s.schoolPerformance:F0}% ({rankTier})   |   <b>Primary Honors:</b> {s.primarySchoolGraduationHonors}\n" +
                        $"<b>Annual Status:</b> {promoStatus}\n" +
                        $"<i>{SchoolHelper.GetTierDescription(s.schoolTier)}</i>";
                }
                else
                {
                    string imperialBadge = s.isImperialScholarshipStudent ? " <color=#FFD700>★ IMPERIAL SCHOLAR ★</color>" : "";
                    schoolSummaryText.text = 
                        $"<b>Institution:</b> {tierBadge}{imperialBadge}   |   <b>Cadet Club:</b> {s.schoolClub}   |   <b>Promotions:</b> +{s.timesPromotedSchool}\n" +
                        $"<b>Youth Academy Drill:</b> {s.schoolPerformance:F0}% ({rankTier})   |   <b>Annual Status:</b> {promoStatus}\n" +
                        $"<i>Final 3-year preparation before the Age 18.0 National Military Ranking Exam! {SchoolHelper.GetTierDescription(s.schoolTier)}</i>";
                }
            }

            if (peersContainer == null) return;

            // Clear old peer rows
            for (int i = peersContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(peersContainer.GetChild(i).gameObject);
            }

            // Build card for each classmate
            foreach (var peer in s.schoolPeers)
            {
                CreatePeerCard(peer, s);
            }
        }

        private void CreateLockedPeersNotice()
        {
            var noticeObj = new GameObject("LockedNotice");
            noticeObj.transform.SetParent(peersContainer, false);

            var layout = noticeObj.AddComponent<LayoutElement>();
            layout.minHeight = 80;
            layout.preferredHeight = 85;
            layout.flexibleWidth = 1f;

            var bg = noticeObj.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.12f, 0.18f, 0.85f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(noticeObj.transform, false);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 17;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.75f, 0.80f, 0.90f);
            text.text = "🏫 <i>No school classmates yet. Formal district primary school enrollment and peer interactions begin at <b>Age 6.0</b>!</i>";

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20, 10);
            rect.offsetMax = new Vector2(-20, -10);
        }

        private void SetButtonLabel(Button btn, string label)
        {
            if (btn == null) return;
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = label;
        }

        private void CreatePeerCard(SchoolPeer peer, SoldierProfile soldier)
        {
            var cardObj = new GameObject($"Peer_{peer.name}");
            cardObj.transform.SetParent(peersContainer, false);

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

            string relColor = (peer.relationship >= 70f) ? "#00FFAA" : (peer.relationship >= 40f ? "#FFD700" : "#FF4444");
            string statusTag = peer.isCrush ? " <color=#FF66AA>[Cadet Crush ♥]</color>" : (peer.isFriend ? " <color=#00FFAA>[Close Friend]</color>" : "");

            infoText.text = 
                $"<b>{peer.name}</b> <size=16>({peer.role}){statusTag}</size>\n" +
                $"<size=17><color=#00D4FF>[{peer.personalityTrait}]</color>  •  Relationship: <color={relColor}><b>{peer.relationship:F0}%</b></color></size>";

            // Action Button
            var actionBtnObj = new GameObject("InteractBtn");
            actionBtnObj.transform.SetParent(cardObj.transform, false);
            var btnLayout = actionBtnObj.AddComponent<LayoutElement>();
            btnLayout.minWidth = 120;
            btnLayout.preferredWidth = 130;

            var btnImg = actionBtnObj.AddComponent<Image>();
            btnImg.color = (peer.relationship < 40f) ? new Color(0.65f, 0.25f, 0.25f) : new Color(0.20f, 0.45f, 0.70f);

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

            btn.onClick.AddListener(() => OpenPeerInteractions(peer, soldier));
        }

        private void OpenPeerInteractions(SchoolPeer peer, SoldierProfile soldier)
        {
            if (peer == null || soldier == null) return;

            string title = $"Academy Interaction: {peer.name}";
            string desc = $"<b>{peer.role}</b> ({peer.gender})\n" +
                          $"Relationship: <b>{peer.relationship:F0}%</b> | Trait: <i>{peer.personalityTrait}</i>\n\n" +
                          $"How would you like to interact with {peer.name} during recess / free drill?";

            string choiceA = "💬 Chat & Hang Out (+Relationship)";
            string choiceB = "⚔️ Field Sparring Challenge (+Combat, +Respect)";
            string choiceC = (soldier.age >= 13.0f) ? "💌 Ask Out / Flirt (Romantic Interest)" : "🎭 Pull a Cadet Prank";

            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "SCHOOL ENCOUNTER",
                title,
                desc,
                choiceA,
                choiceB,
                choiceC,
                // Action A: Chat
                () =>
                {
                    peer.relationship = Mathf.Min(100f, peer.relationship + UnityEngine.Random.Range(6f, 12f));
                    if (peer.relationship >= 75f && !peer.isFriend)
                    {
                        peer.isFriend = true;
                        GameEvents.OnFeedEventLogged?.Invoke("SCHOOL",
                            $"<color=#00FFAA><b>[NEW CLOSE FRIEND]</b></color> You and {peer.name} hit it off! You are now close academy friends (Relationship: {peer.relationship:F0}%).");
                    }
                    else
                    {
                        GameEvents.OnFeedEventLogged?.Invoke("SCHOOL",
                            $"<b>[HUNG OUT]</b> You and {peer.name} chatted about military tactics and instructors (+Relationship: {peer.relationship:F0}%).");
                    }
                    RefreshSchoolDisplay();
                },
                // Action B: Sparring
                () =>
                {
                    soldier.combatSkill = Mathf.Min(100f, soldier.combatSkill + 4f);
                    soldier.fitness = Mathf.Min(100f, soldier.fitness + 2f);
                    peer.relationship = Mathf.Min(100f, peer.relationship + 4f);
                    soldier.squadRespect = Mathf.Min(100f, soldier.squadRespect + 3f);

                    GameEvents.OnFeedEventLogged?.Invoke("SCHOOL",
                        $"<b>[FIELD SPARRING]</b> You and {peer.name} squared off on the training mats in friendly hand-to-hand sparring! " +
                        $"Combat Skill: <b>{soldier.combatSkill:F0}/100</b> (+4 Combat, +4 Relationship).");
                    RefreshSchoolDisplay();
                },
                // Action C: Flirt or Prank
                () =>
                {
                    if (soldier.age >= 13.0f)
                    {
                        // Flirt
                        if (peer.relationship >= 50f)
                        {
                            peer.isCrush = true;
                            peer.relationship = Mathf.Min(100f, peer.relationship + 15f);
                            soldier.childhoodRomanceInterest = peer.name;

                            GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                                $"<color=#FF66AA><b>[CADET SWEETHEART ♥]</b></color> You shyly confessed your feelings to {peer.name} behind the rifle armory. " +
                                $"{peer.name} smiled warmly and accepted! You are now dating! (+15 Relationship).");
                        }
                        else
                        {
                            peer.relationship = Mathf.Max(0f, peer.relationship - 8f);
                            GameEvents.OnFeedEventLogged?.Invoke("ROMANCE",
                                $"<color=#FF8800><b>[AWKWARD REJECTION]</b></color> {peer.name} stepped back awkwardly: " +
                                $"<i>'We should just focus on military drills right now...'</i> (-8 Relationship).");
                        }
                    }
                    else
                    {
                        // Prank
                        if (UnityEngine.Random.value < 0.65f)
                        {
                            soldier.intellect += 3f;
                            GameEvents.OnFeedEventLogged?.Invoke("SCHOOL",
                                $"<b>[CLASSROOM PRANK]</b> You hid a simulated smoke capsule in {peer.name}'s locker! The whole class erupted in laughter (+3 Intellect)!");
                        }
                        else
                        {
                            soldier.meritRating = Mathf.Max(0f, soldier.meritRating - 5f);
                            GameEvents.OnFeedEventLogged?.Invoke("SCHOOL",
                                $"<color=#FF6600><b>[DETENTION]</b></color> Instructor Brant caught you pulling a prank! You were assigned weekend latrine duty (-5 Merit).");
                        }
                    }
                    RefreshSchoolDisplay();
                }
            );
        }

        private void OnStudyHarderClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null)
            {
                s.schoolPerformance = Mathf.Min(100f, s.schoolPerformance + 6f);
                s.intellect = Mathf.Min(100f, s.intellect + 4f);
                s.meritRating = Mathf.Min(100f, s.meritRating + 3f);

                if (s.age < 12.0f)
                {
                    s.primarySchoolScore = Mathf.Min(100f, s.primarySchoolScore + 3.5f);
                    GameEvents.OnFeedEventLogged?.Invoke("SCHOOL",
                        $"<color=#00FFAA>[PRIMARY STUDY HARDER]</color> You spent recess studying tactics scrolls and imperial geography! " +
                        $"Term Performance: <b>{s.schoolPerformance:F0}%</b> | Primary Cumulative Score: <b>{s.primarySchoolScore:F1}/100</b> (+3.5 Primary Score, +4 Intellect).");
                }
                else
                {
                    GameEvents.OnFeedEventLogged?.Invoke("SCHOOL",
                        $"<color=#00FFAA>[CADET TACTICAL STUDY]</color> You studied war logs and weapon manuals in the academy archive! " +
                        $"Cadet Performance: <b>{s.schoolPerformance:F0}%</b> | Intellect: <b>{s.intellect:F0}/100</b> (+4 Intellect, +3 Merit).");
                }

                GameEvents.OnSoldierStatsUpdated?.Invoke(s);
                RefreshSchoolDisplay();
            }
        }

        private void OnJoinClubClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s == null) return;

            GameEvents.OnChildhoodDilemmaTriggered?.Invoke(
                "EXTRACURRICULAR CLUB",
                "Join an Academy Club",
                "Extracurricular cadet clubs hone your abilities and bolster your reputation before the National Military Exam.",
                "🥋 Martial Sparring & CQB Club (+Combat)",
                "♟️ Tactical War Chess & Drone Club (+Intellect)",
                "🎯 Imperial Marksmanship Society (+Combat & Range)",
                () => { s.schoolClub = "Martial Sparring Club"; s.combatSkill += 5f; GameEvents.OnFeedEventLogged?.Invoke("SCHOOL", $"You joined the <b>Martial Sparring Club</b> (+5 Combat Skill)!"); RefreshSchoolDisplay(); },
                () => { s.schoolClub = "War Chess & Tactics Club"; s.intellect += 6f; GameEvents.OnFeedEventLogged?.Invoke("SCHOOL", $"You joined the <b>War Chess & Tactics Club</b> (+6 Intellect)!"); RefreshSchoolDisplay(); },
                () => { s.schoolClub = "Marksmanship Society"; s.combatSkill += 4f; s.fitness += 2f; GameEvents.OnFeedEventLogged?.Invoke("SCHOOL", $"You joined the <b>Marksmanship Society</b> (+4 Combat, +2 Fitness)!"); RefreshSchoolDisplay(); }
            );
        }
    }
}
