using System.ComponentModel;

namespace Task.Management.Services.Models.Auth
{
    public class AuthRequest
    {
        [DisplayName("User Name")]
        public string UserName { get; set; } = string.Empty;

        [DisplayName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
