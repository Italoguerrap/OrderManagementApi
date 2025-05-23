using static OrderManagement.Domain.Enums.OrderStatus;

namespace OrderManagement.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Status Status { get; set; }

        public List<OrderItem> Items { get; set; }
    }
}
