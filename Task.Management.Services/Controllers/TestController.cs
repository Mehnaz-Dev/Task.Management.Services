using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;

namespace Task.Management.Services.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("GetTest")]
        [Produces("application/json")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetTest()
        {
            string bearerToken = Request.Headers[HeaderNames.Authorization].ToString();
            return Ok(new { message = "Authorization is working!" });
        }
    }
}
