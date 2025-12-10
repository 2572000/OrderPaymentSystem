namespace OrderServiceApi.Requests;

public class CreateOrderRequest
{
    public int CustomerId { get; set; }

    public List<CreateOrderItemRequest> Items { get; set; } = new();

    public decimal TotalAmount { get; set; }
}
