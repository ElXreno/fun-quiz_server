namespace API.Entities.Dto;

public class UserRegisterDto
{
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PasswordConfirm { get; set; }

    public User ToEntity()
    {
        return new User
        {
            UserName = UserName,
            DisplayName = UserName,
            Email = Email
        };
    }
}