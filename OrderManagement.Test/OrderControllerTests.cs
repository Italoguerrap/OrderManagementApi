using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using OrderManagement.API.Controllers;
using OrderManagement.API.Requests;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagementApi.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrderManagement.Test
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IValidator<AddProductToOrderRequest>> _validatorMock;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _validatorMock = new Mock<IValidator<AddProductToOrderRequest>>();
            _controller = new OrderController(_orderServiceMock.Object);
        }

        #region Iniciar Pedido
        [Fact]
        public async Task StartOrderAsync_ShouldReturnCreatedStatusCodeWithOrder()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                Status = OrderStatus.Open,
                Items = new List<OrderItemDto>(),
                CreatedAt = DateTime.UtcNow
            };

            _orderServiceMock.Setup(service => service.StartOrderAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(orderDto);

            // Act
            var result = await _controller.StartOrderAsync(CancellationToken.None);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
            Assert.Same(orderDto, statusCodeResult.Value);
        }
        #endregion

        #region Fechar Pedido
        [Fact]
        public async Task CloseOrderAsync_ShouldReturnOkResultWithClosedOrder()
        {
            // Arrange
            long orderId = 1;
            var closedOrderDto = new OrderDto
            {
                Status = OrderStatus.Closed,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductId = 1,
                        ProductName = "Test Product",
                        Price = 10.00m,
                        Quantity = 2
                    }
                },
                CreatedAt = DateTime.UtcNow,
                ClosedAt = DateTime.UtcNow,
                TotalAmount = 20.00m
            };

            _orderServiceMock.Setup(service => service.CloseOrderAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(closedOrderDto);

            // Act
            var result = await _controller.CloseOrderAsync(orderId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(closedOrderDto, okResult.Value);
        }

        [Fact]
        public async Task CloseOrderAsync_WhenExceptionIsThrown_ShouldLetMiddlewareHandleIt()
        {
            // Arrange
            long orderId = 1;
            _orderServiceMock.Setup(service => service.CloseOrderAsync(orderId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OrderManagementException("Não é possível fechar um pedido sem produtos."));

            // Act & Assert
            await Assert.ThrowsAsync<OrderManagementException>(
                () => _controller.CloseOrderAsync(orderId, CancellationToken.None));
        }
        #endregion

        #region Adicionar Produto ao Pedido
        [Fact]
        public async Task AddProductAsync_WithValidRequest_ShouldReturnOkWithUpdatedOrder()
        {
            // Arrange
            long orderId = 1;
            var request = new AddProductToOrderRequest
            {
                ProductId = 5,
                Quantity = 2
            };

            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var updatedOrderDto = new OrderDto
            {
                Status = OrderStatus.Open,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductId = 5,
                        ProductName = "Test Product",
                        Price = 10.00m,
                        Quantity = 2
                    }
                },
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 20.00m
            };

            _orderServiceMock.Setup(service => service.AddProductToOrderAsync(orderId, request.ProductId, request.Quantity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedOrderDto);

            // Act
            var result = await _controller.AddProductAsync(orderId, request, _validatorMock.Object, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(updatedOrderDto, okResult.Value);
        }

        [Fact]
        public async Task AddProductAsync_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            long orderId = 1;
            var request = new AddProductToOrderRequest
            {
                ProductId = 0, // Invalid product ID
                Quantity = 0   // Invalid quantity
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("ProductId", "Product ID must be greater than 0"),
                new ValidationFailure("Quantity", "Quantity must be greater than 0")
            };

            var validationResult = new ValidationResult(validationFailures);
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.AddProductAsync(orderId, request, _validatorMock.Object, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<object>>(badRequestResult.Value);
            Assert.Equal(2, errors.Count());
        }

        [Fact]
        public async Task AddProductAsync_WhenServiceThrowsException_ShouldLetMiddlewareHandleIt()
        {
            // Arrange
            long orderId = 1;
            var request = new AddProductToOrderRequest
            {
                ProductId = 5,
                Quantity = 2
            };

            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _orderServiceMock.Setup(service => service.AddProductToOrderAsync(orderId, request.ProductId, request.Quantity, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OrderManagementException("Não é possível adicionar produtos a um pedido fechado."));

            // Act & Assert
            await Assert.ThrowsAsync<OrderManagementException>(
                () => _controller.AddProductAsync(orderId, request, _validatorMock.Object, CancellationToken.None));
        }
        #endregion

        #region Remover Produto do Pedido
        [Fact]
        public async Task RemoveProductAsync_ShouldReturnOkWithUpdatedOrder()
        {
            // Arrange
            long orderId = 1;
            long productId = 5;

            var updatedOrderDto = new OrderDto
            {
                Status = OrderStatus.Open,
                Items = new List<OrderItemDto>(),
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 0.00m
            };

            _orderServiceMock.Setup(service => service.RemoveProductFromOrderAsync(orderId, productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedOrderDto);

            // Act
            var result = await _controller.RemoveProductAsync(orderId, productId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(updatedOrderDto, okResult.Value);
        }

        [Fact]
        public async Task RemoveProductAsync_WhenServiceThrowsException_ShouldLetMiddlewareHandleIt()
        {
            // Arrange
            long orderId = 1;
            long productId = 5;

            _orderServiceMock.Setup(service => service.RemoveProductFromOrderAsync(orderId, productId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OrderManagementException("Produto não encontrado no pedido."));

            // Act & Assert
            await Assert.ThrowsAsync<OrderManagementException>(
                () => _controller.RemoveProductAsync(orderId, productId, CancellationToken.None));
        }
        #endregion
    }
}
