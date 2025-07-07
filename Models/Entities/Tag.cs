using System.ComponentModel.DataAnnotations;

namespace StackOverFlowClone.Models.Entities;
public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public ICollection<QuestionTag> QuestionTags { get; set; }
}



