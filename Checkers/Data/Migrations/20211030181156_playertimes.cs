using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Checkers.Data.Migrations
{
    public partial class playertimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "User1Time",
                table: "Rooms",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "User2Time",
                table: "Rooms",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User1Time",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "User2Time",
                table: "Rooms");
        }
    }
}
