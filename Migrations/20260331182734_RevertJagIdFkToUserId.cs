using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{

    public partial class RevertJagIdFkToUserId : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Alumni_JagId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Alumni_jag_id",
                table: "Alumni");

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "Alumni",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_user_id",
                table: "Alumni",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alumni_AspNetUsers_user_id",
                table: "Alumni",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumni_AspNetUsers_user_id",
                table: "Alumni");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Alumni_user_id",
                table: "Alumni");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "Alumni");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Alumni_jag_id",
                table: "Alumni",
                column: "jag_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId",
                unique: true,
                filter: "[JagId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Alumni_JagId",
                table: "AspNetUsers",
                column: "JagId",
                principalTable: "Alumni",
                principalColumn: "jag_id");
        }
    }
}
