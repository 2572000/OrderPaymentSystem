namespace OrderServiceApi.Models
{
    public class OrderItem
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;

        public OrderItem()
        {
            
        }

        public OrderItem(int productId,int quantity,decimal unitPrice)
        {
            if(quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
            if(unitPrice <= 0) throw new ArgumentException("unitPrice must be greater than zero.");
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}