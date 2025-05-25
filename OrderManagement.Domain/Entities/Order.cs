using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities
{
    public class Order : BaseEntity
    {
        public OrderStatus Status { get; set; }

        public List<OrderItem> Items { get; set; }
        
        public DateTime? ClosedAt { get; set; }
    }
}
