using StackOverFlowClone.Models.Entities;
using System.Security.Claims;

namespace StackOverFlowClone.Services.Interfaces
{

    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
