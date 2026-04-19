namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.Authenticate(request.Email, request.Password);
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Invalid credentials");
        }

        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<ActionResult<bool>> Register([FromBody] RegisterRequest request)
    {
        var success = await _authService.Register(request.Email, request.UserName, request.Password);
        if (!success)
        {
            return BadRequest("User already exists");
        }

        return Ok(success);
    }
}
