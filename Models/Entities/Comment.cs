using StackOverFlowClone.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StackOverFlowClone.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        public string Body { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; }

        [Required]
        public TargetType TargetType { get; set; }

        [Required]
        public int TargetId { get; set; }
    }

}

