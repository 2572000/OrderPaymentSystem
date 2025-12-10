using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderServiceApi.Requests;
using OrderServiceApi.Services;

namespace OrderServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            var response = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { orderId = response.Id }, response);
        }
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetById(int orderId, CancellationToken cancellationToken)
        {
            var response = await service.GetByIdAsync(orderId, cancellationToken);
            return response is not null ? Ok(response) : NotFound();
        }
        [HttpPost("{orderId}/payment")]
        public async Task<IActionResult> Pay(int orderId,[FromBody] PaymentRequest request, CancellationToken cancellationToken)
        {
            await service.PayAsync(orderId, request, cancellationToken);
            return Created();
        }
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> Cancel(int orderId, CancellationToken cancellationToken)
        {
            await service.CancelAsync(orderId, cancellationToken);
            return NoContent();
        }

    }
}
