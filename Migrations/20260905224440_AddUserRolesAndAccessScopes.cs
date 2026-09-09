using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRolesAndAccessScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "assigned_at",
                table: "AspNetUserRoles",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<string>(
                name: "assigned_by",
                table: "AspNetUserRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "AspNetUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "scope_mode",
                table: "AspNetUserRoles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.CreateTable(
                name: "User_Access_Scopes",
                columns: table => new
                {
                    user_access_scope_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    role_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    college_id = table.Column<int>(type: "int", nullable: true),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    access_level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Access_Scopes", x => x.user_access_scope_id);
                    table.ForeignKey(
                        name: "fk_scope_college",
                        column: x => x.college_id,
                        principalTable: "Colleges",
                        principalColumn: "college_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_scope_department",
                        column: x => x.department_id,
                        principalTable: "Departments",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_scope_role",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_scope_user",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Access_Scopes_college_id",
                table: "User_Access_Scopes",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Access_Scopes_department_id",
                table: "User_Access_Scopes",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Access_Scopes_role_id",
                table: "User_Access_Scopes",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Access_Scopes_user_id",
                table: "User_Access_Scopes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User_Access_Scopes");

            migrationBuilder.DropColumn(
                name: "assigned_at",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "assigned_by",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "scope_mode",
                table: "AspNetUserRoles");
        }
    }
}
