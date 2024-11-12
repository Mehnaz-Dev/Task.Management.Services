using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using Task.Management.Services.Models;
using Task.Management.Services.Models.Auth;

public class AuthenticateRepository
{
    private readonly TaskManagementDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthenticateRepository(TaskManagementDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public AuthResponse AuthenticateUser(string username, string password)
    {
        AuthResponse response = new();
        var user = GetUserByUsername(username);

        if (user != null && user.Password == password)
        {
            response.IsAuthenticated = true;
            response.Token = GenerateJwtToken(user);
        }
        else
        {
            response.IsAuthenticated = false;
            response.ErrorMessage = "Invalid credentials.";
        }

        return response;
    }

    private User GetUserByUsername(string username)
    {
        return _context.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
    }

    private string GenerateJwtToken(User user)
    {
        SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        SigningCredentials signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        List<Claim> claims = new List<Claim>
        {
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.Username, user.Username ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.FirstName, user.FirstName ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.LastName, user.LastName ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.Phone, user.Phone ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.RoleId, user.RoleId.ToString())
        };
        JwtSecurityToken tokenOptions = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    public string GenerateOtp(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            return null;

        var otp = new Random().Next(100000, 999999).ToString();
        user.Otp = otp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10); 
        _context.SaveChanges();

        return otp;
    }

    public void SendOtpEmail(string email, string otp)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("sheebak.equitec@gmail.com", "nogtalafklqigzzx"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@gmail.com"),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is: {otp}. It is valid for 10 minutes.",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending OTP email: {ex.Message}");
        }
    }

    public bool VerifyOtp(string email, string otp)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.Otp == otp && u.OtpExpiry > DateTime.UtcNow);
        if (user == null)
            return false;

        user.Otp = null;
        user.OtpExpiry = null;
        _context.SaveChanges();

        return true;
    }

    public bool ResetPassword(string email, string newPassword)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            return false;

        user.Password = newPassword; 
        user.Otp = null;
        user.OtpExpiry = null;

        _context.SaveChanges();
        return true;
    }

}
