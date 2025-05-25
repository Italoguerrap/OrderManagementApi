using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Context;

namespace OrderManagement.Infrastructure.Repositories
{
    public class OrderItemRepository(AppDbContext dbContext) : IOrderItemRepository
    {
        public async Task<OrderItem> AddAsync(OrderItem entity, CancellationToken cancellationToken)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await dbContext.OrderItems.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var orderItem = await dbContext.OrderItems.FindAsync(id, cancellationToken);
            if (orderItem is null)
                return false;

            orderItem.DeletionAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<OrderItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbContext.OrderItems.Where(o => o.DeletionAt == null).ToListAsync(cancellationToken);
        }

        public async Task<OrderItem?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return await dbContext.OrderItems.FirstOrDefaultAsync(o => o.Id == id && o.DeletionAt == null, cancellationToken);
        }

        public async Task<OrderItem> UpdateAsync(long id, OrderItem entity, CancellationToken cancellationToken)
        {
            var existing = await dbContext.OrderItems.FirstOrDefaultAsync(o => o.Id == id && o.DeletionAt == null, cancellationToken) ?? throw new InvalidOperationException("Item do pedido não encontrado.");

            existing.ProductId = entity.ProductId;
            existing.ProductName = entity.ProductName;
            existing.Price = entity.Price;
            existing.Quantity = entity.Quantity;
            existing.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }
    }
}