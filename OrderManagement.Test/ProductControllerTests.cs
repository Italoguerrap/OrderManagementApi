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

namespace OrderManagement.Test
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IValidator<ProductRequest>> _validatorMock;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _validatorMock = new Mock<IValidator<ProductRequest>>();
            _controller = new ProductController(_productServiceMock.Object);
        }

        #region Criar Produto
        [Fact]
        public async Task CreateProductAsync_WithValidRequest_ShouldReturnCreatedStatusWithProduct()
        {
            // Arrange
            var request = new ProductRequest
            {
                Name = "Test Product",
                Price = 10.00m
            };

            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var createdProductDto = new ProductDto
            {
                Name = "Test Product",
                Price = 10.00m
            };

            _productServiceMock.Setup(service => service.CreateProductAsync(It.IsAny<ProductDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdProductDto);

            // Act
            var result = await _controller.CreateProductAsync(request, _validatorMock.Object, CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
            Assert.Same(createdProductDto, statusCodeResult.Value);
        }

        [Fact]
        public async Task CreateProductAsync_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new ProductRequest
            {
                Name = "",
                Price = -10.00m
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required"),
                new ValidationFailure("Price", "Price must be greater than 0")
            };

            var validationResult = new ValidationResult(validationFailures);
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.CreateProductAsync(request, _validatorMock.Object, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<object>>(badRequestResult.Value);
            Assert.Equal(2, errors.Count());
        }
        #endregion

        #region Atualizar Produto
        [Fact]
        public async Task UpdateProductAsync_WithValidRequest_ShouldReturnOkWithUpdatedProduct()
        {
            // Arrange
            long productId = 1;
            var request = new ProductRequest
            {
                Name = "Updated Product",
                Price = 15.00m
            };

            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var updatedProductDto = new ProductDto
            {
                Name = "Updated Product",
                Price = 15.00m
            };

            _productServiceMock.Setup(service => service.UpdateProductAsync(productId, It.IsAny<ProductDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedProductDto);

            // Act
            var result = await _controller.UpdateProductAsync(productId, request, _validatorMock.Object, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(updatedProductDto, okResult.Value);
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            long productId = 1;
            var request = new ProductRequest
            {
                Name = "",
                Price = -10.00m
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required"),
                new ValidationFailure("Price", "Price must be greater than 0")
            };

            var validationResult = new ValidationResult(validationFailures);
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.UpdateProductAsync(productId, request, _validatorMock.Object, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<object>>(badRequestResult.Value);
            Assert.Equal(2, errors.Count());
        }

        [Fact]
        public async Task UpdateProductAsync_WithNonExistingProduct_ShouldReturnNotFound()
        {
            // Arrange
            long productId = 999;
            var request = new ProductRequest
            {
                Name = "Updated Product",
                Price = 15.00m
            };

            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _productServiceMock.Setup(service => service.UpdateProductAsync(productId, It.IsAny<ProductDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OrderManagementException("Produto não encontrado."));

            // Act
            var result = await _controller.UpdateProductAsync(productId, request, _validatorMock.Object, CancellationToken.None);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Produto não encontrado.", notFoundResult.Value);
        }
        #endregion

        #region Remover Produto
        [Fact]
        public async Task DeleteProductAsync_WithExistingProduct_ShouldReturnNoContent()
        {
            // Arrange
            long productId = 1;
            _productServiceMock.Setup(service => service.DeleteProductAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProductAsync(productId, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProductAsync_WithNonExistingProduct_ShouldReturnNotFound()
        {
            // Arrange
            long productId = 999;
            _productServiceMock.Setup(service => service.DeleteProductAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProductAsync(productId, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region Consultar Produto por ID
        [Fact]
        public async Task GetByIdAsync_WithExistingProduct_ShouldReturnOkWithProduct()
        {
            // Arrange
            long productId = 1;
            var productDto = new ProductDto
            {
                Name = "Test Product",
                Price = 10.00m
            };

            _productServiceMock.Setup(service => service.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productDto);

            // Act
            var result = await _controller.GetByIdAsync(productId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(productDto, okResult.Value);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingProduct_ShouldReturnNotFound()
        {
            // Arrange
            long productId = 999;
            _productServiceMock.Setup(service => service.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductDto)null);

            // Act
            var result = await _controller.GetByIdAsync(productId, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion
    }
}