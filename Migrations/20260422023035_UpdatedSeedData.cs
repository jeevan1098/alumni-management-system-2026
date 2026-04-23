using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Alumni_permanent_email",
                table: "Alumni",
                column: "permanent_email",
                unique: true,
                filter: "[permanent_email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_student_email",
                table: "Alumni",
                column: "student_email",
                unique: true,
                filter: "[student_email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alumni_permanent_email",
                table: "Alumni");

            migrationBuilder.DropIndex(
                name: "IX_Alumni_student_email",
                table: "Alumni");
        }
    }
}
