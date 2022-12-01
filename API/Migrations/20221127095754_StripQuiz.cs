using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    public partial class StripQuiz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_questions_quiz_stages_id_quiz_stages_navigation_id",
                table: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "quiz_stages");

            migrationBuilder.DropTable(
                name: "quiz_teams");

            migrationBuilder.DropColumn(
                name: "id_quiz_stages",
                table: "quiz_questions");

            migrationBuilder.RenameColumn(
                name: "id_quiz_stages_navigation_id",
                table: "quiz_questions",
                newName: "quiz_id");

            migrationBuilder.RenameIndex(
                name: "ix_quiz_questions_id_quiz_stages_navigation_id",
                table: "quiz_questions",
                newName: "ix_quiz_questions_quiz_id");

            migrationBuilder.AddColumn<int>(
                name: "id_quiz",
                table: "quiz_questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_questions_quizzes_quiz_id",
                table: "quiz_questions",
                column: "quiz_id",
                principalTable: "quizzes",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_questions_quizzes_quiz_id",
                table: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "id_quiz",
                table: "quiz_questions");

            migrationBuilder.RenameColumn(
                name: "quiz_id",
                table: "quiz_questions",
                newName: "id_quiz_stages_navigation_id");

            migrationBuilder.RenameIndex(
                name: "ix_quiz_questions_quiz_id",
                table: "quiz_questions",
                newName: "ix_quiz_questions_id_quiz_stages_navigation_id");

            migrationBuilder.AddColumn<int>(
                name: "id_quiz_stages",
                table: "quiz_questions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "quiz_stages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_quizzes_navigation_id = table.Column<int>(type: "integer", nullable: true),
                    id_quizzes = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    score_per_question = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_stages", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_stages_quizzes_id_quizzes_navigation_id",
                        column: x => x.id_quizzes_navigation_id,
                        principalTable: "quizzes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_teams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_quizzes_navigation_id = table.Column<int>(type: "integer", nullable: true),
                    id_quizzes = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_teams", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_teams_quizzes_id_quizzes_navigation_id",
                        column: x => x.id_quizzes_navigation_id,
                        principalTable: "quizzes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_quiz_questions_navigation_id = table.Column<int>(type: "integer", nullable: true),
                    id_quiz_teams_navigation_id = table.Column<int>(type: "integer", nullable: true),
                    answer = table.Column<string>(type: "text", nullable: false),
                    id_quiz_questions = table.Column<int>(type: "integer", nullable: true),
                    id_quiz_teams = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_answers_quiz_questions_id_quiz_questions_navigation_id",
                        column: x => x.id_quiz_questions_navigation_id,
                        principalTable: "quiz_questions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_quiz_answers_quiz_teams_id_quiz_teams_navigation_id",
                        column: x => x.id_quiz_teams_navigation_id,
                        principalTable: "quiz_teams",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_quiz_answers_id_quiz_questions_navigation_id",
                table: "quiz_answers",
                column: "id_quiz_questions_navigation_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_answers_id_quiz_teams_navigation_id",
                table: "quiz_answers",
                column: "id_quiz_teams_navigation_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_stages_id_quizzes_navigation_id",
                table: "quiz_stages",
                column: "id_quizzes_navigation_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_teams_id_quizzes_navigation_id",
                table: "quiz_teams",
                column: "id_quizzes_navigation_id");

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_questions_quiz_stages_id_quiz_stages_navigation_id",
                table: "quiz_questions",
                column: "id_quiz_stages_navigation_id",
                principalTable: "quiz_stages",
                principalColumn: "id");
        }
    }
}
