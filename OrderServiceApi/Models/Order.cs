namespace OrderServiceApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentReference { get; set; }
        public decimal TotalAmount =>Items?.Sum(i => i.Total) ?? 0;
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        public Order()
        {
            
        }
        public Order(int customerId,IEnumerable<OrderItem> items)
        {
            if(!items.Any())
                throw new ArgumentException("An order must have at least one item.");
            CustomerId= customerId;
            CreatedAt= DateTime.UtcNow;
            Items = items.ToList();
        }

    }
}
