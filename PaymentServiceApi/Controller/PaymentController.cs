using Microsoft.AspNetCore.Mvc;
using PaymentServiceApi.Data;
using PaymentServiceApi.Models;
using PaymentServiceApi.Request;

namespace PaymentServiceApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController(
        AppDbContext context,
        ILogger<PaymentController> logger,
        IConfiguration configuration) : ControllerBase
    {
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            logger.LogInformation("Start payment processing request for OrderId: {OrderId}, Amount: {Amount}",
                request?.OrderId, request?.Amount);
            try
            {
                if (request == null || request.OrderId == 0 || request.Amount <= 0)
                {
                    logger.LogWarning("Invalid payment request received. OrderId:{OrderId},Amount: {Amount}",
                        request?.OrderId, request?.Amount);
                    return BadRequest("Invalid payment request.");
                }

                if(string.IsNullOrWhiteSpace(configuration["PaymentGateway:ApiKey"])) //PaymentGateway
                {
                    logger.LogError("Payment gateway configuration is missing. Payment fild for This OrderId:{OrderId}",
                        request.OrderId);
                    throw new InvalidOperationException("Payment gateway configuration is missing.");
                }
                

               
                if (Random.Shared.NextDouble() < 0.1)
                {
                    logger.LogWarning("Payment processing failed for OrderId: {OrderId}", request.OrderId);
                    return StatusCode(502, new { Message = "Payment processing failed." });
                }


                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    PaymentReference = $"txn_{Guid.NewGuid().ToString("N")[..8]}",
                    ProcessedAt = DateTime.UtcNow
                };

                await context.SaveChangesAsync();

                logger.LogInformation("Payment processed successfully for OrderId: {OrderId}, PaymentReference: {PaymentReference}",
                    payment.OrderId, payment.PaymentReference);

                return Created($"/payment/{payment.PaymentReference}", new
                {
                    TransactionId = payment.PaymentReference,
                    Success = true
                });
            }
            catch(Exception ex)
            {
                logger.LogCritical(ex,"Critical error occurred while processing payment for OrderId: {OrderId}",
                    request?.OrderId);
                return StatusCode(500, new { Message = "Critical error occurred while processing the payment." });
            }
        }
    }
}
