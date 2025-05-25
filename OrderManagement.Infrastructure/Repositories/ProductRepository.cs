using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Context;

namespace OrderManagement.Infrastructure.Repositories
{
    public class ProductRepository(AppDbContext dbContext) : IProductRepository
    {
        public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await dbContext.Products.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var product = await dbContext.Products.FindAsync(id, cancellationToken);
            if (product is null)
                return false;

            product.DeletionAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbContext.Products
                .Where(p => p.DeletionAt == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return await dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletionAt == null, cancellationToken);
        }

        public async Task<Product> UpdateAsync(long id, Product entity, CancellationToken cancellationToken)
        {
            var existing = await dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletionAt == null, cancellationToken)
                ?? throw new InvalidOperationException("Produto não encontrado.");

            existing.Name = entity.Name;
            existing.Price = entity.Price;
            existing.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }
    }
}