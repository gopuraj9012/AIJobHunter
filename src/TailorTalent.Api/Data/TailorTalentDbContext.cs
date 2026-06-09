using Microsoft.EntityFrameworkCore;
using TailorTalent.Api.Models;

namespace TailorTalent.Api.Data;

public class TailorTalentDbContext : DbContext
{
    public TailorTalentDbContext(DbContextOptions<TailorTalentDbContext> options) : base(options)
    {
    }

    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<JobDescription> JobDescriptions => Set<JobDescription>();
    public DbSet<TailoringSession> TailoringSessions => Set<TailoringSession>();

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
    }
}