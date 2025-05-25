namespace OrderManagement.Domain.Entities
{
    public class OrderManagementException : Exception
    {
        public OrderManagementException(string message) : base(message)
        {
        }
        public OrderManagementException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
