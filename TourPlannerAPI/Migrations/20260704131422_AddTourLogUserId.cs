using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlannerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTourLogUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TourLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TourLogs_UserId",
                table: "TourLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TourLogs_Users_UserId",
                table: "TourLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourLogs_Users_UserId",
                table: "TourLogs");

            migrationBuilder.DropIndex(
                name: "IX_TourLogs_UserId",
                table: "TourLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TourLogs");
        }
    }
}
