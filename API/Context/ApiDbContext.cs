using API.Entities;
using API.Entities.Dto;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Context;

public class ApiDbContext : IdentityDbContext<User>
{
    public ApiDbContext()
    {
    }

    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Quiz>? Quizzes { get; set; }
    public virtual DbSet<QuizQuestion>? QuizQuestions { get; set; }
    public virtual DbSet<QuizStage>? QuizStages { get; set; }
}