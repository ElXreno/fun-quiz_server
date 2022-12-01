namespace API.Entities.Dto;

public class QuizDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CreatedBy { get; set; }
    public bool IsPublic { get; set; }

    public ICollection<QuizStageDto> QuizStages { get; set; } = new List<QuizStageDto>();
}