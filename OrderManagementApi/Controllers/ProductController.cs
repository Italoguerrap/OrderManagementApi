using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagementApi.Requests;

namespace OrderManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        #region Gerenciamento de Produtos

        /// <summary>
        /// Cria um novo produto
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProductAsync([FromBody] ProductRequest request, [FromServices] IValidator<ProductRequest> validator, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            ProductDto productDto = new ProductDto
            {
                Name = request.Name,
                Price = request.Price
            };

            var createdProduct = await productService.CreateProductAsync(productDto, cancellationToken);

            return StatusCode(201, createdProduct);
        }

        /// <summary>
        /// Atualiza um produto existente
        /// </summary>
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProductAsync([FromRoute] long productId, [FromBody] ProductRequest request, [FromServices] IValidator<ProductRequest> validator, CancellationToken cancellationToken)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            try
            {
                var productDto = new ProductDto
                {
                    Name = request.Name,
                    Price = request.Price
                };

                var updatedProduct = await productService.UpdateProductAsync(productId, productDto, cancellationToken);
                return Ok(updatedProduct);
            }
            catch (OrderManagementException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Remove um produto existente
        /// </summary>
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProductAsync([FromRoute] long productId, CancellationToken cancellationToken)
        {
            var result = await productService.DeleteProductAsync(productId, cancellationToken);
            return result ? NoContent() : NotFound();
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtém todos os produtos com suporte a paginação
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProductsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var products = await productService.GetAllProductsAsync(cancellationToken);
            var totalCount = products.Count();
            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = pagedProducts
            });
        }

        /// <summary>
        /// Obtém um produto específico pelo seu ID
        /// </summary>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] long productId, CancellationToken cancellationToken)
        {
            var product = await productService.GetByIdAsync(productId, cancellationToken);
            return product is null ? NotFound() : Ok(product);
        }

        #endregion
    }
}