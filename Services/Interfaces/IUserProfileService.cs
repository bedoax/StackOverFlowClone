using StackOverFlowClone.Models.DTOs.User;
using StackOverFlowClone.Models.Entities;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserDto> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
