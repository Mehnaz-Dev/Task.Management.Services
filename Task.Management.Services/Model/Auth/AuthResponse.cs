namespace Task.Management.Services.Model.Auth
{
    public class AuthResponse
    {
        public bool IsAuthenticated { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
