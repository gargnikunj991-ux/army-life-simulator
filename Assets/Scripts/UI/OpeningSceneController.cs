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
    public class OpeningSceneController : MonoBehaviour
    {
        public static OpeningSceneController Instance { get; private set; }

        [Header("Main Panels")]
        public GameObject openingScreenPanel;
        public GameObject mainHUDPanel;

        [Header("Opening Menu Controls")]
        public Button createNewLifeButton;
        public Button continueOrManageButton;
        public TextMeshProUGUI continueOrManageButtonText;
        public TextMeshProUGUI totalLivesCountLabel;

        [Header("Character Creation Modal")]
        public GameObject characterCreationModal;
        public TMP_InputField characterFirstNameInput;
        public TMP_InputField characterLastNameInput;
        public Button randomizeNameButton;
        
        [Header("Gender Selection")]
        public Button maleGenderButton;
        public Button femaleGenderButton;
        public TextMeshProUGUI maleGenderText;
        public TextMeshProUGUI femaleGenderText;

        [Header("Birth Country Selection")]
        public Button prevCountryButton;
        public Button nextCountryButton;
        public TextMeshProUGUI countryTitleText;
        public TextMeshProUGUI countryDescText;

        [Header("Social Lineage Selection")]
        public Button previousLineageButton;
        public Button nextLineageButton;
        public TextMeshProUGUI lineageTitleText;
        public TextMeshProUGUI lineageDetailsText;

        [Header("Appearance Customization (BitLife style)")]
        public Button prevHairStyleButton;
        public Button nextHairStyleButton;
        public TextMeshProUGUI hairStyleText;

        public Button prevHairColorButton;
        public Button nextHairColorButton;
        public TextMeshProUGUI hairColorText;

        public Button prevEyeColorButton;
        public Button nextEyeColorButton;
        public TextMeshProUGUI eyeColorText;

        public Button prevSkinToneButton;
        public Button nextSkinToneButton;
        public TextMeshProUGUI skinToneText;

        public Button randomizeAppearanceButton;
        public TextMeshProUGUI characterPreviewBadgeText;

        [Header("Modal Actions")]
        public Button confirmStartLifeButton;
        public Button cancelCreationButton;

        [Header("Manage Lives Modal")]
        public GameObject manageLivesModal;
        public Transform livesListContent;
        public Button closeManageLivesButton;

        [Header("In-Game Return Button")]
        public Button returnToMenuButton;

        // Customization State
        private string selectedGender = "Male";
        private int selectedCountryIndex = 0;
        private int selectedLineageIndex = 0;
        private int selectedHairStyleIndex = 0;
        private int selectedHairColorIndex = 0;
        private int selectedEyeColorIndex = 0;
        private int selectedSkinToneIndex = 0;

        private List<FamilyLineageSO> availableLineages = new List<FamilyLineageSO>();

        private static readonly (string title, string desc)[] Origins = 
        {
            ("Farmhand (Country Grit)", "Raised in rural agrarian provinces. Built with iron endurance and rustic toughness.\n<b>Bonus:</b> +15% Health, +15% Fitness | <b>Trait:</b> Country Grit"),
            ("Street Tough (Street Brawler)", "Hardened in city alleyways, dockyards, and underground bare-knuckle rings.\n<b>Bonus:</b> +15% Combat Skill, -10% Discipline | <b>Trait:</b> Street Brawler"),
            ("Working Mechanic (Field Craftsman)", "Apprenticed in motor pools, tank depots, and munition factories.\n<b>Bonus:</b> +15% Intellect (Tactics/Tech), +$200 Cash | <b>Trait:</b> Field Craftsman"),
            ("Military Legacy (Soldier's Son)", "Born into a lineage of seasoned infantry veterans and drill sergeants.\n<b>Bonus:</b> +15% CO Favor, +15% Squad Respect | <b>Trait:</b> Soldier's Son")
        };

        private static readonly (string name, string desc)[] Countries = 
        {
            ("The Valerian Empire", "Feudal military superpower. Masters of subterranean ancient sect ley-lines and noble martial dynasties."),
            ("The Iron Federation", "Cybernetic & industrial superpower. Powered by nanite infusions, armored mech-titans, and orbital EMPs."),
            ("The Ouroboros Dominion", "Beast-cultivating warlords. Savage warriors surgically fused with mutated atmospheric beasts.")
        };

        private static readonly string[] HairStyles = 
        {
            "Military Buzzcut", "Short Crew Cut", "Slicked Back", "Cultivator Topknot", "Undercut Fade", "Long Flowing Braids"
        };

        private static readonly string[] HairColors = 
        {
            "Obsidian Black", "Platinum Silver", "Chestnut Brown", "Golden Blonde", "Crimson Red", "Aether White"
        };

        private static readonly string[] EyeColors = 
        {
            "Steel Grey", "Aether Cyan", "Emerald Green", "Amber Gold", "Amethyst Violet", "Dark Brown"
        };

        private static readonly string[] SkinTones = 
        {
            "Fair Porcelain", "Warm Olive", "Sun Tan", "Deep Bronze", "Dark Ebony"
        };

        private static readonly string[] MaleFirstNames = 
        {
            "Ethan", "Valerius", "Silas", "Logan", "Alexander", "Darius", "Cassian", 
            "Marcus", "Cedric", "Rowan", "Gabriel", "Victor", "Lucas", "Julian", "Damian", "Arthur", "Maximilian"
        };

        private static readonly string[] FemaleFirstNames = 
        {
            "Aurelia", "Seraphina", "Elena", "Valeria", "Kaelen", "Lyra", "Isabella", 
            "Diana", "Cynthia", "Morgana", "Cassandra", "Victoria", "Helena", "Athena", "Vivian"
        };

        private static readonly string[] LastNames = 
        {
            "Vance", "Cross", "Drake", "Storm", "Vane", "Blackwood", "Ironclad", 
            "Sterling", "Hawthorne", "Valen", "Frost", "Mercer", "Thorne", "Ashford", "Vanguard", "Pendleton"
        };

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

            // GUARANTEE: The Opening Screen is ALWAYS the first screen shown when pressing Play!
            if (openingScreenPanel != null) openingScreenPanel.SetActive(true);
            if (mainHUDPanel != null) mainHUDPanel.SetActive(false);
            if (characterCreationModal != null) characterCreationModal.SetActive(false);
            if (manageLivesModal != null) manageLivesModal.SetActive(false);

            // Runtime UI alignment safeguard: ensure proper scrolling, clipping, and element heights
            ApplyRuntimeLayoutSafeguards();
        }

        private void ApplyRuntimeLayoutSafeguards()
        {
            if (characterCreationModal != null)
            {
                // Ensure 100% solid background so title screen never bleeds or overlays
                var modalImg = characterCreationModal.GetComponent<Image>();
                if (modalImg != null)
                {
                    modalImg.color = new Color(0.07f, 0.09f, 0.13f, 1.0f);
                }

                var vlgs = characterCreationModal.GetComponentsInChildren<VerticalLayoutGroup>(true);
                foreach (var vlg in vlgs)
                {
                    vlg.childControlHeight = true;
                    vlg.childControlWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.childForceExpandWidth = true;
                    vlg.spacing = Mathf.Max(vlg.spacing, 14f);
                }

                var scroll = characterCreationModal.GetComponentInChildren<ScrollRect>(true);
                if (scroll != null)
                {
                    scroll.scrollSensitivity = 35f;
                    if (scroll.viewport == null)
                    {
                        var mask = scroll.GetComponentInChildren<RectMask2D>(true);
                        if (mask != null) scroll.viewport = mask.GetComponent<RectTransform>();
                        else scroll.gameObject.AddComponent<RectMask2D>();
                    }

                    if (scroll.content != null)
                    {
                        for (int i = 0; i < scroll.content.childCount; i++)
                        {
                            var child = scroll.content.GetChild(i).gameObject;
                            string cName = child.name;

                            // Hide any old legacy numbered headers if present in existing scenes
                            if (cName.EndsWith("Header") || cName.Contains("IdentityHeader") || cName.Contains("LineageHeader"))
                            {
                                child.SetActive(false);
                                continue;
                            }

                            var le = child.GetComponent<LayoutElement>();
                            if (le == null) le = child.AddComponent<LayoutElement>();

                            if (cName.Contains("Country"))
                            {
                                le.minHeight = 155f;
                                le.preferredHeight = 155f;
                            }
                            else if (cName.Contains("Fate") || cName.Contains("Lineage"))
                            {
                                le.minHeight = 140f;
                                le.preferredHeight = 140f;
                            }
                            else if (cName.Contains("Preview"))
                            {
                                le.minHeight = 130f;
                                le.preferredHeight = 130f;
                            }
                            else if (cName.Contains("RandLook") || cName.Contains("AppearanceBtn"))
                            {
                                le.minHeight = 65f;
                                le.preferredHeight = 65f;
                            }
                            else
                            {
                                if (le.minHeight < 60f) le.minHeight = 74f;
                                if (le.preferredHeight < 60f) le.preferredHeight = 74f;
                            }

                            var hlg = child.GetComponent<HorizontalLayoutGroup>();
                            if (hlg != null)
                            {
                                hlg.childControlWidth = true;
                                hlg.childControlHeight = true;
                                hlg.childForceExpandHeight = true;
                                hlg.spacing = Mathf.Max(hlg.spacing, 12f);
                            }
                        }
                    }
                }
            }
        }

        private void Start()
        {
            InitLineageList();
            WireEventListeners();
            RefreshMenuState();

            // Always display the opening screen first when game boots up
            ShowOpeningScreen();
        }

        private void InitLineageList()
        {
            availableLineages.Clear();
            if (GameManager.Instance != null && GameManager.Instance.familyLineageDatabase != null)
            {
                foreach (var lin in GameManager.Instance.familyLineageDatabase)
                {
                    if (lin != null) availableLineages.Add(lin);
                }
            }
        }

        private void WireEventListeners()
        {
            if (createNewLifeButton != null)
                createNewLifeButton.onClick.AddListener(OnCreateNewLifeClicked);

            if (continueOrManageButton != null)
                continueOrManageButton.onClick.AddListener(OnContinueOrManageClicked);

            if (randomizeNameButton != null)
                randomizeNameButton.onClick.AddListener(OnRandomizeNameClicked);

            if (characterFirstNameInput != null)
                characterFirstNameInput.onValueChanged.AddListener(_ => UpdateCreationUI());

            if (characterLastNameInput != null)
                characterLastNameInput.onValueChanged.AddListener(_ => UpdateCreationUI());

            if (maleGenderButton != null)
                maleGenderButton.onClick.AddListener(() => SetGender("Male"));

            if (femaleGenderButton != null)
                femaleGenderButton.onClick.AddListener(() => SetGender("Female"));

            if (prevCountryButton != null)
                prevCountryButton.onClick.AddListener(OnPrevCountryClicked);

            if (nextCountryButton != null)
                nextCountryButton.onClick.AddListener(OnNextCountryClicked);

            if (previousLineageButton != null)
                previousLineageButton.onClick.AddListener(OnPreviousLineageClicked);

            if (nextLineageButton != null)
                nextLineageButton.onClick.AddListener(OnNextLineageClicked);

            if (prevHairStyleButton != null)
                prevHairStyleButton.onClick.AddListener(OnPrevHairStyleClicked);
            if (nextHairStyleButton != null)
                nextHairStyleButton.onClick.AddListener(OnNextHairStyleClicked);

            if (prevHairColorButton != null)
                prevHairColorButton.onClick.AddListener(OnPrevHairColorClicked);
            if (nextHairColorButton != null)
                nextHairColorButton.onClick.AddListener(OnNextHairColorClicked);

            if (prevEyeColorButton != null)
                prevEyeColorButton.onClick.AddListener(OnPrevEyeColorClicked);
            if (nextEyeColorButton != null)
                nextEyeColorButton.onClick.AddListener(OnNextEyeColorClicked);

            if (prevSkinToneButton != null)
                prevSkinToneButton.onClick.AddListener(OnPrevSkinToneClicked);
            if (nextSkinToneButton != null)
                nextSkinToneButton.onClick.AddListener(OnNextSkinToneClicked);

            if (randomizeAppearanceButton != null)
                randomizeAppearanceButton.onClick.AddListener(OnRandomizeAppearanceClicked);

            if (confirmStartLifeButton != null)
                confirmStartLifeButton.onClick.AddListener(OnConfirmStartLifeClicked);

            if (cancelCreationButton != null)
                cancelCreationButton.onClick.AddListener(OnCancelCreationClicked);

            if (closeManageLivesButton != null)
                closeManageLivesButton.onClick.AddListener(OnCloseManageLivesClicked);

            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }

        public void RefreshMenuState()
        {
            InitLineageList();
            var lives = LifePersistenceManager.GetAllLives();
            int count = lives != null ? lives.Count : 0;

            if (totalLivesCountLabel != null)
            {
                totalLivesCountLabel.text = count > 0 
                    ? $"Active Military Service Records: <b>{count}</b>" 
                    : "No Recorded Service Lives Found";
            }

            if (continueOrManageButton != null && continueOrManageButtonText != null)
            {
                if (count == 0)
                {
                    // 0 lives: Button disabled
                    continueOrManageButton.interactable = false;
                    continueOrManageButtonText.text = "CONTINUE (NO SAVES)";
                    continueOrManageButtonText.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
                }
                else if (count == 1)
                {
                    // Exactly 1 life: "CONTINUE"
                    var singleLife = lives[0];
                    continueOrManageButton.interactable = true;
                    string status = singleLife.isDead ? " [DECEASED]" : (singleLife.isRetired ? " [RETIRED]" : "");
                    continueOrManageButtonText.text = $"CONTINUE ({singleLife.characterName}{status})";
                    continueOrManageButtonText.color = new Color(0.3f, 1f, 0.6f);
                }
                else
                {
                    // More than 1 life: "MANAGE LIVES"
                    continueOrManageButton.interactable = true;
                    continueOrManageButtonText.text = $"MANAGE LIVES ({count})";
                    continueOrManageButtonText.color = new Color(0.4f, 0.8f, 1f);
                }
            }
        }

        public void ShowOpeningScreen()
        {
            if (openingScreenPanel != null) openingScreenPanel.SetActive(true);
            if (mainHUDPanel != null) mainHUDPanel.SetActive(false);
            if (characterCreationModal != null) characterCreationModal.SetActive(false);
            if (manageLivesModal != null) manageLivesModal.SetActive(false);

            RefreshMenuState();
        }

        public void ShowMainHUD()
        {
            if (openingScreenPanel != null) openingScreenPanel.SetActive(false);
            if (characterCreationModal != null) characterCreationModal.SetActive(false);
            if (manageLivesModal != null) manageLivesModal.SetActive(false);
            if (mainHUDPanel != null) mainHUDPanel.SetActive(true);

            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                var hud = mainHUDPanel != null ? mainHUDPanel.GetComponent<MainHUDView>() : null;
                if (hud != null) hud.UpdateHUD(GameManager.Instance.currentSoldier);
            }
        }

        private void OnContinueOrManageClicked()
        {
            var lives = LifePersistenceManager.GetAllLives();
            int count = lives != null ? lives.Count : 0;

            if (count == 0) return;
            
            if (count == 1)
            {
                ResumeLifeById(lives[0].lifeId);
            }
            else
            {
                OpenManageLivesModal();
            }
        }

        private void ResumeLifeById(string lifeId)
        {
            ShowMainHUD();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeLife(lifeId);
            }
        }

        // ==================== CHARACTER CREATION ====================

        public void OpenCharacterCreationModal() => OnCreateNewLifeClicked();

        private void OnCreateNewLifeClicked()
        {
            if (characterCreationModal != null)
            {
                if (openingScreenPanel != null) openingScreenPanel.SetActive(false);
                if (manageLivesModal != null) manageLivesModal.SetActive(false);
                if (mainHUDPanel != null) mainHUDPanel.SetActive(false);

                characterCreationModal.SetActive(true);
                selectedLineageIndex = 0;
                selectedCountryIndex = 0;
                selectedGender = "Male";
                
                OnRandomizeNameClicked();
                OnRandomizeAppearanceClicked();
                UpdateCreationUI();

                ApplyRuntimeLayoutSafeguards();
                Canvas.ForceUpdateCanvases();
                var rt = characterCreationModal.GetComponent<RectTransform>();
                if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        private void SetGender(string gender)
        {
            selectedGender = gender;
            string[] firstPool = (selectedGender == "Female") ? FemaleFirstNames : MaleFirstNames;
            if (characterFirstNameInput != null)
            {
                characterFirstNameInput.text = firstPool[UnityEngine.Random.Range(0, firstPool.Length)];
            }
            UpdateCreationUI();
        }

        private void OnRandomizeNameClicked()
        {
            string[] firstPool = (selectedGender == "Female") ? FemaleFirstNames : MaleFirstNames;
            string first = firstPool[UnityEngine.Random.Range(0, firstPool.Length)];
            string last = LastNames[UnityEngine.Random.Range(0, LastNames.Length)];

            if (characterFirstNameInput != null) characterFirstNameInput.text = first;
            if (characterLastNameInput != null) characterLastNameInput.text = last;

            UpdateCreationUI();
        }

        private void OnPrevCountryClicked()
        {
            selectedCountryIndex = (selectedCountryIndex - 1 + Countries.Length) % Countries.Length;
            UpdateCreationUI();
        }

        private void OnNextCountryClicked()
        {
            selectedCountryIndex = (selectedCountryIndex + 1) % Countries.Length;
            UpdateCreationUI();
        }

        private void OnPreviousLineageClicked()
        {
            selectedLineageIndex = (selectedLineageIndex - 1 + Origins.Length) % Origins.Length;
            UpdateCreationUI();
        }

        private void OnNextLineageClicked()
        {
            selectedLineageIndex = (selectedLineageIndex + 1) % Origins.Length;
            UpdateCreationUI();
        }

        private void OnPrevHairStyleClicked()
        {
            selectedHairStyleIndex = (selectedHairStyleIndex - 1 + HairStyles.Length) % HairStyles.Length;
            UpdateCreationUI();
        }

        private void OnNextHairStyleClicked()
        {
            selectedHairStyleIndex = (selectedHairStyleIndex + 1) % HairStyles.Length;
            UpdateCreationUI();
        }

        private void OnPrevHairColorClicked()
        {
            selectedHairColorIndex = (selectedHairColorIndex - 1 + HairColors.Length) % HairColors.Length;
            UpdateCreationUI();
        }

        private void OnNextHairColorClicked()
        {
            selectedHairColorIndex = (selectedHairColorIndex + 1) % HairColors.Length;
            UpdateCreationUI();
        }

        private void OnPrevEyeColorClicked()
        {
            selectedEyeColorIndex = (selectedEyeColorIndex - 1 + EyeColors.Length) % EyeColors.Length;
            UpdateCreationUI();
        }

        private void OnNextEyeColorClicked()
        {
            selectedEyeColorIndex = (selectedEyeColorIndex + 1) % EyeColors.Length;
            UpdateCreationUI();
        }

        private void OnPrevSkinToneClicked()
        {
            selectedSkinToneIndex = (selectedSkinToneIndex - 1 + SkinTones.Length) % SkinTones.Length;
            UpdateCreationUI();
        }

        private void OnNextSkinToneClicked()
        {
            selectedSkinToneIndex = (selectedSkinToneIndex + 1) % SkinTones.Length;
            UpdateCreationUI();
        }

        private void OnRandomizeAppearanceClicked()
        {
            selectedHairStyleIndex = UnityEngine.Random.Range(0, HairStyles.Length);
            selectedHairColorIndex = UnityEngine.Random.Range(0, HairColors.Length);
            selectedEyeColorIndex = UnityEngine.Random.Range(0, EyeColors.Length);
            selectedSkinToneIndex = UnityEngine.Random.Range(0, SkinTones.Length);
            UpdateCreationUI();
        }

        private void UpdateCreationUI()
        {
            // Gender Buttons Highlight
            if (maleGenderButton != null)
            {
                var img = maleGenderButton.GetComponent<Image>();
                if (img != null) img.color = (selectedGender == "Male") ? new Color(0.2f, 0.5f, 0.85f) : new Color(0.18f, 0.22f, 0.28f);
            }
            if (femaleGenderButton != null)
            {
                var img = femaleGenderButton.GetComponent<Image>();
                if (img != null) img.color = (selectedGender == "Female") ? new Color(0.85f, 0.3f, 0.6f) : new Color(0.18f, 0.22f, 0.28f);
            }

            // Country
            var country = Countries[selectedCountryIndex];
            if (countryTitleText != null) countryTitleText.text = $"<color=#00FFAA><b>{country.name}</b></color>";
            if (countryDescText != null) countryDescText.text = country.desc;

            // Background Origin
            var origin = Origins[selectedLineageIndex % Origins.Length];
            if (lineageTitleText != null) lineageTitleText.text = $"<color=#FFD700><b>{origin.title}</b></color>";
            if (lineageDetailsText != null) lineageDetailsText.text = origin.desc;

            // Appearance Texts
            if (hairStyleText != null) hairStyleText.text = HairStyles[selectedHairStyleIndex];
            if (hairColorText != null) hairColorText.text = HairColors[selectedHairColorIndex];
            if (eyeColorText != null) eyeColorText.text = EyeColors[selectedEyeColorIndex];
            if (skinToneText != null) skinToneText.text = SkinTones[selectedSkinToneIndex];

            // Summary Badge
            if (characterPreviewBadgeText != null)
            {
                string first = (characterFirstNameInput != null && !string.IsNullOrWhiteSpace(characterFirstNameInput.text)) ? characterFirstNameInput.text.Trim() : "Ethan";
                string last = (characterLastNameInput != null && !string.IsNullOrWhiteSpace(characterLastNameInput.text)) ? characterLastNameInput.text.Trim() : "Vance";
                string genderIcon = (selectedGender == "Female") ? "[F]" : "[M]";
                characterPreviewBadgeText.text = 
                    $"<color=#00D4FF><b>{genderIcon} {first} {last}</b></color> (Enlistment Age 18.0)\n" +
                    $"<b>Service:</b> 1st Infantry Division  |  <b>Initial Rank:</b> Private Recruit (E-1)\n" +
                    $"<b>Origin:</b> {origin.title}";

                if (confirmStartLifeButton != null)
                {
                    var confirmTxt = confirmStartLifeButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (confirmTxt != null) confirmTxt.text = $"ENLIST {first.ToUpper()} INTO SERVICE!";
                }
            }
        }

        private void OnConfirmStartLifeClicked()
        {
            string first = (characterFirstNameInput != null && !string.IsNullOrWhiteSpace(characterFirstNameInput.text)) ? characterFirstNameInput.text.Trim() : "Ethan";
            string last = (characterLastNameInput != null && !string.IsNullOrWhiteSpace(characterLastNameInput.text)) ? characterLastNameInput.text.Trim() : "Vance";
            string fullName = $"{first} {last}";

            FamilyLineageSO chosenLineage = null;
            SoldierOrigin chosenOrigin = (SoldierOrigin)(selectedLineageIndex % Origins.Length);

            string country = Countries[selectedCountryIndex].name;
            string hairStyle = HairStyles[selectedHairStyleIndex];
            string hairColor = HairColors[selectedHairColorIndex];
            string eyeColor = EyeColors[selectedEyeColorIndex];
            string skinTone = SkinTones[selectedSkinToneIndex];

            // Activate Main HUD first so all event listeners (BitLifeFeedView, MainHUDView) are registered!
            ShowMainHUD();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewLife(
                    fullName, 
                    chosenLineage, 
                    selectedGender, 
                    country, 
                    hairStyle, 
                    hairColor, 
                    eyeColor, 
                    skinTone, 
                    looks: 85f,
                    explicitTalent: null,
                    origin: chosenOrigin);
            }
        }

        private void OnCancelCreationClicked()
        {
            ShowOpeningScreen();
        }

        // ==================== MANAGE LIVES MODAL ====================

        public void OpenManageLivesModal()
        {
            if (openingScreenPanel != null) openingScreenPanel.SetActive(false);
            if (characterCreationModal != null) characterCreationModal.SetActive(false);
            if (mainHUDPanel != null) mainHUDPanel.SetActive(false);

            if (manageLivesModal != null)
            {
                manageLivesModal.SetActive(true);
                PopulateManageLivesList();
            }
        }

        private void OnCloseManageLivesClicked()
        {
            ShowOpeningScreen();
        }

        public void PopulateManageLivesList()
        {
            if (livesListContent == null) return;

            foreach (Transform child in livesListContent)
            {
                Destroy(child.gameObject);
            }

            var lives = LifePersistenceManager.GetAllLives();
            if (lives == null || lives.Count == 0)
            {
                var emptyObj = new GameObject("EmptyNotice");
                emptyObj.transform.SetParent(livesListContent, false);
                var emptyText = emptyObj.AddComponent<TextMeshProUGUI>();
                emptyText.text = "No saved military lives found.";
                emptyText.fontSize = 28;
                emptyText.alignment = TextAlignmentOptions.Center;
                emptyText.color = new Color(0.7f, 0.7f, 0.7f);
                return;
            }

            foreach (var life in lives)
            {
                CreateLifeEntryUI(life);
            }
        }

        private void CreateLifeEntryUI(SavedLifeData life)
        {
            var cardObj = new GameObject($"LifeCard_{life.lifeId}");
            cardObj.transform.SetParent(livesListContent, false);

            var rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 180);

            var bg = cardObj.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.17f, 0.21f);

            var layout = cardObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 15, 15);
            layout.spacing = 10;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var csf = cardObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 1. Header Row
            var headerObj = new GameObject("HeaderRow");
            headerObj.transform.SetParent(cardObj.transform, false);
            var headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.fontSize = 30;
            headerText.fontStyle = FontStyles.Bold;
            headerObj.AddComponent<LayoutElement>().preferredHeight = 40;

            string statusColor = life.isDead ? "#FF4444" : (life.isRetired ? "#FFD700" : "#00FFAA");
            string statusStr = life.isDead ? "KIA / DECEASED" : (life.isRetired ? "HONORABLY RETIRED" : "ACTIVE DUTY");
            string genIcon = (life.gender == "Female") ? "[F]" : "[M]";
            headerText.text = $"<b>{life.characterName} ({genIcon})</b>  <color={statusColor}>[{statusStr}]</color>";

            // 2. Details Row
            var detailsObj = new GameObject("DetailsRow");
            detailsObj.transform.SetParent(cardObj.transform, false);
            var detailsText = detailsObj.AddComponent<TextMeshProUGUI>();
            detailsText.fontSize = 22;
            detailsText.color = new Color(0.8f, 0.85f, 0.9f);
            string powerDisplay = life.powerTier > 0 
                ? $"Tier {life.powerTier} ({PowerTierKnowledgeSystem.GetSubTierName(life.subTier)})/12" 
                : $"Pre-Tier (Stage {Mathf.Clamp(life.preTierStage, 1, 4)}/4)";

            detailsText.text = 
                $"Homeland: <b>{life.birthCountry ?? "The Valerian Empire"}</b> | Age: <b>{life.age:F1}</b> | Rank: <b>{life.rankName} ({life.rankPaygrade})</b>\n" +
                $"Lineage: <b>{life.lineageArchetypeName}</b> | Power: <b>{powerDisplay}</b> | Wealth: <b>${life.cashBalance:N0}</b>\n" +
                $"Look: <i>{life.hairStyle}, {life.hairColor} hair, {life.eyeColor} eyes</i>";

            // 3. Buttons Row
            var btnRowObj = new GameObject("ButtonsRow");
            btnRowObj.transform.SetParent(cardObj.transform, false);
            btnRowObj.AddComponent<LayoutElement>().preferredHeight = 50;
            var btnRowLayout = btnRowObj.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 20;
            btnRowLayout.childControlHeight = true;
            btnRowLayout.childControlWidth = true;
            btnRowLayout.childForceExpandHeight = true;
            btnRowLayout.childForceExpandWidth = true;

            var rowRect = btnRowObj.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 50);

            // Play / Resume Button
            var playBtnObj = new GameObject("PlayBtn");
            playBtnObj.transform.SetParent(btnRowObj.transform, false);
            var playBg = playBtnObj.AddComponent<Image>();
            playBg.color = new Color(0.18f, 0.65f, 0.35f);
            var playBtn = playBtnObj.AddComponent<Button>();
            var playText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            playText.transform.SetParent(playBtnObj.transform, false);
            playText.text = life.isDead ? "REINCARNATE / VIEW" : "RESUME LIFE";
            playText.alignment = TextAlignmentOptions.Center;
            playText.fontSize = 24;
            playText.fontStyle = FontStyles.Bold;
            SetFullScreen(playText.GetComponent<RectTransform>());

            string currentLifeId = life.lifeId;
            playBtn.onClick.AddListener(() =>
            {
                manageLivesModal.SetActive(false);
                ResumeLifeById(currentLifeId);
            });

            // Delete Button
            var delBtnObj = new GameObject("DelBtn");
            delBtnObj.transform.SetParent(btnRowObj.transform, false);
            var delBg = delBtnObj.AddComponent<Image>();
            delBg.color = new Color(0.75f, 0.20f, 0.20f);
            var delBtn = delBtnObj.AddComponent<Button>();
            var delText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            delText.transform.SetParent(delBtnObj.transform, false);
            delText.text = "DISCHARGE / DELETE";
            delText.alignment = TextAlignmentOptions.Center;
            delText.fontSize = 22;
            delText.fontStyle = FontStyles.Bold;
            SetFullScreen(delText.GetComponent<RectTransform>());

            delBtn.onClick.AddListener(() =>
            {
                LifePersistenceManager.DeleteLife(currentLifeId);
                PopulateManageLivesList();
                RefreshMenuState();
            });
        }

        private void OnReturnToMenuClicked()
        {
            if (GameManager.Instance != null && GameManager.Instance.currentSoldier != null)
            {
                LifePersistenceManager.SaveSoldierLife(GameManager.Instance.currentSoldier);
            }

            ShowOpeningScreen();
        }

        private static void SetFullScreen(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
