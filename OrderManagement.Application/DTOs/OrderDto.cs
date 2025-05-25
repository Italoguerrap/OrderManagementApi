using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs
{
    public class OrderDto
    {
        public OrderStatus Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal TotalAmount { get; set; }
    }
}