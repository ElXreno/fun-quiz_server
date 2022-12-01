using API.Entities.Addons;
using API.Entities.Dto;
using API.Entities.Dto.New;

namespace API.Entities;

public sealed class QuizQuestion
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public QuizAnswerTypes RequiredAnswerType { get; set; }
    public string[] RightAnswers { get; set; } = Array.Empty<string>();
    public string[] WrongAnswers { get; set; } = Array.Empty<string>();

    public int QuizStageId { get; set; }
    public QuizStage QuizStage { get; set; }

    public QuizQuestionsDtoNew ToDtoNew()
    {
        return new QuizQuestionsDtoNew
        {
            Id = Id,
            Question = Question,
            RequiredAnswerType = RequiredAnswerType.ToString(),
            RightAnswers = RightAnswers,
            WrongAnswers = WrongAnswers
        };
    }

    public QuizQuestionDto ToDto()
    {
        return new QuizQuestionDto
        {
            Id = Id,
            Question = Question,
            RequiredAnswerType = RequiredAnswerType.ToString(),
            RightAnswers = RightAnswers,
            WrongAnswers = WrongAnswers
        };
    }
}