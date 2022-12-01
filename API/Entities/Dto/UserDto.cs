namespace API.Entities.Dto;

public class UserDto
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public TokensDto? Tokens { get; set; }
}