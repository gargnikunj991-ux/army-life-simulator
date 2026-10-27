#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ArmyLife.Data;
using UnityEditor;
using UnityEngine;

namespace ArmyLife.Editor
{
    /// <summary>
    /// Editor utility that programmatically synthesizes, renders, and configures clean, crisp 128x128 PNG sprites
    /// for all enlisted chevrons, officer rank insignia, military branch crests, and ribbon decorations.
    /// </summary>
    public static class InsigniaSpriteGenerator
    {
        private const string INSIGNIA_FOLDER = "Assets/Sprites/Insignias";
        private const string CREST_FOLDER = "Assets/Sprites/Crests";
        private const string RIBBON_FOLDER = "Assets/Sprites/Ribbons";

        [MenuItem("Army Life/Generate All Insignias, Crests and Ribbons", false, 50)]
        public static int GenerateAllSprites()
        {
            Debug.Log("<color=#00D4FF>[InsigniaSpriteGenerator] Beginning programmatic sprite synthesis (128x128)...</color>");

            EnsureDirectory(INSIGNIA_FOLDER);
            EnsureDirectory(CREST_FOLDER);
            EnsureDirectory(RIBBON_FOLDER);

            int count = 0;

            // 1. GENERATE ENLISTED CHEVRONS (E-1 to E-9)
            count += GenerateEnlistedInsignias();

            // 2. GENERATE OFFICER RANKS (O-1 to O-10)
            count += GenerateOfficerInsignias();

            // 3. GENERATE BRANCH & SPECIAL FORCES CRESTS
            count += GenerateBranchCrests();

            // 4. GENERATE 12+ MEDAL RIBBON TEXTURES
            count += GenerateRibbonTextures();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FFAA>[InsigniaSpriteGenerator] Successfully synthesized and imported {count} military sprites!</color>");
            return count;
        }

        private static void EnsureDirectory(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        #region Enlisted Ranks (E-1 to E-9)
        private static int GenerateEnlistedInsignias()
        {
            int generated = 0;
            // E-1 Recruit (clean / subtle outline)
            SaveSprite(GenerateEnlistedChevron("E-1", 0, 0, false, false), Path.Combine(INSIGNIA_FOLDER, "E-1.png"));
            generated++;

            // E-2 Private (1 chevron)
            SaveSprite(GenerateEnlistedChevron("E-2", 1, 0, false, false), Path.Combine(INSIGNIA_FOLDER, "E-2.png"));
            generated++;

            // E-3 Private First Class (1 chevron + 1 rocker)
            SaveSprite(GenerateEnlistedChevron("E-3", 1, 1, false, false), Path.Combine(INSIGNIA_FOLDER, "E-3.png"));
            generated++;

            // E-4 Corporal (2 chevrons)
            SaveSprite(GenerateEnlistedChevron("E-4", 2, 0, false, false), Path.Combine(INSIGNIA_FOLDER, "E-4.png"));
            generated++;

            // E-5 Sergeant (3 chevrons)
            SaveSprite(GenerateEnlistedChevron("E-5", 3, 0, false, false), Path.Combine(INSIGNIA_FOLDER, "E-5.png"));
            generated++;

            // E-6 Staff Sgt (3 chevrons + 1 rocker)
            SaveSprite(GenerateEnlistedChevron("E-6", 3, 1, false, false), Path.Combine(INSIGNIA_FOLDER, "E-6.png"));
            generated++;

            // E-7 Sgt First Class (3 chevrons + 2 rockers)
            SaveSprite(GenerateEnlistedChevron("E-7", 3, 2, false, false), Path.Combine(INSIGNIA_FOLDER, "E-7.png"));
            generated++;

            // E-8 First Sgt (3 chevrons + 3 rockers + diamond lozenge)
            SaveSprite(GenerateEnlistedChevron("E-8", 3, 3, true, false), Path.Combine(INSIGNIA_FOLDER, "E-8.png"));
            generated++;

            // E-9 Sgt Major (3 chevrons + 3 rockers + star)
            SaveSprite(GenerateEnlistedChevron("E-9", 3, 3, false, true), Path.Combine(INSIGNIA_FOLDER, "E-9.png"));
            generated++;

            return generated;
        }

        private static Texture2D GenerateEnlistedChevron(string paygrade, int chevrons, int rockers, bool hasDiamond, bool hasStar)
        {
            var tex = CreateEmptyTexture();
            Color chevronGold = new Color(0.96f, 0.78f, 0.18f, 1f);
            Color chevronShadow = new Color(0.60f, 0.45f, 0.05f, 1f);
            Color patchBg = new Color(0.12f, 0.15f, 0.18f, 0.92f);
            Color patchBorder = new Color(0.25f, 0.30f, 0.35f, 0.8f);

            // Draw shield/patch backing
            DrawPatchBacking(tex, patchBg, patchBorder);

            if (paygrade == "E-1")
            {
                // Clean / recruit blank emblem with subtle imperial chevron imprint
                DrawCircle(tex, 64, 64, 18, new Color(0.35f, 0.42f, 0.48f, 0.6f), false);
                DrawCircle(tex, 64, 64, 10, new Color(0.35f, 0.42f, 0.48f, 0.4f), false);
                tex.Apply();
                return tex;
            }

            int topY = 92;
            int spacing = 12;

            // Draw upper chevrons pointing up
            for (int i = 0; i < chevrons; i++)
            {
                int cy = topY - (i * spacing);
                DrawChevronShape(tex, 64, cy, 76, 24, 7, chevronGold, chevronShadow);
            }

            // Draw lower rockers curving up
            int bottomY = 32;
            for (int r = 0; r < rockers; r++)
            {
                int ry = bottomY + (r * spacing);
                DrawRockerShape(tex, 64, ry, 74, 16, 6, chevronGold, chevronShadow);
            }

            // Center device
            if (hasDiamond)
            {
                DrawDiamond(tex, 64, 56, 18, 22, chevronGold);
                DrawDiamond(tex, 64, 56, 10, 14, chevronShadow);
            }
            else if (hasStar)
            {
                DrawStar(tex, 64, 56, 14, 6, chevronGold);
            }

            tex.Apply();
            return tex;
        }
        #endregion

        #region Officer Ranks (O-1 to O-10)
        private static int GenerateOfficerInsignias()
        {
            int generated = 0;
            // O-1 2nd Lieutenant (gold bar)
            SaveSprite(GenerateBarInsignia(new Color(0.96f, 0.78f, 0.18f, 1f), "O-1"), Path.Combine(INSIGNIA_FOLDER, "O-1.png"));
            generated++;

            // O-2 1st Lieutenant (silver bar)
            SaveSprite(GenerateBarInsignia(new Color(0.92f, 0.94f, 0.98f, 1f), "O-2"), Path.Combine(INSIGNIA_FOLDER, "O-2.png"));
            generated++;

            // O-3 Captain (double silver bars)
            SaveSprite(GenerateCaptainDoubleBars(), Path.Combine(INSIGNIA_FOLDER, "O-3.png"));
            generated++;

            // O-4 Major (gold oak leaf)
            SaveSprite(GenerateOakLeaf(new Color(0.96f, 0.78f, 0.18f, 1f)), Path.Combine(INSIGNIA_FOLDER, "O-4.png"));
            generated++;

            // O-5 Lt Colonel (silver oak leaf)
            SaveSprite(GenerateOakLeaf(new Color(0.92f, 0.94f, 0.98f, 1f)), Path.Combine(INSIGNIA_FOLDER, "O-5.png"));
            generated++;

            // O-6 Colonel (silver eagle)
            SaveSprite(GenerateColonelEagle(), Path.Combine(INSIGNIA_FOLDER, "O-6.png"));
            generated++;

            // O-7 Brig General (1 star)
            SaveSprite(GenerateGeneralStars(1), Path.Combine(INSIGNIA_FOLDER, "O-7.png"));
            generated++;

            // O-8 Major General (2 stars)
            SaveSprite(GenerateGeneralStars(2), Path.Combine(INSIGNIA_FOLDER, "O-8.png"));
            generated++;

            // O-9 Lt General (3 stars)
            SaveSprite(GenerateGeneralStars(3), Path.Combine(INSIGNIA_FOLDER, "O-9.png"));
            generated++;

            // O-10 General (4 stars)
            SaveSprite(GenerateGeneralStars(4), Path.Combine(INSIGNIA_FOLDER, "O-10.png"));
            generated++;

            return generated;
        }

        private static Texture2D GenerateBarInsignia(Color barColor, string label)
        {
            var tex = CreateEmptyTexture();
            DrawPatchBacking(tex, new Color(0.12f, 0.15f, 0.18f, 0.92f), new Color(0.3f, 0.35f, 0.4f, 0.8f));

            Color shadow = barColor * 0.7f;
            shadow.a = 1f;

            // Vertical officer bar centered: 24 wide, 72 tall
            DrawBeveledRect(tex, 64 - 14, 64 - 38, 28, 76, barColor, shadow);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateCaptainDoubleBars()
        {
            var tex = CreateEmptyTexture();
            DrawPatchBacking(tex, new Color(0.12f, 0.15f, 0.18f, 0.92f), new Color(0.3f, 0.35f, 0.4f, 0.8f));

            Color silver = new Color(0.92f, 0.94f, 0.98f, 1f);
            Color shadow = silver * 0.7f;
            shadow.a = 1f;

            // Two parallel vertical bars
            DrawBeveledRect(tex, 44 - 10, 64 - 36, 20, 72, silver, shadow);
            DrawBeveledRect(tex, 84 - 10, 64 - 36, 20, 72, silver, shadow);

            // Cross-bridges at top and bottom
            DrawRect(tex, 44, 64 - 28, 40, 8, silver);
            DrawRect(tex, 44, 64 + 20, 40, 8, silver);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateOakLeaf(Color leafColor)
        {
            var tex = CreateEmptyTexture();
            DrawPatchBacking(tex, new Color(0.12f, 0.15f, 0.18f, 0.92f), new Color(0.3f, 0.35f, 0.4f, 0.8f));

            Color shadow = leafColor * 0.65f;
            shadow.a = 1f;

            // Draw stylized symmetrical oak leaf cluster
            int cx = 64;
            int cy = 64;

            // Central vein / stem
            DrawLine(tex, cx, cy - 35, cx, cy + 32, shadow, 4);

            // Leaf lobes
            DrawCircle(tex, cx, cy + 24, 15, leafColor, true);
            DrawCircle(tex, cx - 18, cy + 12, 16, leafColor, true);
            DrawCircle(tex, cx + 18, cy + 12, 16, leafColor, true);

            DrawCircle(tex, cx - 22, cy - 8, 17, leafColor, true);
            DrawCircle(tex, cx + 22, cy - 8, 17, leafColor, true);

            DrawCircle(tex, cx - 14, cy - 24, 13, leafColor, true);
            DrawCircle(tex, cx + 14, cy - 24, 13, leafColor, true);
            DrawCircle(tex, cx, cy - 32, 11, leafColor, true);

            // Inner leaf rib highlights
            DrawLine(tex, cx, cy + 10, cx - 14, cy + 18, shadow, 2);
            DrawLine(tex, cx, cy + 10, cx + 14, cy + 18, shadow, 2);
            DrawLine(tex, cx, cy - 8, cx - 16, cy, shadow, 2);
            DrawLine(tex, cx, cy - 8, cx + 16, cy, shadow, 2);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateColonelEagle()
        {
            var tex = CreateEmptyTexture();
            DrawPatchBacking(tex, new Color(0.12f, 0.15f, 0.18f, 0.92f), new Color(0.3f, 0.35f, 0.4f, 0.8f));

            Color silver = new Color(0.92f, 0.95f, 0.98f, 1f);
            Color shadow = new Color(0.65f, 0.70f, 0.78f, 1f);

            int cx = 64;
            int cy = 64;

            // Spread wings
            for (int x = 20; x <= 108; x++)
            {
                float dx = Mathf.Abs(x - cx);
                int wingTop = (int)(cy + 16 + (dx * 0.42f));
                int wingBot = (int)(cy - 6 + (dx * 0.15f));
                for (int y = wingBot; y <= wingTop; y++)
                {
                    tex.SetPixel(x, y, silver);
                }
            }

            // Shield on eagle's chest
            DrawShield(tex, cx, cy - 6, 26, 32, new Color(0.15f, 0.22f, 0.32f, 1f), shadow);

            // Eagle Head turned right
            DrawCircle(tex, cx + 4, cy + 24, 11, silver, true);
            DrawPolygonTriangle(tex, cx + 12, cy + 24, cx + 22, cy + 22, cx + 12, cy + 19, new Color(0.96f, 0.78f, 0.18f, 1f));

            // Talons grasping arrows & olive branch
            DrawRect(tex, cx - 18, cy - 24, 36, 6, new Color(0.96f, 0.78f, 0.18f, 1f));

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateGeneralStars(int starCount)
        {
            var tex = CreateEmptyTexture();
            DrawPatchBacking(tex, new Color(0.12f, 0.15f, 0.18f, 0.92f), new Color(0.3f, 0.35f, 0.4f, 0.8f));

            Color silver = new Color(0.95f, 0.97f, 1f, 1f);

            switch (starCount)
            {
                case 1:
                    DrawStar(tex, 64, 64, 26, 11, silver);
                    break;
                case 2:
                    DrawStar(tex, 44, 64, 18, 8, silver);
                    DrawStar(tex, 84, 64, 18, 8, silver);
                    break;
                case 3:
                    DrawStar(tex, 32, 64, 15, 6, silver);
                    DrawStar(tex, 64, 64, 15, 6, silver);
                    DrawStar(tex, 96, 64, 15, 6, silver);
                    break;
                case 4:
                    DrawStar(tex, 24, 64, 13, 5, silver);
                    DrawStar(tex, 51, 64, 13, 5, silver);
                    DrawStar(tex, 77, 64, 13, 5, silver);
                    DrawStar(tex, 104, 64, 13, 5, silver);
                    break;
            }

            tex.Apply();
            return tex;
        }
        #endregion

        #region Branch Crests
        private static int GenerateBranchCrests()
        {
            int count = 0;

            // 1. Imperial Army Crest (crossed swords & shield)
            var armyCrest = GenerateArmyCrest();
            SaveSprite(armyCrest, Path.Combine(CREST_FOLDER, "Imperial_Army_Crest.png"));
            SaveSprite(armyCrest, Path.Combine(CREST_FOLDER, "Army_Crest.png"));
            count++;

            // 2. Imperial Air Force Wings (aerospace wings & prop star)
            var afWings = GenerateAirForceWings();
            SaveSprite(afWings, Path.Combine(CREST_FOLDER, "Imperial_Air_Force_Wings.png"));
            SaveSprite(afWings, Path.Combine(CREST_FOLDER, "Air_Force_Wings.png"));
            count++;

            // 3. Special Forces Crest (Green Beret dagger & lightning bolts)
            var sfCrest = GenerateSpecialForcesCrest();
            SaveSprite(sfCrest, Path.Combine(CREST_FOLDER, "Special_Forces_Crest.png"));
            count++;

            return count;
        }

        private static Texture2D GenerateArmyCrest()
        {
            var tex = CreateEmptyTexture();
            DrawCircularBacking(tex, new Color(0.10f, 0.14f, 0.18f, 0.95f), new Color(0.85f, 0.70f, 0.22f, 1f));

            Color swordSilver = new Color(0.92f, 0.94f, 0.97f, 1f);
            Color swordHilt = new Color(0.85f, 0.70f, 0.22f, 1f);

            // Crossed swords diagonally
            DrawLine(tex, 28, 28, 100, 100, swordSilver, 5);
            DrawLine(tex, 100, 28, 28, 100, swordSilver, 5);

            // Hilts & guards
            DrawLine(tex, 22, 34, 34, 22, swordHilt, 6);
            DrawLine(tex, 94, 22, 106, 34, swordHilt, 6);
            DrawCircle(tex, 25, 25, 5, swordHilt, true);
            DrawCircle(tex, 103, 25, 5, swordHilt, true);

            // Central Imperial Guard Shield
            DrawShield(tex, 64, 60, 48, 56, new Color(0.65f, 0.10f, 0.15f, 1f), new Color(0.96f, 0.80f, 0.25f, 1f));

            // Imperial Star / Emblem on shield
            DrawStar(tex, 64, 62, 14, 6, new Color(0.96f, 0.85f, 0.25f, 1f));

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateAirForceWings()
        {
            var tex = CreateEmptyTexture();
            DrawCircularBacking(tex, new Color(0.08f, 0.12f, 0.18f, 0.95f), new Color(0.35f, 0.65f, 0.95f, 1f));

            Color wingSilver = new Color(0.90f, 0.94f, 0.98f, 1f);
            Color goldProp = new Color(0.96f, 0.80f, 0.22f, 1f);

            // Swept wings extending left & right
            int cx = 64;
            int cy = 64;

            for (int x = 14; x <= 114; x++)
            {
                float dx = Mathf.Abs(x - cx);
                if (dx > 8)
                {
                    int topY = (int)(cy + 8 + (Mathf.Sin(dx * 0.05f) * 16));
                    int botY = (int)(cy - 8 + (dx * 0.1f));
                    for (int y = botY; y <= topY; y++)
                    {
                        tex.SetPixel(x, y, wingSilver);
                    }
                }
            }

            // Central circle / prop hub
            DrawCircle(tex, cx, cy, 22, new Color(0.12f, 0.22f, 0.38f, 1f), true);
            DrawCircle(tex, cx, cy, 22, goldProp, false);

            // Propeller blades
            DrawPolygonTriangle(tex, cx - 4, cy, cx + 4, cy, cx, cy + 28, goldProp);
            DrawPolygonTriangle(tex, cx - 4, cy, cx + 4, cy, cx, cy - 28, goldProp);

            // Central 5-pointed star
            DrawStar(tex, cx, cy, 14, 6, new Color(1f, 0.95f, 0.4f, 1f));

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateSpecialForcesCrest()
        {
            var tex = CreateEmptyTexture();
            DrawCircularBacking(tex, new Color(0.08f, 0.16f, 0.12f, 0.95f), new Color(0.20f, 0.85f, 0.50f, 1f));

            Color lightningGold = new Color(1f, 0.85f, 0.20f, 1f);
            Color daggerSteel = new Color(0.92f, 0.94f, 0.98f, 1f);
            Color daggerHandle = new Color(0.25f, 0.30f, 0.35f, 1f);

            int cx = 64;
            int cy = 64;

            // Two crossed lightning bolts
            DrawLine(tex, 28, 92, 60, 60, lightningGold, 4);
            DrawLine(tex, 56, 64, 48, 56, lightningGold, 4);
            DrawLine(tex, 52, 60, 100, 32, lightningGold, 4);

            DrawLine(tex, 100, 92, 68, 60, lightningGold, 4);
            DrawLine(tex, 72, 64, 80, 56, lightningGold, 4);
            DrawLine(tex, 76, 60, 28, 32, lightningGold, 4);

            // Vertical Fairbairn-Sykes Commando Dagger in center
            // Blade (pointing up)
            DrawPolygonTriangle(tex, cx - 5, cy - 6, cx + 5, cy - 6, cx, cy + 42, daggerSteel);
            DrawLine(tex, cx, cy - 6, cx, cy + 40, daggerSteel * 0.7f, 2);

            // Crossguard
            DrawBeveledRect(tex, cx - 18, cy - 10, 36, 6, new Color(0.85f, 0.70f, 0.22f, 1f), new Color(0.5f, 0.4f, 0.1f, 1f));

            // Grip / Handle
            DrawBeveledRect(tex, cx - 4, cy - 36, 8, 26, daggerHandle, daggerHandle * 0.6f);

            // Pommel
            DrawCircle(tex, cx, cy - 38, 5, new Color(0.85f, 0.70f, 0.22f, 1f), true);

            tex.Apply();
            return tex;
        }
        #endregion

        #region Ribbons
        private static int GenerateRibbonTextures()
        {
            var medals = MedalRegistry.GetAllOfficialMedals();
            int count = 0;

            foreach (var def in medals)
            {
                var tex = GenerateSingleRibbon(def);
                string path = Path.Combine(RIBBON_FOLDER, $"{def.medalId}.png");
                SaveSprite(tex, path);

                // Also save with friendly filename alias if different
                string altPath = Path.Combine(RIBBON_FOLDER, $"Ribbon_{def.medalId}.png");
                if (!File.Exists(altPath))
                {
                    SaveSprite(tex, altPath);
                }
                count++;
            }

            return count;
        }

        private static Texture2D GenerateSingleRibbon(MedalRibbonDefinition def)
        {
            var tex = CreateEmptyTexture();
            Color[] colors = def.GetColors();

            // Ribbon aspect: Centered horizontal bar, 114 wide, 40 tall
            int rx = 7;
            int ry = 44;
            int rw = 114;
            int rh = 40;

            Color goldBorder = new Color(0.85f, 0.72f, 0.28f, 1f);
            Color shadowBorder = new Color(0.40f, 0.32f, 0.10f, 1f);

            // Outer gold mounting frame
            DrawBeveledRect(tex, rx - 2, ry - 2, rw + 4, rh + 4, goldBorder, shadowBorder);

            // Vertical stripes inside
            for (int x = 0; x < rw; x++)
            {
                int colorIdx = Mathf.FloorToInt((float)x / rw * colors.Length);
                colorIdx = Mathf.Clamp(colorIdx, 0, colors.Length - 1);
                Color c = colors[colorIdx];

                for (int y = 0; y < rh; y++)
                {
                    // Subtle fabric texture gradient
                    float grain = 0.95f + (Mathf.Sin(y * 1.5f) * 0.05f);
                    Color pixelColor = new Color(c.r * grain, c.g * grain, c.b * grain, 1f);
                    tex.SetPixel(rx + x, ry + y, pixelColor);
                }
            }

            // Draw center appurtenance device icon if applicable
            int cx = 64;
            int cy = 64;
            Color deviceGold = new Color(1f, 0.90f, 0.35f, 1f);
            Color deviceSilver = new Color(0.95f, 0.96f, 0.98f, 1f);

            if (def.deviceIcon == "★")
            {
                Color starCol = def.category == MedalCategory.ImperialValor ? deviceGold : deviceSilver;
                DrawStar(tex, cx, cy, 10, 4, starCol);
            }
            else if (def.deviceIcon == "V")
            {
                DrawLetterV(tex, cx, cy, 12, deviceGold);
            }
            else if (def.deviceIcon == "⚔")
            {
                DrawMiniCrossedSwords(tex, cx, cy, 12, deviceSilver);
            }
            else if (def.deviceIcon == "✚")
            {
                DrawCross(tex, cx, cy, 12, new Color(0.85f, 0.15f, 0.15f, 1f));
            }
            else if (def.deviceIcon == "🛡")
            {
                DrawShield(tex, cx, cy, 14, 16, new Color(0.2f, 0.25f, 0.3f, 1f), deviceGold);
            }

            tex.Apply();
            return tex;
        }
        #endregion

        #region Texture Importer & Disk Saving
        private static void SaveSprite(Texture2D tex, string relativePath)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath).Replace('\\', '/');
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngData);

            // AssetDatabase configure TextureImporter
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
        }
        #endregion

        #region Geometric Drawing Primitives
        private static Texture2D CreateEmptyTexture()
        {
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }
            return tex;
        }

        private static void DrawPatchBacking(Texture2D tex, Color bg, Color border)
        {
            int cx = 64;
            int cy = 64;
            int rx = 52;
            int ry = 58;

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float nx = (x - cx) / (float)rx;
                    float ny = (y - cy) / (float)ry;
                    float dist = (nx * nx) + (ny * ny);

                    if (dist <= 1.0f)
                    {
                        if (dist >= 0.88f) tex.SetPixel(x, y, border);
                        else tex.SetPixel(x, y, bg);
                    }
                }
            }
        }

        private static void DrawCircularBacking(Texture2D tex, Color bg, Color border)
        {
            int cx = 64;
            int cy = 64;
            int r = 56;

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= r)
                    {
                        if (dist >= r - 4) tex.SetPixel(x, y, border);
                        else tex.SetPixel(x, y, bg);
                    }
                }
            }
        }

        private static void DrawChevronShape(Texture2D tex, int cx, int cy, int width, int height, int thickness, Color col, Color shadow)
        {
            int halfW = width / 2;
            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                float frac = 1f - (Mathf.Abs(x - cx) / (float)halfW);
                int yApex = cy + (int)(frac * height);

                for (int t = 0; t < thickness; t++)
                {
                    int py = yApex - t;
                    if (py >= 0 && py < 128 && x >= 0 && x < 128)
                    {
                        Color c = (t == thickness - 1) ? shadow : col;
                        tex.SetPixel(x, py, c);
                    }
                }
            }
        }

        private static void DrawRockerShape(Texture2D tex, int cx, int cy, int width, int height, int thickness, Color col, Color shadow)
        {
            int halfW = width / 2;
            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                float frac = Mathf.Abs(x - cx) / (float)halfW;
                int yCurve = cy + (int)(frac * frac * height);

                for (int t = 0; t < thickness; t++)
                {
                    int py = yCurve - t;
                    if (py >= 0 && py < 128 && x >= 0 && x < 128)
                    {
                        Color c = (t == thickness - 1) ? shadow : col;
                        tex.SetPixel(x, py, c);
                    }
                }
            }
        }

        private static void DrawDiamond(Texture2D tex, int cx, int cy, int w, int h, Color col)
        {
            for (int y = cy - h; y <= cy + h; y++)
            {
                for (int x = cx - w; x <= cx + w; x++)
                {
                    float dx = Mathf.Abs(x - cx) / (float)w;
                    float dy = Mathf.Abs(y - cy) / (float)h;
                    if (dx + dy <= 1.0f)
                    {
                        tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static void DrawStar(Texture2D tex, int cx, int cy, int outerR, int innerR, Color col)
        {
            // 5 points
            for (int y = cy - outerR; y <= cy + outerR; y++)
            {
                for (int x = cx - outerR; x <= cx + outerR; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > outerR) continue;

                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;

                    // Point at 90 deg (top)
                    float shifted = (angle + 90f + 36f) % 72f;
                    float t = Mathf.Abs(shifted - 36f) / 36f;
                    float limit = Mathf.Lerp(innerR, outerR, t);

                    if (dist <= limit)
                    {
                        tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static void DrawShield(Texture2D tex, int cx, int cy, int w, int h, Color fill, Color border)
        {
            int halfW = w / 2;
            int halfH = h / 2;

            for (int y = cy - halfH; y <= cy + halfH; y++)
            {
                for (int x = cx - halfW; x <= cx + halfW; x++)
                {
                    float nx = Mathf.Abs(x - cx) / (float)halfW;
                    float ny = (y - (cy - halfH)) / (float)h;

                    // Shield curve formula
                    float limit = (ny > 0.4f) ? 1.0f - ((1f - ny) * (1f - ny) * 0.8f) : 1.0f;
                    if (nx <= limit)
                    {
                        if (nx >= limit - 0.15f || y >= cy + halfH - 3 || y <= cy - halfH + 3)
                            tex.SetPixel(x, y, border);
                        else
                            tex.SetPixel(x, y, fill);
                    }
                }
            }
        }

        private static void DrawBeveledRect(Texture2D tex, int x, int y, int w, int h, Color fill, Color shadow)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    if (px == x || py == y + h - 1)
                        tex.SetPixel(px, py, fill * 1.15f);
                    else if (px == x + w - 1 || py == y)
                        tex.SetPixel(px, py, shadow);
                    else
                        tex.SetPixel(px, py, fill);
                }
            }
        }

        private static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color col)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    if (px >= 0 && px < 128 && py >= 0 && py < 128)
                        tex.SetPixel(px, py, col);
                }
            }
        }

        private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col, int thickness)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int halfT = thickness / 2;

            while (true)
            {
                for (int tx = -halfT; tx <= halfT; tx++)
                {
                    for (int ty = -halfT; ty <= halfT; ty++)
                    {
                        int px = x0 + tx;
                        int py = y0 + ty;
                        if (px >= 0 && px < 128 && py >= 0 && py < 128)
                        {
                            tex.SetPixel(px, py, col);
                        }
                    }
                }

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color col, bool fill)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (fill)
                    {
                        if (dist <= radius && x >= 0 && x < 128 && y >= 0 && y < 128)
                            tex.SetPixel(x, y, col);
                    }
                    else
                    {
                        if (dist <= radius && dist >= radius - 2 && x >= 0 && x < 128 && y >= 0 && y < 128)
                            tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static void DrawPolygonTriangle(Texture2D tex, int x0, int y0, int x1, int y1, int x2, int y2, Color col)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsPointInTriangle(x, y, x0, y0, x1, y1, x2, y2))
                    {
                        if (x >= 0 && x < 128 && y >= 0 && y < 128)
                            tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static bool IsPointInTriangle(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            float d1 = Sign(px, py, x0, y0, x1, y1);
            float d2 = Sign(px, py, x1, y1, x2, y2);
            float d3 = Sign(px, py, x2, y2, x0, y0);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        private static float Sign(int p1x, int p1y, int p2x, int p2y, int p3x, int p3y)
        {
            return (p1x - p3x) * (p2y - p3y) - (p2x - p3x) * (p1y - p3y);
        }

        private static void DrawLetterV(Texture2D tex, int cx, int cy, int size, Color col)
        {
            int half = size / 2;
            DrawLine(tex, cx - half, cy + half, cx, cy - half, col, 3);
            DrawLine(tex, cx, cy - half, cx + half, cy + half, col, 3);
        }

        private static void DrawCross(Texture2D tex, int cx, int cy, int size, Color col)
        {
            int half = size / 2;
            DrawLine(tex, cx - half, cy, cx + half, cy, col, 4);
            DrawLine(tex, cx, cy - half, cx, cy + half, col, 4);
        }

        private static void DrawMiniCrossedSwords(Texture2D tex, int cx, int cy, int size, Color col)
        {
            int half = size / 2;
            DrawLine(tex, cx - half, cy - half, cx + half, cy + half, col, 2);
            DrawLine(tex, cx - half, cy + half, cx + half, cy - half, col, 2);
        }
        #endregion
    }
}
#endif
