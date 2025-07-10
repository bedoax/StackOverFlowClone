using StackOverFlowClone.Models.DTOs.Auth;
using StackOverFlowClone.Models.Entities;
using System.Security.Claims;

namespace StackOverFlowClone.Services.Interfaces
{

    public interface ITokenService
    {
        
        Task<AuthResult> GenerateTokensAsync(User user); // for make refresh token
        string GenerateAccessToken(User user, List<string> permissions, string role);
        string GenerateSecureRefreshToken();
        List<string> GetPermissionsForRole(string role);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);


    }
}
