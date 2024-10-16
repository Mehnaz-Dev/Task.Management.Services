using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Task.Management.Services.Model;
using Task.Management.Services.Model.Auth;
using Task.Management.Services.Models;
using ClaimTypes = Task.Management.Services.Model.Auth.ClaimTypes;


namespace Task.Management.Services.Repositories
{
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
            string? domain = _configuration["Domain"];
            AuthResponse response = new();

            try
            {
                using PrincipalContext pc = new(ContextType.Domain, domain);
                if (pc.ValidateCredentials(username, password))
                {
                    response.IsAuthenticated = true;

                    // Get the user from the database
                    var user = GetUserByUsername(username);

                    response.Token = GenerateJwtToken(user);
                }
                else
                {
                    response.IsAuthenticated = false;
                    response.ErrorMessage = "Invalid credentials.";
                }
            }
            catch (Exception)
            {
                response.ErrorMessage = "An error occurred while validating credentials.";
            }

            return response;
        }

        // Method to retrieve user by username
        private User GetUserByUsername(string username)
        {
            return _context.Users
                .FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        }

        // Generate JWT token for authenticated user
        private string GenerateJwtToken(User user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Username, user.Username ?? string.Empty),
                new Claim(Task.Management.Services.Model.Auth.ClaimTypes.FirstName, user.FirstName ?? string.Empty),
                new Claim(Task.Management.Services.Model.Auth.ClaimTypes.LastName, user.LastName ?? string.Empty),
                new Claim(Task.Management.Services.Model.Auth.ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(Task.Management.Services.Model.Auth.ClaimTypes.Phone, user.Phone ?? string.Empty),
                new Claim(Task.Management.Services.Model.Auth.ClaimTypes.RoleId, user.RoleId.ToString())
            };

            var tokenOptions = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
    }
}
