using System.ComponentModel.DataAnnotations;
using API.Entities.Addons;

namespace API.Entities.Dto.New;

public class QuizQuestionsAddDtoNew
{
    [StringLength(40, MinimumLength = 4)]
    public string Question { get; set; }

    [EnumDataType(typeof(QuizAnswerTypes))]
    public string RequiredAnswerType { get; set; }
    [MinLength(1)]
    public string[] RightAnswers { get; set; }
    [MinLength(1)]
    public string[] WrongAnswers { get; set; }

    public QuizQuestion ToEntity()
    {
        return new QuizQuestion
        {
            Question = Question,
            RequiredAnswerType = Enum.Parse<QuizAnswerTypes>(RequiredAnswerType),
            RightAnswers = RightAnswers,
            WrongAnswers = WrongAnswers
        };
    }
}