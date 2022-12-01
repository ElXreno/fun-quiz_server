using API.Entities.Addons;

namespace API.Entities.Dto;

public class QuizQuestionDto
{
    public int Id { get; set; }
    public string Question { get; set; } = null!;
    public string RequiredAnswerType { get; set; } = null!;
    public string[] RightAnswers { get; set; } = null!;
    public string[] WrongAnswers { get; set; } = null!;

    public QuizQuestion ToEntity()
    {
        return new QuizQuestion
        {
            Id = Id,
            Question = Question,
            RequiredAnswerType = Enum.Parse<QuizAnswerTypes>(RequiredAnswerType),
            RightAnswers = RightAnswers,
            WrongAnswers = WrongAnswers
        };
    }
}