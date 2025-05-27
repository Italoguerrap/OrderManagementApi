using System.Text.RegularExpressions;

namespace OrderManagement.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Cpf { get; set; }
        public string PasswordHash { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public void SetCpfWithoutMask()
        {
            if (!string.IsNullOrEmpty(Cpf))
            {
                Cpf = Regex.Replace(Cpf, @"[^\d]", "");
            }
        }
    }
}