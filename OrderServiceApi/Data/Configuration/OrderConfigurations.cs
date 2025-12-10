using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderServiceApi.Models;

namespace OrderServiceApi.Data.Configuration
{
    public class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {

            builder.Property(o => o.CustomerId)
                   .IsRequired();

            builder.Property(o => o.CreatedAt)
                   .IsRequired();
            builder.HasMany(o=>o.Items)
                   .WithOne()
                   .HasForeignKey(oi => oi.OrderId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
