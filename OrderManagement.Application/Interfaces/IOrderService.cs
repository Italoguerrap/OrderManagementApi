using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> StartOrderAsync(CancellationToken cancellationToken);
        Task<OrderDto> AddProductToOrderAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken);
        Task<OrderDto> RemoveProductFromOrderAsync(long orderId, long productId, CancellationToken cancellationToken);
        Task<OrderDto> CloseOrderAsync(long orderId, CancellationToken cancellationToken);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken);
        Task<OrderDto?> GetByIdAsync(long orderId, CancellationToken cancellationToken);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken);
    }
}