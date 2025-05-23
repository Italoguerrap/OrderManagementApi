using AutoMapper;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Services
{
    public class OrderItemRepository(IOrderItemRepository orderItemRepository, IMapper mapper) : IOrderItemRepository
    {
        public Task<OrderItem> AddAsync(OrderItem entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OrderItem?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OrderItem> UpdateAsync(long id, OrderItem entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
