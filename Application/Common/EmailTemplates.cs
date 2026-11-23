using System.Net;
using Application.DTOs.Orders;

namespace Application.Common
{
    public static class EmailTemplates
    {
        private static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        private static string Layout(string title, string body) => $"""
            <div style="font-family:Arial,sans-serif;max-width:560px;margin:auto;color:#222">
              <h2 style="margin-bottom:8px">{E(title)}</h2>
              {body}
              <hr style="border:none;border-top:1px solid #ddd;margin:24px 0"/>
              <p style="font-size:12px;color:#888">If you did not request this, you can safely ignore this email.</p>
            </div>
            """;

        private static string Button(string url, string label) =>
            $"<p><a href=\"{E(url)}\" style=\"background:#2563eb;color:#fff;padding:10px 18px;border-radius:6px;text-decoration:none;display:inline-block\">{E(label)}</a></p>" +
            $"<p style=\"font-size:12px;color:#666\">Or copy this link: {E(url)}</p>";

        public static (string Subject, string Html) ConfirmEmail(string firstName, string link, int validHours) =>
            ("Confirm your email", Layout("Confirm your email",
                $"<p>Hi {E(firstName)}, welcome! Please confirm your email address.</p>{Button(link, "Confirm email")}" +
                $"<p style=\"font-size:12px;color:#666\">This link is valid for {validHours} hours.</p>"));

        public static (string Subject, string Html) ResetPassword(string firstName, string link, int validMinutes) =>
            ("Reset your password", Layout("Reset your password",
                $"<p>Hi {E(firstName)}, we received a request to reset your password.</p>{Button(link, "Choose a new password")}" +
                $"<p style=\"font-size:12px;color:#666\">This link is valid for {validMinutes} minutes.</p>"));

        public static (string Subject, string Html) PasswordChanged(string firstName) =>
            ("Your password was changed", Layout("Password changed",
                $"<p>Hi {E(firstName)}, your password was just changed and all your sessions were signed out.</p>" +
                "<p>If this wasn't you, reset your password immediately.</p>"));

        public static (string Subject, string Html) OrderConfirmation(string firstName, OrderDto order)
        {
            var rows = string.Concat(order.Items.Select(i =>
                $"<tr><td style=\"padding:4px 8px\">{E(i.ProductName)}</td><td style=\"padding:4px 8px;text-align:center\">{i.Quantity}</td>" +
                $"<td style=\"padding:4px 8px;text-align:right\">{i.SubTotal:0.00}</td></tr>"));

            var body = $"<p>Hi {E(firstName)}, thank you for your order <b>#{order.Id.ToString()[..8].ToUpperInvariant()}</b>.</p>" +
                "<table style=\"width:100%;border-collapse:collapse\"><tr><th align=\"left\">Item</th><th>Qty</th><th align=\"right\">Total</th></tr>" +
                rows + "</table>" +
                $"<p style=\"text-align:right\">Subtotal: {order.SubTotal:0.00}<br/>" +
                (order.DiscountAmount > 0 ? $"Discount: -{order.DiscountAmount:0.00}<br/>" : "") +
                $"Shipping: {order.ShippingCost:0.00}<br/>Tax: {order.TaxAmount:0.00}<br/><b>Total: {order.TotalPrice:0.00}</b></p>" +
                $"<p>Delivery to: {E(order.ShippingStreet)}, {E(order.ShippingCity)}</p>";

            return ($"Order confirmation #{order.Id.ToString()[..8].ToUpperInvariant()}", Layout("Order received", body));
        }
    }
}
