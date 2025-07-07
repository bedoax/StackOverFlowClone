namespace StackOverFlowClone.Models.DTOs.Question
{
    public class CreateQuestionDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public ICollection<string> Tags { get; set; } = new List<string>();
    }

}
