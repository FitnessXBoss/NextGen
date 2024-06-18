using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NextGen.src.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NextGen.src.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentStatusService _paymentStatusService;
        private static readonly ConcurrentDictionary<string, PaymentNotification> _paymentNotifications = new();

        public PaymentController(ILogger<PaymentController> logger, IPaymentStatusService paymentStatusService)
        {
            _logger = logger;
            _paymentStatusService = paymentStatusService;
        }

        [HttpPost("paymentSuccessful")]
        public async Task<IActionResult> PaymentSuccessful([FromBody] PaymentNotification notification)
        {
            _logger.LogInformation("Received payment notification: {Comment}, {Amount}, {Sender}",
                notification.Comment, notification.Amount, notification.Sender);

            _paymentNotifications[notification.Comment] = notification;

            await _paymentStatusService.NotifyPaymentStatusAsync(notification);

            return Ok(new { message = "Payment received successfully" });
        }

        [HttpGet("checkStatus")]
        public IActionResult CheckStatus([FromQuery] string uniqueId)
        {
            if (_paymentNotifications.TryGetValue(uniqueId, out var notification))
            {
                return Ok(new
                {
                    paymentReceived = true,
                    notification.Comment,
                    notification.Amount,
                    notification.Sender
                });
            }

            return Ok(new { paymentReceived = false });
        }
    }
}
