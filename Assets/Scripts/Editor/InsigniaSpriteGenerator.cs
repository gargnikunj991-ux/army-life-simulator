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
    /// Premium military insignia, branch crest, and medal ribbon generator.
    /// Synthesizes high-resolution 256x256 anti-aliased vector-style sprites with professional
    /// metallic beveling, embroidery borders, heraldic geometry, and ribbon cloth texture.
    /// Eliminates all crude procedural/AI-style artifacts.
    /// </summary>
    public static class InsigniaSpriteGenerator
    {
        private const string INSIGNIA_FOLDER = "Assets/Sprites/Insignias";
        private const string CREST_FOLDER = "Assets/Sprites/Crests";
        private const string RIBBON_FOLDER = "Assets/Sprites/Ribbons";
        private const int TEX_SIZE = 256;

        [MenuItem("Army Life/Generate All Insignias, Crests and Ribbons", false, 50)]
        public static int GenerateAllSprites()
        {
            Debug.Log("<color=#D4AF37>[InsigniaSpriteGenerator] Synthesizing 256x256 High-Resolution Military Graphics...</color>");

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

            // 4. GENERATE MEDAL RIBBONS
            count += GenerateRibbonTextures();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#38A65E>[InsigniaSpriteGenerator] Successfully rendered and imported {count} military insignia assets!</color>");
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
            // E-1 Private Recruit: Clean embroidered backing patch with subtle tactical chevron outline
            SaveSprite(RenderEnlistedPatch(0, 0, false, false, "E-1"), Path.Combine(INSIGNIA_FOLDER, "E-1.png"));
            generated++;

            // E-2 Private: 1 Chevron
            SaveSprite(RenderEnlistedPatch(1, 0, false, false, "E-2"), Path.Combine(INSIGNIA_FOLDER, "E-2.png"));
            generated++;

            // E-3 Private First Class: 1 Chevron + 1 Rocker
            SaveSprite(RenderEnlistedPatch(1, 1, false, false, "E-3"), Path.Combine(INSIGNIA_FOLDER, "E-3.png"));
            generated++;

            // E-4 Corporal: 2 Chevrons
            SaveSprite(RenderEnlistedPatch(2, 0, false, false, "E-4"), Path.Combine(INSIGNIA_FOLDER, "E-4.png"));
            generated++;

            // E-5 Sergeant: 3 Chevrons
            SaveSprite(RenderEnlistedPatch(3, 0, false, false, "E-5"), Path.Combine(INSIGNIA_FOLDER, "E-5.png"));
            generated++;

            // E-6 Staff Sergeant: 3 Chevrons + 1 Rocker
            SaveSprite(RenderEnlistedPatch(3, 1, false, false, "E-6"), Path.Combine(INSIGNIA_FOLDER, "E-6.png"));
            generated++;

            // E-7 Sergeant First Class: 3 Chevrons + 2 Rockers
            SaveSprite(RenderEnlistedPatch(3, 2, false, false, "E-7"), Path.Combine(INSIGNIA_FOLDER, "E-7.png"));
            generated++;

            // E-8 Master Sergeant / First Sergeant: 3 Chevrons + 3 Rockers + Diamond
            SaveSprite(RenderEnlistedPatch(3, 3, true, false, "E-8"), Path.Combine(INSIGNIA_FOLDER, "E-8.png"));
            generated++;

            // E-9 Command Sergeant Major: 3 Chevrons + 3 Rockers + Star
            SaveSprite(RenderEnlistedPatch(3, 3, false, true, "E-9"), Path.Combine(INSIGNIA_FOLDER, "E-9.png"));
            generated++;

            return generated;
        }

        private static Texture2D RenderEnlistedPatch(int chevrons, int rockers, bool diamond, bool star, string paygrade)
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;

            Color patchFill = new Color(0.09f, 0.12f, 0.15f, 0.96f);
            Color patchBorder = new Color(0.24f, 0.30f, 0.38f, 1f);
            Color goldTop = new Color(0.96f, 0.82f, 0.32f, 1f);
            Color goldBase = new Color(0.78f, 0.60f, 0.16f, 1f);

            // Render backing tactical chevron shield
            DrawTacticalShield(tex, cx, cy, 100, 115, patchFill, patchBorder);

            if (chevrons == 0)
            {
                // Subtle recruit stencil mark
                DrawSmoothCircle(tex, cx, cy, 14, new Color(0.35f, 0.42f, 0.52f, 0.6f), true);
                DrawSmoothCircle(tex, cx, cy, 8, patchFill, true);
                tex.Apply();
                return tex;
            }

            // Draw Top Chevrons (pointing upward)
            int startY = 175;
            int chevronSpacing = 26;
            for (int i = 0; i < chevrons; i++)
            {
                int yPos = startY - (i * chevronSpacing);
                DrawSmoothChevron(tex, cx, yPos, 68, 12, goldTop, goldBase);
            }

            // Draw Bottom Rockers (curving downward)
            int rockerStartY = 65;
            int rockerSpacing = 20;
            for (int i = 0; i < rockers; i++)
            {
                int yPos = rockerStartY + (i * rockerSpacing);
                DrawSmoothRocker(tex, cx, yPos, 70, 11, goldTop, goldBase);
            }

            // Center insignia for E-8 (Diamond) and E-9 (Star)
            if (diamond)
            {
                DrawSmoothDiamond(tex, cx, 114, 16, 22, goldTop);
            }
            else if (star)
            {
                DrawSmoothStar(tex, cx, 114, 18, 8, goldTop);
            }

            tex.Apply();
            return tex;
        }
        #endregion

        #region Officer Ranks (O-1 to O-10)
        private static int GenerateOfficerInsignias()
        {
            int count = 0;

            Color goldLight = new Color(0.96f, 0.82f, 0.28f, 1f);
            Color goldDark = new Color(0.75f, 0.55f, 0.14f, 1f);

            Color silverLight = new Color(0.96f, 0.98f, 1f, 1f);
            Color silverDark = new Color(0.72f, 0.78f, 0.86f, 1f);

            // O-1: 2nd Lieutenant (Gold Bar)
            SaveSprite(RenderOfficerBar(goldLight, goldDark), Path.Combine(INSIGNIA_FOLDER, "O-1.png"));
            count++;

            // O-2: 1st Lieutenant (Silver Bar)
            SaveSprite(RenderOfficerBar(silverLight, silverDark), Path.Combine(INSIGNIA_FOLDER, "O-2.png"));
            count++;

            // O-3: Captain (Two Parallel Silver Bars)
            SaveSprite(RenderCaptainBars(silverLight, silverDark), Path.Combine(INSIGNIA_FOLDER, "O-3.png"));
            count++;

            // O-4: Major (Gold Oak Leaf)
            SaveSprite(RenderOakLeaf(goldLight, goldDark), Path.Combine(INSIGNIA_FOLDER, "O-4.png"));
            count++;

            // O-5: Lieutenant Colonel (Silver Oak Leaf)
            SaveSprite(RenderOakLeaf(silverLight, silverDark), Path.Combine(INSIGNIA_FOLDER, "O-5.png"));
            count++;

            // O-6: Colonel (Silver Eagle)
            SaveSprite(RenderColonelEagle(silverLight, silverDark), Path.Combine(INSIGNIA_FOLDER, "O-6.png"));
            count++;

            // O-7 to O-10: General Officers (1 to 4 Stars)
            SaveSprite(RenderGeneralStars(1, silverLight), Path.Combine(INSIGNIA_FOLDER, "O-7.png")); count++;
            SaveSprite(RenderGeneralStars(2, silverLight), Path.Combine(INSIGNIA_FOLDER, "O-8.png")); count++;
            SaveSprite(RenderGeneralStars(3, silverLight), Path.Combine(INSIGNIA_FOLDER, "O-9.png")); count++;
            SaveSprite(RenderGeneralStars(4, silverLight), Path.Combine(INSIGNIA_FOLDER, "O-10.png")); count++;

            return count;
        }

        private static Texture2D RenderOfficerBar(Color lightCol, Color darkCol)
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;
            DrawTacticalShield(tex, cx, cy, 95, 110, new Color(0.09f, 0.12f, 0.15f, 0.96f), new Color(0.24f, 0.30f, 0.38f, 1f));

            // Single vertical beveled metallic bar
            DrawBeveledRect(tex, cx - 22, cy - 64, 44, 128, lightCol, darkCol, 5);
            tex.Apply();
            return tex;
        }

        private static Texture2D RenderCaptainBars(Color lightCol, Color darkCol)
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;
            DrawTacticalShield(tex, cx, cy, 95, 110, new Color(0.09f, 0.12f, 0.15f, 0.96f), new Color(0.24f, 0.30f, 0.38f, 1f));

            // Two parallel bars
            DrawBeveledRect(tex, cx - 44, cy - 60, 30, 120, lightCol, darkCol, 4);
            DrawBeveledRect(tex, cx + 14, cy - 60, 30, 120, lightCol, darkCol, 4);
            // Connecting crosspieces
            DrawBeveledRect(tex, cx - 18, cy + 24, 36, 14, lightCol, darkCol, 2);
            DrawBeveledRect(tex, cx - 18, cy - 38, 36, 14, lightCol, darkCol, 2);

            tex.Apply();
            return tex;
        }

        private static Texture2D RenderOakLeaf(Color lightCol, Color darkCol)
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;
            DrawTacticalShield(tex, cx, cy, 95, 110, new Color(0.09f, 0.12f, 0.15f, 0.96f), new Color(0.24f, 0.30f, 0.38f, 1f));

            // Center stem
            DrawBeveledRect(tex, cx - 5, cy - 70, 10, 140, lightCol, darkCol, 2);

            // Leaf lobes
            for (int dy = -40; dy <= 45; dy += 22)
            {
                int lobeW = (int)(Mathf.Cos(dy / 60f) * 44f);
                DrawSmoothCircle(tex, cx - lobeW, cy + dy, 20, lightCol, true);
                DrawSmoothCircle(tex, cx + lobeW, cy + dy, 20, lightCol, true);
            }
            // Top tip
            DrawSmoothCircle(tex, cx, cy + 62, 16, lightCol, true);

            tex.Apply();
            return tex;
        }

        private static Texture2D RenderColonelEagle(Color lightCol, Color darkCol)
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;
            DrawTacticalShield(tex, cx, cy, 95, 110, new Color(0.09f, 0.12f, 0.15f, 0.96f), new Color(0.24f, 0.30f, 0.38f, 1f));

            // Eagle chest shield
            DrawTacticalShield(tex, cx, cy - 10, 32, 42, lightCol * 0.9f, darkCol);

            // Left swept wing
            for (int i = 0; i < 7; i++)
            {
                int x0 = cx - 18;
                int y0 = cy - 5 + (i * 8);
                int x1 = cx - 75 + (i * 5);
                int y1 = cy + 25 + (i * 7);
                DrawSmoothLine(tex, x0, y0, x1, y1, 7, lightCol);
            }

            // Right swept wing
            for (int i = 0; i < 7; i++)
            {
                int x0 = cx + 18;
                int y0 = cy - 5 + (i * 8);
                int x1 = cx + 75 - (i * 5);
                int y1 = cy + 25 + (i * 7);
                DrawSmoothLine(tex, x0, y0, x1, y1, 7, lightCol);
            }

            // Head & Beak
            DrawSmoothCircle(tex, cx, cy + 42, 14, lightCol, true);
            DrawSmoothTriangle(tex, cx + 8, cy + 45, cx + 22, cy + 40, cx + 8, cy + 35, goldTopCol);

            tex.Apply();
            return tex;
        }
        private static readonly Color goldTopCol = new Color(0.96f, 0.82f, 0.28f, 1f);

        private static Texture2D RenderGeneralStars(int count, Color starColor)
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;
            DrawTacticalShield(tex, cx, cy, 95, 110, new Color(0.09f, 0.12f, 0.15f, 0.96f), new Color(0.24f, 0.30f, 0.38f, 1f));

            switch (count)
            {
                case 1:
                    DrawSmoothStar(tex, cx, cy, 38, 16, starColor);
                    break;
                case 2:
                    DrawSmoothStar(tex, cx - 42, cy, 30, 13, starColor);
                    DrawSmoothStar(tex, cx + 42, cy, 30, 13, starColor);
                    break;
                case 3:
                    DrawSmoothStar(tex, cx - 58, cy, 26, 11, starColor);
                    DrawSmoothStar(tex, cx, cy + 15, 28, 12, starColor);
                    DrawSmoothStar(tex, cx + 58, cy, 26, 11, starColor);
                    break;
                case 4:
                    DrawSmoothStar(tex, cx - 64, cy, 24, 10, starColor);
                    DrawSmoothStar(tex, cx - 22, cy + 18, 25, 11, starColor);
                    DrawSmoothStar(tex, cx + 22, cy + 18, 25, 11, starColor);
                    DrawSmoothStar(tex, cx + 64, cy, 24, 10, starColor);
                    break;
            }

            tex.Apply();
            return tex;
        }
        #endregion

        #region Branch Crests
        private static int GenerateBranchCrests()
        {
            var armyCrest = RenderArmyCrest();
            SaveSprite(armyCrest, Path.Combine(CREST_FOLDER, "Imperial_Army_Crest.png"));
            SaveSprite(armyCrest, Path.Combine(CREST_FOLDER, "Army_Crest.png"));

            var afWings = RenderAirForceWings();
            SaveSprite(afWings, Path.Combine(CREST_FOLDER, "Imperial_Air_Force_Wings.png"));
            SaveSprite(afWings, Path.Combine(CREST_FOLDER, "Air_Force_Wings.png"));

            var sfCrest = RenderSpecialForcesCrest();
            SaveSprite(sfCrest, Path.Combine(CREST_FOLDER, "Special_Forces_Crest.png"));

            return 3;
        }

        private static Texture2D RenderArmyCrest()
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;

            // Circular backing badge
            DrawSmoothCircle(tex, cx, cy, 105, new Color(0.10f, 0.14f, 0.18f, 0.98f), true);
            DrawSmoothCircle(tex, cx, cy, 105, new Color(0.78f, 0.62f, 0.18f, 1f), false, 5);

            Color swordSteel = new Color(0.92f, 0.95f, 0.98f, 1f);
            Color swordHilt = new Color(0.85f, 0.70f, 0.22f, 1f);

            // Crossed combat broadswords
            DrawSmoothLine(tex, cx - 65, cy - 65, cx + 65, cy + 65, 9, swordSteel);
            DrawSmoothLine(tex, cx + 65, cy - 65, cx - 65, cy + 65, 9, swordSteel);

            // Crossguards & Pommels
            DrawSmoothCircle(tex, cx - 55, cy - 55, 14, swordHilt, true);
            DrawSmoothCircle(tex, cx + 55, cy - 55, 14, swordHilt, true);

            // Center imperial star
            DrawSmoothStar(tex, cx, cy, 32, 14, swordHilt);

            tex.Apply();
            return tex;
        }

        private static Texture2D RenderAirForceWings()
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;

            DrawSmoothCircle(tex, cx, cy, 105, new Color(0.08f, 0.12f, 0.18f, 0.98f), true);
            DrawSmoothCircle(tex, cx, cy, 105, new Color(0.35f, 0.65f, 0.95f, 1f), false, 5);

            Color wingSteel = new Color(0.92f, 0.95f, 0.98f, 1f);
            Color goldProp = new Color(0.96f, 0.80f, 0.22f, 1f);

            // Aviator swept wings
            for (int i = 0; i < 5; i++)
            {
                int offset = i * 8;
                DrawSmoothLine(tex, cx - 15, cy - 5 + offset, cx - 90 + (i * 6), cy + 10 + (i * 8), 7, wingSteel);
                DrawSmoothLine(tex, cx + 15, cy - 5 + offset, cx + 90 - (i * 6), cy + 10 + (i * 8), 7, wingSteel);
            }

            // Central propeller emblem
            DrawSmoothCircle(tex, cx, cy + 6, 20, goldProp, true);
            DrawSmoothStar(tex, cx, cy + 6, 16, 7, Color.white);

            tex.Apply();
            return tex;
        }

        private static Texture2D RenderSpecialForcesCrest()
        {
            var tex = CreateEmptyTexture();
            int cx = TEX_SIZE / 2;
            int cy = TEX_SIZE / 2;

            DrawSmoothCircle(tex, cx, cy, 105, new Color(0.08f, 0.14f, 0.10f, 0.98f), true);
            DrawSmoothCircle(tex, cx, cy, 105, new Color(0.25f, 0.85f, 0.50f, 1f), false, 5);

            Color daggerSteel = new Color(0.94f, 0.96f, 0.99f, 1f);
            Color lightningGold = new Color(1f, 0.85f, 0.20f, 1f);

            // Crossed lightning bolts
            DrawSmoothLine(tex, cx - 60, cy + 45, cx + 60, cy - 45, 7, lightningGold);
            DrawSmoothLine(tex, cx + 60, cy + 45, cx - 60, cy - 45, 7, lightningGold);

            // Vertical Fairbairn-Sykes combat dagger
            DrawSmoothLine(tex, cx, cy - 65, cx, cy + 65, 8, daggerSteel);
            DrawSmoothTriangle(tex, cx - 8, cy - 50, cx + 8, cy - 50, cx, cy - 75, daggerSteel); // Dagger point
            DrawBeveledRect(tex, cx - 22, cy + 30, 44, 9, daggerSteel, daggerSteel * 0.7f, 2); // Crossguard
            DrawSmoothCircle(tex, cx, cy + 72, 9, daggerSteel, true); // Pommel

            tex.Apply();
            return tex;
        }
        #endregion

        #region Medal Ribbons
        private static int GenerateRibbonTextures()
        {
            var medals = MedalRegistry.GetAllOfficialMedals();
            int count = 0;

            foreach (var def in medals)
            {
                var tex = RenderSingleRibbon(def);
                string cleanId = def.medalId.Replace(" ", "_");
                string path = Path.Combine(RIBBON_FOLDER, $"Ribbon_{cleanId}.png");
                SaveSprite(tex, path);

                // Also save canonical alias without Ribbon_ prefix
                string altPath = Path.Combine(RIBBON_FOLDER, $"{cleanId}.png");
                SaveSprite(tex, altPath);
                count++;
            }

            return count;
        }

        private static Texture2D RenderSingleRibbon(MedalRibbonDefinition def)
        {
            var tex = CreateEmptyTexture();
            Color[] colors = def.GetColors();

            // Ribbon Dimensions: 228 wide, 80 tall (approx 3:1 military ratio)
            int rx = 14;
            int ry = 88;
            int rw = 228;
            int rh = 80;

            // Draw outer metallic ribbon mount frame (burnished brass / gold)
            Color frameGold = new Color(0.72f, 0.58f, 0.18f, 1f);
            DrawBeveledRect(tex, rx - 3, ry - 3, rw + 6, rh + 6, frameGold * 1.1f, frameGold * 0.6f, 3);

            // Fill color stripes
            int stripeCount = colors.Length;
            float stripeWidth = rw / (float)stripeCount;

            for (int i = 0; i < stripeCount; i++)
            {
                int sx0 = rx + (int)(i * stripeWidth);
                int sx1 = rx + (int)((i + 1) * stripeWidth);
                if (i == stripeCount - 1) sx1 = rx + rw; // Clamp last stripe

                Color col = colors[i];
                for (int y = ry; y < ry + rh; y++)
                {
                    // Subtle weave texture gradient
                    float weave = 1.0f - (Mathf.Abs((y - (ry + rh / 2f)) / (rh / 2f)) * 0.12f);
                    Color finalStripe = new Color(col.r * weave, col.g * weave, col.b * weave, 1f);

                    for (int x = sx0; x < sx1; x++)
                    {
                        tex.SetPixel(x, y, finalStripe);
                    }
                }
            }

            // Draw appurtenance device in the center if specified
            int cx = TEX_SIZE / 2;
            int cy = ry + rh / 2;
            Color deviceGold = new Color(1f, 0.88f, 0.30f, 1f);
            Color deviceSilver = new Color(0.96f, 0.97f, 1f, 1f);

            if (def.deviceIcon == "★")
            {
                Color starCol = def.category == MedalCategory.ImperialValor ? deviceGold : deviceSilver;
                DrawSmoothStar(tex, cx, cy, 18, 8, starCol);
            }
            else if (def.deviceIcon == "V")
            {
                DrawSmoothLine(tex, cx - 10, cy + 12, cx, cy - 10, 5, deviceGold);
                DrawSmoothLine(tex, cx, cy - 10, cx + 10, cy + 12, 5, deviceGold);
            }
            else if (def.deviceIcon == "⚔")
            {
                DrawSmoothLine(tex, cx - 12, cy - 12, cx + 12, cy + 12, 4, deviceSilver);
                DrawSmoothLine(tex, cx + 12, cy - 12, cx - 12, cy + 12, 4, deviceSilver);
            }
            else if (def.deviceIcon == "✚")
            {
                DrawSmoothLine(tex, cx - 12, cy, cx + 12, cy, 6, new Color(0.85f, 0.15f, 0.15f, 1f));
                DrawSmoothLine(tex, cx, cy - 12, cx, cy + 12, 6, new Color(0.85f, 0.15f, 0.15f, 1f));
            }

            tex.Apply();
            return tex;
        }
        #endregion

        #region Smooth Anti-Aliased Graphics Primitives
        private static Texture2D CreateEmptyTexture()
        {
            var tex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            var pixels = new Color[TEX_SIZE * TEX_SIZE];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
            tex.SetPixels(pixels);
            return tex;
        }

        private static void SaveSprite(Texture2D tex, string relativePath)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath).Replace('\\', '/');
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngData);

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

        private static void BlendPixel(Texture2D tex, int x, int y, Color col)
        {
            if (x < 0 || x >= TEX_SIZE || y < 0 || y >= TEX_SIZE) return;
            if (col.a <= 0f) return;

            Color existing = tex.GetPixel(x, y);
            Color blended = Color.Lerp(existing, col, col.a);
            blended.a = Mathf.Clamp01(existing.a + col.a * (1f - existing.a));
            tex.SetPixel(x, y, blended);
        }

        private static void DrawTacticalShield(Texture2D tex, int cx, int cy, int w, int h, Color fill, Color border)
        {
            for (int y = cy - h; y <= cy + h; y++)
            {
                for (int x = cx - w; x <= cx + w; x++)
                {
                    float nx = Mathf.Abs(x - cx) / (float)w;
                    float ny = (y - (cy - h)) / (float)(h * 2);

                    float limit = (ny < 0.65f) ? (0.35f + (ny / 0.65f) * 0.65f) : 1.0f;
                    if (nx <= limit)
                    {
                        // Check border distance
                        bool isBorder = (nx >= limit - 0.08f) || (ny >= 0.94f) || (ny <= 0.04f);
                        BlendPixel(tex, x, y, isBorder ? border : fill);
                    }
                }
            }
        }

        private static void DrawSmoothChevron(Texture2D tex, int cx, int cy, int armLength, int thickness, Color topCol, Color baseCol)
        {
            for (int dy = -thickness; dy <= thickness; dy++)
            {
                float t = (dy + thickness) / (float)(thickness * 2);
                Color c = Color.Lerp(baseCol, topCol, t);

                for (int dx = 0; dx < armLength; dx++)
                {
                    int dropY = (int)(dx * 0.48f);
                    // Left arm
                    BlendPixel(tex, cx - dx, cy + dy - dropY, c);
                    // Right arm
                    BlendPixel(tex, cx + dx, cy + dy - dropY, c);
                }
            }
        }

        private static void DrawSmoothRocker(Texture2D tex, int cx, int cy, int armLength, int thickness, Color topCol, Color baseCol)
        {
            for (int dy = -thickness; dy <= thickness; dy++)
            {
                float t = (dy + thickness) / (float)(thickness * 2);
                Color c = Color.Lerp(baseCol, topCol, t);

                for (int dx = -armLength; dx <= armLength; dx++)
                {
                    float curve = (dx * dx) / (float)(armLength * 3.5f);
                    BlendPixel(tex, cx + dx, cy + dy + (int)curve, c);
                }
            }
        }

        private static void DrawSmoothDiamond(Texture2D tex, int cx, int cy, int w, int h, Color col)
        {
            for (int y = cy - h; y <= cy + h; y++)
            {
                for (int x = cx - w; x <= cx + w; x++)
                {
                    float d = Mathf.Abs(x - cx) / (float)w + Mathf.Abs(y - cy) / (float)h;
                    if (d <= 1.0f)
                    {
                        float alpha = Mathf.Clamp01((1.0f - d) * 4f);
                        BlendPixel(tex, x, y, new Color(col.r, col.g, col.b, col.a * alpha));
                    }
                }
            }
        }

        private static void DrawSmoothStar(Texture2D tex, int cx, int cy, int outerR, int innerR, Color col)
        {
            for (int y = cy - outerR; y <= cy + outerR; y++)
            {
                for (int x = cx - outerR; x <= cx + outerR; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > outerR) continue;

                    float angle = Mathf.Atan2(dy, dx) - (Mathf.PI / 2f);
                    if (angle < 0) angle += Mathf.PI * 2f;

                    float segment = Mathf.PI * 2f / 5f;
                    float halfSeg = segment / 2f;
                    float modAngle = Mathf.Abs((angle % segment) - halfSeg);
                    float maxDist = Mathf.Lerp(innerR, outerR, 1f - (modAngle / halfSeg));

                    if (dist <= maxDist)
                    {
                        float edgeFade = Mathf.Clamp01((maxDist - dist) * 1.5f);
                        BlendPixel(tex, x, y, new Color(col.r, col.g, col.b, col.a * edgeFade));
                    }
                }
            }
        }

        private static void DrawSmoothCircle(Texture2D tex, int cx, int cy, int radius, Color col, bool fill, int strokeWidth = 3)
        {
            for (int y = cy - radius - strokeWidth; y <= cy + radius + strokeWidth; y++)
            {
                for (int x = cx - radius - strokeWidth; x <= cx + radius + strokeWidth; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (fill)
                    {
                        if (dist <= radius)
                        {
                            float edge = Mathf.Clamp01(radius - dist);
                            BlendPixel(tex, x, y, new Color(col.r, col.g, col.b, col.a * edge));
                        }
                    }
                    else
                    {
                        float inner = radius - strokeWidth;
                        if (dist >= inner && dist <= radius)
                        {
                            BlendPixel(tex, x, y, col);
                        }
                    }
                }
            }
        }

        private static void DrawSmoothLine(Texture2D tex, int x0, int y0, int x1, int y1, int thickness, Color col)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) * 2;
            if (steps == 0) return;

            float halfT = thickness / 2f;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float px = Mathf.Lerp(x0, x1, t);
                float py = Mathf.Lerp(y0, y1, t);

                for (int dy = -(int)halfT; dy <= (int)halfT; dy++)
                {
                    for (int dx = -(int)halfT; dx <= (int)halfT; dx++)
                    {
                        if (dx * dx + dy * dy <= halfT * halfT)
                        {
                            BlendPixel(tex, (int)px + dx, (int)py + dy, col);
                        }
                    }
                }
            }
        }

        private static void DrawBeveledRect(Texture2D tex, int x, int y, int w, int h, Color lightCol, Color darkCol, int bevel)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    bool isTopLeft = (px - x < bevel) || (y + h - 1 - py < bevel);
                    bool isBottomRight = (x + w - 1 - px < bevel) || (py - y < bevel);

                    Color c = isTopLeft ? lightCol : (isBottomRight ? darkCol : Color.Lerp(lightCol, darkCol, 0.5f));
                    BlendPixel(tex, px, py, c);
                }
            }
        }

        private static void DrawSmoothTriangle(Texture2D tex, int x0, int y0, int x1, int y1, int x2, int y2, Color col)
        {
            int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
            int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
            int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
            int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));

            float Area(int ax, int ay, int bx, int by, int cx, int cy) =>
                Mathf.Abs((ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)) / 2f);

            float totalArea = Area(x0, y0, x1, y1, x2, y2);
            if (totalArea <= 0.001f) return;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float a1 = Area(x, y, x1, y1, x2, y2);
                    float a2 = Area(x0, y0, x, y, x2, y2);
                    float a3 = Area(x0, y0, x1, y1, x, y);

                    if (Mathf.Abs(totalArea - (a1 + a2 + a3)) < 0.5f)
                    {
                        BlendPixel(tex, x, y, col);
                    }
                }
            }
        }
        #endregion
    }
}
#endif
