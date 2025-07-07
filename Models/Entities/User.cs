using Microsoft.AspNetCore.Identity;
using StackOverFlowClone.Models.Role;

namespace StackOverFlowClone.Models.Entities
{
    public class User:IdentityUser<int>
    {

        public int Reputation { get; set; } = 0;
        public bool IsBanned { get; set; } = false;
        public DateTime? BannedUntil { get; set; }
        public string? BanReason { get; set; }
        //Navigation properties
        public ICollection<Question> Questions { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Vote> Votes { get; set; }
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    }

}