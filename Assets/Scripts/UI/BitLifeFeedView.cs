using System.Text;
using ArmyLife.Core;
using ArmyLife.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArmyLife.UI
{
    public class BitLifeFeedView : MonoBehaviour
    {
        [Header("UI Feed Components")]
        public TextMeshProUGUI feedContentText;
        public ScrollRect feedScrollRect;

        private readonly StringBuilder feedBuilder = new StringBuilder();
        private const int MaxFeedEntries = 100;
        private int entryCount = 0;

        private void Awake()
        {
            if (feedContentText != null)
            {
                feedContentText.raycastTarget = false;
            }
            if (feedScrollRect != null)
            {
                if (feedScrollRect.GetComponent<RectMask2D>() == null && feedScrollRect.viewport == null)
                {
                    feedScrollRect.gameObject.AddComponent<RectMask2D>();
                }
            }
        }

        private void OnEnable()
        {
            GameEvents.OnFeedEventLogged += AddFeedEntry;
        }

        private void OnDisable()
        {
            GameEvents.OnFeedEventLogged -= AddFeedEntry;
        }

        public void ClearFeed()
        {
            feedBuilder.Clear();
            entryCount = 0;
            if (feedContentText != null)
            {
                feedContentText.text = "";
            }
        }

        public void AddFeedEntry(string category, string message)
        {
            string categoryBadge = FormatCategoryBadge(category);
            string formattedEntry = $"{categoryBadge} {message}\n\n";

            feedBuilder.Insert(0, formattedEntry); // Add newest at top
            entryCount++;

            if (entryCount > MaxFeedEntries)
            {
                // Trim older entries if needed
                feedBuilder.Length = Mathf.Min(feedBuilder.Length, 8000);
            }

            if (feedContentText != null)
            {
                feedContentText.text = feedBuilder.ToString();
            }

            // Reset scroll to top to see latest
            if (feedScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                feedScrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private string FormatCategoryBadge(string category)
        {
            ThemePalette p = ThemeManager.Instance != null 
                ? ThemeManager.Instance.GetCurrentPalette() 
                : ThemeManager.GetPalette(ThemeMode.Dark);

            string goldHex = ColorUtility.ToHtmlStringRGB(p.accentGold);
            string greenHex = ColorUtility.ToHtmlStringRGB(p.accentEmerald);
            string blueHex = ColorUtility.ToHtmlStringRGB(p.accentCobalt);
            string redHex = ColorUtility.ToHtmlStringRGB(p.accentCrimson);
            string coyoteHex = ColorUtility.ToHtmlStringRGB(p.accentCoyote);
            string oliveHex = ColorUtility.ToHtmlStringRGB(p.accentOlive);

            switch (category.ToUpper())
            {
                case "ORIGIN":
                case "ENLISTMENT":    return $"<color=#{oliveHex}><b>[ENLISTMENT]</b></color>";
                case "PAYDAY":
                case "FINANCES":      return $"<color=#{greenHex}><b>[PAYROLL]</b></color>";
                case "PROMOTION":     return $"<color=#{goldHex}><b>[PROMOTION]</b></color>";
                case "QUALIFICATION": return $"<color=#{goldHex}><b>[QUALIFICATION]</b></color>";
                case "ASVAB":         return $"<color=#{blueHex}><b>[MEPS / ASVAB]</b></color>";
                case "INTELLIGENCE":  return $"<color=#{coyoteHex}><b>[INTELLIGENCE]</b></color>";
                case "MARRIAGE":
                case "PERSONAL":
                case "FAMILY":        return $"<color=#{coyoteHex}><b>[PERSONAL LIFE]</b></color>";
                case "CLEARANCE":     return $"<color=#{goldHex}><b>[SECURITY CLEARANCE]</b></color>";
                case "WARNING":       return $"<color=#{goldHex}><b>[DISCIPLINARY WARNING]</b></color>";
                case "COMBAT":
                case "WARFARE":       return $"<color=#{coyoteHex}><b>[COMBAT DEPLOYMENT]</b></color>";
                case "CASUALTY":      return $"<color=#{redHex}><b>[CASUALTY REPORT]</b></color>";
                case "RETIREMENT":    return $"<color=#{goldHex}><b>[HONORABLE RETIREMENT]</b></color>";
                case "WOUND":         return $"<color=#{redHex}><b>[COMBAT WOUND]</b></color>";
                case "DEATH":
                case "KIA":           return $"<color=#{redHex}><b>[K.I.A. NOTICE]</b></color>";
                case "SETTINGS":      return $"<color=#{blueHex}><b>[SETTINGS]</b></color>";
                case "MEDICAL":       return $"<color=#{redHex}><b>[MEDICAL CLINIC]</b></color>";
                case "TRAINING":      return $"<color=#{oliveHex}><b>[TRAINING DRILL]</b></color>";
                case "BARRACKS":      return $"<color=#{oliveHex}><b>[BARRACKS LIFE]</b></color>";
                case "ARMORY":
                case "PX":            return $"<color=#{coyoteHex}><b>[PX ARMORY]</b></color>";
                default:              return $"<color=#{coyoteHex}><b>[{category.ToUpper()}]</b></color>";
            }
        }
    }
}
