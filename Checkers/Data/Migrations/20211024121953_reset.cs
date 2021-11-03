using Microsoft.EntityFrameworkCore.Migrations;

namespace Checkers.Data.Migrations
{
    public partial class reset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Rooms",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "User1IsActive",
                table: "Rooms",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "User2IsActive",
                table: "Rooms",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "User1IsActive",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "User2IsActive",
                table: "Rooms");
        }
    }
}
