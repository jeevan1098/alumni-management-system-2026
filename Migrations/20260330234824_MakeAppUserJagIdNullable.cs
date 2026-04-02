using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{

    public partial class MakeAppUserJagIdNullable : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "JagId",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId",
                unique: true,
                filter: "[JagId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "JagId",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId",
                unique: true);
        }
    }
}
