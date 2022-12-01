namespace API.Entities.Dto;

public class QuizAddDto
{
    public string Name { get; set; }
    public bool IsPublic { get; set; }

    public IEnumerable<QuizStageDto> QuizStages { get; set; } = new List<QuizStageDto>();
}