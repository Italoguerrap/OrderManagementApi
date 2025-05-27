namespace OrderManagement.Application.DTOs
{
    public class TokenDto
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; }
        public UserDto User { get; set; }
    }
}