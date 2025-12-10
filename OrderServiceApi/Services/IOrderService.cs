using OrderServiceApi.Requests;
using OrderServiceApi.Responses;

namespace OrderServiceApi.Services
{
    public interface IOrderService
    {
        Task<OrderResponse?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
        Task PayAsync(int orderId, PaymentRequest request, CancellationToken cancellationToken = default);
        Task CancelAsync(int orderId, CancellationToken cancellationToken = default);
    }
}
