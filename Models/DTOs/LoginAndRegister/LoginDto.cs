namespace StackOverFlowClone.Models.DTOs.LoginAndRegister
{
    public class LoginDto  // ✅ Fixed typo from "LoginDtp"
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
