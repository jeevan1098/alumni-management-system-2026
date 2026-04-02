using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{

    public partial class ChangeSolicitationCodeToBool : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "solicitation_code",
                table: "Alumni",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "solicitation_code",
                table: "Alumni",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
