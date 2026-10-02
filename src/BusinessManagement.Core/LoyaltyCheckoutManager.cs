using System;

namespace BusinessManagement.Core // Match your project's namespace
{
    public class LoyaltyCheckoutManager
    {
        private const decimal RedemptionRate = 0.50m;        // 1 Point = Rs. 0.50 discount value
        private const decimal MaxDiscountPercentage = 0.30m;  // Points can cover at most 30% of the cart
        private const decimal RupeesPerPointEarned = 100.00m; // Earn 1 point per Rs. 100 spent

        public CheckoutCalculationResult CalculateSafeCheckout(decimal cartSubtotal, int customerAvailablePoints, int pointsRequestedForRedemption)
        {
            // 1. Ensure customer isn't trying to use more points than they actually have
            int validPointsToRedeem = Math.Min(pointsRequestedForRedemption, customerAvailablePoints);

            // 2. Calculate requested discount value
            decimal requestedDiscount = validPointsToRedeem * RedemptionRate;

            // 3. SAFE-PROOF CEILING: Calculate maximum allowable discount (30% of cart subtotal)
            decimal maxAllowedDiscount = cartSubtotal * MaxDiscountPercentage;

            // 4. Enforce the ceiling: Take the lower amount to protect profit margins
            decimal finalDiscountApplied = Math.Min(requestedDiscount, maxAllowedDiscount);

            // 5. Recalculate exact points spent if the ceiling clipped the request
            int actualPointsDeducted = (int)(finalDiscountApplied / RedemptionRate);

            // 6. Compute final payable total
            decimal finalPayableAmount = cartSubtotal - finalDiscountApplied;

            // 7. Calculate points earned on the final payable amount
            int pointsEarned = (int)Math.Floor(finalPayableAmount / RupeesPerPointEarned);

            return new CheckoutCalculationResult
            {
                FinalPayable = finalPayableAmount,
                DiscountApplied = finalDiscountApplied,
                PointsRedeemed = actualPointsDeducted,
                PointsEarned = pointsEarned
            };
        }
    }

    public class CheckoutCalculationResult
    {
        public decimal FinalPayable { get; set; }
        public decimal DiscountApplied { get; set; }
        public int PointsRedeemed { get; set; }
        public int PointsEarned { get; set; }
    }
}