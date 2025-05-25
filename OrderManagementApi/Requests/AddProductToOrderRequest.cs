namespace OrderManagement.API.Requests
{
    public class AddProductToOrderRequest
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
