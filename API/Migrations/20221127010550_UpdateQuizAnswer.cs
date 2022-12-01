using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class UpdateQuizAnswer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "required_answer_type",
                table: "quiz_questions",
                newName: "required_quiz_answer_type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "required_quiz_answer_type",
                table: "quiz_questions",
                newName: "required_answer_type");
        }
    }
}
