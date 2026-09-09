using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegesDepartmentsAndStudentOrgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ao_org",
                table: "Alumni_Organizations");

            migrationBuilder.DropTable(
                name: "Organization_Types");

            migrationBuilder.DropColumn(
                name: "department",
                table: "Degree_Programs");

            migrationBuilder.DropColumn(
                name: "age_at_graduation",
                table: "Alumni");

            migrationBuilder.RenameColumn(
                name: "organization_type_id",
                table: "Alumni_Organizations",
                newName: "organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_Alumni_Organizations_organization_type_id",
                table: "Alumni_Organizations",
                newName: "IX_Alumni_Organizations_organization_id");

            migrationBuilder.AddColumn<int>(
                name: "college_id",
                table: "Alumni",
                type: "int",
                nullable: true);

            // Added as nullable, backfilled below, then no NOT NULL constraint is
            // enforced at the DB level (matches DegreeProgram.DepartmentId being
            // required going forward, but avoids breaking any existing rows).
            migrationBuilder.AddColumn<int>(
                name: "department_id",
                table: "Degree_Programs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "Degree_Programs",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "date_of_birth",
                table: "Alumni",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                table: "Alumni",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suffix",
                table: "Alumni",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Colleges",
                columns: table => new
                {
                    college_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    college_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    is_internal = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colleges", x => x.college_id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    college_id = table.Column<int>(type: "int", nullable: false),
                    department_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.department_id);
                    table.ForeignKey(
                        name: "fk_dept_college",
                        column: x => x.college_id,
                        principalTable: "Colleges",
                        principalColumn: "college_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Student_Organizations",
                columns: table => new
                {
                    organization_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    organization_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    college_id = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Organizations", x => x.organization_id);
                    table.ForeignKey(
                        name: "fk_studorg_college",
                        column: x => x.college_id,
                        principalTable: "Colleges",
                        principalColumn: "college_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_studorg_department",
                        column: x => x.department_id,
                        principalTable: "Departments",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Seed a default College/Department and backfill any pre-existing
            // Degree_Programs rows before department_id is tightened to NOT NULL,
            // so this migration is safe to run against a database that already
            // has seeded sample data.
            migrationBuilder.Sql(@"
INSERT INTO Colleges (college_name, is_internal, is_active) VALUES (N'School of Computing', 1, 1);
DECLARE @DefaultCollegeId INT = SCOPE_IDENTITY();
INSERT INTO Departments (college_id, department_name, is_active) VALUES (@DefaultCollegeId, N'General', 1);
DECLARE @DefaultDepartmentId INT = SCOPE_IDENTITY();
UPDATE Degree_Programs SET department_id = @DefaultDepartmentId WHERE department_id IS NULL;
");

            migrationBuilder.AlterColumn<int>(
                name: "department_id",
                table: "Degree_Programs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Degree_Programs_department_id",
                table: "Degree_Programs",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_college_id",
                table: "Alumni",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_college_id",
                table: "Departments",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Organizations_college_id",
                table: "Student_Organizations",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Organizations_department_id",
                table: "Student_Organizations",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "fk_alumni_college",
                table: "Alumni",
                column: "college_id",
                principalTable: "Colleges",
                principalColumn: "college_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_ao_org",
                table: "Alumni_Organizations",
                column: "organization_id",
                principalTable: "Student_Organizations",
                principalColumn: "organization_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_degree_department",
                table: "Degree_Programs",
                column: "department_id",
                principalTable: "Departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alumni_college",
                table: "Alumni");

            migrationBuilder.DropForeignKey(
                name: "fk_ao_org",
                table: "Alumni_Organizations");

            migrationBuilder.DropForeignKey(
                name: "fk_degree_department",
                table: "Degree_Programs");

            migrationBuilder.DropTable(
                name: "Student_Organizations");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_Degree_Programs_department_id",
                table: "Degree_Programs");

            migrationBuilder.DropIndex(
                name: "IX_Alumni_college_id",
                table: "Alumni");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "Degree_Programs");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "Degree_Programs");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "Alumni");

            migrationBuilder.DropColumn(
                name: "middle_name",
                table: "Alumni");

            migrationBuilder.DropColumn(
                name: "suffix",
                table: "Alumni");

            migrationBuilder.DropColumn(
                name: "college_id",
                table: "Alumni");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "Alumni_Organizations",
                newName: "organization_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_Alumni_Organizations_organization_id",
                table: "Alumni_Organizations",
                newName: "IX_Alumni_Organizations_organization_type_id");

            migrationBuilder.AddColumn<int>(
                name: "age_at_graduation",
                table: "Alumni",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "Degree_Programs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Organization_Types",
                columns: table => new
                {
                    organization_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    organization_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Organiza__466C7A244B987C0F", x => x.organization_type_id);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_ao_org",
                table: "Alumni_Organizations",
                column: "organization_type_id",
                principalTable: "Organization_Types",
                principalColumn: "organization_type_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
