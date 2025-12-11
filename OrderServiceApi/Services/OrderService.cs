using OrderServiceApi.Models;
using OrderServiceApi.Repositories;
using OrderServiceApi.Requests;
using OrderServiceApi.Responses;
using System.Globalization;

namespace OrderServiceApi.Services
{
    public class OrderService(IOrderRepository orderRepository,
                              ILogger<OrderService> logger,
                              HttpClient paymentHttpClient) : IOrderService
    {
        public async Task<OrderResponse?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var order=await orderRepository.GetByIdAsync(orderId,cancellationToken);
            
            if(order==null)
            {
                logger.LogWarning("Order with id: {OrderId} not found", orderId);
                return null;
            }
            return OrderResponse.FromModel(order);
        }
        
        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            var items=request.Items.Select(i=>new OrderItem
            {
                ProductId=i.ProductId,
                Quantity=i.Quantity,
                UnitPrice=i.UnitPrice
            }).ToList();

            var order = new Order(request.CustomerId, items);

            await orderRepository.AddAsync(order,cancellationToken);

            logger.LogInformation("Order with id: {OrderId} created successfully", order.Id);

            return OrderResponse.FromModel(order);
        }
        
        public async Task CancelAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
            
            if(order == null)
            {
                logger.LogWarning("Order with id: {OrderId} not found for cancellation", orderId);
                throw new KeyNotFoundException($"Order {orderId} not found");
            }

            if (order.PaidAt.HasValue)
            {
                logger.LogWarning("Attempt to cancel paid order with id: {OrderId}", orderId);
                throw new InvalidOperationException("paid invoice can not be cancelled");
            }

            await orderRepository.RemoveAsync(order, cancellationToken);
            logger.LogInformation("Order with id: {OrderId} cancelled successfully", orderId);
        }

        public async Task PayAsync(int orderId, PaymentRequest request, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
            {
                logger.LogWarning("Order with id: {OrderId} not found for payment", orderId);
                throw new KeyNotFoundException($"Order {orderId} not found");
            }

            if (order.PaidAt.HasValue)
            {
                logger.LogWarning("Attempt to cancel paid order with id: {OrderId}", orderId);
                throw new InvalidOperationException("paid invoice can not be cancelled");
            }

            var payload = new Dictionary<string, string?>
            {
                { "OrderId", orderId.ToString() },
                { "Amount", order.TotalAmount.ToString(CultureInfo.InvariantCulture) },
                { "Currency", "USD" },
                { "PaymentMethod", request.PaymentMethod.ToString() },
                { "CardNumber", request.CardNumber },
                { "CardHolderName", request.CardHolderName },
            };

            HttpResponseMessage response= new HttpResponseMessage();

            try
            {
               response = await paymentHttpClient.PostAsJsonAsync("api/Payment/process", payload, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Payment processing failed for order id: {OrderId}", orderId);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("Payment processing failed for order id: {OrderId} with status: {StatusCode}, body: {Body}",
                    orderId, (int)response.StatusCode, body);
                throw new InvalidOperationException($"Payment failed with status: {(int)response.StatusCode}, body: {body}");
            }

            var paymentResult = await response.Content.ReadFromJsonAsync<PaymentResponse>(cancellationToken);

            if (paymentResult is null)
            {
                var raw = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to deserialize payment response for order id: {OrderId}. Raw response: {Raw}",
                    orderId, raw);
                throw new InvalidOperationException($"Deserialization failed. Raw response: {raw}");
            }

            if (!paymentResult.Success)
            {
                logger.LogWarning("Payment was declined for order id: {OrderId}", orderId);
                throw new InvalidOperationException("Payment was declined");
            }

            order.PaidAt = DateTime.UtcNow;
            order.PaymentReference = paymentResult.TransactionId;

            await orderRepository.UpdateAsync(order, cancellationToken);
            logger.LogInformation("Payment processed successfully for order id: {OrderId}", orderId);
        }
    }
}
