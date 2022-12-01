using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class Test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_questions_quizzes_quiz_id",
                table: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "id_quiz",
                table: "quiz_questions");

            migrationBuilder.AlterColumn<int>(
                name: "quiz_id",
                table: "quiz_questions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_questions_quizzes_quiz_id",
                table: "quiz_questions",
                column: "quiz_id",
                principalTable: "quizzes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_questions_quizzes_quiz_id",
                table: "quiz_questions");

            migrationBuilder.AlterColumn<int>(
                name: "quiz_id",
                table: "quiz_questions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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
    }
}
