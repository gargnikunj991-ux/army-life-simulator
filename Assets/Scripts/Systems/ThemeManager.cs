using System;
using UnityEngine;

namespace ArmyLife.Systems
{
    public enum ThemeMode
    {
        Dark,       // Gritty Tactical (Gunmetal Slate / Olive Drab / Brass Gold)
        Light,      // Classified Field Dossier (Field Khaki Parchment / Stencil Ink)
        DesertTan,  // Arid Combat (Desert Coyote / Scorched Dust / Bronze)
        NightOps    // War Room Phosphor HUD (Stealth Black / Radar Green)
    }

    [Serializable]
    public struct ThemePalette
    {
        public ThemeMode mode;
        public string themeName;            // Human-readable theme label

        public Color backgroundColor;       // Main canvas / root background
        public Color panelBackground;       // Header / Bottom bar / Top container
        public Color cardBackground;        // Feed background / modal dialog background
        public Color subCardBackground;     // Inner cards / scroll items
        public Color borderColor;           // Outline borders / dividers

        public Color textPrimary;           // Primary headings and main text
        public Color textSecondary;         // Labels, dates, status subtext
        public Color textMuted;             // Subtle hints, inactive labels

        public Color btnNormalBg;           // Standard action buttons
        public Color btnNormalText;         // Standard action button text
        public Color btnAgeGrowBg;          // Special Age/Advance center button
        public Color btnAgeGrowText;        // Special Age/Advance button text

        // Military Accents
        public Color accentGold;            // Polished Brass / Medals / Officer Ranks
        public Color accentEmerald;         // Army Payroll / Field Readiness / Survival
        public Color accentCobalt;          // Tactical Recon / Intellect / Navy
        public Color accentPurple;          // Honors & Commendations (backwards compatibility)
        public Color accentCrimson;         // Combat Alert / Critical Wounds / K.I.A.
        public Color accentCoyote;          // Tactical Coyote Tan / Field Gear / Ammo
        public Color accentOlive;           // Military Olive Drab / Standard Issue

        // Stat Gauges
        public Color healthBarColor;        // Health / Vitality fill
        public Color fitnessBarColor;       // Readiness / Physical stamina fill
        public Color combatBarColor;        // Marksmanship / Weapon skill fill
        public Color intellectBarColor;     // Discipline / Tactical doctrine fill
        public Color barBackgroundColor;    // Gauge trough background
    }

    public class ThemeManager : MonoBehaviour
    {
        private static ThemeManager _instance;
        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<ThemeManager>(FindObjectsInactive.Include);
                    if (_instance == null)
                    {
                        var go = new GameObject("ThemeManager");
                        _instance = go.AddComponent<ThemeManager>();
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        private const string THEME_PREF_KEY = "ArmyLife_ThemeMode";

        [SerializeField] private ThemeMode currentMode = ThemeMode.Dark;

        public static event Action<ThemeMode, ThemePalette> OnThemeChanged;

        public ThemeMode CurrentMode => currentMode;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                LoadSavedTheme();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Broadcast initial active theme so all UI components adapt on startup
            OnThemeChanged?.Invoke(currentMode, GetPalette(currentMode));
        }

        public void LoadSavedTheme()
        {
            string saved = PlayerPrefs.GetString(THEME_PREF_KEY, "Dark");
            if (Enum.TryParse<ThemeMode>(saved, true, out var loaded))
            {
                currentMode = loaded;
            }
            else
            {
                currentMode = ThemeMode.Dark;
            }
        }

        public void SetTheme(ThemeMode newMode)
        {
            currentMode = newMode;
            PlayerPrefs.SetString(THEME_PREF_KEY, currentMode.ToString());
            PlayerPrefs.Save();

            ThemePalette palette = GetPalette(currentMode);
            OnThemeChanged?.Invoke(currentMode, palette);
        }

        public void ToggleTheme()
        {
            CycleNextTheme();
        }

        public void CycleNextTheme()
        {
            int total = Enum.GetValues(typeof(ThemeMode)).Length;
            int next = ((int)currentMode + 1) % total;
            SetTheme((ThemeMode)next);
        }

        public ThemePalette GetCurrentPalette()
        {
            return GetPalette(currentMode);
        }

        public static string GetThemeDisplayName(ThemeMode mode)
        {
            switch (mode)
            {
                case ThemeMode.Dark:      return "Tactical Drab (Dark)";
                case ThemeMode.Light:     return "Classified Dossier (Light)";
                case ThemeMode.DesertTan: return "Desert Coyote (Arid)";
                case ThemeMode.NightOps:  return "Night Ops (Phosphor HUD)";
                default:                  return mode.ToString();
            }
        }

        public static ThemePalette GetPalette(ThemeMode mode)
        {
            switch (mode)
            {
                case ThemeMode.Light:
                    return new ThemePalette
                    {
                        mode = ThemeMode.Light,
                        themeName = "Classified Dossier (Light)",

                        // Aged Field Manual / Manila Cardstock
                        backgroundColor = new Color(0.918f, 0.894f, 0.843f, 1f),      // #EAE4D7 Aged field paper
                        panelBackground = new Color(0.863f, 0.835f, 0.773f, 1f),      // #DCD5C5 Heavy dossier board
                        cardBackground = new Color(0.961f, 0.945f, 0.910f, 1f),       // #F5F1E8 Clean field stationery
                        subCardBackground = new Color(0.894f, 0.867f, 0.816f, 1f),    // #E4DDD0 Inner docket card
                        borderColor = new Color(0.702f, 0.659f, 0.580f, 1f),          // #B3A894 Weathered binder edge

                        textPrimary = new Color(0.094f, 0.110f, 0.094f, 1f),          // #181C18 Deep typewriter ink
                        textSecondary = new Color(0.247f, 0.286f, 0.247f, 1f),        // #3F493F Field carbon record
                        textMuted = new Color(0.427f, 0.467f, 0.424f, 1f),            // #6D776C Faint stencil gray

                        btnNormalBg = new Color(0.784f, 0.753f, 0.686f, 1f),          // #C8C0AF Sturdy dossier tab
                        btnNormalText = new Color(0.086f, 0.102f, 0.086f, 1f),        // #161A16 Stencil text
                        btnAgeGrowBg = new Color(0.184f, 0.329f, 0.204f, 1f),         // #2F5434 Deep Olive Seal
                        btnAgeGrowText = new Color(0.973f, 0.965f, 0.941f, 1f),       // #F8F6F0 Ivory text

                        accentGold = new Color(0.612f, 0.451f, 0.102f, 1f),           // #9C731A Antique medal bronze
                        accentEmerald = new Color(0.137f, 0.420f, 0.231f, 1f),        // #236B3B Official stamp green
                        accentCobalt = new Color(0.153f, 0.353f, 0.510f, 1f),         // #275A82 Naval dispatch blue
                        accentPurple = new Color(0.369f, 0.239f, 0.510f, 1f),         // #5E3D82 Regimental violet
                        accentCrimson = new Color(0.651f, 0.137f, 0.137f, 1f),        // #A62323 REDACTED crimson stamp
                        accentCoyote = new Color(0.510f, 0.373f, 0.180f, 1f),         // #825F2E Field leather
                        accentOlive = new Color(0.227f, 0.310f, 0.196f, 1f),          // #3A4F32 Field coat olive

                        healthBarColor = new Color(0.690f, 0.169f, 0.169f, 1f),       // Red cross field dressing
                        fitnessBarColor = new Color(0.173f, 0.482f, 0.263f, 1f),      // Stamina green
                        combatBarColor = new Color(0.702f, 0.408f, 0.133f, 1f),       // Marksmanship amber
                        intellectBarColor = new Color(0.180f, 0.384f, 0.541f, 1f),    // Doctrine navy
                        barBackgroundColor = new Color(0.773f, 0.741f, 0.690f, 1f)    // Trough paper
                    };

                case ThemeMode.DesertTan:
                    return new ThemePalette
                    {
                        mode = ThemeMode.DesertTan,
                        themeName = "Desert Coyote (Arid)",

                        // Scorched Earth / Desert Storm Kevlar / Coyote Tan
                        backgroundColor = new Color(0.102f, 0.086f, 0.071f, 1f),      // #1A1612 Dark scorched earth
                        panelBackground = new Color(0.157f, 0.129f, 0.106f, 1f),      // #28211B Coyote armor plate
                        cardBackground = new Color(0.118f, 0.098f, 0.078f, 1f),       // #1E1914 Arid outpost card
                        subCardBackground = new Color(0.180f, 0.149f, 0.122f, 1f),    // #2E261F Desert equipment tray
                        borderColor = new Color(0.294f, 0.235f, 0.184f, 1f),          // #4B3C2F Weathered bronze border

                        textPrimary = new Color(0.961f, 0.929f, 0.882f, 1f),          // #F5EDE1 Desert chalk white
                        textSecondary = new Color(0.722f, 0.659f, 0.580f, 1f),        // #B8A894 Khaki sand dust
                        textMuted = new Color(0.459f, 0.404f, 0.333f, 1f),            // #756755 Muted tan

                        btnNormalBg = new Color(0.220f, 0.184f, 0.149f, 1f),          // #382F26 Coyote brown button
                        btnNormalText = new Color(0.937f, 0.910f, 0.867f, 1f),        // #EFE8DD Light tan text
                        btnAgeGrowBg = new Color(0.612f, 0.384f, 0.157f, 1f),         // #9C6228 Desert storm amber
                        btnAgeGrowText = Color.white,

                        accentGold = new Color(0.875f, 0.671f, 0.263f, 1f),           // #DFAB43 Desert brass
                        accentEmerald = new Color(0.282f, 0.620f, 0.384f, 1f),        // #489E62 Oasis green
                        accentCobalt = new Color(0.263f, 0.498f, 0.639f, 1f),         // #437FA3 Sky reconnaissance
                        accentPurple = new Color(0.541f, 0.376f, 0.580f, 1f),         // #8A6094 Muted amethyst
                        accentCrimson = new Color(0.761f, 0.263f, 0.220f, 1f),        // #C24338 Red sand alert
                        accentCoyote = new Color(0.831f, 0.639f, 0.384f, 1f),         // #D4A362 Coyote tan
                        accentOlive = new Color(0.420f, 0.455f, 0.298f, 1f),          // #6B744C Arid olive camo

                        healthBarColor = new Color(0.722f, 0.220f, 0.188f, 1f),
                        fitnessBarColor = new Color(0.282f, 0.620f, 0.384f, 1f),
                        combatBarColor = new Color(0.851f, 0.533f, 0.204f, 1f),
                        intellectBarColor = new Color(0.263f, 0.498f, 0.639f, 1f),
                        barBackgroundColor = new Color(0.141f, 0.118f, 0.094f, 1f)
                    };

                case ThemeMode.NightOps:
                    return new ThemePalette
                    {
                        mode = ThemeMode.NightOps,
                        themeName = "Night Ops (Phosphor HUD)",

                        // Night Vision Scope / Radar CRT / Stealth Black
                        backgroundColor = new Color(0.027f, 0.039f, 0.031f, 1f),      // #070A08 Pitch black
                        panelBackground = new Color(0.055f, 0.078f, 0.063f, 1f),      // #0E1410 Stealth panel
                        cardBackground = new Color(0.039f, 0.059f, 0.047f, 1f),       // #0A0F0C Radar CRT card
                        subCardBackground = new Color(0.071f, 0.110f, 0.082f, 1f),    // #121C15 FLIR sensor card
                        borderColor = new Color(0.110f, 0.200f, 0.133f, 1f),          // #1C3322 Night vision grid

                        textPrimary = new Color(0.200f, 1.000f, 0.467f, 1f),          // #33FF77 Bright phosphor green
                        textSecondary = new Color(0.125f, 0.761f, 0.353f, 1f),        // #20C25A Telemetry green
                        textMuted = new Color(0.098f, 0.388f, 0.208f, 1f),            // #196335 Subdued grid green

                        btnNormalBg = new Color(0.075f, 0.145f, 0.090f, 1f),          // #132517 Console button
                        btnNormalText = new Color(0.200f, 1.000f, 0.467f, 1f),        // #33FF77 Phosphor text
                        btnAgeGrowBg = new Color(0.122f, 0.522f, 0.251f, 1f),         // #1F8540 Active radar ping
                        btnAgeGrowText = new Color(0.910f, 1.000f, 0.910f, 1f),       // #E8FFE8 Pale green

                        accentGold = new Color(1.000f, 0.784f, 0.231f, 1f),           // #FFC83B Targeting amber
                        accentEmerald = new Color(0.176f, 0.902f, 0.431f, 1f),        // #2DE66E Locked green
                        accentCobalt = new Color(0.176f, 0.639f, 0.902f, 1f),         // #2DA3E6 Blue force tracker
                        accentPurple = new Color(0.612f, 0.294f, 0.910f, 1f),         // #9C4BE8 ECM violet
                        accentCrimson = new Color(1.000f, 0.231f, 0.231f, 1f),        // #FF3B3B Hostile SAM alert
                        accentCoyote = new Color(0.831f, 0.639f, 0.349f, 1f),         // #D4A359 Thermal orange/coyote
                        accentOlive = new Color(0.302f, 0.490f, 0.259f, 1f),          // #4D7D42 Night camo

                        healthBarColor = new Color(1.000f, 0.231f, 0.231f, 1f),
                        fitnessBarColor = new Color(0.176f, 0.902f, 0.431f, 1f),
                        combatBarColor = new Color(1.000f, 0.624f, 0.141f, 1f),
                        intellectBarColor = new Color(0.176f, 0.639f, 0.902f, 1f),
                        barBackgroundColor = new Color(0.059f, 0.102f, 0.071f, 1f)
                    };

                case ThemeMode.Dark:
                default:
                    return new ThemePalette
                    {
                        mode = ThemeMode.Dark,
                        themeName = "Tactical Drab (Dark)",

                        // Gritty Military & Tactical: Gunmetal Charcoal & Olive Drab
                        backgroundColor = new Color(0.063f, 0.075f, 0.067f, 1f),      // #101311 Deep tactical carbon
                        panelBackground = new Color(0.098f, 0.125f, 0.106f, 1f),      // #19201B Field gunmetal & drab
                        cardBackground = new Color(0.075f, 0.094f, 0.078f, 1f),       // #131814 Matte ballistic container
                        subCardBackground = new Color(0.114f, 0.145f, 0.122f, 1f),    // #1D251F Inner equipment tray
                        borderColor = new Color(0.173f, 0.227f, 0.184f, 1f),          // #2C3A2F Muted military olive border

                        textPrimary = new Color(0.937f, 0.949f, 0.925f, 1f),          // #EFF2EC Crisp chalk white
                        textSecondary = new Color(0.639f, 0.675f, 0.631f, 1f),        // #A3ACA1 Field equipment slate
                        textMuted = new Color(0.408f, 0.451f, 0.400f, 1f),            // #687366 Camo gray

                        btnNormalBg = new Color(0.141f, 0.188f, 0.149f, 1f),          // #243026 Stout olive drab button
                        btnNormalText = new Color(0.898f, 0.922f, 0.886f, 1f),        // #E5EBE2 Off-white button text
                        btnAgeGrowBg = new Color(0.231f, 0.424f, 0.259f, 1f),         // #3B6C42 Standard issue army green
                        btnAgeGrowText = Color.white,

                        accentGold = new Color(0.831f, 0.686f, 0.216f, 1f),           // #D4AF37 Polished officer brass gold
                        accentEmerald = new Color(0.220f, 0.651f, 0.369f, 1f),        // #38A65E Army payroll green & health
                        accentCobalt = new Color(0.294f, 0.541f, 0.722f, 1f),         // #4B8AB8 Reconnaissance & staff blue
                        accentPurple = new Color(0.557f, 0.384f, 0.769f, 1f),         // #8E62C4 Special citations violet
                        accentCrimson = new Color(0.780f, 0.224f, 0.180f, 1f),        // #C7392E Combat alert & casualty crimson
                        accentCoyote = new Color(0.780f, 0.612f, 0.337f, 1f),         // #C79C56 Tactical coyote tan
                        accentOlive = new Color(0.373f, 0.482f, 0.290f, 1f),          // #5F7B4A Standard Olive Drab

                        healthBarColor = new Color(0.722f, 0.196f, 0.196f, 1f),       // Combat wound / health red
                        fitnessBarColor = new Color(0.220f, 0.651f, 0.369f, 1f),      // Readiness stamina green
                        combatBarColor = new Color(0.851f, 0.522f, 0.196f, 1f),       // Marksmanship ammo amber
                        intellectBarColor = new Color(0.243f, 0.494f, 0.659f, 1f),    // Discipline doctrine blue
                        barBackgroundColor = new Color(0.082f, 0.106f, 0.086f, 1f)    // #151B16 Dark trough
                    };
            }
        }
    }
}
