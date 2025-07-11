using StackOverFlowClone.Models.Enum;

namespace StackOverFlowClone.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public string Message { get; set; } 
        public string? Title { get; set; }
        public string? Url { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

 
        public int UserId { get; set; }
        public User User { get; set; } = null!;

    
        public NotificationType Type { get; set; }

       
        public int? TargetId { get; set; }
        public TargetType? TargetType { get; set; }
    }

}
