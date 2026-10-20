using System;
using UnityEngine;

namespace ArmyLife.Systems
{
    public enum ThemeMode
    {
        Dark,
        Light
    }

    [Serializable]
    public struct ThemePalette
    {
        public ThemeMode mode;
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
        public Color btnAgeGrowBg;          // Special Age/Grow center button
        public Color btnAgeGrowText;        // Special Age/Grow button text

        public Color accentGold;            // Clan / Nobility / Prestige
        public Color accentEmerald;         // Cash / Vitality / Success
        public Color accentCobalt;          // School / Ranks / Technology
        public Color accentPurple;          // Cultivation / CMP / Spirit
        public Color accentCrimson;         // Danger / Health / War
    }

    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager Instance { get; private set; }

        private const string THEME_PREF_KEY = "ArmyLife_ThemeMode";

        [SerializeField] private ThemeMode currentMode = ThemeMode.Dark;

        public static event Action<ThemeMode, ThemePalette> OnThemeChanged;

        public ThemeMode CurrentMode => currentMode;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSavedTheme();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSavedTheme()
        {
            string saved = PlayerPrefs.GetString(THEME_PREF_KEY, "Dark");
            if (Enum.TryParse(saved, out ThemeMode loaded))
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
            SetTheme(currentMode == ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark);
        }

        public ThemePalette GetCurrentPalette()
        {
            return GetPalette(currentMode);
        }

        public static ThemePalette GetPalette(ThemeMode mode)
        {
            if (mode == ThemeMode.Light)
            {
                return new ThemePalette
                {
                    mode = ThemeMode.Light,
                    // Modern porcelain & clean ivory slate
                    backgroundColor = new Color(0.94f, 0.95f, 0.97f, 1f),      // #F0F2F7
                    panelBackground = new Color(1.00f, 1.00f, 1.00f, 1f),      // Pure White
                    cardBackground = new Color(0.96f, 0.97f, 0.99f, 1f),       // Soft Ice White
                    subCardBackground = new Color(0.91f, 0.93f, 0.96f, 1f),    // Light Slate Card
                    borderColor = new Color(0.80f, 0.84f, 0.89f, 1f),          // Soft Divider Border

                    textPrimary = new Color(0.08f, 0.11f, 0.16f, 1f),          // Deep Obsidian Charcoal
                    textSecondary = new Color(0.30f, 0.36f, 0.44f, 1f),        // Muted Slate Text
                    textMuted = new Color(0.55f, 0.60f, 0.68f, 1f),            // Faint Hint Text

                    btnNormalBg = new Color(0.88f, 0.91f, 0.95f, 1f),          // Clean Light Button
                    btnNormalText = new Color(0.10f, 0.14f, 0.20f, 1f),        // Dark Charcoal Text
                    btnAgeGrowBg = new Color(0.08f, 0.65f, 0.36f, 1f),         // Vibrant Emerald
                    btnAgeGrowText = Color.white,

                    accentGold = new Color(0.82f, 0.52f, 0.05f, 1f),           // Royal Amber Gold
                    accentEmerald = new Color(0.05f, 0.60f, 0.30f, 1f),        // Rich Jade/Emerald
                    accentCobalt = new Color(0.12f, 0.44f, 0.85f, 1f),         // Vivid Azure Blue
                    accentPurple = new Color(0.48f, 0.25f, 0.78f, 1f),         // Royal Imperial Violet
                    accentCrimson = new Color(0.85f, 0.18f, 0.22f, 1f)         // Ruby Crimson
                };
            }
            else
            {
                return new ThemePalette
                {
                    mode = ThemeMode.Dark,
                    // Premium tactical slate & deep graphite
                    backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f),      // #0F1217
                    panelBackground = new Color(0.11f, 0.14f, 0.18f, 1f),      // #1C232E
                    cardBackground = new Color(0.09f, 0.11f, 0.14f, 1f),       // Deep Slate Card
                    subCardBackground = new Color(0.14f, 0.18f, 0.23f, 1f),    // Inner Card Container
                    borderColor = new Color(0.20f, 0.25f, 0.32f, 1f),          // Muted Border

                    textPrimary = new Color(0.96f, 0.97f, 0.99f, 1f),          // Crisp Pure White
                    textSecondary = new Color(0.70f, 0.76f, 0.84f, 1f),        // Soft Silver Slate
                    textMuted = new Color(0.45f, 0.50f, 0.58f, 1f),            // Low-contrast hint

                    btnNormalBg = new Color(0.18f, 0.23f, 0.30f, 1f),          // Dark Slate Button
                    btnNormalText = new Color(0.92f, 0.95f, 0.98f, 1f),        // White Button Text
                    btnAgeGrowBg = new Color(0.12f, 0.72f, 0.38f, 1f),         // Vibrant Emerald Grow
                    btnAgeGrowText = Color.white,

                    accentGold = new Color(1.00f, 0.82f, 0.25f, 1f),           // Glowing Celestial Gold
                    accentEmerald = new Color(0.25f, 0.85f, 0.50f, 1f),        // Neon Emerald
                    accentCobalt = new Color(0.28f, 0.65f, 1.00f, 1f),         // Cyan / Cobalt
                    accentPurple = new Color(0.70f, 0.45f, 1.00f, 1f),         // Ethereal Violet
                    accentCrimson = new Color(0.95f, 0.30f, 0.35f, 1f)         // Alert Crimson
                };
            }
        }
    }
}
