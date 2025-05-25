using AutoMapper;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Services
{
    public class OrderService(IOrderRepository orderRepository, IProductRepository productRepository, IOrderItemRepository orderItemRepository, IMapper mapper) : IOrderService
    {
        public async Task<OrderDto> StartOrderAsync(CancellationToken cancellationToken)
        {
            Order order = new Order
            {
                Status = OrderStatus.Open,
                Items = new List<OrderItem>(),
                CreatedAt = DateTime.UtcNow
            };

            order = await orderRepository.AddAsync(order, cancellationToken);

            var orderDto = mapper.Map<OrderDto>(order);
            orderDto.TotalAmount = CalculateOrderTotal(order);
            return orderDto;
        }

        public async Task<OrderDto> AddProductToOrderAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken)
        {
            Order order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new OrderManagementException("Pedido não encontrado.");

            if (order.Status != OrderStatus.Open)
                throw new OrderManagementException("Não é possível adicionar produtos a um pedido fechado.");

            var product = await productRepository.GetByIdAsync(productId, cancellationToken) ?? throw new OrderManagementException("Produto não encontrado.");

            order.Items ??= new List<OrderItem>();

            var orderItem = order.Items.FirstOrDefault(i => i.ProductId == productId && i.DeletionAt == null);

            if (orderItem != null)
                orderItem.Quantity += quantity;
            else
            {
                var newItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                };

                order.Items.Add(newItem);
            }

            order = await orderRepository.UpdateAsync(orderId, order, cancellationToken);
            
            var orderDto = mapper.Map<OrderDto>(order);
            orderDto.TotalAmount = CalculateOrderTotal(order);
            
            return orderDto;
        }

        public async Task<OrderDto> RemoveProductFromOrderAsync(long orderId, long productId, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new OrderManagementException("Pedido não encontrado.");

            if (order.Status != OrderStatus.Open)
                throw new OrderManagementException("Não é possível remover produtos de um pedido fechado.");

            order.Items ??= new List<OrderItem>();

            var orderItem = order.Items.FirstOrDefault(i => i.ProductId == productId && i.DeletionAt == null);
            if (orderItem == null)
                throw new OrderManagementException("Produto não encontrado no pedido.");

            orderItem.DeletionAt = DateTime.UtcNow;
            
            order = await orderRepository.UpdateAsync(orderId, order, cancellationToken);
            
            var orderDto = mapper.Map<OrderDto>(order);
            orderDto.TotalAmount = CalculateOrderTotal(order);
            
            return orderDto;
        }

        public async Task<OrderDto> CloseOrderAsync(long orderId, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new OrderManagementException("Pedido não encontrado.");

            if (order.Items == null || !order.Items.Any(i => i.DeletionAt == null))
                throw new OrderManagementException("Não é possível fechar um pedido sem produtos.");

            order.Status = OrderStatus.Closed;
            order.ClosedAt = DateTime.UtcNow;

            order = await orderRepository.UpdateAsync(orderId, order, cancellationToken);
            
            var orderDto = mapper.Map<OrderDto>(order);
            orderDto.TotalAmount = CalculateOrderTotal(order);
            
            return orderDto;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetAllAsync(cancellationToken);
            return orders.Select(o => {
                var dto = mapper.Map<OrderDto>(o);
                dto.TotalAmount = CalculateOrderTotal(o);
                return dto;
            });
        }

        public async Task<OrderDto?> GetByIdAsync(long orderId, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order is null)
                return null;
            
            var dto = mapper.Map<OrderDto>(order);
            dto.TotalAmount = CalculateOrderTotal(order);
            return dto;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetByStatusAsync(status, cancellationToken);
            return orders.Select(o => {
                var dto = mapper.Map<OrderDto>(o);
                dto.TotalAmount = CalculateOrderTotal(o);
                return dto;
            });
        }

        private decimal CalculateOrderTotal(Order order)
        {
            return order.Items?.Where(i => i.DeletionAt == null).Sum(i => i.Price * i.Quantity) ?? 0;
        }
    }
}