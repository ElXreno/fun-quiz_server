using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    public partial class QuizStage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_questions_quizzes_quiz_id",
                table: "quiz_questions");

            migrationBuilder.RenameColumn(
                name: "quiz_id",
                table: "quiz_questions",
                newName: "quiz_stage_id");

            migrationBuilder.RenameIndex(
                name: "ix_quiz_questions_quiz_id",
                table: "quiz_questions",
                newName: "ix_quiz_questions_quiz_stage_id");

            migrationBuilder.CreateTable(
                name: "quiz_stages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    score_per_question = table.Column<int>(type: "integer", nullable: false),
                    quiz_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_stages", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_stages_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_quiz_stages_quiz_id",
                table: "quiz_stages",
                column: "quiz_id");

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_questions_quiz_stages_quiz_stage_id",
                table: "quiz_questions",
                column: "quiz_stage_id",
                principalTable: "quiz_stages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_questions_quiz_stages_quiz_stage_id",
                table: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quiz_stages");

            migrationBuilder.RenameColumn(
                name: "quiz_stage_id",
                table: "quiz_questions",
                newName: "quiz_id");

            migrationBuilder.RenameIndex(
                name: "ix_quiz_questions_quiz_stage_id",
                table: "quiz_questions",
                newName: "ix_quiz_questions_quiz_id");

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_questions_quizzes_quiz_id",
                table: "quiz_questions",
                column: "quiz_id",
                principalTable: "quizzes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
