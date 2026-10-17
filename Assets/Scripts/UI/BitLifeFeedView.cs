using System.Text;
using ArmyLife.Core;
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

        private void OnEnable()
        {
            GameEvents.OnFeedEventLogged += AddFeedEntry;
        }

        private void OnDisable()
        {
            GameEvents.OnFeedEventLogged -= AddFeedEntry;
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
            switch (category.ToUpper())
            {
                case "BIRTH":         return "<color=#00FFAA><b>[ORIGIN]</b></color>";
                case "PAYDAY":        return "<color=#44FF44><b>[FINANCES]</b></color>";
                case "PROMOTION":     return "<color=#00AAFF><b>[PROMOTION]</b></color>";
                case "BREAKTHROUGH":  return "<color=#FFD700><b>[CULTIVATION]</b></color>";
                case "TESTING":       return "<color=#00FFFF><b>[APPARATUS]</b></color>";
                case "SECRET CAVE":   return "<color=#FF00FF><b>[ANCIENT SECT]</b></color>";
                case "MARRIAGE":      return "<color=#FF88FF><b>[DYNASTY]</b></color>";
                case "CLEARANCE":     return "<color=#FFCC00><b>[SECURITY]</b></color>";
                case "BOTTLENECK":    return "<color=#FF6600><b>[WARNING]</b></color>";
                case "COMBAT":        return "<color=#00FF88><b>[WARFARE]</b></color>";
                case "CASUALTY":      return "<color=#FF4444><b>[CASUALTY]</b></color>";
                case "RETIREMENT":    return "<color=#FFDD44><b>[RETIREMENT]</b></color>";
                case "CRIPPLED":      return "<color=#AA0000><b>[CRIPPLED]</b></color>";
                case "DEATH":         return "<color=#FF0000><b>[FATALITY]</b></color>";
                case "REINCARNATION": return "<color=#FFD700><b>[SUCCESSION]</b></color>";
                default:              return $"<b>[{category}]</b>";
            }
        }
    }
}
