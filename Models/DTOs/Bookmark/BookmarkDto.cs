namespace StackOverFlowClone.Models.DTOs.Bookmark
{
    public class BookmarkDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string QuestionTitle { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
