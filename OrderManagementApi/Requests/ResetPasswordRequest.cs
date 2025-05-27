namespace OrderManagementApi.Requests
{
    public class ResetPasswordRequest
    {
        public string Cpf { get; set; }
        public string NewPassword { get; set; }
    }
}