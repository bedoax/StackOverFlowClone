using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackOverFlowClone.Models.Entities
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Body { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        // Navigation property
        public User User { get; set; }

        public ICollection<Answer> Answers { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Vote> Votes { get; set; }
        // الجديد هنا
        public ICollection<QuestionTag> QuestionTags { get; set; }
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}
