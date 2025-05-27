using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Context;

namespace OrderManagement.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext dbContext) : IUserRepository
    {

        public async Task<User> AddAsync(User entity, CancellationToken cancellationToken)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await dbContext.Users.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.FindAsync(id, cancellationToken);
            if (user is null)
                return false;

            user.DeletionAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbContext.Users
                .Where(u => u.DeletionAt == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.DeletionAt == null, cancellationToken);
        }

        public async Task<User?> GetByCpfAsync(string cpf, CancellationToken cancellationToken)
        {
            return await dbContext.Users
                .FirstOrDefaultAsync(u => u.Cpf == cpf && u.DeletionAt == null, cancellationToken);
        }

        public async Task<User> UpdateAsync(long id, User entity, CancellationToken cancellationToken)
        {
            var existing = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.DeletionAt == null, cancellationToken) 
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            existing.Cpf = entity.Cpf;
            existing.PasswordHash = entity.PasswordHash;
            existing.RefreshToken = entity.RefreshToken;
            existing.RefreshTokenExpiryTime = entity.RefreshTokenExpiryTime;
            existing.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }
    }
}