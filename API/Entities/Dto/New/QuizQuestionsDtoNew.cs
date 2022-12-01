namespace API.Entities.Dto.New;

public class QuizQuestionsDtoNew
{
    public int Id { get; set; }
    public string Question { get; set; }
    public string RequiredAnswerType { get; set; }
    public string[] RightAnswers { get; set; }
    public string[] WrongAnswers { get; set; }
}