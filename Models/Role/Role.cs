using Microsoft.AspNetCore.Identity;
using StackOverFlowClone.Models.Entities;

namespace StackOverFlowClone.Models.Role
{
    public class Role : IdentityRole<int>  // Inheriting from IdentityRole<int> for custom roles
    {
        // You can add additional properties for roles if necessary
        public string Description { get; set; }  // Example custom property (optional)


    }
}
