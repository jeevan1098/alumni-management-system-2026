using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAlumniAppUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumni_AspNetUsers_jag_id",
                table: "Alumni");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_JagId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
