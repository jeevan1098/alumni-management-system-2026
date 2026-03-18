using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeAtGraduationColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumni_AspNetUsers_user_id",
                table: "Alumni");

            migrationBuilder.DropTable(
                name: "Audit_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Alumni_user_id",
                table: "Alumni");

            migrationBuilder.DropColumn(
                name: "degree_program",
                table: "Alumni_Registry");

            migrationBuilder.DropColumn(
                name: "email_on_record",
                table: "Alumni_Registry");

            migrationBuilder.DropColumn(
                name: "graduation_year",
                table: "Alumni_Registry");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "Alumni");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "Alumni");

            migrationBuilder.AddColumn<int>(
                name: "age_at_graduation",
                table: "Alumni",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumni_AspNetUsers_jag_id",
                table: "Alumni");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_JagId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "age_at_graduation",
                table: "Alumni");

            migrationBuilder.AddColumn<string>(
                name: "degree_program",
                table: "Alumni_Registry",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_on_record",
                table: "Alumni_Registry",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "graduation_year",
                table: "Alumni_Registry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "date_of_birth",
                table: "Alumni",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "Alumni",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Audit_Logs",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    entity_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    entity_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_Logs", x => x.audit_id);
                    table.ForeignKey(
                        name: "FK_Audit_Logs_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_user_id",
                table: "Alumni",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Logs_user_id",
                table: "Audit_Logs",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alumni_AspNetUsers_user_id",
                table: "Alumni",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
