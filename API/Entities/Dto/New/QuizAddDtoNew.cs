using System.ComponentModel.DataAnnotations;

namespace API.Entities.Dto.New;

public class QuizAddDtoNew
{
    [StringLength(40, MinimumLength = 4)]
    public string Name { get; set; }
    public bool IsPublic { get; set; }

    public Quiz ToEntity(User user)
    {
        return new Quiz
        {
            Name = Name,
            CreatedBy = user,
            IsPublic = IsPublic
        };
    }
}