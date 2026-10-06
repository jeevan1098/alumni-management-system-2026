using Alumni_Management_System.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    // Data-only: Is Active now means "living", but manual creates and bulk
    // imports used to save every new alumnus as inactive. Mark them all active.
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261005120000_MarkExistingAlumniActive")]
    public partial class MarkExistingAlumniActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Alumni] SET [is_active] = 1 WHERE [is_active] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible - which rows were inactive before isn't recorded.
        }
    }
}
