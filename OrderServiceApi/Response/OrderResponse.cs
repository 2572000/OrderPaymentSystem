using OrderServiceApi.Models;

namespace OrderServiceApi.Responses;

public class OrderResponse
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];

    public static OrderResponse FromModel(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            PaymentReference = order.PaymentReference,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(OrderItemResponse.FromModel).ToList()
        };
    }
}
