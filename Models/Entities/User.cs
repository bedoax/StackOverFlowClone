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
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        // to make design which if user can open from browser or phone etc
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

}