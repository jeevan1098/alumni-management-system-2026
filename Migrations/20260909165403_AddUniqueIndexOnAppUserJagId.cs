using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnAppUserJagId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UQ_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_AspNetUsers_JagId",
                table: "AspNetUsers");
        }
    }
}
