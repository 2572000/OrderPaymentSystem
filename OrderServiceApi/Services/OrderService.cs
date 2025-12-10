using OrderServiceApi.Models;
using OrderServiceApi.Repositories;
using OrderServiceApi.Requests;
using OrderServiceApi.Responses;
using System.Globalization;

namespace OrderServiceApi.Services
{
    public class OrderService(IOrderRepository orderRepository, HttpClient paymentHttpClient) : IOrderService
    {
        public async Task<OrderResponse?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var order=await orderRepository.GetByIdAsync(orderId,cancellationToken);
            if(order==null)
            {
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
            return OrderResponse.FromModel(order);
        }
        
        public async Task CancelAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(orderId, cancellationToken)
                  ?? throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.PaidAt.HasValue)
            {
                throw new InvalidOperationException("paid invoice can not be cancelled");
            }

            await orderRepository.RemoveAsync(order, cancellationToken);
        }

        public async Task PayAsync(int orderId, PaymentRequest request, CancellationToken cancellationToken = default)
        {
            var order = await orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {orderId} not found");

            if (order.PaidAt.HasValue)
                throw new InvalidOperationException("Order has already been paid.");

            var payload = new Dictionary<string, string?>
            {
                { "OrderId", orderId.ToString() },
                { "Amount", order.TotalAmount.ToString(CultureInfo.InvariantCulture) },
                { "Currency", "USD" },
                { "PaymentMethod", request.PaymentMethod.ToString() },
                { "CardNumber", request.CardNumber },
                { "CardHolderName", request.CardHolderName },
            };
            
            var response = await paymentHttpClient.PostAsJsonAsync("api/Payment/process", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Payment failed with status: {(int)response.StatusCode}, body: {body}");
            }

            var paymentResult = await response.Content.ReadFromJsonAsync<PaymentResponse>(cancellationToken);

            if (paymentResult is null)
            {
                var raw = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Deserialization failed. Raw response: {raw}");
            }

            if (!paymentResult.Success)
                throw new InvalidOperationException("Payment was declined");

            order.PaidAt = DateTime.UtcNow;
            order.PaymentReference = paymentResult.TransactionId;

            await orderRepository.UpdateAsync(order, cancellationToken);
        }
    }
}
