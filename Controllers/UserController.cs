using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs;
using StackOverFlowClone.Models.DTOs.User;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Implementations;
using StackOverFlowClone.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

[Route("api/users")]
[ApiController]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserAccountService _userAccountService;
    private readonly IUserProfileService _userProfileService;
    private readonly IUserModerationService _userModerationService;
    private readonly AppDbContext _context;
    private readonly UserSettingsService _userSettingsService;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    public UserController(IUserAccountService userAccountService,IUserProfileService userProfileService, AppDbContext context, IUserModerationService userModerationService, UserSettingsService userSettingsService, IConfiguration configuration, ITokenService tokenService, UserManager<User> userManager)
    {
        _userAccountService = userAccountService;
        _userProfileService = userProfileService;
        _userModerationService = userModerationService;
        _userSettingsService = userSettingsService;
        _tokenService = tokenService;
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
    }

    // --- User Profile Endpoints ---

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(int pageNumer , int size)
    {
        var users = await _context.Users.Skip((pageNumer-1)*size).Take(size).Select(user => new
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
        }).ToListAsync();
        return Ok(users);
    }

    [HttpGet("id/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userProfileService.GetUserByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userProfileService.GetUserByEmailAsync(email);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize (Policy = "UpdateUserProfile")]
    public async Task<IActionResult> UpdateUserProfile(
        [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        int userId = GetUserIdFromClaims();
        var result = await _userProfileService.UpdateUserProfileAsync(userId, updateUserDto);
        return result ? NoContent() : NotFound();
    }
    [Authorize(Roles = "user")]
    [HttpPut("{userId}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        int userId = GetUserIdFromClaims();
        var result = await _userProfileService.ChangePasswordAsync(
            userId,
            changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword);

        return result ? NoContent() : BadRequest("Password change failed.");
    }


    [HttpGet("{userId}/activity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserActivity(int userId)
    {
        if (userId <= 0)
            return BadRequest("Invalid user ID.");

        var activity = await _userModerationService.GetUserActivityAsync(userId);
        return activity == null ? NotFound() : Ok(activity);
    }


    [HttpGet("{userId}/reputation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReputation()
    {
        int userId = GetUserIdFromClaims();
        if (userId <= 0)
            return BadRequest("Invalid user ID.");

        var reputation = await _userModerationService.GetReputationAsync(userId);
        return Ok(reputation);
    }
    // --- otp Endpoints ---
    [HttpPost("send")]
    public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email is required" });

        var otp =_userSettingsService.GenerateOtp();
        await _userSettingsService.SendOtpEmailAsync(request.Email, otp);

       var isitExist =  _context.Otps.Any(o => o.Email == request.Email);
        if (isitExist)
        {

            var existingOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == request.Email);
            _context.Otps.Remove(existingOtp);
            _context.SaveChangesAsync();
        }
        _context.Otps.Add(new Otp
        {
            Email = request.Email,
            OtpCode = otp,

        });
        _context.SaveChangesAsync();
        return Ok(new { message = "OTP sent successfully" });
    }

    [HttpGet("{otp}")]
    public async Task<IActionResult> VerifyOtp(string otp)
    {
        if (string.IsNullOrWhiteSpace(otp))
            return BadRequest(new { error = "OTP is required" });
        var existingOtp = await _context.Otps.FirstOrDefaultAsync(o => o.OtpCode == otp);
        if (existingOtp == null)
            return NotFound(new { error = "Invalid OTP" });
        // Optionally, you can remove the OTP after successful verification
        var userInfo = await _context.Users.FirstOrDefaultAsync(u => u.Email == existingOtp.Email);
        _context.Otps.Remove(existingOtp);
        await _context.SaveChangesAsync();
        
        if (userInfo == null)
            return NotFound(new { error = "User not found" });
        var token = await _tokenService.GenerateTokensAsync(userInfo);
       
        return Ok(new { message = "OTP verified successfully \n token Access : " + token.AccessToken  + "\n RefreshToken : " + token.RefreshToken});
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
            return BadRequest(new { error = "Token and new password are required." });

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:secretKey"]);

        try
        {
            var principal = handler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Invalid token - no email." });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { error = "User not found." });

            // Generate a reset token from Identity
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the password using Identity's logic
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { error = "Password reset failed", details = result.Errors });

            return Ok(new { message = "Password reset successfully." });
        }
        catch (Exception)
        {
            return BadRequest(new { error = "Invalid or expired token." });
        }
    }


    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return int.Parse(userIdClaim.Value);
    }
}