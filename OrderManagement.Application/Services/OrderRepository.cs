using AutoMapper;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Services
{
    public class OrderRepository(IOrderRepository orderRepository, IMapper mapper) : IOrderRepository
    {
        public Task<Order> AddAsync(Order entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetByStatusAsync(OrderStatus.Status status, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Order?> GetWithItemsAsync(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Order> UpdateAsync(long id, Order entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
