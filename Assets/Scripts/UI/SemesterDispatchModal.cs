using System;
using ArmyLife.Core;
using ArmyLife.Data;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    /// <summary>
    /// Dramatic Semester Dispatch & Milestone Modal:
    /// Delivers an authentic, BitLife-style narrative turn experience upon advancing each semester:
    /// Homefront letters from family, strategic warfront telegrams, fireteam status updates,
    /// and clean financial compensation accounting.
    /// </summary>
    public class SemesterDispatchModal : MonoBehaviour
    {
        public static SemesterDispatchModal Instance { get; private set; }

        [Header("Modal Root")]
        public GameObject modalRoot;
        public Image modalBackdropImage;
        public Image dialogPanelImage;

        [Header("Header Elements")]
        public TextMeshProUGUI dispatchHeaderTitleText;
        public TextMeshProUGUI soldierRankBannerText;
        public Button confirmProceedButton;
        public TextMeshProUGUI confirmButtonText;

        [Header("Dispatch Sections")]
        public TextMeshProUGUI homefrontNewsText;
        public TextMeshProUGUI warfrontSitrepText;
        public TextMeshProUGUI squadmateStatusText;
        public TextMeshProUGUI financialSummaryText;

        private Action onConfirmedCallback;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (confirmProceedButton != null) confirmProceedButton.onClick.AddListener(OnConfirmClicked);
            ThemeManager.OnThemeChanged += OnThemeChanged;
        }

        private void OnDisable()
        {
            if (confirmProceedButton != null) confirmProceedButton.onClick.RemoveListener(OnConfirmClicked);
            ThemeManager.OnThemeChanged -= OnThemeChanged;
        }

        public void DisplayDispatch(SoldierProfile soldier, long basePay, long hazardBonus, long expenses, Action onConfirmed = null)
        {
            if (soldier == null) return;
            this.onConfirmedCallback = onConfirmed;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (modalRoot != null) modalRoot.SetActive(true);

            // Compute authentic military calendar
            int year = 940 + (soldier.totalSemestersLived / 2);
            string season = (soldier.totalSemestersLived % 2 == 0) ? "SPRING" : "AUTUMN";
            int semester = (soldier.totalSemestersLived % 2) + 1;

            if (dispatchHeaderTitleText != null)
            {
                dispatchHeaderTitleText.text = $"★ OFFICIAL MILITARY DISPATCH • {season} {year} ★";
            }

            if (soldierRankBannerText != null)
            {
                string rank = soldier.currentRank != null ? soldier.currentRank.rankName : "Private";
                string theater = WarTheaterInfo.GetInfo(soldier.currentTheater).theaterName;
                string dutyStatus = soldier.isInCombatDeployment 
                    ? $"Active Frontline Deployment ({theater})" 
                    : (soldier.inMedicalConvalescence ? "Field Hospital Convalescence" : "Garrison Refit & Training");
                soldierRankBannerText.text = $"<b>{rank.ToUpper()} {soldier.characterName.ToUpper()}</b> • {dutyStatus}";
            }

            // 1. Homefront News
            if (homefrontNewsText != null)
            {
                homefrontNewsText.text = GenerateHomefrontStory(soldier);
            }

            // 2. Warfront Sitrep
            if (warfrontSitrepText != null)
            {
                var curTheater = WarTheaterInfo.GetInfo(soldier.currentTheater);
                int progress = DynamicWarFrontEngine.GetFrontlineProgress(soldier.currentTheater);
                string sitrep = DynamicWarFrontEngine.GetTheaterStatusText(soldier.currentTheater);
                warfrontSitrepText.text = $"<b>Sector Sitrep:</b> {curTheater.theaterName}\n" +
                    $"{sitrep}\n" +
                    $"<i>Allied Forces hold strategic perimeter. Threat Level: {curTheater.dangerLevel}/5.</i>";
            }

            // 3. Fireteam Squad Cohesion
            if (squadmateStatusText != null)
            {
                squadmateStatusText.text = GenerateSquadmateStatusStory(soldier);
            }

            // 4. Financial Summary
            if (financialSummaryText != null)
            {
                long net = (basePay + hazardBonus) - expenses;
                string netSign = net >= 0 ? "+" : "";
                string netColor = net >= 0 ? "#00FFAA" : "#FF5555";

                financialSummaryText.text = 
                    $"Active Base Pay: <color=#FFD700>+${basePay:N0}</color>   |   Hazard/CMP Bounty: <color=#00D4FF>+${hazardBonus:N0}</color>   |   Expenses & Upkeep: <color=#FF8866>-${expenses:N0}</color>\n" +
                    $"<b>Net Semester Cashflow:</b> <color={netColor}><b>{netSign}${net:N0}</b></color>   |   Current Treasury Balance: <b>${soldier.cashBalance:N0}</b>";
            }

            AudioManager.PlayRadioSquelch();

            ThemeMode mode = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentMode : ThemeMode.Dark;
            ApplyThemePalette(ThemeManager.GetPalette(mode));
        }

        private string GenerateHomefrontStory(SoldierProfile soldier)
        {
            if (soldier.isMarried && !string.IsNullOrEmpty(soldier.spouseName))
            {
                if (soldier.childHeir != null)
                {
                    return $"<color=#FF88FF><b>[HOMEFRONT LETTER • SPOUSE & CHILD]</b></color>\n" +
                           $"\"Dearest {soldier.characterName}, little {soldier.childHeir.childName} looked up at your framed military portrait today and proudly saluted! " +
                           $"Your remittance arrived just in time for winter provisions. Stay safe, my love—we count the days until your rotation home.\"";
                }
                return $"<color=#FF88FF><b>[HOMEFRONT LETTER • SPOUSE]</b></color>\n" +
                       $"\"My brave {soldier.characterName}, the snow has begun falling over the family manor. I enclosed a pressed wildflower from our garden in this letter. " +
                       $"The neighbors speak of your valor on the front lines with awe. Return safely to my arms.\"";
            }

            if (soldier.clanMembers != null && soldier.clanMembers.Count > 0)
            {
                var livingSibling = soldier.clanMembers.Find(m => m.IsSibling && m.isAlive);
                if (livingSibling != null && UnityEngine.Random.value < 0.5f)
                {
                    string sibRole = livingSibling.relationType == ClanRelationType.Sister || livingSibling.relationType == ClanRelationType.YoungerSister ? "SISTER" : "BROTHER";
                    if (livingSibling.age < 10)
                    {
                        return $"<color=#00D4FF><b>[LETTER FROM {sibRole} • {livingSibling.name.ToUpper()}]</b></color>\n" +
                               $"\"Dear Big {(soldier.gender == "Female" ? "Sister" : "Brother")}, I drew a picture of your regiment's flag for school today! " +
                               $"Mom helped me fold this letter. Please come home safe!\"";
                    }
                    return $"<color=#00D4FF><b>[LETTER FROM {sibRole} • {livingSibling.name.ToUpper()}]</b></color>\n" +
                           $"\"Dear {(soldier.gender == "Female" ? "Sister" : "Brother")}, our hometown held a rally for the expeditionary troops this week. " +
                           $"Everyone asks how you are doing on the front line. Make us proud and keep your head down!\"";
                }

                var mother = soldier.clanMembers.Find(m => m.relationType == ClanRelationType.Mother && m.isAlive);
                if (mother != null)
                {
                    var father = soldier.clanMembers.Find(m => m.relationType == ClanRelationType.Father && m.isAlive);
                    string mention = father != null ? $"Your father {father.name} and I" : "The family and I";
                    return $"<color=#00FFAA><b>[LETTER FROM MOTHER • {mother.name.ToUpper()}]</b></color>\n" +
                           $"\"My dearest child, {mention} read the regimental dispatches every morning with our hearts in our throats. " +
                           $"Keep your helmet strapped and trust your squadmates. We pray for your safe return.\"";
                }

                var livingFather = soldier.clanMembers.Find(m => m.relationType == ClanRelationType.Father && m.isAlive);
                if (livingFather != null)
                {
                    return $"<color=#FFD700><b>[LETTER FROM FATHER • {livingFather.name.ToUpper()}]</b></color>\n" +
                           $"\"Son, keep your boots dry and your rifle cleaned. The homefront harvest is steady. " +
                           $"Serve with honor and bring pride to our name.\"";
                }
            }

            return $"<color=#00FFAA><b>[REGIMENTAL GARRISON DISPATCH]</b></color>\n" +
                   $"Company clerk delivered fresh mail call and clean wool blankets to your squad quarters. " +
                   $"Morale remains steadfast as the regiment prepares for the upcoming tour.";
        }

        private string GenerateSquadmateStatusStory(SoldierProfile soldier)
        {
            if (soldier.squadMates == null || soldier.squadMates.Count == 0)
            {
                return "Fireteam assigned to standard perimeter watch.";
            }

            var rookie = soldier.squadMates.Find(m => m.role == SquadRole.RiflemanRookie && !m.isDead);
            var medic = soldier.squadMates.Find(m => m.role == SquadRole.CombatMedic && !m.isDead);
            var gunner = soldier.squadMates.Find(m => m.role == SquadRole.HeavyGunner && !m.isDead);

            string line1 = rookie != null 
                ? $"• <b>{rookie.nickname}</b> cleaned his service rifle and ran additional reload drills (+Comradeship)." 
                : "• Fireteam riflemen on standby.";
            string line2 = medic != null 
                ? $"• <b>{medic.nickname}</b> secured fresh field tourniquets and morphine syrettes from regimental triage." 
                : "• Medical supplies restocked.";
            string line3 = gunner != null 
                ? $"• <b>{gunner.nickname}</b> inspected the heavy machine gun barrel and linked 500 rounds of armor-piercing ammo." 
                : "• Support weapons verified.";

            return $"{line1}\n{line2}\n{line3}";
        }

        private void OnConfirmClicked()
        {
            AudioManager.PlayUiClick();
            CloseModal();
            onConfirmedCallback?.Invoke();
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
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
                    ? new Color(0.12f, 0.12f, 0.10f, 0.85f) 
                    : new Color(0f, 0f, 0f, 0.92f);
            }
            if (dialogPanelImage != null) dialogPanelImage.color = p.panelBackground;
            if (dispatchHeaderTitleText != null) dispatchHeaderTitleText.color = p.accentGold;
            if (soldierRankBannerText != null) soldierRankBannerText.color = p.textPrimary;

            if (confirmProceedButton != null)
            {
                var img = confirmProceedButton.GetComponent<Image>();
                if (img != null) img.color = p.accentEmerald;
                if (confirmButtonText != null) confirmButtonText.color = Color.white;
            }
        }
    }
}
