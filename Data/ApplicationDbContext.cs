using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alumni_Management_System.Models;
using Alumni_Management_System.Models.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Alumni_Management_System.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole, string,
        IdentityUserClaim<string>, AppUserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // --- DbSets ---
        public virtual DbSet<Alumni> Alumni { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<AlumniDegree> AlumniDegrees { get; set; }
        public virtual DbSet<AlumniEmployment> AlumniEmployments { get; set; }
        public virtual DbSet<AlumniInternship> AlumniInternships { get; set; }
        public virtual DbSet<AlumniMessage> AlumniMessages { get; set; }
        public virtual DbSet<AlumniOrganization> AlumniOrganizations { get; set; }
        public virtual DbSet<AlumniRegistry> AlumniRegistries { get; set; }
        public virtual DbSet<DegreeProgram> DegreePrograms { get; set; }
        public virtual DbSet<Employer> Employers { get; set; }
        public virtual DbSet<College> Colleges { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<StudentOrganization> StudentOrganizations { get; set; }
        public virtual DbSet<UserAccessScope> UserAccessScopes { get; set; }

        // --- Audit DbSets ---
        public virtual DbSet<AlumniAudit> AlumniAudits { get; set; }
        public virtual DbSet<AlumniDegreeAudit> AlumniDegreeAudits { get; set; }
        public virtual DbSet<AlumniEmploymentAudit> AlumniEmploymentAudits { get; set; }
        public virtual DbSet<AlumniInternshipAudit> AlumniInternshipAudits { get; set; }
        public virtual DbSet<AlumniMessageAudit> AlumniMessageAudits { get; set; }
        public virtual DbSet<AlumniOrganizationAudit> AlumniOrganizationAudits { get; set; }
        public virtual DbSet<AlumniRegistryAudit> AlumniRegistryAudits { get; set; }
        public virtual DbSet<CollegeAudit> CollegeAudits { get; set; }
        public virtual DbSet<DepartmentAudit> DepartmentAudits { get; set; }
        public virtual DbSet<StudentOrganizationAudit> StudentOrganizationAudits { get; set; }
        public virtual DbSet<DegreeProgramAudit> DegreeProgramAudits { get; set; }
        public virtual DbSet<EmployerAudit> EmployerAudits { get; set; }
        public virtual DbSet<MessageAudit> MessageAudits { get; set; }
        public virtual DbSet<UserAccessScopeAudit> UserAccessScopeAudits { get; set; }

        // Maps a tracked entity's CLR type to a factory that builds its matching
        // *_Audit row. The source entity's primary key is read generically via
        // EF metadata (entry.Metadata.FindPrimaryKey()) rather than hardcoded
        // per type, so this only needs the id VALUE, not the id property name.
        private static readonly Dictionary<Type, Func<int, string, string, DateTime, string, string, object>> AuditFactories = new()
        {
            [typeof(Alumni)] = (id, action, changedBy, changedAt, oldV, newV) => new AlumniAudit { AlumniId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(AlumniDegree)] = (id, action, changedBy, changedAt, oldV, newV) => new AlumniDegreeAudit { AlumniDegreeId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(AlumniEmployment)] = (id, action, changedBy, changedAt, oldV, newV) => new AlumniEmploymentAudit { AlumniEmploymentId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(AlumniInternship)] = (id, action, changedBy, changedAt, oldV, newV) => new AlumniInternshipAudit { AlumniInternshipId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(AlumniMessage)] = (id, action, changedBy, changedAt, oldV, newV) => new AlumniMessageAudit { AlumniMessageId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(AlumniOrganization)] = (id, action, changedBy, changedAt, oldV, newV) => new AlumniOrganizationAudit { AlumniOrganizationId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(AlumniRegistry)] = (id, action, changedBy, changedAt, oldV, newV) => new AlumniRegistryAudit { RegistryId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(College)] = (id, action, changedBy, changedAt, oldV, newV) => new CollegeAudit { CollegeId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(Department)] = (id, action, changedBy, changedAt, oldV, newV) => new DepartmentAudit { DepartmentId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(StudentOrganization)] = (id, action, changedBy, changedAt, oldV, newV) => new StudentOrganizationAudit { OrganizationId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(DegreeProgram)] = (id, action, changedBy, changedAt, oldV, newV) => new DegreeProgramAudit { DegreeId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(Employer)] = (id, action, changedBy, changedAt, oldV, newV) => new EmployerAudit { EmployerId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(Message)] = (id, action, changedBy, changedAt, oldV, newV) => new MessageAudit { MessageId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
            [typeof(UserAccessScope)] = (id, action, changedBy, changedAt, oldV, newV) => new UserAccessScopeAudit { UserAccessScopeId = id, ActionType = action, ChangedBy = changedBy, ChangedAt = changedAt, OldValues = oldV, NewValues = newV },
        };

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var pending = CapturePendingAudits();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            PersistAudits(pending);
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var pending = CapturePendingAudits();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await PersistAuditsAsync(pending, cancellationToken);
            return result;
        }

        private List<(EntityEntry Entry, string ActionType, string OldValuesJson)> CapturePendingAudits()
        {
            var pending = new List<(EntityEntry, string, string)>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (!AuditFactories.ContainsKey(entry.Entity.GetType()))
                {
                    continue;
                }

                string actionType = entry.State switch
                {
                    EntityState.Added => "Added",
                    EntityState.Modified => "Modified",
                    EntityState.Deleted => "Deleted",
                    _ => null
                };

                if (actionType == null)
                {
                    continue;
                }

                // Original values are only meaningful before the save commits -
                // afterward EF resets them to match the new current state.
                string oldValuesJson = actionType != "Added" ? SerializeValues(entry.OriginalValues) : null;
                pending.Add((entry, actionType, oldValuesJson));
            }

            return pending;
        }

        private void PersistAudits(List<(EntityEntry Entry, string ActionType, string OldValuesJson)> pending)
        {
            if (pending.Count == 0)
            {
                return;
            }

            foreach (var auditRow in BuildAuditRows(pending))
            {
                Add(auditRow);
            }

            base.SaveChanges(true);
        }

        private async Task PersistAuditsAsync(List<(EntityEntry Entry, string ActionType, string OldValuesJson)> pending, CancellationToken cancellationToken)
        {
            if (pending.Count == 0)
            {
                return;
            }

            foreach (var auditRow in BuildAuditRows(pending))
            {
                Add(auditRow);
            }

            await base.SaveChangesAsync(cancellationToken);
        }

        private IEnumerable<object> BuildAuditRows(List<(EntityEntry Entry, string ActionType, string OldValuesJson)> pending)
        {
            var changedBy = GetCurrentUserName();
            var changedAt = DateTime.Now;

            foreach (var (entry, actionType, oldValuesJson) in pending)
            {
                // Read the PK generically via EF metadata - the same code path
                // works for every audited entity without hardcoding property names,
                // and reflects the real generated value for newly Added rows since
                // this runs after the main SaveChanges/SaveChangesAsync call above.
                var pkProperty = entry.Metadata.FindPrimaryKey()!.Properties.Single();
                var idValue = Convert.ToInt32(entry.Property(pkProperty.Name).CurrentValue);

                string newValuesJson = actionType != "Deleted" ? SerializeValues(entry.CurrentValues) : null;

                yield return AuditFactories[entry.Entity.GetType()](idValue, actionType, changedBy, changedAt, oldValuesJson, newValuesJson);
            }
        }

        private static string SerializeValues(PropertyValues values)
        {
            if (values == null)
            {
                return null;
            }

            var dict = new Dictionary<string, object>();
            foreach (var property in values.Properties)
            {
                dict[property.Name] = values[property];
            }

            return JsonSerializer.Serialize(dict);
        }

        private string GetCurrentUserName()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true ? user.Identity.Name : "System";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=DefaultConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CRITICAL: This must be first to configure Identity keys correctly
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUserRole>(entity =>
            {
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.ScopeMode).HasDefaultValue("System");
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("(getdate())");
            });

            // --- Alumni Configurations ---
            modelBuilder.Entity<Alumni>(entity =>
            {
                entity.HasKey(e => e.AlumniId).HasName("PK__Alumni__BB1DF35C3C7BFE94");

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Privacy).HasDefaultValue(true);

                entity.HasIndex(a => a.JagId).IsUnique();

                // Alumni <-> AppUser is a logical link by JagId value, not a
                // DB foreign key (see [NotMapped] on both nav properties) -
                // AppUser.JagId is shared by Admin/Staff accounts too, and an
                // Alumni row can exist before anyone has registered a login
                // for it (bulk import), so a hard FK either direction is
                // wrong here.

                entity.HasOne(a => a.College).WithMany(c => c.Alumni)
                      .HasConstraintName("fk_alumni_college")
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });

            // --- Other Entity Configurations ---

            modelBuilder.Entity<AlumniDegree>(entity =>
            {
                entity.HasKey(e => e.AlumniDegreeId).HasName("PK__Alumni_D__F0FA85104CA70CF5");
                entity.HasOne(d => d.Alumni).WithMany(p => p.AlumniDegrees).HasConstraintName("fk_ad_alumni");
                entity.HasOne(d => d.Degree).WithMany(p => p.AlumniDegrees).HasConstraintName("fk_ad_degree");
            });

            modelBuilder.Entity<AlumniEmployment>(entity =>
            {
                entity.HasKey(e => e.AlumniEmploymentId).HasName("PK__Alumni_E__22E422E8BDA6C801");
                entity.HasOne(d => d.Alumni).WithMany(p => p.AlumniEmployments).HasConstraintName("fk_ae_alumni");
                entity.HasOne(d => d.Employer).WithMany(p => p.AlumniEmployments).HasConstraintName("fk_ae_employer");
            });

            modelBuilder.Entity<AlumniInternship>(entity =>
            {
                entity.HasKey(e => e.AlumniInternshipId).HasName("PK__Alumni_I__FFE040B307B79B52");
                entity.HasOne(d => d.Alumni).WithMany(p => p.AlumniInternships).HasConstraintName("fk_ai_alumni");
                entity.HasOne(d => d.Employer).WithMany(p => p.AlumniInternships).HasConstraintName("fk_ai_employer");
            });

            modelBuilder.Entity<AlumniMessage>(entity =>
            {
                entity.HasKey(e => e.AlumniMessageId).HasName("PK__Alumni_M__E6B241004CC7DEC9");
                entity.Property(e => e.SentAt).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Alumni).WithMany(p => p.AlumniMessages).HasConstraintName("fk_am_alumni");
                entity.HasOne(d => d.Message).WithMany(p => p.AlumniMessages)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("fk_am_message");
            });

            modelBuilder.Entity<AlumniOrganization>(entity =>
            {
                entity.HasKey(e => e.AlumniOrganizationId).HasName("PK__Alumni_O__8366569D1933C852");
                entity.HasOne(d => d.Alumni).WithMany(p => p.AlumniOrganizations).HasConstraintName("fk_ao_alumni");
                entity.HasOne(d => d.Organization).WithMany(p => p.AlumniOrganizations).HasConstraintName("fk_ao_org");
            });

            modelBuilder.Entity<College>(entity =>
            {
                entity.HasKey(e => e.CollegeId);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId);
                entity.HasOne(d => d.College).WithMany(c => c.Departments)
                      .HasConstraintName("fk_dept_college")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StudentOrganization>(entity =>
            {
                entity.HasKey(e => e.OrganizationId);
                entity.HasOne(d => d.College).WithMany(c => c.StudentOrganizations)
                      .HasConstraintName("fk_studorg_college")
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Department).WithMany(dp => dp.StudentOrganizations)
                      .HasConstraintName("fk_studorg_department")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserAccessScope>(entity =>
            {
                entity.HasKey(e => e.UserAccessScopeId);

                entity.HasOne(e => e.User).WithMany()
                      .HasForeignKey(e => e.UserId)
                      .HasConstraintName("fk_scope_user")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role).WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .HasConstraintName("fk_scope_role")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.College).WithMany()
                      .HasForeignKey(e => e.CollegeId)
                      .HasConstraintName("fk_scope_college")
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department).WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .HasConstraintName("fk_scope_department")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AlumniRegistry>(entity =>
            {
                entity.HasKey(e => e.RegistryId).HasName("PK__Alumni_R__EF8E9CE8B1C6B2D8");
            });

            modelBuilder.Entity<DegreeProgram>(entity =>
            {
                entity.HasKey(e => e.DegreeId).HasName("PK__Degree_P__A1AFAEBBB780871C");
                entity.HasOne(d => d.Department).WithMany(dp => dp.DegreePrograms)
                      .HasConstraintName("fk_degree_department")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Employer>(entity =>
            {
                entity.HasKey(e => e.EmployerId).HasName("PK__Employer__365FA4E7DF9F3065");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId).HasName("PK__Messages__0BBF6EE63BAB61CB");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            });

            //Commented out to prevent conflicting configurations in partial files
            //OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}