using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> QueryAsync(OrderFilterCriteriaDto filter, CancellationToken cancellationToken);
        Task<List<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken);
    }
}