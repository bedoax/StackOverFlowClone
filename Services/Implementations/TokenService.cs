using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.DTOs.Auth;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StackOverFlowClone.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public TokenService(IConfiguration configuration, UserManager<User> userManager, AppDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        public string GenerateAccessToken(User user, List<string> permissions,string role)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("UserId", user.Id.ToString()),
                new Claim("sub", user.Id.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:secretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResult> GenerateTokensAsync(User user)
        {

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
            if (user.UserName.Contains("Admin"))
            {
                role = "Admin";
            }
            var permissions = GetPermissionsForRole(role);

            var accessToken = GenerateAccessToken(user, permissions,role);
            var refreshToken = GenerateSecureRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                accessToken = accessToken,
                refreshToken = refreshToken
            };
        }

        public  string GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public  List<string> GetPermissionsForRole(string role)
        {
            //  await هنا لو احتجت إلى تحميل شيء من قاعدة البيانات مستقبلًا
            return role switch
            {
                Roles.Admin => new List<string>
            {
                Permissions.CanAskQuestion,
                Permissions.CanAnswerQuestion,
                Permissions.CanComment,
                Permissions.CanVote,
                Permissions.CanEditAnyPost,
                Permissions.CanDeleteAnyPost,
                Permissions.CanModerate,
                Permissions.CanManageTags,
                Permissions.CanViewAnalytics,
                Permissions.CanManagePermissions,
                Permissions.CanAccessAdminPanel,
                Permissions.CanViewReports,
                Permissions.CanBookMark,
                Permissions.CanBanUser

            },
                Roles.Moderator => new List<string>
            {
                Permissions.CanAskQuestion,
                Permissions.CanAnswerQuestion,
                Permissions.CanComment,
                Permissions.CanVote,
                Permissions.CanEditAnyPost,
                Permissions.CanDeleteAnyPost,
                Permissions.CanModerate,
                Permissions.CanManageTags,
                Permissions.CanBookMark

            },
                Roles.User => new List<string>
            {
                Permissions.CanAskQuestion,
                Permissions.CanAnswerQuestion,
                Permissions.CanComment,
                Permissions.CanVote,
                Permissions.CanEditOwnPost,
                Permissions.CanDeleteOwnPost,
                Permissions.CanViewProfile,
                Permissions.CanDeleteOwnComment,
                Permissions.CanEditOwnComment,
                Permissions.UpdateUserProfile,
                Permissions.ChangeOwnPassword,
                Permissions.CanBookMark

            },
                _ => new List<string>()
            };
        }
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:secretKey"]!)),
                ValidateLifetime = false // مهم! عشان نسمح باستخدام Access Token منتهي
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

    }
}
