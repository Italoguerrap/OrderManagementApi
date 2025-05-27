using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByCpfAsync(string cpf, CancellationToken cancellationToken);
    }
}