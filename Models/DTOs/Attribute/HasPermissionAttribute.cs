using Microsoft.AspNetCore.Authorization;

namespace StackOverFlowClone.Models.DTOs.Attribute
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }
}
