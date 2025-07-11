using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.User;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
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

    public UserController(IUserAccountService userAccountService,IUserProfileService userProfileService,AppDbContext context)
    {
        _userAccountService = userAccountService;
        _userProfileService = userProfileService;

        _context = context;
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

    [Authorize(Roles = "user")]
    [HttpGet("{userId}/activity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserActivity()
    {
        int userId = GetUserIdFromClaims();
        if (userId <= 0)
            return BadRequest("Invalid user ID.");

        var activity = await _userModerationService.GetUserActivityAsync(userId);
        return activity == null ? NotFound() : Ok(activity);
    }
    [Authorize(Roles = "user")]
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