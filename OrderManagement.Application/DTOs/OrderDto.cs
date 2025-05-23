using OrderManagement.Domain.Entities;
using static OrderManagement.Domain.Enums.OrderStatus;

namespace OrderManagement.Application.DTOs
{
    public class OrderDto
    {
        public Status Status { get; set; }

        public List<OrderItem> Items { get; set; }
    }
}
