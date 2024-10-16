using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[Authorize] // This will protect all actions in this controller
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("GetUserData")]
    [Produces("application/json")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(string))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(string))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success - JWT token will be returned", typeof(string))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetUserData()
    {
        // This endpoint will only be accessible with a valid JWT token
        return Ok(new { Message = "Authorized user data access!" });
    }

    [HttpGet("admin-tasks")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminTasks()
    {
        return Ok("This is an admin-only task.");
    }

    [AllowAnonymous] // This endpoint can be accessed without a token
    [HttpGet("PublicEndpoint")]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { Message = "This is a public endpoint!" });
    }
}
