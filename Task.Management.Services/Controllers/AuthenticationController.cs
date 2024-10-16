using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using Task.Management.Services.Model.Auth;
using Task.Management.Services.Repositories;

namespace Task.Management.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticateRepository _authenticateRepository;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IConfiguration configuration, AuthenticateRepository authenticateRepository, ILogger<AuthenticationController> logger)
        {
            _configuration = configuration;
            _authenticateRepository = authenticateRepository;
            _logger = logger;
        }

        [HttpPost("Login")]
        [Produces("application/json")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(string))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(string))]
        [SwaggerResponse(StatusCodes.Status200OK, "Success - JWT token will be returned", typeof(string))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        public ActionResult<string> Login([FromBody] AuthRequest user)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                {
                    _logger.LogWarning("Invalid login attempt: {User}", user?.Username);
                    return BadRequest(new { status = StatusCodes.Status400BadRequest, errors = new { message = "Invalid username or password." } });
                }

                AuthResponse authResponse = _authenticateRepository.AuthenticateUser(user.Username, user.Password);
                if (authResponse.IsAuthenticated)
                {
                    return Ok(authResponse.Token);
                }

                _logger.LogWarning("Authentication failed for user: {User}, Error: {Error}", user.Username, authResponse.ErrorMessage);
                return BadRequest(new { status = StatusCodes.Status400BadRequest, errors = new { message = authResponse.ErrorMessage } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user: {UserName}", user?.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
