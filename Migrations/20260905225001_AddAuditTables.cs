using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alumni_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Degrees_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_degree_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni_Degrees_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Employment_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_employment_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni_Employment_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Internships_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_internship_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni_Internships_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Messages_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_message_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni_Messages_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Organizations_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_organization_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni_Organizations_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Registry_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    registry_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumni_Registry_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Colleges_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    college_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colleges_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Degree_Programs_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    degree_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degree_Programs_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Departments_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Employers_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employer_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employers_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Messages_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    message_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "Student_Organizations_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    organization_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Organizations_Audit", x => x.audit_id);
                });

            migrationBuilder.CreateTable(
                name: "User_Access_Scopes_Audit",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_access_scope_id = table.Column<int>(type: "int", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Access_Scopes_Audit", x => x.audit_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alumni_Audit");

            migrationBuilder.DropTable(
                name: "Alumni_Degrees_Audit");

            migrationBuilder.DropTable(
                name: "Alumni_Employment_Audit");

            migrationBuilder.DropTable(
                name: "Alumni_Internships_Audit");

            migrationBuilder.DropTable(
                name: "Alumni_Messages_Audit");

            migrationBuilder.DropTable(
                name: "Alumni_Organizations_Audit");

            migrationBuilder.DropTable(
                name: "Alumni_Registry_Audit");

            migrationBuilder.DropTable(
                name: "Colleges_Audit");

            migrationBuilder.DropTable(
                name: "Degree_Programs_Audit");

            migrationBuilder.DropTable(
                name: "Departments_Audit");

            migrationBuilder.DropTable(
                name: "Employers_Audit");

            migrationBuilder.DropTable(
                name: "Messages_Audit");

            migrationBuilder.DropTable(
                name: "Student_Organizations_Audit");

            migrationBuilder.DropTable(
                name: "User_Access_Scopes_Audit");
        }
    }
}
