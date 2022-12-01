using API.Entities.Dto.New;
using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public sealed class User : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<Quiz> Quizzes { get; } = new List<Quiz>();

    public UserInfoDto ToUserInfoDto(IEnumerable<string?> roles)
    {
        return new UserInfoDto
        {
            Id = Id,
            DisplayName = DisplayName,
            Email = Email,
            UserName = UserName,
            Roles = roles
        };
    }
}