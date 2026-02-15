using Alumni_Management_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Alumni_Management_System.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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
        public virtual DbSet<OrganizationType> OrganizationTypes { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }

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

            // --- Alumni Configurations ---
            modelBuilder.Entity<Alumni>(entity =>
            {
                entity.HasKey(e => e.AlumniId).HasName("PK__Alumni__BB1DF35C3C7BFE94");

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Privacy).HasDefaultValue(true);

                entity.HasIndex(a => a.JagId).IsUnique();

                // Configure One-to-One with ApplicationUser
                entity.HasOne(a => a.User)
                      .WithOne(u => u.Alumni)
                      .HasForeignKey<Alumni>(a => a.UserId)
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
                entity.HasOne(d => d.OrganizationType).WithMany(p => p.AlumniOrganizations).HasConstraintName("fk_ao_org");
            });

            modelBuilder.Entity<AlumniRegistry>(entity =>
            {
                entity.HasKey(e => e.RegistryId).HasName("PK__Alumni_R__EF8E9CE8B1C6B2D8");
            });

            modelBuilder.Entity<DegreeProgram>(entity =>
            {
                entity.HasKey(e => e.DegreeId).HasName("PK__Degree_P__A1AFAEBBB780871C");
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

            modelBuilder.Entity<OrganizationType>(entity =>
            {
                entity.HasKey(e => e.OrganizationTypeId).HasName("PK__Organiza__466C7A244B987C0F");
            });

            //Commented out to prevent conflicting configurations in partial files
            //OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}