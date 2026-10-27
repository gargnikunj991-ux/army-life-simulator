using System;
using System.Collections.Generic;
using System.IO;
using ArmyLife.Data;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArmyLife.Systems
{
    /// <summary>
    /// Central runtime manager and cache for military rank insignias, branch crests, and medal ribbons.
    /// Supports automatic disk lookup, AssetDatabase resolution in Editor, and runtime procedural synthesis.
    /// </summary>
    public static class InsigniaSpriteManager
    {
        private static readonly Dictionary<string, Sprite> _rankInsigniaCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> _crestCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> _ribbonCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        private const string INSIGNIA_DIR = "Assets/Sprites/Insignias";
        private const string CREST_DIR = "Assets/Sprites/Crests";
        private const string RIBBON_DIR = "Assets/Sprites/Ribbons";

        /// <summary>
        /// Retrieves the crisp 128x128 rank insignia sprite for the given paygrade (e.g. "E-1", "E-5", "O-1", "O-10").
        /// </summary>
        public static Sprite GetRankInsignia(string paygrade)
        {
            if (string.IsNullOrEmpty(paygrade)) paygrade = "E-1";
            string clean = paygrade.Trim().ToUpper();

            if (_rankInsigniaCache.TryGetValue(clean, out var cached) && cached != null)
                return cached;

            Sprite loaded = LoadSpriteFromDisk(Path.Combine(INSIGNIA_DIR, $"{clean}.png"));
            if (loaded == null)
            {
                loaded = GenerateFallbackRankSprite(clean);
            }

            if (loaded != null)
            {
                _rankInsigniaCache[clean] = loaded;
            }
            return loaded;
        }

        /// <summary>
        /// Retrieves the military branch crest or special forces insignia sprite.
        /// </summary>
        public static Sprite GetBranchCrest(MilitaryBranch branch, bool isSpecialForces = false)
        {
            string key = isSpecialForces ? "Special_Forces_Crest" :
                (branch == MilitaryBranch.AirForce ? "Imperial_Air_Force_Wings" : "Imperial_Army_Crest");

            if (_crestCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            Sprite loaded = LoadSpriteFromDisk(Path.Combine(CREST_DIR, $"{key}.png"));
            if (loaded == null)
            {
                loaded = GenerateFallbackCrestSprite(key);
            }

            if (loaded != null)
            {
                _crestCache[key] = loaded;
            }
            return loaded;
        }

        /// <summary>
        /// Retrieves the 128x128 medal ribbon texture sprite for the specified medal ID or display name.
        /// </summary>
        public static Sprite GetMedalRibbon(string medalId)
        {
            if (string.IsNullOrEmpty(medalId)) return null;

            var def = MedalRegistry.GetDefinition(medalId);
            string id = def != null ? def.medalId : medalId.Replace(" ", "_").ToUpper();

            if (_ribbonCache.TryGetValue(id, out var cached) && cached != null)
                return cached;

            Sprite loaded = LoadSpriteFromDisk(Path.Combine(RIBBON_DIR, $"{id}.png"));
            if (loaded == null)
            {
                loaded = GenerateFallbackRibbonSprite(def);
            }

            if (loaded != null)
            {
                _ribbonCache[id] = loaded;
                if (def != null && !string.IsNullOrEmpty(def.displayName))
                {
                    _ribbonCache[def.displayName] = loaded;
                }
            }
            return loaded;
        }

        public static void ClearCache()
        {
            _rankInsigniaCache.Clear();
            _crestCache.Clear();
            _ribbonCache.Clear();
        }

        private static Sprite LoadSpriteFromDisk(string assetPath)
        {
            string normalizedPath = assetPath.Replace('\\', '/');

#if UNITY_EDITOR
            Sprite editorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(normalizedPath);
            if (editorSprite != null) return editorSprite;
#endif

            // Fallback: Read PNG bytes directly from project disk
            if (File.Exists(normalizedPath))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(normalizedPath);
                    Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(bytes))
                    {
                        tex.filterMode = FilterMode.Bilinear;
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[InsigniaSpriteManager] Failed reading sprite from {normalizedPath}: {ex.Message}");
                }
            }
            return null;
        }

        private static Sprite GenerateFallbackRankSprite(string paygrade)
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Color darkBg = new Color(0.12f, 0.14f, 0.16f, 0.95f);
            Color border = new Color(0.85f, 0.70f, 0.25f, 1f);

            // Fill background circle
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 64;
                    float dy = y - 64;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= 56)
                    {
                        if (dist >= 52) tex.SetPixel(x, y, border);
                        else tex.SetPixel(x, y, darkBg);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            // Draw distinctive indicator for paygrade
            Color gold = new Color(1f, 0.85f, 0.2f, 1f);
            Color silver = new Color(0.88f, 0.90f, 0.95f, 1f);
            Color markCol = paygrade.StartsWith("O") ? gold : silver;

            for (int y = 54; y <= 74; y++)
            {
                for (int x = 44; x <= 84; x++)
                {
                    tex.SetPixel(x, y, markCol);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        }

        private static Sprite GenerateFallbackCrestSprite(string crestKey)
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Color crestColor = crestKey.Contains("Wings") ? new Color(0.3f, 0.65f, 0.95f, 1f) :
                               (crestKey.Contains("Special") ? new Color(0.2f, 0.85f, 0.5f, 1f) : new Color(0.9f, 0.75f, 0.2f, 1f));

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 64;
                    float dy = y - 64;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= 54)
                    {
                        tex.SetPixel(x, y, (dist >= 50) ? crestColor : new Color(0.1f, 0.12f, 0.15f, 0.9f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        }

        private static Sprite GenerateFallbackRibbonSprite(MedalRibbonDefinition def)
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Color[] colors = def != null ? def.GetColors() : new Color[] { Color.gray };

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    if (y >= 44 && y <= 84 && x >= 10 && x <= 118)
                    {
                        int stripeIdx = Mathf.FloorToInt((float)(x - 10) / 108f * colors.Length);
                        stripeIdx = Mathf.Clamp(stripeIdx, 0, colors.Length - 1);
                        tex.SetPixel(x, y, colors[stripeIdx]);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        }
    }
}
