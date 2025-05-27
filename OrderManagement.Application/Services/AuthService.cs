using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Services
{
    public class AuthService(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher<User> passwordHasher, IMapper mapper) : IAuthService
    {

        public async Task<TokenDto> AuthenticateAsync(string cpf, string password, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByCpfAsync(cpf, cancellationToken);
            if (user == null)
                throw new OrderManagementException("Usuário não encontrado");

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                throw new OrderManagementException("Credenciais inválidas");

            var token = tokenService.GenerateAccessToken(user);

            // Atualiza o refresh token no banco de dados
            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userRepository.UpdateAsync(user.Id, user, cancellationToken);

            return token;
        }

        public async Task<TokenDto> RegisterAsync(string cpf, string password, CancellationToken cancellationToken)
        {
            var existingUser = await userRepository.GetByCpfAsync(cpf, cancellationToken);
            if (existingUser != null)
                throw new OrderManagementException("CPF já cadastrado");

            var user = new User { Cpf = cpf };
            user.SetCpfWithoutMask();
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            user = await userRepository.AddAsync(user, cancellationToken);
            var token = tokenService.GenerateAccessToken(user);

            // Salva o refresh token
            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userRepository.UpdateAsync(user.Id, user, cancellationToken);

            return token;
        }

        public async Task<TokenDto> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
        {
            var userId = tokenService.ValidateToken(accessToken);
            if (!userId.HasValue)
                throw new OrderManagementException("Token inválido");

            var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new OrderManagementException("Refresh token inválido ou expirado");

            var newToken = tokenService.GenerateAccessToken(user);

            // Atualiza o refresh token
            user.RefreshToken = newToken.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userRepository.UpdateAsync(user.Id, user, cancellationToken);

            return newToken;
        }

        public async Task<bool> ResetPasswordAsync(string cpf, string newPassword, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByCpfAsync(cpf, cancellationToken);
            if (user == null)
                throw new OrderManagementException("Usuário não encontrado");

            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            await userRepository.UpdateAsync(user.Id, user, cancellationToken);
            return true;
        }

        public bool ValidateToken(string token)
        {
            return tokenService.ValidateToken(token) != null;
        }
    }
}