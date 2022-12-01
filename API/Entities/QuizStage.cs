using API.Entities.Dto;

namespace API.Entities;

public class QuizStage
{
    public int Id { get; set; }
    public int ScorePerQuestion { get; set; }
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; }

    public QuizStageDto ToDto()
    {
        return new QuizStageDto
        {
            Id = Id,
            ScorePerQuestion = ScorePerQuestion,
            QuizQuestions = QuizQuestions.Select(q => q.ToDto()).ToList()
        };
    }
}