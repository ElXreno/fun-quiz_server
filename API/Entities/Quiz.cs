using API.Entities.Dto.New;

namespace API.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public User CreatedBy { get; set; }
    public bool IsPublic { get; set; }

    public ICollection<QuizStage> QuizStages { get; set; } = new List<QuizStage>();
    // public IEnumerable<QuizTeam> QuizTeams { get; set; } = new List<QuizTeam>();

    public QuizDtoNew ToDto()
    {
        return new QuizDtoNew
        {
            Id = Id,
            Name = Name
        };
    }
}