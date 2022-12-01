namespace API.Entities.Dto.New;

public class QuizDtoNew
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Quiz ToEntity(User createdBy, bool isPublic)
    {
        return new Quiz
        {
            Id = Id,
            Name = Name,
            CreatedBy = createdBy,
            IsPublic = isPublic
        };
    }
}