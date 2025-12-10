using Microsoft.EntityFrameworkCore;
using OrderServiceApi.Data;
using OrderServiceApi.Models;

namespace OrderServiceApi.Repositories
{
    public class OrderRepository(AppDbContext context) : IOrderRepository
    {
        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await context.Orders.AddAsync(order, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var order = await context.Orders.AsNoTracking().Include(o=>o.Items).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
            if (order == null)
                return null;
            return order;
        }

        public async Task RemoveAsync(Order order, CancellationToken cancellationToken = default)
        {
            
                context.Orders.Remove(order);
                await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            context.Orders.Update(order);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
