namespace StackOverFlowClone.Models.DTOs.User
{
    public class UpdateUserDto
    {
        public int id {  get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string bio { get; set; }
        // add Ifileform to can user upload photo later 
    }
}
