using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentServiceApi.Request;
using System.Threading.Tasks;

namespace PaymentServiceApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment(PaymentRequest request)
        {
            //Simulate payment processing Delay
            await Task.Delay(Random.Shared.Next(100,500));

            var success = Random.Shared.NextDouble() > 0.1; 

            if (!success)
                return StatusCode(StatusCodes.Status502BadGateway, new { Message = "Payment processing failed." });


            // Payment processing logic would go here
            return Ok(new 
            {
                TransactionId = $"txn_{Guid.NewGuid():N}"[..8],
                Success=true
            });
        }
    }
}
