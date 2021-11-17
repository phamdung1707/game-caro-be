using Microsoft.EntityFrameworkCore.Migrations;

namespace game_caro_be.Migrations
{
    public partial class AddMoneyAndCountGameUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "countGame",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "countWin",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "money",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "countGame",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "countWin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "money",
                table: "Users");
        }
    }
}
