using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.User;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
using System.Security.Claims;

public class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public UserProfileService(AppDbContext context,UserManager<User>userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
                .Where(x => x.Id == id)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    userName = x.UserName,
                    passwordHash = x.PasswordHash
                })
                .FirstOrDefaultAsync();
        return  user;
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x=>x.Email == email);
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.UserName = updateUserDto.Username ?? user.UserName;
        user.Email = updateUserDto.Email ?? user.Email;
        await _context.SaveChangesAsync();
        return true;
    }

    // StackOverFlowClone.Services.Implementations/UserProfileService.cs
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        // Verify current password correctly
        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, currentPassword);
        if (!isPasswordValid) return false;

        // Set new password
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }



}
