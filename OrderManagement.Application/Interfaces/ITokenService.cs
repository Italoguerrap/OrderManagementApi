using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Interfaces
{
    public interface ITokenService
    {
        TokenDto GenerateAccessToken(User user);
        string GenerateRefreshToken();
        long? ValidateToken(string token);
    }
}