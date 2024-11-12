using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Task.Management.Services.Models;
using Task.Management.Services.Models.Auth;

namespace Task.Management.Services.Controllers
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
        public ActionResult Login(AuthRequest user)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
                {
                    _logger.LogWarning("Invalid login attempt: {User}", user?.UserName);
                    return BadRequest(new { status = StatusCodes.Status400BadRequest, errors = new { message = "Invalid username or password." } });
                }

                AuthResponse authResponse = _authenticateRepository.AuthenticateUser(user.UserName, user.Password);
                if (authResponse.IsAuthenticated)
                {
                    return Ok(authResponse.Token);
                }

                _logger.LogWarning("Authentication failed for user: {User}, Error: {Error}", user.UserName, authResponse.ErrorMessage);
                return BadRequest(new { status = StatusCodes.Status400BadRequest, errors = new { message = authResponse.ErrorMessage } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user: {UserName}", user?.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        [HttpPost("ForgotPassword")]
        public ActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required.");

            var otp = _authenticateRepository.GenerateOtp(request.Email);
            if (otp == null)
                return BadRequest("User not found.");

            _authenticateRepository.SendOtpEmail(request.Email, otp);
            return Ok("OTP has been sent to your email.");
        }

        [HttpPost("VerifyOtp")]
        public ActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
                return BadRequest("Email and OTP are required.");

            var isValid = _authenticateRepository.VerifyOtp(request.Email, request.Otp);
            if (!isValid)
                return BadRequest("Invalid or expired OTP.");

            return Ok("OTP verified successfully.");
        }

        [HttpPost("ResetPassword")]
        public ActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword) || string.IsNullOrEmpty(request.ConfirmPassword))
                return BadRequest("Email, new password, and confirmation password are required.");

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("New password and confirmation password do not match.");

            var result = _authenticateRepository.ResetPassword(request.Email, request.NewPassword);
            if (!result)
                return BadRequest("Failed to reset password. User may not exist.");

            return Ok("Password reset successfully.");
        }
    }
}