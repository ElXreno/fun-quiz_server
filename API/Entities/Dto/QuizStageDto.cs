namespace API.Entities.Dto;

public class QuizStageDto
{
    public int Id { get; set; }
    public int ScorePerQuestion { get; set; }
    public ICollection<QuizQuestionDto> QuizQuestions { get; set; } = new List<QuizQuestionDto>();

    public QuizStage ToEntity()
    {
        return new QuizStage
        {
            Id = Id,
            ScorePerQuestion = ScorePerQuestion,
            QuizQuestions = QuizQuestions.Select(qq => qq.ToEntity()).ToList()
        };
    }
}