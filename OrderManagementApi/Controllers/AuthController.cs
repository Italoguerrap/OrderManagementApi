using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagementApi.Requests;
using System.Text.RegularExpressions;

namespace OrderManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #region Autenticação

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, [FromServices] IValidator<RegisterRequest> validator, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            try
            {
                string cpf = Regex.Replace(request.Cpf, @"[^\d]", "");
                TokenDto token = await _authService.RegisterAsync(cpf, request.Password, cancellationToken);

                return Ok(new
                {
                    token.AccessToken,
                    token.Expiration,
                    token.RefreshToken,
                    User = new { Id = token.User.Id, Cpf = token.User.Cpf }
                });
            }
            catch (OrderManagementException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro ao processar a solicitação");
            }
        }

        /// <summary>
        /// Autentica um usuário existente
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, [FromServices] IValidator<LoginRequest> validator, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            try
            {
                string cpf = Regex.Replace(request.Cpf, @"[^\d]", "");
                TokenDto token = await _authService.AuthenticateAsync(cpf, request.Password, cancellationToken);

                return Ok(new
                {
                    token.AccessToken,
                    token.Expiration,
                    token.RefreshToken,
                    User = new { Id = token.User.Id, Cpf = token.User.Cpf }
                });
            }
            catch (OrderManagementException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro ao processar a solicitação");
            }
        }
        #endregion

        #region Recuperação de Senha

        /// <summary>
        /// Redefine a senha de um usuário
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, [FromServices] IValidator<ResetPasswordRequest> validator, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            try
            {
                string cpf = Regex.Replace(request.Cpf, @"[^\d]", "");
                bool result = await _authService.ResetPasswordAsync(cpf, request.NewPassword, cancellationToken);

                return Ok(new { Message = "Senha redefinida com sucesso" });
            }
            catch (OrderManagementException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro ao processar a solicitação");
            }
        }

        #endregion
    }
}
