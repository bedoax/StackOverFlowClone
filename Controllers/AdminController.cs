using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverFlowClone.Models.DTOs.User;
using StackOverFlowClone.Services.Interfaces;

 // 🔁 استبدال Roles ب Policy مناسبة
[Route("api/admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IUserModerationService _moderation;

    public AdminController(IUserModerationService moderation)
    {
        _moderation = moderation;
    }

    [Authorize(Policy = "CanBanUser")]
    [HttpPut("ban/{userId}")]
    public async Task<IActionResult> BanUser(int userId, BanUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _moderation.BanUserAsync(userId, dto.BanReason, dto.BannedUntil);
        return result ? NoContent() : NotFound();
    }

    [Authorize(Policy = "CanBanUser")]
    [HttpPut("unban/{userId}")]
    public async Task<IActionResult> UnbanUser(int userId)
    {
        var result = await _moderation.UnbanUserAsync(userId);
        return result ? NoContent() : NotFound();
    }

    [Authorize(Policy = "CanManagePermissions")]
    [HttpPut("promote/{userId}")]
    public async Task<IActionResult> PromoteToModerator(int userId)
    {
        var result = await _moderation.PromoteToModeratorAsync(userId);
        return result ? NoContent() : NotFound();
    }

    [Authorize(Policy = "CanManagePermissions")]
    [HttpPut("demote/{userId}")]
    public async Task<IActionResult> DemoteFromModerator(int userId)
    {
        var result = await _moderation.DemoteFromModeratorAsync(userId);
        return result ? NoContent() : NotFound();
    }

    [Authorize(Policy = "CanViewReports")]
    [HttpGet("isbanned/{userId}")]
    public async Task<IActionResult> IsUserBanned(int userId)
    {
        var result = await _moderation.IsUserBannedAsync(userId);
        return Ok(result);
    }
}
