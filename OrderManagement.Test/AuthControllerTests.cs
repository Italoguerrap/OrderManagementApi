using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagementApi.Controllers;
using OrderManagementApi.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;

namespace OrderManagement.Test
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IValidator<RegisterRequest>> _registerValidatorMock;
        private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
        private readonly Mock<IValidator<ResetPasswordRequest>> _resetPasswordValidatorMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _registerValidatorMock = new Mock<IValidator<RegisterRequest>>();
            _loginValidatorMock = new Mock<IValidator<LoginRequest>>();
            _resetPasswordValidatorMock = new Mock<IValidator<ResetPasswordRequest>>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        #region Registro de Usuário
        [Fact]
        public async Task RegisterAsync_WithValidRequest_ShouldReturnOkWithToken()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Cpf = "123.456.789-00",
                Password = "Password123!"
            };

            var validationResult = new ValidationResult();
            _registerValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var userDto = new UserDto
            {
                Id = 1,
                Cpf = "12345678900"
            };

            var tokenDto = new TokenDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                Expiration = DateTime.UtcNow.AddHours(1),
                User = userDto
            };

            _authServiceMock.Setup(service => service.RegisterAsync(It.IsAny<string>(), request.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenDto);

            // Act
            var result = await _controller.RegisterAsync(request, _registerValidatorMock.Object, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonConvert.SerializeObject(okResult.Value);
            var responseValue = JsonConvert.DeserializeObject<TokenDto>(json);

            Assert.Equal(tokenDto.AccessToken, responseValue.AccessToken);
            Assert.Equal(tokenDto.RefreshToken, responseValue.RefreshToken);
            Assert.Equal(tokenDto.Expiration, responseValue.Expiration);
            Assert.Equal(userDto.Id, responseValue.User.Id);
            Assert.Equal(userDto.Cpf, responseValue.User.Cpf);
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Cpf = "invalid-cpf",
                Password = "123" // Too short
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Cpf", "CPF is invalid"),
                new ValidationFailure("Password", "Password must be at least 6 characters")
            };

            var validationResult = new ValidationResult(validationFailures);
            _registerValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.RegisterAsync(request, _registerValidatorMock.Object, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<object>>(badRequestResult.Value);
            Assert.Equal(2, errors.Count());
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateCpf_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Cpf = "123.456.789-00",
                Password = "Password123!"
            };

            var validationResult = new ValidationResult();
            _registerValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _authServiceMock.Setup(service => service.RegisterAsync(It.IsAny<string>(), request.Password, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OrderManagementException("CPF já cadastrado."));

            // Act
            var result = await _controller.RegisterAsync(request, _registerValidatorMock.Object, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("CPF já cadastrado.", badRequestResult.Value);
        }
        #endregion

        #region Login de Usuário
        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnOkWithToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Cpf = "123.456.789-00",
                Password = "Password123!"
            };

            var validationResult = new ValidationResult();
            _loginValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var expectedUser = new UserDto
            {
                Id = 1,
                Cpf = "12345678900"
            };

            var expectedToken = new TokenDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                Expiration = DateTime.UtcNow.AddHours(1),
                User = expectedUser
            };

            _authServiceMock
                .Setup(s => s.AuthenticateAsync(It.IsAny<string>(), request.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            // Act
            var result = await _controller.LoginAsync(request, _loginValidatorMock.Object, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonConvert.SerializeObject(okResult.Value);
            var actualToken = JsonConvert.DeserializeObject<TokenDto>(json);

            Assert.Equal(expectedToken.AccessToken, actualToken.AccessToken);
            Assert.Equal(expectedToken.RefreshToken, actualToken.RefreshToken);
            Assert.Equal(expectedToken.Expiration, actualToken.Expiration);
            Assert.Equal(expectedUser.Id, actualToken.User.Id);
            Assert.Equal(expectedUser.Cpf, actualToken.User.Cpf);
        }


        [Fact]
        public async Task LoginAsync_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Cpf = "invalid-cpf",
                Password = "" // Empty password
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Cpf", "CPF is invalid"),
                new ValidationFailure("Password", "Password is required")
            };

            var validationResult = new ValidationResult(validationFailures);
            _loginValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.LoginAsync(request, _loginValidatorMock.Object, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<object>>(badRequestResult.Value);
            Assert.Equal(2, errors.Count());
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Cpf = "123.456.789-00",
                Password = "WrongPassword123!"
            };

            var validationResult = new ValidationResult();
            _loginValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _authServiceMock.Setup(service => service.AuthenticateAsync(It.IsAny<string>(), request.Password, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OrderManagementException("Credenciais inválidas."));

            // Act
            var result = await _controller.LoginAsync(request, _loginValidatorMock.Object, CancellationToken.None);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Credenciais inválidas.", unauthorizedResult.Value);
        }
        #endregion
    }
}