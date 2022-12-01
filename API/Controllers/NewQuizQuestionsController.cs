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
public class NewQuizQuestionsController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly UserManager<User> _userManager;

    public NewQuizQuestionsController(ApiDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("getQuizQuestionsByQuizId/{quizId:int}")]
    public async Task<ActionResult<IEnumerable<QuizQuestionsDtoNew>>> GetQuizQuestions(int quizId)
    {
        if (_context.Quizzes == null) return NotFound();

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quiz = await _context.Quizzes
            .Where(q => q.Id == quizId && q.CreatedBy == user)
            .Include(q => q.QuizStages)
            .FirstOrDefaultAsync();

        if (quiz == null) return NotFound();

        var quizQuestions = quiz.QuizStages
            .Select(q => q.ToDto())
            .ToList();

        return Ok(quizQuestions);
    }

    [HttpPost("addQuizQuestions/{quizId:int}/{stageId:int}")]
    public async Task<ActionResult<QuizQuestionsDtoNew>> AddQuizQuestions(int quizId, int stageId,
        IEnumerable<QuizQuestionsAddDtoNew> listQuizQuestionsDto)
    {
        if (_context.Quizzes == null) return NotFound();
        if (_context.QuizQuestions == null) return NotFound();

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quiz = await _context.Quizzes
            .Where(q => q.Id == quizId && q.CreatedBy == user)
            .Include(q => q.QuizStages)
            .FirstOrDefaultAsync();

        if (quiz == null) return NotFound();

        var quizStage = quiz.QuizStages.FirstOrDefault(qs => qs.Id == stageId);

        if (quizStage == null) return NotFound();

        foreach (var quizQuestion in listQuizQuestionsDto.Select(q => q.ToEntity()))
        {
            quizStage.QuizQuestions.Add(quizQuestion);
        }

        _context.Entry(quiz).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return Ok(quiz.QuizStages.Select(q => q.ToDto()));
    }

    [HttpPut("updateQuizQuestion/{quizId:int}/{stageId:int}/{id:int}")]
    public async Task<ActionResult<QuizQuestionsDtoNew>> UpdateQuizQuestion(int quizId, int stageId, int id, QuizQuestionsAddDtoNew quizQuestionDto)
    {
        if (_context.Quizzes == null) return NotFound();
        if (_context.QuizQuestions == null) return NotFound();

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quiz = await _context.Quizzes
            .Where(q => q.Id == quizId && q.CreatedBy == user)
            .Include(q => q.QuizStages)
            .FirstOrDefaultAsync();

        if (quiz == null) return NotFound();
        
        var quizStage = quiz.QuizStages.FirstOrDefault(qs => qs.Id == stageId);

        if (quizStage == null) return NotFound();

        var quizQuestion = quizStage.QuizQuestions
            .FirstOrDefault(q => q.Id == id);

        if (quizQuestion == null) return NotFound();

        quizQuestion.Question = quizQuestionDto.Question;
        quizQuestion.RequiredAnswerType = Enum.Parse<QuizAnswerTypes>(quizQuestionDto.RequiredAnswerType);
        quizQuestion.RightAnswers = quizQuestionDto.RightAnswers;
        quizQuestion.WrongAnswers = quizQuestionDto.WrongAnswers;

        _context.Entry(quizQuestion).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return Ok(quizQuestion.ToDto());
    }

    [HttpDelete("deleteQuizQuestion/{quizId:int}/{stageId:int}/{id:int}")]
    public async Task<ActionResult<QuizQuestionsDtoNew>> DeleteQuizQuestion(int quizId, int stageId, int id)
    {
        if (_context.Quizzes == null) return NotFound();
        if (_context.QuizQuestions == null) return NotFound();

        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) return NotFound();

        var quiz = await _context.Quizzes
            .Where(q => q.Id == quizId && q.CreatedBy == user)
            .Include(q => q.QuizStages)
            .FirstOrDefaultAsync();

        if (quiz == null) return NotFound();
        
        var quizStage = quiz.QuizStages.FirstOrDefault(qs => qs.Id == stageId);

        if (quizStage == null) return NotFound();

        var quizQuestion = quizStage.QuizQuestions
            .FirstOrDefault(q => q.Id == id);

        if (quizQuestion == null) return NotFound();

        _context.QuizQuestions.Remove(quizQuestion);
        await _context.SaveChangesAsync();

        return Ok(quizQuestion.ToDto());
    }
}