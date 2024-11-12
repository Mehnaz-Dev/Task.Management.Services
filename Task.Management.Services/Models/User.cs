namespace Task.Management.Services.Models
{
    public class User
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Otp { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public DateTime? OtpSentAt { get; set; }
        public long? RoleId { get; set; }
    }
}
