using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{

    public partial class AddIsFirstLoginToAppUser : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_first_login",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_first_login",
                table: "AspNetUsers");
        }
    }
}
