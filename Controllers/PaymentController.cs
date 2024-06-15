using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NextGen.src.Services;
using System.Threading.Tasks;

namespace NextGen.src.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentStatusService _paymentStatusService;

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

            await _paymentStatusService.NotifyPaymentStatusAsync(notification);

            return Ok(new { message = "Payment received successfully" });
        }

        [HttpGet("checkStatus")]
        public IActionResult CheckStatus([FromQuery] string uniqueId)
        {
            // Здесь должна быть логика проверки статуса платежа по uniqueId
            // Возвращаем mock статус для примера
            var isPaymentSuccessful = true; // Здесь нужно вставить реальную проверку

            return Ok(new { IsPaymentSuccessful = isPaymentSuccessful });
        }


    }
}
