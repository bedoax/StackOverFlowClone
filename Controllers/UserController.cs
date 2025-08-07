using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.User;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Implementations;
using StackOverFlowClone.Services.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

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
    public UserController(IUserAccountService userAccountService,IUserProfileService userProfileService,AppDbContext context, IUserModerationService userModerationService,UserSettingsService userSettingsService, IConfiguration configuration)
    {
        _userAccountService = userAccountService;
        _userProfileService = userProfileService;
        _userModerationService = userModerationService;
        _userSettingsService = userSettingsService;
        _context = context;
        _configuration = configuration;

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
    [HttpPost("send")]
    public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email is required" });

        var otp =_userSettingsService.GenerateOtp();
        await _userSettingsService.SendOtpEmailAsync(request.Email, otp);

        return Ok(new { message = "OTP sent successfully" });
    }

/*    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(0, 1000000).ToString("D6");
    }

    private async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var fromEmail = _configuration["Email"];
        var fromPassword = _configuration["pass"];

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, "MyApp"),
            Subject = "Your OTP Code",
            Body = $"Your One-Time Password (OTP) is: {otp}",
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(fromEmail, fromPassword)
        };

        await smtp.SendMailAsync(message);
    }*/
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