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

        #region Iniciar Pedido
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
        #endregion

        #region Fechar Pedido
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
        public async Task CloseOrderAsync_WithProducts_ShouldCloseOrderSuccessfully()
        {
            // Arrange
            var now = DateTime.UtcNow;
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
                CreatedAt = now
            };

            var closedOrder = new Order
            {
                Id = 1,
                Status = OrderStatus.Closed,
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
                CreatedAt = now,
                ClosedAt = now
            };

            var expectedDto = new OrderDto
            {
                Status = OrderStatus.Closed,
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
                CreatedAt = now,
                ClosedAt = now,
                TotalAmount = 20.00m
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
            
            _orderRepoMock.Setup(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(closedOrder);

            _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.CloseOrderAsync(1, CancellationToken.None);

            // Assert
            Assert.Equal(OrderStatus.Closed, result.Status);
            Assert.NotNull(result.ClosedAt);
            Assert.Equal(20.00m, result.TotalAmount);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion

        #region Adicionar Produto ao Pedido
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
        public async Task AddProductToOrderAsync_WithNewProduct_ShouldAddProductSuccessfully()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Open,
                Items = new List<OrderItem>(),
                CreatedAt = now
            };

            var product = new Product
            {
                Id = 5,
                Name = "Test Product",
                Price = 10.00m
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
                        Quantity = 2
                    }
                },
                CreatedAt = now
            };

            var expectedDto = new OrderDto
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
                CreatedAt = now,
                TotalAmount = 20.00m
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
            
            _productRepoMock.Setup(repo => repo.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            
            _orderRepoMock.Setup(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedOrder);

            _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.AddProductToOrderAsync(1, 5, 2, CancellationToken.None);

            // Assert
            Assert.Equal(OrderStatus.Open, result.Status);
            Assert.Single(result.Items);
            Assert.Equal(20.00m, result.TotalAmount);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddProductToOrderAsync_WithExistingProduct_ShouldIncreaseQuantity()
        {
            // Arrange
            var now = DateTime.UtcNow;
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
                CreatedAt = now
            };

            var product = new Product
            {
                Id = 5,
                Name = "Test Product",
                Price = 10.00m
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
                        Quantity = 5
                    }
                },
                CreatedAt = now
            };

            var expectedDto = new OrderDto
            {
                Status = OrderStatus.Open,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductId = 5,
                        ProductName = "Test Product",
                        Price = 10.00m,
                        Quantity = 5
                    }
                },
                CreatedAt = now,
                TotalAmount = 50.00m
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);
            
            _productRepoMock.Setup(repo => repo.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            
            _orderRepoMock.Setup(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedOrder);

            _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.AddProductToOrderAsync(1, 5, 3, CancellationToken.None);

            // Assert
            Assert.Equal(OrderStatus.Open, result.Status);
            Assert.Single(result.Items);
            Assert.Equal(5, result.Items[0].Quantity);
            Assert.Equal(50.00m, result.TotalAmount);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddProductToOrderAsync_ProductNotFound_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Open,
                Items = new List<OrderItem>(),
                CreatedAt = DateTime.UtcNow
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _productRepoMock.Setup(repo => repo.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OrderManagementException>(
                () => _orderService.AddProductToOrderAsync(1, 5, 2, CancellationToken.None));
            
            Assert.Equal("Produto não encontrado.", exception.Message);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<long>(), It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddProductToOrderAsync_OrderNotFound_ShouldThrowException()
        {
            // Arrange
            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OrderManagementException>(
                () => _orderService.AddProductToOrderAsync(1, 5, 2, CancellationToken.None));
            
            Assert.Equal("Pedido não encontrado.", exception.Message);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<long>(), It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion
        
        #region Remover Produto do Pedido
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
                Items = new List<OrderItemDto>(),
                CreatedAt = order.CreatedAt,
                TotalAmount = 0
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
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalAmount);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(1, It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveProductFromOrderAsync_ProductNotInOrder_ShouldThrowException()
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

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OrderManagementException>(
                () => _orderService.RemoveProductFromOrderAsync(1, 999, CancellationToken.None));
            
            Assert.Equal("Produto não encontrado no pedido.", exception.Message);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<long>(), It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveProductFromOrderAsync_ClosedOrder_ShouldThrowException()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Closed,
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
                CreatedAt = DateTime.UtcNow,
                ClosedAt = DateTime.UtcNow
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<OrderManagementException>(
                () => _orderService.RemoveProductFromOrderAsync(1, 5, CancellationToken.None));
            
            Assert.Equal("Não é possível remover produtos de um pedido fechado.", exception.Message);
            _orderRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<long>(), It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        #endregion

        #region Consultar Todos os Pedidos
        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    Status = OrderStatus.Open,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = 10,
                            ProductId = 5,
                            ProductName = "Product 1",
                            Price = 10.00m,
                            Quantity = 2
                        }
                    },
                    CreatedAt = now
                },
                new Order
                {
                    Id = 2,
                    Status = OrderStatus.Closed,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = 11,
                            ProductId = 6,
                            ProductName = "Product 2",
                            Price = 15.00m,
                            Quantity = 1
                        }
                    },
                    CreatedAt = now,
                    ClosedAt = now
                }
            };

            var orderDtos = new List<OrderDto>
            {
                new OrderDto
                {
                    Status = OrderStatus.Open,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto
                        {
                            ProductId = 5,
                            ProductName = "Product 1",
                            Price = 10.00m,
                            Quantity = 2
                        }
                    },
                    CreatedAt = now,
                    TotalAmount = 20.00m
                },
                new OrderDto
                {
                    Status = OrderStatus.Closed,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto
                        {
                            ProductId = 6,
                            ProductName = "Product 2",
                            Price = 15.00m,
                            Quantity = 1
                        }
                    },
                    CreatedAt = now,
                    ClosedAt = now,
                    TotalAmount = 15.00m
                }
            };

            _orderRepoMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders);

            _mapperMock.Setup(m => m.Map<OrderDto>(orders[0]))
                .Returns(orderDtos[0]);

            _mapperMock.Setup(m => m.Map<OrderDto>(orders[1]))
                .Returns(orderDtos[1]);

            // Act
            var result = await _orderService.GetAllOrdersAsync(CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(OrderStatus.Open, resultList[0].Status);
            Assert.Equal(OrderStatus.Closed, resultList[1].Status);
            Assert.Equal(20.00m, resultList[0].TotalAmount);
            Assert.Equal(15.00m, resultList[1].TotalAmount);
        }
        #endregion

        #region Consultar Pedido por ID
        [Fact]
        public async Task GetByIdAsync_ExistingOrder_ShouldReturnOrder()
        {
            // Arrange
            var now = DateTime.UtcNow;
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
                CreatedAt = now
            };

            var expectedDto = new OrderDto
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
                CreatedAt = now,
                TotalAmount = 20.00m
            };

            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.GetByIdAsync(1, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderStatus.Open, result.Status);
            Assert.Single(result.Items);
            Assert.Equal(20.00m, result.TotalAmount);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingOrder_ShouldReturnNull()
        {
            // Arrange
            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.GetByIdAsync(1, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region Consultar Pedidos por Status
        [Fact]
        public async Task GetOrdersByStatusAsync_ShouldReturnOrdersWithSpecifiedStatus()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var openOrders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    Status = OrderStatus.Open,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = 10,
                            ProductId = 5,
                            ProductName = "Product 1",
                            Price = 10.00m,
                            Quantity = 2
                        }
                    },
                    CreatedAt = now
                },
                new Order
                {
                    Id = 3,
                    Status = OrderStatus.Open,
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Id = 12,
                            ProductId = 7,
                            ProductName = "Product 3",
                            Price = 5.00m,
                            Quantity = 4
                        }
                    },
                    CreatedAt = now
                }
            };

            var openOrderDtos = new List<OrderDto>
            {
                new OrderDto
                {
                    Status = OrderStatus.Open,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto
                        {
                            ProductId = 5,
                            ProductName = "Product 1",
                            Price = 10.00m,
                            Quantity = 2
                        }
                    },
                    CreatedAt = now,
                    TotalAmount = 20.00m
                },
                new OrderDto
                {
                    Status = OrderStatus.Open,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto
                        {
                            ProductId = 7,
                            ProductName = "Product 3",
                            Price = 5.00m,
                            Quantity = 4
                        }
                    },
                    CreatedAt = now,
                    TotalAmount = 20.00m
                }
            };

            _orderRepoMock.Setup(repo => repo.GetByStatusAsync(OrderStatus.Open, It.IsAny<CancellationToken>()))
                .ReturnsAsync(openOrders);

            _mapperMock.Setup(m => m.Map<OrderDto>(openOrders[0]))
                .Returns(openOrderDtos[0]);

            _mapperMock.Setup(m => m.Map<OrderDto>(openOrders[1]))
                .Returns(openOrderDtos[1]);

            // Act
            var result = await _orderService.GetOrdersByStatusAsync(OrderStatus.Open, CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, item => Assert.Equal(OrderStatus.Open, item.Status));
        }
        #endregion
    }
}