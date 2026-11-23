using Application.Contracts.Services;
using Application.DTOs.Common;
using Application.DTOs.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace E_Commerce.Controllers
{
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _payments;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService payments, ILogger<PaymentsController> logger)
        {
            _payments = payments;
            _logger = logger;
        }

        /// <summary>
        /// Start an online payment for one of my orders (CreditCard / DebitCard / Wallet).
        /// Returns the Paymob checkout URL to redirect the customer to. The order is marked paid only when
        /// Paymob's signed webhook confirms it - never from the browser redirect.
        /// </summary>
        [Authorize]
        [EnableRateLimiting("auth")]
        [HttpPost("orders/{orderId:guid}/pay")]
        [ProducesResponseType(typeof(ApiResponseDto<PayOrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<ApiResponseDto<PayOrderResponseDto>>> PayOrder(Guid orderId, CancellationToken ct)
        {
            var result = await _payments.PayOrderAsync(orderId, ct);
            return Ok(ApiResponseDto<PayOrderResponseDto>.Ok(result, "Redirect the customer to checkoutUrl to complete the payment."));
        }

        /// <summary>
        /// Paymob "transaction processed" callback (server to server). Authenticated by HMAC (query parameter "hmac").
        /// </summary>
        [AllowAnonymous]
        [HttpPost("paymob/webhook")]
        [RequestSizeLimit(64 * 1024)]
        public async Task<IActionResult> PaymobWebhook([FromQuery] string? hmac, CancellationToken ct)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync(ct);

            var result = await _payments.HandlePaymobWebhookAsync(body, hmac, ct);

            if (result == WebhookResult.InvalidSignature)
                return Unauthorized(ApiResponseDto<object?>.Fail("Invalid signature.", 401));

            // Processed or intentionally ignored: 200 so Paymob stops retrying
            return Ok();
        }

        /// <summary>Request a full refund of a completed online payment (Admin). Status becomes Refunded once Paymob confirms.</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{paymentId:int}/refund")]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object?>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<object?>>> Refund(int paymentId, CancellationToken ct)
        {
            await _payments.RefundPaymentAsync(paymentId, ct);
            return Ok(ApiResponseDto<object?>.Ok(null, "Refund requested. The payment status updates when Paymob confirms."));
        }
    }
}
