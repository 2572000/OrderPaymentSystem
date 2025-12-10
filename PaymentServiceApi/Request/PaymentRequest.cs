using PaymentServiceApi.Enum;

namespace PaymentServiceApi.Request
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal  Amount { get; set; }
        public string Currency { get; set; }="USD";
        public PaymentMethod PaymentMethod { get; set; }
        public string? CardNumber { get; set; }
        public string? CardHoderName { get; set; }

    }
}
