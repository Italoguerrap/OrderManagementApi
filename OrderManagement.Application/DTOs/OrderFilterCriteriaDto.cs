using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs
{
    public class OrderFilterCriteriaDto
    {
        public DateOnly? OrderDate { get; set; }
        public OrderStatus? Status { get; set; }
        public decimal? MinTotalAmount { get; set; }
        public decimal? MaxTotalAmount { get; set; }
    }
}