using Microsoft.AspNetCore.Mvc;
using NextGen.src.Models;
using NextGen.src.Services;
using System.Diagnostics;

namespace NextGen.src.Services.Api
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentStatusService _paymentStatusService;

        public PaymentController(IPaymentStatusService paymentStatusService)
        {
            _paymentStatusService = paymentStatusService;
        }

        [HttpPost("paymentSuccessful")]
        public IActionResult PaymentSuccessful([FromBody] PaymentNotification notification)
        {
            try
            {
                Debug.WriteLine("Received notification:");
                Debug.WriteLine($"Comment: {notification.Comment}");
                Debug.WriteLine($"Amount: {notification.Amount}");
                Debug.WriteLine($"Sender: {notification.Sender}");

                _paymentStatusService.UpdatePaymentStatus(notification);

                return Ok(new { message = "Payment received successfully", notification });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing payment notification: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
