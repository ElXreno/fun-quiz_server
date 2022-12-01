using System.Security.Claims;
using API.Context;
using API.Entities;
using API.Entities.Addons;
using API.Entities.Dto.New;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.Moderator}, {UserRoles.User}")]
[Route("api/[controller]")]
[ApiController]
public class NewQuizController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly UserManager<User> _userManager;

    public NewQuizController(ApiDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("getUserQuizzes")]
    public async Task<ActionResult<IEnumerable<QuizDtoNew>>> GetQuizzes()
    {
        if (_context.Quizzes == null) return NotFound();

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quizzes = await _context.Quizzes
            .Where(q => q.CreatedBy == user)
            .Include(q => q.CreatedBy)
            .Select(q => q.ToDto())
            .ToListAsync();

        return quizzes;
    }

    [HttpGet("getPublicQuizzes")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<QuizDtoNew>>> GetPublicQuizzes()
    {
        if (_context.Quizzes == null) return NotFound();

        var quizzes = await _context.Quizzes
            .Where(q => q.IsPublic)
            .Include(q => q.CreatedBy)
            .Select(q => q.ToDto())
            .ToListAsync();

        return quizzes;
    }

    [HttpPost("createQuiz")]
    public async Task<ActionResult<QuizDtoNew>> PostQuiz(QuizAddDtoNew quizAddDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (_context.Quizzes == null) return Problem("Entity set 'ApiDbContext.Quizzes' is null.");

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quiz = quizAddDto.ToEntity(user);

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuizzes), new { id = quiz.Id }, quiz.ToDto());
    }

    [HttpPut("updateQuiz/{id:int}")]
    public async Task<IActionResult> PutQuiz(int id, QuizAddDtoNew quizAddDto)
    {
        if (_context.Quizzes == null) return Problem("Entity set 'ApiDbContext.Quizzes' is null.");

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quiz = await _context.Quizzes.FindAsync(id);

        if (quiz == null) return NotFound();

        if (quiz.CreatedBy != user) return Forbid();

        quiz.Name = quizAddDto.Name;
        quiz.IsPublic = quizAddDto.IsPublic;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!QuizExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("deleteQuiz/{id:int}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        if (_context.Quizzes == null) return Problem("Entity set 'ApiDbContext.Quizzes' is null.");

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quiz = await _context.Quizzes.FindAsync(id);

        if (quiz == null) return NotFound();

        if (quiz.CreatedBy != user) return Forbid();

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool QuizExists(int id)
    {
        return _context.Quizzes != null && _context.Quizzes.Any(e => e.Id == id);
    }
}