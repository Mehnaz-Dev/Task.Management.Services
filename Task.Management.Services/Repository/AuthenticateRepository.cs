using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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

    // Authenticate the user and generate a JWT token
    public AuthResponse AuthenticateUser(string username, string password)
    {
        AuthResponse response = new();

        // Get the user from the database
        var user = GetUserByUsername(username);

        // Directly compare the password (not secure, only for temporary use)
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
        return _context.Users
            .FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
    }

    private string GenerateJwtToken(User user)
    {
        SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        SigningCredentials signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        List<Claim> claims =
        [
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.Username, user.Username ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.FirstName, user.FirstName ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.LastName, user.LastName ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.Phone, user.Phone ?? string.Empty),
            new Claim(Task.Management.Services.Models.Auth.ClaimTypes.RoleId, user.RoleId.ToString())
        ];
        JwtSecurityToken tokenOptions = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
}