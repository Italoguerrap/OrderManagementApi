using OrderManagement.Domain.Entities;
using static OrderManagement.Domain.Enums.OrderStatus;

namespace OrderManagement.Application.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetByStatusAsync(Status status, CancellationToken cancellationToken);
        Task<Order?> GetWithItemsAsync(long id, CancellationToken cancellationToken);
    }
}