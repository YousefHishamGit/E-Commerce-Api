namespace Domain.Enums
{
    public static class PaymentMethodExtensions
    {
        /// <summary>Methods that are paid online through the payment gateway (Paymob).</summary>
        public static bool IsOnline(this PaymentMethod method)
            => method is PaymentMethod.CreditCard or PaymentMethod.DebitCard or PaymentMethod.Wallet;
    }
}
