using StackOverFlowClone.Models.DTOs.LoginAndRegister;
using StackOverFlowClone.Models.DTOs.User;

namespace StackOverFlowClone.Services.Interfaces
{
    public interface IUserAccountService
    {
        Task<UserDto> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto); // Returns JWT token
        Task<bool> DeleteUserAsync(int userId);
    }
}
