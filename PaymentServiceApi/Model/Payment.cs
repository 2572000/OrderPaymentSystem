namespace PaymentServiceApi.Models;

public class Payment
{
    public int OrderId { get; set; }

    public string? PaymentReference { get; set; }

    public DateTime ProcessedAt { get; set; }

    public decimal Amount { get; set; }
}