using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDateOfBirthWithAgeAtGraduation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "Alumni");

            migrationBuilder.AddColumn<int>(
                name: "age_at_graduation",
                table: "Alumni",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "age_at_graduation",
                table: "Alumni");

            migrationBuilder.AddColumn<DateOnly>(
                name: "date_of_birth",
                table: "Alumni",
                type: "date",
                nullable: true);
        }
    }
}
