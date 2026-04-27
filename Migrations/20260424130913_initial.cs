using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alumni_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alumni_Registry",
                columns: table => new
                {
                    registry_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    jag_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    account_created = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Alumni_R__EF8E9CE8B1C6B2D8", x => x.registry_id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JagId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_first_login = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Degree_Programs",
                columns: table => new
                {
                    degree_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    institution = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    degree_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    major_field_of_study = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Degree_P__A1AFAEBBB780871C", x => x.degree_id);
                });

            migrationBuilder.CreateTable(
                name: "Employers",
                columns: table => new
                {
                    employer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employer_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Employer__365FA4E7DF9F3065", x => x.employer_id);
                });

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

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alumni",
                columns: table => new
                {
                    alumni_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    jag_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    prefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    preferred_first_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    last_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    age_at_graduation = table.Column<int>(type: "int", nullable: true),
                    student_email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    permanent_email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    postcode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    graduation_year = table.Column<int>(type: "int", nullable: false),
                    solicitation_code = table.Column<bool>(type: "bit", nullable: false),
                    social_media_account = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    privacy = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    last_updated = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Alumni__BB1DF35C3C7BFE94", x => x.alumni_id);
                    table.ForeignKey(
                        name: "FK_Alumni_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    message_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    message_body = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    message_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Messages__0BBF6EE63BAB61CB", x => x.message_id);
                    table.ForeignKey(
                        name: "FK_Messages_AspNetUsers_created_by",
                        column: x => x.created_by,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Degrees",
                columns: table => new
                {
                    alumni_degree_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_id = table.Column<int>(type: "int", nullable: false),
                    degree_id = table.Column<int>(type: "int", nullable: false),
                    date_conferred = table.Column<DateOnly>(type: "date", nullable: false),
                    years_to_complete_degree = table.Column<int>(type: "int", nullable: true),
                    gpa = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    employment_while_studying = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    degree_specific_job = table.Column<bool>(type: "bit", nullable: true),
                    participated_in_research = table.Column<bool>(type: "bit", nullable: true),
                    job_secured_upon_graduation = table.Column<bool>(type: "bit", nullable: true),
                    attended_or_plans_grad_school = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Alumni_D__F0FA85104CA70CF5", x => x.alumni_degree_id);
                    table.ForeignKey(
                        name: "fk_ad_alumni",
                        column: x => x.alumni_id,
                        principalTable: "Alumni",
                        principalColumn: "alumni_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ad_degree",
                        column: x => x.degree_id,
                        principalTable: "Degree_Programs",
                        principalColumn: "degree_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Employment",
                columns: table => new
                {
                    alumni_employment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_id = table.Column<int>(type: "int", nullable: false),
                    employer_id = table.Column<int>(type: "int", nullable: false),
                    job_title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    salary_range = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Alumni_E__22E422E8BDA6C801", x => x.alumni_employment_id);
                    table.ForeignKey(
                        name: "fk_ae_alumni",
                        column: x => x.alumni_id,
                        principalTable: "Alumni",
                        principalColumn: "alumni_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ae_employer",
                        column: x => x.employer_id,
                        principalTable: "Employers",
                        principalColumn: "employer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Internships",
                columns: table => new
                {
                    alumni_internship_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_id = table.Column<int>(type: "int", nullable: false),
                    employer_id = table.Column<int>(type: "int", nullable: false),
                    internship_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Alumni_I__FFE040B307B79B52", x => x.alumni_internship_id);
                    table.ForeignKey(
                        name: "fk_ai_alumni",
                        column: x => x.alumni_id,
                        principalTable: "Alumni",
                        principalColumn: "alumni_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ai_employer",
                        column: x => x.employer_id,
                        principalTable: "Employers",
                        principalColumn: "employer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Organizations",
                columns: table => new
                {
                    alumni_organization_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_id = table.Column<int>(type: "int", nullable: false),
                    organization_type_id = table.Column<int>(type: "int", nullable: false),
                    officer_roles = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Alumni_O__8366569D1933C852", x => x.alumni_organization_id);
                    table.ForeignKey(
                        name: "fk_ao_alumni",
                        column: x => x.alumni_id,
                        principalTable: "Alumni",
                        principalColumn: "alumni_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ao_org",
                        column: x => x.organization_type_id,
                        principalTable: "Organization_Types",
                        principalColumn: "organization_type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alumni_Messages",
                columns: table => new
                {
                    alumni_message_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumni_id = table.Column<int>(type: "int", nullable: false),
                    message_id = table.Column<int>(type: "int", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Alumni_M__E6B241004CC7DEC9", x => x.alumni_message_id);
                    table.ForeignKey(
                        name: "fk_am_alumni",
                        column: x => x.alumni_id,
                        principalTable: "Alumni",
                        principalColumn: "alumni_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_am_message",
                        column: x => x.message_id,
                        principalTable: "Messages",
                        principalColumn: "message_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_jag_id",
                table: "Alumni",
                column: "jag_id",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_user_id",
                table: "Alumni",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Alumni__FBF400ED6AB71E0A",
                table: "Alumni",
                column: "jag_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Degrees_alumni_id",
                table: "Alumni_Degrees",
                column: "alumni_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Degrees_degree_id",
                table: "Alumni_Degrees",
                column: "degree_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Employment_alumni_id",
                table: "Alumni_Employment",
                column: "alumni_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Employment_employer_id",
                table: "Alumni_Employment",
                column: "employer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Internships_alumni_id",
                table: "Alumni_Internships",
                column: "alumni_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Internships_employer_id",
                table: "Alumni_Internships",
                column: "employer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Messages_alumni_id",
                table: "Alumni_Messages",
                column: "alumni_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Messages_message_id",
                table: "Alumni_Messages",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Organizations_alumni_id",
                table: "Alumni_Organizations",
                column: "alumni_id");

            migrationBuilder.CreateIndex(
                name: "IX_Alumni_Organizations_organization_type_id",
                table: "Alumni_Organizations",
                column: "organization_type_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Alumni_R__FBF400ED39BA228D",
                table: "Alumni_Registry",
                column: "jag_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_JagId",
                table: "AspNetUsers",
                column: "JagId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_created_by",
                table: "Messages",
                column: "created_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alumni_Degrees");

            migrationBuilder.DropTable(
                name: "Alumni_Employment");

            migrationBuilder.DropTable(
                name: "Alumni_Internships");

            migrationBuilder.DropTable(
                name: "Alumni_Messages");

            migrationBuilder.DropTable(
                name: "Alumni_Organizations");

            migrationBuilder.DropTable(
                name: "Alumni_Registry");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Degree_Programs");

            migrationBuilder.DropTable(
                name: "Employers");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Alumni");

            migrationBuilder.DropTable(
                name: "Organization_Types");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
