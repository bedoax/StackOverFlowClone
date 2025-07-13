using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Auth;
using StackOverFlowClone.Models.DTOs.LoginAndRegister;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace StackOverFlowClone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public AuthController(UserManager<User> userManager, ITokenService tokenService,AppDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");
            
            var token = await _tokenService.GenerateTokensAsync(user); // تعديل هنا


            var expiredTokens = _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow);

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();

            return Ok(new { token });
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            if (principal == null)
                return Unauthorized("Invalid access token");

            var userId = principal.FindFirstValue("UserId");
            if (userId == null)
                return Unauthorized("Invalid access token");

            var user = await _context.Users.Include(u => u.RefreshTokens)
                                           .FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            var refreshToken = user.RefreshTokens
                 .OrderByDescending(r => r.ExpiresAt)
                 .FirstOrDefault(r => r.Token == model.RefreshToken);

            if (user == null || refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
                    return Unauthorized("Invalid refresh token");

            var role = principal.FindFirstValue(ClaimTypes.Role) ?? "user";
            var permissions =   _tokenService.GetPermissionsForRole(role );
            var newAccessToken = _tokenService.GenerateAccessToken(user, permissions,role);
            var newRefreshToken = _tokenService.GenerateSecureRefreshToken();

            // Save new refresh token
            refreshToken.Token = newRefreshToken;
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });

        }

    }
}
