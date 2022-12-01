using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByInQuizEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by_id",
                table: "quizzes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_quizzes_created_by_id",
                table: "quizzes",
                column: "created_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_quizzes_users_created_by_id",
                table: "quizzes",
                column: "created_by_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quizzes_users_created_by_id",
                table: "quizzes");

            migrationBuilder.DropIndex(
                name: "ix_quizzes_created_by_id",
                table: "quizzes");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "quizzes");
        }
    }
}
