using System.Text;
using ArmyLife.Core;
using ArmyLife.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class CareerDossierModal : MonoBehaviour
    {
        public static CareerDossierModal Instance { get; private set; }

        [Header("Modal Root & Visuals")]
        public GameObject modalRoot;
        public Image modalBackdrop;

        [Header("Header Labels")]
        public TextMeshProUGUI headerTitleText;
        public TextMeshProUGUI soldierNameRankText;
        public TextMeshProUGUI unitAndOriginText;

        [Header("Service Record Summary")]
        public TextMeshProUGUI serviceStatusText;
        public TextMeshProUGUI serviceYearsText;
        public TextMeshProUGUI combatToursText;
        public TextMeshProUGUI finalScoreText;

        [Header("Medals & Honors Rack")]
        public TextMeshProUGUI medalsRackText;

        [Header("Action Buttons")]
        public Button enlistNewRecruitButton;
        public Button closeDossierButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (enlistNewRecruitButton != null)
                enlistNewRecruitButton.onClick.AddListener(OnEnlistNewRecruitClicked);

            if (closeDossierButton != null)
                closeDossierButton.onClick.AddListener(CloseModal);
        }

        private void OnDisable()
        {
            if (enlistNewRecruitButton != null)
                enlistNewRecruitButton.onClick.RemoveListener(OnEnlistNewRecruitClicked);

            if (closeDossierButton != null)
                closeDossierButton.onClick.RemoveListener(CloseModal);
        }

        public void OpenDossier(SoldierProfile soldier)
        {
            if (soldier == null) return;

            gameObject.SetActive(true);
            if (modalRoot != null) modalRoot.SetActive(true);
            transform.SetAsLastSibling();

            PopulateDossier(soldier);
        }

        public void CloseModal()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        private void PopulateDossier(SoldierProfile soldier)
        {
            string rankName = soldier.currentRank != null ? soldier.currentRank.rankName : "Private Recruit";
            string paygrade = soldier.currentRank != null ? soldier.currentRank.paygrade : "E-1";

            if (soldierNameRankText != null)
            {
                soldierNameRankText.text = $"<color=#00FFAA><b>{rankName.ToUpper()} {soldier.characterName.ToUpper()}</b></color> ({paygrade})";
            }

            if (unitAndOriginText != null)
            {
                var theater = WarTheaterInfo.GetInfo(soldier.currentTheater);
                unitAndOriginText.text = $"<b>Generation:</b> #{soldier.generationNumber} | <b>Origin:</b> {soldier.origin} (<i>{soldier.originTrait}</i>) | <b>Frontline Theater:</b> {theater.theaterName}";
            }

            // Status Evaluation
            if (serviceStatusText != null)
            {
                if (soldier.isDead)
                {
                    serviceStatusText.text = $"<color=#FF4444><b>[STATUS: KILLED IN ACTION]</b></color>\n<i>{soldier.deathReason}</i>";
                }
                else if (soldier.isRetired)
                {
                    string careerStr = soldier.veteranCareer != VeteranCareer.None ? $" • {soldier.veteranCareer}" : "";
                    serviceStatusText.text = $"<color=#00FFAA><b>[STATUS: HONORABLY RETIRED{careerStr.ToUpper()}]</b></color>\n<i>Awarded Full Lifetime Military Pension & Veteran Honors</i>";
                }
                else if (soldier.currentRank != null && (soldier.currentRank.paygrade == "O-10" || soldier.currentRank.rankName.Contains("General")))
                {
                    serviceStatusText.text = "<color=#FFD700><b>[STATUS: SUPREME VICTORY • GENERAL OF THE ARMY]</b></color>\n<i>Supreme Allied Commander of National Armed Forces</i>";
                }
                else
                {
                    serviceStatusText.text = "<color=#44D4FF><b>[STATUS: ACTIVE SERVICE RECORD]</b></color>\n<i>Serving with Honor in the Armed Forces</i>";
                }
            }

            // Service Statistics
            float years = soldier.totalSemestersLived * 0.5f;
            int tours = soldier.totalSemestersLived;
            string wealthTitle = "Modest Commoner";
            if (soldier.cashBalance >= 10000000L || soldier.familyHouseTier == FamilyHouseTier.ImperialDynasticSeat)
                wealthTitle = "<color=#FFD700><b>The Gilded Sovereign Myth (Apex Dynasty)</b></color>";
            else if (soldier.cashBalance >= 2500000L || soldier.familyHouseTier == FamilyHouseTier.SovereignImperialChateau)
                wealthTitle = "<color=#FFAA00><b>Sovereign Royal Powerbroker</b></color>";
            else if (soldier.cashBalance >= 500000L || soldier.familyHouseTier == FamilyHouseTier.DucalPalaceCitadel)
                wealthTitle = "<color=#00FFAA><b>Grand Provincial Oligarch</b></color>";
            else if (soldier.cashBalance >= 100000L || soldier.familyHouseTier == FamilyHouseTier.AncestralEstate)
                wealthTitle = "<color=#00D4FF><b>Prominent Patrician Nobility</b></color>";
            else if (soldier.cashBalance >= 25000L)
                wealthTitle = "Comfortable Veteran";

            string estateName = soldier.familyHouseTier != FamilyHouseTier.None
                ? $"\n<b>Dynastic Estate:</b> {EconomyData.GetFamilyHouseDisplayName(soldier.familyHouseTier)}"
                : "";
            string bizName = soldier.ownedBusiness != CommercialBusiness.None
                ? $"\n<b>Commercial Empire:</b> {EconomyData.GetBusinessDisplayName(soldier.ownedBusiness)}"
                : "";

            if (serviceYearsText != null)
            {
                serviceYearsText.text = $"<b>Years Served:</b> {years:F1} Years ({tours} Tours of Duty)\n<b>Final Bank Balance:</b> ${soldier.cashBalance:N0} ({wealthTitle}){estateName}{bizName}";
            }

            if (combatToursText != null)
            {
                string extLabel = soldier.consecutiveCombatSemesters >= 2 
                    ? $"\n<color=#FFD700><b>Extended Combat Veteran:</b> {soldier.consecutiveCombatSemesters * 0.5f:F1} yrs continuous frontline service (+10% Bonus Qualified)</color>" 
                    : "";
                string sfLabel = soldier.isSpecialForces
                    ? $"\n<color=#FF4444><b>Special Forces Operator:</b> {soldier.specialForcesMissionsSurvived} Black Ops Missions Survived</color>"
                    : "";
                string academyLabel = soldier.graduatedWarAcademy
                    ? "\n<color=#00FFAA><b>War Academy Graduate:</b> Commissioned Officer (O-1)</color>"
                    : "";

                int aliveSquad = 0;
                if (soldier.squadMates != null)
                {
                    foreach (var m in soldier.squadMates) if (!m.isDead) aliveSquad++;
                }
                string squadLabel = $"\n<color=#00D4FF><b>Brothers in Arms:</b> {aliveSquad}/4 Squadmates Survived War</color>";
                string heirLabel = soldier.childHeir != null 
                    ? $"\n<color=#FFD700><b>Next Generation Heir:</b> {soldier.childHeir.childName} {soldier.lastName} (Age {soldier.childHeir.age:F1})</color>"
                    : "";

                combatToursText.text = $"<b>Promotions Earned:</b> {soldier.totalPromotionsEarned} | <b>Combat:</b> {soldier.combatSkill:F0}% | <b>Discipline:</b> {soldier.intellect:F0}%{extLabel}{sfLabel}{academyLabel}{squadLabel}{heirLabel}";
            }

            // Medals & Honors
            if (medalsRackText != null)
            {
                if (soldier.earnedMedals != null && soldier.earnedMedals.Count > 0)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < soldier.earnedMedals.Count; i++)
                    {
                        sb.AppendLine($"• <color=#FFD700><b>{soldier.earnedMedals[i]}</b></color>");
                    }
                    medalsRackText.text = sb.ToString();
                }
                else
                {
                    medalsRackText.text = "<color=#888888>No combat decorations recorded in service file.</color>";
                }
            }

            // Final Career Legacy Score
            int rankOrder = soldier.currentRank != null ? soldier.currentRank.rankOrder : 1;
            int medalScore = (soldier.earnedMedals != null ? soldier.earnedMedals.Count : 0) * 350;
            int tourScore = soldier.totalSemestersLived * 100;
            int bankScore = Mathf.Clamp((int)(soldier.cashBalance / 25), 0, 5000);
            int victoryBonus = (soldier.currentRank != null && soldier.currentRank.paygrade == "O-10") ? 10000 : (soldier.isRetired ? 3000 : 0);
            int sfBonus = soldier.isSpecialForces ? (2000 + (soldier.specialForcesMissionsSurvived * 500)) : 0;
            int academyBonus = soldier.graduatedWarAcademy ? 1500 : 0;

            int estateBonus = 0;
            switch (soldier.familyHouseTier)
            {
                case FamilyHouseTier.AncestralEstate: estateBonus = 1500; break;
                case FamilyHouseTier.DucalPalaceCitadel: estateBonus = 5000; break;
                case FamilyHouseTier.SovereignImperialChateau: estateBonus = 12000; break;
                case FamilyHouseTier.ImperialDynasticSeat: estateBonus = 30000; break;
            }

            int empireBonus = 0;
            switch (soldier.ownedBusiness)
            {
                case CommercialBusiness.DefenseMunitionsConglomerate: empireBonus = 4000; break;
                case CommercialBusiness.ImperialShippingCartel: empireBonus = 10000; break;
                case CommercialBusiness.CentralBankingHouseAndMint: empireBonus = 25000; break;
            }

            int totalScore = (rankOrder * 750) + medalScore + tourScore + bankScore + victoryBonus + sfBonus + academyBonus + estateBonus + empireBonus;

            if (finalScoreText != null)
            {
                finalScoreText.text = $"<color=#FFD700><b>CAREER LEGACY SCORE: {totalScore:N0} PTS</b></color>";
            }

            // Update button label for Generational Continuation
            if (enlistNewRecruitButton != null)
            {
                var btnTxt = enlistNewRecruitButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null)
                {
                    if (soldier.childHeir != null && soldier.childHeir.isEligibleForEnlistment)
                    {
                        btnTxt.text = $"PASS TORCH: ENLIST AS {soldier.childHeir.childName.ToUpper()} (GEN {soldier.generationNumber + 1})";
                    }
                    else
                    {
                        btnTxt.text = "ENLIST NEW RECRUIT";
                    }
                }
            }
        }

        private void OnEnlistNewRecruitClicked()
        {
            var s = GameManager.Instance != null ? GameManager.Instance.currentSoldier : null;
            if (s != null && s.childHeir != null && s.childHeir.isEligibleForEnlistment)
            {
                CloseModal();
                GameManager.Instance.ContinueAsChildHeir();
                return;
            }

            CloseModal();

            if (OpeningSceneController.Instance != null)
            {
                OpeningSceneController.Instance.OpenCharacterCreationModal();
            }
        }
    }
}
