using Microsoft.EntityFrameworkCore.Migrations;

namespace Checkers.Data.Migrations
{
    public partial class LastMovement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastMovedId",
                table: "BoardStates",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardStates_LastMovedId",
                table: "BoardStates",
                column: "LastMovedId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardStates_Field_LastMovedId",
                table: "BoardStates",
                column: "LastMovedId",
                principalTable: "Field",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardStates_Field_LastMovedId",
                table: "BoardStates");

            migrationBuilder.DropIndex(
                name: "IX_BoardStates_LastMovedId",
                table: "BoardStates");

            migrationBuilder.DropColumn(
                name: "LastMovedId",
                table: "BoardStates");
        }
    }
}
