using System.Drawing;

public class SlowMovingItemModel
{
    public string SKU { get; set; }
    public string ItemName { get; set; }
    public int StockQuantity { get; set; }
    public int UnitsSoldLast30Days { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal HoldingValue => StockQuantity * UnitPrice;

    // AI Recommendation property generated dynamically based on metrics
    public string SmartRecommendation { get; set; }
    public Color ActionBadgeColor { get; set; }
}

public static class InventoryIntelligenceEngine
{
    public static List<SlowMovingItemModel> AnalyzeSlowMovers(List<SlowMovingItemModel> rawItems)
    {
        var recommendations = new List<SlowMovingItemModel>();

        foreach (var item in rawItems)
        {
            // Scenario 1: High stock quantity with very low velocity (< 3 sold in 30 days)
            if (item.StockQuantity >= 15 && item.UnitsSoldLast30Days <= 2)
            {
                item.SmartRecommendation = "⚡ Action: Run 20% Flash Sale or Bundle with a fast-moving item.";
                item.ActionBadgeColor = Color.FromArgb(239, 68, 68); // Red-ish alert
            }
            // Scenario 2: Moderate stock, slow movement, but high capital tied up (Holding Value > 5000)
            else if (item.HoldingValue > 5000 && item.UnitsSoldLast30Days <= 5)
            {
                item.SmartRecommendation = "🎯 Action: Target VIP Fidelity Members via Email/SMS campaign.";
                item.ActionBadgeColor = Color.FromArgb(245, 158, 11); // Amber warning
            }
            // Scenario 3: Standard slow mover
            else
            {
                item.SmartRecommendation = "🔄 Action: Move to front-of-store shelf or POS counter display.";
                item.ActionBadgeColor = Color.FromArgb(59, 130, 246); // Blue info
            }

            recommendations.Add(item);
        }

        return recommendations;
    }
}