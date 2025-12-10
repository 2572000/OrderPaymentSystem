using OrderServiceApi.Models;

namespace OrderServiceApi.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
        Task RemoveAsync(Order order, CancellationToken cancellationToken = default);
    }
}
