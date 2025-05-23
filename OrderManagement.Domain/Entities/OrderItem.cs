namespace OrderManagement.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}