namespace Application.Common
{
    /// <summary>Store-wide pricing rules. Bound from the "Shop" configuration section.</summary>
    public class ShopSettings
    {
        /// <summary>Flat shipping fee applied to every order below the free-shipping threshold.</summary>
        public decimal FlatShippingCost { get; set; }

        /// <summary>Orders with a taxable amount at or above this value ship for free. 0 = never free.</summary>
        public decimal FreeShippingThreshold { get; set; }

        /// <summary>Tax (VAT) percentage applied on the amount after discounts, e.g. 14.</summary>
        public decimal TaxRatePercent { get; set; }
    }

    public record OrderTotals(decimal SubTotal, decimal Discount, decimal Shipping, decimal Tax, decimal Total);

    /// <summary>Single source of truth for basket and order totals. Never trust client/cached values.</summary>
    public static class PricingCalculator
    {
        public static OrderTotals Calculate(decimal subtotal, decimal discount, ShopSettings settings)
        {
            subtotal = Math.Max(0m, subtotal);
            discount = Math.Clamp(discount, 0m, subtotal);

            var taxable = subtotal - discount;
            var tax = Math.Round(taxable * settings.TaxRatePercent / 100m, 2);

            var shipping = subtotal == 0m ? 0m
                : settings.FreeShippingThreshold > 0m && taxable >= settings.FreeShippingThreshold ? 0m
                : settings.FlatShippingCost;

            return new OrderTotals(subtotal, discount, shipping, tax, taxable + tax + shipping);
        }
    }
}
