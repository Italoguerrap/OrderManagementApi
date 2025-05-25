using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Context;

namespace OrderManagement.Infrastructure.Repositories
{
    public class OrderRepository(AppDbContext dbContext) : IOrderRepository
    {
        public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Items ??= new List<OrderItem>();

            await dbContext.Orders.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return await dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id && o.DeletionAt == null, cancellationToken);
        }

        public async Task<List<Order>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbContext.Orders
                .Include(o => o.Items)
                .Where(o => o.DeletionAt == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order> UpdateAsync(long id, Order entity, CancellationToken cancellationToken)
        {
            var existing = await dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id && o.DeletionAt == null, cancellationToken) ?? throw new InvalidOperationException("Pedido não encontrado.");

            existing.Status = entity.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Items = entity.Items;
            existing.ClosedAt = entity.ClosedAt;

            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var order = await dbContext.Orders.FindAsync(id, cancellationToken);
            if (order is null)
                return false;

            order.DeletionAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<Order>> QueryAsync(OrderFilterCriteriaDto filter, CancellationToken cancellationToken)
        {
            var queryable = dbContext.Orders
                .Include(o => o.Items)
                .Where(o => o.DeletionAt == null);

            if (filter.Status.HasValue)
                queryable = queryable.Where(o => o.Status == filter.Status.Value);

            if (filter.OrderDate.HasValue)
            {
                var date = filter.OrderDate.Value.ToDateTime(TimeOnly.MinValue);
                var nextDay = date.AddDays(1);
                queryable = queryable.Where(o => o.CreatedAt >= date && o.CreatedAt < nextDay);
            }

            if (filter.MinTotalAmount.HasValue || filter.MaxTotalAmount.HasValue)
            {
                queryable = queryable.Where(o =>
                    (filter.MinTotalAmount == null || o.Items.Sum(i => i.Price * i.Quantity) >= filter.MinTotalAmount) &&
                    (filter.MaxTotalAmount == null || o.Items.Sum(i => i.Price * i.Quantity) <= filter.MaxTotalAmount)
                );
            }

            return await queryable.ToListAsync(cancellationToken);
        }

        public async Task<List<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken)
        {
            return await dbContext.Orders
                .Include(o => o.Items)
                .Where(o => o.Status == status && o.DeletionAt == null)
                .ToListAsync(cancellationToken);
        }
    }
}