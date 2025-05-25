using AutoMapper;
using Moq;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Test
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _orderItemRepoMock = new Mock<IOrderItemRepository>();
            _mapperMock = new Mock<IMapper>();
            _orderService = new OrderService(
                _orderRepoMock.Object, 
                _productRepoMock.Object, 
                _orderItemRepoMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task StartOrderAsync_ShouldCreateNewOrderWithOpenStatus()
        {
            // Arrange
            var newOrder = new Order
            {
                Id = 1,
                Status = OrderStatus.Open,
                Items = new List<OrderItem>(),
                CreatedAt = DateTime.UtcNow
            };

            var expectedDto = new OrderDto
            {
                Status = OrderStatus.Open,
                Items = new List<OrderItemDto>(),
                CreatedAt = newOrder.CreatedAt
            };

            _orderRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newOrder);

            _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.StartOrderAsync(CancellationToken.None);

            // Assert
            Assert.Equal(OrderStatus.Open, result.Status);
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
            _orderRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CloseOrderAsync_WithNoProducts_ShouldThrowException()
        {
            // Arrange
            var emptyOrder = new Order
            {
                Id = 1,
                Status = OrderStatus.Open,
                Items = new List<OrderItem>(),
                CreatedAt = DateTime.UtcNow
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyOrder);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OrderManagementException>(
                () => _orderService.CloseOrderAsync(1, CancellationToken.None));
            
            Assert.Equal("Não é possível fechar um pedido sem produtos.", exception.Message);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<long>(), It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddProductToOrderAsync_ToClosedOrder_ShouldThrowException()
        {
            // Arrange
            var closedOrder = new Order
            {
                Id = 1,
                Status = OrderStatus.Closed,
                Items = new List<OrderItem>(),
                CreatedAt = DateTime.UtcNow,
                ClosedAt = DateTime.UtcNow
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(closedOrder);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OrderManagementException>(
                () => _orderService.AddProductToOrderAsync(1, 1, 1, CancellationToken.None));
            
            Assert.Equal("Não é possível adicionar produtos a um pedido fechado.", exception.Message);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<long>(), It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task RemoveProductFromOrderAsync_ShouldMarkItemAsDeleted()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Open,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 10,
                        ProductId = 5,
                        ProductName = "Test Product",
                        Price = 10.00m,
                        Quantity = 2
                    }
                },
                CreatedAt = DateTime.UtcNow
            };

            var updatedOrder = new Order
            {
                Id = 1,
                Status = OrderStatus.Open,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 10,
                        ProductId = 5,
                        ProductName = "Test Product",
                        Price = 10.00m,
                        Quantity = 2,
                        DeletionAt = DateTime.UtcNow
                    }
                },
                CreatedAt = DateTime.UtcNow
            };

            var expectedDto = new OrderDto
            {
                Status = OrderStatus.Open,
                Items = new List<OrderItemDto>(), // Empty because the item is marked as deleted
                CreatedAt = order.CreatedAt,
                TotalAmount = 0 // Total is 0 because all items are deleted
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
            
            _orderRepoMock.Setup(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedOrder);

            _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.RemoveProductFromOrderAsync(1, 5, CancellationToken.None);

            // Assert
            Assert.Empty(result.Items); // The items should be empty because the mapper filters out deleted items
            Assert.Equal(0, result.TotalAmount); // Total should be 0 as all items are deleted
            _orderRepoMock.Verify(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}