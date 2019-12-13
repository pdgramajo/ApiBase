using Microsoft.EntityFrameworkCore.Migrations;

namespace api.Migrations
{
    public partial class EnabledUpperCamelCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "Users",
                newName: "Enabled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Enabled",
                table: "Users",
                newName: "enabled");
        }
    }
}
