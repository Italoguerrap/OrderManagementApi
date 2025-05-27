using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenDto> AuthenticateAsync(string cpf, string password, CancellationToken cancellationToken);
        Task<TokenDto> RegisterAsync(string cpf, string password, CancellationToken cancellationToken);
        Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken);
        Task<bool> ResetPasswordAsync(string cpf, string newPassword, CancellationToken cancellationToken);
        bool ValidateToken(string token);
    }
}