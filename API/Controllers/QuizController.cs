using System.Security.Claims;
using API.Context;
using API.Entities;
using API.Entities.Addons;
using API.Entities.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class QuizController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly UserManager<User> _userManager;

    public QuizController(ApiDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/Quiz
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizzes(bool @public = false)
    {
        if (_context.Quizzes == null) return NotFound();

        List<Quiz> quizzes;

        if (@public)
        {
            quizzes = await _context.Quizzes
                .Where(q => q.IsPublic == @public)
                .Include(q => q.CreatedBy)
                .Include(q => q.QuizStages)
                .ThenInclude(q => q.QuizQuestions)
                .ToListAsync();
        }
        else
        {
            // Get all quizzes by user
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            quizzes = await _context.Quizzes
                .Where(q => q.CreatedBy == user)
                .Include(q => q.QuizStages)
                .ThenInclude(q => q.QuizQuestions)
                .ToListAsync();
        }

        return quizzes.Select(q => new QuizDto
        {
            Id = q.Id,
            CreatedBy = q.CreatedBy.Id,
            Name = q.Name,
            IsPublic = q.IsPublic,
            QuizStages = _context.QuizStages
                .Where(qs => qs.QuizId == q.Id)
                .Select(qs => qs.ToDto())
                .ToList()
        }).ToList();
    }

    // GET: api/Quiz/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuizDto>> GetQuiz(int id)
    {
        if (_context.Quizzes == null) return NotFound();
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);
        var quiz = await _context.Quizzes
            .Where(q => q.CreatedBy == user)
            .Where(q => q.Id == id)
            .Include(q => q.QuizStages)
            .ThenInclude(q => q.QuizQuestions)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                Name = q.Name,
                CreatedBy = q.CreatedBy.Id,
                IsPublic = q.IsPublic,
                QuizStages = _context.QuizStages
                    .Where(qs => qs.QuizId == q.Id)
                    .Select(qs => qs.ToDto())
                    .ToList()
            }).FirstOrDefaultAsync();

        if (quiz == null) return NotFound();

        return new QuizDto
        {
            Id = quiz.Id,
            Name = quiz.Name,
            CreatedBy = quiz.CreatedBy,
            IsPublic = quiz.IsPublic
        };
    }

    // PUT: api/Quiz/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:int}")]
    public async Task<ActionResult<QuizDto>> PutQuiz(int id, QuizDto quizDto)
    {
        if (id != quizDto.Id) return BadRequest();

        if (_context.Quizzes == null) return NotFound();

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        var quiz = await _context.Quizzes
            .Where(q => q.CreatedBy == user)
            .Where(q => q.Id == id)
            .Include(q => q.QuizStages)
            .ThenInclude(q => q.QuizQuestions)
            .FirstOrDefaultAsync();

        if (quiz == null) return NotFound();

        if (quiz.CreatedBy.Id != quizDto.CreatedBy) return BadRequest();

        // var quizStages = _context.QuizStages
        //     .Where(qs => qs.QuizId == quiz.Id)
        //     .Include(qs => qs.QuizQuestions);
        //
        // // remove all quiz questions which are not in the quizAddDto
        // var quizQuestions = await quizStages
        //     .Where(qq => qq.QuizId == quiz.Id)
        //     .Select(qq => qq.QuizQuestions)
        //     .ToListAsync();
        //
        // _context.QuizQuestions.RemoveRange(quizQuestions);
        // await _context.SaveChangesAsync();

        quiz.Name = quizDto.Name;
        quiz.IsPublic = quizDto.IsPublic;
        quiz.QuizStages = quizDto.QuizStages.Select(qs => new QuizStage
        {
            Id = qs.Id,
            ScorePerQuestion = qs.ScorePerQuestion,
            QuizQuestions = qs.QuizQuestions.Select(qq => qq.ToEntity()).ToList()
        }).ToList();

        _context.Entry(quiz).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return new QuizDto
        {
            Id = quiz.Id,
            Name = quiz.Name,
            CreatedBy = quiz.CreatedBy.Id,
            IsPublic = quiz.IsPublic,
            QuizStages = quiz.QuizStages.Select(qs => qs.ToDto()).ToList()
        };
    }

    // POST: api/Quiz
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<QuizDto>> PostQuiz(QuizAddDto quizAddDto)
    {
        if (_context.Quizzes == null) return Problem("Entity set 'ApiDbContext.Quizzes'  is null.");

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return Problem("User not found.");

        var quiz = new Quiz
        {
            Name = quizAddDto.Name,
            CreatedBy = user,
            IsPublic = quizAddDto.IsPublic
        };
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        quiz.QuizStages = quizAddDto.QuizStages.Select(qs => qs.ToEntity()).ToList();

        _context.Entry(quiz).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return CreatedAtAction("GetQuiz", new { id = quiz.Id }, new QuizDto
        {
            Id = quiz.Id,
            Name = quiz.Name,
            CreatedBy = quiz.CreatedBy.Id,
            IsPublic = quiz.IsPublic,
            QuizStages = quiz.QuizStages.Select(qs => qs.ToDto()).ToList()
        });
    }

    // DELETE: api/Quiz/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        if (_context.Quizzes == null) return NotFound();
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);
        var quiz = await _context.Quizzes
            .Where(q => q.CreatedBy == user)
            .Where(q => q.Id == id)
            .Include(q => q.QuizStages)
            .ThenInclude(q => q.QuizQuestions)
            .FirstOrDefaultAsync();
        if (quiz == null) return NotFound();

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool QuizExists(int id)
    {
        return (_context.Quizzes?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}