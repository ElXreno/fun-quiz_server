using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Context;
using API.Entities;
using API.Entities.Addons;
using API.Entities.Dto;
using API.Entities.Dto.New;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

[ApiController] // Allows us to use the [FromBody] attribute without having to specify it on each method
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;

    private readonly ApiDbContext _context;
    private readonly UserManager<User> _userManager;

    public AccountController(IConfiguration configuration, UserManager<User> userManager, ApiDbContext context)
    {
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("getPasswordRequirements")]
    public PasswordOptions GetPasswordRequirements()
    {
        return _userManager.Options.Password;
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("getUsers")]
    public async Task<ActionResult<IEnumerable<UserInfoDto>>> GetUsers()
    {
        var users = await _context.Users.Select(u =>
            u.ToUserInfoDto(new List<string?>())
        ).ToListAsync();
        return Ok(users);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("updateUser/{id}")]
    public async Task<ActionResult> UpdateUser(string id, UserInfoDto userInfoDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (userInfoDto.Id != id)
        {
            return BadRequest();
        }

        user.UserName = userInfoDto.UserName;
        user.DisplayName = userInfoDto.DisplayName;
        user.Email = userInfoDto.Email;

        foreach (var role in userInfoDto.Roles)
        {
            // check if all roles are valid
            if (!UserRoles.AllRoles.Contains(role))
            {
                return BadRequest();
            }

            // remove from role if not in dto
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            // add to role if not in db
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(result.Errors);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("deleteUser/{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded) return Ok();
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(UserLoginDto userLoginDto)
    {
        var user = await _userManager.FindByEmailAsync(userLoginDto.Email);

        if (user == null) return Unauthorized();

        var result = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

        if (!result) return Unauthorized();

        var authClaims = await GetClaims(user);

        var (tokens, expires) = GenerateTokens(authClaims);

        _context.UserTokens.Add(new IdentityUserToken<string>
        {
            UserId = user.Id,
            Name = $"RefreshToken:{expires}", // expire in 200 days
            Value = tokens.RefreshToken,
            LoginProvider = "FunQuizAPI"
        });
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Tokens = tokens
        };
    }

    // [Authorize]
    // [HttpPost("loginViaToken")]
    // public async Task<ActionResult<UserDto>> LoginViaTokens()
    // {
    //     var email = User.FindFirstValue(ClaimTypes.Email);
    //
    //     var user = await _userManager.FindByEmailAsync(email);
    //
    //     if (user == null) return NotFound();
    //
    //     var authClaims = await GetClaims(user);
    //     
    //     var (tokens, expires) = GenerateTokens(authClaims);
    //     
    //     _context.UserTokens.Add(new IdentityUserToken<string>
    //     {
    //         UserId = user.Id,
    //         Name = $"RefreshToken:{expires}", // expire in 200 days
    //         Value = tokens.RefreshToken,
    //         LoginProvider = "FunQuizAPI"
    //     });
    //     await _context.SaveChangesAsync();
    //
    //     return new UserDto
    //     {
    //         Id = user.Id,
    //         UserName = user.UserName,
    //         DisplayName = user.DisplayName,
    //         Email = user.Email,
    //         Tokens = tokens
    //     };
    // }

    [HttpPost("register")]
    public async Task<ActionResult<UserRegisterDto>> Register(UserRegisterDto userRegisterDto)
    {
        if (_userManager.FindByNameAsync(userRegisterDto.UserName).Result != null)
            return BadRequest("Username is taken");

        if (_userManager.FindByEmailAsync(userRegisterDto.Email).Result != null) return BadRequest("Email is taken");

        if (userRegisterDto.Password != userRegisterDto.PasswordConfirm) return BadRequest("Passwords do not match");

        var user = userRegisterDto.ToEntity();

        var result = await _userManager.CreateAsync(user, userRegisterDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        // if user is first user, make them admin and moderator
        if (_userManager.Users.Count() == 1)
        {
            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            await _userManager.AddToRoleAsync(user, UserRoles.Moderator);
        }

        await _userManager.AddToRoleAsync(user, UserRoles.User);

        var authClaims = await GetClaims(user);

        var (tokens, expires) = GenerateTokens(authClaims);

        _context.UserTokens.Add(new IdentityUserToken<string>
        {
            UserId = user.Id,
            Name = $"RefreshToken:{expires}",
            Value = tokens.RefreshToken,
            LoginProvider = "FunQuizAPI"
        });
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost("registerAdmin")]
    public async Task<ActionResult<UserRegisterDto>> RegisterAdmin(UserRegisterDto userRegisterDto)
    {
        var result = await Register(userRegisterDto);

        if (result.Result is not OkResult) return result;

        var user = await _userManager.FindByNameAsync(userRegisterDto.UserName);
        await _userManager.AddToRoleAsync(user, UserRoles.Admin);

        return result;
    }

    [HttpPost("refreshToken")]
    public async Task<ActionResult<TokensDto>> RefreshToken(TokensDto tokensDto)
    {
        var oldRefreshToken = _context.UserTokens
            .FirstOrDefault(x => x.Value == tokensDto.RefreshToken);

        if (oldRefreshToken == null) return Unauthorized("Invalid refresh token");

        var user = await _userManager.FindByIdAsync(oldRefreshToken.UserId);

        if (user == null) return Unauthorized("Invalid token");

        var authClaims = await GetClaims(user);

        var (tokens, expires) = GenerateTokens(authClaims);

        _context.UserTokens.Remove(oldRefreshToken);
        _context.UserTokens.Add(new IdentityUserToken<string>
        {
            UserId = user.Id,
            Name = $"RefreshToken:{expires}",
            Value = tokens.RefreshToken,
            LoginProvider = "FunQuizAPI"
        });

        await _context.SaveChangesAsync();

        return tokens;
    }

    private async Task<IEnumerable<Claim>> GetClaims(User user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

        return claims;
    }

    private (JwtSecurityToken, long) GenerateJwtToken(IEnumerable<Claim> authClaims)
    {
        var authSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException()));

        var expires = DateTime.Now.AddMinutes(int.Parse(_configuration["JWT:TokenExpirationInMinutes"] ?? "30"));

        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"],
            expires: expires,
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return (token, ((DateTimeOffset)expires).ToUnixTimeSeconds());
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private (TokensDto, long) GenerateTokens(IEnumerable<Claim> authClaims)
    {
        var (token, expires) = GenerateJwtToken(authClaims);
        return (new TokensDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = GenerateRefreshToken()
        }, expires);
    }
}