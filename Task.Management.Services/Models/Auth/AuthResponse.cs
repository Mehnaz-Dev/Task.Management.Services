namespace Task.Management.Services.Models.Auth
{
    public class AuthResponse
    {
        public bool IsAuthenticated { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
