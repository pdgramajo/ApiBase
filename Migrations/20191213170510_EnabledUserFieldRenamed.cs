using Microsoft.EntityFrameworkCore.Migrations;

namespace api.Migrations
{
    public partial class EnabledUserFieldRenamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enable",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "enabled",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "enabled",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
