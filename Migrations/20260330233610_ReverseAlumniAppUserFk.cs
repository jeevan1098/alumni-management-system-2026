using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{

    public partial class ReverseAlumniAppUserFk : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumni_AspNetUsers_jag_id",
                table: "Alumni");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_JagId",
                table: "AspNetUsers");

            migrationBuilder.Sql(@"
                DELETE FROM [AspNetUsers]
                WHERE [JagId] IS NOT NULL
                  AND [JagId] NOT IN (SELECT [jag_id] FROM [Alumni]);
            ");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Alumni_jag_id",
                table: "Alumni",
                column: "jag_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Alumni_JagId",
                table: "AspNetUsers",
                column: "JagId",
                principalTable: "Alumni",
                principalColumn: "jag_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alumni_AspNetUsers_jag_id",
                table: "Alumni",
                column: "jag_id",
                principalTable: "AspNetUsers",
                principalColumn: "JagId");
        }
    }
}
