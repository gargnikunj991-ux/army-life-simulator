using System;
using UnityEngine;

namespace ArmyLife.Systems
{
    public enum GameDifficulty
    {
        Story,
        Realistic,
        Ironclad
    }

    public enum SimulationPacing
    {
        Standard,
        Fast
    }

    public enum TextFlowSpeed
    {
        Slow,
        Normal,
        Instant
    }

    /// <summary>
    /// Centralized gameplay settings engine.
    /// Provides dynamic difficulty scaling (Story, Realistic, Ironclad), pacing control,
    /// and accessible visual toggles across combat, promotion boards, taxation, and survival.
    /// </summary>
    public static class DifficultySettings
    {
        public const string PREF_DIFFICULTY = "ArmyLife_Difficulty";
        public const string PREF_PACING = "ArmyLife_Pacing";
        public const string PREF_SCREEN_SHAKE = "ArmyLife_ScreenShake";
        public const string PREF_RISK_CONFIRM = "ArmyLife_RiskConfirm";
        public const string PREF_TEXT_SPEED = "ArmyLife_TextSpeed";
        public const string PREF_HIGH_CONTRAST = "ArmyLife_HighContrast";

        public static GameDifficulty CurrentDifficulty
        {
            get
            {
                string s = PlayerPrefs.GetString(PREF_DIFFICULTY, "Realistic");
                if (Enum.TryParse<GameDifficulty>(s, true, out var diff)) return diff;
                return GameDifficulty.Realistic;
            }
            set
            {
                PlayerPrefs.SetString(PREF_DIFFICULTY, value.ToString());
                PlayerPrefs.Save();
            }
        }

        public static SimulationPacing CurrentPacing
        {
            get
            {
                string s = PlayerPrefs.GetString(PREF_PACING, "Standard");
                if (Enum.TryParse<SimulationPacing>(s, true, out var p)) return p;
                return SimulationPacing.Standard;
            }
            set
            {
                PlayerPrefs.SetString(PREF_PACING, value.ToString());
                PlayerPrefs.Save();
            }
        }

        public static TextFlowSpeed CurrentTextSpeed
        {
            get
            {
                int val = PlayerPrefs.GetInt(PREF_TEXT_SPEED, 1);
                return (TextFlowSpeed)Mathf.Clamp(val, 0, 2);
            }
            set
            {
                PlayerPrefs.SetInt(PREF_TEXT_SPEED, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static bool ScreenShakeEnabled
        {
            get => PlayerPrefs.GetInt(PREF_SCREEN_SHAKE, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(PREF_SCREEN_SHAKE, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool RiskConfirmationEnabled
        {
            get => PlayerPrefs.GetInt(PREF_RISK_CONFIRM, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(PREF_RISK_CONFIRM, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool HighContrastEnabled
        {
            get => PlayerPrefs.GetInt(PREF_HIGH_CONTRAST, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(PREF_HIGH_CONTRAST, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static float CombatMortalityMultiplier => CurrentDifficulty switch
        {
            GameDifficulty.Story => 0.55f,      // -45% combat damage and casualty rate
            GameDifficulty.Ironclad => 1.45f,   // +45% combat lethality
            _ => 1.0f                           // Realistic standard
        };

        public static float WarTaxRate => CurrentDifficulty switch
        {
            GameDifficulty.Story => 0.20f,      // 20% Wartime Tax
            GameDifficulty.Ironclad => 0.50f,   // 50% Imperial War Tax
            _ => 0.40f                          // 40% Standard War Tax
        };

        public static float PromotionPointsMultiplier => CurrentDifficulty switch
        {
            GameDifficulty.Story => 0.80f,      // -20% required promotion merit points
            GameDifficulty.Ironclad => 1.25f,   // +25% required promotion merit points
            _ => 1.0f
        };

        public static float PromotionFitnessModifier => CurrentDifficulty switch
        {
            GameDifficulty.Story => -5f,
            GameDifficulty.Ironclad => +5f,
            _ => 0f
        };

        public static bool AllowCasualtyRevival => CurrentDifficulty == GameDifficulty.Story;
        public static bool IsPermadeathForced => CurrentDifficulty == GameDifficulty.Ironclad;

        public static string GetDifficultyDescription(GameDifficulty diff) => diff switch
        {
            GameDifficulty.Story => "<b>Story Mode:</b> Low combat casualties, forgiving court-martials, 20% war tax, emergency CASEVAC revival.",
            GameDifficulty.Ironclad => "<color=#FF4444><b>Ironclad Mode:</b> Brutal frontline mortality, 50% war tax, strict promotion boards, permadeath enabled!</color>",
            _ => "<b>Realistic Mode:</b> Authentic military simulation, 40% war tax, competitive promotion boards, authentic casualty risks."
        };

        public static string GetPacingDescription(SimulationPacing pace) => pace switch
        {
            SimulationPacing.Fast => "<b>Fast-Forward:</b> Instant turn progression without simulation delays or prompt pauses.",
            _ => "<b>Standard Pacing:</b> Full historical log feeds, step-by-step decision intervals, and immersive battle alerts."
        };
    }
}
