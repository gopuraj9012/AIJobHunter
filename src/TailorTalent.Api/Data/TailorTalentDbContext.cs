using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Models;
using TailorTalent.Api.Models.Subscription;

namespace TailorTalent.Api.Data;

public class TailorTalentDbContext : DbContext
{
    public TailorTalentDbContext(DbContextOptions<TailorTalentDbContext> options) : base(options)
    {
    }

    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<JobDescription> JobDescriptions => Set<JobDescription>();
    public DbSet<TailoringSession> TailoringSessions => Set<TailoringSession>();

    // Users & Auth
    public DbSet<User> Users => Set<User>();

    // Subscription & Credits
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<UserCredits> UserCredits => Set<UserCredits>();
    public DbSet<CreditTransaction> CreditTransactions => Set<CreditTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Resume configuration
        modelBuilder.Entity<Resume>(entity =>
        {
            entity.ToTable("Resumes");
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.CreatedAt);

            entity.Property(r => r.RawContent)
                  .HasColumnType("TEXT");

            entity.Property(r => r.ParsedSectionsJson)
                  .HasColumnType("TEXT")
                  .HasDefaultValue("{}");
        });

        // JobDescription configuration
        modelBuilder.Entity<JobDescription>(entity =>
        {
            entity.ToTable("JobDescriptions");
            entity.HasIndex(j => j.UserId);
            entity.HasIndex(j => j.CreatedAt);

            entity.Property(j => j.RawContent)
                  .HasColumnType("TEXT");

            entity.Property(j => j.ParsedRequirementsJson)
                  .HasColumnType("TEXT")
                  .HasDefaultValue("{}");
        });

        // TailoringSession configuration
        modelBuilder.Entity<TailoringSession>(entity =>
        {
            entity.ToTable("TailoringSessions");
            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.CreatedAt);

            entity.Property(t => t.TailoredContent)
                  .HasColumnType("TEXT");

            entity.Property(t => t.CoverLetter)
                  .HasColumnType("TEXT");

            entity.HasOne(t => t.Resume)
                  .WithMany(r => r.TailoringSessions)
                  .HasForeignKey(t => t.ResumeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.JobDescription)
                  .WithMany(j => j.TailoringSessions)
                  .HasForeignKey(t => t.JobDescriptionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserSubscription configuration
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("UserSubscriptions");
            entity.HasIndex(s => s.UserId).IsUnique();
            entity.Property(s => s.Plan).HasConversion<int>();
        });

        // UserCredits configuration
        modelBuilder.Entity<UserCredits>(entity =>
        {
            entity.ToTable("UserCredits");
            entity.HasIndex(c => c.UserId).IsUnique();
        });

        // CreditTransaction configuration
        modelBuilder.Entity<CreditTransaction>(entity =>
        {
            entity.ToTable("CreditTransactions");
            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => t.CreatedAt);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
}