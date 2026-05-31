using System;
using System.Linq;
using System.Threading.Tasks;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.WebApi.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger,
            IWebHostEnvironment environment)
        {
            _authService = authService;
            _logger = logger;
            _environment = environment;
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login attempt with invalid model state for username: {Username}", loginDto.Username);
                return BadRequest(ModelState);
            }

            var token = await _authService.LoginAsync(loginDto);

            if (token == null)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", loginDto.Username);
                return Unauthorized("Invalid username or password.");
            }

            _logger.LogInformation("Successful login for username: {Username}", loginDto.Username);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(8)
            });

            return Ok(new { message = "Login successful", token = token });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var username = User.Identity?.Name
                ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            return Ok(new { username });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("Logout request received");
            Response.Cookies.Delete("jwt_token");
            return Ok(new { message = "Logout successful" });
        }
    }
}
